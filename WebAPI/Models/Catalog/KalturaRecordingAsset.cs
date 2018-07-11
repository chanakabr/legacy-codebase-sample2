using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Recording-asset info
    /// </summary>
    [Serializable]
    public partial class KalturaRecordingAsset : KalturaProgramAsset
    {
        /// <summary>
        /// Recording identifier
        /// </summary>
        [DataMember(Name = "recordingId")]
        [JsonProperty(PropertyName = "recordingId")]
        [XmlElement(ElementName = "recordingId")]
        public string RecordingId
        {
            get;
            set;
        }

        /// <summary>
        /// Recording Type: single/season/series
        /// </summary>
        [DataMember(Name = "recordingType")]
        [JsonProperty(PropertyName = "recordingType")]
        [XmlElement(ElementName = "recordingType", IsNullable = true)]
        public WebAPI.Models.ConditionalAccess.KalturaRecordingType? RecordingType { get; set; }
    }
}