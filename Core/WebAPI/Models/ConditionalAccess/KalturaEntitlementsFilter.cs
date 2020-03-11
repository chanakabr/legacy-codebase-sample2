using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Entitlements filter 
    /// </summary>
    [Obsolete]
    public partial class KalturaEntitlementsFilter : KalturaOTTObject 
    {
        /// <summary>
        ///The type of the entitlements to return
        /// </summary>
        [DataMember(Name = "entitlementType")]
        [JsonProperty("entitlementType")]
        [XmlElement(ElementName = "entitlementType")]
        [OldStandardProperty("entitlement_type")]
        public KalturaTransactionType EntitlementType { get; set; }

        /// <summary>
        ///Reference type to filter by
        /// </summary>
        [DataMember(Name = "by")]
        [JsonProperty("by")]
        [XmlElement(ElementName = "by")]
        public KalturaEntityReferenceBy By { get; set; }
    }
}