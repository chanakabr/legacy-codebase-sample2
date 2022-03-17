using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public partial class KalturaPromotionInfo : KalturaOTTObject
    {
        /// <summary>
        /// Campaign Id
        /// </summary>
        [DataMember(Name = "campaignId")]
        [JsonProperty("campaignId")]
        [XmlElement(ElementName = "campaignId")]
        public long? CampaignId { get; set; }
    } 
}