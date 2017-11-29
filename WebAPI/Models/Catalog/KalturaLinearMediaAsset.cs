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
    /// Linear media asset info
    /// </summary>
    [Serializable]
    public class KalturaLinearMediaAsset : KalturaMediaAsset
    {
        /// <summary>
        /// Id of epg channel
        /// </summary>
        [DataMember(Name = "epgChannelId")]
        [JsonProperty(PropertyName = "epgChannelId")]
        [XmlElement(ElementName = "epgChannelId")]
        public long? EpgChannelId { get; set; }
    }
}