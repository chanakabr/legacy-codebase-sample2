using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KLogMonitor;
using System.Reflection;

namespace ElasticSearch.Searcher
{

    public interface IESFacet
    {

    }


    public class ESTermsFacet : IESFacet
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public FilteredQuery Query { get; set; }
        public QueryFilter FacetFilter { get; set; }
        internal List<ESTermsFacetItem> facetItems;

        public ESTermsFacet(string facetName, string field, int size = 10)
        {
            facetItems = new List<ESTermsFacetItem>();
            this.AddTermFacet(facetName, field, size);
        }

        public ESTermsFacet()
        {
            facetItems = new List<ESTermsFacetItem>();
        }

        public ESTermsFacet AddTermFacet(string facetName, string field, int size = 10)
        {
            var item = new ESTermsFacetItem();
            item.FacetName = facetName;
            item.Field = field;
            item.Size = size;
            facetItems.Add(item);
            return this;
        }

        public ESTermsFacet AddTermFacet(ESTermsFacetItem facetItem)
        {
            if (facetItem != null)
            {
                ESTermsFacetItem item = new ESTermsFacetItem();
                item.FacetName = facetItem.FacetName;
                item.Field = facetItem.Field;
                item.Size = facetItem.Size;
                item.FacetFilter = facetItem.FacetFilter;
                facetItems.Add(item);
            }
            return this;
        }

        public class ESTermsFacetItem
        {
            public string Field;
            public int Size;
            public string FacetName { get; set; }
            public BaseFilterCompositeType FacetFilter { get; set; }
        }

