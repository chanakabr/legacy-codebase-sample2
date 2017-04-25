using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.QueueObjects;
using ApiObjects.Response;
using Core.Catalog;
using Core.Catalog.Request;
using Core.Catalog.Response;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using Core.Notification.Adapters;
using QueueWrapper;
using QueueWrapper.Queues.QueueObjects;
using ScheduledTasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using TVinciShared;
using APILogic.AmazonSnsAdapter;

namespace Core.Notification
{
    public class AnnouncementManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string ROUTING_KEY_CHECK_PENDING_TRANSACTION = "PROCESS_MESSAGE_ANNOUNCEMENTS";


        private const string ROUTING_KEY_NOTIFICATION_CLEANUP = "PROCESS_NOTIFIACTION_CLEANUP";
        public const double NOTIFICATION_CLEANUP_INTERVAL_SEC = 86400; // 24 hours


        private const int MAX_MSG_LENGTH = 250 * 1024;
        private const int MIN_TIME_FOR_START_TIME_SECONDS = 30;

        private static string CatalogSignString = Guid.NewGuid().ToString();
        private static string CatalogSignatureKey = ODBCWrapper.Utils.GetTcmConfigValue("CatalogSignatureKey");

        private const string ANNOUNCEMENT_NOT_FOUND = "Announcement Not Found";
        private const string ANNOUNCEMENT_QUEUE_NAME_FORMAT = @"Announcement_{0}_{1}"; // Announcement_{GID}_{AnnID}


        private static string outerPushDomainName = ODBCWrapper.Utils.GetTcmConfigValue("PushDomainName");
        private static string outerPushServerSecret = ODBCWrapper.Utils.GetTcmConfigValue("PushServerKey");
        private static string outerPushServerIV = ODBCWrapper.Utils.GetTcmConfigValue("PushServerIV");

        //private static string PushNotificationQueueTTLMilliSec = ODBCWrapper.Utils.GetTcmConfigValue("PushNotificationQueueTTLMilliSec");
        public const int PUSH_MESSAGE_EXPIRATION_MILLI_SEC = 3000;

        public static AddMessageAnnouncementResponse AddMessageAnnouncement(int groupId, MessageAnnouncement announcement, bool enforceMsgAllowedTime = false, bool validateMsgStartTime = true)
        {
            AddMessageAnnouncementResponse response = new AddMessageAnnouncementResponse();
            response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            if (ConvertStartTimeToUtc(ref announcement) == false)
            {
                response.Status = new Status((int)eResponseStatus.AnnouncementInvalidTimezone, "Invalid timezone");
                return response;
            }

            // validation of start start time is relevant only for system announcements
            if (validateMsgStartTime)
                response.Status = ValidateAnnouncement(groupId, announcement);

            if (response.Status.Code != (int)eResponseStatus.OK)
                return response;

            // change time to allowed start time if needed
            if (enforceMsgAllowedTime)
                EnforceAllowedStartTime(groupId, announcement);

            // add message announcement to DB
            announcement = AddMessageAnnouncementToDB(groupId, announcement);

            // add message to queue
            bool qRes = true;
            if (announcement.MessageAnnouncementId != 0)
            {
                // remove announcement from cache
                NotificationCache.Instance().RemoveAnnouncementsFromCache(groupId);

                qRes = AddMessageAnnouncementToQueue(groupId, announcement);
            }
            else
                log.ErrorFormat("Error while inserting announcement {0} to DB", announcement.Name);

            if (!qRes)
                log.ErrorFormat("Error while inserting announcement {0} to queue", announcement.MessageAnnouncementId);

            if (announcement.MessageAnnouncementId == 0 || !qRes)
            {
                response.Status = new Status((int)eResponseStatus.Error, "Internal Error");
                return response;
            }

            response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            response.Id = announcement.MessageAnnouncementId;
            response.Announcement = announcement;
            return response;
        }

        private static void EnforceAllowedStartTime(int groupId, MessageAnnouncement announcement)
        {
            var settings = NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            DateTime announcementStartTime = ODBCWrapper.Utils.UnixTimestampToDateTime(announcement.StartTime);

            log.DebugFormat("EnforceAllowedStartTime: checking allowed time for: group: {0}, announcement name: {1}, announcement start time: {2}, start time: {3}, settings: {4}",
                groupId, announcement.Name, announcement.StartTime, announcementStartTime.ToString(), JsonConvert.SerializeObject(settings));

            if (settings.settings.PushStartHour.HasValue &&
                settings.settings.PushEndHour.HasValue &&
                settings.settings.PushStartHour.Value != settings.settings.PushEndHour.Value)
            {
                DateTime allowedStart = new DateTime(1990, 1, 1, settings.settings.PushStartHour.Value, 0, 0);
                DateTime allowedEnd = new DateTime(1990, 1, 1, settings.settings.PushEndHour.Value, 0, 0);

                DateTime startTime = new DateTime(1990, 1, 1, announcementStartTime.Hour, announcementStartTime.Minute, announcementStartTime.Second);

                if (allowedEnd <= allowedStart)
                {
                    allowedEnd = allowedEnd.AddDays(1);

                    if (startTime < allowedStart)
                        startTime = startTime.AddDays(1);
                }

                if (startTime < allowedStart || startTime > allowedEnd)
                {
                    DateTime newStartTime = new DateTime(announcementStartTime.Year, announcementStartTime.Month, announcementStartTime.Day, allowedStart.Hour, 0, 0);

                    if (newStartTime < announcementStartTime)
                        newStartTime = newStartTime.AddDays(1);

                    announcementStartTime = newStartTime;

                    announcement.StartTime = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(announcementStartTime);

                    log.DebugFormat("Message start time was not in allowed interval, updated to {0}", newStartTime);
                }
            }
        }

        public static MessageAnnouncementResponse UpdateMessageAnnouncement(int groupId, int announcementId, MessageAnnouncement announcement, bool enforceMsgAllowedTime = false, bool validateMsgStartTime = true)
        {
            MessageAnnouncementResponse response = new MessageAnnouncementResponse();
            response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            if (validateMsgStartTime)
                response.Status = ValidateAnnouncement(groupId, announcement);

            // convert start time
            if (ConvertStartTimeToUtc(ref announcement) == false)
                response.Status = new Status((int)eResponseStatus.AnnouncementInvalidTimezone, "Invalid time zone");

            if (response.Status.Code != (int)eResponseStatus.OK)
                return response;

            DataRow dr = DAL.NotificationDal.Get_MessageAnnouncement(announcementId);
            if (dr == null)
            {
                log.ErrorFormat("Announcement not exist in DB: group: {0} Id: {1}", groupId, announcementId);
                response.Status = new Status((int)eResponseStatus.AnnouncementNotFound, ANNOUNCEMENT_NOT_FOUND);
                return response;
            }

            DateTime? startTime = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "start_time");

            if (validateMsgStartTime)
            {
                if (!startTime.HasValue || startTime.Value < DateTime.UtcNow)
                {
                    log.ErrorFormat("Announcement start time passed Id: {0} start time: {1}", announcementId, startTime);
                    response.Status = new Status((int)eResponseStatus.AnnouncementUpdateNotAllowed, "Announcement Update Not Allowed");
                    return response;
                }
            }

