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
    public class KalturaGenericRulesList : KalturaOTTObject
    {
        /// <summary>
        /// A list of generic rules
        /// </summary>
        [DataMember(Name = "rules")]
        [JsonProperty("rules")]
        [XmlElement(ElementName = "rules")]
        public List<KalturaGenericRule> GenericRules;
    }
}