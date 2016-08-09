using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// List of inbox message.
    /// </summary>
    [DataContract(Name = "KalturaTopicResponse", Namespace = "")]
    [XmlRoot("KalturaTopicResponse")]
    [Obsolete]
    public class KalturaTopicResponse : KalturaListResponse
    {
        /// <summary>
        /// Follow data list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaTopic> Topics{ get; set; }
    }

    /// <summary>
    /// List of inbox message.
    /// </summary>
    [DataContract(Name = "KalturaTopicListResponse", Namespace = "")]
    [XmlRoot("KalturaTopicListResponse")]
    public class KalturaTopicListResponse : KalturaListResponse
    {
        /// <summary>
        /// Follow data list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaTopic> Topics { get; set; }
    }
}