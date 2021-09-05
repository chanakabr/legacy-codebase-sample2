using ApiObjects.Base;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using ApiObjects.SearchObjects;
using TVinciShared;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaSearchAssetListFilter : KalturaSearchAssetFilter
    {
        /// <summary>
        /// Exclude watched asset. 
        /// </summary>
        [DataMember(Name = "excludeWatched")]
        [JsonProperty("excludeWatched")]
        [XmlElement(ElementName = "excludeWatched", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool ExcludeWatched { get; set; }

        //SearchAssets - Unified search across – VOD: Movies, TV Series/episodes, EPG content.
        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            if (!this.ExcludeWatched)
            {
                return base.GetAssets(contextData, responseProfile, pager);
            }

            if (pager.getPageIndex() > 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "excludeWatched", "pageIndex");
            }

            if (!contextData.UserId.HasValue || contextData.UserId.Value == 0)
            {
                throw new BadRequestException(BadRequestException.INVALID_USER_ID, "userId");
            }
            int userId = (int)contextData.UserId.Value;

            var groupbys = this.getGroupByValue();
            if (groupbys != null && groupbys.Count > 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "excludeWatched", "groupBy");
            }

            int domainId = (int)(contextData.DomainId ?? 0);

            var filter = new ApiLogic.Catalog.SearchAssetsFilter
            {
                GroupId = contextData.GroupId,
                SiteGuid = userId.ToString(),
                DomainId = domainId,
                Udid = contextData.Udid,
                Language = contextData.Language,
                PageIndex = pager.getPageIndex(),
                PageSize = pager.PageSize,
                Filter = this.Ksql,
                AssetTypes = this.getTypeIn(),
                EpgChannelIds = this.getEpgChannelIdIn(),
                TrendingDays = TrendingDaysEqual,
                GroupByType = GenericExtensionMethods.ConvertEnumsById<KalturaGroupingOption, GroupingOption>
                                (this.GroupingOptionEqual, GroupingOption.Omit).Value
            };

            var response = ClientsManager.CatalogClient().SearchAssetsExcludeWatched(filter, this.OrderBy, contextData.ManagementData, this.DynamicOrderBy);

            return response;
        }
    }
}