using System.Collections.Generic;
using ApiObjects.SearchObjects;
using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Ordering
{
    public interface IKalturaOrderMapper
    {
        IReadOnlyCollection<AssetOrder> MapParameters(IEnumerable<KalturaBaseAssetOrder> sourceList);
        IReadOnlyCollection<AssetOrder> MapParameters(IEnumerable<KalturaBaseAssetOrder> sourceList, OrderBy defaultOrderByField);
    }
}