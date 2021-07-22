using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;

namespace WebAPI.Models.Notification
{
    [Serializable]
    public partial class KalturaTriggerCampaignEvent : KalturaEventObject
    {
        /// <summary>
        /// User Id
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty(PropertyName = "userId")]
        [XmlElement(ElementName = "userId")]
        [SchemeProperty(ReadOnly = true)]
        public long UserId { get; set; }

        /// <summary>
        /// Campaign Id
        /// </summary>
        [DataMember(Name = "campaignId")]
        [JsonProperty(PropertyName = "campaignId")]
        [XmlElement(ElementName = "campaignId")]
        [SchemeProperty(ReadOnly = true)]
        public long CampaignId { get; set; }

        /// <summary>
        /// Udid
        /// </summary>
        [DataMember(Name = "udid")]
        [JsonProperty(PropertyName = "udid")]
        [XmlElement(ElementName = "udid")]
        [SchemeProperty(ReadOnly = true)]
        public string Udid { get; set; }

        /// <summary>
        /// Household Id
        /// </summary>
        [DataMember(Name = "householdId")]
        [JsonProperty(PropertyName = "householdId")]
        [XmlElement(ElementName = "householdId")]
        [SchemeProperty(ReadOnly = true)]
        public long HouseholdId { get; set; }
    }
}
