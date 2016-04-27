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
        [DataMember(Name = "requestId")]
        [JsonProperty("requestId")]
        [XmlElement(ElementName = "requestId", IsNullable = true)]
        public string RequestId { get; set; }

    }
}