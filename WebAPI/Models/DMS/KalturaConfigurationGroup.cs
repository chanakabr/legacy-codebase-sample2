using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public class KalturaConfigurationGroup : KalturaOTTObject
    {
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        public int PartnerId { get; set; }

        [DataMember(Name = "isDefault")]
        [JsonProperty("isDefault")]
        [XmlElement(ElementName = "isDefault")]
        public bool IsDefault { get; set; }

        [DataMember(Name = "tags")]
        [JsonProperty("tags")]
        [XmlElement(ElementName = "tags")]
        public KalturaStringValueArray Tags { get; set; }

        [DataMember(Name = "numberOfDevices")]
        [JsonProperty("numberOfDevices")]
        [XmlElement(ElementName = "numberOfDevices")]
        public long NumberOfDevices { get; set; }

        [DataMember(Name = "configFileIds")]
        [JsonProperty("configFileIds")]
        [XmlElement(ElementName = "configFileIds")]
        public KalturaStringValueArray ConfigFileIds { get; set; }

        [DataMember(Name = "docType")]
        [JsonProperty("docType")]
        [XmlElement(ElementName = "docType")]
        private string DocType { get; set; }

        public KalturaConfigurationGroup()
        {
            this.DocType = "group_configuration";
            this.Tags = new KalturaStringValueArray();
            this.ConfigFileIds = new KalturaStringValueArray();
        }
    }
}