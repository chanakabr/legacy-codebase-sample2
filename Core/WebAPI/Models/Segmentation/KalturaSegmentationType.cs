using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Segmentation type - defines at least one segment 
    /// </summary>
    public partial class KalturaSegmentationType : KalturaOTTObject
    {
        /// <summary>
        /// Id of segmentation type
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Name of segmentation type
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Description of segmentation type
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty(PropertyName = "description")]
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Segmentation conditions - can be empty
        /// </summary>
        [DataMember(Name = "conditions", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "conditions", NullValueHandling = NullValueHandling.Ignore)]
        [XmlArray(ElementName = "conditions", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaBaseSegmentCondition> Conditions { get; set; }

        /// <summary>
        /// Boolean operator between segmentation type's conditions - defaults to "And"
        /// </summary>
        [DataMember(Name = "conditionsOperator")]
        [JsonProperty(PropertyName = "conditionsOperator")]
        [XmlElement(ElementName = "conditionsOperator", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaBooleanOperator? ConditionsOperator { get; set; }

        /// <summary>
        /// Segmentation conditions - can be empty
        /// </summary>
        [DataMember(Name = "actions", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "actions", NullValueHandling = NullValueHandling.Ignore)]
        [XmlArray(ElementName = "actions", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaBaseSegmentAction> Actions { get; set; }

        /// <summary>
        /// Segmentation values - can be empty (so only one segment will be created)
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty(PropertyName = "value")]
        [XmlElement(ElementName = "value")]
        public KalturaBaseSegmentValue Value { get; set; }

        /// <summary>
        /// Create date of segmentation type
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Update date of segmentation type
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty(PropertyName = "updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        /// <summary>
        /// Last date of execution of segmentation type
        /// </summary>
        [DataMember(Name = "executeDate")]
        [JsonProperty(PropertyName = "executeDate")]
        [XmlElement(ElementName = "executeDate")]
        [SchemeProperty(ReadOnly = true)]
        public long ExecuteDate { get; set; }

        /// <summary>
        /// Segmentation type version
        /// </summary>
        [DataMember(Name = "version")]
        [JsonProperty(PropertyName = "version")]
        [XmlElement(ElementName = "version")]
        [SchemeProperty(ReadOnly = true)]
        public long Version { get; set; }

        /// <summary>
        /// Asset User Rule Id
        /// </summary>
        [DataMember(Name = "assetUserRuleId")]
        [JsonProperty(PropertyName = "assetUserRuleId")]
        [XmlElement(ElementName = "assetUserRuleId")]
        [SchemeProperty(IsNullable = true, MinLong = 1, RequiresPermission = (int)RequestType.ALL)]
        public long? AssetUserRuleId { get; set; }
    }
}