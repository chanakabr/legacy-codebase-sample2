using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using System.Security.Cryptography;
using System.Configuration;


namespace Tvinci.Data.Loaders
{
    [Serializable]
    public abstract class CatalogRequestManager
    {
        public static string EndPointAddress;
        public static string SignatureKey;

        protected Provider m_oProvider;

        protected BaseRequest m_oRequest;
        protected BaseResponse m_oResponse;
        protected Filter m_oFilter;
        protected string m_sUserIP;
        protected string m_sSignature;
        protected string m_sSignString;

        public int GroupID { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string SiteGuid { get; set; }
        public int DomainId { get; set; }
        public int UtcOffset { get; set; }

        #region Public Properties for Filter
        public bool OnlyActiveMedia
        {
            get
            {
                return m_oFilter.m_bOnlyActiveMedia;
            }
            set
            {
                m_oFilter.m_bOnlyActiveMedia = value;
            }
        }
        public bool UseFinalDate
        {
            get
            {
                return m_oFilter.m_bUseFinalDate;

            }
            set
            {
                m_oFilter.m_bUseFinalDate = value;
            }
        }
        public bool UseStartDate
        {
            get
            {
                return m_oFilter.m_bUseStartDate;
            }
            set
            {
                m_oFilter.m_bUseStartDate = value;
            }
        }
        public int Language
        {
            get
            {
                return m_oFilter.m_nLanguage;
            }
            set
            {
                m_oFilter.m_nLanguage = value;
            }
        }
        public string DeviceId
        {
            get
            {
                return m_oFilter.m_sDeviceId;
            }
            set
            {
                m_oFilter.m_sDeviceId = value;
            }
        }
        public string Platform
        {
            get
            {
                return m_oFilter.m_sPlatform;
            }
            set
            {
                m_oFilter.m_sPlatform = value;
            }
        }
        public int UserTypeID
        {
            get
            {
                return m_oFilter.m_nUserTypeID; 
            }
            set
            {
                m_oFilter.m_nUserTypeID = value;
            }
        }
        
        #endregion
        
        #region Constructors
        //Constructors with default Provider (TVMCatalogProvider)
        public CatalogRequestManager()
        {
            m_oProvider = new TVMCatalogProvider(EndPointAddress);
            m_sSignString = Guid.NewGuid().ToString();
            m_sSignature = GetSignature(m_sSignString);
        }

        public CatalogRequestManager(int groupID, string userIP, int pageSize, int pageIndex) : this()
        {
            GroupID = groupID;
            PageSize = pageSize;
            PageIndex = pageIndex;
            m_sUserIP = userIP;

            // Default Values
            m_oFilter = new Filter()
            {
                m_bOnlyActiveMedia = true,
                m_bUseFinalDate = false,
                m_bUseStartDate = true,
                m_sDeviceId = string.Empty,
                m_sPlatform = string.Empty,
                m_nUserTypeID = 0  
            };
        }

        //Constructor for another provider
        public CatalogRequestManager(int groupID, string userIP, int pageSize, int pageIndex, Provider provider)
            : this(groupID, userIP, pageSize, pageIndex)
        {
            m_oProvider = provider;
        }

        #endregion

        protected abstract void BuildSpecificRequest();
        protected abstract void Log(string message, object obj);

        public void BuildRequest()
        {
            BuildSpecificRequest();
            m_oRequest.m_nGroupID = GroupID;
            m_oRequest.m_oFilter = m_oFilter;
            m_oRequest.m_nPageSize = PageSize;
            m_oRequest.m_nPageIndex = PageIndex;
            m_oRequest.m_sSignature = m_sSignature;
            m_oRequest.m_sSignString = m_sSignString;
            m_oRequest.m_sUserIP = m_sUserIP;
            m_oRequest.m_sSiteGuid = SiteGuid;
            m_oRequest.domainId = DomainId;
        }

        private string GetSignature(string signString)
        {
            string retVal;
            //Get key from DB
            string hmacSecret = SignatureKey;
            // The HMAC secret as configured in the skin
            // Values are always transferred using UTF-8 encoding
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            // Calculate the HMAC
            // signingString is the SignString from the request
            HMACSHA1 myhmacsha1 = new HMACSHA1(encoding.GetBytes(hmacSecret));
            retVal = System.Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(signString)));
            myhmacsha1.Clear();
            return retVal;
        }

    }
}
