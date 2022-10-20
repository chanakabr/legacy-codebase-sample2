using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
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
        public KalturaCanaryDeploymentAuthenticationMsOwnerShip AuthenticationMsOwnerShip { get; set; }

        /// <summary>
        /// SegmentationMsOwnerShip
        /// </summary>
        [DataMember(Name = "segmentationMsOwnerShip")]
        [JsonProperty("segmentationMsOwnerShip")]
        [XmlElement(ElementName = "segmentationMsOwnerShip")]
        public KalturaCanaryDeploymentSegmentationMsOwnerShip SegmentationMsOwnerShip { get; set; }
    }
}
