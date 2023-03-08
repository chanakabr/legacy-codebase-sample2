using ApiLogic.Api.Managers.Rule;
using ApiLogic.IndexManager.Helpers;
using ApiObjects.Base;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;
using static WebAPI.Exceptions.ApiException;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaChannelFilter : KalturaBaseSearchAssetFilter
    {
        /// <summary>
        /// <seealso cref="IndexManagerCommonHelpers.GroupBySearchIsSupportedForOrder(ApiObjects.SearchObjects.OrderBy)"/>
        /// </summary>
        private static readonly HashSet<KalturaAssetOrderByType> SupportedGroupByOrders = new HashSet<KalturaAssetOrderByType> {
            KalturaAssetOrderByType.CREATE_DATE_ASC,
            KalturaAssetOrderByType.CREATE_DATE_DESC,
            KalturaAssetOrderByType.START_DATE_ASC,
            KalturaAssetOrderByType.START_DATE_DESC,
            KalturaAssetOrderByType.NAME_ASC,
            KalturaAssetOrderByType.NAME_DESC
        };

        protected override int AllowedOrderingLevelsCount => getGroupByValue()?.Count > 0
            ? 1 : base.AllowedOrderingLevelsCount;

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

        public override IReadOnlyCollection<KalturaBaseAssetOrder> Orderings =>
            DynamicOrderBy == null
            && !OrderBy.HasValue
            && (OrderParameters == null || OrderParameters.Count == 0)
                ? null
                : base.Orderings;

        internal override void Validate()
        {
            base.Validate();
            var groupByList = getGroupByValue();
            if (ExcludeWatched || groupByList == null || !groupByList.Any())
            {
                return;
            }

            if (groupByList.Count > 1)
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "groupBy");
            }

            if (Orderings?.Count > 1)
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "orderingParameters");
            }

            var ordering = Orderings?.SingleOrDefault();
            var isOrderingCompatibleWithGroupBy = ordering == null
                || ordering is KalturaAssetDynamicOrder
                || ordering is KalturaAssetOrder order && SupportedGroupByOrders.Contains(order.OrderBy);
            if (!isOrderingCompatibleWithGroupBy)
            {
                throw new BadRequestException(
                    new ApiExceptionType(
                        StatusCode.EnumValueNotSupported,
                        StatusCode.BadRequest,
                        "Enumerator value [@value@] is not supported when using with groupBy",
                        "value"),
                    OrderParameters);
            }
        }

        // Returns assets that belong to a channel
        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            var domainId = (int)(contextData.DomainId ?? 0);
            var ksqlFilter = FilterAsset.Instance.UpdateKsql(Ksql, contextData.GroupId, contextData.SessionCharacteristicKey);
            var isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(
                contextData.GroupId,
                contextData.UserId.ToString(),
                ignoreDoesGroupUsesTemplates: true);
            var shouldApplyPriorityGroups = ShouldApplyPriorityGroupsEqual ?? false;

            if (!ExcludeWatched)
            {
                return ClientsManager.CatalogClient().GetChannelAssets(
                    contextData,
                    pager.GetRealPageIndex(),
                    pager.PageSize,
                    IdEqual,
                    Orderings,
                    ksqlFilter,
                    responseProfile,
                    isAllowedToViewInactiveAssets,
                    getGroupByValue(),
                    GroupingOptionEqual == KalturaGroupingOption.Include,
                    shouldApplyPriorityGroups);
            }

            ValidateForExcludeWatched(contextData, pager);

            return ClientsManager.CatalogClient().GetChannelAssetsExcludeWatched(
                contextData,
                pager.GetRealPageIndex(),
                pager.PageSize,
                IdEqual,
                Orderings,
                ksqlFilter,
                isAllowedToViewInactiveAssets,
                responseProfile,
                shouldApplyPriorityGroups);

        }
    }
}
