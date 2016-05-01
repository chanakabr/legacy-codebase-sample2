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
        /// Recording object
        /// </summary>
        [DataMember(Name = "recording")]
        [JsonProperty("recording")]
        [XmlElement(ElementName = "recording", IsNullable = true)]
        public KalturaRecording Recording { get; set; }
    }
}