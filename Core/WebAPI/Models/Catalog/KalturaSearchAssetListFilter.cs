using ApiObjects.Base;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

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

            var response = ClientsManager.CatalogClient().SearchAssetsExcludeWatched(contextData.GroupId, userId, domainId, contextData.Udid, contextData.Language, pager.getPageIndex(), pager.PageSize, 
                this.Ksql, this.OrderBy, this.getTypeIn(), this.getEpgChannelIdIn(), contextData.ManagementData, this.DynamicOrderBy, this.TrendingDaysEqual);

            return response;
        }
    }
}