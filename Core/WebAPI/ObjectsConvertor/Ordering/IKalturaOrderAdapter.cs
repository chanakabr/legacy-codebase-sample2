using System.Collections.Generic;
using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Ordering
{
    public interface IKalturaOrderAdapter
    {
        List<KalturaBaseAssetOrder> MapToOrderingList(
            KalturaDynamicOrderBy dynamicOrderBy,
            KalturaAssetOrderBy defaultOrderBy);

        List<KalturaBaseAssetOrder> MapToOrderingList(KalturaAssetOrderBy source, int? trendingDaysEqual = null);
    }
}