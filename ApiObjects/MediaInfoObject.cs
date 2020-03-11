using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class MediaInfoObject : BaseCacheObject
    {
        public MediaInfoObject() 
        {
            m_sTitle = "";
            m_sTypeName = "";
            m_nTypeID = 0;
            m_sDescription = "";
            m_oTagsObjects = null;
            m_oBoolObjects = null;
            m_oDoubleObjects = null;
            m_oStrObjects = null;
        }

        public override string GetCacheKey(int nMediaID)
        {
            string sKey = this.GetType().ToString() + "_" + nMediaID.ToString();
            return sKey;
        }

        public void Initialize(MetaM2MObject[] sTagsObjects , MetaStrObject[] oStrObjects , MetaDoubleObject[] oDoubleObjects , MetaBoolObject[] oBoolObjects,
            string sTitle , string sDescription , string sTypeName , Int32 nTypeID)
        {
            m_oTagsObjects = sTagsObjects;
            m_oBoolObjects = oBoolObjects;
            m_oDoubleObjects = oDoubleObjects;
            m_oStrObjects = oStrObjects;
            m_sTitle = sTitle;
            m_sDescription = sDescription;
            m_sTypeName = sTypeName;
            m_nTypeID = nTypeID;
        }

        public string m_sTitle;
        public string m_sTypeName;
        public Int32 m_nTypeID;
        public string m_sDescription;
        public MetaM2MObject[] m_oTagsObjects;
        public MetaStrObject[] m_oStrObjects;
        public MetaDoubleObject[] m_oDoubleObjects;
        public MetaBoolObject[] m_oBoolObjects;
    }
}
