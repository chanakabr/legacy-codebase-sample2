using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Social
{
    /// <summary>
    /// Returns social configuration for the partner 
    /// </summary>
    public class KalturaSocialConfig : KalturaOTTObject
    {
        /// <summary>
        ///The application identifier
        /// </summary>
        [DataMember(Name = "app_id")]
        [JsonProperty("app_id")]
        [XmlElement(ElementName = "app_id")]
        public string AppId { get; set; }

        /// <summary>
        /// List of application permissions
        /// </summary>
        [DataMember(Name = "permissions")]
        [JsonProperty("permissions")]
        [XmlElement(ElementName = "permissions")]
        public string Permissions { get; set; }
    }
}