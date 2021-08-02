using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using ApiObjects.Base;
using WebAPI.ClientManagers.Client;

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

        internal HashSet<int> GetPartnerListTypeIn()
        {
            return this.GetItemsIn<HashSet<int>, int>(PartnerListTypeIn, "KalturaPersonalListSearchFilter.PartnerListTypeIn", false, true);
        }

        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            int domainId = (int)(contextData.DomainId ?? 0);

            var response = ClientsManager.CatalogClient().GetPersonalListAssets(contextData.GroupId, contextData.UserId.ToString(), domainId, contextData.Udid, contextData.Language, this.Ksql, 
                this.OrderBy, this.DynamicOrderBy, this.getGroupByValue(), pager.getPageIndex(), pager.getPageSize(), this.GetPartnerListTypeIn(), responseProfile, this.TrendingDaysEqual);

            return response;
        }
    }
}