using System;
using System.Collections.Generic;
using System.Threading;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.ObjectsConvertor.Ordering
{
    public class KalturaOrderAdapter : IKalturaOrderAdapter
    {
        private static readonly Lazy<IKalturaOrderAdapter> LazyInstance =
            new Lazy<IKalturaOrderAdapter>(() => new KalturaOrderAdapter(), LazyThreadSafetyMode.PublicationOnly);

        public static IKalturaOrderAdapter Instance => LazyInstance.Value;

        public List<KalturaBaseAssetOrder> MapToOrderingList(
            KalturaDynamicOrderBy dynamicOrderBy,
            KalturaAssetOrderBy defaultOrderBy)
            => dynamicOrderBy?.OrderBy != null && !string.IsNullOrEmpty(dynamicOrderBy.Name)
                ? new List<KalturaBaseAssetOrder>
                {
                    new KalturaAssetDynamicOrder { OrderBy = dynamicOrderBy.OrderBy.Value, Name = dynamicOrderBy.Name }
                }
                : new List<KalturaBaseAssetOrder> { MapToKalturaAssetOrder(defaultOrderBy) };

        public List<KalturaBaseAssetOrder> MapToOrderingList(
            KalturaAssetOrderBy source,
            int? trendingDaysEqual = null)
            => new List<KalturaBaseAssetOrder> { MapToKalturaAssetOrder(source, trendingDaysEqual) };

        private static KalturaBaseAssetOrder MapToKalturaAssetOrder(KalturaAssetOrderBy source,
            int? trendingDaysEqual = null)
        {
            switch (source)
            {
                case KalturaAssetOrderBy.NAME_ASC:
                    return new KalturaAssetOrder
                    {
                        OrderBy = KalturaAssetOrderByType.NAME_ASC
                    };
                case KalturaAssetOrderBy.NAME_DESC:
                    return new KalturaAssetOrder
                    {
                        OrderBy = KalturaAssetOrderByType.NAME_DESC
                    };
                case KalturaAssetOrderBy.VIEWS_DESC:
                    return new KalturaAssetStatisticsOrder
                    {
                        OrderBy = KalturaAssetOrderByStatistics.VIEWS_DESC,
                        TrendingDaysEqual = trendingDaysEqual
                    };
                case KalturaAssetOrderBy.RATINGS_DESC:
                    return new KalturaAssetOrder
                    {
                        OrderBy = KalturaAssetOrderByType.RATINGS_DESC
                    };
                case KalturaAssetOrderBy.VOTES_DESC:
                    return new KalturaAssetOrder
                    {
                        OrderBy = KalturaAssetOrderByType.VOTES_DESC
                    };
                case KalturaAssetOrderBy.START_DATE_DESC:
                    return new KalturaAssetOrder
                    {
                        OrderBy = KalturaAssetOrderByType.START_DATE_DESC
                    };
                case KalturaAssetOrderBy.START_DATE_ASC:
                    return new KalturaAssetOrder
                    {
                        OrderBy = KalturaAssetOrderByType.START_DATE_ASC
                    };
                case KalturaAssetOrderBy.LIKES_DESC:
                    return new KalturaAssetOrder
                    {
                        OrderBy = KalturaAssetOrderByType.LIKES_DESC
                    };
                case KalturaAssetOrderBy.CREATE_DATE_ASC:
                    return new KalturaAssetOrder
                    {
                        OrderBy = KalturaAssetOrderByType.CREATE_DATE_ASC
                    };
                case KalturaAssetOrderBy.CREATE_DATE_DESC:
                    return new KalturaAssetOrder
                    {
                        OrderBy = KalturaAssetOrderByType.CREATE_DATE_DESC
                    };
                default:
                    return new KalturaAssetOrder
                    {
                        OrderBy = KalturaAssetOrderByType.RELEVANCY_DESC
                    };
            }
        }
    }
}