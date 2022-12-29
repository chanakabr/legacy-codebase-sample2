using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Media file in discovery context
    /// </summary>
    [Serializable]
    public partial class KalturaDiscoveryMediaFile : KalturaMediaFile
    {
        /// <summary>
        /// show, if file could be played 
        /// </summary>
        [DataMember(Name = "isPlaybackable")]
        [JsonProperty("isPlaybackable")]
        [XmlArray(ElementName = "isPlaybackable")]
        public bool IsPlaybackable { get; set; }
    }
}