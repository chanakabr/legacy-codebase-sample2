using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects.SearchObjects;
using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Ordering
{
    public class KalturaOrderMapper : IKalturaOrderMapper
    {
        private static readonly Lazy<IKalturaOrderMapper> LazyInstance = new Lazy<IKalturaOrderMapper>(
            () => new KalturaOrderMapper(), LazyThreadSafetyMode.PublicationOnly);

        public static IKalturaOrderMapper Instance => LazyInstance.Value;

        public IReadOnlyCollection<AssetOrder> MapParameters(IEnumerable<KalturaBaseAssetOrder> sourceList)
            => MapParameters(sourceList, OrderBy.ID);

        public IReadOnlyCollection<AssetOrder> MapParameters(IEnumerable<KalturaBaseAssetOrder> sourceList, OrderBy defaultOrderByField)
        {
            var result = sourceList?
                .Select(MapParameters)
                .Where(orderingParameters => orderingParameters != null)
                .ToList() ?? new List<AssetOrder>();

            if (!result.Any())
            {
                result.Add(new AssetOrder { Direction = OrderDir.DESC, Field = defaultOrderByField });
            }

            return result;
        }

        private static AssetOrder MapParameters(KalturaBaseAssetOrder source)
        {
            switch (source)
            {
                case KalturaAssetDynamicOrder dynamicParameters:
                    return MapParameters(dynamicParameters.OrderBy, dynamicParameters.Name);
                case KalturaAssetStatisticsOrder orderByStatisticsParameters:
                    return MapParameters(orderByStatisticsParameters);
                case KalturaAssetOrder orderByParameters:
                    return MapParameters(orderByParameters);
                default:
                    return null;
            }
        }

        private static AssetOrderByStatistics MapParameters(KalturaAssetStatisticsOrder parameters)
            => new AssetOrderByStatistics
            {
                Field = OrderBy.VIEWS,
                Direction = OrderDir.DESC,
                TrendingAssetWindow = parameters.TrendingDaysEqual.HasValue
                    ? DateTime.UtcNow.AddDays(-parameters.TrendingDaysEqual.Value)
                    : (DateTime?)null
            };

        private static AssetOrder MapParameters(KalturaMetaTagOrderBy orderBy, string name)
        {
            var orderingValues = MapToOrderingValues(orderBy);
            if (!orderingValues.HasValue)
            {
                return null;
            }

            var (field, direction) = orderingValues.Value;
            return new AssetOrderByMeta
            {
                Field = field,
                Direction = direction,
                MetaName = name
            };
        }

        private static AssetOrder MapParameters(KalturaAssetOrder source)
        {
            var orderingValues = MapToOrderingValues(source.OrderBy);
            if (!orderingValues.HasValue)
            {
                return null;
            }

            var (field, direction) = orderingValues.Value;
            switch (field)
            {
                case OrderBy.RATING:
                case OrderBy.VOTES_COUNT:
                case OrderBy.LIKE_COUNTER:
                case OrderBy.VIEWS:
                    return new AssetOrderByStatistics
                    {
                        Field = field,
                        Direction = direction
                    };
                default:
                    return new AssetOrder
                    {
                        Field = field,
                        Direction = direction
                    };
            }
        }

        private static (OrderBy, OrderDir)? MapToOrderingValues(KalturaMetaTagOrderBy source)
        {
            switch (source)
            {
                case KalturaMetaTagOrderBy.META_ASC:
                    return (OrderBy.META, OrderDir.ASC);
                case KalturaMetaTagOrderBy.META_DESC:
                    return (OrderBy.META, OrderDir.DESC);
                default:
                    return null;
            }
        }

        private static (OrderBy, OrderDir)? MapToOrderingValues(KalturaAssetOrderByType source)
        {
            switch (source)
            {
                case KalturaAssetOrderByType.NAME_ASC:
                    return (OrderBy.NAME, OrderDir.ASC);
                case KalturaAssetOrderByType.NAME_DESC:
                    return (OrderBy.NAME, OrderDir.DESC);
                case KalturaAssetOrderByType.RATINGS_DESC:
                    return (OrderBy.RATING, OrderDir.DESC);
                case KalturaAssetOrderByType.VOTES_DESC:
                    return (OrderBy.VOTES_COUNT, OrderDir.DESC);
                case KalturaAssetOrderByType.START_DATE_DESC:
                    return (OrderBy.START_DATE, OrderDir.DESC);
                case KalturaAssetOrderByType.RELEVANCY_DESC:
                    return (OrderBy.RELATED, OrderDir.DESC);
                case KalturaAssetOrderByType.START_DATE_ASC:
                    return (OrderBy.START_DATE, OrderDir.ASC);
                case KalturaAssetOrderByType.CREATE_DATE_ASC:
                    return (OrderBy.CREATE_DATE, OrderDir.ASC);
                case KalturaAssetOrderByType.CREATE_DATE_DESC:
                    return (OrderBy.CREATE_DATE, OrderDir.DESC);
                case KalturaAssetOrderByType.LIKES_DESC:
                    return (OrderBy.LIKE_COUNTER, OrderDir.DESC);
                default:
                    return null;
            }
        }
    }
}