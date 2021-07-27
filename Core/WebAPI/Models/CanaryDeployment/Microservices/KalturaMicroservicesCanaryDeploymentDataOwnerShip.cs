using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.CanaryDeployment.Microservices
{
    public partial class KalturaMicroservicesCanaryDeploymentDataOwnerShip : KalturaOTTObject
    {
        /// <summary>
        /// AuthenticationMsOwnerShip
        /// </summary>
        [DataMember(Name = "authenticationMsOwnerShip")]
        [JsonProperty("authenticationMsOwnerShip")]
        [XmlElement(ElementName = "authenticationMsOwnerShip")]
        public Microservices.KalturaCanaryDeploymentAuthenticationMsOwnerShip AuthenticationMsOwnerShip { get; set; }
    }
}
