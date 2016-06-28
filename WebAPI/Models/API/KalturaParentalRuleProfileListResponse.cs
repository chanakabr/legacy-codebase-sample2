using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// ParentalRules list
    /// </summary>
    [DataContract(Name = "ParentalRules", Namespace = "")]
    [XmlRoot("ParentalRules")]
    public class KalturaParentalRuleProfileListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of parental rules
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaParentalRuleProfile> ParentalRule { get; set; }
    }
}