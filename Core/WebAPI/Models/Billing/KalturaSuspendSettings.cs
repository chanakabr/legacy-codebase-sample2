using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    /// <summary>
    /// Suspend Settings
    /// </summary>
    public partial class KalturaSuspendSettings : KalturaOTTObject
    {
        /// <summary>
        /// revoke entitlements
        /// </summary>
        [DataMember(Name = "revokeEntitlements")]
        [JsonProperty("revokeEntitlements")]
        [XmlElement(ElementName = "revokeEntitlements")]
        public bool RevokeEntitlements { get; set; }

        /// <summary>
        /// stop renew
        /// </summary>
        [DataMember(Name = "stopRenew")]
        [JsonProperty("stopRenew")]
        [XmlElement(ElementName = "stopRenew")]
        public bool StopRenew { get; set; }
    }
}
