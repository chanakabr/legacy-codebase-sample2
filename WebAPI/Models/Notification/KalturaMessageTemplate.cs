using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    [Serializable]
    [OldStandard("dateFormat", "date_format")]
    [OldStandard("assetType", "asset_type")]
    public class KalturaMessageTemplate : KalturaOTTObject
    {
        /// <summary>
        ///The actual message with placeholders to be presented to the user
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty(PropertyName = "message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        [DataMember(Name = "dateFormat")]
        [JsonProperty(PropertyName = "dateFormat")]
        [XmlElement(ElementName = "dateFormat")]
        public string DateFormat { get; set; }

        /// <summary>
        /// OTT asset type
        /// Possible values: Series
        /// </summary>
        [DataMember(Name = "assetType")]
        [JsonProperty(PropertyName = "assetType")]
        [XmlElement(ElementName = "assetType")]
        public KalturaOTTAssetType AssetType { get; set; }

        [DataMember(Name = "sound")]
        [JsonProperty(PropertyName = "sound")]
        [XmlElement(ElementName = "sound")]
        public string Sound { get; set; }

        [DataMember(Name = "action")]
        [JsonProperty(PropertyName = "action")]
        [XmlElement(ElementName = "action")]
        public string Action { get; set; }

        [DataMember(Name = "url")]
        [JsonProperty(PropertyName = "url")]
        [XmlElement(ElementName = "url")]
        public string URL { get; set; }
    }
}