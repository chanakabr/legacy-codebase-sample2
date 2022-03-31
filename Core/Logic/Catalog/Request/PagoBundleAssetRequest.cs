using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using ApiLogic.Pricing.Handlers;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Catalog.Response;
using Phx.Lib.Log;

namespace Core.Catalog.Request
{
    public class PagoBundleAssetRequest : BaseEpg, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public long PagoId { get; set; }

        [DataMember]
        public string AssetFilterKsql { get; set; }

        [DataMember]
        public List<int> AssetTypes { get; set; }

        public override BaseResponse GetResponse(BaseRequest baseRequest)
        {
            try
            {
                var request = baseRequest as PagoBundleAssetRequest;

                if (request == null)
                {
                    throw new Exception("Request object is null");
                }

                CheckSignature(request);

                var noEpgAssetType = AssetTypes?.Any() == true && !AssetTypes.Contains(0);

                if (noEpgAssetType)
                {
                    return CreateEmptyResponse();
                }

                var externalOfferId = GetPagoExternalOfferId();

                // Empty list for consistency with subscription and collection search
                if (externalOfferId == null)
                {
                    return CreateEmptyResponse();
                }

                var searchRequest = new UnifiedSearchRequest
                {
                    m_sSignature = request.m_sSignature,
                    m_sSignString = request.m_sSignString,
                    m_nGroupID = request.m_nGroupID,
                    m_oFilter = request.m_oFilter,
                    m_nPageIndex = request.m_nPageIndex,
                    m_nPageSize = request.m_nPageSize,
                    shouldIgnoreDeviceRuleID = true,
                    shouldDateSearchesApplyToAllTypes = true,
                    order = new OrderObj
                    {
                        m_eOrderBy = OrderBy.ID,
                        m_eOrderDir = OrderDir.ASC
                    },
                    filterQuery = $"(and external_offer_id = {externalOfferId} {AssetFilterKsql})",
                    isInternalSearch = true,
                    shouldIgnoreEndDate = true,
                    isAllowedToViewInactiveAssets = true,
                    assetTypes = new List<int> {UnifiedSearchDefinitions.EPG_ASSET_TYPE}
                };

                var searchResponse =  searchRequest.GetResponse(searchRequest) as UnifiedSearchResponse;

                return searchResponse;
            }
            catch (Exception ex)
            {
                log.Error("GetResponse", ex);
                throw;
            }
        }

        private string GetPagoExternalOfferId()
        {
            var pago = PagoManager.Instance.GetProgramAssetGroupOffer(m_nGroupID, PagoId, true);

            if (pago == null)
            {
                return null;
            }

            var externalOfferId = pago.ExternalOfferId;

            return string.IsNullOrEmpty(externalOfferId) ? null : externalOfferId;
        }

        private UnifiedSearchResponse CreateEmptyResponse()
        {
            return new UnifiedSearchResponse
            {
                status = new Status(eResponseStatus.OK),
                searchResults = new List<UnifiedSearchResult>()
            };
        }
    }
}