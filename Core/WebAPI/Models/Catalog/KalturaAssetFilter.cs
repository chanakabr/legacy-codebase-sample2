using ApiLogic.Api.Managers.Rule;
using ApiObjects.Base;
using ApiObjects.SearchObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.InternalModels;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.ObjectsConvertor.Ordering;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaAssetFilter : KalturaPersistedFilter<KalturaAssetOrderBy>
    {
        public override KalturaAssetOrderBy GetDefaultOrderByValue() => KalturaAssetOrderBy.CREATE_DATE_DESC;

        internal virtual void Validate()
        {
            ValidateOrdering();
            ValidateSecondaryOrdering();
            ValidateOrderingsCount();
            ValidateTrending();
        }

        protected virtual int AllowedOrderingLevelsCount => 2;

        /// <summary>
        /// order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(IsNullable = true)]
        public new KalturaAssetOrderBy? OrderBy { get; set; }

        /// <summary>
        /// dynamicOrderBy - order by Meta
        /// </summary>
        [DataMember(Name = "dynamicOrderBy")]
        [JsonProperty("dynamicOrderBy")]
        [XmlElement(ElementName = "dynamicOrderBy", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(IsNullable = true)]
        public KalturaDynamicOrderBy DynamicOrderBy { get; set; }

        /// <summary>
        /// Parameters for asset list sorting.
        /// </summary>
        [DataMember(Name = "orderingParameters")]
        [JsonProperty(PropertyName = "orderingParameters")]
        [XmlElement(ElementName = "orderingParameters")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(IsNullable = true)]
        public List<KalturaBaseAssetOrder> OrderParameters { get; set; }

        /// <summary>
        /// Trending Days Equal
        /// </summary>
        [DataMember(Name = "trendingDaysEqual")]
        [JsonProperty("trendingDaysEqual")]
        [XmlElement(ElementName = "trendingDaysEqual", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinInteger = 1, MaxInteger = 366)]
        public int? TrendingDaysEqual { get; set; }

        /// <summary>
        /// Should apply priority groups filter or not.
        /// </summary>
        [DataMember(Name = "shouldApplyPriorityGroupsEqual")]
        [JsonProperty("shouldApplyPriorityGroupsEqual")]
        [XmlElement(ElementName = "shouldApplyPriorityGroupsEqual", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public bool? ShouldApplyPriorityGroupsEqual { get; set; }

        internal virtual KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            // TODO refactoring. duplicate with KalturaSearchAssetFilter
            var userId = contextData.UserId.ToString();
            var domainId = (int)(contextData.DomainId ?? 0);
            var isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, userId, true);

            var searchAssetsFilter = new SearchAssetsFilter
            {
                GroupId = contextData.GroupId,
                SiteGuid = userId,
                DomainId = domainId,
                Udid = contextData.Udid,
                Language = contextData.Language,
                PageIndex = pager.GetRealPageIndex(),
                PageSize = pager.PageSize,
                Filter = FilterAsset.Instance.UpdateKsql(null, contextData.GroupId, contextData.SessionCharacteristicKey),
                AssetTypes = null,
                EpgChannelIds = null,
                ManagementData = contextData.ManagementData,
                GroupBy = null,
                IsAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets,
                IgnoreEndDate = false,
                GroupByType = GroupingOption.Omit,
                IsPersonalListSearch = false,
                UseFinal = false,
                OrderingParameters = Orderings,
                ResponseProfile = responseProfile,
                ShouldApplyPriorityGroups = ShouldApplyPriorityGroupsEqual.GetValueOrDefault(),
                OriginalUserId = contextData.OriginalUserId
            };

            return ClientsManager.CatalogClient().SearchAssets(searchAssetsFilter);
        }

        public virtual IReadOnlyCollection<KalturaBaseAssetOrder> Orderings
        {
            get
            {
                if (OrderParameters != null)
                {
                    return OrderParameters.Any()
                        ? OrderParameters
                        : KalturaOrderAdapter.Instance.MapToOrderingList(GetDefaultOrderByValue());
                }

                if (DynamicOrderBy?.OrderBy != null)
                {
                    return KalturaOrderAdapter.Instance.MapToOrderingList(DynamicOrderBy, GetDefaultOrderByValue());
                }

                var orderByValue = OrderBy ?? GetDefaultOrderByValue();

                return KalturaOrderAdapter.Instance.MapToOrderingList(orderByValue, TrendingDaysEqual);
            }
        }

        private void ValidateOrderingsCount()
        {
            if (OrderParameters?.Count > AllowedOrderingLevelsCount)
            {
                throw new BadRequestException(
                    BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED,
                    "KalturaAssetFilter.orderingParameters",
                    OrderParameters.Count);
            }
        }

        private void ValidateOrdering()
        {
            if (OrderParameters?.Count > 0 && (OrderBy.HasValue || DynamicOrderBy != null || TrendingDaysEqual.HasValue))
            {
                throw new BadRequestException(
                    BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER,
                    "KalturaAssetFilter.orderBy",
                    "KalturaAssetFilter.orderingParameters",
                    "KalturaAssetFilter.dynamicOrderBy",
                    "KalturaAssetFilter.trendingDaysEqual");
            }
        }

        private void ValidateSecondaryOrdering()
        {
            if (OrderParameters?.Count > 1)
            {
                var duplicatedOrdering = OrderParameters
                    .OfType<KalturaAssetOrder>()
                    .Select(x => GetOrderByFieldWithoutDirection(x.OrderBy))
                    .GroupBy(x => x.orderByField)
                    .FirstOrDefault(x => x.Count() > 1);

                var duplicatedDynamicOrdering = OrderParameters
                    .OfType<KalturaAssetDynamicOrder>()
                    .GroupBy(x => x.Name.ToLower())
                    .FirstOrDefault(x => x.Count() > 1);

                var statisticsOrderings = OrderParameters
                    .OfType<KalturaAssetStatisticsOrder>()
                    .ToList();

                if (duplicatedOrdering != null || duplicatedDynamicOrdering != null || statisticsOrderings.Count > 1)
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT,
                        "KalturaAssetFilter.orderingParameters");
                }
            }
        }

        private static (KalturaAssetOrderByType orderBy, string orderByField) GetOrderByFieldWithoutDirection(
            KalturaAssetOrderByType orderBy)
        {
            string[] postfixesToTrim = { "_asc", "_desc" };
            var source = orderBy.ToString();
            var existingPostfix = postfixesToTrim
                .FirstOrDefault(x => source.EndsWith(x, StringComparison.OrdinalIgnoreCase));

            return string.IsNullOrEmpty(existingPostfix)
                ? (orderBy, source)
                : (orderBy, source.Remove(source.LastIndexOf(existingPostfix, StringComparison.OrdinalIgnoreCase)));
        }

        private void ValidateTrending()
        {
            if (OrderBy != KalturaAssetOrderBy.VIEWS_DESC && TrendingDaysEqual.HasValue)
                throw new BadRequestException(
                    BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER,
                    "KalturaAssetFilter.orderBy",
                    "KalturaAssetFilter.orderingParameters",
                    "KalturaAssetFilter.TrendingDaysEqual");
        }

        protected static void ValidateForExcludeWatched(ContextData contextData, KalturaFilterPager pager)
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
    }
}
