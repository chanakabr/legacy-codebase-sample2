using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Kaltura bookmark data
    /// </summary>
    [Serializable]
    public class KalturaPlayerAssetData : KalturaOTTObject
    {
        [DataMember(Name = "action")]
        [JsonProperty(PropertyName = "action")]
        [XmlArrayItem(ElementName = "action")]
        public string action;

        [DataMember(Name = "location")]
        [JsonProperty(PropertyName = "location")]
        [XmlArrayItem(ElementName = "location")]
        public int location;

        [DataMember(Name = "averageBitRate")]
        [JsonProperty(PropertyName = "averageBitRate")]
        [XmlArrayItem(ElementName = "averageBitRate")]
        public int averageBitRate;

        [DataMember(Name = "totalBitRate")]
        [JsonProperty(PropertyName = "totalBitRate")]
        [XmlArrayItem(ElementName = "totalBitRate")]
        public int totalBitRate;

        [DataMember(Name = "currentBitRate")]
        [JsonProperty(PropertyName = "currentBitRate")]
        [XmlArrayItem(ElementName = "currentBitRate")]
        public int currentBitRate;
    }
}