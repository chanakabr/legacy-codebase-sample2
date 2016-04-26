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
    /// Request filter
    /// </summary>
    public class KalturaRequestFilter : KalturaFilter
    {

        /// <summary>
        /// request ID
        /// </summary>
        [DataMember(Name = "request_id")]
        [JsonProperty("request_id")]
        [XmlElement(ElementName = "request_id", IsNullable = true)]
        public string request_id { get; set; }

    }
}