using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.CanaryDeployment.Elasticsearch
{
    public partial class KalturaElasticsearchCanaryDeploymentConfiguration : KalturaOTTObject
    {
        
        /// <summary>
        /// ElasticsearchActiveVersion
        /// </summary>
        [DataMember(Name = "elasticsearchActiveVersion")]
        [JsonProperty("elasticsearchActiveVersion")]
        [XmlElement(ElementName = "elasticsearchActiveVersion")]
        public KalturaElasticsearchVersion ElasticsearchActiveVersion { get; set; }
        
        /// <summary>
        /// EnableMigrationEvents
        /// </summary>
        [DataMember(Name = "enableMigrationEvents")]
        [JsonProperty("enableMigrationEvents")]
        [XmlElement(ElementName = "enableMigrationEvents")]
        public bool EnableMigrationEvents { get; set; }
    }
    
    public enum KalturaElasticsearchVersion
    {
        ES_2_3,
        ES_7,
    }
}