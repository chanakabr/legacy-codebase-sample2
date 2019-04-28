using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ApiObjects.Catalog
{
    [DataContract]
    [Serializable]
    public class TagMeta : IEquatable<TagMeta>
    {
        [DataMember]
        [JsonProperty("m_sName")]
        public string m_sName;

        [DataMember]
        [JsonProperty("m_sType")]
        public string m_sType;

        public TagMeta()
        {
        }

        public TagMeta(string sName, string sType)
        {
            m_sName = sName;
            m_sType = sType;
        }

        public bool Equals(TagMeta other)
        {
            if (other == null)
                return false;

            return m_sName.Equals(other.m_sName) &&
                m_sType.Equals(other.m_sType);
        }       
    }

    [DataContract]
    [Serializable]
    public class Metas : IEquatable<Metas>
    {
        [DataMember]
        [JsonProperty("m_oTagMeta")]
        public TagMeta m_oTagMeta;

        [DataMember]
        [JsonProperty("m_sValue")]
        public string m_sValue;

        [DataMember]
        [JsonProperty(PropertyName = "Value",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public LanguageContainer[] Value;

        public Metas()
        {
        }

        public Metas(TagMeta tagMeta, string value, IEnumerable<LanguageContainer> languageContainer = null)
        {
            m_oTagMeta = tagMeta;
            m_sValue = value;
            if (languageContainer != null)
            {
                Value = new List<LanguageContainer>(languageContainer).ToArray();
            }
        }

        public bool Equals(Metas other)
        {
            if (other == null)
                return false;

            // compare Value
            if (!m_sValue.Equals(other.m_sValue))
            {
                return false;
            }

            int valueCount = Value != null ? Value.Length : 0;
            int otherValueCount = other.Value != null ? other.Value.Length : 0;

            if (valueCount != otherValueCount)
                return false;

            if (valueCount > 0)
            {
                Dictionary<string, string> languageContainerDic = new Dictionary<string, string>();
                foreach (LanguageContainer lc in Value)
                {
                    languageContainerDic.Add(lc.LanguageCode, lc.Value);
                }

                foreach (LanguageContainer lc in other.Value)
                {
                    if (!languageContainerDic.ContainsKey(lc.LanguageCode) || !languageContainerDic[lc.LanguageCode].Equals(lc.Value))
                        return false;
                }
            }

            return true;
        }
    }

    [DataContract]
    [Serializable]
    public class Tags : IEquatable<Tags>
    {
        [DataMember]
        [JsonProperty("m_oTagMeta")]
        public TagMeta m_oTagMeta;

        [DataMember]
        [JsonProperty("m_lValues")]
        public List<string> m_lValues;

        [DataMember]
        [JsonProperty(PropertyName = "Values",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<LanguageContainer[]> Values;

        public Tags()
        {
        }

        public Tags(TagMeta tagMeta, List<string> values, IEnumerable<LanguageContainer[]> languageContainers)
        {
            m_oTagMeta = tagMeta;
            if (values != null)
            {
                m_lValues = new List<string>(values);
            }
            else
            {
                m_lValues = new List<string>();
            }

            if (languageContainers != null)
            {
                Values = new List<LanguageContainer[]>(languageContainers);
            }
            else
            {
                Values = new List<LanguageContainer[]>();
            }
        }
        
        public bool Equals(Tags other)
        {
            if (other == null)
                return false;

            int valueCount = m_lValues != null ? m_lValues.Count : 0;
            int otherValueCount = other.m_lValues != null ? other.m_lValues.Count : 0;

            if (valueCount != otherValueCount)
                return false;

            if (valueCount > 0)
            {
                for (int i = 0; i < m_lValues.Count; i++)
                {
                    if (!m_lValues[i].Equals(other.m_lValues[i]))
                        return false;
                }
            }

            return true;
        }
    }

    public class TagsComparer : IEqualityComparer<Tags>
    {
        public bool Equals(Tags t1, Tags t2)
        {
            return t1.m_oTagMeta.m_sName.Equals(t2.m_oTagMeta.m_sName) &&
                   t1.m_oTagMeta.m_sType.Equals(t1.m_oTagMeta.m_sType);
        }

        public int GetHashCode(Tags t)
        {
            return t.m_oTagMeta.GetHashCode();
        }
    }

    public class MetasComparer : IEqualityComparer<Metas>
    {
        public bool Equals(Metas m1, Metas m2)
        {
            return m1.m_oTagMeta.m_sName.Equals(m2.m_oTagMeta.m_sName) &&
                   m1.m_oTagMeta.m_sType.Equals(m1.m_oTagMeta.m_sType);
        }

        public int GetHashCode(Metas m)
        {
            return m.m_oTagMeta.GetHashCode();
        }
    }   
}