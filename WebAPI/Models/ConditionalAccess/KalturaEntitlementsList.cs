using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Entitlements list
    /// </summary>
    [DataContract(Name = "Entitlements", Namespace = "")]
    [XmlRoot("Entitlements")]
    public class KalturaEntitlementsList : KalturaOTTObject
    {
        /// <summary>
        /// A list of entitlements
        /// </summary>
        [DataMember(Name = "entitlements")]
        [JsonProperty("entitlements")]
        public List<KalturaEntitlement> Entitlements { get; set; }
    }
}