using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Catalog
{
    [DataContract]
    public class TagMeta
    {
        [DataMember]
        public string m_sName;
        [DataMember]
        public string m_sType;
        public TagMeta()
        {
        }
        public TagMeta(string sName, string sType)
        {
            m_sName = sName;
            m_sType = sType;
        }
    }
    [DataContract]
    public class Metas
    {
        [DataMember]
        public TagMeta m_oTagMeta;
        [DataMember]
        public string m_sValue;

        public Metas()
        {
        }
    }
    [DataContract]
    public class Tags
    {
        [DataMember]
        public TagMeta m_oTagMeta;
        [DataMember]
        public List<string> m_lValues;

        public Tags()
        {
        }
    }

    [DataContract]
    public class KeyValue
    {
        [DataMember]
        public string m_sKey;
        [DataMember]
        public string m_sValue;

        public KeyValue()
        {
        }
        public KeyValue(string sKey, string sValue)
        {
            m_sKey = sKey;
            m_sValue = sValue;
        }
    }
}
