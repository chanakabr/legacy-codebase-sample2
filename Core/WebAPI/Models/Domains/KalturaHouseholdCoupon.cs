using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Household Coupon details
    /// </summary>
    [Serializable]
    public partial class KalturaHouseholdCoupon : KalturaOTTObjectSupportNullable
    {        
        /// <summary>
        /// Coupon code
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Last Usage Date
        /// </summary>
        [DataMember(Name = "lastUsageDate")]
        [JsonProperty("lastUsageDate")]
        [XmlElement(ElementName = "lastUsageDate")]
        public long? LastUsageDate { get; set; }
    }
}