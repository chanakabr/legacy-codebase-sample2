using ApiLogic.Api.Managers.Rule;
using ApiObjects.Base;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;

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

        public int GetMediaId => IdEqual.Value;

        internal List<int> GetTypeIn => WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(TypeIn, "KalturaRelatedFilter.typeIn");

        internal override void Validate()
        {
            base.Validate();
            if (!IdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaRelatedFilter.IdEqual");
            }

            if (ExcludeWatched && getGroupByValue()?.Count > 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "excludeWatched", "groupBy");
            }
        }

        //Return list of media assets that are related to a provided asset ID (of type VOD). 
        //Returned assets can be within multi VOD asset types or be of same type as the provided asset. 
        //Response is ordered by relevancy. On-demand, per asset enrichment is supported. Maximum number of returned assets – 20, using paging
        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            var ksqlFilter = FilterAsset.Instance.UpdateKsql(Ksql, contextData.GroupId, contextData.SessionCharacteristicKey);
            var shouldApplyPriorityGroups = this.ShouldApplyPriorityGroupsEqual ?? false;
            if (!ExcludeWatched)
            {
                return ClientsManager.CatalogClient().GetRelatedMedia(
                    contextData,
                    pager.GetRealPageIndex(),
                    pager.PageSize,
                    GetMediaId,
                    ksqlFilter,
                    GetTypeIn,
                    Orderings,
                    getGroupByValue(),
                    responseProfile,
                    shouldApplyPriorityGroups);
            }

            ValidateForExcludeWatched(contextData, pager);

            return ClientsManager.CatalogClient().GetRelatedMediaExcludeWatched(
                contextData,
                pager.GetRealPageIndex(),
                pager.PageSize,
                GetMediaId,
                ksqlFilter,
                GetTypeIn,
                Orderings,
                responseProfile,
                shouldApplyPriorityGroups);
        }
    }
}