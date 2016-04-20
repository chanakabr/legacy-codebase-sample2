using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.Notification
{
    [Serializable]
    public class KalturaFollowDataTvSeries : KalturaFollowDataBase
    {
        /// <summary>
        /// Asset Id
        /// </summary>
        [DataMember(Name = "asset_id")]
        [JsonProperty(PropertyName = "asset_id")]
        [XmlElement(ElementName = "asset_id")]
        public int AssetId { get; set; }
    }
}