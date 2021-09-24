using System;
using ApiObjects.Response;
using Core.Catalog;
using Core.Catalog.CatalogManagement;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public partial class KalturaLinearAssetStructFilter : KalturaBaseAssetStructFilter
    {
        internal override GenericListResponse<AssetStruct> GetResponse(int groupId)
            => CatalogManager.Instance.GetLinearAssetStructs(groupId);
    }
}