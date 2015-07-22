using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Pricing usage module
    /// </summary>
    public class KalturaUsageModule
    {
        /// <summary>
        /// Usage module identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// Usage module name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The maximum number of times an item in this usage module can be viewed
        /// </summary>
        [DataMember(Name = "max_views_number")]
        [JsonProperty("max_views_number")]
        public int MaxViewsNumber { get; set; }

        /// <summary>
        /// The amount time an item is available for viewing since a user started watching the item
        /// </summary>
        [DataMember(Name = "view_life_cycle")]
        [JsonProperty("view_life_cycle")]
        public int ViewLifeCycle { get; set; }

        /// <summary>
        /// The amount time an item is available for viewing
        /// </summary>
        [DataMember(Name = "full_life_cycle")]
        [JsonProperty("full_life_cycle")]
        public int FullLifeCycle { get; set; }

        /// <summary>
        /// Identifies a specific coupon linked to this object
        /// </summary>
        [DataMember(Name = "coupon_id")]
        [JsonProperty("coupon_id")]
        public int CouponId { get; set; }

        /// <summary>
        /// Denotes whether this object is available only as part of a subscription or can be sold separately
        /// </summary>
        [DataMember(Name = "is_subscription_only")]
        [JsonProperty("is_subscription_only")]
        public bool IsSubscriptionOnly { get; set; }
                
        /// <summary>
        /// Time period during which the end user can waive his rights to cancel a purchase. When the time period is passed, the purchase can no longer be cancelled
        /// </summary>
        [DataMember(Name = "waiver_period")]
        [JsonProperty("waiver_period")]
        public int WaiverPeriod { get; set; }

        /// <summary>
        /// Indicates whether or not the end user has the right to waive his rights to cancel a purchase
        /// </summary>
        [DataMember(Name = "is_waiver_enabled")]
        [JsonProperty("is_waiver_enabled")]
        public bool IsWaiverEnabled { get; set; }

        /// <summary>
        /// Indicates that usage is targeted for offline playback
        /// </summary>
        [DataMember(Name = "is_offline_playback")]
        [JsonProperty("is_offline_playback")]
        public bool IsOfflinePlayback { get; set; }


    }
}