using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using ApiObjects.Catalog;

namespace Core.Catalog
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

    [DataContract]
    public class BundleKeyValue
    {
        [DataMember]
        public int m_nBundleCode;
        [DataMember]
        public eBundleType m_eBundleType;

        public BundleKeyValue()
        {

        }

        public BundleKeyValue(int nBundleCode, eBundleType eBundleType)
        {
            this.m_nBundleCode = nBundleCode;
            this.m_eBundleType = eBundleType;
        }
    }

    [DataContract]
    public class BundleTriple
    {
        [DataMember]
        public int m_nBundleCode;
        [DataMember]
        public eBundleType m_eBundleType;
        [DataMember]
        public bool m_bIsContained;

        public BundleTriple(int nBundleCode, eBundleType eBundleType, bool bIsContained)
        {
            this.m_nBundleCode = nBundleCode;
            this.m_eBundleType = eBundleType;
            this.m_bIsContained = bIsContained;
        }
    }
}
