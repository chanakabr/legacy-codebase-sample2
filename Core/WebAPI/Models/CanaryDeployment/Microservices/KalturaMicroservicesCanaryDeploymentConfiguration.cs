using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.CanaryDeployment.Microservices
{
    public partial class KalturaMicroservicesCanaryDeploymentConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// DataOwnerShip
        /// </summary>
        [DataMember(Name = "dataOwnerShip")]
        [JsonProperty("dataOwnerShip")]
        [XmlElement(ElementName = "dataOwnerShip")]
        public KalturaMicroservicesCanaryDeploymentDataOwnerShip DataOwnerShip { get; set; }

        /// <summary>
        /// RoutingConfiguration
        /// </summary>
        [DataMember(Name = "routingConfiguration")]
        [JsonProperty("routingConfiguration")]
        [XmlElement(ElementName = "routingConfiguration")]
        public SerializableDictionary<string, KalturaStringValue> RoutingConfiguration { get; set; }

        /// <summary>
        /// MigrationEvents
        /// </summary>
        [DataMember(Name = "migrationEvents")]
        [JsonProperty("migrationEvents")]
        [XmlElement(ElementName = "migrationEvents")]
        public KalturaMicroservicesCanaryDeploymentMigrationEvents MigrationEvents { get; set; }

    }
}
