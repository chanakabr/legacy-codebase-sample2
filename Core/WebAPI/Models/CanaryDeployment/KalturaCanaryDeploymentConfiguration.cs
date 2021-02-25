using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.CanaryDeployment
{
    public partial class KalturaCanaryDeploymentConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// DataOwnerShip
        /// </summary>
        [DataMember(Name = "dataOwnerShip")]
        [JsonProperty("dataOwnerShip")]
        [XmlElement(ElementName = "dataOwnerShip")]
        public KalturaCanaryDeploymentDataOwnerShip DataOwnerShip { get; set; }

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
        public KalturaCanaryDeploymentMigrationEvents MigrationEvents { get; set; }

        /// <summary>
        /// ShouldProduceInvalidationEventsToKafka
        /// </summary>
        [DataMember(Name = "shouldProduceInvalidationEventsToKafka")]
        [JsonProperty("shouldProduceInvalidationEventsToKafka")]
        [XmlElement(ElementName = "shouldProduceInvalidationEventsToKafka")]
        public bool ShouldProduceInvalidationEventsToKafka { get; set; }
    }
}
