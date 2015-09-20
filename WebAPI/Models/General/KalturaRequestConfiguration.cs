using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Define client request optional configurations
    /// </summary>
    public class KalturaRequestConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// Impersonated partner id
        /// </summary>
        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        public int PartnerID { get; set; }

        /// <summary>
        /// Kaltura API session
        /// </summary>
        [DataMember(Name = "ks")]
        [JsonProperty("ks")]
        [XmlElement(ElementName = "ks")]
        public string KS { get; set; }

        /// <summary>
        /// Response profile (Not in used)
        /// </summary>
        [DataMember(Name = "responseProfile")]
        [JsonProperty("responseProfile")]
        [XmlElement(ElementName = "responseProfile", IsNullable = true)]
        public KalturaBaseResponseProfile ResponseProfile { get; set; }
    }
}