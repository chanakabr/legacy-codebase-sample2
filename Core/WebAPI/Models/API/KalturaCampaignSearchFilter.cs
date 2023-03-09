using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    public partial class KalturaCampaignSearchFilter : KalturaCampaignFilter
    {
        /// <summary>
        /// start Date Greater Than Or Equal
        /// </summary>
        [DataMember(Name = "startDateGreaterThanOrEqual")]
        [JsonProperty("startDateGreaterThanOrEqual")]
        [XmlElement(ElementName = "startDateGreaterThanOrEqual", IsNullable = true)]
        public long? StartDateGreaterThanOrEqual { get; set; }

        /// <summary>
        /// end Date Greater Than Or Equal
        /// </summary>
        [DataMember(Name = "endDateLessThanOrEqual")]
        [JsonProperty("endDateLessThanOrEqual")]
        [XmlElement(ElementName = "endDateLessThanOrEqual", IsNullable = true)]
        public long? EndDateLessThanOrEqual { get; set; }

        /// <summary>
        /// state Equal
        /// </summary>
        [DataMember(Name = "stateEqual")]
        [JsonProperty("stateEqual")]
        [XmlElement(ElementName = "stateEqual", IsNullable = true)]
        public KalturaObjectState? StateEqual { get; set; }

        /// <summary>
        /// has Promotion
        /// </summary>
        [DataMember(Name = "hasPromotion")]
        [JsonProperty("hasPromotion")]
        [XmlElement(ElementName = "hasPromotion", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool? HasPromotion { get; set; }

        /// <summary>
        /// Filter the Campaign with this name.
        /// </summary>
        [DataMember(Name = "nameEqual")]
        [JsonProperty("nameEqual")]
        [XmlElement(ElementName = "nameEqual", IsNullable = true)]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL)]
        public string NameEqual { get; set; }

        /// <summary>
        /// A string that is included in the Campaign name
        /// </summary>
        [DataMember(Name = "nameContains")]
        [JsonProperty("nameContains")]
        [XmlElement(ElementName = "nameContains", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL)]
        public string NameContains { get; set; }

        /// <summary>
        /// Comma separated Campaign State list
        /// </summary>
        [DataMember(Name = "stateIn")]
        [JsonProperty("stateIn")]
        [XmlElement(ElementName = "stateIn", IsNullable = true)]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL, MinLength = 1, IsNullable = true)]
        public string StateIn { get; set; }

        /// <summary>
        /// Comma separated AssetUserRule Ids to filter by
        /// </summary>
        [DataMember(Name = "assetUserRuleIdIn")]
        [JsonProperty("assetUserRuleIdIn")]
        [XmlElement(ElementName = "assetUserRuleIdIn", IsNullable = true)]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL, MinLength = 1, IsNullable = true, DynamicMinInt = 1)]
        public string AssetUserRuleIdIn { get; set; }
    }
}