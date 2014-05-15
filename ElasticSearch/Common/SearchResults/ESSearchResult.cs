using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ElasticSearch.Common.SearchResults
{
    public class ESSearchResult
    {
        private HitStatus m_hits;

        public ESSearchResult(string jsonResult)
        {
            Response = jsonResult;
        }

        [JsonIgnore]
        private bool m_isNotcalled = true;

        [JsonIgnore]
        public string Response { set; get; }


        public HitStatus GetHits()
        {
            if (m_hits == null && m_isNotcalled)
            {
                m_isNotcalled = false;
                try
                {
                    if (!string.IsNullOrEmpty(Response))
                    {
                        var temp = JsonConvert.DeserializeObject<SearchHits>(Response);
                        if (temp != null && temp.Hits != null)
                        {
                            m_hits = temp.Hits;
                        }
                    }
                }
                catch (System.Exception e)
                {

                }

            }
            if (m_hits != null) return m_hits;
            return new HitStatus();
        }

        public List<string> GetHitIds()
        {
            var temp = GetHits();
            if (temp.Hits.Count > 0)
            {
                return temp.Hits.Select(hit => hit.Id).ToList();
            }
            return new List<string>();
        }

        public int GetTotalCount()
        {
            return GetHits().Total;
        }

        public SortedList<string, Dictionary<string, object>> GetFields()
        {
            var result = new SortedList<string, Dictionary<string, object>>();
            HitStatus hitStatus = GetHits();
            if (hitStatus != null)
            {
                foreach (Hits hit in hitStatus.Hits)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (var fileItem in hit.Source)
                    {
                        if (dict.ContainsKey(fileItem.Key))
                        {
                            object value = dict[fileItem.Key];
                            value = value + "," + fileItem.Value;
                            dict[fileItem.Key] = value;
                        }
                        else
                        {
                            dict.Add(fileItem.Key, fileItem.Value);
                        }
                    }
                    if (dict.Count > 0)
                    {
                        result.Add(hit.Id, dict);
                    }
                }
            }

            return result;
        }
        internal Dictionary<string, string> _hightlights;

     



    }
}
