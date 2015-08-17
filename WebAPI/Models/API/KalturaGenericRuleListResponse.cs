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
    /// GenericRules list
    /// </summary>
    [DataContract(Name = "GenericRules", Namespace = "")]
    [XmlRoot("GenericRules")]
    public class KalturaGenericRuleListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of generic rules
        /// </summary>
        [DataMember(Name = "rules")]
        [JsonProperty("rules")]
        [XmlArray(ElementName = "rules")]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaGenericRule> GenericRules { get; set; }
    }
}