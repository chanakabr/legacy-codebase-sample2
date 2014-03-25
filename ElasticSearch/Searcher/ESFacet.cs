using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Searcher
{

    public interface IESFacet
    {

    }


    public class ESTermsFacet : IESFacet
    {
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
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter sw = new System.IO.StringWriter(sb);
            Newtonsoft.Json.JsonWriter jsonWriter = new Newtonsoft.Json.JsonTextWriter(sw);
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
            string sRes = sw.ToString();

            jsonWriter.Close();
            sw.Close();

            return sRes;
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
                                dict[tm.Value<string>().ToString()] = count.Value<int>();       
                            }
                            catch(Exception ex)
                            {
                                Logger.Logger.Log("Error", string.Format("search facets json parse failure. ex={0}; stack={1}", ex.Message, ex.StackTrace), "ElasticSearch");
                            }
                        }

                        dFacetResults[fkey] = dict;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log("Error", string.Format("Could not parse facet results. ex={0}; stack={1}", ex.Message, ex.StackTrace), "ElasticSearch");
                }
            }

            return dFacetResults;
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
