using ApiLogic.Notification;
using APILogic.AmazonSnsAdapter;
using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.QueueObjects;
using ApiObjects.Response;
using Phx.Lib.Appconfig;
using Core.Catalog;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Core.Notification.Adapters;
using DAL;
using Phx.Lib.Log;
using Newtonsoft.Json;
using QueueWrapper;
using QueueWrapper.Queues.QueueObjects;
using ScheduledTasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using ApiObjects.Catalog;
using TVinciShared;
using ApiObjects.Base;
using CachingProvider.LayeredCache;
using System.Threading;
using ApiLogic.Modules.Services;
using ApiLogic.Notification.Managers;
using ApiObjects.EventBus;
using EventBus.Abstraction;
using EventBus.Kafka;
using Phoenix.Generated.Api.Events.Logical.announcementMessage;

namespace Core.Notification
{
    public interface IAnnouncementManager
    {
        GetAllMessageAnnouncementsResponse Get_AllMessageAnnouncements(int groupId, int pageSize, int pageIndex, MessageAnnouncementFilter filter, bool isMessageAnnouncements);
        List<InboxMessage> GetUserFollowedSeries(int groupId, int userId);
    }

    public class AnnouncementManager: IAnnouncementManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string ROUTING_KEY_NOTIFICATION_CLEANUP = "PROCESS_NOTIFIACTION_CLEANUP";
        private const int MAX_MSG_LENGTH = 250 * 1024;
        private const int MIN_TIME_FOR_START_TIME_SECONDS = 30;
        private static string CatalogSignString = Guid.NewGuid().ToString();
        private static string CatalogSignatureKey = ApplicationConfiguration.Current.CatalogSignatureKey.Value;
        private const string ANNOUNCEMENT_NOT_FOUND = "Announcement Not Found";
        private const string ANNOUNCEMENT_QUEUE_NAME_FORMAT = @"Announcement_{0}_{1}"; // Announcement_{GID}_{AnnID}
        private static string outerPushDomainName = ApplicationConfiguration.Current.AnnouncementManagerConfiguration.PushDomainName.Value;
        private static string outerPushServerSecret = ApplicationConfiguration.Current.AnnouncementManagerConfiguration.PushServerKey.Value;
        private static string outerPushServerIV = ApplicationConfiguration.Current.AnnouncementManagerConfiguration.PushServerIV.Value;

        public const string ROUTING_KEY_PROCESS_MESSAGE_ANNOUNCEMENTS = "PROCESS_MESSAGE_ANNOUNCEMENTS";
        public const double NOTIFICATION_CLEANUP_INTERVAL_SEC = 86400; // 24 hours
        public const int PUSH_MESSAGE_EXPIRATION_MILLI_SEC = 3000;
        public const string EPG_DATETIME_FORMAT = "dd/MM/yyyy HH:mm:ss";

