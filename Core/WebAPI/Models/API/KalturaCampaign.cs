using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Campaign
    /// </summary>
    public partial class KalturaCampaign : KalturaOTTObjectSupportNullable
    {
        /// <summary>
        /// ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Create date of the rule
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true, RequiresPermission = (int)RequestType.READ)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Update date of the rule
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty(PropertyName = "updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true, RequiresPermission = (int)RequestType.READ)]
        public long UpdateDate { get; set; }

        /// <summary>
        /// Start date of the rule
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty(PropertyName = "startDate")]
        [XmlElement(ElementName = "startDate")]
        [SchemeProperty(MinLong = 1)]
        public long StartDate { get; set; }

        /// <summary>
        /// End date of the rule
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty(PropertyName = "endDate")]
        [XmlElement(ElementName = "endDate")]
        [SchemeProperty(MinLong = 2)]
        public long EndDate { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// systemName
        /// </summary>
        [DataMember(Name = "systemName")]
        [JsonProperty("systemName")]
        [XmlElement(ElementName = "systemName")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public string SystemName { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MaxLength = 1024, RequiresPermission = (int)RequestType.READ)]
        public string Description { get; set; }

        /// <summary>
        /// state
        /// </summary>
        [DataMember(Name = "state")]
        [JsonProperty("state")]
        [XmlElement(ElementName = "state")]
        [SchemeProperty(ReadOnly = true, RequiresPermission = (int)RequestType.READ)]
        public KalturaObjectState State { get; set; }

        /// <summary>
        /// The Promotion that is promoted to the user
        /// </summary>
        [DataMember(Name = "promotion")]
        [JsonProperty("promotion")]
        [XmlElement(ElementName = "promotion", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaBasePromotion Promotion { get; set; }

        /// <summary>
        /// Free text message to the user that gives information about the campaign.
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty("message")]
        [XmlElement(ElementName = "message")]
        [SchemeProperty(MaxLength = 1024)]
        public string Message { get; set; }

        /// <summary>
        /// Comma separated collection IDs list
        /// </summary>
        [DataMember(Name = "collectionIdIn")]
        [JsonProperty("collectionIdIn")]
        [XmlElement(ElementName = "collectionIdIn")]
        [SchemeProperty(IsNullable = true)]
        public string CollectionIdIn { get; set; }

        /// <summary>
        /// Asset user rule identifier 
        /// </summary>
        [DataMember(Name = "assetUserRuleId")]
        [JsonProperty("assetUserRuleId")]
        [XmlElement(ElementName = "assetUserRuleId")]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL, IsNullable = true, MinLong = 1)]
        public long? AssetUserRuleId { get; set; }
    }
}