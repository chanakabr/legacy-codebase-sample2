using ApiLogic.Api.Managers.Rule;
using ApiObjects.Base;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaPersonalListSearchFilter : KalturaBaseSearchAssetFilter
    {
        /// <summary>
        /// Comma separated list of partner list types to search within. 
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "partnerListTypeIn")]
        [JsonProperty("partnerListTypeIn")]
        [SchemeProperty(DynamicMinInt = 0)]
        [XmlElement(ElementName = "partnerListTypeIn", IsNullable = true)]
        public string PartnerListTypeIn { get; set; }

        private HashSet<int> GetPartnerListTypeIn()
            => WebAPI.Utils.Utils.ParseCommaSeparatedValues<HashSet<int>, int>(PartnerListTypeIn, "KalturaPersonalListSearchFilter.PartnerListTypeIn", false, true);

        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            var ksqlFilter = FilterAsset.Instance.UpdateKsql(Ksql, contextData.GroupId, contextData.SessionCharacteristicKey);

            return ClientsManager.CatalogClient().GetPersonalListAssets(
                contextData,
                ksqlFilter, 
                Orderings,
                getGroupByValue(),
                pager.GetRealPageIndex(),
                pager.PageSize.Value,
                GetPartnerListTypeIn(),
                responseProfile,
                ShouldApplyPriorityGroupsEqual ?? false);
        }
    }
}
