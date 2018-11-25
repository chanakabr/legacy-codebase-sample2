using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.MultiRequest
{
    /// <summary>
    /// Skip current request according to skip condition
    /// </summary>
    public abstract partial class KalturaSkipCondition : KalturaOTTObject
    {
    }

    /// <summary>
    /// Skips current request if an error occurs according to the selected skip option 
    /// </summary>
    public partial class KalturaSkipOnErrorCondition : KalturaSkipCondition
    {
        /// <summary>
        /// Indicates which error should be considered to skip the current request
        /// </summary>
        [DataMember(Name = "condition")]
        [JsonProperty("condition")]
        [XmlElement(ElementName = "condition")]
        public KalturaSkipOptions Condition { get; set; }

        public KalturaSkipOnErrorCondition()
        {
            Condition = KalturaSkipOptions.No;
        }
    }

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

    //public partial class KalturaAggregatedPropertySkipCondition : KalturaPropertySkipCondition
    //{
    //    public KalturaAggregationType AggregationType { get; set; }
    //}
}