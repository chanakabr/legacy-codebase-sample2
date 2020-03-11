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
        /// Segmentation type version
        /// </summary>
        [DataMember(Name = "version")]
        [JsonProperty(PropertyName = "version")]
        [XmlElement(ElementName = "version")]
        [SchemeProperty(ReadOnly = true)]
        public long Version { get; set; }

    }

    public enum KalturaContentAction
    {
        watch_linear,
        watch_vod,
        catchup,
        npvr,
        favorite,
        recording,
        social_action
    }
    
    public enum KalturaMonetizationType
    {
        ppv,
        subscription,
        boxset
    }

    public enum KalturaMathemticalOperatorType
    {
        count,
        sum,
        avg
    }

    public enum KalturaContentActionConditionLengthType
    {
        minutes,
        percentage
    }
}