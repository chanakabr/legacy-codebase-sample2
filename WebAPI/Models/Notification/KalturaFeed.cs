using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    [Serializable]
    public class KalturaFeed : KalturaOTTObject
    {
        /// <summary>
        /// Asset identifier
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty(PropertyName = "assetId")]
        [XmlElement(ElementName = "assetId")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("asset_id")]
        public long AssetId{ get; set; }
       
    }
}