using System;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{

    [Serializable]
    public abstract partial class KalturaBaseAssetStructFilter : KalturaFilter<KalturaAssetStructOrderBy>
    {
        public override KalturaAssetStructOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetStructOrderBy.NAME_ASC;
        }
    }
}