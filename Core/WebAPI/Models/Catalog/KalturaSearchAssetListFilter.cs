using ApiLogic.Api.Managers.Rule;
using ApiObjects.Base;
using ApiObjects.SearchObjects;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.InternalModels;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;

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

        internal override void Validate()
        {
            base.Validate();
            if (ExcludeWatched && getGroupByValue()?.Count > 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "excludeWatched", "groupBy");
            }
        }

        //SearchAssets - Unified search across – VOD: Movies, TV Series/episodes, EPG content.
        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            if (!ExcludeWatched)
            {
                return base.GetAssets(contextData, responseProfile, pager);
            }

            ValidateForExcludeWatched(contextData, pager);
            var userId = (int)contextData.UserId.Value;
            var domainId = (int)(contextData.DomainId ?? 0);
            var ksqlFilter = FilterAsset.Instance.UpdateKsql(Ksql, contextData.GroupId, contextData.SessionCharacteristicKey);

            var filter = new SearchAssetsFilter
            {
                GroupId = contextData.GroupId,
                SiteGuid = userId.ToString(),
                DomainId = domainId,
                Udid = contextData.Udid,
                Language = contextData.Language,
                PageIndex = pager.GetRealPageIndex(),
                PageSize = pager.PageSize,
                Filter = ksqlFilter,
                AssetTypes = getTypeIn(),
                EpgChannelIds = this.getEpgChannelIdIn(),
                GroupByType = GenericExtensionMethods.ConvertEnumsById<KalturaGroupingOption, GroupingOption>
                                (this.GroupingOptionEqual, GroupingOption.Omit).Value,
                OrderingParameters = Orderings,
                ShouldApplyPriorityGroups = this.ShouldApplyPriorityGroupsEqual ?? false,
                ResponseProfile = responseProfile,
                OriginalUserId = contextData.OriginalUserId
            };

            return ClientsManager.CatalogClient().SearchAssetsExcludeWatched(filter, contextData.ManagementData);
        }
    }
}
