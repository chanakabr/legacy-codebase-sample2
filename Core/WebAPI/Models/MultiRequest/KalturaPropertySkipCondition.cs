using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;

namespace WebAPI.Models.MultiRequest
{
    /// <summary>
    /// Skips current request according to condition on given property 
    /// </summary>
    public partial class KalturaPropertySkipCondition : KalturaSkipCondition
    {
        /// <summary>
        /// The property path on which the condition is checked
        /// </summary>
        [DataMember(Name = "propertyPath")]
        [JsonProperty("propertyPath")]
        [XmlElement(ElementName = "propertyPath")]
        public string PropertyPath { get; set; }

        /// <summary>
        /// The operator that applies the check to the condition
        /// </summary>
        [DataMember(Name = "operator")]
        [JsonProperty("operator")]
        [XmlElement(ElementName = "operator")]
        public KalturaSkipOperators Operator { get; set; }

        /// <summary>
        /// The value on which the condition is checked
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }
    }
}