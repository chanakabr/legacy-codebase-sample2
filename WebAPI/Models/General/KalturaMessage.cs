using ApiObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.App_Start;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Message
    /// </summary>
    public partial class KalturaMessage : KalturaOTTObject
    {
        /// <summary>
        /// Massage code
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public int Code { get; set; }

        /// <summary>
        /// Message details
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty("message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Message args
        /// </summary>
        [DataMember(Name = "args")]
        [JsonProperty("args")]
        [XmlElement(ElementName = "args", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> Args { get; set; }
    }
}