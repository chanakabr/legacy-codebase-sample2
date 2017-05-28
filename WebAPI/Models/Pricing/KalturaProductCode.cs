using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Product Code
    /// </summary>
    public class KalturaProductCode : KalturaOTTObject
    {
        /// <summary>
        /// Provider Name
        /// </summary>
        [DataMember(Name = "inappProvider")]
        [JsonProperty("inappProvider")]
        [XmlElement(ElementName = "inappProvider", IsNullable = true)]
        public string InappProvider { get; set; }

        /// <summary>
        /// Product Code
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code", IsNullable = true)]
        public string Code { get; set; }
    }
}