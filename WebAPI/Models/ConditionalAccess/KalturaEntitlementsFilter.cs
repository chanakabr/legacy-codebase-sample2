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
    public class KalturaEntitlementsFilter : KalturaOTTObject
    {
        /// <summary>
        ///Reference type to filter by
        /// </summary>
        [DataMember(Name = "by")]
        [JsonProperty("by")]
        [XmlElement(ElementName = "by")]
        public KalturaReferenceType By { get; set; }

        /// <summary>
        ///Identifier to filter by (user identifier or household identifier)
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        /// <summary>
        ///The type of the entitlements to return
        /// </summary>
        [DataMember(Name = "entitlement_type")]
        [JsonProperty("entitlement_type")]
        [XmlElement(ElementName = "entitlement_type")]
        public KalturaTransactionType EntitlementType { get; set; }
    }
}