using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public class KalturaConfigurationGroup : KalturaOTTObject
    {
        //TODO: onlt name is ediatble

        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]        
        public string Id { get; set; }

        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        [SchemeProperty(ReadOnly = true)]
        public int PartnerId { get; set; }

        [DataMember(Name = "isDefault")]
        [JsonProperty("isDefault")]
        [XmlElement(ElementName = "isDefault")]
        [SchemeProperty(ReadOnly = true)]
        public bool IsDefault { get; set; }

        [DataMember(Name = "tags")]
        [JsonProperty("tags")]
        [XmlElement(ElementName = "tags")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaStringValue> Tags { get; set; }

        [DataMember(Name = "numberOfDevices")]
        [JsonProperty("numberOfDevices")]
        [XmlElement(ElementName = "numberOfDevices")]
        [SchemeProperty(ReadOnly = true)]
        public long NumberOfDevices { get; set; }

        [DataMember(Name = "configFiles")]
        [JsonProperty("configFiles")]
        [XmlElement(ElementName = "configFiles")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaConfigurationMin> ConfigFiles { get; set; }       

        public KalturaConfigurationGroup()
        {
            this.Tags = new  List<KalturaStringValue>();
            this.ConfigFiles = new List<KalturaConfigurationMin>();
        }
    }
}