using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using ApiObjects.Base;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using System.Collections.Generic;
using ApiLogic.Api.Managers.Rule;
using WebAPI.Managers.Models;
using static WebAPI.Exceptions.ApiException;
using Core.Catalog;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaChannelFilter : KalturaBaseSearchAssetFilter
    {
        /// <summary>
        /// <seealso cref="IndexingUtils.GroupBySearchIsSupportedForOrder(ApiObjects.SearchObjects.OrderBy)"/>
        /// </summary>
        private static readonly HashSet<KalturaAssetOrderBy> supportedGroupByOrders = new HashSet<KalturaAssetOrderBy> {
            KalturaAssetOrderBy.CREATE_DATE_ASC,
            KalturaAssetOrderBy.CREATE_DATE_DESC,
            KalturaAssetOrderBy.START_DATE_ASC,
            KalturaAssetOrderBy.START_DATE_DESC,
            KalturaAssetOrderBy.NAME_ASC,
            KalturaAssetOrderBy.NAME_DESC
        };

        private bool shouldUseChannelDefault = true;

        /// <summary>
        ///Channel Id
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual")]
        [SchemeProperty(MinInteger = 1)]
        public int IdEqual { get; set; }

        /// <summary>
        /// Exclude watched asset. 
        /// </summary>
        [DataMember(Name = "excludeWatched")]
        [JsonProperty("excludeWatched")]
        [XmlElement(ElementName = "excludeWatched", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool ExcludeWatched { get; set; }

        /// <summary>
        /// order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaAssetOrderBy OrderBy
        {
            get { return base.OrderBy; }
            set
            {
                base.OrderBy = value;
                shouldUseChannelDefault = false;
            }
        }

        public bool GetShouldUseChannelDefault()
        {
            if (DynamicOrderBy != null)
            {
                return false;
            }
            return shouldUseChannelDefault;
        }

        // Returns assets that belong to a channel
        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            KalturaAssetListResponse response = null;

            int domainId = (int)(contextData.DomainId ?? 0);
            var ksqlFilter = FilterAsset.Instance.UpdateKsql(Ksql, contextData.GroupId, contextData.SessionCharacteristicKey);
            if (this.ExcludeWatched)
            {
                if (pager.getPageIndex() > 0)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "excludeWatched", "pageIndex");
                }

                if (!contextData.UserId.HasValue || contextData.UserId.Value == 0)
                {
                    throw new BadRequestException(BadRequestException.INVALID_USER_ID, "userId");
                }
                int userId = (int)contextData.UserId.Value;

                response = ClientsManager.CatalogClient().GetChannelAssetsExcludeWatched(contextData.GroupId, userId, domainId, contextData.Udid, contextData.Language, pager.getPageIndex(),
                    pager.PageSize, this.IdEqual, this.OrderBy, ksqlFilter, this.GetShouldUseChannelDefault(), this.DynamicOrderBy, this.TrendingDaysEqual);
            }
            else
            {
                var groupByList = getGroupByValue();
                if (groupByList?.Count > 1)
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "groupBy");
                }
                if (groupByList?.Count == 1 && !shouldUseChannelDefault && !supportedGroupByOrders.Contains(OrderBy) && DynamicOrderBy?.OrderBy == null)
                {
                    var ex = new ApiExceptionType(StatusCode.EnumValueNotSupported, StatusCode.BadRequest, "Enumerator value [@value@] is not supported when using with groupBy", "value");
                    throw new BadRequestException(ex, OrderBy);
                }

                var userId = contextData.UserId.ToString();
                var allowIncludedGroupBy = GroupingOptionEqual == KalturaGroupingOption.Include;
                bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, userId, true);

                response = ClientsManager.CatalogClient().GetChannelAssets(contextData.GroupId, userId, domainId, contextData.Udid, contextData.Language, pager.getPageIndex(), 
                    pager.PageSize, this.IdEqual, this.OrderBy, ksqlFilter, this.GetShouldUseChannelDefault(), this.DynamicOrderBy, responseProfile, isAllowedToViewInactiveAssets, groupByList, allowIncludedGroupBy, this.TrendingDaysEqual);
            }

            return response;
        }
    }
}