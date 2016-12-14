using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Notifications
{
    [Serializable]
    public class KalturaAssetReminder : KalturaReminder
    {
        [DataMember(Name = "assetId")]
        [JsonProperty(PropertyName = "assetId")]
        [XmlElement(ElementName = "assetId")]
        public long AssetId { get; set; }
    }
}