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
    /// <summary>
    /// Series Recordings info wrapper
    /// </summary>
    [Serializable]
    public class KalturaSeriesRecordingListResponse : KalturaListResponse
    {
        /// <summary>
        /// Series Recordings
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaSeriesRecording> Objects { get; set; }

    }
}