using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    [Serializable]
    public class KalturaFeed : KalturaOTTObject
    {
        [DataMember(Name = "asset_id")]
        [JsonProperty(PropertyName = "asset_id")]
        [XmlElement(ElementName = "asset_id")]
        public long AssetId{ get; set; }
       
    }
}