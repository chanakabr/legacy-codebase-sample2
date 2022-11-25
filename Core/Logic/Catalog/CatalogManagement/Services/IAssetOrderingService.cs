using System.Collections.Generic;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ApiObjects.SearchObjects.GroupRepresentatives;
using Core.Catalog.Request;
using GroupsCacheManager;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface IAssetOrderingService
    {
        ChannelEsOrderingResult MapToChannelEsOrderByFields(
            InternalChannelRequest request,
            Channel channel,
            AssetListEsOrderingCommonInput input);

        AssetListEsOrderingResult MapToEsOrderByFields(
            OrderObj order,
            IReadOnlyCollection<AssetOrder> orderings,
            AssetListEsOrderingCommonInput input);

        AssetListEsOrderingResult MapToEsOrderByFields(
            OrderObj order,
            AssetListEsOrderingCommonInput input);

        AssetListEsOrderingResult MapToEsOrderByFields(
            MediaRelatedRequest request,
            AssetListEsOrderingCommonInput input);

        GenericResponse<IEsOrderByField> MapToEsOrderByField(GroupRepresentativesRequest request, CatalogClientData clientData);
    }
}