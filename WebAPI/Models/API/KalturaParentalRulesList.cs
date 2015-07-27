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
    public class KalturaParentalRulesList : KalturaOTTObject
    {
        /// <summary>
        /// A list of parental rules
        /// </summary>
        [DataMember(Name = "rules")]
        [JsonProperty("rules")]
        [XmlElement(ElementName = "rules")]
        public List<KalturaParentalRule> ParentalRules;
    }
}