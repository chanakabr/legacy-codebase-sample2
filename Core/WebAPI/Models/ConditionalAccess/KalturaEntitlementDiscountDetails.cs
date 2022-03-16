using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Entitlement discount details
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(KalturaCouponEntitlementDiscountDetails))]
    public partial class KalturaEntitlementDiscountDetails : KalturaOTTObject
    {
        /// <summary>
        /// Amount
        /// </summary>
        [DataMember(Name = "amount")]
        [JsonProperty("amount")]
        [XmlElement(ElementName = "amount")]
        [SchemeProperty(ReadOnly = true)]
        public double Amount { get; set; }

        /// <summary>
        /// Start date
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        [SchemeProperty(ReadOnly = true)]
        public long? StartDate { get; set; }

        /// <summary>
        /// End date
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate")]
        [SchemeProperty(ReadOnly = true)]
        public long? EndDate { get; set; }
    }
}