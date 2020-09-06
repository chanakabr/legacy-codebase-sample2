using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Configuration;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Core.Catalog;
using ApiObjects;
using KLogMonitor;
using System.Reflection;

namespace Tvinci.Data.Loaders
{
    [Serializable]
    public abstract class CatalogRequestManager
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
            m_oProvider = new TVMCatalogProvider();
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

        public string FlashVarsFileFormat { get; set; }
        public string FlashVarsSubFileFormat { get; set; }

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

        /// <summary>
        /// Gets media and epg objects according to list of Ids from search result
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="response"></param>
        /// <param name="medias"></param>
        /// <param name="epgs"></param>
        protected void GetAssets(string cacheKey, UnifiedSearchResponse response, out List<MediaObj> medias,
                                out List<ProgramObj> epgs, out List<ProgramObj> recordings)
        {
            // Insert the UnifiedSearchResponse to cache for failover support
            CacheManager.Cache.InsertFailOverResponse(m_oResponse, cacheKey);

            medias = null;
            epgs = null;
            recordings = null;
            List<long> mediaIds = response.searchResults.Where(asset => asset.AssetType == eAssetTypes.MEDIA).Select(asset => long.Parse(asset.AssetId)).ToList();
            List<long> epgIds = response.searchResults.Where(asset => asset.AssetType == eAssetTypes.EPG).Select(asset => long.Parse(asset.AssetId)).ToList(); ;
            List<long> recordigIds = response.searchResults.Where(asset => asset.AssetType == eAssetTypes.NPVR).Select(asset => long.Parse(asset.AssetId)).ToList(); ;

            GetAssetsFromCatalog(mediaIds, epgIds, recordigIds, out medias, out epgs, out recordings);
        }

        protected void GetAssetsFromCatalog(List<long> mediaIds, List<long> epgIds, List<long> recordingIds, 
                                            out List<MediaObj> medias, out List<ProgramObj> epgs, out List<ProgramObj> recordings)
        {
            medias = new List<MediaObj>();
            epgs = new List<ProgramObj>();
            recordings = new List<ProgramObj>();

            if ((mediaIds != null && mediaIds.Count > 0) || (epgIds != null && epgIds.Count > 0) || (recordingIds != null && recordingIds.Count > 0))
            {
                List<long> requestEpgIds = new List<long>();
                if (epgIds != null)
                {
                    requestEpgIds.AddRange(epgIds);
                }

                if (recordingIds != null)
                {
                    requestEpgIds.AddRange(recordingIds);
                }

                // Build AssetInfoRequest with the missing ids
                AssetInfoRequest request = new AssetInfoRequest()
                {
                    epgIds = requestEpgIds,
                    mediaIds = mediaIds,
                    m_nGroupID = GroupID,
                    m_nPageIndex = PageIndex,
                    m_nPageSize = PageSize,
                    m_oFilter = m_oFilter,
                    m_sSignature = m_sSignature,
                    m_sSignString = m_sSignString,
                    m_sSiteGuid = SiteGuid,
                    m_sUserIP = m_sUserIP
                };

                BaseResponse response = null;
                eProviderResult providerResult = m_oProvider.TryExecuteGetBaseResponse(request, out response);
                if (providerResult == eProviderResult.Success && response != null)
                {
                    AssetInfoResponse assetInfoResponse = (AssetInfoResponse)response;

                    if (assetInfoResponse.mediaList != null)
                    {
                        if (assetInfoResponse.mediaList.Any(m => m == null))
                        {
                            logger.Warn("CatalogRequestManager: Received response from Catalog with null media objects");
                        }

                        medias = assetInfoResponse.mediaList.Where(m => m != null).ToList();
                    }
                    else
                    {
                        medias = new List<MediaObj>();
                    }

                    if (assetInfoResponse.epgList != null)
                    {
                        if (assetInfoResponse.epgList.Any(m => m == null))
                        {
                            logger.Warn("CatalogRequestManager: Received response from Catalog with null EPG objects");
                        }

                        epgs = assetInfoResponse.epgList.Where(m => m != null).ToList();

                        if (recordingIds != null && recordingIds.Count > 0)
                        {
                            foreach (long recordingId in recordingIds)
                            {
                                try
                                {
                                    int index = epgs.FindIndex(x => x.AssetId == recordingId.ToString());
                                    recordings.Add(epgs[index]);
                                    epgs.RemoveAt(index);
                                }
                                catch (Exception ex)
                                {
                                    Log($"recording with id {recordingId} wasn't found. ex = {ex}", null);
                                }
                            }
                        }
                    }
                    else
                    {
                        epgs = new List<ProgramObj>();
                        recordings = new List<ProgramObj>();
                    }
                }
            }
        }
    }
}
