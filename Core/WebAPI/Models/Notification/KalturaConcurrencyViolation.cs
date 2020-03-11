using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.API;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    [Serializable]
    public partial class KalturaConcurrencyViolation : KalturaEventObject
    {
        /// <summary>
        /// Timestamp
        /// </summary>
        [DataMember(Name = "timestamp")]
        [JsonProperty(PropertyName = "timestamp")]
        [XmlElement(ElementName = "timestamp")]
        public long Timestamp { get; set; }

        /// <summary>
        /// UDID
        /// </summary>
        [DataMember(Name = "udid")]
        [JsonProperty(PropertyName = "udid")]
        [XmlElement(ElementName = "udid")]
        public string UDID { get; set; }

        /// <summary>
        /// Asset Id
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty(PropertyName = "assetId")]
        [XmlElement(ElementName = "assetId")]
        public string AssetId { get; set; }

        /// <summary>
        /// Violation Rule
        /// </summary>
        [DataMember(Name = "violationRule")]
        [JsonProperty(PropertyName = "violationRule")]
        [XmlElement(ElementName = "violationRule")]
        public string ViolationRule { get; set; }

        /// <summary>
        /// Household Id
        /// </summary>
        [DataMember(Name = "householdId")]
        [JsonProperty(PropertyName = "householdId")]
        [XmlElement(ElementName = "householdId")]
        public string HouseholdId { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty(PropertyName = "userId")]
        [XmlElement(ElementName = "userId")]
        public string UserId { get; set; }
    }
}
