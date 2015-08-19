using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Holder object for household device registration status enum
    /// </summary>
    public class KalturaDeviceRegistrationStatusHolder : KalturaOTTObject
    {
        /// <summary>
        /// Household device registration status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement("status")]
        public KalturaDeviceRegistrationStatus Status { get; set; }
    }
}