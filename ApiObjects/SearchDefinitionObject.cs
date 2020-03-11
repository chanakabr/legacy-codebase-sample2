using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public enum AndOr
    {
        And = 0,
        Or = 1
    }

    public class SearchDefinitionObject : BaseCacheObject
    {
        public SearchDefinitionObject() 
        {
            m_sTitle = "";
            m_sTypeName = "";
            m_sDescription = "";
            m_oTagObjects = null;
            m_oMetaBoolObjects = null;
            m_oMetaDoubleObjects = null;
            m_oMetaStrObjects = null;
            m_sOrderByObjects = null;
            m_dMinDate = new DateTime(2099, 1, 1);
            m_dMaxDate = new DateTime(2099, 1, 1);
            m_eAndOr = AndOr.And;
            m_bExact = true;
            m_oPageDefinition = null;
        }

        public override string GetCacheKey(int nMediaID)
        {
            string sKey = this.GetType().ToString() + "_" + nMediaID.ToString();
            return sKey;
        }

        public void Initialize(MetaM2MObject[] oTagsObjects ,MetaStrObject[] oMetaStrObjects, MetaDoubleObject[] oMetaDoubleObjects, MetaBoolObject[] oMetaBoolObjects, string sTitle , string sDescription , string sTypeName , 
            DateTime dMin , DateTime dMax , AndOr andOrOr , bool bExact , SearchOrderByObject[] sOrderByObjects ,
            PageDefinition oPageDefinition)
        {
            m_oPageDefinition = oPageDefinition;
            m_oTagObjects = oTagsObjects;
            m_oMetaStrObjects = oMetaStrObjects;
            m_oMetaDoubleObjects = oMetaDoubleObjects;
            m_oMetaBoolObjects = oMetaBoolObjects;
            m_sTitle = sTitle;
            m_sDescription = sDescription;
            m_sTypeName = sTypeName;
            m_dMinDate = dMin;
            m_dMaxDate = dMax;
            m_eAndOr = andOrOr;
            m_bExact = bExact;
            m_sOrderByObjects = sOrderByObjects;
        }

        public string m_sTitle;
        public string m_sDescription;
        public string m_sTypeName;
        public DateTime m_dMinDate;
        public DateTime m_dMaxDate;
        public MetaM2MObject[] m_oTagObjects;
        public MetaStrObject[] m_oMetaStrObjects;
        public MetaDoubleObject[] m_oMetaDoubleObjects;
        public MetaBoolObject[] m_oMetaBoolObjects;
        public SearchOrderByObject[] m_sOrderByObjects;
        public AndOr m_eAndOr;
        public bool m_bExact;
        public PageDefinition m_oPageDefinition;
    }
}
