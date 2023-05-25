using ApiLogic.IndexManager.Helpers;
using ApiObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;
using static WebAPI.Exceptions.ApiException;

namespace WebAPI.ModelsValidators
{
    public static class AssetFilterValidator
    {
        /// <summary>
        /// <seealso cref="IndexManagerCommonHelpers.GroupBySearchIsSupportedForOrder(ApiObjects.SearchObjects.OrderBy)"/>
        /// </summary>
        private static readonly HashSet<KalturaAssetOrderByType> SupportedGroupByOrdersForChannelFilter = new HashSet<KalturaAssetOrderByType> {
            KalturaAssetOrderByType.CREATE_DATE_ASC,
            KalturaAssetOrderByType.CREATE_DATE_DESC,
            KalturaAssetOrderByType.START_DATE_ASC,
            KalturaAssetOrderByType.START_DATE_DESC,
            KalturaAssetOrderByType.NAME_ASC,
            KalturaAssetOrderByType.NAME_DESC
        };

        public static void Validate(this KalturaAssetFilter filter)
        {
            ValidateOrdering(filter);
            ValidateSecondaryOrdering(filter);
            ValidateOrderingsCount(filter);
            ValidateTrending(filter);

            switch (filter)
            {
                case KalturaChannelFilter f: ValidateChannelFilter(f); break;
                //case KalturaSearchExternalFilter - no extra validation
                case KalturaScheduledRecordingProgramFilter f: ValidateScheduledRecordingProgramFilter(f); break;
                //case KalturaRelatedExternalFilter - no extra validation
                //case KalturaChannelExternalFilter - no extra validation
                //case KalturaBundleFilter - no extra validation
                //case KalturaPersonalListSearchFilter - no extra validation
                case KalturaSearchAssetListFilter f: ValidateSearchAssetListFilter(f); break;
                //case KalturaSearchAssetFilter - no extra validation
                case KalturaRelatedFilter f: ValidateRelatedFilter(f); break;
                //case KalturaBaseSearchAssetFilter - no extra validation
                default: break;
            }
        }

        private static void ValidateOrderingsCount(KalturaAssetFilter filter)
        {
            if (filter.OrderParameters?.Count > GetAllowedOrderingLevelsCount(filter))
            {
                throw new BadRequestException(
                    BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED,
                    "KalturaAssetFilter.orderingParameters",
                    filter.OrderParameters.Count);
            }
        }

        private static int GetAllowedOrderingLevelsCount(KalturaAssetFilter filter)
        {
            switch (filter)
            {
                case KalturaChannelFilter channel: return channel.getGroupByValue()?.Count > 0 ? 1 : 2;
                default: return 2;
            }
        }

        private static void ValidateOrdering(KalturaAssetFilter filter)
        {
            if (filter.OrderParameters?.Count > 0 && (filter.OrderBy.HasValue || filter.DynamicOrderBy != null || filter.TrendingDaysEqual.HasValue))
            {
                throw new BadRequestException(
                    BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER,
                    "KalturaAssetFilter.orderBy",
                    "KalturaAssetFilter.orderingParameters",
                    "KalturaAssetFilter.dynamicOrderBy",
                    "KalturaAssetFilter.trendingDaysEqual");
            }
        }

