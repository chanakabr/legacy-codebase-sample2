using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Currency details
    /// </summary>
    public class KalturaCurrency : KalturaOTTObject
    {

        /// <summary>
        /// Currency name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Currency Sign
        /// </summary>
        [DataMember(Name = "sign")]
        [JsonProperty("sign")]
        [XmlElement(ElementName = "sign")]
        public string Sign { get; set; }

        /// <summary>
        /// Is the default Currency of the account
        /// </summary>
        [DataMember(Name = "isDefault")]
        [JsonProperty("isDefault")]
        [XmlElement(ElementName = "isDefault")]
        public bool IsDefault { get; set; }

    }
}