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
    public partial class KalturaBundleFilter : KalturaAssetFilter
    {
        /// <summary>
        ///Bundle Id. 
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual")]
        [SchemeProperty(MinInteger = 1)]
        public int IdEqual { get; set; }

        /// <summary>
        /// Comma separated list of asset types to search within. 
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "typeIn")]
        [JsonProperty("typeIn")]
        [XmlElement(ElementName = "typeIn", IsNullable = true)]
        public string TypeIn { get; set; }            
       
        /// <summary>
        /// bundleType - possible values: Subscription or Collection
        /// </summary>
        [DataMember(Name = "bundleTypeEqual")]
        [JsonProperty("bundleTypeEqual")]
        [XmlElement(ElementName = "bundleTypeEqual")]
        public KalturaBundleType BundleTypeEqual { get; set; }

        internal List<int> getTypeIn()
        {
            return this.GetItemsIn<List<int>, int>(TypeIn, "KalturaBundleFilter.typeIn");
        }

        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            var userId = contextData.UserId.ToString();
            int domainId = (int)(contextData.DomainId ?? 0);
            bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, userId, true);

            var response = ClientsManager.CatalogClient().GetBundleAssets(contextData.GroupId, userId, domainId, contextData.Udid, contextData.Language,
               pager.getPageIndex(), pager.PageSize, this.IdEqual, this.OrderBy, this.getTypeIn(), this.BundleTypeEqual,
               isAllowedToViewInactiveAssets, this.DynamicOrderBy);
            
            return response;
        }
    }
}