using KLogMonitor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearch.Searcher
{
    public enum eElasticAggregationType
    {
        avg,
        cardinality,
        extended_stats,
        geo_bounds,
        geo_centroid,
        max,
        min,
        percentiles,
        stats,
        sum,
        value_count,
        terms
    }

    public class ESAggregations
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static Dictionary<string, Dictionary<int, int>> IntegerFacetResults(ref string resJson)
        {
            Dictionary<string, Dictionary<int, int>> dFacetResults = new Dictionary<string, Dictionary<int, int>>();
            if (!string.IsNullOrEmpty(resJson))
            {
                try
                {
                    var jObject = JObject.Parse(resJson);
                    var facets = jObject["facets"];

                    foreach (JToken token in facets)
                    {
                        var facetKey = token.Value<JProperty>();
                        var fkey = facetKey.Name;
                        var fvalue = facetKey.Value;

                        var terms = fvalue["terms"];

                        var dict = new Dictionary<int, int>();
                        foreach (JToken term in terms)
                        {
                            try
                            {
                                var tm = term["term"];
                                var count = term["count"];
                                dict[Int32.Parse(tm.Value<string>())] = count.Value<int>();
                            }
                            catch (Exception ex)
                            {
                                log.Error("Error - " + string.Format("search facets json parse failure. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
                            }
                        }

                        dFacetResults[fkey] = dict;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + string.Format("Could not parse facet results. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
                }
            }

            return dFacetResults;
        }

        public static Dictionary<string, Dictionary<string, int>> FacetResults(ref string resJson)
        {
            Dictionary<string, Dictionary<string, int>> dFacetResults = new Dictionary<string, Dictionary<string, int>>();
            if (!string.IsNullOrEmpty(resJson))
            {
                try
                {
                    var jObject = JObject.Parse(resJson);
                    var facets = jObject["facets"];

                    foreach (JToken token in facets)
                    {
                        var facetKey = token.Value<JProperty>();
                        var fkey = facetKey.Name;
                        var fvalue = facetKey.Value;

                        var terms = fvalue["terms"];

                        var dict = new Dictionary<string, int>();
                        foreach (JToken term in terms)
                        {
                            try
                            {
                                var tm = term["term"];
                                var count = term["count"];
                                dict[tm.Value<string>()] = count.Value<int>();
                            }
                            catch (Exception ex)
                            {
                                log.Error("Error - " + string.Format("search facets json parse failure. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
                            }
                        }

                        dFacetResults[fkey] = dict;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + string.Format("Could not parse facet results. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
                }
            }

            return dFacetResults;
        }

        public static Dictionary<string, Dictionary<string, int>> DeserializeAggrgations(string json)
        {
            Dictionary<string, Dictionary<string, int>> result = new Dictionary<string, Dictionary<string, int>>();

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var jObject = JObject.Parse(json);
                    var facets = jObject["aggregations"];

                    foreach (JToken token in facets)
                    {
                        var facetKey = token.Value<JProperty>();
                        var fkey = facetKey.Name;
                        var fvalue = facetKey.Value;

                        var buckets = fvalue["buckets"];

                        Dictionary<string, int> currentDictionary = new Dictionary<string, int>();
                        foreach (JToken bucket in buckets)
                        {
                            try
                            {
                                var key = bucket["key"];
                                var count = bucket["doc_count"];
                                currentDictionary[key.Value<string>()] = count.Value<int>();
                            }
                            catch (Exception ex)
                            {
                                log.Error("Error - " + string.Format("search aggregations json parse failure. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
                            }
                        }

                        result[fkey] = currentDictionary;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + string.Format("Could not parse aggregations results. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
                }
            }

            return result;
        }
    }

    public class ESBaseAggsItem
    {
        #region Members
        
        public string Name;
        public IESTerm Filter;
        public eElasticAggregationType Type;
        public List<ESBaseAggsItem> SubAggrgations;
        public string Meta;

        public string Field;
        List<KeyValuePair<string, string>> AdditionalInnerParameters;

        public bool IsNumeric;

        #endregion

        #region Ctor

        public ESBaseAggsItem()
        {
            this.SubAggrgations = new List<ESBaseAggsItem>();
            this.AdditionalInnerParameters = new List<KeyValuePair<string, string>>();
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            string result = string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("\"{0}\":", this.Name);
            sb.Append("{");

            // META
            if (!string.IsNullOrEmpty(this.Meta))
            {
                sb.Append("\"meta\":{");
                sb.Append(this.Meta);
                sb.Append("},");
            }

            // AGGREGATIONS (sub aggregations)
            if (this.SubAggrgations != null && this.SubAggrgations.Count > 0)
            {
                sb.Append("\"aggregations\":{");

                foreach (ESBaseAggsItem item in this.SubAggrgations)
                {
                    sb.AppendFormat("{0},", item.ToString());
                }

                sb.Remove(sb.Length - 1, 1);
                sb.Append("},");
            }

            // FILTER
            if (this.Filter != null && !this.Filter.IsEmpty())
            {
                sb.Append("\"filter\":{");
                sb.Append(this.Filter.ToString());
                sb.AppendFormat("},");
            }

            // Inner part - the actual aggregation part...
            string innerPart = this.InnerToString();

            sb.Append(innerPart);
            sb.Append("}");

            result = sb.ToString();

            return result;
        }

        protected virtual string InnerToString()
        {
            string result = string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("\"{0}\":", this.Type);
            sb.Append("{");
            sb.AppendFormat("\"field\": \"{0}\"", this.Field);

            if (this.AdditionalInnerParameters != null && this.AdditionalInnerParameters.Count > 0)
            {
                foreach (var item in this.AdditionalInnerParameters)
                {
                    sb.AppendFormat(",\"{0}\": \"{1}\"", item.Key, item.Value);
                }
            }

            sb.Append("}");

            result = sb.ToString();

            return result;
        }

        #endregion

    }
}
