using RestfulTVPApi.Catalog;
using RestfulTVPApi.Clients.Utils;
using RestfulTVPApi.Objects.Models;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using RestfulTVPApi.Objects.Extentions;

namespace RestfulTVPApi.Clients
{
    public class CatalogClient : BaseClient
    {
        #region Variables
        private const string MEDIA_CACHE_KEY_PREFIX = "media";
        private const string EPG_CACHE_KEY_PREFIX = "epg";
        private const string CACHE_KEY_FORMAT = "{0}_lng{1}";

        private readonly ILog logger = LogManager.GetLogger(typeof(CatalogClient));
        
        public string Signature { get; set; }
        public string SignString { get; set; }
        public string SignatureKey
        {
            set
            {
                SignString = Guid.NewGuid().ToString();
                Signature = GetSignature(SignString, value);
            }
        }


        public int CacheDuration { get; set; }


        #endregion

        #region CTOR

        public CatalogClient(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
            
        }

        public CatalogClient()
        {

        }

        #endregion

        #region Properties

        protected RestfulTVPApi.Catalog.IserviceClient Catalog
        {
            get
            {
                return (Module as RestfulTVPApi.Catalog.IserviceClient);
            }
        }

        #endregion

        private string GetSignature(string signString, string signatureKey)
        {
            string retVal;
            //Get key from DB
            string hmacSecret = signatureKey;
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

        public List<AssetInfo> SearchAssets(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform,  string siteGuid, string udid, int language, int pageIndex, int pageSize, 
            string filter, OrderObj order, List<int> assetTypes, List<string> with)
        {
            List<AssetInfo> result = null;

            // build request
            UnifiedSearchRequest request = new UnifiedSearchRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oFilter = new Filter() 
                { 
                    m_sPlatform = platform.ToString(),
                    m_sDeviceId = udid,
                    m_nLanguage = language,
                },
                m_nGroupID = groupID,
                m_nPageIndex = pageIndex,
                m_nPageSize = pageSize,
                filterQuery = filter,
                order = order,
                assetTypes = assetTypes,
            };


            // build failover cahce key
            StringBuilder key = new StringBuilder();

            // g = GroupId
            // ps = PageSize
            // pi = PageIndex
            // ob = OrderBy
            // od = OrderDir
            // ov = OrderValue 
            // at = AssetTypes
            //f = filter

            key.AppendFormat("Unified_search_g={0}_ps={1}_pi={2}", groupID, pageSize, pageIndex);
            if (order != null)
            {
                key.AppendFormat("_ob={0}_od={1}", order.m_eOrderBy, order.m_eOrderDir);
                if (!string.IsNullOrEmpty(order.m_sOrderValue))
                    key.AppendFormat("_ov={0}", order.m_sOrderValue);
            }
            if (assetTypes != null && assetTypes.Count > 0)
                key.AppendFormat("_at={0}", string.Join(",", assetTypes.Select(at => at.ToString()).ToArray()));
            if (!string.IsNullOrEmpty(filter))
                key.AppendFormat("_f={0}", filter);

            result = CatalogUtils.SearchAssets(Catalog, request, key.ToString(), with);

            return result;
        }

        public List<AssetStats> GetAssetsStats(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform, string siteGuid, string udid, List<int> assetIds, long startTime, long endTime, RestfulTVPApi.Catalog.StatsType assetType)
        {
            List<AssetStats> result = null;
            AssetStatsRequest request = new AssetStatsRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupID,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_sPlatform = platform.ToString(),
                },
                m_nAssetIDs = assetIds,
                m_dStartDate = RestfulTVPApi.ServiceInterface.Utils.ConvertFromUnixTimestamp(startTime),
                m_dEndDate = RestfulTVPApi.ServiceInterface.Utils.ConvertFromUnixTimestamp(endTime),
                m_type = assetType
            };

            var response = Catalog.GetResponse(request) as AssetStatsResponse;

            if (response != null)
            {
                result = response.m_lAssetStat != null ? response.m_lAssetStat.Select(a => AssetStats.CreateFromObject(a)).ToList() : null;
            }

            return result;
        }


        public string MediaMark(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform, string siteGuid, string udid, int language, int mediaId, int mediaFileId, int location,
            string mediaCdn, string errorMessage, string errorCode, string mediaDuration, string action, int totalBitRate, int currentBitRate, int avgBitRate, string npvrId = null)
        {
            string res = null;

            MediaMarkRequest request = new MediaMarkRequest()
            {
                m_sSignature = Signature,
                m_sSignString = SignString,
                m_oMediaPlayRequestData = new MediaPlayRequestData()
                {
                    m_nAvgBitRate = avgBitRate,
                    m_nCurrentBitRate = currentBitRate,
                    m_nLoc = location,
                    m_nMediaFileID = mediaFileId,
                    m_nMediaID = mediaId,
                    m_nTotalBitRate = totalBitRate,
                    m_sAction = action,
                    m_sMediaDuration = mediaDuration,
                    m_sSiteGuid = siteGuid,
                    m_sUDID = udid,
                    m_sNpvrID = npvrId
                },
                m_sErrorCode = errorCode,
                m_sErrorMessage = errorMessage,
                m_sMediaCDN = mediaCdn,
                m_sSiteGuid = siteGuid,
                m_nGroupID = groupID,
                m_oFilter = new Filter()
                {
                    m_sDeviceId = udid,
                    m_nLanguage = language, 
                    m_sPlatform = platform.ToString(),
                }
            };

            var response = Catalog.GetResponse(request) as MediaMarkResponse;

            if (response != null)
            {
                res = response.m_sStatus;
            }

            return res;
        }
    }
}