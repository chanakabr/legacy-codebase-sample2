using ApiObjects.Base;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaAssetFilter : KalturaPersistedFilter<KalturaAssetOrderBy>
    {
        public override KalturaAssetOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetOrderBy.CREATE_DATE_DESC;
        }

        internal virtual void Validate()
        {
            this.ValidateTrending();
        }

        /// <summary>
        /// dynamicOrderBy - order by Meta
        /// </summary>
        [DataMember(Name = "dynamicOrderBy")]
        [JsonProperty("dynamicOrderBy")]
        [XmlElement(ElementName = "dynamicOrderBy", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaDynamicOrderBy DynamicOrderBy { get; set; }

        /// <summary>
        /// Trending Days Equal
        /// </summary>
        [DataMember(Name = "trendingDaysEqual")]
        [JsonProperty("trendingDaysEqual")]
        [XmlElement(ElementName = "trendingDaysEqual", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinInteger = 1, MaxInteger = 366)]
        public int? TrendingDaysEqual { get; set; }

        internal virtual KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            // TODO refactoring. duplicate with KalturaSearchAssetFilter
            var userId = contextData.UserId.ToString();
            var domainId = (int)(contextData.DomainId ?? 0);
            var isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, userId, true);

            var searchAssetsFilter = new ApiLogic.Catalog.SearchAssetsFilter
            {
                GroupId = contextData.GroupId,
                SiteGuid = userId,
                DomainId = domainId,
                Udid = contextData.Udid,
                Language = contextData.Language,
                PageIndex = pager.getPageIndex(),
                PageSize = pager.PageSize,
                Filter = null,
                AssetTypes = null,
                EpgChannelIds = null,
                ManagementData = contextData.ManagementData,
                GroupBy = null,
                IsAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets,
                IgnoreEndDate = false,
                GroupByType = ApiObjects.SearchObjects.GroupingOption.Omit,
                IsPersonalListSearch = false,
                UseFinal = false,
                TrendingDays = TrendingDaysEqual
            };

            var response = ClientsManager.CatalogClient().SearchAssets(searchAssetsFilter, OrderBy, DynamicOrderBy, responseProfile);
            return response;
        }

        internal void ValidateTrending()
        {
            if (this.OrderBy != KalturaAssetOrderBy.VIEWS_DESC && this.TrendingDaysEqual.HasValue)
                throw new Exceptions.BadRequestException(Exceptions.BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER,
                    "KalturaSearchAssetFilter.orderBy", "KalturaSearchAssetFilter.TrendingDaysEqual");
        }
    }
}