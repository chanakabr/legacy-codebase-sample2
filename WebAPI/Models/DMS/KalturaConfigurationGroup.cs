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
        /// <summary>
        /// Configuration group identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]        
        public string Id { get; set; }

        /// <summary>
        /// Configuration group name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Partner id
        /// </summary>
        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        [SchemeProperty(ReadOnly = true)]
        public int PartnerId { get; set; } // TODO: check for ignore

        /// <summary>
        /// Is default
        /// </summary>
        [DataMember(Name = "isDefault")]
        [JsonProperty("isDefault")]
        [XmlElement(ElementName = "isDefault")]
        [SchemeProperty(ReadOnly = true)]
        public bool IsDefault { get; set; }

        /// <summary>
        /// tags
        /// </summary>
        [DataMember(Name = "tags")]
        [JsonProperty("tags")]
        [XmlElement(ElementName = "tags")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaStringValue> Tags { get; set; }

        /// <summary>
        /// Number of devices
        /// </summary>
        [DataMember(Name = "numberOfDevices")]
        [JsonProperty("numberOfDevices")]
        [XmlElement(ElementName = "numberOfDevices")]
        [SchemeProperty(ReadOnly = true)]
        public long NumberOfDevices { get; set; }

        /// <summary>
        /// Configuration files
        /// </summary>
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