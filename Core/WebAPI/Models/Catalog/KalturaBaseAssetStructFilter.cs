using System;
using ApiObjects.Response;
using Core.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public enum KalturaAssetStructOrderBy
    {
        NAME_ASC,
        NAME_DESC,
        SYSTEM_NAME_ASC,
        SYSTEM_NAME_DESC,
        CREATE_DATE_ASC,
        CREATE_DATE_DESC,
        UPDATE_DATE_ASC,
        UPDATE_DATE_DESC
    }

    [Serializable]
    public abstract partial class KalturaBaseAssetStructFilter : KalturaFilter<KalturaAssetStructOrderBy>
    {
        internal virtual void Validate()
        {}

        internal abstract GenericListResponse<AssetStruct> GetResponse(int groupId);

        public override KalturaAssetStructOrderBy GetDefaultOrderByValue() => KalturaAssetStructOrderBy.NAME_ASC;
    }
}