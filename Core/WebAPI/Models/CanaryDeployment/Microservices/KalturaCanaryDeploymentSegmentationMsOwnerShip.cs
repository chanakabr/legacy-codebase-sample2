using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.CanaryDeployment.Microservices
{
    public partial class KalturaCanaryDeploymentSegmentationMsOwnerShip : KalturaOTTObject
    {
        /// <summary>
        /// segmentationType, userSegment and householdSegment
        /// </summary>
        [DataMember(Name = "segmentation")]
        [JsonProperty("segmentation")]
        [XmlElement(ElementName = "segmentation")]
        public bool Segmentation { get; set; }
    }
}
