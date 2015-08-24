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
    /// Entitlements filter 
    /// </summary>
    public class KalturaEntitlementsFilter : KalturaEntityReferenceByFilter
    {
        /// <summary>
        ///The type of the entitlements to return
        /// </summary>
        [DataMember(Name = "entitlement_type")]
        [JsonProperty("entitlement_type")]
        [XmlElement(ElementName = "entitlement_type")]
        public KalturaTransactionType EntitlementType { get; set; }
    }
}