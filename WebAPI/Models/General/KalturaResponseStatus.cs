using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Response Status
    /// </summary>
    public partial class KalturaResponseStatus : KalturaOTTObject
    {
        /// <summary>
        /// Code
        /// </summary>
        [DataMember(Name = "code")]
        [XmlElement("code")]
        [JsonProperty("code")]
        [SchemeProperty(ReadOnly = true)]
        public int Code { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        [DataMember(Name = "message")]
        [XmlElement("message")]
        [JsonProperty("message")]
        [SchemeProperty(ReadOnly = true)]
        public string Message { get; set; }

        // TODO SHIR - SET ARGS FOR KalturaStatus
        ///// <summary>
        ///// Args
        ///// </summary>
        //[DataMember(Name = "args")]
        //[XmlElement("args")]
        //[JsonProperty("args")]
        //[SchemeProperty(ReadOnly = true)]
        //public SerializableDictionary<KalturaStringValue, KalturaOTTObject> Args { get; set; }
    }
}