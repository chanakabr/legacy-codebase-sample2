using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.API
{
    public class KalturaOSSAdapterInsertResponse
    {
        /// <summary>
        /// OSS adapter identifier
        /// </summary>
        [DataMember(Name = "oss_adapter_id")]
        [JsonProperty("oss_adapter_id")]
        [XmlElement(ElementName = "oss_adapter_id")]
        public int OSSAdapterId { get; set; }

        /// <summary>
        /// Shared secret
        /// </summary>
        [DataMember(Name = "shared_secret")]
        [JsonProperty("shared_secret")]
        [XmlElement(ElementName = "shared_secret")]
        public string SharedSecret { get; set; }
    }
}