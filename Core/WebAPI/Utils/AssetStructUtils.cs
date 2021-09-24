using System.Collections.Generic;
using System.Linq;
using WebAPI.Models.Catalog;

namespace WebAPI.Utils
{
    public static class AssetStructUtils
    {
        public static IEnumerable<KalturaAssetStruct> GetSortedAssetStructs(
            IEnumerable<KalturaAssetStruct> source,
            KalturaAssetStructOrderBy orderBy)
        {
            switch (orderBy)
            {
                case KalturaAssetStructOrderBy.NAME_ASC:
                    return source.OrderBy(x => x.Name.ToString());
                case KalturaAssetStructOrderBy.NAME_DESC:
                    return source.OrderByDescending(x => x.Name.ToString());
                case KalturaAssetStructOrderBy.SYSTEM_NAME_ASC:
                    return source.OrderBy(x => x.SystemName);
                case KalturaAssetStructOrderBy.SYSTEM_NAME_DESC:
                    return source.OrderByDescending(x => x.SystemName);
                case KalturaAssetStructOrderBy.CREATE_DATE_ASC:
                    return source.OrderBy(x => x.CreateDate);
                case KalturaAssetStructOrderBy.CREATE_DATE_DESC:
                    return source.OrderByDescending(x => x.CreateDate);
                case KalturaAssetStructOrderBy.UPDATE_DATE_ASC:
                    return source.OrderBy(x => x.UpdateDate);
                case KalturaAssetStructOrderBy.UPDATE_DATE_DESC:
                    return source.OrderByDescending(x => x.UpdateDate);
                default:
                    return source;
            }
        }
    }
}