using ApiObjects.Base;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ApiLogic.Api.Managers.Rule;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaRelatedFilter : KalturaBaseSearchAssetFilter
    {
        /// <summary>
        /// the ID of the asset for which to return related assets
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual", IsNullable = true)]
        [SchemeProperty(MinInteger = 1)]
        public int? IdEqual { get; set; }

        /// <summary>
        /// Comma separated list of asset types to search within. 
        /// Possible values: any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – same type as the provided asset.
        /// </summary>
        [DataMember(Name = "typeIn")]
        [JsonProperty("typeIn")]
        [XmlElement(ElementName = "typeIn", IsNullable = true)]
        public string TypeIn { get; set; }

        /// <summary>
        /// Exclude watched asset. 
        /// </summary>
        [DataMember(Name = "excludeWatched")]
        [JsonProperty("excludeWatched")]
        [XmlElement(ElementName = "excludeWatched", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool ExcludeWatched { get; set; }

        public int getMediaId()
        {
            return IdEqual.Value;
        }

        internal List<int> getTypeIn()
        {
            return this.GetItemsIn<List<int>, int>(TypeIn, "KalturaRelatedFilter.typeIn");
        }

        internal override void Validate()
        {
            if (!IdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaRelatedFilter.IdEqual");
            }
        }

        //Return list of media assets that are related to a provided asset ID (of type VOD). 
        //Returned assets can be within multi VOD asset types or be of same type as the provided asset. 
        //Response is ordered by relevancy. On-demand, per asset enrichment is supported. Maximum number of returned assets – 20, using paging
        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            KalturaAssetListResponse response = null;
            int domainId = (int)(contextData.DomainId ?? 0);
            var ksqlFilter = FilterAsset.Instance.UpdateKsql(Ksql, contextData.GroupId, contextData.SessionCharacteristicKey);
            var shouldApplyPriorityGroups = this.ShouldApplyPriorityGroupsEqual ?? false;
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

                var groupbys = this.getGroupByValue();
                if (groupbys != null && groupbys.Count > 0)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "excludeWatched", "groupBy");
                }

                response = ClientsManager.CatalogClient().GetRelatedMediaExcludeWatched(contextData.GroupId, userId, domainId, contextData.Udid,
                    contextData.Language, pager.getPageIndex(), pager.PageSize, this.getMediaId(), ksqlFilter, this.getTypeIn(),
                    this.OrderBy, this.DynamicOrderBy, this.TrendingDaysEqual, responseProfile, shouldApplyPriorityGroups);
            }
            else
            {
                response = ClientsManager.CatalogClient().GetRelatedMedia(contextData.GroupId, contextData.UserId.ToString(), domainId, contextData.Udid,
                    contextData.Language, pager.getPageIndex(), pager.PageSize, this.getMediaId(), ksqlFilter, this.getTypeIn(),
                    this.OrderBy, this.DynamicOrderBy, this.getGroupByValue(), responseProfile, this.TrendingDaysEqual, shouldApplyPriorityGroups);
            }

            return response;
        }
    }
}
