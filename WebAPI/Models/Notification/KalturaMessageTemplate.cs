using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    [Serializable]
    public class KalturaMessageTemplate : KalturaOTTObject
    {
        /// <summary>
        ///The actual message with placeholders to be presented to the user
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty(PropertyName = "message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        [DataMember(Name = "Date_Format")]
        [JsonProperty(PropertyName = "Date_Format")]
        [XmlElement(ElementName = "Date_Format")]
        public string DateFormat { get; set; }

        /// <summary>
        /// OTT asset type
        /// Possible values: Series
        /// </summary>
        [DataMember(Name = "asset_type")]
        [JsonProperty(PropertyName = "asset_type")]
        [XmlElement(ElementName = "asset_type")]
        public KalturaOTTAssetType AssetType { get; set; }        
    }
}