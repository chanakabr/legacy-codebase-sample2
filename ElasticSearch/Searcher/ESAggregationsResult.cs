using KLogMonitor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearch.Searcher
{
    [DataContract]
    public class ESAggregationsResult
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Parse Results

        public static Dictionary<string, Dictionary<T, int>> DeserializeAggrgations<T>(string json)
        {
            Dictionary<string, Dictionary<T, int>> result = new Dictionary<string, Dictionary<T, int>>();

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var jObject = JObject.Parse(json);
                    var aggregations = jObject["aggregations"];

                    if (aggregations != null)
                    {
                        foreach (JToken token in aggregations)
                        {
                            var aggregationKey = token.Value<JProperty>();
                            var fkey = aggregationKey.Name;
                            var fvalue = aggregationKey.Value;

                            var buckets = fvalue["buckets"];

                            if (buckets != null)
                            {
                                Dictionary<T, int> currentDictionary = new Dictionary<T, int>();
                                foreach (JToken bucket in buckets)
                                {
                                    try
                                    {
                                        var key = bucket["key"];
                                        var count = bucket["doc_count"];
                                        currentDictionary[key.Value<T>()] = count.Value<int>();
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Error(string.Format("Error - search aggregations json parse failure. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
                                    }
                                }

                                result[fkey] = currentDictionary;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Error - Could not parse aggregations results. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
                }
            }

            return result;
        }

        public static Dictionary<string, List<StatisticsAggregationResult>> DeserializeStatisticsAggregations(string json, string subAggregationName)
        {
            Dictionary<string, List<StatisticsAggregationResult>> result = new Dictionary<string, List<StatisticsAggregationResult>>();

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var jObject = JObject.Parse(json);

                    if (jObject == null)
                    {
                        return result;
                    }

                    var aggregations = jObject["aggregations"];

                    if (aggregations == null)
                    {
                        return result;
                    }

                    foreach (JToken token in aggregations)
                    {
                        var aggregationKey = token.Value<JProperty>();
                        var fkey = aggregationKey.Name;
                        var fvalue = aggregationKey.Value;

                        var buckets = fvalue["buckets"];

                        var list = new List<StatisticsAggregationResult>();
                        foreach (JToken term in buckets)
                        {
                            try
                            {
                                var tm = term["key"];
                                var subAggregation = term[subAggregationName];

                                var count = term["doc_count"];
                                var min = subAggregation["min"];
                                var max = subAggregation["max"];
                                var sum = subAggregation["sum"];
                                var avg = subAggregation["avg"];

                                list.Add(new StatisticsAggregationResult()
                                {
                                    key = tm.Value<string>(),
                                    count = count.Value<int>(),
                                    min = min.Value<long>(),
                                    max = max.Value<long>(),
                                    sum = sum.Value<long>(),
                                    avg = avg.Value<double>()
                                });
                            }
                            catch (Exception ex)
                            {
                                log.Error("Error - " + string.Format("search aggregations json parse failure. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
                            }
                        }

                        result[fkey] = list;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + string.Format("Could not parse aggregations results. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
                }
            }

            return result;
        }

        public static ESAggregationsResult FullParse(string json, List<ESBaseAggsItem> searchAggregations)
        {
            ESAggregationsResult result = new ESAggregationsResult();

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var jObject = JObject.Parse(json);

                    if (jObject == null)
                    {
                        return result;
                    }

                    var aggregations = jObject["aggregations"];

                    if (aggregations == null)
                    {
                        return result;
                    }

                    result.Aggregations = new Dictionary<string, ESAggregationResult>();

                    Stack<KeyValuePair<ESBaseAggsItem, JToken>> queue = new Stack<KeyValuePair<ESBaseAggsItem, JToken>>();

                    foreach (var currentAggregation in searchAggregations)
                    {
                        JToken token = aggregations[currentAggregation.Name];

                        try
                        {
                            var currentAggregationResult = SingleAggregationParse(token, currentAggregation);
                            result.Aggregations.Add(currentAggregation.Name, currentAggregationResult);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error - " + string.Format("search aggregations json parse failure. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + string.Format("Could not parse aggregations results. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
                }
            }

            return result;
        }

        public static ESAggregationResult SingleAggregationParse(JToken currentToken, ESBaseAggsItem aggregationItem)
        {
            ESAggregationResult result = new ESAggregationResult();
            
            int doc_count = 0;
            int doc_count_error_upper_bound = 0;
            int sum_other_doc_count = 0;
            int count = 0;
            double min = 0;
            double max = 0;
            double avg = 0;
            double sum = 0;
            string min_as_string = string.Empty;
            string max_as_string = string.Empty;
            string avg_as_string = string.Empty;
            string sum_as_string = string.Empty;

            if (currentToken["doc_count"] != null)
            {
                doc_count = currentToken["doc_count"].Value<int>();
            }

            if (currentToken["doc_count_error_upper_bound"] != null)
            {
                doc_count_error_upper_bound = currentToken["doc_count_error_upper_bound"].Value<int>();
            }

            if (currentToken["sum_other_doc_count"] != null)
            {
                sum_other_doc_count = currentToken["sum_other_doc_count"].Value<int>();
            }

            if (currentToken["count"] != null)
            {
                count = currentToken["count"].Value<int>();
            }

            if (currentToken["min"] != null)
            {
                min = currentToken["min"].Value<double>();
            }

            if (currentToken["max"] != null)
            {
                max = currentToken["max"].Value<double>();
            }

            if (currentToken["avg"] != null)
            {
                avg = currentToken["avg"].Value<double>();
            }

            if (currentToken["sum"] != null)
            {
                sum = currentToken["sum"].Value<double>();
            }

            if (currentToken["min_as_string"] != null)
            {
                min_as_string = currentToken["min_as_string"].Value<string>();
            }

            if (currentToken["max_as_string"] != null)
            {
                max_as_string = currentToken["max_as_string"].Value<string>();
            }

            if (currentToken["avg_as_string"] != null)
            {
                avg_as_string = currentToken["avg_as_string"].Value<string>();
            }

            if (currentToken["sum_as_string"] != null)
            {
                sum_as_string = currentToken["sum_as_string"].Value<string>();
            }

            result.avg = avg;
            result.avg_as_string = avg_as_string;
            result.count = count;
            result.doc_count = doc_count;
            result.doc_count_error_upper_bound = doc_count_error_upper_bound;
            result.max = max;
            result.max_as_string = max_as_string;
            result.min = min;
            result.min_as_string = min_as_string;
            result.sum = sum;
            result.sum_as_string = sum_as_string;
            result.sum_other_doc_count = sum_other_doc_count;

            Dictionary<string, ESAggregationResult> subs = new Dictionary<string, ESAggregationResult>();
            List<ESAggregationBucket> buckets = new List<ESAggregationBucket>();

            if (aggregationItem.SubAggrgations != null)
            {
                foreach (var subAggregation in aggregationItem.SubAggrgations)
                {
                    JToken subToken = currentToken[subAggregation.Name];

                    if (subToken != null)
                    {
                        ESAggregationResult subResult = SingleAggregationParse(subToken, subAggregation);

                        subs.Add(subAggregation.Name, subResult);
                    }
                }
            }

            JToken bucketsToken = currentToken["buckets"];

            if (bucketsToken != null)
            {
                foreach (JToken bucketToken in bucketsToken)
                {
                    ESAggregationBucket bucket = new ESAggregationBucket();

                    string key = string.Empty;
                    int bucket_doc_count = 0;

                    if (bucketToken["doc_count"] != null)
                    {
                        bucket_doc_count = bucketToken["doc_count"].Value<int>();
                    }

                    if (bucketToken["key"] != null)
                    {
                        key = bucketToken["key"].Value<string>();
                    }

                    bucket.key = key;
                    bucket.doc_count = bucket_doc_count;

                    Dictionary<string, ESAggregationResult> bucketAggregations = new Dictionary<string, ESAggregationResult>();

                    foreach (var subAggregation in aggregationItem.SubAggrgations)
                    {
                        JToken subToken = bucketToken[subAggregation.Name];

                        if (subToken != null)
                        {
                            ESAggregationResult subResult = SingleAggregationParse(subToken, subAggregation);

                            bucketAggregations.Add(subAggregation.Name, subResult);
                        }
                    }

                    bucket.Aggregations = bucketAggregations;

                    buckets.Add(bucket);
                }
            }

            result.Aggregations = subs;
            result.buckets = buckets;

            return result;
        }

        #endregion

        [DataMember]
        public Dictionary<string, ESAggregationResult> Aggregations;
    }

    public class ESAggregationResult
    {
        public int doc_count;
        public Dictionary<string, ESAggregationResult> Aggregations;
        public int doc_count_error_upper_bound;
        public int sum_other_doc_count = 0;
        public List<ESAggregationBucket> buckets;

        public int count;
        public double min;
        public double max;
        public double avg;
        public double sum;
        public string min_as_string;
        public string max_as_string;
        public string avg_as_string;
        public string sum_as_string;
    }

    public class ESAggregationBucket
    {
        public string key;
        public int doc_count;
        public Dictionary<string, ESAggregationResult> Aggregations;
    }

    public class ESAggregationBucketStatistics
    {

    }

    #region Result

    public class StatisticsAggregationResult
    {
        public string key
        {
            get;
            set;
        }
        public int count
        {
            get;
            set;
        }
        public long min
        {
            get;
            set;
        }
        public long max
        {
            get;
            set;
        }
        public long sum
        {
            get;
            set;
        }
        public double avg
        {
            get;
            set;
        }
    }

    #endregion

    #region Comparaer

    public class AggregationsComparer : IComparer<StatisticsAggregationResult>
    {
        public enum eCompareType
        {
            Key,
            Count,
            Min,
            Max,
            Sum,
            Average
        };

        private eCompareType m_compareType;
        public AggregationsComparer(eCompareType compareType)
        {
            m_compareType = compareType;
        }

        public int Compare(StatisticsAggregationResult stats1, StatisticsAggregationResult stats2)
        {

            if (stats1 == null && stats2 == null)
                return 0;
            else if (stats1 == null)
                return 1;
            else if (stats2 == null)
                return -1;
            else
            {
                switch (m_compareType)
                {
                    case eCompareType.Key:
                    return string.Compare(stats2.key, stats1.key);
                    case eCompareType.Sum:
                    return stats2.sum.CompareTo(stats1.sum);
                    case eCompareType.Count:
                    return stats2.count.CompareTo(stats1.count);
                    case eCompareType.Min:
                    return stats2.min.CompareTo(stats1.min);
                    case eCompareType.Max:
                    return stats2.max.CompareTo(stats1.max);
                    case eCompareType.Average:
                    return stats2.avg.CompareTo(stats1.avg);
                    default:
                    return stats2.count.CompareTo(stats1.count);
                }
            }
        }
    }

    #endregion
}
