using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Core.Catalog
{
    [DataContract]
    public class FileMedia
    {
        [DataMember]
        public Int32 m_nFileId;
        [DataMember]
        public double m_nDuration;
        [DataMember]
        public string m_sFileFormat;
        [DataMember]
        public string m_sUrl;
        [DataMember]
        public string m_sBillingType;
        [DataMember]
        public int m_nCdnID;
        [DataMember]
        public AdProvider m_oPreProvider;
        [DataMember]
        public AdProvider m_oBreakProvider;
        [DataMember]
        public AdProvider m_oOverlayProvider;
        [DataMember]
        public AdProvider m_oPostProvider;
        [DataMember]
        public string m_sBreakpoints;
        [DataMember]
        public string m_sOverlaypoints;
        [DataMember]
        public bool m_bIsPreSkipEnabled;
        [DataMember]
        public bool m_bIsPostSkipEnabled;
        [DataMember]
        public string m_sCoGUID;
        [DataMember]
        public string m_sLanguage;
        [DataMember]
        public int m_nIsDefaultLanguage;
        [DataMember]
        public string m_sAltUrl;
        [DataMember]
        public int m_nAltCdnID;
        [DataMember]
        public string m_sAltCoGUID;
        [DataMember]
        public int m_nMediaID;

        public FileMedia()
        {
            m_nFileId = 0;
            m_nDuration = 0;
            m_sFileFormat = string.Empty;
            m_sUrl = string.Empty;
            m_sBillingType = string.Empty;
            m_nCdnID = 0;
            m_sCoGUID = string.Empty;
            m_sAltCoGUID = string.Empty;
            m_sAltUrl = string.Empty;
            m_nAltCdnID = 0;
            initializeAdvertisingMembers();
           
        }

        public FileMedia(Int32 nFileId, double nDuration, string sFormatFile, string sUrl, string sBillingType, int nCdnID, string sCoGUID)
        {
            m_nFileId = nFileId;
            m_nDuration = nDuration;
            m_sFileFormat = sFormatFile;
            m_sUrl = sUrl;
            m_sBillingType = sBillingType;
            m_nCdnID = nCdnID;
            m_sCoGUID = sCoGUID;
            initializeAdvertisingMembers();
           
        }

        public FileMedia(Int32 nFileId, double nDuration, string sFormatFile, string sUrl, string sBillingType, int nCdnID, 
            AdProvider preProv, AdProvider breakProv, AdProvider overlayProv, AdProvider postProv, string breakpoints, string overlaypoints, 
            bool isPreSkipEnabled, bool isPostSkipEnabled)
        {
            m_nFileId = nFileId;
            m_nDuration = nDuration;
            m_sFileFormat = sFormatFile;
            m_sUrl = sUrl;
            m_sBillingType = sBillingType;
            m_nCdnID = nCdnID;
            m_oPreProvider = preProv;
            m_oBreakProvider = breakProv;
            m_oOverlayProvider = overlayProv;
            m_oPostProvider = postProv;
            m_sBreakpoints = breakpoints;
            m_sOverlaypoints = overlaypoints;
            m_bIsPreSkipEnabled = isPreSkipEnabled;
            m_bIsPostSkipEnabled = isPostSkipEnabled;

        }

        private void initializeAdvertisingMembers()
        {
            m_sBreakpoints = string.Empty;
            m_sOverlaypoints = string.Empty;
        }
    }
}
