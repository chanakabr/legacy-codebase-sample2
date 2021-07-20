using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.CanaryDeployment.Microservices
{
    public partial class KalturaMicroservicesCanaryDeploymentMigrationEvents : KalturaOTTObject
    {
        /// <summary>
        /// AppToken
        /// </summary>
        [DataMember(Name = "appToken")]
        [JsonProperty("appToken")]
        [XmlElement(ElementName = "appToken")]
        public bool AppToken { get; set; }

        /// <summary>
        /// RefreshToken
        /// </summary>
        [DataMember(Name = "refreshToken")]
        [JsonProperty("refreshToken")]
        [XmlElement(ElementName = "refreshToken")]
        public bool RefreshToken { get; set; }

        /// <summary>
        /// UserPinCode
        /// </summary>
        [DataMember(Name = "userPinCode")]
        [JsonProperty("userPinCode")]
        [XmlElement(ElementName = "userPinCode")]
        public bool UserPinCode { get; set; }

        /// <summary>
        /// DevicePinCode
        /// </summary>
        [DataMember(Name = "devicePinCode")]
        [JsonProperty("devicePinCode")]
        [XmlElement(ElementName = "devicePinCode")]
        public bool DevicePinCode { get; set; }

        /// <summary>
        /// SessionRevocation
        /// </summary>
        [DataMember(Name = "sessionRevocation")]
        [JsonProperty("sessionRevocation")]
        [XmlElement(ElementName = "sessionRevocation")]
        public bool SessionRevocation { get; set; }

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
    }
}
