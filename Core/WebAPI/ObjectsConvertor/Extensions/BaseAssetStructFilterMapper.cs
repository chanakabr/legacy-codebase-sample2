using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using AutoMapper;
using Core.Catalog.CatalogManagement;
using System;
using System.Collections.Generic;
using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class BaseAssetStructFilterMapper
    {
        internal static GenericListResponse<AssetStruct> GetResponse(this KalturaBaseAssetStructFilter model, int groupId)
        {
            switch (model)
            {
                case KalturaLinearAssetStructFilter c: return c.GetResponse(groupId);
                case KalturaAssetStructFilter c: return c.GetResponse(groupId);
                default: throw new NotImplementedException($"Validate for {model.objectType} is not implemented");
            }
        }
    }

    public static class LinearAssetStructFilterMapper
    {
        internal static GenericListResponse<AssetStruct> GetResponse(this KalturaLinearAssetStructFilter model, int groupId)
            => CatalogManager.Instance.GetLinearAssetStructs(groupId);
    }

    public static class AssetStructFilterMapper
    {
        internal static GenericListResponse<ApiObjects.Catalog.AssetStruct> GetResponse(this KalturaAssetStructFilter model, int groupId)
        {
            if (model.MetaIdEqual > 0)
            {
                return CatalogManager.Instance.GetAssetStructsByTopicId(groupId, model.MetaIdEqual.Value, model.IsProtectedEqual);
            }

            if (model.ObjectVirtualAssetInfoTypeEqual.HasValue)
            {
                var virtualEntityType = Mapper.Map<ObjectVirtualAssetInfoType>(model.ObjectVirtualAssetInfoTypeEqual);

                return CatalogManager.Instance.GetAssetStructByVirtualEntityType(groupId, virtualEntityType);
            }

            return CatalogManager.Instance.GetAssetStructsByIds(groupId, model.GetAssetStructIds(), model.IsProtectedEqual);
        }

        internal static List<long> GetAssetStructIds(this KalturaAssetStructFilter model)
            => WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(model.IdIn, "KalturaAssetStructFilter.idIn", checkDuplicate: true, ignoreDefaultValueValidation: true);
    }
}