        private static void ValidateSecondaryOrdering(KalturaAssetFilter filter)
        {
            if (filter.OrderParameters?.Count > 1)
            {
                var duplicatedOrdering = filter.OrderParameters
                    .OfType<KalturaAssetOrder>()
                    .Select(x => GetOrderByFieldWithoutDirection(x.OrderBy))
                    .GroupBy(x => x.orderByField)
                    .FirstOrDefault(x => x.Count() > 1);

                var duplicatedDynamicOrdering = filter.OrderParameters
                    .OfType<KalturaAssetDynamicOrder>()
                    .GroupBy(x => x.Name.ToLower())
                    .FirstOrDefault(x => x.Count() > 1);

                var statisticsOrderings = filter.OrderParameters
                    .OfType<KalturaAssetStatisticsOrder>()
                    .ToList();

                if (duplicatedOrdering != null || duplicatedDynamicOrdering != null || statisticsOrderings.Count > 1)
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT,
                        "KalturaAssetFilter.orderingParameters");
                }
            }
        }

        private static (KalturaAssetOrderByType orderBy, string orderByField) GetOrderByFieldWithoutDirection(KalturaAssetOrderByType orderBy)
        {
            string[] postfixesToTrim = { "_asc", "_desc" };
            var source = orderBy.ToString();
            var existingPostfix = postfixesToTrim
                .FirstOrDefault(x => source.EndsWith(x, StringComparison.OrdinalIgnoreCase));

            return string.IsNullOrEmpty(existingPostfix)
                ? (orderBy, source)
                : (orderBy, source.Remove(source.LastIndexOf(existingPostfix, StringComparison.OrdinalIgnoreCase)));
        }

        private static void ValidateTrending(KalturaAssetFilter filter)
        {
            if (filter.OrderBy != KalturaAssetOrderBy.VIEWS_DESC && filter.TrendingDaysEqual.HasValue)
                throw new BadRequestException(
                    BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER,
                    "KalturaAssetFilter.orderBy",
                    "KalturaAssetFilter.orderingParameters",
                    "KalturaAssetFilter.TrendingDaysEqual");
        }

        public static void ValidateForExcludeWatched(ContextData contextData, KalturaFilterPager pager)
        {
            if (pager.GetRealPageIndex() > 0)
            {
                throw new BadRequestException(
                    BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER,
                    "excludeWatched",
                    "pageIndex");
            }

            if (!contextData.UserId.HasValue || contextData.UserId.Value == 0)
            {
                throw new BadRequestException(BadRequestException.INVALID_USER_ID, "userId");
            }
        }

        private static void ValidateChannelFilter(KalturaChannelFilter filter)
        {
            var groupByList = filter.getGroupByValue();
            if (filter.ExcludeWatched || groupByList == null || !groupByList.Any())
            {
                return;
            }

            if (groupByList.Count > 1)
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "groupBy");
            }

            if (filter.GetOrderings()?.Count > 1)
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "orderingParameters");
            }

            var ordering = filter.GetOrderings()?.SingleOrDefault();
            var isOrderingCompatibleWithGroupBy = ordering == null
                || ordering is KalturaAssetDynamicOrder
                || ordering is KalturaAssetOrder order && SupportedGroupByOrdersForChannelFilter.Contains(order.OrderBy);
            if (!isOrderingCompatibleWithGroupBy)
            {
                throw new BadRequestException(
                    new ApiExceptionType(
                        StatusCode.EnumValueNotSupported,
                        StatusCode.BadRequest,
                        "Enumerator value [@value@] is not supported when using with groupBy",
                        "value"),
                    filter.OrderParameters);
            }
        }

        private static void ValidateScheduledRecordingProgramFilter(KalturaScheduledRecordingProgramFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.SeriesIdsIn) && filter.RecordingTypeEqual != KalturaScheduledRecordingAssetType.series)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaScheduledRecordingProgramFilter.SeriesIdsIn", "KalturaScheduledRecordingProgramFilter.RecordingTypeEqual");
            }
        }

        private static void ValidateSearchAssetListFilter(KalturaSearchAssetListFilter filter)
        {
            if (filter.ExcludeWatched && filter.getGroupByValue()?.Count > 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "excludeWatched", "groupBy");
            }
        }

        private static void ValidateRelatedFilter(KalturaRelatedFilter filter)
        {
            if (!filter.IdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaRelatedFilter.IdEqual");
            }

            if (filter.ExcludeWatched && filter.getGroupByValue()?.Count > 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "excludeWatched", "groupBy");
            }
        }
    }
}
