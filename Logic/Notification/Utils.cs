using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Globalization;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;
using ApiObjects.Notification;
using ApiObjects;
using System.Data;
using System.Web;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
using DAL;
using CachingProvider.LayeredCache;

namespace Core.Notification
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());



        static public string GetWSURL(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        public static string ExtractDate(DateTime date, string format)
        {
            try
            {
                string result = date.ToString(format);
                return result;
            }
            catch (FormatException ex)
            {
                log.Error("", ex);
                return string.Empty;
            }
            catch (Exception exp)
            {
                log.Error("", exp);
                return string.Empty;
            }
        }

        public static MessageAnnouncement GetMessageAnnouncementFromDataRow(DataRow row)
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
                                                              status,
                                                              ODBCWrapper.Utils.GetSafeStr(row, "message_reference"),
                                                              ODBCWrapper.Utils.GetIntSafeVal(row, "announcement_id"),
                                                              ODBCWrapper.Utils.GetSafeStr(row, "image_url"));

            msg.MessageAnnouncementId = ODBCWrapper.Utils.GetIntSafeVal(row, "id");

            return msg;
        }

        ///<summary>
        /// Base 64 Encoding with URL and Filename Safe Alphabet using UTF-8 character set.
        ///</summary>
        ///<param name="str">The original string</param>
        ///<returns>The Base64 encoded string</returns>
        public static string Base64ForUrlEncode(string str)
        {
            byte[] encbuff = Encoding.UTF8.GetBytes(str);
            return HttpServerUtility.UrlTokenEncode(encbuff);
        }
        ///<summary>
        /// Decode Base64 encoded string with URL and Filename Safe Alphabet using UTF-8.
        ///</summary>
        ///<param name="str">Base64 code</param>
        ///<returns>The decoded string.</returns>
        public static string Base64ForUrlDecode(string str)
        {
            byte[] decbuff = HttpServerUtility.UrlTokenDecode(str);
            return Encoding.UTF8.GetString(decbuff);
        }


        public static List<Core.Catalog.Response.UnifiedSearchResult> SearchSeriesEpisodes(int groupId, string seriesId, long? seasonNumber, long epgChannelId)
        {
            string seriesIdName, seasonNumberName, episodeNumberName;
            if (!Core.ConditionalAccess.Utils.GetSeriesMetaTagsFieldsNamesForSearch(groupId, out seriesIdName, out seasonNumberName, out episodeNumberName))
            {
                log.ErrorFormat("failed to 'GetSeriesMetaTagsNamesForGroup' for groupId = {0} ", groupId);
                return null;
            }

            // build the filter query for the search
            string ksql = string.Format("(and {0} = '{1}' epg_channel_id = '{2}' {3} start_date > '0')",
                seriesIdName, seriesId, epgChannelId, seasonNumber.HasValue && seasonNumber.Value != 0  ? string.Format("{0} = '{1}' ", seasonNumberName, seasonNumber) : string.Empty);

            // get program ids
            try
            {
                Core.Catalog.Request.UnifiedSearchRequest request = new Core.Catalog.Request.UnifiedSearchRequest()
                {
                    m_nGroupID = groupId,
                    m_dServerTime = DateTime.UtcNow,
                    m_nPageIndex = 0,
                    m_nPageSize = 0,
                    assetTypes = new List<int> { 0 },
                    filterQuery = ksql.ToString(),
                    order = new ApiObjects.SearchObjects.OrderObj()
                    {
                        m_eOrderBy = ApiObjects.SearchObjects.OrderBy.START_DATE,
                        m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC
                    },
                    m_oFilter = new Core.Catalog.Filter()
                    {
                        m_bOnlyActiveMedia = true
                    },
                };

                Core.ConditionalAccess.Utils.FillCatalogSignature(request);
                string catalogUrl = GetWSURL("WS_Catalog");
                if (string.IsNullOrEmpty(catalogUrl))
                {
                    log.Error("Catalog Url is null or empty");
                    return null;
                }

                Core.Catalog.Response.UnifiedSearchResponse response = request.GetResponse(request) as Core.Catalog.Response.UnifiedSearchResponse;

                if (response == null || response.status == null)
                {
                    log.ErrorFormat("Got empty response from Catalog 'GetResponse' for 'UnifiedSearchRequest'");
                    return null;
                }
                if (response.status.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Got error response from catalog 'GetResponse' for 'UnifiedSearchRequest'. response: code = {0}, message = {1}", response.status.Code, response.status.Message);
                    return null;
                }

                return response.searchResults;
            }

            catch (Exception ex)
            {
                log.Error("SearchSeriesEpisodes - Failed UnifiedSearchRequest Request To Catalog", ex);
                return null;
            }
        }

        internal static void WaitForAllTasksToFinish(List<Task> tasks)
        {
            try
            {
                if (tasks != null && tasks.Count > 0)
                {
                    Task.WaitAll(tasks.ToArray());
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed waiting for all tasks to finish", ex);
            }
        }

        internal static List<DbSeriesReminder> GetSeriesReminders(int groupId, List<long> seriesReminderIds)
        {
            List<DbSeriesReminder> res = null;
            if (seriesReminderIds == null || seriesReminderIds.Count == 0)
            {
                return res;
            }
            
            try
            {
                Dictionary<string, DbSeriesReminder> seriesReminderMap = null;
                Dictionary<string, string> keyToOriginalValueMap = LayeredCacheKeys.GetSeriesRemindersKeysMap(groupId, seriesReminderIds);
                Dictionary<string, List<string>> invalidationKeysMap = LayeredCacheKeys.GetSeriesRemindersInvalidationKeysMap(groupId, seriesReminderIds);
                if (!LayeredCache.Instance.GetValues<DbSeriesReminder>(keyToOriginalValueMap, ref seriesReminderMap, GetSeriesReminder, new Dictionary<string, object>() { { "groupId", groupId },
                                                                        { "seriesReminderIds", seriesReminderIds } }, groupId, LayeredCacheConfigNames.GET_SERIES_REMINDERS_CACHE_CONFIG_NAME,
                                                                        invalidationKeysMap))
                {
                    log.ErrorFormat("Failed getting seriesReminders from LayeredCache, groupId: {0}, seriesReminderIds", groupId, string.Join(",", seriesReminderIds));
                }
                else if (seriesReminderMap != null)
                {
                    res = seriesReminderMap.Values.ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetSeriesReminders for groupId: {0}, seriesReminderIds: {1}", groupId, string.Join(",", seriesReminderIds)), ex);
            }

            return res;
        }

        internal static Tuple<Dictionary<string, DbSeriesReminder>, bool> GetSeriesReminder(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, DbSeriesReminder> result = new Dictionary<string, DbSeriesReminder>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("seriesReminderIds"))
                {                                        
                    int? groupId = funcParams["groupId"] as int?;
                    List<long> seriesReminderIds = null;
                    if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                    {
                        seriesReminderIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => long.Parse(x)).ToList();                        
                    }
                    else
                    {
                        seriesReminderIds = funcParams["seriesReminderIds"] != null ? funcParams["seriesReminderIds"] as List<long> : null;
                    }

                    if (seriesReminderIds != null && seriesReminderIds.Count > 0 && groupId.HasValue)
                    {
                        Dictionary<long, DbSeriesReminder> tempResult = new Dictionary<long, DbSeriesReminder>();
                        List<DbSeriesReminder> seriesReminders = NotificationDal.GetSeriesReminders(groupId.Value, seriesReminderIds);
                        if (seriesReminders != null && seriesReminders.Count > 0)
                        {
                            tempResult = seriesReminders.ToDictionary(x => long.Parse(x.ID.ToString()), x => x);                            
                        }

                        List<long> missingIds = seriesReminderIds.Where(x => !tempResult.ContainsKey(x)).ToList();                        
                        if (missingIds != null)
                        {
                            foreach (long missingId in missingIds)
                            {
                                tempResult.Add(missingId, new DbSeriesReminder());
                            }
                        }

                        result = tempResult.ToDictionary(x => LayeredCacheKeys.GetSeriesRemindersKey(groupId.Value, x.Key), x => x.Value);
                    }

                    res = result.Keys.Count() == seriesReminderIds.Count();                    
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetSeriesReminder failed with params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, DbSeriesReminder>, bool>(result, res);
        }

        internal static bool GetSeriesMetaTagsFieldsNamesAndTypes(int groupId, out Tuple<string, FieldTypes> seriesIdName,
            out Tuple<string, FieldTypes> seasonNumberName, out Tuple<string, FieldTypes> episodeNumberName)
        {
            seriesIdName = seasonNumberName = episodeNumberName = null;

            var metaTagsMappings = ConditionalAccess.Utils.GetAliasMappingFields(groupId);
            if (metaTagsMappings == null || metaTagsMappings.Count == 0)
            {
                log.ErrorFormat("failed to 'GetAliasMappingFields' for seriesId. groupId = {0} ", groupId);
                return false;
            }

            var feild = metaTagsMappings.Where(m => m.Alias.ToLower() == "series_id").FirstOrDefault();
            if (feild == null)
            {
                log.ErrorFormat("alias for series_id was not found. group_id = {0}", groupId);
                return false;
            }

            seriesIdName = new Tuple<string, FieldTypes>(feild.Name, feild.FieldType);

            feild = metaTagsMappings.Where(m => m.Alias.ToLower() == "season_number").FirstOrDefault();
            if (feild != null)
            {
                seasonNumberName = new Tuple<string, FieldTypes>(feild.Name, feild.FieldType);
            }

            feild = metaTagsMappings.Where(m => m.Alias.ToLower() == "episode_number").FirstOrDefault();
            if (feild != null)
            {
                episodeNumberName = new Tuple<string, FieldTypes>(feild.Name, feild.FieldType);
            }

            return true;
        }

        internal static List<DbReminder> GetReminders(int groupId, List<long> reminderIds)
        {
            List<DbReminder> res = null;
            if (reminderIds == null || reminderIds.Count == 0)
            {
                return res;
            }

            try
            {
                Dictionary<string, DbReminder> reminderMap = null;
                Dictionary<string, string> keyToOriginalValueMap = LayeredCacheKeys.GetRemindersKeysMap(groupId, reminderIds);
                Dictionary<string, List<string>> invalidationKeysMap = LayeredCacheKeys.GetRemindersInvalidationKeysMap(groupId, reminderIds);
                if (!LayeredCache.Instance.GetValues<DbReminder>(keyToOriginalValueMap, ref reminderMap, GetReminders, new Dictionary<string, object>() { { "groupId", groupId },
                                                                        { "reminderIds", reminderIds } }, groupId, LayeredCacheConfigNames.GET_REMINDERS_CACHE_CONFIG_NAME,
                                                                        invalidationKeysMap))
                {
                    log.ErrorFormat("Failed getting reminders from LayeredCache, groupId: {0}, reminderIds", groupId, string.Join(",", reminderIds));
                }
                else if (reminderMap != null)
                {
                    res = reminderMap.Values.ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetReminders for groupId: {0}, reminderIds: {1}", groupId, string.Join(",", reminderIds)), ex);
            }

            return res;
        }

        internal static Tuple<Dictionary<string, DbReminder>, bool> GetReminders(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, DbReminder> result = new Dictionary<string, DbReminder>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("reminderIds"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    List<long> reminderIds = null;
                    if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                    {
                        reminderIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => long.Parse(x)).ToList();                        
                    }
                    else
                    {
                        reminderIds = funcParams["reminderIds"] != null ? funcParams["reminderIds"] as List<long> : null;
                    }

                    if (reminderIds != null && reminderIds.Count > 0 && groupId.HasValue)
                    {
                        Dictionary<long, DbReminder> tempResult = new Dictionary<long, DbReminder>();
                        List<DbReminder> reminders = NotificationDal.GetReminders(groupId.Value, reminderIds);
                        if (reminders != null && reminders.Count > 0)
                        {
                            tempResult = reminders.ToDictionary(x => long.Parse(x.ID.ToString()), x => x);
                        }

                        List<long> missingIds = reminderIds.Where(x => !tempResult.ContainsKey(x)).ToList();
                        if (missingIds != null)
                        {
                            foreach (long missingId in missingIds)
                            {
                                tempResult.Add(missingId, new DbReminder());
                            }
                        }

                        result = tempResult.ToDictionary(x => LayeredCacheKeys.GetRemindersKey(groupId.Value, x.Key), x => x.Value);
                    }

                    res = result.Keys.Count() == reminderIds.Count();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetReminders failed with params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, DbReminder>, bool>(result, res);
        }


        public static Status GetUserNotificationData(int groupId, int userId, out UserNotification userNotificationData)
        {
            bool docExists = false;
            userNotificationData = DAL.NotificationDal.GetUserNotificationData(groupId, userId, ref docExists);
            if (userNotificationData == null)
            {
                if (docExists)
                {
                    // error while getting user notification data
                    log.ErrorFormat("error retrieving user notification data. GID: {0}, UID: {1}", groupId, userId);
                    return new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
                else
                {
                    log.DebugFormat("user announcement data wasn't found - going to create a new one. GID: {0}, UID: {1}", groupId, userId);

                    // create user notification object
                    userNotificationData = new UserNotification(userId) { CreateDateSec = TVinciShared.DateUtils.UnixTimeStampNow() };

                    //update user settings according to partner settings configuration                    
                    userNotificationData.Settings.EnablePush = NotificationSettings.IsPartnerPushEnabled(groupId, userId);

                    userNotificationData.Settings.EnableMail = NotificationSettings.IsPartnerMailNotificationEnabled(groupId);

                    if (userNotificationData.Settings.EnableMail.Value)
                    {
                        Users.User user = Users.User.GetUser(userId, groupId);
                        userNotificationData.Email = user.m_oBasicData.m_sEmail;
                    }
                }
            }

            return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
        }

    }
}
