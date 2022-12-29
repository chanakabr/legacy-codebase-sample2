using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// partner configuration for commerce
    /// </summary>
    public partial class KalturaCommercePartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// configuration for bookmark event threshold (when to dispatch the event) in seconds.
        /// </summary>
        [DataMember(Name = "bookmarkEventThresholds")]
        [JsonProperty("bookmarkEventThresholds")]
        [XmlElement(ElementName = "bookmarkEventThresholds", IsNullable = true)]
        public List<KalturaBookmarkEventThreshold> BookmarkEventThresholds { get; set; }

        /// <summary>
        /// configuration for keep add-ons after subscription deletion
        /// </summary>
        [DataMember(Name = "keepSubscriptionAddOns")]
        [JsonProperty("keepSubscriptionAddOns")]
        [XmlElement(ElementName = "keepSubscriptionAddOns", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public bool? KeepSubscriptionAddOns { get; set; }

        /// <summary>
        /// configuration for asset start entitlement padding e.g. asset start time - padding still relevant for asset 
        /// </summary>
        [DataMember(Name = "programAssetEntitlementPaddingStart")]
        [JsonProperty("programAssetEntitlementPaddingStart")]
        [XmlElement(ElementName = "programAssetEntitlementPaddingStart")]
        [SchemeProperty(MinInteger = 0, MaxInteger = 7200)]
        public int? ProgramAssetEntitlementPaddingStart { get; set; }

        /// <summary>
        /// configuration for asset end entitlement padding e.g. asset end time + padding still relevant for asset
        /// </summary>
        [DataMember(Name = "programAssetEntitlementPaddingEnd")]
        [JsonProperty("programAssetEntitlementPaddingEnd")]
        [XmlElement(ElementName = "programAssetEntitlementPaddingEnd")]
        [SchemeProperty(MinInteger = 0, MaxInteger = 7200)]
        public int? ProgramAssetEntitlementPaddingEnd { get; set; }
    }
}