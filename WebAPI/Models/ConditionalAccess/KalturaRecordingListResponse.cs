using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess 
{
    /// <summary>
    /// Recordings info wrapper
    /// </summary>
    [Serializable]
    public class KalturaRecordingListResponse : KalturaListResponse
    {
        /// <summary>
        /// Recordings
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaRecordingAsset> Objects { get; set; }

    }

        /// <summary>
    /// Recording Info
    /// </summary>
    [Serializable]
    public class KalturaRecordingAsset : KalturaRecording
    {
        /// <summary>
        /// Asset
        /// </summary>
        [DataMember(Name = "asset")]
        [JsonProperty("asset")]
        [XmlElement(ElementName = "asset", IsNullable = true)]
        public KalturaAssetInfo Asset { get; set; }


    }

}