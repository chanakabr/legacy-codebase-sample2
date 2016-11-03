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
    [Obsolete]
    public class KalturaRecordingContext : KalturaOTTObject
    {

        /// <summary>
        /// query status code
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code")]
        public int Code { get; set; }

        /// <summary>
        /// query status message
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty("message")]
        [XmlElement(ElementName = "message", IsNullable = true)]
        public string Message { get; set; }

        /// <summary>
        /// Asset identifier
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty("assetId")]
        [XmlElement(ElementName = "assetId")]
        public long AssetId { get; set; }

        /// <summary>
        /// Recording object
        /// </summary>
        [DataMember(Name = "recording")]
        [JsonProperty("recording")]
        [XmlElement(ElementName = "recording", IsNullable = true)]
        public KalturaRecording Recording { get; set; }
    }
}