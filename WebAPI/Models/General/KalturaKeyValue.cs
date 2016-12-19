using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.General
{
    public class KalturaKeyValue : KalturaOTTObject
    {
        /// <summary>
        /// Key
        /// </summary>
        [DataMember(Name = "key")]
        [JsonProperty("key")]
        [XmlElement(ElementName = "key", IsNullable = true)]
        public string key { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value", IsNullable = true)]
        public string value { get; set; }
    }
}
