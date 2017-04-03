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
    /// A representation to return an array of values
    /// </summary>
    [XmlInclude(typeof(KalturaStringValue))]
    [XmlInclude(typeof(KalturaIntegerValue))]
    [XmlInclude(typeof(KalturaBooleanValue))]
    [XmlInclude(typeof(KalturaDoubleValue))]
    [XmlInclude(typeof(KalturaMultilingualStringValue))]
    [XmlInclude(typeof(KalturaLongValue))]    
    public abstract class KalturaValue : KalturaOTTObject
    {
        /// <summary>
        /// Description
        /// </summary>
        [DataMember(Name = "description")]
        [XmlElement("description", IsNullable = true)]
        [JsonProperty("description")]
        public string description { get; set; }
    }
}