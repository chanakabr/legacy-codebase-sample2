using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    [Serializable]
    [OldStandard("dateFormat", "date_format")]
    [OldStandard("assetType", "asset_type")]
    public class KalturaMessageTemplate : KalturaOTTObject
    {
        /// <summary>
        ///The message template with placeholders
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty(PropertyName = "message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Default date format for the date &amp; time entries used in the template
        /// </summary>
        [DataMember(Name = "dateFormat")]
        [JsonProperty(PropertyName = "dateFormat")]
        [XmlElement(ElementName = "dateFormat")]
        public string DateFormat { get; set; }

        /// <summary>
        /// Template type. Possible values: Series
        /// </summary>
        [DataMember(Name = "assetType")]
        [JsonProperty(PropertyName = "assetType")]
        [XmlElement(ElementName = "assetType")]
        public KalturaOTTAssetType AssetType { get; set; }

        /// <summary>
        /// Sound file name to play upon message arrival to the device (if supported by target device)
        /// </summary>
        [DataMember(Name = "sound")]
        [JsonProperty(PropertyName = "sound")]
        [XmlElement(ElementName = "sound")]
        public string Sound { get; set; }

        /// <summary>
        /// an optional action
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty(PropertyName = "action")]
        [XmlElement(ElementName = "action")]
        public string Action { get; set; }

        /// <summary>
        /// URL template for deep linking. Example - /app/location/{mediaId}
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty(PropertyName = "url")]
        [XmlElement(ElementName = "url")]
        public string URL { get; set; }
    }
}