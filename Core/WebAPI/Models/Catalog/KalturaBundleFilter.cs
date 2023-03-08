using ApiLogic.Api.Managers.Rule;
using ApiObjects.Base;
using ApiObjects.SearchObjects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.InternalModels;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;

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

        private List<int> getTypeIn() => WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(TypeIn, "KalturaBundleFilter.typeIn");

        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            var userId = contextData.UserId.ToString();
            var domainId = (int)(contextData.DomainId ?? 0);
            var isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(
                contextData.GroupId,
                userId,
                ignoreDoesGroupUsesTemplates: true);

            var filter = new SearchAssetsFilter
            {
                GroupId = contextData.GroupId,
                SiteGuid = userId,
                DomainId = domainId,
                Udid = contextData.Udid,
                Language = contextData.Language,
                PageIndex = pager.GetRealPageIndex(),
                PageSize = pager.PageSize,
                AssetTypes = getTypeIn(),
                IsAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets,
                GroupByType = GroupingOption.Omit,
                Filter = FilterAsset.Instance.UpdateKsql(null, contextData.GroupId, contextData.SessionCharacteristicKey),
                OrderingParameters = Orderings,
                OriginalUserId = contextData.OriginalUserId
            };

            return ClientsManager.CatalogClient().GetBundleAssets(filter, this.IdEqual, this.BundleTypeEqual);
        }
    }
}