        private static readonly Lazy<AnnouncementManager> lazy = new Lazy<AnnouncementManager>(() =>
            new AnnouncementManager(LayeredCache.Instance, NotificationDal.Instance, NotificationSettings.Instance, NotificationCache.Instance()),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly ILayeredCache _layeredCache;
        private readonly INotificationDal _notificationRepository;
        private readonly INotificationSettings _notificationSettings;
        private readonly INotificationCache _notificationCache;
        public static AnnouncementManager Instance { get { return lazy.Value; } }

        public AnnouncementManager(ILayeredCache layeredCache, INotificationDal notificationRepository, INotificationSettings notificationSettings, INotificationCache notificationCache)
        {
            _layeredCache = layeredCache;
            _notificationRepository = notificationRepository;
            _notificationSettings = notificationSettings;
            _notificationCache = notificationCache;
        }

        public static AddMessageAnnouncementResponse AddMessageAnnouncement(int groupId, MessageAnnouncement announcement, bool enforceMsgAllowedTime = false, bool validateMsgStartTime = true)
        {
            AddMessageAnnouncementResponse response = new AddMessageAnnouncementResponse();
            response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            if (announcement.StartTime == 0)
            {
                //BEO-9216
                announcement.StartTime = DateUtils.GetUtcUnixTimestampNow();
                announcement.Timezone = TimeZoneInfo.Utc.Id;
            }

            if (!ConvertStartTimeToUtc(ref announcement))
            {
                response.Status = new Status((int)eResponseStatus.AnnouncementInvalidTimezone, "Invalid timezone");
                return response;
            }

            // validation of start time is relevant only for system announcements
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
            DateTime announcementStartTime = DateUtils.UtcUnixTimestampSecondsToDateTime(announcement.StartTime);

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

                    announcement.StartTime = DateUtils.DateTimeToUtcUnixTimestampSeconds(announcementStartTime);

                    log.DebugFormat("Message start time was not in allowed interval, updated to {0}", newStartTime);
                }
            }
        }

        public MessageAnnouncementResponse UpdateMessageAnnouncement(int groupId, int announcementId, MessageAnnouncement announcement, bool enforceMsgAllowedTime = false, bool validateMsgStartTime = true)
        {
            var response = new MessageAnnouncementResponse
            {
                Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString())
            };

            if (validateMsgStartTime)
                response.Status = ValidateAnnouncement(groupId, announcement);

            // convert start time
            if (ConvertStartTimeToUtc(ref announcement) == false)
                response.Status = new Status((int)eResponseStatus.AnnouncementInvalidTimezone, "Invalid time zone");

            if (response.Status.Code != (int)eResponseStatus.OK)
                return response;

            var messageAnnouncementResponse = GetMessageAnnouncement(groupId, announcementId);
            if (!messageAnnouncementResponse.HasObject())
            {
                response.Status = messageAnnouncementResponse.Status;
                return response;
            }

            if (validateMsgStartTime)
            {
                if (messageAnnouncementResponse.Object.StartTime < DateUtils.GetUtcUnixTimestampNow())
                {
                    log.Error($"Announcement start time passed Id: {announcementId} start time: {messageAnnouncementResponse.Object.StartTime}");
                    response.Status = new Status((int)eResponseStatus.AnnouncementUpdateNotAllowed, "Announcement Update Not Allowed");
                    return response;
                }
            }

            // change time to allowed start time if needed
            if (enforceMsgAllowedTime)
            {
                EnforceAllowedStartTime(groupId, announcement);
            }

            DateTime announcementStartTime = DateUtils.UtcUnixTimestampSecondsToDateTime(announcement.StartTime);

            announcement = _notificationRepository.Update_MessageAnnouncement(announcementId, groupId, (int)announcement.Recipients, announcement.Name, announcement.Message, announcement.Enabled, announcementStartTime, announcement.Timezone, 0, null,
                announcement.ImageUrl, announcement.IncludeMail, announcement.MailTemplate, announcement.MailSubject, announcement.IncludeIot, announcement.IncludeSms, announcement.IncludeUserInbox);

            if(announcement.IncludeUserInbox)
                SetSystemMessageAnnouncementsInvalidation(groupId);

            // add a new message to queue when new time updated
            if (announcement.StartTime != messageAnnouncementResponse.Object.StartTime)
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

        public Status UpdateMessageSystemAnnouncementStatus(int groupId, long id, bool status)
        {
            // validate system announcements are enabled
            if (!_notificationSettings.IsPartnerSystemAnnouncementEnabled(groupId))
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

            _notificationRepository.Update_MessageAnnouncementStatus(ODBCWrapper.Utils.GetIntSafeVal(dr, "id"), groupId, status);
            return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
        }

        public Status DeleteMessageAnnouncement(int groupId, long id)
        {
            // validate system announcements are enabled
            var messageAnnouncementResponse = GetMessageAnnouncement(groupId, id);
            if (!messageAnnouncementResponse.HasObject())
            {
                return messageAnnouncementResponse.Status;
            }

            _notificationRepository.Delete_MessageAnnouncement(id, groupId);

            return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
        }

        private bool HandleRecipientOtherTvSeries(int groupId, int messageId, long startTime, int announcementId, ref DataRow messageAnnouncementDataRow, ref string url, ref string ImageUrl, ref string sound,
                                                         ref string category, out string annExternalId, out string singleQueueName, out bool failRes, out List<KeyValuePair<string, string>> mergeVars, out string mailExternalId)
        {
            failRes = false;
            annExternalId = string.Empty;
            singleQueueName = string.Empty;
            mergeVars = new List<KeyValuePair<string, string>>();
            mailExternalId = string.Empty;

            // check if announcement is for series, if not - return true to do nothing. if yes, check no msg was sent for series in the last 24H.
            // get topic push external id's of guests and logged in users
            List<DbAnnouncement> announcements = null;
            DbAnnouncement announcement = null;
            _notificationCache.TryGetAnnouncements(groupId, ref announcements);
            if (announcements != null)
                announcement = announcements.FirstOrDefault(x => x.ID == announcementId);

            if (announcement == null)
            {
                log.ErrorFormat("announcement wasn't found. GID: {0}, announcementId: {1}", groupId, announcementId);
                return false;
            }

            annExternalId = announcement.ExternalId;
            singleQueueName = announcement.QueueName;
            mailExternalId = announcement.MailExternalId;

            # region get asset details from catalog

            // for tv series msg ref is asset id of the asset msg is for.
            int assetId = ODBCWrapper.Utils.GetIntSafeVal(messageAnnouncementDataRow, "message_reference");
            if (assetId == 0)
            {
                log.DebugFormat("HandleRecipientTypeOther: asset not found for message announcement with recipients 'other': group: {0} Id: {1}", groupId, messageId);
                return false;
            }

            var episodeMedia = FollowManager.GetMediaObj(groupId, assetId);
            if (episodeMedia == null)
            {
                return false;
            }

            CatalogGroupCache cache = null;
            AssetStruct episodeAssetStruct = null;
            var episodeAssociationTag = FollowManager.GetEpisodeAssociationTag(groupId, ref cache, ref episodeAssetStruct, episodeMedia.m_oMediaType.m_nTypeID).ToLower().Trim();
            if (announcement.FollowPhrase == null ||
                !announcement.FollowPhrase.ToLower().Trim().StartsWith(episodeAssociationTag))
            {
                return true;
            }

            var topicType = MetaType.Tag;
            if (cache != null && episodeAssetStruct != null)
            {
                topicType = cache.TopicsMapById[episodeAssetStruct.ConnectedParentMetaId.Value].Type;
            }

            var seriesNames = FollowManager.GetSeriesNames(topicType, episodeAssociationTag, episodeMedia.m_lTags, episodeMedia.m_lMetas);
            if (seriesNames.Count == 0)
            {
                log.DebugFormat("HandleRecipientTypeOther: couldn't get series from catalog: group {0}, media: {1}", groupId, assetId);
                return false;
            }

            #endregion

            long utcNow = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            // get message announcements of announcement.
            var drs = _notificationRepository.Get_MessageAnnouncementsByAnnouncementId(announcementId, utcNow, startTime);
            if (drs.failRes)
            {
                return false;
            }

            log.DebugFormat("HandleRecipientOtherTvSeries: about to send message announcement for: group {0}, asset: {1}, id: {2}", groupId, assetId, messageId);

            // get msg template and build msg with it
            var msgTemplateResponse = FollowManager.GetMessageTemplate(groupId, MessageTemplateType.Series);
            if (msgTemplateResponse != null &&
                msgTemplateResponse.Status != null &&
                msgTemplateResponse.Status.Code == (int)eResponseStatus.OK &&
                msgTemplateResponse.MessageTemplate != null)
            {
                var imageUrl = Utils.GetMediaImageUrlByRatio(episodeMedia.m_lPicture, msgTemplateResponse.MessageTemplate.RatioId);
                var formatedCatalogStartDate = episodeMedia.m_dCatalogStartDate.ToString(msgTemplateResponse.MessageTemplate.DateFormat);
                var formatedStartDate = episodeMedia.m_dStartDate.ToString(msgTemplateResponse.MessageTemplate.DateFormat);
                var seriesName = seriesNames[0];
                category = msgTemplateResponse.MessageTemplate.Action;
                sound = msgTemplateResponse.MessageTemplate.Sound;

                url = msgTemplateResponse.MessageTemplate.URL.Replace("{" + eFollowSeriesPlaceHolders.CatalaogStartDate + "}", formatedCatalogStartDate).
                                                              Replace("{" + eFollowSeriesPlaceHolders.MediaId + "}", assetId.ToString()).
                                                              Replace("{" + eFollowSeriesPlaceHolders.MediaName + "}", episodeMedia.m_sName).
                                                              Replace("{" + eFollowSeriesPlaceHolders.SeriesName + "}", seriesName).
                                                              Replace("{" + eFollowSeriesPlaceHolders.StartDate + "}", formatedStartDate);

                string message = ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "message");
                if (!string.IsNullOrEmpty(message))
                {
                    messageAnnouncementDataRow["message"] = message.Replace("{" + eFollowSeriesPlaceHolders.CatalaogStartDate + "}", formatedCatalogStartDate).
                                                                    Replace("{" + eFollowSeriesPlaceHolders.MediaId + "}", assetId.ToString()).
                                                                    Replace("{" + eFollowSeriesPlaceHolders.MediaName + "}", episodeMedia.m_sName).
                                                                    Replace("{" + eFollowSeriesPlaceHolders.SeriesName + "}", seriesName).
                                                                    Replace("{" + eFollowSeriesPlaceHolders.StartDate + "}", formatedStartDate);
                }

                mergeVars = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(eFollowSeriesPlaceHolders.CatalaogStartDate.ToString(), formatedCatalogStartDate),
                    new KeyValuePair<string, string>(eFollowSeriesPlaceHolders.MediaId.ToString(), assetId.ToString()),
                    new KeyValuePair<string, string>(eFollowSeriesPlaceHolders.MediaName.ToString(), episodeMedia.m_sName),
                    new KeyValuePair<string, string>(eFollowSeriesPlaceHolders.SeriesName.ToString(), seriesName),
                    new KeyValuePair<string, string>(eFollowSeriesPlaceHolders.StartDate.ToString(), formatedStartDate),
                    new KeyValuePair<string, string>(eFollowSeriesPlaceHolders.Image.ToString(), imageUrl),
                    new KeyValuePair<string, string>(eFollowSeriesPlaceHolders.ReferenceId.ToString(), announcement.FollowReference),
                };
            }

            return true;
        }

        public GetAllMessageAnnouncementsResponse Get_AllMessageAnnouncements(int groupId, int pageSize, int pageIndex, MessageAnnouncementFilter filter, bool isMessageAnnouncements)
        {
            GetAllMessageAnnouncementsResponse ret = new GetAllMessageAnnouncementsResponse
            {
                Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString())
            };

            // validate system announcements are enabled
            if (!_notificationSettings.IsPartnerSystemAnnouncementEnabled(groupId))
            {
                log.Error($"Get_AllMessageAnnouncements  - partner system announcements are disabled. groupID = {groupId}");
                ret.Status = new Status((int)eResponseStatus.FeatureDisabled, "Feature Disabled");
                return ret;
            }

            ret.messageAnnouncements = new List<MessageAnnouncement>();

            if (filter != null)
            {
                if (filter.MessageAnnouncementIds != null && filter.MessageAnnouncementIds.Count > 0)
                {
                    if (pageIndex != 0)
                    {
                        ret.Status.Set(eResponseStatus.InvalidValue, "Page index value must be 1.");
                        return ret;
                    }

                    if (pageSize < filter.MessageAnnouncementIds.Count)
                    {
                        ret.Status.Set(eResponseStatus.InvalidValue, "Page size must to be greater or equal to the size of MessageAnnouncement.Ids");
                        return ret;
                    }

                    foreach (var id in filter.MessageAnnouncementIds)
                    {
                        var msgResponse = GetMessageAnnouncement(groupId, id);
                        if (msgResponse.HasObject())
                        {
                            ret.messageAnnouncements.Add(msgResponse.Object);
                        }
                    }

                    ret.totalCount = ret.messageAnnouncements.Count;
                    return ret;
                }
            }

            if (isMessageAnnouncements)
            { 
                ret.messageAnnouncements = NotificationDal.Get_MessageAllAnnouncements(groupId, pageSize, pageIndex);
                ret.totalCount = NotificationDal.Get_MessageAllAnnouncementsCount(groupId);
            }
            else
            {
                var ctx = new ContextData(groupId);
                ret = ListSystemMessageAnnouncements(ctx);
                ret.totalCount = ret.messageAnnouncements.Count;    
            }
            
            ret.Status = new Status(eResponseStatus.OK);
            return ret;
        }

        private GetAllMessageAnnouncementsResponse ListSystemMessageAnnouncements(ContextData contextData)
        {
            GetAllMessageAnnouncementsResponse inboxMessages = null;

            var key = LayeredCacheKeys.GetSystemMessageAnnouncementsKey(contextData.GroupId);
            var cacheResult = _layeredCache.Get(key,
                                                ref inboxMessages,
                                                GetAllSystemMessageAnnouncements,
                                                new Dictionary<string, object>() {
                                                        { "groupId", contextData.GroupId },
                                                },
                                                contextData.GroupId,
                                                LayeredCacheConfigNames.GET_ALL_MESSAGE_ANNOUNCEMENTS,
                                                new List<string>() {
                                                        LayeredCacheKeys.GetSystemMessageAnnouncementsInvalidationKey(contextData.GroupId)
                                                });

            return inboxMessages ?? new GetAllMessageAnnouncementsResponse();
        }

        private Tuple<GetAllMessageAnnouncementsResponse, bool> GetAllSystemMessageAnnouncements(Dictionary<string, object> arg)
        {
            var response = new GetAllMessageAnnouncementsResponse();

            var groupId = (int)arg["groupId"];
            var limit = _notificationSettings.GetInboxMessageTTLDays(groupId);
            var (Announcements, TotalCount) = _notificationRepository.Get_AllSystemAnnouncements(groupId, limit);
            response.messageAnnouncements = Announcements;
            response.totalCount = TotalCount;

            return new Tuple<GetAllMessageAnnouncementsResponse, bool>(response, response != null);
        }

        public List<InboxMessage> GetUserFollowedSeries(int groupId, int userId)
        {
            if (!Utils.GetUserNotificationData(groupId, userId, out var userNotificationData).IsOkStatusCode())
                return new List<InboxMessage>();

            var response = new List<InboxMessage>();
            var ttl = _notificationSettings.GetInboxMessageTTLDays(groupId);

            //Validate Announcements
            var groupAnnouncements = _notificationRepository.GetAnnouncements(groupId);
            if (groupAnnouncements == null || groupAnnouncements.IsEmpty())
                return response;

            var groupAnnouncementsIds = groupAnnouncements.Select(x => (long)x.ID).ToList();

            var userAnnouncements = userNotificationData.Announcements.Select(x => x.AnnouncementId).ToList();
            var innerJoin = groupAnnouncementsIds.Intersect(userAnnouncements).ToList();

            var userNotificationAnnouncements = userNotificationData.Announcements
                .Where(x => innerJoin.Contains(x.AnnouncementId));

            foreach (var followAnnouncement in userNotificationAnnouncements)
            {
                var _messageAnnouncements = GetFollowMessageAnnouncementCache(groupId, followAnnouncement.AnnouncementId);
                if (!_messageAnnouncements.IsOkStatusCode())
                {
                    log.Error($"Failed to GetFollowMessageAnnouncementCache with AnnouncementId: {followAnnouncement.AnnouncementId}");
                    continue;
                }

                foreach (var ma in _messageAnnouncements.Objects)
                {
                    response.Add(
                    new InboxMessage
                    {
                        Id = ma.MessageAnnouncementId.ToString(),
                        Category = eMessageCategory.Followed,
                        Message = ma.Message,
                        ExpirationDate = DateUtils.UtcUnixTimestampSecondsToDateTime(ma.StartTime)
                                                    .AddDays(ttl),
                        ImageUrl = ma.ImageUrl,
                        UserId = userId,
                        CreatedAtSec = ma.StartTime
                    });
                }
            }

            return response;
        }

        public void SetSystemMessageAnnouncementsInvalidation(int partnerId)
        {
            var invalidationKey = LayeredCacheKeys.GetSystemMessageAnnouncementsInvalidationKey(partnerId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                log.Error($"Failed to set invalidation key for GetSystemMessageAnnouncementsInvalidationKey with invalidationKey: {invalidationKey}");
            }
        }

        public GenericListResponse<MessageAnnouncement> GetFollowMessageAnnouncementCache(int groupId, long announcementId)
        {
            List<MessageAnnouncement> response = null;

            var key = LayeredCacheKeys.GetFollowMessageAnnouncementKey(groupId, announcementId);
            var cacheResult = _layeredCache.Get(key,
                                                ref response,
                                                GetMessageFollowAnnouncementDB,
                                                new Dictionary<string, object>() {
                                                        { "groupId", groupId },
                                                        { "id", announcementId },
                                                },
                                                groupId,
                                                LayeredCacheConfigNames.GET_MESSAGE_FOLLOW_ANNOUNCEMENT_DB,
                                                new List<string>() {
                                                        LayeredCacheKeys.GetFollowMessageAnnouncementInvalidationKey(groupId, announcementId)
                                                });

            var _status = new Status(response != null ? eResponseStatus.OK : eResponseStatus.AnnouncementNotFound);
            return new GenericListResponse<MessageAnnouncement>(_status, response) { TotalItems = response.Count };
        }

        private Tuple<List<MessageAnnouncement>, bool> GetMessageFollowAnnouncementDB(Dictionary<string, object> arg)
        {
            int.TryParse(arg["groupId"].ToString(), out var groupId);
            long.TryParse(arg["id"].ToString(), out var announcementId);
            var response = GetMessageAnnouncementsByAnnouncementId(groupId, announcementId);

            return new Tuple<List<MessageAnnouncement>, bool>(response?.Objects, response.IsOkStatusCode());
        }

        public void InvalidateFollowMessageAnnouncement(int groupId, long announcementId)
        {
            var key = LayeredCacheKeys.GetFollowMessageAnnouncementInvalidationKey(groupId, announcementId);
            if (!LayeredCache.Instance.SetInvalidationKey(key))
                log.Warn($"Failed to invalidate InvalidateFollowMessageAnnouncement with key: {key}");
        }

        private static MessageAnnouncement AddMessageAnnouncementToDB(int groupId, MessageAnnouncement announcement)
        {
            try
            {
                DateTime announcementStartTime = DateUtils.UtcUnixTimestampSecondsToDateTime(announcement.StartTime);
               var newRow = DAL.NotificationDal.Insert_MessageAnnouncement(groupId, (int)announcement.Recipients, announcement.Name, announcement.Message,
                    announcement.Enabled, announcementStartTime, announcement.Timezone, 0, announcement.MessageReference, null,
                    announcement.ImageUrl, announcement.IncludeMail, announcement.MailTemplate, announcement.MailSubject, announcement.IncludeSms, announcement.IncludeIot, announcement.AnnouncementId, announcement.IncludeUserInbox);
                return newRow;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("AddMessageAnnouncement Exception = {0}", ex.Message));
                return null;
            }
        }

        private static bool AddMessageAnnouncementToQueue(int groupId, MessageAnnouncement announcement)
        {
            bool res = true;

            MessageAnnouncementQueue que = new MessageAnnouncementQueue();
            MessageAnnouncementData messageAnnouncementData = new MessageAnnouncementData(groupId, announcement.StartTime, announcement.MessageAnnouncementId)
            {
                ETA = DateUtils.UtcUnixTimestampSecondsToDateTime(announcement.StartTime)
            };

            res = que.Enqueue(messageAnnouncementData, ROUTING_KEY_PROCESS_MESSAGE_ANNOUNCEMENTS);

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
                DateTime announcementStartTime = DateUtils.UtcUnixTimestampSecondsToDateTime(announcement.StartTime);
                DateTime convertedTime = ODBCWrapper.Utils.ConvertToUtc(announcementStartTime, announcement.Timezone);
                announcement.StartTime = DateUtils.DateTimeToUtcUnixTimestampSeconds(convertedTime);
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

            if (announcement.IncludeIot && !NotificationSettings.IsPartnerIotNotificationEnabled(groupId))
            {
                log.ErrorFormat("Invalid send attempt to IOT");
                return new Status((int)eResponseStatus.ActionIsNotAllowed, "Invalid send attempt to IOT");
            }

            return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
        }

        public Status CreateSystemAnnouncement(int groupId)
        {
            try
            {
                List<DbAnnouncement> dbAnnouncements = null;
                _notificationCache.TryGetAnnouncements(groupId, ref dbAnnouncements);
                string announcementName = string.Empty;
                string externalAnnouncementId = string.Empty;

                // validate system announcements are enabled
                if (!_notificationSettings.IsPartnerSystemAnnouncementEnabled(groupId))
                {
                    log.ErrorFormat("CreateSystemAnnouncement  - partner system announcements are disabled. groupID = {0}", groupId);
                    return new Status((int)eResponseStatus.FeatureDisabled, "Feature Disabled");
                }

                if (dbAnnouncements != null && dbAnnouncements.FirstOrDefault(x => x.RecipientsType == eAnnouncementRecipientsType.Guests) == null)
                {
                    // create guest topic
                    announcementName = "Guest";
                    externalAnnouncementId = NotificationAdapter.CreateAnnouncement(groupId, announcementName);
                    if (string.IsNullOrEmpty(externalAnnouncementId))
                    {
                        log.ErrorFormat("CreateSystemAnnouncement failed Create guest announcement groupID = {0}, announcementName = {1}", groupId, announcementName);
                    }

                    // insert ARN to DB
                    if (DAL.NotificationDal.Insert_Announcement(groupId, announcementName, externalAnnouncementId, (int)eMessageType.Push, (int)eAnnouncementRecipientsType.Guests, string.Empty) == 0)
                    {
                        log.ErrorFormat("CreateSystemAnnouncement failed insert guest announcement to DB groupID = {0}, announcementName = {1}", groupId, announcementName);
                        return new Status((int)eResponseStatus.Error, "fail insert guest announcement to DB");
                    }
                }

                if (dbAnnouncements != null && dbAnnouncements.FirstOrDefault(x => x.RecipientsType == eAnnouncementRecipientsType.LoggedIn) == null)
                {
                    // create logged-in topic
                    announcementName = "LoggedIn";
                    externalAnnouncementId = string.Empty;
                    externalAnnouncementId = NotificationAdapter.CreateAnnouncement(groupId, announcementName);
                    if (string.IsNullOrEmpty(externalAnnouncementId))
                    {
                        log.ErrorFormat("CreateSystemAnnouncement failed Create logged in announcement groupID = {0}, announcementName = {1}", groupId, announcementName);
                    }

                    // insert ARN to DB
                    if (DAL.NotificationDal.Insert_Announcement(groupId, announcementName, externalAnnouncementId, (int)eMessageType.Push, (int)eAnnouncementRecipientsType.LoggedIn, string.Empty) == 0)
                    {
                        log.ErrorFormat("CreateSystemAnnouncement failed insert logged in announcement to DB groupID = {0}, announcementName = {1}", groupId, announcementName);
                        return new Status((int)eResponseStatus.Error, "fail insert Logged in announcement to DB");
                    }
                }

                // BEO-6157: Try to send mail announcement only if partner has an adapter defined
                if (NotificationSettings.IsPartnerMailNotificationEnabled(groupId))
                {
                    if (dbAnnouncements != null && dbAnnouncements.FirstOrDefault(x => x.RecipientsType == eAnnouncementRecipientsType.Mail) == null)
                    {
                        announcementName = "Mail";
                        string mailExternalAnnouncementId = MailNotificationAdapterClient.CreateAnnouncement(groupId, announcementName);
                        if (string.IsNullOrEmpty(mailExternalAnnouncementId))
                        {
                            log.ErrorFormat("CreateSystemAnnouncement failed Create mail announcement groupID = {0}, announcementName = {1}", groupId, announcementName);
                            return new Status((int)eResponseStatus.FailCreateAnnouncement, "fail create mail announcement");
                        }

                        if (DAL.NotificationDal.Insert_Announcement(groupId, announcementName, string.Empty, (int)eMessageType.Mail, (int)eAnnouncementRecipientsType.Mail, mailExternalAnnouncementId) == 0)
                        {
                            log.ErrorFormat("CreateSystemAnnouncement failed insert mail announcement to DB groupID = {0}, announcementName = {1}", groupId, announcementName);
                            return new Status((int)eResponseStatus.Error, "fail insert mail announcement to DB");
                        }
                    }
                }

                // BEO-6157: Try to send SMS announcement only if partner has SMS notification enabled
                if (NotificationSettings.IsPartnerSmsNotificationEnabled(groupId))
                {
                    if (dbAnnouncements != null && dbAnnouncements.FirstOrDefault(x => x.RecipientsType == eAnnouncementRecipientsType.Sms) == null)
                    {
                        announcementName = "Sms";
                        string smsExternalAnnouncementId = NotificationAdapter.CreateAnnouncement(groupId, announcementName);
                        if (string.IsNullOrEmpty(smsExternalAnnouncementId))
                        {
                            log.ErrorFormat("CreateSystemAnnouncement failed Create SMS announcement groupID = {0}, announcementName = {1}", groupId, announcementName);
                            return new Status((int)eResponseStatus.FailCreateAnnouncement, "fail create SMS announcement");
                        }

                        if (DAL.NotificationDal.Insert_Announcement(groupId, announcementName, smsExternalAnnouncementId, (int)eMessageType.Sms,
                            (int)eAnnouncementRecipientsType.Sms, string.Empty) == 0)
                        {
                            log.ErrorFormat("CreateSystemAnnouncement failed insert SMS announcement to DB groupID = {0}, announcementName = {1}", groupId, announcementName);
                            return new Status((int)eResponseStatus.Error, "fail insert SMS announcement to DB");
                        }
                    }
                }

                NotificationCache.Instance().RemoveAnnouncementsFromCache(groupId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CreateSystemAnnouncement failed groupId = {0}, ex = {1}", groupId, ex);
                return new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
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
            if (ODBCWrapper.Utils.GetDateSafeVal(messageAnnouncementDataRow, "start_time") != DateUtils.UtcUnixTimestampSecondsToDateTime(startTime))
            {
                DAL.NotificationDal.Update_MessageAnnouncementActiveStatus(groupId, messageId, 0);
                log.DebugFormat("Announcement start time is different than DB start time. DB start time: {0}, announcement start time: {1} group: {2} Id: {3}",
                    ODBCWrapper.Utils.GetDateSafeVal(messageAnnouncementDataRow, "start_time"),
                    DateUtils.UtcUnixTimestampSecondsToDateTime(startTime),
                    groupId,
                    messageId);
                return false;
            }

            // validate recipient type is legal
            if (!Enum.TryParse(ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "recipients"), out eAnnouncementRecipientsType recipients))
            {
                DAL.NotificationDal.Update_MessageAnnouncementActiveStatus(groupId, messageId, 0);
                log.ErrorFormat("invalid recipients type for announcement {0}", messageId);
                return false;
            }

            long currentTimeSec = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
            string singleTopicExternalId = string.Empty;
            string singleQueueName = string.Empty;
            List<string> topicExternalIds = new List<string>();
            List<string> queueNames = new List<string>();
            bool includeMail = ODBCWrapper.Utils.GetIntSafeVal(messageAnnouncementDataRow, "INCLUDE_EMAIL") == 1;
            bool includeSms = ODBCWrapper.Utils.GetIntSafeVal(messageAnnouncementDataRow, "INCLUDE_SMS") == 1;
            bool includeIot = ODBCWrapper.Utils.GetIntSafeVal(messageAnnouncementDataRow, "INCLUDE_IOT") == 1;
            string mailTemplate = ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "MAIL_TEMPLATE");
            string mailSubject = ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "MAIL_SUBJECT");
            bool includeUserInbox = ODBCWrapper.Utils.GetIntSafeVal(messageAnnouncementDataRow, "INCLUDE_USER_INBOX") == 1;

            // in case system announcement - the image URL is taken from the message announcement and not from template
            string imageUrl = ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "image_url");

            List<DbAnnouncement> announcements = null;
            NotificationCache.Instance().TryGetAnnouncements(groupId, ref announcements);

            switch (recipients)
            {
                case eAnnouncementRecipientsType.All:
                    {
                        if (NotificationSettings.IsPartnerPushEnabled(groupId))
                        {
                            // get topic push external id's of guests and logged in users
                            List<DbAnnouncement> announcementGuestAndLoggedIn = null;
                            if (announcements != null)
                                announcementGuestAndLoggedIn = announcements.Where(x => x.RecipientsType == eAnnouncementRecipientsType.LoggedIn || x.RecipientsType == eAnnouncementRecipientsType.Guests).ToList();

                            if (announcementGuestAndLoggedIn != null && announcementGuestAndLoggedIn.Count() > 0)
                            {
                                foreach (var announcement in announcementGuestAndLoggedIn)
                                    topicExternalIds.Add(announcement.ExternalId);
                            }
                        }

                        // send inbox messages
                        if (includeUserInbox && NotificationSettings.Instance.IsPartnerInboxEnabled(groupId))
                        {
                            Instance.SetSystemMessageAnnouncementsInvalidation(groupId);
                            log.Debug($"Invalidating cache due to: Setting system announcement inbox message. " +
                                $"groupId: {groupId}, messageId: {messageId}");
                        }

                        if (NotificationSettings.IsPartnerMailNotificationEnabled(groupId) && includeMail)
                        {
                            PublishMailSystemAnnouncement(groupId, messageAnnouncementDataRow, announcements, mailTemplate, mailSubject);
                        }

                        if (NotificationSettings.IsPartnerSmsNotificationEnabled(groupId) && includeSms)
                        {
                            PublishSmsSystemAnnouncement(groupId, messageAnnouncementDataRow, announcements,
                                ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "message"), url, sound, category, imageUrl);
                        }

                        // add the Q name to list to be sent to later
                        DbAnnouncement loggedInAnnouncement = null;
                        if (announcements != null)
                            loggedInAnnouncement = announcements.FirstOrDefault(x => x.RecipientsType == eAnnouncementRecipientsType.LoggedIn);

                        if (loggedInAnnouncement != null && !string.IsNullOrEmpty(loggedInAnnouncement.QueueName))
                            queueNames.Add(loggedInAnnouncement.QueueName);

                        break;
                    }

                case eAnnouncementRecipientsType.Guests:
                    {
                        if (NotificationSettings.IsPartnerPushEnabled(groupId))
                        {
                            DbAnnouncement announcementGuest = null;
                            if (announcements != null)
                                announcementGuest = announcements.FirstOrDefault(x => x.RecipientsType == eAnnouncementRecipientsType.Guests);

                            if (announcementGuest != null)
                                topicExternalIds.Add(announcementGuest.ExternalId);
                            else
                            {
                                DAL.NotificationDal.Update_MessageAnnouncementActiveStatus(groupId, messageId, 0);
                                log.ErrorFormat("external announcement id is empty for announcement {0}", messageId);
                                return false;
                            }
                        }

                        if (NotificationSettings.IsPartnerMailNotificationEnabled(groupId) && includeMail)
                        {
                            PublishMailSystemAnnouncement(groupId, messageAnnouncementDataRow, announcements, mailTemplate, mailSubject);
                        }

                        if (NotificationSettings.IsPartnerSmsNotificationEnabled(groupId) && includeSms)
                        {
                            PublishSmsSystemAnnouncement(groupId, messageAnnouncementDataRow, announcements,
                                ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "message"), url, sound, category, imageUrl);
                        }

                        break;
                    }

                case eAnnouncementRecipientsType.LoggedIn:
                    {
                        if (NotificationSettings.IsPartnerPushEnabled(groupId))
                        {
                            // get topic push external id's of logged-in users
                            DbAnnouncement announcementLoggedIn = null;
                            if (announcements != null)
                                announcementLoggedIn = announcements.FirstOrDefault(x => x.RecipientsType == eAnnouncementRecipientsType.LoggedIn);

                            if (announcementLoggedIn != null)
                                topicExternalIds.Add(announcementLoggedIn.ExternalId);
                            else
                            {
                                DAL.NotificationDal.Update_MessageAnnouncementActiveStatus(groupId, messageId, 0);
                                log.ErrorFormat("external announcement id is empty for announcement {0}", messageId);
                                return false;
                            }
                        }

                        // send inbox messages
                        if (includeUserInbox && NotificationSettings.Instance.IsPartnerInboxEnabled(groupId))
                        {
                            Instance.SetSystemMessageAnnouncementsInvalidation(groupId);
                            log.Debug($"Invalidating cache due to: Setting system announcement inbox message. " +
                                $"groupId: {groupId}, messageId: {messageId}");
                        }

                        if (NotificationSettings.IsPartnerMailNotificationEnabled(groupId) && includeMail)
                        {
                            PublishMailSystemAnnouncement(groupId, messageAnnouncementDataRow, announcements, mailTemplate, mailSubject);
                        }

                        if (NotificationSettings.IsPartnerSmsNotificationEnabled(groupId) && includeSms)
                        {
                            PublishSmsSystemAnnouncement(groupId, messageAnnouncementDataRow, announcements,
                                ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "message"), url, sound, category, imageUrl);
                        }

                        // add the Q name to list to be sent to later
                        DbAnnouncement loggedInAnn = null;
                        if (announcements != null)
                            loggedInAnn = announcements.FirstOrDefault(x => x.RecipientsType == eAnnouncementRecipientsType.LoggedIn);

                        if (loggedInAnn != null && !string.IsNullOrEmpty(loggedInAnn.QueueName))
                            queueNames.Add(loggedInAnn.QueueName);

                        break;
                    }

                case eAnnouncementRecipientsType.Other:
                    {
                        bool res;

                        // get announcement id (of series)
                        int announcementId = ODBCWrapper.Utils.GetIntSafeVal(messageAnnouncementDataRow, "announcement_id");
                        if (announcementId == 0)
                        {
                            log.DebugFormat("Announcement id invalid for message announcement with recipients 'other': group: {0} message ID: {1}", groupId, ODBCWrapper.Utils.GetIntSafeVal(messageAnnouncementDataRow, "ID"));
                            return false;
                        }

                        List<KeyValuePair<string, string>> mergeVars = null;
                        string mailExternalId = string.Empty;
                        if (!Instance.HandleRecipientOtherTvSeries(groupId, messageId, startTime, announcementId, ref messageAnnouncementDataRow, ref url, ref imageUrl, ref sound, ref category, out singleTopicExternalId,
                            out singleQueueName, out res, out mergeVars, out mailExternalId))
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
                        //BEO-11019
                        if (includeUserInbox && NotificationSettings.Instance.IsPartnerInboxEnabled(groupId))
                        {
                            log.Debug($"Skipping SetUserInboxMessageFromView, groupId: {groupId}, " +
                                $"startTime: {startTime}, messageId: {messageId}");
                        }

                        if (NotificationSettings.IsPartnerMailNotificationEnabled(groupId) && !string.IsNullOrEmpty(mailExternalId))
                        {
                            var msgTemplateResponse = FollowManager.GetMessageTemplate(groupId, MessageTemplateType.Series);
                            if (msgTemplateResponse != null &&
                                msgTemplateResponse.Status != null &&
                                msgTemplateResponse.Status.Code == (int)eResponseStatus.OK &&
                                msgTemplateResponse.MessageTemplate != null)
                            {
                                string subject = msgTemplateResponse.MessageTemplate.MailSubject;
                                string template = msgTemplateResponse.MessageTemplate.MailTemplate;
                                foreach (var mergeVar in mergeVars)
                                {
                                    subject = subject.Replace("{" + mergeVar.Key + "}", mergeVar.Value);
                                }

                                if (!MailNotificationAdapterClient.PublishToAnnouncement(groupId, mailExternalId, subject, mergeVars, template))
                                {
                                    log.ErrorFormat("failed to send follow announcement to mail adapter. annoucementId = {0}", announcementId);
                                }
                                else
                                {
                                    log.DebugFormat("Successfully sent follow announcement to mail. announcementId: {0}", announcementId);

                                    // update follow external result
                                    if (NotificationDal.AddMailExternalResult(groupId, announcementId, MailMessageType.Follow, string.Empty, true) == 0)
                                    {
                                        log.ErrorFormat("Failed to add mail external result for follow announcement. announcementId = {0}", announcementId);
                                    }
                                }
                            }
                        }

                        break;
                    }
            }

            string resultMsgIds = "";

            var conditionValue = announcements.Any(ann => ann.RecipientsType == eAnnouncementRecipientsType.All ||
                                                          ann.RecipientsType == eAnnouncementRecipientsType.LoggedIn);
            //send IoT system announcement message
            if (includeIot && NotificationSettings.IsPartnerIotNotificationEnabled(groupId)
                && conditionValue)
            {
                PublishIotSystemAnnouncement(groupId, ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "message"));
            }

            // send push messages
            if (NotificationSettings.IsPartnerPushEnabled(groupId) || NotificationSettings.IsPartnerSmsNotificationEnabled(groupId))
            {
                // send to Amazon
                if (topicExternalIds != null && topicExternalIds.Count > 0)
                {
                    foreach (string extAnnouncementId in topicExternalIds)
                    {
                        string resultMsgId = NotificationAdapter.PublishToAnnouncement(groupId, extAnnouncementId, string.Empty, new MessageData() { Alert = ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "message"), Url = url, Sound = sound, Category = category, ImageUrl = imageUrl });
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
                    MessageAnnouncementFullData data = new MessageAnnouncementFullData(groupId, ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "message"), url, sound, category, startTime, imageUrl);
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

        private static void PublishSmsSystemAnnouncement(int groupId, DataRow messageAnnouncementDataRow, List<DbAnnouncement> announcements,
            string alert, string url, string sound, string category, string imageUrl)
        {
            DbAnnouncement smsAnnouncement = null;

            if (announcements != null)
                smsAnnouncement = announcements.FirstOrDefault(x => x.RecipientsType == eAnnouncementRecipientsType.Sms);

            if (smsAnnouncement != null)
            {
                string resultMsgId = NotificationAdapter.PublishToAnnouncement(groupId, smsAnnouncement.ExternalId, string.Empty,
                    new MessageData()
                    {
                        Alert = ODBCWrapper.Utils.GetSafeStr(messageAnnouncementDataRow, "message"),
                        Url = url,
                        Sound = sound,
                        Category = category,
                        ImageUrl = imageUrl
                    });
                if (string.IsNullOrEmpty(resultMsgId))
                {
                    log.ErrorFormat("failed to send SMS system announcement to adapter. annoucementId = {0}", smsAnnouncement.ID);
                }
                else
                {
                    log.DebugFormat("Successfully sent SMS system announcement. announcementId: {0}", smsAnnouncement.ID);
                }
            }
        }

        private static void PublishIotSystemAnnouncement(int groupId, string message)
        {
            var shorterMessage = message.Substring(0, Math.Min(message.Length, 10));
            try
            {
                log.DebugFormat(
                    $"PublishIotSystemAnnouncement message {shorterMessage}");
                new IotManager(new IotAnnouncementMessageRequest {GroupId = groupId})
                    .PublishIotAnnouncementMessageKafkaEvent(groupId, message);
            }
            catch (Exception e)
            {
                log.Error($"Failed to send IoT system announcement. message = {shorterMessage}, exception = {e}");
            }
        }

        private static void PublishMailSystemAnnouncement(int groupId, DataRow messageAnnouncementDataRow, List<DbAnnouncement> announcements, string template, string subject)
        {
            DbAnnouncement mailAnnouncement = null;

            if (announcements != null)
                mailAnnouncement = announcements.FirstOrDefault(x => x.RecipientsType == eAnnouncementRecipientsType.Mail);

            if (mailAnnouncement != null)
            {
                if (!MailNotificationAdapterClient.PublishToAnnouncement(groupId, mailAnnouncement.MailExternalId, subject, null, template))
                {
                    log.ErrorFormat("failed to send system announcement to mail adapter. annoucementId = {0}", mailAnnouncement.ID);
                }
                else
                {
                    log.DebugFormat("Successfully sent system announcement to mail. announcementId: {0}", mailAnnouncement.ID);

                    // update system external result
                    if (NotificationDal.AddMailExternalResult(groupId, mailAnnouncement.ID, MailMessageType.SystemAnnouncement, string.Empty, true) == 0)
                    {
                        log.ErrorFormat("Failed to add mail external result for system announcement. announcementId = {0}", mailAnnouncement.ID);
                    }
                }
            }
        }

        public static Status DeleteAnnouncement(int groupId, long announcementId)
        {
            Status responseStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            string logData = string.Format("GID: {0}, announcementId: {1}", groupId, announcementId);

            // get announcement
            List<DbAnnouncement> announcements = null;
            DbAnnouncement announcement = null;
            NotificationCache.Instance().TryGetAnnouncements(groupId, ref announcements);

            if (announcements != null)
                announcement = announcements.FirstOrDefault(x => x.ID == announcementId);
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
                List<DbAnnouncement> announcements = null;
                NotificationCache.Instance().TryGetAnnouncements(partnerSettings.PartnerId, ref announcements);
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
                        TimeSpan.FromSeconds(TVinciShared.DateUtils.GetUtcUnixTimestampNow() - announcement.LastMessageSentDateSec).TotalDays > partnerSettings.TopicExpirationDurationDays)
                    {
                        Status deleteAnnouncementResp = DeleteAnnouncement(partnerSettings.PartnerId, announcement.ID);
                        if (deleteAnnouncementResp != null && deleteAnnouncementResp.Code != (int)eResponseStatus.OK)
                        {
                            log.ErrorFormat("Error while trying to delete old topic. GID: {0}, Announcement ID: {1}, Announcement updated at: {2}, topic expiration duration in days: {3}",
                                partnerSettings.PartnerId,
                                announcement.ID,
                                TVinciShared.DateUtils.UtcUnixTimestampSecondsToDateTime(announcement.LastMessageSentDateSec).ToString(),
                                partnerSettings.TopicExpirationDurationDays);
                        }
                        else
                        {
                            totalAnnouncementsDeleted++;
                            log.DebugFormat("successfully deleted old topic. GID: {0}, Announcement ID: {1}, Announcement updated at: {2}, topic expiration duration in days: {3}",
                                partnerSettings.PartnerId,
                                announcement.ID,
                                TVinciShared.DateUtils.UtcUnixTimestampSecondsToDateTime(announcement.LastMessageSentDateSec).ToString(),
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
                List<DbAnnouncement> announcements = null;
                DbAnnouncement announcement = null;
                NotificationCache.Instance().TryGetAnnouncements(groupId, ref announcements);

                if (announcements != null)
                    announcement = announcements.FirstOrDefault(x => x.ID == announcementId);
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
                List<DbAnnouncement> announcements = null;
                DbAnnouncement announcement = null;
                NotificationCache.Instance().TryGetAnnouncements(groupId, ref announcements);

                if (announcements != null)
                    announcement = announcements.FirstOrDefault(x => x.ID == announcementId);

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
                List<DbAnnouncement> announcements = null;
                List<DbAnnouncement> topicAnnouncements = null;
                NotificationCache.Instance().TryGetAnnouncements(groupId, ref announcements);

                if (announcements != null)
                    topicAnnouncements = announcements.Where(x => x.RecipientsType == eAnnouncementRecipientsType.Other).ToList();

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

                response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
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
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
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

            List<DbAnnouncement> announcements = null;
            DbAnnouncement announcement = null;
            NotificationCache.Instance().TryGetAnnouncements(groupId, ref announcements);

            if (announcements != null)
                announcement = announcements.FirstOrDefault(x => x.ID == announcementId);

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
            List<DbAnnouncement> announcements = null;
            NotificationCache.Instance().TryGetAnnouncements(groupId, ref announcements);
            if (announcements == null)
            {
                log.Error("GetPushWebParams: announcements were not found.");
                response.Status = new Status((int)eResponseStatus.AnnouncementNotFound, "announcements were not found");
                return response;
            }

            // get logged-in announcement
            var loggedInAnnouncement = announcements.FirstOrDefault(x => x.RecipientsType == eAnnouncementRecipientsType.LoggedIn);
            int loggedAnnouncementId = 0;
            if (loggedInAnnouncement != null)
                loggedAnnouncementId = loggedInAnnouncement.ID;
            else
                log.Error("GetPushWebParams: logged-in announcement wasn't found.");

            // build queue name
            string queueName = string.Format(ANNOUNCEMENT_QUEUE_NAME_FORMAT, groupId, loggedAnnouncementId);

            // get relevant announcement
            var announcement = announcements.FirstOrDefault(x => x.ID == loggedAnnouncementId);
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

        public static Status GetEpgProgram(int groupId, int assetId, out ProgramObj epgProgram)
        {
            Status status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
            EpgProgramResponse epgProgramResponse = null;
            epgProgram = null;
            EpgProgramDetailsRequest epgRequest = new EpgProgramDetailsRequest();

            try
            {
                // get EPG information
                epgRequest = new EpgProgramDetailsRequest()
                {
                    m_lProgramsIds = new List<int> { assetId },
                    m_nGroupID = groupId,
                    m_sSignature = NotificationUtils.GetSignature(CatalogSignString, CatalogSignatureKey),
                    m_sSignString = CatalogSignString
                };

                epgProgramResponse = epgRequest.GetProgramsByIDs(epgRequest);

                if (epgProgramResponse != null && epgProgramResponse.m_lObj != null && epgProgramResponse.m_lObj.Count > 0)
                {
                    epgProgram = epgProgramResponse.m_lObj[0] as ProgramObj;
                    if (epgProgram == null)
                    {
                        log.ErrorFormat("Error when getting EPG information. request: {0}", JsonConvert.SerializeObject(epgRequest));
                        return status;
                    }
                }
                else
                {
                    log.ErrorFormat("Error when getting EPG information. request: {0}", JsonConvert.SerializeObject(epgRequest));
                    status = new Status((int)eResponseStatus.ProgramDoesntExist, "program does not exists");
                    return status;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error when calling catalog to get EPG information. request: {0}, ex: {1}", JsonConvert.SerializeObject(epgRequest), ex);
                return status;
            }

            return new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
        }

        public static Status GetMedia(int partnerId, int assetId, out MediaObj media, out string seriesName)
        {
            media = null;
            seriesName = string.Empty;

            try
            {
                var mediaObj = FollowManager.GetMediaObj(partnerId, assetId);
                if (mediaObj != null)
                {
                    CatalogGroupCache cache = null;
                    AssetStruct episodeAssetStruct = null;
                    var episodeAssociationTag = FollowManager.GetEpisodeAssociationTag(partnerId, ref cache, ref episodeAssetStruct, mediaObj.m_oMediaType.m_nTypeID).ToLower().Trim();

                    var topicType = MetaType.Tag;
                    if (cache != null && episodeAssetStruct != null)
                    {
                        topicType = cache.TopicsMapById[episodeAssetStruct.ConnectedParentMetaId.Value].Type;
                    }

                    var seriesNames = FollowManager.GetSeriesNames(topicType, episodeAssociationTag, mediaObj.m_lTags, mediaObj.m_lMetas);
                    if (seriesNames.Count > 0)
                    {
                        seriesName = seriesNames.FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error getting media. partner ID: {0}, asset ID: {1}. ex: {2}", partnerId, assetId, ex);
                return Status.Error;
            }

            return Status.Ok;
        }

        public GenericResponse<MessageAnnouncement> GetMessageAnnouncement(int groupId, long id)
        {
            var response = new GenericResponse<MessageAnnouncement>();

            try
            {
                // validate system announcements are enabled
                if (!_notificationSettings.IsPartnerSystemAnnouncementEnabled(groupId))
                {
                    log.Error($"GetMessageAnnouncement - partner system announcements are disabled. groupId={groupId}");
                    response.SetStatus(eResponseStatus.FeatureDisabled, "Feature Disabled");
                    return response;
                }

                var dr = NotificationDal.Get_MessageAnnouncement(id, groupId);
                if (dr == null)
                {
                    log.Error($"Announcement not exist in DB: group: {groupId}, Id: {id}");
                    response.SetStatus(eResponseStatus.AnnouncementNotFound, ANNOUNCEMENT_NOT_FOUND);
                    return response;
                }

                response.Object = dr;
                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                response.SetStatus(eResponseStatus.Error);
                log.Error($"An Exception was occurred in GetMessageAnnouncement. groupId:{groupId}, id:{id}.", ex);
            }

            return response;
        }

        public GenericListResponse<MessageAnnouncement> GetMessageAnnouncementsByAnnouncementId(int groupId, long announcementId)
        {
            var response = new GenericListResponse<MessageAnnouncement>();

            try
            {
                // validate system announcements are enabled
                if (!_notificationSettings.IsPartnerSystemAnnouncementEnabled(groupId))
                {
                    log.Error($"GetMessageAnnouncement - partner system announcements are disabled. groupId={groupId}");
                    response.SetStatus(eResponseStatus.FeatureDisabled, "Feature Disabled");
                    return response;
                }

                var utcNow = DateUtils.GetUtcUnixTimestampNow();
                var announcements = _notificationRepository.Get_MessageAnnouncementsByAnnouncementId((int)announcementId, utcNow, utcNow);

                if (announcements.failRes &&
                    (announcements.messageAnnouncement == null || !announcements.messageAnnouncement.Any()))
                {
                    log.Error($"Announcement not exist in DB: group: {groupId}, Id: {announcementId}");
                    response.SetStatus(eResponseStatus.AnnouncementNotFound, ANNOUNCEMENT_NOT_FOUND);
                    return response;
                }

                response.Objects = announcements.messageAnnouncement;
                response.TotalItems = announcements.messageAnnouncement.Count;
                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                response.SetStatus(eResponseStatus.Error);
                log.Error($"An Exception was occurred in GetMessageAnnouncement. groupId:{groupId}, announcement Id:{announcementId}.", ex);
            }

            return response;
        }
    }
}