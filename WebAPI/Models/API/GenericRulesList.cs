using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.API
{
    [DataContract(Name = "GenericRules", Namespace = "")]
    [XmlRoot("GenericRules")]
    public class GenericRulesList
    {
        [DataMember(Name = "rules")]
        [JsonProperty("rules")]
        public List<GenericRule> GenericRules;
    }
}