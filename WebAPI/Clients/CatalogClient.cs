using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WebAPI.Clients
{
    public class CatalogClient : BaseClient
    {
        private const string MEDIA_CACHE_KEY_PREFIX = "media";
        private const string EPG_CACHE_KEY_PREFIX = "epg";
        private const string CACHE_KEY_FORMAT = "{0}_lng{1}";
        
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

        public CatalogClient()
        {

        }

        protected WebAPI.Catalog.IserviceClient Catalog
        {
            get
            {
                return (Module as WebAPI.Catalog.IserviceClient);
            }
        }


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

        //public SearchAssetsResponse SearchAssets(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform, string siteGuid, string udid, int language, int pageIndex, int pageSize, 
        //    string filter, string orderBy, List<int> assetTypes, List<string> with)
        //{
        //    SearchAssetsResponse result = new SearchAssetsResponse();

        //    // Create catalog order object
        //    OrderObj order = new OrderObj();
        //    if (!string.IsNullOrEmpty(orderBy))
        //    {
        //        switch (orderBy.ToLower())
        //        {
        //            case "a_to_z":
        //                order.m_eOrderBy = OrderBy.NAME;
        //                order.m_eOrderDir = OrderDir.ASC;
        //                break;
        //            case "z_to_a":
        //                order.m_eOrderBy = OrderBy.NAME;
        //                order.m_eOrderDir = OrderDir.DESC;
        //                break;
        //            case "views":
        //                order.m_eOrderBy = OrderBy.VIEWS;
        //                order.m_eOrderDir = OrderDir.DESC;
        //                break;
        //            case "ratings":
        //                order.m_eOrderBy = OrderBy.RATING;
        //                order.m_eOrderDir = OrderDir.DESC;
        //                break;
        //            case "votes":
        //                order.m_eOrderBy = OrderBy.VOTES_COUNT;
        //                order.m_eOrderDir = OrderDir.DESC;
        //                break;
        //            case "newest":
        //                order.m_eOrderBy = OrderBy.CREATE_DATE;
        //                order.m_eOrderDir = OrderDir.DESC;
        //                break;
        //            case "relevancy":
        //                order.m_eOrderBy = OrderBy.RELATED;
        //                order.m_eOrderDir = OrderDir.DESC;
        //                break;
        //            default:
        //                throw new Exception("Unknown orderBy value");
                        
        //        }
        //    }

        //    // build request
        //    UnifiedSearchRequest request = new UnifiedSearchRequest()
        //    {
        //        m_sSignature = Signature,
        //        m_sSignString = SignString,
        //        m_oFilter = new Filter() 
        //        { 
        //            m_sPlatform = platform.ToString(),
        //            m_sDeviceId = udid,
        //            m_nLanguage = language,
        //        },
        //        m_nGroupID = groupID,
        //        m_nPageIndex = pageIndex,
        //        m_nPageSize = pageSize,
        //        filterQuery = filter,
        //        order = order,
        //        assetTypes = assetTypes,
        //    };

        //    // build failover cahce key
        //    StringBuilder key = new StringBuilder();
        //    key.AppendFormat("Unified_search_g={0}_ps={1}_pi={2}_ob={3}_od={4}_ov={5}_f={6}", groupID, pageSize, pageIndex, order.m_eOrderBy, order.m_eOrderDir, order.m_sOrderValue, filter);
        //    if (assetTypes != null && assetTypes.Count > 0)
        //        key.AppendFormat("_at={0}", string.Join(",", assetTypes.Select(at => at.ToString()).ToArray()));

        //    result = CatalogUtils.SearchAssets(Catalog, SignString, Signature, CacheDuration, request, key.ToString(), with);

        //    return result;
        //}

        //public List<AssetStats> GetAssetsStats(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform, 
        //    string siteGuid, string udid, List<int> assetIds, long startTime, long endTime,
        //    RestfulTVPApi.Objects.RequestModels.Enums.StatsType assetType)
        //{
        //    List<AssetStats> result = null;
        //    AssetStatsRequest request = new AssetStatsRequest()
        //    {
        //        m_sSignature = Signature,
        //        m_sSignString = SignString,
        //        m_sSiteGuid = siteGuid,
        //        m_nGroupID = groupID,
        //        m_oFilter = new Filter()
        //        {
        //            m_sDeviceId = udid,
        //            m_sPlatform = platform.ToString(),
        //        },
        //        m_nAssetIDs = assetIds,
        //        m_dStartDate = RestfulTVPApi.ServiceInterface.Utils.ConvertFromUnixTimestamp(startTime),
        //        m_dEndDate = RestfulTVPApi.ServiceInterface.Utils.ConvertFromUnixTimestamp(endTime),
        //        m_type = RestfulTVPApi.Objects.RequestModels.Enums.ConvertStatsType(assetType)
        //    };

        //    var response = Catalog.GetResponse(request) as AssetStatsResponse;

        //    if (response != null)
        //    {
        //        result = response.m_lAssetStat != null ? 
        //            response.m_lAssetStat.Select(a => AssetStats.CreateFromObject(a)).ToList() : null;
        //    }
        //    else
        //    {
        //        throw new Exception("Failed to receive stats from catalog");
        //    }

        //    return result;
        //}

        //public string MediaMark(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform, string siteGuid, string udid, int language, int mediaId, int mediaFileId, int location,
        //    string mediaCdn, string errorMessage, string errorCode, string mediaDuration, string action, int totalBitRate, int currentBitRate, int avgBitRate, string npvrId = null)
        //{
        //    string res = null;

        //    MediaMarkRequest request = new MediaMarkRequest()
        //    {
        //        m_sSignature = Signature,
        //        m_sSignString = SignString,
        //        m_oMediaPlayRequestData = new MediaPlayRequestData()
        //        {
        //            m_nAvgBitRate = avgBitRate,
        //            m_nCurrentBitRate = currentBitRate,
        //            m_nLoc = location,
        //            m_nMediaFileID = mediaFileId,
        //            m_nMediaID = mediaId,
        //            m_nTotalBitRate = totalBitRate,
        //            m_sAction = action,
        //            m_sMediaDuration = mediaDuration,
        //            m_sSiteGuid = siteGuid,
        //            m_sUDID = udid,
        //            m_sNpvrID = npvrId
        //        },
        //        m_sErrorCode = errorCode,
        //        m_sErrorMessage = errorMessage,
        //        m_sMediaCDN = mediaCdn,
        //        m_sSiteGuid = siteGuid,
        //        m_nGroupID = groupID,
        //        m_oFilter = new Filter()
        //        {
        //            m_sDeviceId = udid,
        //            m_nLanguage = language, 
        //            m_sPlatform = platform.ToString(),
        //        }
        //    };

        //    var response = Catalog.GetResponse(request) as MediaMarkResponse;

        //    if (response != null)
        //    {
        //        res = response.m_sStatus;
        //    }
        //    else
        //    {
        //        throw new Exception("Failed to receive stats from catalog");
        //    }

        //    return res;
        //}
    }
}