        public override string ToString()
        {
            string sRes = string.Empty;
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter sw = null;
            Newtonsoft.Json.JsonWriter jsonWriter = null;
            try
            {
                sw = new System.IO.StringWriter(sb);
                jsonWriter = new Newtonsoft.Json.JsonTextWriter(sw);
                {
                    jsonWriter.WriteRawValue("{");
                    if (Query != null)
                    {
                        Query.m_bIsRoot = false;
                        jsonWriter.WriteRawValue(Query.ToString());
                        jsonWriter.WriteRawValue(",");
                    }

                    if (facetItems != null)
                    {
                        jsonWriter.WriteRawValue("\"facets\":");
                        jsonWriter.WriteStartObject();
                        foreach (ESTermsFacet.ESTermsFacetItem termsFacetItem in this.facetItems)
                        {
                            jsonWriter.WritePropertyName(termsFacetItem.FacetName);

                            //1
                            jsonWriter.WriteStartObject();
                            jsonWriter.WritePropertyName("terms");
                            jsonWriter.WriteStartObject();
                            jsonWriter.WritePropertyName("field");
                            jsonWriter.WriteValue(termsFacetItem.Field);
                            jsonWriter.WritePropertyName("size");
                            jsonWriter.WriteValue(termsFacetItem.Size);
                            jsonWriter.WriteEndObject();
                            //1

                            //2
                            if (termsFacetItem.FacetFilter != null && !termsFacetItem.FacetFilter.IsEmpty())
                            {
                                jsonWriter.WritePropertyName("facet_filter");
                                jsonWriter.WriteStartObject();
                                jsonWriter.WriteRaw(termsFacetItem.FacetFilter.ToString());
                                jsonWriter.WriteEndObject();
                            }
                            //2
                            jsonWriter.WriteEndObject();
                        }
                        jsonWriter.WriteEndObject();
                    }

                    jsonWriter.WriteRawValue("}");
                }
                sRes = sw.ToString();

            }
            finally
            {
                if (jsonWriter != null)
                {
                    jsonWriter.Close();
                }
                if (sw != null)
                {
                    sw.Close();
                }
            }

            return sRes;
        }

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
    }


    public class ESTermsStatsFacet : IESFacet
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public FilteredQuery Query { get; set; }
        public QueryFilter FacetFilter { get; set; }
        internal List<ESTermsStatsFacetItem> facetItems;

        public ESTermsStatsFacet(string facetName, string keyField, string valueField, int size = 10)
        {
            facetItems = new List<ESTermsStatsFacetItem>();
            this.AddTermStatsFacet(facetName, keyField, valueField, size);
        }

        public ESTermsStatsFacet()
        {
            facetItems = new List<ESTermsStatsFacetItem>();
        }

        public ESTermsStatsFacet AddTermStatsFacet(string facetName, string keyField, string valueField, int size = 10)
        {
            var item = new ESTermsStatsFacetItem();
            item.FacetName = facetName;
            item.KeyField = keyField;
            item.Size = size;
            item.ValueField = valueField;
            facetItems.Add(item);
            return this;
        }

        public ESTermsStatsFacet AddTermStatsFacet(ESTermsStatsFacetItem facetItem)
        {
            if (facetItem != null)
            {
                ESTermsStatsFacetItem item = new ESTermsStatsFacetItem();
                item.FacetName = facetItem.FacetName;
                item.KeyField = facetItem.KeyField;
                item.ValueField = facetItem.ValueField;
                item.Size = facetItem.Size;
                item.FacetFilter = facetItem.FacetFilter;
                facetItems.Add(item);
            }
            return this;
        }

        public class ESTermsStatsFacetItem
        {
            public string KeyField;
            public string ValueField;
            public int Size;
            public string FacetName { get; set; }
            public BaseFilterCompositeType FacetFilter { get; set; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string sRes = string.Empty;
            System.IO.StringWriter sw = null;
            Newtonsoft.Json.JsonWriter jsonWriter = null;
            try
            {
                sw = new System.IO.StringWriter(sb);
                jsonWriter = new Newtonsoft.Json.JsonTextWriter(sw);
                {
                    jsonWriter.WriteRawValue("{");
                    if (Query != null)
                    {
                        Query.m_bIsRoot = false;
                        jsonWriter.WriteRawValue(Query.ToString());
                        jsonWriter.WriteRawValue(",");
                    }

                    if (facetItems != null)
                    {
                        jsonWriter.WriteRawValue("\"facets\":");
                        jsonWriter.WriteStartObject();
                        foreach (ESTermsStatsFacet.ESTermsStatsFacetItem termsFacetItem in this.facetItems)
                        {
                            jsonWriter.WritePropertyName(termsFacetItem.FacetName);

                            //1
                            jsonWriter.WriteStartObject();
                            jsonWriter.WritePropertyName("terms_stats");
                            jsonWriter.WriteStartObject();
                            jsonWriter.WritePropertyName("key_field");
                            jsonWriter.WriteValue(termsFacetItem.KeyField);
                            jsonWriter.WritePropertyName("value_field");
                            jsonWriter.WriteValue(termsFacetItem.ValueField);
                            jsonWriter.WritePropertyName("size");
                            jsonWriter.WriteValue(termsFacetItem.Size);
                            jsonWriter.WriteEndObject();
                            //1

                            //2
                            if (termsFacetItem.FacetFilter != null && !termsFacetItem.FacetFilter.IsEmpty())
                            {
                                jsonWriter.WritePropertyName("facet_filter");
                                jsonWriter.WriteStartObject();
                                jsonWriter.WriteRaw(termsFacetItem.FacetFilter.ToString());
                                jsonWriter.WriteEndObject();
                            }
                            //2
                            jsonWriter.WriteEndObject();
                        }
                        jsonWriter.WriteEndObject();
                    }

                    jsonWriter.WriteRawValue("}");
                }
                sRes = sw.ToString();

            }
            finally
            {
                if (jsonWriter != null)
                {
                    jsonWriter.Close();
                }
                if (sw != null)
                {
                    sw.Close();
                }
            }

            return sRes;
        }

        public class StatisticFacetResult
        {
            public string term
            {
                get;
                set;
            }
            public int count
            {
                get;
                set;
            }
            public int totalCount
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
            public long total
            {
                get;
                set;
            }
            public double mean
            {
                get;
                set;
            }
        }


        public static Dictionary<string, List<StatisticFacetResult>> FacetResults(ref string resJson)
        {
            Dictionary<string, List<StatisticFacetResult>> dFacetResults = new Dictionary<string, List<StatisticFacetResult>>();
            if (!string.IsNullOrEmpty(resJson))
            {
                try
                {
                    var jObject = JObject.Parse(resJson);

                    if (jObject == null)
                    {
                        return dFacetResults;
                    }

                    var facets = jObject["facets"];

                    if (facets == null)
                    {
                        return dFacetResults;
                    }

                    foreach (JToken token in facets)
                    {
                        var facetKey = token.Value<JProperty>();
                        var fkey = facetKey.Name;
                        var fvalue = facetKey.Value;

                        var terms = fvalue["terms"];

                        var list = new List<StatisticFacetResult>();
                        foreach (JToken term in terms)
                        {
                            try
                            {
                                var tm = term["term"];
                                var count = term["count"];
                                var total_count = term["total_count"];
                                var min = term["min"];
                                var max = term["max"];
                                var total = term["total"];
                                var mean = term["mean"];
                                list.Add(new StatisticFacetResult()
                                        {
                                            term = tm.Value<string>(),
                                            count = count.Value<int>(),
                                            totalCount = total_count.Value<int>(),
                                            min = min.Value<long>(),
                                            max = max.Value<long>(),
                                            total = total.Value<long>(),
                                            mean = mean.Value<double>()
                                        });
                            }
                            catch (Exception ex)
                            {
                                log.Error("Error - " + string.Format("search facets json parse failure. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
                            }
                        }

                        dFacetResults[fkey] = list;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + string.Format("Could not parse facet results. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
                }
            }

            return dFacetResults;
        }


        public class FacetCompare : IComparer<StatisticFacetResult>
        {
            public enum eCompareType { TERM, COUNT, TOTAL_COUNT, MIN, MAX, TOTAL, MEAN };

            private eCompareType m_compareType;
            public FacetCompare(eCompareType compareType)
            {
                m_compareType = compareType;
            }

            public int Compare(StatisticFacetResult stats1, StatisticFacetResult stats2)
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

                        case eCompareType.MAX:
                            return stats2.max.CompareTo(stats1.max);
                        case eCompareType.MIN:
                            return stats2.min.CompareTo(stats1.min);
                        case eCompareType.TOTAL:
                            return stats2.total.CompareTo(stats1.total);
                        case eCompareType.TOTAL_COUNT:
                            return stats2.totalCount.CompareTo(stats1.totalCount);
                        case eCompareType.TERM:
                            return string.Compare(stats2.term, stats1.term);
                        case eCompareType.MEAN:
                            return stats2.mean.CompareTo(stats1.mean);
                        case eCompareType.COUNT:
                        default:
                            return stats2.count.CompareTo(stats1.count);
                    }
                }
            }
        }
    }

    public class ESFacteFilter : QueryFilter
    {
        public override string ToString()
        {
            string sRes = string.Empty;

            if (FilterSettings != null && !FilterSettings.IsEmpty())
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"facet_filter\": {");
                sb.Append(FilterSettings.ToString());
                sb.Append("}");
                sRes = sb.ToString();
            }

            return sRes;
        }
    }
}
