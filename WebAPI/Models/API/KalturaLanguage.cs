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
    /// Language details
    /// </summary>
    public class KalturaLanguage : KalturaOTTObject
    {

        /// <summary>
        /// Language name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Language system name
        /// </summary>
        [DataMember(Name = "systemName")]
        [JsonProperty("systemName")]
        [XmlElement(ElementName = "systemName", IsNullable=true)]
        public string SystemName { get; set; }

        /// <summary>
        /// Language code
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }

        /// <summary>
        /// Language direction (LTR/RTL)
        /// </summary>
        [DataMember(Name = "direction")]
        [JsonProperty("direction")]
        [XmlElement(ElementName = "direction")]
        public string Direction { get; set; }

        /// <summary>
        /// Is the default language of the account
        /// </summary>
        [DataMember(Name = "isDefault")]
        [JsonProperty("isDefault")]
        [XmlElement(ElementName = "isDefault")]
        public bool IsDefault { get; set; }

    }
}