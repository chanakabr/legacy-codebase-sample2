using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaRecordingContext : KalturaOTTObject
    {

        /// <summary>
        /// query status
        /// </summary>
        [DataMember(Name = "queryStatus")]
        [JsonProperty("queryStatus")]
        [XmlElement(ElementName = "queryStatus", IsNullable = true)]
        public int QueryStatus { get; set; }

        /// <summary>
        /// Recording object
        /// </summary>
        [DataMember(Name = "recording")]
        [JsonProperty("recording")]
        [XmlElement(ElementName = "recording", IsNullable = true)]
        public KalturaRecording Recording { get; set; }
    }
}