using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Pricing usage module
    /// </summary>
    [OldStandard("maxViewsNumber", "max_views_number")]
    [OldStandard("viewLifeCycle", "view_life_cycle")]
    [OldStandard("fullLifeCycle", "full_life_cycle")]
    [OldStandard("couponId", "coupon_id")]
    [OldStandard("waiverPeriod", "waiver_period")]
    [OldStandard("isWaiverEnabled", "is_waiver_enabled")]
    [OldStandard("isOfflinePlayback", "is_offline_playback")]
    public class KalturaUsageModule : KalturaOTTObject
    {
        /// <summary>
        /// Usage module identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public long? Id { get; set; }

        /// <summary>
        /// Usage module name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The maximum number of times an item in this usage module can be viewed
        /// </summary>
        [DataMember(Name = "maxViewsNumber")]
        [JsonProperty("maxViewsNumber")]
        [XmlElement(ElementName = "maxViewsNumber")]
        public int? MaxViewsNumber { get; set; }

        /// <summary>
        /// The amount time an item is available for viewing since a user started watching the item
        /// </summary>
        [DataMember(Name = "viewLifeCycle")]
        [JsonProperty("viewLifeCycle")]
        [XmlElement(ElementName = "viewLifeCycle")]
        public int? ViewLifeCycle { get; set; }

        /// <summary>
        /// The amount time an item is available for viewing
        /// </summary>
        [DataMember(Name = "fullLifeCycle")]
        [JsonProperty("fullLifeCycle")]
        [XmlElement(ElementName = "fullLifeCycle")]
        public int? FullLifeCycle { get; set; }

        /// <summary>
        /// Identifies a specific coupon linked to this object
        /// </summary>
        [DataMember(Name = "couponId")]
        [JsonProperty("couponId")]
        [XmlElement(ElementName = "couponId")]
        public int? CouponId { get; set; }
                
        /// <summary>
        /// Time period during which the end user can waive his rights to cancel a purchase. When the time period is passed, the purchase can no longer be cancelled
        /// </summary>
        [DataMember(Name = "waiverPeriod")]
        [JsonProperty("waiverPeriod")]
        [XmlElement(ElementName = "waiverPeriod")]
        public int? WaiverPeriod { get; set; }

        /// <summary>
        /// Indicates whether or not the end user has the right to waive his rights to cancel a purchase
        /// </summary>
        [DataMember(Name = "isWaiverEnabled")]
        [JsonProperty("isWaiverEnabled")]
        [XmlElement(ElementName = "isWaiverEnabled")]
        public bool? IsWaiverEnabled { get; set; }

        /// <summary>
        /// Indicates that usage is targeted for offline playback
        /// </summary>
        [DataMember(Name = "isOfflinePlayback")]
        [JsonProperty("isOfflinePlayback")]
        [XmlElement(ElementName = "isOfflinePlayback")]
        public bool? IsOfflinePlayback { get; set; }
    }
}