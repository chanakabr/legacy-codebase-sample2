using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.ConditionalAccess
{
    [DataContract(Name = "Entitlements", Namespace = "")]
    [XmlRoot("Entitlements")]
    public class EntitlementsList
    {
        [DataMember(Name = "entitlements")]
        [JsonProperty("entitlements")]
        public List<Entitlement> Entitlements { get; set; }
    }
}