using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Coupons group details
    /// </summary>
    public class KalturaCouponsGroup : KalturaOTTObject
    {
        /// <summary>
        /// Coupon group identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Coupon group name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }


        /// <summary>
        /// A list of the descriptions of the coupon group on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "descriptions")]
        [JsonProperty("descriptions")]
        [XmlArray(ElementName = "descriptions")]
        [XmlArrayItem("item")] 
        public List<KalturaTranslationContainer> Descriptions { get; set; }

        /// <summary>
        /// The first date the coupons in this coupons group are valid
        /// </summary>
        [DataMember(Name = "start_date")]
        [JsonProperty("start_date")]
        [XmlElement(ElementName = "start_date")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The last date the coupons in this coupons group are valid
        /// </summary>
        [DataMember(Name = "end_date")]
        [JsonProperty("end_date")]
        [XmlElement(ElementName = "end_date")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Maximum number of uses for each coupon in the group
        /// </summary>
        [DataMember(Name = "max_uses_number")]
        [JsonProperty("max_uses_number")]
        [XmlElement(ElementName = "max_uses_number")]
        public int MaxUsesNumber { get; set; }

        /// <summary>
        /// Maximum number of uses for each coupon in the group on a renewable subscription
        /// </summary>
        [DataMember(Name = "max_uses_number_on_renewable_sub")]
        [JsonProperty("max_uses_number_on_renewable_sub")]
        [XmlElement(ElementName = "max_uses_number_on_renewable_sub")]
        public int MaxUsesNumberOnRenewableSub { get; set; }
    }
}