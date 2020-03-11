using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class MediaFileObject : BaseCacheObject
    {
        public MediaFileObject()
        {
            m_dDuration = 0.0;
            m_nFileID = 0;
            m_nMediaID = 0;
            m_sBilling = "";
            m_sFileFormat = "";
            m_sOrigFileFormat = "";
            m_nCdnID = 0;
            m_sCDNImplType = "";
            m_sNotifyUrl = "";
            m_sFileQuality = "";
            m_sUrl = "";
            m_sConfigData = "";
            m_nViews = 0;
            m_oMediaAdSchema = null;
        }

        public override string GetCacheKey(int nMediaFileID)
        {
            string sKey = this.GetType().ToString() + "_" + nMediaFileID.ToString();
            return sKey;
        }

        public void Initialize(double dDuration, Int32 nFileID, string sBilling, string sFileFormat, string sOrigFileFormat,
            Int32 nCdnID, string sCDNImplType, string sNotifyUrl, string sFileQuality , string sUrl, string sConfigData , Int32 nMediaID ,
            Int32 nViews, MediaAdSchema oMediaAdASchema)
        {
            m_nViews = nViews;
            m_nMediaID = nMediaID;
            m_dDuration = dDuration;
            m_nFileID = nFileID;
            m_sBilling = sBilling;
            m_sFileFormat = sFileFormat;
            m_sOrigFileFormat = sOrigFileFormat;
            m_nCdnID = nCdnID;
            m_sCDNImplType = sCDNImplType;
            m_sNotifyUrl = sNotifyUrl;
            m_sFileQuality = sFileQuality;
            m_sUrl = sUrl;
            m_sConfigData = sConfigData;
            m_oMediaAdSchema = oMediaAdASchema;
        }

        public Int32 m_nMediaID;
        public Int32 m_nViews;
        public double m_dDuration;
        public Int32 m_nFileID;
        public string m_sBilling;
        public string m_sFileFormat;
        public string m_sOrigFileFormat;
        public Int32 m_nCdnID;
        public string m_sCDNImplType;
        public string m_sNotifyUrl;
        public string m_sFileQuality;
        public string m_sUrl;
        public string m_sConfigData;
        public MediaAdSchema m_oMediaAdSchema;
    }
}
