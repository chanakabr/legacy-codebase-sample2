using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// List of registry settings.
    /// </summary>
    [DataContract(Name = "KalturaRegistrySettingsListResponse", Namespace = "")]
    [XmlRoot("KalturaRegistrySettingsListResponse")]
    public class KalturaRegistrySettingsListResponse : KalturaListResponse
    {
        /// <summary>
        /// Registry settings list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaRegistrySettings> RegistrySettings { get; set; }
    }

    public class KalturaRegistrySettings : KalturaOTTObject    
    {
        /// <summary>
        /// Permission item identifier
        /// </summary>
        [DataMember(Name = "key")]
        [JsonProperty("key")]
        [XmlElement(ElementName = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Permission item name
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }
    }
}
