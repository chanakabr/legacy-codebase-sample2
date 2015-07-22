using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.API
{
    /// <summary>
    /// ParentalRules list
    /// </summary>
    [DataContract(Name = "ParentalRules", Namespace = "")]
    [XmlRoot("ParentalRules")]
    public class KalturaParentalRulesList
    {
        /// <summary>
        /// A list of parental rules
        /// </summary>
        [DataMember(Name = "rules")]
        [JsonProperty("rules")]
        public List<KalturaParentalRule> ParentalRules;
    }
}