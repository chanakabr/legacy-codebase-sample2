using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Recordings info wrapper
    /// </summary>
    [Serializable]
    public partial class KalturaRecordingListResponse : KalturaListResponse
    {
        /// <summary>
        /// Recordings
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaRecording> Objects { get; set; }

    }

}