            // change time to allowed start time if needed
            if (enforceMsgAllowedTime)
            {
                EnforceAllowedStartTime(groupId, announcement);
            }

            int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");
            DateTime announcementStartTime = ODBCWrapper.Utils.UnixTimestampToDateTime(announcement.StartTime);

            DataRow row = DAL.NotificationDal.Update_MessageAnnouncement(id, groupId, (int)announcement.Recipients, announcement.Name, announcement.Message, announcement.Enabled, announcementStartTime, announcement.Timezone, 0);
            announcement = Core.Notification.Utils.GetMessageAnnouncementFromDataRow(row);

            // add a new message to queue when new time updated
            if (ODBCWrapper.Utils.UnixTimestampToDateTime(announcement.StartTime) != startTime)
            {
                if (!AddMessageAnnouncementToQueue(groupId, announcement))
                {
                    response.Status = new Status((int)eResponseStatus.Error, "Internal Error");
                    return response;
                }
            }

            response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            response.Announcement = announcement;
            return response;
        }

        public static Status UpdateMessageSystemAnnouncementStatus(int groupId, long id, bool status)
        {
            // validate system announcements are enabled
            if (!NotificationSettings.IsPartnerSystemAnnouncementEnabled(groupId))
            {
                log.ErrorFormat("UpdateMessageAnnouncementStatus  - partner system announcements are disabled. groupID = {0}", groupId);
                return new Status((int)eResponseStatus.FeatureDisabled, "Feature Disabled");
            }

            DataRow dr = ODBCWrapper.Utils.GetTableSingleRow("message_announcements", id, "MESSAGE_BOX_CONNECTION_STRING");
            if (dr == null)
            {
                log.ErrorFormat("Announcement not exist in DB: group: {0} Id: {1}", groupId, id);
                return new Status((int)eResponseStatus.AnnouncementNotFound, ANNOUNCEMENT_NOT_FOUND);
            }

            DateTime? startTime = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "start_time");

            if (!startTime.HasValue || startTime.Value < DateTime.UtcNow)
            {
                log.ErrorFormat("Announcement start time passed Id: {0} start time: {1}", id, startTime);
                return new Status((int)eResponseStatus.AnnouncementUpdateNotAllowed, "Announcement Update Not Allowed");
            }

            DAL.NotificationDal.Update_MessageAnnouncementStatus(ODBCWrapper.Utils.GetIntSafeVal(dr, "id"), groupId, status);

            return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
        }

        public static Status DeleteMessageAnnouncement(int groupId, long id)
        {
            // validate system announcements are enabled
            if (!NotificationSettings.IsPartnerSystemAnnouncementEnabled(groupId))
            {
                log.ErrorFormat("DeleteMessageAnnouncement  - partner system announcements are disabled. groupID = {0}", groupId);
                return new Status((int)eResponseStatus.FeatureDisabled, "Feature Disabled");
            }

            DataRow dr = DAL.NotificationDal.Get_MessageAnnouncement(id);
            if (dr == null)
            {
                log.ErrorFormat("Announcement not exist in DB: group: {0} Id: {1}", groupId, id);
                return new Status((int)eResponseStatus.AnnouncementNotFound, ANNOUNCEMENT_NOT_FOUND);
            }

            DAL.NotificationDal.Delete_MessageAnnouncement(id, groupId);

            return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
        }

        private static bool HandleRecipientOtherTvSeries(int groupId, int messageId, long startTime, int announcementId, ref DataRow messageAnnouncementDataRow, ref string url, ref string sound,
                                                            ref string category, out string annExternalId, out string singleQueueName, out bool failRes)
        {
            failRes = false;
            string[] seriesNames = null;
            MediaResponse response = null;
            DateTime catalogStartDateStr = DateTime.MinValue;
            long catalogStartDate = 0;
            string mediaName = string.Empty;
            DateTime startDate = DateTime.MinValue;
            annExternalId = string.Empty;
            singleQueueName = string.Empty;

            // check if announcement is for series, if not - return true to do nothing. if yes, check no msg was sent for series in the last 24H.
            // get topic push external id's of guests and logged in users
            var announcement = NotificationCache.Instance().GetAnnouncements(groupId).Where(x => x.ID == announcementId).FirstOrDefault();
            if (announcement == null)
            {
                log.ErrorFormat("announcement wasn't found. GID: {0}, announcementId: {1}", groupId, announcementId);
                return false;
            }

            if (announcement.FollowPhrase == null || !announcement.FollowPhrase.ToLower().Trim().StartsWith(FollowManager.GetEpisodeAssociationTag(groupId).ToLower().Trim()))
                return true;

            annExternalId = announcement.ExternalId;
            singleQueueName = announcement.QueueName;

            # region get asset details from catalog
            // for tv series msg ref is asset id of the asset msg is for.
            int assetId = ODBCWrapper.Utils.GetIntSafeVal(messageAnnouncementDataRow, "message_reference");
            if (assetId == 0)
            {
                log.DebugFormat("HandleRecipientTypeOther: asset not found for message announcement with recipients 'other': group: {0} Id: {1}", groupId, messageId);
                return false;
            }

            // send get media to catalog
            string catalogUrl = Core.Notification.Utils.GetWSURL(NotificationUtils.CATALOG_WS);
            var request = new MediasProtocolRequest()
            {
                m_sSignature = NotificationUtils.GetSignature(CatalogSignString, CatalogSignatureKey),
                m_sSignString = CatalogSignString,
                m_lMediasIds = new List<int> { assetId },
                m_nGroupID = groupId,
                m_oFilter = new Filter()
            };

            try
            {
                response = request.GetMediasByIDs(request);
            }
            catch (Exception ex)
            {
                log.Error("HandleRecipientTypeOther: error when calling catalog: ", ex);
                return false;
            }
            ///////

            // check response params
            if (response != null && response.m_lObj !=
                null && response.m_lObj.Count > 0 &&
                response.m_lObj.First() != null &&
                response.m_lObj.First() is MediaObj)
            {
                MediaObj mediaObj = response.m_lObj.First() as MediaObj;
                if (mediaObj.m_lTags != null && mediaObj.m_lTags.Count > 0 && mediaObj.m_lTags.First() != null)
                {
                    catalogStartDateStr = mediaObj.m_dCatalogStartDate;
                    catalogStartDate = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(mediaObj.m_dCatalogStartDate);
                    startDate = mediaObj.m_dStartDate;
                    mediaName = mediaObj.m_sName;

                    // check if part of a series
                    foreach (var tag in mediaObj.m_lTags)
                    {
                        if (tag.m_oTagMeta != null &&
                            tag.m_oTagMeta.m_sName.ToLower().Trim() == FollowManager.GetEpisodeAssociationTag(groupId).ToLower().Trim())
                        {
                            seriesNames = tag.m_lValues.ToArray();
                        }
                    }
                }
            }

            // check series name was found
            if (seriesNames == null || seriesNames.Length == 0)
            {
                log.DebugFormat("HandleRecipientTypeOther: couldn't get series from catalog: group {0}, media: {1}", groupId, assetId);
                return false;
            }
            #endregion

            long utcNow = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(DateTime.UtcNow);

            // get message announcements of announcement.
            DataRowCollection drs = DAL.NotificationDal.Get_MessageAnnouncementByAnnouncementId(announcementId);

            // check in all series messages if any msg was sent in the last 24h
            if (drs != null && drs.Count > 0)
                foreach (DataRow row in drs)
                {
                    MessageAnnouncement msg = Core.Notification.Utils.GetMessageAnnouncementFromDataRow(row);

                    if (msg.Status == eAnnouncementStatus.Sent)
                    {
                        // if message already sent for asset in the last 24h and edited asset is also for the next 24h, abort message.
                        if ((utcNow - msg.StartTime) < new TimeSpan(24, 0, 0).TotalSeconds)
                        {
                            log.DebugFormat("HandleRecipientOtherTvSeries: Found a sent message in the last 24h, new message will not be sent. old msg name: {0}, old msg time: {1}, message time: {2}", msg.Name, msg.StartTime, startTime);
                            // sent in last 24 hours abort
                            failRes = true;
                            return false;
                        }
                    }
                }

            log.DebugFormat("HandleRecipientOtherTvSeries: about to send message announcement for: group {0}, asset: {1}, id: {2}", groupId, assetId, messageId);

            // get msg template and build msg with it
            var msgTemplateResponse = FollowManager.GetMessageTemplate(groupId, MessageTemplateType.Series);
            if (msgTemplateResponse != null &&
                msgTemplateResponse.Status != null &&
                msgTemplateResponse.Status.Code == (int)eResponseStatus.OK &&
                msgTemplateResponse.MessageTemplate != null)
            {
                category = msgTemplateResponse.MessageTemplate.Action;
                sound = msgTemplateResponse.MessageTemplate.Sound;
                url = msgTemplateResponse.MessageTemplate.URL.Replace("{" + eFollowSeriesPlaceHolders.CatalaogStartDate + "}", catalogStartDateStr.ToString(msgTemplateResponse.MessageTemplate.DateFormat)).
                                                            Replace("{" + eFollowSeriesPlaceHolders.MediaId + "}", assetId.ToString()).
                                                            Replace("{" + eFollowSeriesPlaceHolders.MediaName + "}", mediaName).
                                                            Replace("{" + eFollowSeriesPlaceHolders.SeriesName + "}", (seriesNames != null && seriesNames.Length > 0) ? seriesNames[0] : string.Empty).
                                                            Replace("{" + eFollowSeriesPlaceHolders.StartDate + "}", startDate.ToString(msgTemplateResponse.MessageTemplate.DateFormat)); ;

                string message = ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "message");
                if (!string.IsNullOrEmpty(message))
                    messageAnnouncementDataRow["message"] = message.Replace("{" + eFollowSeriesPlaceHolders.CatalaogStartDate + "}", catalogStartDateStr.ToString(msgTemplateResponse.MessageTemplate.DateFormat)).
                                            Replace("{" + eFollowSeriesPlaceHolders.MediaId + "}", assetId.ToString()).
                                            Replace("{" + eFollowSeriesPlaceHolders.MediaName + "}", mediaName).
                                            Replace("{" + eFollowSeriesPlaceHolders.SeriesName + "}", (seriesNames != null && seriesNames.Length > 0) ? seriesNames[0] : string.Empty).
                                            Replace("{" + eFollowSeriesPlaceHolders.StartDate + "}", startDate.ToString(msgTemplateResponse.MessageTemplate.DateFormat));
            }

            return true;
        }

        public static GetAllMessageAnnouncementsResponse Get_AllMessageAnnouncements(int groupId, int pageSize, int pageIndex)
        {
            GetAllMessageAnnouncementsResponse ret = new GetAllMessageAnnouncementsResponse();
            ret.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            // validate system announcements are enabled
            if (!NotificationSettings.IsPartnerSystemAnnouncementEnabled(groupId))
            {
                log.ErrorFormat("CreateSystemAnnouncement  - partner system announcements are disabled. groupID = {0}", groupId);
                ret.Status = new Status((int)eResponseStatus.FeatureDisabled, "Feature Disabled");
                return ret;
            }

            ret.messageAnnouncements = new List<MessageAnnouncement>();

            List<DataRow> rows = DAL.NotificationDal.Get_MessageAllAnnouncements(groupId, pageSize, pageIndex);

            if (rows != null)
            {
                foreach (DataRow row in rows)
                {
                    string timezone = ODBCWrapper.Utils.GetSafeStr(row, "timezone");

                    DateTime convertedTime = ODBCWrapper.Utils.ConvertFromUtc(ODBCWrapper.Utils.GetDateSafeVal(row, "start_time"), timezone);
                    long startTime = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(convertedTime);
                    ApiObjects.eAnnouncementRecipientsType recipients = ApiObjects.eAnnouncementRecipientsType.Other;
                    int dbRecipients = ODBCWrapper.Utils.GetIntSafeVal(row, "recipients");
                    if (Enum.IsDefined(typeof(ApiObjects.eAnnouncementRecipientsType), dbRecipients))
                        recipients = (ApiObjects.eAnnouncementRecipientsType)dbRecipients;

                    eAnnouncementStatus status = eAnnouncementStatus.NotSent;
                    int dbStatus = ODBCWrapper.Utils.GetIntSafeVal(row, "sent");
                    if (Enum.IsDefined(typeof(eAnnouncementStatus), dbStatus))
                        status = (eAnnouncementStatus)dbStatus;

                    MessageAnnouncement msg = new MessageAnnouncement(ODBCWrapper.Utils.GetSafeStr(row, "name"),
                                                                      ODBCWrapper.Utils.GetSafeStr(row, "message"),
                                                                      (ODBCWrapper.Utils.GetIntSafeVal(row, "is_active") == 0) ? false : true,
                                                                      startTime,
                                                                      timezone,
                                                                      recipients,
                                                                      status);

                    msg.MessageAnnouncementId = ODBCWrapper.Utils.GetIntSafeVal(row, "id");

                    ret.messageAnnouncements.Add(msg);
                }

                ret.totalCount = DAL.NotificationDal.Get_MessageAllAnnouncementsCount(groupId);
            }

            return ret;
        }

        private static MessageAnnouncement AddMessageAnnouncementToDB(int groupId, MessageAnnouncement announcement)
        {
            try
            {
                DateTime announcementStartTime = ODBCWrapper.Utils.UnixTimestampToDateTime(announcement.StartTime);
                DataRow row = DAL.NotificationDal.Insert_MessageAnnouncement(groupId, (int)announcement.Recipients, announcement.Name, announcement.Message, announcement.Enabled, announcementStartTime, announcement.Timezone, 0, announcement.AnnouncementId, announcement.MessageReference);
                return Core.Notification.Utils.GetMessageAnnouncementFromDataRow(row);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("AddMessageAnnouncement Exception = {0}", ex.Message));
                return null;
            }
        }

        private static bool AddMessageAnnouncementToQueue(int groupId, MessageAnnouncement announcement)
        {
            MessageAnnouncementQueue que = new MessageAnnouncementQueue();
            MessageAnnouncementData messageAnnouncementData = new MessageAnnouncementData(groupId, announcement.StartTime, announcement.MessageAnnouncementId)
            {
                ETA = ODBCWrapper.Utils.UnixTimestampToDateTime(announcement.StartTime)
            };

            bool res = que.Enqueue(messageAnnouncementData, ROUTING_KEY_CHECK_PENDING_TRANSACTION);

            if (res)
                log.DebugFormat("Successfully inserted a message to announcement queue: {0}", messageAnnouncementData);
            else
                log.ErrorFormat("Error while inserting announcement {0} to queue", messageAnnouncementData);

            return res;
        }

        private static bool ConvertStartTimeToUtc(ref MessageAnnouncement announcement)
        {
            try
            {
                DateTime announcementStartTime = ODBCWrapper.Utils.UnixTimestampToDateTime(announcement.StartTime);
                DateTime convertedTime = ODBCWrapper.Utils.ConvertToUtc(announcementStartTime, announcement.Timezone);
                announcement.StartTime = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(convertedTime);
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetUtcTime: caught an exceptions for time zone {0}: {1}", announcement.Timezone, ex);
            }

            return false;
        }

        private static Status ValidateAnnouncement(int groupId, MessageAnnouncement announcement)
        {
            if (string.IsNullOrEmpty(announcement.Message))
            {
                log.ErrorFormat("Empty message is not valid.");
                return new Status((int)eResponseStatus.AnnouncementMessageIsEmpty, "Announcement Message Is Empty");
            }

            if (ASCIIEncoding.Unicode.GetByteCount(announcement.Message) > MAX_MSG_LENGTH)
            {
                log.ErrorFormat("Message too long");
                return new Status((int)eResponseStatus.AnnouncementMessageTooLong, "Announcement Message Too Long");
            }
            return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
        }

        public static Status CreateSystemAnnouncement(int groupId)
        {
            try
            {
                // validate system announcements are enabled
                if (!NotificationSettings.IsPartnerSystemAnnouncementEnabled(groupId))
                {
                    log.ErrorFormat("CreateSystemAnnouncement  - partner system announcements are disabled. groupID = {0}", groupId);
                    return new Status((int)eResponseStatus.FeatureDisabled, "Feature Disabled");
                }

                // create guest topic
                string announcementName = "Guest";
                string externalAnnouncementId = NotificationAdapter.CreateAnnouncement(groupId, announcementName);
                if (string.IsNullOrEmpty(externalAnnouncementId))
                {
                    log.ErrorFormat("CreateSystemAnnouncement failed Create guest announcement groupID = {0}, announcementName = {1}", groupId, announcementName);
                    return new Status((int)eResponseStatus.FailCreateAnnouncement, "fail create Guest announcement");
                }

                // insert ARN to DB
                if (DAL.NotificationDal.Insert_Announcement(groupId, announcementName, externalAnnouncementId, (int)eMessageType.Push, (int)eAnnouncementRecipientsType.Guests) == 0)
                {
                    log.ErrorFormat("CreateSystemAnnouncement failed insert guest announcement to DB groupID = {0}, announcementName = {1}", groupId, announcementName);
                    return new Status((int)eResponseStatus.Error, "fail insert guest announcement to DB");
                }

                // create logged-in topic
                announcementName = "LoggedIn";
                externalAnnouncementId = string.Empty;
                externalAnnouncementId = NotificationAdapter.CreateAnnouncement(groupId, announcementName);
                if (string.IsNullOrEmpty(externalAnnouncementId))
                {
                    log.ErrorFormat("CreateSystemAnnouncement failed Create logged in announcement groupID = {0}, announcementName = {1}", groupId, announcementName);
                    return new Status((int)eResponseStatus.FailCreateAnnouncement, "fail create LoggedIn announcement");
                }

                // insert ARN to DB 
                if (DAL.NotificationDal.Insert_Announcement(groupId, announcementName, externalAnnouncementId, (int)eMessageType.Push, (int)eAnnouncementRecipientsType.LoggedIn) == 0)
                {
                    log.ErrorFormat("CreateSystemAnnouncement failed insert logged in announcement to DB groupID = {0}, announcementName = {1}", groupId, announcementName);
                    return new Status((int)eResponseStatus.Error, "fail insert Logged in announcement to DB");
                }

                NotificationCache.Instance().RemoveAnnouncementsFromCache(groupId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CreateSystemAnnouncement failed groupId = {0}, ex = {1}", groupId, ex);
                return new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()); ;
        }

        public static bool SendMessageAnnouncement(int groupId, long startTime, int messageId)
        {
            string url = string.Empty;
            string sound = string.Empty;
            string category = string.Empty;

            // get message announcements
            DataRow messageAnnouncementDataRow = DAL.NotificationDal.Get_MessageAnnouncementWithActiveStatus(messageId);
            if (messageAnnouncementDataRow == null)
            {
                log.ErrorFormat("message announcement couldn't be found on DB. group: {0} message Id: {1}", groupId, messageId);
                return false;
            }

            // validate start time is same as DB start time
            if (ODBCWrapper.Utils.GetDateSafeVal(messageAnnouncementDataRow, "start_time") != ODBCWrapper.Utils.UnixTimestampToDateTime(startTime))
            {
                DAL.NotificationDal.Update_MessageAnnouncementActiveStatus(groupId, messageId, 0);
                log.DebugFormat("Announcement start time is different than DB start time. DB start time: {0}, announcement start time: {1} group: {2} Id: {3}",
                    ODBCWrapper.Utils.GetDateSafeVal(messageAnnouncementDataRow, "start_time"),
                    ODBCWrapper.Utils.UnixTimestampToDateTime(startTime),
                    groupId,
                    messageId);
                return false;
            }

            // validate recipient type is legal
            eAnnouncementRecipientsType recipients;
            if (!Enum.TryParse<eAnnouncementRecipientsType>(ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "recipients"), out recipients))
            {
                DAL.NotificationDal.Update_MessageAnnouncementActiveStatus(groupId, messageId, 0);
                log.ErrorFormat("invalid recipients type for announcement {0}", messageId);
                return false;
            }

            long currentTimeSec = ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);
            string singleTopicExternalId = string.Empty;
            string singleQueueName = string.Empty;
            List<string> topicExternalIds = new List<string>();
            List<string> queueNames = new List<string>();

            switch (recipients)
            {
                case eAnnouncementRecipientsType.All:

                    if (NotificationSettings.IsPartnerPushEnabled(groupId))
                    {
                        // get topic push external id's of guests and logged in users
                        var announcements = NotificationCache.Instance().GetAnnouncements(groupId).Where(x => x.RecipientsType == eAnnouncementRecipientsType.LoggedIn || x.RecipientsType == eAnnouncementRecipientsType.Guests);
                        foreach (var announcement in announcements)
                            topicExternalIds.Add(announcement.ExternalId);
                    }

                    // send inbox messages
                    if (NotificationSettings.IsPartnerInboxAnnouncementEnabled(groupId))
                    {
                        InboxMessage inboxMessage = new InboxMessage()
                        {
                            Category = eMessageCategory.SystemAnnouncement,
                            CreatedAtSec = currentTimeSec,
                            Id = ODBCWrapper.Utils.GetIntSafeVal(messageAnnouncementDataRow, "ID").ToString(),
                            Message = ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "message"),
                            State = eMessageState.Unread,
                            UpdatedAtSec = currentTimeSec,
                            Url = url
                        };

                        if (!NotificationDal.SetSystemAnnouncementMessage(groupId, inboxMessage, NotificationSettings.GetInboxMessageTTLDays(groupId)))
                            log.ErrorFormat("Error while setting system announcement inbox message. GID: {0}, InboxMessage: {1}", groupId, JsonConvert.SerializeObject(inboxMessage));
                    }

                    // add the Q name to list to be sent to later
                    var loggedInAnnouncement = NotificationCache.Instance().GetAnnouncements(groupId).FirstOrDefault(x => x.RecipientsType == eAnnouncementRecipientsType.LoggedIn);
                    if (loggedInAnnouncement != null && !string.IsNullOrEmpty(loggedInAnnouncement.QueueName))
                        queueNames.Add(loggedInAnnouncement.QueueName);

                    break;

                case eAnnouncementRecipientsType.Guests:

                    if (NotificationSettings.IsPartnerPushEnabled(groupId))
                    {
                        // get topic push external id's of guests users
                        singleTopicExternalId = DAL.NotificationDal.Get_AnnouncementExternalIdByRecipients(groupId, (int)recipients);
                        if (!string.IsNullOrEmpty(singleTopicExternalId))
                            topicExternalIds.Add(singleTopicExternalId);
                        else
                        {
                            DAL.NotificationDal.Update_MessageAnnouncementActiveStatus(groupId, messageId, 0);
                            log.ErrorFormat("external announcement id is empty for announcement {0}", messageId);
                            return false;
                        }
                    }

                    break;

                case eAnnouncementRecipientsType.LoggedIn:

                    if (NotificationSettings.IsPartnerPushEnabled(groupId))
                    {
                        // get topic push external id's of logged-in users
                        singleTopicExternalId = DAL.NotificationDal.Get_AnnouncementExternalIdByRecipients(groupId, (int)recipients);
                        if (!string.IsNullOrEmpty(singleTopicExternalId))
                            topicExternalIds.Add(singleTopicExternalId);
                        else
                        {
                            DAL.NotificationDal.Update_MessageAnnouncementActiveStatus(groupId, messageId, 0);
                            log.ErrorFormat("external announcement id is empty for announcement {0}", messageId);
                            return false;
                        }
                    }

                    // send inbox messages
                    if (NotificationSettings.IsPartnerInboxAnnouncementEnabled(groupId))
                    {
                        InboxMessage inboxMessage = new InboxMessage()
                        {
                            Category = eMessageCategory.SystemAnnouncement,
                            CreatedAtSec = currentTimeSec,
                            Id = ODBCWrapper.Utils.GetIntSafeVal(messageAnnouncementDataRow, "ID").ToString(),
                            Message = ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "message"),
                            State = eMessageState.Unread,
                            UpdatedAtSec = currentTimeSec,
                            Url = url
                        };

                        if (!NotificationDal.SetSystemAnnouncementMessage(groupId, inboxMessage, NotificationSettings.GetInboxMessageTTLDays(groupId)))
                            log.ErrorFormat("Error while setting system announcement inbox message. GID: {0}, InboxMessage: {1}", groupId, JsonConvert.SerializeObject(inboxMessage));
                    }

                    // add the Q name to list to be sent to later
                    var loggedInAnn = NotificationCache.Instance().GetAnnouncements(groupId).FirstOrDefault(x => x.RecipientsType == eAnnouncementRecipientsType.LoggedIn);
                    if (loggedInAnn != null && !string.IsNullOrEmpty(loggedInAnn.QueueName))
                        queueNames.Add(loggedInAnn.QueueName);

                    break;

                case eAnnouncementRecipientsType.Other:

                    bool res;

                    // get announcement id (of series)
                    int announcementId = ODBCWrapper.Utils.GetIntSafeVal(messageAnnouncementDataRow, "announcement_id");
                    if (announcementId == 0)
                    {
                        log.DebugFormat("Announcement id invalid for message announcement with recipients 'other': group: {0} message ID: {1}", groupId, ODBCWrapper.Utils.GetIntSafeVal(messageAnnouncementDataRow, "ID"));
                        return false;
                    }

                    if (!HandleRecipientOtherTvSeries(groupId, messageId, startTime, announcementId, ref messageAnnouncementDataRow, ref url, ref sound, ref category, out singleTopicExternalId, out singleQueueName, out res))
                    {
                        DAL.NotificationDal.Update_MessageAnnouncementActiveStatus(groupId, messageId, 0);
                        return res;
                    }

                    // add the topic to list to be sent to later
                    if (!string.IsNullOrEmpty(singleTopicExternalId))
                        topicExternalIds.Add(singleTopicExternalId);

                    // add the Q name to list to be sent to later
                    if (!string.IsNullOrEmpty(singleQueueName))
                        queueNames.Add(singleQueueName);


                    // send inbox messages
                    if (NotificationSettings.IsPartnerInboxAnnouncementEnabled(groupId))
                    {
                        List<int> followingUserIds = NotificationDal.GetUsersFollowNotificationView(groupId, announcementId);
                        if (followingUserIds != null)
                            foreach (var userId in followingUserIds)
                            {
                                InboxMessage inboxMessage = new InboxMessage()
                                {
                                    Category = eMessageCategory.Followed,
                                    CreatedAtSec = currentTimeSec,
                                    Id = Guid.NewGuid().ToString(),
                                    Message = ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "message"),
                                    State = eMessageState.Unread,
                                    UpdatedAtSec = currentTimeSec,
                                    Url = url,
                                    UserId = userId
                                };

                                if (!NotificationDal.SetUserInboxMessage(groupId, inboxMessage, NotificationSettings.GetInboxMessageTTLDays(groupId)))
                                {
                                    log.ErrorFormat("Error while setting user follow series inbox message. GID: {0}, InboxMessage: {1}",
                                        groupId,
                                        JsonConvert.SerializeObject(inboxMessage));
                                }
                            }
                    }
                    break;
            }

            string resultMsgIds = "";

            // send push messages
            if (NotificationSettings.IsPartnerPushEnabled(groupId))
            {
                // send to Amazon
                if (topicExternalIds != null && topicExternalIds.Count > 0)
                {
                    foreach (string extAnnouncementId in topicExternalIds)
                    {
                        string resultMsgId = NotificationAdapter.PublishToAnnouncement(groupId, extAnnouncementId, string.Empty, new MessageData() { Alert = ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "message"), Url = url, Sound = sound, Category = category });
                        if (!string.IsNullOrEmpty(resultMsgIds))
                            resultMsgIds += ",";
                        resultMsgIds += resultMsgId;
                    }

                    if (string.IsNullOrEmpty(resultMsgIds))
                        log.ErrorFormat("failed to publish message to push topic. result message id is empty for announcement {0}", messageId);
                }
                else
                    log.DebugFormat("no topic external IDs found for message ID: {0}", messageId);

                // send to push web - rabbit.                
                if (queueNames != null && queueNames.Count > 0)
                {
                    MessageAnnouncementFullData data = new MessageAnnouncementFullData(groupId, ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "message"), url, sound, category, startTime);
                    foreach (string qName in queueNames)
                    {
                        // enqueue message with small expiration date
                        GeneralDynamicQueue q = new GeneralDynamicQueue(qName, QueueWrapper.Enums.ConfigType.PushNotifications);
                        if (!q.Enqueue(data, qName, PUSH_MESSAGE_EXPIRATION_MILLI_SEC))
                        {
                            log.ErrorFormat("Failed pushing message announcement to queue for web push. announcement ID: {0}, message ID: {1}, Queue name: {2}",
                                ODBCWrapper.Utils.GetIntSafeVal(messageAnnouncementDataRow, "announcement_id"),
                                messageId,
                                qName);
                        }
                        else
                        {
                            log.DebugFormat("Inserted message to web push queue date: {0}, announcement ID: {1}, message ID: {2}, Queue name: {3}",
                             JsonConvert.SerializeObject(data),
                             ODBCWrapper.Utils.GetIntSafeVal(messageAnnouncementDataRow, "announcement_id"),
                             messageId,
                             qName);
                        }
                    }
                }
                else
                    log.DebugFormat("no queues found for message ID: {0}", messageId);
            }

            log.DebugFormat("Successfully sent announcement: Id: {0}", messageId);
            DAL.NotificationDal.Update_MessageAnnouncementSent(messageId, groupId, (int)eAnnouncementStatus.Sent);
            DAL.NotificationDal.Update_MessageAnnouncementResultMessageId(messageId, groupId, resultMsgIds);
            return true;
        }

        public static Status DeleteAnnouncement(int groupId, long announcementId)
        {
            Status responseStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            string logData = string.Format("GID: {0}, announcementId: {1}", groupId, announcementId);

            // get announcement
            var announcement = NotificationCache.Instance().GetAnnouncements(groupId).FirstOrDefault(x => x.ID == announcementId);
            if (announcement == null)
            {
                log.ErrorFormat("announcement ID wasn't found. {0}", logData);
                responseStatus = new Status() { Code = (int)eResponseStatus.AnnouncementNotFound, Message = eResponseStatus.AnnouncementNotFound.ToString() };
                return responseStatus;
            }

            // delete Amazon topic

            if (announcement.ExternalId != null &&
                !NotificationAdapter.DeleteAnnouncement(groupId, announcement.ExternalId))
            {
                log.ErrorFormat("Error while trying to delete follow series topic from external adapter. {0}, external topic ID: {1}",
                    logData,
                    announcement.ExternalId != null ? announcement.ExternalId : string.Empty);
            }
            else
            {
                log.DebugFormat("Successfully removed follow series announcement from external adapter. {0},  external topic ID: {1}",
                    logData,
                    announcement.ExternalId != null ? announcement.ExternalId : string.Empty);
            }

            // delete announcement from DB
            if (!NotificationDal.DeleteAnnouncement(groupId, announcementId))
            {
                log.ErrorFormat("Error while trying to delete DB announcement. {0}", logData);
            }
            else
            {
                responseStatus = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                log.DebugFormat("Successfully removed DB announcement. {0}", logData);

                // remove announcements from cache
                NotificationCache.Instance().RemoveAnnouncementsFromCache(groupId);
            }

            return responseStatus;
        }

        public static Status DeleteAnnouncementsOlderThan(ref bool createNextRunningIteration, ref double nextIntervalSec)
        {
            createNextRunningIteration = true;
            Status responseStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            List<NotificationPartnerSettings> partnerNotificationSettings = new List<NotificationPartnerSettings>();
            nextIntervalSec = NOTIFICATION_CLEANUP_INTERVAL_SEC;
            DateTime currentTime = DateTime.UtcNow;
            log.DebugFormat("Starting DeleteAnnouncementsOlderThan iteration");
            BaseScheduledTaskLastRunDetails notificationCleanupTask = new BaseScheduledTaskLastRunDetails(ScheduledTaskType.notificationCleanup);
            ScheduledTaskLastRunDetails lastRunDetails = notificationCleanupTask.GetLastRunDetails();
            notificationCleanupTask = lastRunDetails != null ? (BaseScheduledTaskLastRunDetails)lastRunDetails : null;

            if (notificationCleanupTask != null && notificationCleanupTask.Status.Code == (int)eResponseStatus.OK && notificationCleanupTask.NextRunIntervalInSeconds > 0)
            {
                nextIntervalSec = notificationCleanupTask.NextRunIntervalInSeconds;
                if (notificationCleanupTask.LastRunDate.AddSeconds(nextIntervalSec) > currentTime)
                {
                    log.ErrorFormat("Cannot start notification cleanup iteration - minimum time haven't passed. current time: {0}, last running date: {1}, minimum date: {2}",
                                     currentTime,
                                     notificationCleanupTask.LastRunDate,
                                     notificationCleanupTask.LastRunDate.AddSeconds(nextIntervalSec));
                    createNextRunningIteration = false;
                    return responseStatus;
                }
            }

            // get partner/s notification settings
            partnerNotificationSettings.AddRange(NotificationDal.GetNotificationPartnerSettings(0));
            if (partnerNotificationSettings == null || partnerNotificationSettings.Count == 0)
            {
                log.Error("Error getting partners notification settings.");
                return responseStatus;
            }

            int totalAnnouncementsDeleted = 0;
            foreach (var partnerSettings in partnerNotificationSettings)
            {
                // get all announcements
                var announcements = NotificationCache.Instance().GetAnnouncements(partnerSettings.PartnerId);
                if (announcements == null)
                {
                    log.ErrorFormat("Error getting announcements. GID {0}", partnerSettings.PartnerId);
                    continue;
                }

                foreach (var announcement in announcements)
                {
                    // check if topic expiration passed
                    if (announcement.RecipientsType == eAnnouncementRecipientsType.Other &&
                        announcement.LastMessageSentDateSec > 0 &&
                        TimeSpan.FromSeconds(TVinciShared.DateUtils.UnixTimeStampNow() - announcement.LastMessageSentDateSec).TotalDays > partnerSettings.TopicExpirationDurationDays)
                    {
                        Status deleteAnnouncementResp = DeleteAnnouncement(partnerSettings.PartnerId, announcement.ID);
                        if (deleteAnnouncementResp != null && deleteAnnouncementResp.Code != (int)eResponseStatus.OK)
                        {
                            log.ErrorFormat("Error while trying to delete old topic. GID: {0}, Announcement ID: {1}, Announcement updated at: {2}, topic expiration duration in days: {3}",
                                partnerSettings.PartnerId,
                                announcement.ID,
                                TVinciShared.DateUtils.UnixTimeStampToDateTime(announcement.LastMessageSentDateSec).ToString(),
                                partnerSettings.TopicExpirationDurationDays);
                        }
                        else
                        {
                            totalAnnouncementsDeleted++;
                            log.DebugFormat("successfully deleted old topic. GID: {0}, Announcement ID: {1}, Announcement updated at: {2}, topic expiration duration in days: {3}",
                                partnerSettings.PartnerId,
                                announcement.ID,
                                TVinciShared.DateUtils.UnixTimeStampToDateTime(announcement.LastMessageSentDateSec).ToString(),
                                partnerSettings.TopicExpirationDurationDays);
                        }
                    }
                }
            }

            // update last run details
            notificationCleanupTask = new BaseScheduledTaskLastRunDetails(currentTime, totalAnnouncementsDeleted, nextIntervalSec, ScheduledTaskType.notificationCleanup);
            if (!notificationCleanupTask.SetLastRunDetails())
            {
                log.ErrorFormat("Error while trying to update notification cleanup last run details, NotificationCleanupResponse: {0}", notificationCleanupTask.ToString());
                return responseStatus;
            }

            log.DebugFormat("Finished DeleteAnnouncementsOlderThan iteration");

            responseStatus = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
            return responseStatus;
        }

        public static Dictionary<string, int> GetAmountOfSubscribersPerAnnouncement(int groupId)
        {
            Dictionary<string, int> result = null;

            try
            {
                List<KeyValuePair<object, int>> amountOfSubscribersPerAnnouncement = NotificationDal.GetAmountOfSubscribersPerAnnouncement(groupId);

                if (amountOfSubscribersPerAnnouncement == null)
                    log.DebugFormat("No users following the announcement were found. GID: {0}", groupId);
                else
                {
                    //build result for AnnouncementId (key) AmountOfSubscribers(value)
                    result = new Dictionary<string, int>();
                    try
                    {
                        string key = string.Empty;

                        foreach (var item in amountOfSubscribersPerAnnouncement)
                        {
                            key = item.Key.ToString().Replace(']', ' ').Split(',')[1].Trim();
                            if (!result.ContainsKey(key))
                                result.Add(key, item.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error while creating amountOfSubscribersPerAnnouncement Table GID: {0}", groupId, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while creating amountOfSubscribersPerAnnouncement Table GID: {0}", groupId, ex);
            }

            return result;

        }

        public static Status UpdateAnnouncement(int groupId, int announcementId, eTopicAutomaticIssueNotification topicAutomaticIssueNotification)
        {
            string logData = string.Format("GID: {0}, AnnouncementId: {1}, topicAutomaticIssueNotification: {2}", groupId, announcementId, topicAutomaticIssueNotification.ToString());

            try
            {
                // check if Announcement exist
                // get announcement
                var announcement = NotificationCache.Instance().GetAnnouncements(groupId).FirstOrDefault(x => x.ID == announcementId);
                if (announcement == null)
                {
                    log.ErrorFormat("Announcement not exist in DB: {0}", logData);
                    return new Status((int)eResponseStatus.AnnouncementNotFound, ANNOUNCEMENT_NOT_FOUND);
                }

                //automaticSending according to topicAutomaticIssueNotification enum
                bool? automaticSending = null;

                switch (topicAutomaticIssueNotification)
                {
                    case eTopicAutomaticIssueNotification.Yes:
                        automaticSending = true;
                        break;
                    case eTopicAutomaticIssueNotification.No:
                        automaticSending = false;
                        break;
                    case eTopicAutomaticIssueNotification.Default:
                    default:
                        automaticSending = null;
                        break;
                }

                //update announcement 
                var isSet = NotificationDal.UpdateAnnouncement(groupId, announcementId, automaticSending);

                if (!isSet)
                {
                    log.ErrorFormat("Failed updating announcement. {0}", logData);
                    return new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
                else
                {
                    // remove announcement from cache
                    NotificationCache.Instance().RemoveAnnouncementsFromCache(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at UpdateAnnouncement. {0}", logData, ex);
                return new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()); ;
        }

        public static AnnouncementsResponse GetAnnouncement(int groupId, int announcementId)
        {
            AnnouncementsResponse response = new AnnouncementsResponse();

            string logData = string.Format("GID: {0}, id: {1}", groupId, announcementId);

            try
            {
                // get announcement
                var announcement = NotificationCache.Instance().GetAnnouncements(groupId).FirstOrDefault(x => x.ID == announcementId);
                if (announcement != null)
                {
                    response.Announcements = new List<DbAnnouncement>();
                    response.Announcements.Add(announcement);
                    response.TotalCount = 1;
                    // add  amountOfSubscribers to announcements result
                    SetAmountOfSubscibers(groupId, ref response);
                    response.Status = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                }
                else
                {
                    log.ErrorFormat("announcement ID wasn't found. GID: {0}, announcementId: {1}", groupId, announcementId);
                    response.Status = new Status() { Code = (int)eResponseStatus.AnnouncementNotFound, Message = ANNOUNCEMENT_NOT_FOUND };
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetAnnouncement {0}", logData, ex);
                response.Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            }

            return response;
        }

        public static AnnouncementsResponse GetAnnouncements(int groupId, int pageSize, int pageIndex)
        {
            AnnouncementsResponse response = new AnnouncementsResponse();

            string logData = string.Format("GID: {0}, pageSize: {1}, pageIndex: {2}", groupId, pageSize, pageIndex);

            try
            {
                // get announcements
                var topicAnnouncements = NotificationCache.Instance().GetAnnouncements(groupId).Where(x => x.RecipientsType == eAnnouncementRecipientsType.Other).ToList();
                if (topicAnnouncements == null)
                {
                    log.ErrorFormat("Error while trying to fetch Other announcement from DB. {0}", logData);
                    response.Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
                    return response;
                }

                response.TotalCount = topicAnnouncements.Count;
                // paging
                response.Announcements = topicAnnouncements.Skip(pageSize * pageIndex).Take(pageSize).ToList();

                // add  amountOfSubscribers to announcements result
                SetAmountOfSubscibers(groupId, ref response);

                response.Status = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in GetAnnouncements {0}", logData, ex);
            }

            return response;
        }

        private static void SetAmountOfSubscibers(int groupId, ref AnnouncementsResponse response)
        {
            // add  amountOfSubscribers to announcements result
            var dictAmountOfSubscribersPerAnnouncement = GetAmountOfSubscribersPerAnnouncement(groupId);
            string announcementId = string.Empty;   // dictAmountOfSubscribersPerAnnouncement key
            int amountOfSubscribers = 0;            // dictAmountOfSubscribersPerAnnouncement value

            foreach (var announcement in response.Announcements)
            {
                announcementId = announcement.ID.ToString();
                //in case announcementId exist at Dic, add the subscribers amount
                if (dictAmountOfSubscribersPerAnnouncement.ContainsKey(announcementId))
                {

                    dictAmountOfSubscribersPerAnnouncement.TryGetValue(announcementId, out amountOfSubscribers);
                    announcement.SubscribersAmount = amountOfSubscribers;
                }
            }
        }

        public static Status CreateNextNotificationCleanupIteration(eSetupTask task, DateTime nextIteration)
        {
            Status result = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            var queue = new SetupTasksQueue();
            CelerySetupTaskData data;
            switch (task)
            {
                case eSetupTask.NotificationSeriesCleanupIteration:

                    data = new CelerySetupTaskData(0, task, new Dictionary<string, object>()) { ETA = nextIteration };
                    break;

                case eSetupTask.ReminderCleanupIteration:

                    data = new CelerySetupTaskData(0, task, new Dictionary<string, object>()) { ETA = nextIteration };
                    break;

                default:
                    result.Message = string.Format("cleaning type was not implemented. task: {0}", task.ToString());
                    return result;
            }

            try
            {
                var success = queue.Enqueue(data, ROUTING_KEY_NOTIFICATION_CLEANUP);
                if (success)
                {
                    log.DebugFormat("next notification cleanup notification message was created for task: {0}. running date: {1}", task.ToString(), nextIteration.ToString());
                    result.Code = (int)eResponseStatus.OK;
                    result.Message = eResponseStatus.OK.ToString();
                }
                else
                    log.ErrorFormat("error creating next notification cleanup notification for task: {0}.", task.ToString());
            }
            catch (Exception ex)
            {
                log.Error("Error in CreateNextNotificationCleanupIteration", ex);
            }

            return result;
        }

        public static RegistryResponse RegisterPushAnnouncementParameters(int groupId, long announcementId, string hash, string ip)
        {
            RegistryResponse response = new RegistryResponse();
            response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            List<long> announcementIds = new List<long>();
            // build queue name
            string queueName = string.Format(ANNOUNCEMENT_QUEUE_NAME_FORMAT, groupId, announcementId);

            // get relevant announcement
            var announcement = NotificationCache.Instance().GetAnnouncements(groupId).Where(x => x.ID == announcementId).FirstOrDefault();
            if (announcement == null)
            {
                log.ErrorFormat("GetPushWebParams: announcement not found. id: {0}.", announcementId);
                response.Status = new Status((int)eResponseStatus.ItemNotFound, eResponseStatus.ItemNotFound.ToString());
            }
            else
            {
                // update web push queue name on DB (if necessary)
                if (string.IsNullOrEmpty(announcement.QueueName))
                {
                    if (!DAL.NotificationDal.UpdateAnnouncement(groupId, (int)announcementId, announcement.AutomaticIssueFollowNotification, queueName: queueName))
                    {
                        log.ErrorFormat("Error while trying to update announcement with web push queue name. GID: {0}, announcing ID: {1}, queue name: {2}",
                            groupId,
                            announcement.ID,
                            queueName);
                    }
                    else
                    {
                        log.DebugFormat("Successfully update announcement with web push queue name. GID: {0}, announcing ID: {1}, queue name: {2}",
                            groupId,
                            announcement.ID,
                            queueName);

                        // remove announcements from cache
                        NotificationCache.Instance().RemoveAnnouncementsFromCache(groupId);
                    }
                }

                // create key
                string keyToEncrypt = string.Format("{0}:{1}", queueName, hash);
                string encryptedKey = EncryptUtils.EncryptRJ128(outerPushServerSecret, outerPushServerIV, keyToEncrypt);

                // create URL
                Random rand = new Random();
                string tokenPart = string.Format("{0}:{1}:{2}:{3}", outerPushServerSecret, ip, hash, rand.Next());
                string encryptedtokenPart = EncryptUtils.EncryptRJ128(outerPushServerSecret, outerPushServerIV, tokenPart);
                string token = HttpUtility.UrlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(groupId + ":" + encryptedtokenPart)));
                string url = string.Format(@"http://{0}/?p={1}&x={2}", outerPushDomainName, groupId, token);

                log.DebugFormat("GetPushWebParams: Create URL and key for queue: {0}. URL: {1}, KEY: {2}", queueName, url, encryptedKey);
                response.Url = url;
                response.Key = encryptedKey;
                response.NotificationId = announcementId;
            }
            return response;
        }

        public static RegistryResponse RegisterPushSystemParameters(int groupId, string hash, string ip)
        {
            RegistryResponse response = new RegistryResponse();
            response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            List<int> announcementIds = new List<int>();

            // get all announcements
            var announcements = NotificationCache.Instance().GetAnnouncements(groupId);
            if (announcements == null)
            {
                log.Error("GetPushWebParams: announcements were not found.");
                response.Status = new Status((int)eResponseStatus.AnnouncementNotFound, "announcements were not found");
                return response;
            }

            // get logged-in announcement
            var loggedInAnnouncement = announcements.Where(x => x.RecipientsType == eAnnouncementRecipientsType.LoggedIn).FirstOrDefault();
            int loggedAnnouncementId = 0;
            if (loggedInAnnouncement != null)
                loggedAnnouncementId = loggedInAnnouncement.ID;
            else
                log.Error("GetPushWebParams: logged-in announcement wasn't found.");

            // build queue name
            string queueName = string.Format(ANNOUNCEMENT_QUEUE_NAME_FORMAT, groupId, loggedAnnouncementId);

            // get relevant announcement
            var announcement = announcements.Where(x => x.ID == loggedAnnouncementId).FirstOrDefault();
            if (announcement == null)
            {
                log.ErrorFormat("GetPushWebParams: announcement not found. id: {0}.", loggedAnnouncementId);
                response.Status = new Status((int)eResponseStatus.ItemNotFound, eResponseStatus.ItemNotFound.ToString());
            }
            else
            {
                // update web push queue name on DB (if necessary)
                if (string.IsNullOrEmpty(announcement.QueueName))
                {
                    if (!DAL.NotificationDal.UpdateAnnouncement(groupId, loggedAnnouncementId, announcement.AutomaticIssueFollowNotification, queueName: queueName))
                    {
                        log.ErrorFormat("Error while trying to update announcement with web push queue name. GID: {0}, announcing ID: {1}, queue name: {2}",
                            groupId,
                            announcement.ID,
                            queueName);
                    }
                    else
                    {
                        log.DebugFormat("Successfully update announcement with web push queue name. GID: {0}, announcing ID: {1}, queue name: {2}",
                            groupId,
                            announcement.ID,
                            queueName);

                        // remove announcements from cache
                        NotificationCache.Instance().RemoveAnnouncementsFromCache(groupId);
                    }
                }

                // create key
                string keyToEncrypt = string.Format("{0}:{1}", queueName, hash);
                string encryptedKey = EncryptUtils.EncryptRJ128(outerPushServerSecret, outerPushServerIV, keyToEncrypt);

                // create URL
                Random rand = new Random();
                string tokenPart = string.Format("{0}:{1}:{2}:{3}", outerPushServerSecret, ip, hash, rand.Next());
                string encryptedtokenPart = EncryptUtils.EncryptRJ128(outerPushServerSecret, outerPushServerIV, tokenPart);
                string token = HttpUtility.UrlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(groupId + ":" + encryptedtokenPart)));
                string url = string.Format(@"http://{0}/?p={1}&x={2}", outerPushDomainName, groupId, token);

                log.DebugFormat("GetPushWebParams: Create URL and key for queue: {0}. URL: {1}, KEY: {2}", queueName, url, encryptedKey);
                response.Url = url;
                response.Key = encryptedKey;
                response.NotificationId = loggedAnnouncementId;
            }
            return response;
        }
    }
}
