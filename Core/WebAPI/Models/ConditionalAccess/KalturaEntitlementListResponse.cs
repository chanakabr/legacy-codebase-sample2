using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Entitlements list
    /// </summary>
    [DataContract(Name = "Entitlements", Namespace = "")]
    [XmlRoot("Entitlements")]
    public partial class KalturaEntitlementListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of entitlements
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaEntitlement> Entitlements { get; set; }
    }
}