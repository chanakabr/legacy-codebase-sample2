using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.CanaryDeployment.Microservices
{
    public partial class KalturaCanaryDeploymentAuthenticationMsOwnerShip : KalturaOTTObject
    {
        /// <summary>
        /// UserLoginHistory
        /// </summary>
        [DataMember(Name = "userLoginHistory")]
        [JsonProperty("userLoginHistory")]
        [XmlElement(ElementName = "userLoginHistory")]
        public bool UserLoginHistory { get; set; }

        /// <summary>
        /// DeviceLoginHistory
        /// </summary>
        [DataMember(Name = "deviceLoginHistory")]
        [JsonProperty("deviceLoginHistory")]
        [XmlElement(ElementName = "deviceLoginHistory")]
        public bool DeviceLoginHistory { get; set; }

        /// <summary>
        /// SessionRevocation
        /// </summary>
        [DataMember(Name = "sessionRevocation")]
        [JsonProperty("sessionRevocation")]
        [XmlElement(ElementName = "sessionRevocation")]
        public bool SessionRevocation { get; set; }

        /// <summary>
        /// SSOAdapterProfiles
        /// </summary>
        [DataMember(Name = "sSOAdapterProfiles")]
        [JsonProperty("sSOAdapterProfiles")]
        [XmlElement(ElementName = "sSOAdapterProfiles")]
        public bool SSOAdapterProfiles { get; set; }

        /// <summary>
        /// RefreshToken
        /// </summary>
        [DataMember(Name = "refreshToken")]
        [JsonProperty("refreshToken")]
        [XmlElement(ElementName = "refreshToken")]
        public bool RefreshToken { get; set; }

        /// <summary>
        /// DeviceLoginPin
        /// </summary>
        [DataMember(Name = "deviceLoginPin")]
        [JsonProperty("deviceLoginPin")]
        [XmlElement(ElementName = "deviceLoginPin")]
        public bool DeviceLoginPin { get; set; }
    }
}
