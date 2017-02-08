using ApiObjects;
using ApiObjects.Statistics;
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

            string urlV1 = Utils.GetWSURL("ES_URL_V1");
            string urlV2 = Utils.GetWSURL("ES_URL_V2");
            string originalUrl = Utils.GetWSURL("ES_URL");

            HashSet<string> urls = new HashSet<string>();
            urls.Add(urlV1);
            urls.Add(urlV2);
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
                foreach (var url in urls)
                {
                    try
                    {
                        ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();

                        if (esApi.IndexExists(statisticsIndex))
                        {
                            bool currentSuccess = esApi.InsertRecord(statisticsIndex, ElasticSearch.Common.Utils.ES_STATS_TYPE, guid, viewJson);

                            if (!currentSuccess)
                            {
                                result = false;
                                log.Debug("InsertMediaViewToES " + string.Format("Was unable to insert record to ES. index={0};type={1};doc={2}",
                                    statisticsIndex, ElasticSearch.Common.Utils.ES_STATS_TYPE, action));
                            }

                            if (increaseViewCount)
                            {
                                esApi.IncrementField(regularIndex, "media", mediaID.ToString(), "views");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("InsertMediaViewToES - " +
                            string.Format("Failed ex={0}, group={1};type={2}", ex.Message, groupId, ElasticSearch.Common.Utils.ES_STATS_TYPE), ex);
                        result = false;
                    }
                }
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

            string urlV1 = Utils.GetWSURL("ES_URL_V1");
            string urlV2 = Utils.GetWSURL("ES_URL_V2");
            string originalUrl = Utils.GetWSURL("ES_URL");

            HashSet<string> urls = new HashSet<string>();
            urls.Add(urlV1);
            urls.Add(urlV2);
            urls.Add(originalUrl);

            if (urls.Count > 0)
            {
                result = true;
            }

            Guid guid = Guid.NewGuid();
            string statisticsIndex = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(groupId);
            string normalIndex = groupId.ToString();

            foreach (var url in urls)
            {
                try
                {
                    ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi(url);


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

                        bool currentSuccess = esApi.InsertRecord(statisticsIndex, ElasticSearch.Common.Utils.ES_STATS_TYPE, guid.ToString(), actionStatsJson);

                        if (!currentSuccess)
                        {
                            result = false;
                            log.Debug("InsertStatisticsToES " + string.Format("Was unable to insert record to ES. index={0};type={1};doc={2}",
                                statisticsIndex, ElasticSearch.Common.Utils.ES_STATS_TYPE, actionStatsJson));
                        }

                        string fieldToUpdate = string.Empty;
                        bool shouldIncrement = true;

                        switch (action)
                        {
                            case eUserAction.LIKE:
                            {
                                fieldToUpdate = "likes";
                                break;
                            }
                            case eUserAction.UNLIKE:
                            {
                                fieldToUpdate = "likes";
                                shouldIncrement = false;
                                break;
                            }
                            case eUserAction.SHARE:
                            {
                                fieldToUpdate = "shares";
                                break;
                            }
                            case eUserAction.RATES:
                            {
                                fieldToUpdate = "votes";
                                break;
                            }
                            case eUserAction.UNKNOWN:
                            case eUserAction.POST:
                            case eUserAction.WATCHES:
                            case eUserAction.WANTS_TO_WATCH:
                            case eUserAction.FOLLOWS:
                            case eUserAction.UNFOLLOW:
                            default:
                            {
                                break;
                            }
                        }

                        // Recalculate rating of document
                        if (action == eUserAction.RATES)
                        {
                            string partialUpdate = string.Concat(
                                "{ \"script\": \"ctx._source.rating=(((ctx._source.rating * ctx._source.votes) + ", rateValue ,") / (ctx._source.votes + 1))\" }");
                            esApi.PartialUpdate(statisticsIndex, "media", assetId.ToString(), partialUpdate);
                        }

                        // Sunny: this currently doesn't work well and causes many errors. I comment these lines for now, 
                        // but I believe we should have a total number of likes/views/whatever on the document itself
                        //if (!string.IsNullOrEmpty(fieldToUpdate))
                        //{
                        //    if (shouldIncrement)
                        //    {
                        //        esApi.IncrementField(normalIndex, "media", assetId.ToString(), fieldToUpdate);
                        //    }
                        //    else
                        //    {
                        //        esApi.DecrementField(normalIndex, "media", assetId.ToString(), fieldToUpdate);
                        //    }
                        //}
                    }
                }
                catch (Exception ex)
                {
                    log.Error("InsertStatisticsToES - " +
                        string.Format("Failed ex={0}, group={1};type={2}", ex.Message, groupId, ElasticSearch.Common.Utils.ES_STATS_TYPE), ex);
                    result = false;
                }
            }

            return result;
        }

    }
}
