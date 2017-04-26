using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using AdapterControllers;
using Core.Catalog.Response;
using GroupsCacheManager;
using KLogMonitor;
using System.Reflection;
using ApiObjects;
using ApiObjects.SearchObjects;
using ApiObjects.Response;

namespace Core.Catalog.Request
{
    /**************************************************************************************
   * return : Return Search medias from external recommendation engine
   * *************************************************************************************/
    [DataContract]
    public class MediaSearchExternalRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public string m_sQuery;
        [DataMember]
        public string m_sDeviceID;
        [DataMember]
        public Int32 m_nUtcOffset;
        [DataMember]
        public List<Int32> m_nMediaTypes;
        [DataMember]
        public string m_sLanguage = null;

        public MediaSearchExternalRequest()
            : base()
        {
            m_nMediaTypes = new List<Int32>();
        }

        public MediaSearchExternalRequest(Int32 nGroupID, string sSiteGuid, string query, Filter filter, string sUserIP, Int32 utcOffset, string language,
                                           string sSignature, string sSignString, List<Int32> filterTypeIDs = null, Int32 nPageSize = 5, Int32 nPageIndex = 0)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, filter, sSignature, sSignString)
        {
            m_sQuery = query;
            m_sSiteGuid = sSiteGuid;
            m_nUtcOffset = utcOffset;            
            m_sDeviceID = filter.m_sDeviceId;
            m_nMediaTypes = filterTypeIDs;
            m_nMediaTypes = filterTypeIDs;
        }

        public MediaSearchExternalRequest(MediaSearchExternalRequest m)
            : base(m.m_nPageSize, m.m_nPageIndex, m.m_sUserIP, m.m_nGroupID, m.m_oFilter, m.m_sSignature, m.m_sSignString)
        {
            m_sQuery = m.m_sQuery;
            m_sSiteGuid = m.m_sSiteGuid;
            m_nUtcOffset = m.m_nUtcOffset;
            m_sDeviceID = m.m_sDeviceID;
            m_nMediaTypes = m.m_nMediaTypes;
            m_nMediaTypes = m.m_nMediaTypes;
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            MediaSearchExternalRequest mediaSearchRequest = oBaseRequest as MediaSearchExternalRequest;
            MediaIdsStatusResponse mediaResponse = new MediaIdsStatusResponse();

            try
            {
                if (this.m_oFilter != null)
                {
                    this.m_sDeviceID = this.m_oFilter.m_sDeviceId;
                }

                CheckSignature(mediaSearchRequest);

                int totalItems;
                List<RecommendationResult> results;

                Status status = CatalogLogic.GetExternalSearchAssets(mediaSearchRequest, out totalItems, out results, out mediaResponse.RequestId);
                if (status.Code != 0)
                {
                    mediaResponse.Status = status;
                    return mediaResponse;
                }

                ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

                var allRecommendations = results.Select(result =>
                    new UnifiedSearchResult()
                    {
                        AssetId = result.id,
                        AssetType = (eAssetTypes)result.type,
                        m_dUpdateDate = DateTime.MinValue
                    }
                    ).ToList();

                List<UnifiedSearchResult> searchResultsList = new List<UnifiedSearchResult>();

                if (allRecommendations != null && allRecommendations.Count > 0)
                    searchResultsList =
                        searcher.FillUpdateDates(mediaSearchRequest.m_nGroupID, allRecommendations, ref totalItems, mediaSearchRequest.m_nPageSize, mediaSearchRequest.m_nPageIndex);

                mediaResponse.m_nTotalItems = totalItems;
                mediaResponse.m_nMediaIds = searchResultsList.Select(result => new SearchResult() { assetID = int.Parse(result.AssetId), UpdateDate = result.m_dUpdateDate }).ToList();

                return mediaResponse;
            }
            catch (Exception ex)
            {
                log.Error("GetMediaSearchExternal", ex);
                throw;
            }
        }
    }
}
