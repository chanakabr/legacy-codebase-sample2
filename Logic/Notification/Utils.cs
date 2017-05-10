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
                                                              ODBCWrapper.Utils.GetIntSafeVal(row, "announcement_id"));

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
                seriesIdName, seriesId, epgChannelId, seasonNumber.HasValue ? string.Format("{0} = '{1}' ", seasonNumberName, seasonNumber) : string.Empty);

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
                List<string> keys = new List<string>();
                Dictionary<string, List<string>> invalidationKeysMap = LayeredCacheKeys.GetSeriesRemindersInvalidationKeysMap(groupId, seriesReminderIds);
                if (invalidationKeysMap != null && invalidationKeysMap.Count > 0)
                {
                    keys = invalidationKeysMap.Keys.ToList();
                }

                if (!LayeredCache.Instance.GetValues<DbSeriesReminder>(keys, ref seriesReminderMap, GetSeriesReminder, new Dictionary<string, object>() { { "groupId", groupId },
                                                                        { "seriesReminderIds", seriesReminderIds } }, groupId, LayeredCacheConfigNames.GET_SERIES_REMINDERS_CACHE_CONFIG_NAME,
                                                                        invalidationKeysMap))
                {
                    log.ErrorFormat("Failed getting seriesReminders from LayeredCache, groupId: {0}, seriesReminderIds", groupId, string.Join(",", seriesReminderIds));
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
                if (funcParams != null && funcParams.Count == 2 && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("seriesReminderIds"))
                {                                        
                    int? groupId = funcParams["groupId"] as int?;
                    List<long> seriesReminderIds = funcParams["seriesReminderIds"] != null ? funcParams["seriesReminderIds"] as List<long> : null;

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
    }
}
