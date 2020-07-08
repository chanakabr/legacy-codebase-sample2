using ApiObjects;
using ApiObjects.Statistics;
using ConfigurationManager;
using ElasticSearch.Common;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearch.Utilities
{
    public class ESStatisticsUtilities
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static bool InsertMediaView(int groupId, int mediaID, int mediaTypeID, string action, 
            int playTime, bool increaseViewCount, List<string> languages = null)
        {            
            bool result = false;
            string guid = Guid.NewGuid().ToString();

            MediaView view = new MediaView()
            {
                GroupID = groupId,
                MediaID = mediaID,
                Location = playTime,
                MediaType = mediaTypeID.ToString(),
                Action = action,
                Date = DateTime.UtcNow
            };

            string originalUrl = ApplicationConfiguration.Current.ElasticSearchConfiguration.URL.Value;

            HashSet<string> urls = new HashSet<string>();
            urls.Add(originalUrl);

            if (urls.Count > 0)
            {
                result = true;
            }

            string statisticsIndex = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(view.GroupID);
            string regularIndex = view.GroupID.ToString();

            string viewJson = Newtonsoft.Json.JsonConvert.SerializeObject(view);

            if (!string.IsNullOrEmpty(viewJson))
            {
                Parallel.ForEach<string>(urls, url =>
                {
                    try
                    {
                        ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();

                        bool currentSuccess = esApi.InsertRecord(statisticsIndex, ElasticSearch.Common.Utils.ES_STATS_TYPE, guid, viewJson);

                        if (!currentSuccess)
                        {
                            result = false;
                            log.Debug("InsertMediaViewToES " + string.Format("Was unable to insert record to ES. index={0};type={1};doc={2}",
                                statisticsIndex, ElasticSearch.Common.Utils.ES_STATS_TYPE, action));
                        }

                        // 
                        // Increment views field - it is not used and causes performance issues in production. 
                        // Until it will be used properly in ordering, it shouldn't happen
                        //
                        //if (increaseViewCount)
                        //{
                        //    esApi.IncrementField(regularIndex, "media", mediaID.ToString(), "views");
                        //}
                    }
                    catch (Exception ex)
                    {
                        log.Error("InsertMediaViewToES - " +
                            string.Format("Failed ex={0}, group={1};type={2}", ex.Message, groupId, ElasticSearch.Common.Utils.ES_STATS_TYPE), ex);
                        result = false;
                    }
                });
            }

            return result;
        }

        /// <summary>
        /// Puts a new document to ES statsistics index
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="assetId"></param>
        /// <param name="action"></param>
        /// <param name="rateValue"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool InsertSocialActionStatistics(int groupId, int assetId, int mediaType, eUserAction action,
            int rateValue = 0, DateTime date = default(DateTime))
        {
            bool result = false;

            Guid guid = Guid.NewGuid();
            string statisticsIndex = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(groupId);
            string normalIndex = groupId.ToString();

            try
            {
                ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();

                if (esApi.IndexExists(statisticsIndex))
                {
                    if (date == default(DateTime))
                    {
                        date = DateTime.UtcNow;
                    }

                    SocialActionStatistics actionStats = new SocialActionStatistics()
                    {
                        Action = action.ToString().ToLower(),
                        Date = date,
                        GroupID = groupId,
                        MediaID = assetId,
                        MediaType = mediaType.ToString(),
                        RateValue = rateValue
                    };

                    string actionStatsJson = Newtonsoft.Json.JsonConvert.SerializeObject(actionStats);

                    result = esApi.InsertRecord(statisticsIndex, ElasticSearch.Common.Utils.ES_STATS_TYPE, guid.ToString(), actionStatsJson);

                    if (!result)
                    {
                        log.Debug("InsertStatisticsToES " + string.Format("Was unable to insert record to ES. index={0};type={1};doc={2}",
                            statisticsIndex, ElasticSearch.Common.Utils.ES_STATS_TYPE, actionStatsJson));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("InsertStatisticsToES - " +
                    string.Format("Failed ex={0}, group={1};type={2}", ex.Message, groupId, ElasticSearch.Common.Utils.ES_STATS_TYPE), ex);
                result = false;
            }


            return result;
        }
    }
}
