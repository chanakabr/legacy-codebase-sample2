using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.Social
{
    /// <summary>
    /// Returns social configuration for the partner 
    /// </summary>
    [OldStandard("appId", "app_id")]
    public class KalturaSocialConfig : KalturaOTTObject
    {
        /// <summary>
        ///The application identifier
        /// </summary>
        [DataMember(Name = "appId")]
        [JsonProperty("appId")]
        [XmlElement(ElementName = "appId")]
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