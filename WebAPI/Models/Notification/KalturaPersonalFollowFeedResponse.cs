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
    /// <summary>
    /// List of message follow data.
    /// </summary>
    [DataContract(Name = "KalturaPersonalFollowFeedResponse", Namespace = "")]
    [XmlRoot("KalturaPersonalFollowFeedResponse")]
    public class KalturaPersonalFollowFeedResponse : KalturaListResponse
    {
        /// <summary>
        /// Follow data list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaPersonalFollowFeed> PersonalFollowFeed { get; set; }
    }
}