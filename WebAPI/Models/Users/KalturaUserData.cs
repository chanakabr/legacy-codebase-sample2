using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.ComponentModel.DataAnnotations;
using WebAPI.Models.General;
using System.Xml.Serialization;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// User Data
    /// </summary>
    public class KalturaUserData : KalturaOTTObject
    {
        /// <summary>
        /// Basic Data
        /// </summary>
        [DataMember(Name = "basic_data")]
        [JsonProperty("basic_data")]
        [XmlElement(ElementName = "basic_data")]
        [Required]
        public KalturaUserBasicData userBasicData { get; set; }

        /// <summary>
        /// Dynamic Data
        /// </summary>
        [DataMember(Name = "dynamic_data")]
        [JsonProperty("dynamic_data")]
        [XmlElement(ElementName = "dynamic_data")]
        public SerializableDictionary<string, KalturaStringValue> userDynamicData { get; set; }
    }
}