using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Watch history asset wrapper
    /// </summary>
    [Serializable]
    public partial class KalturaStreamingDeviceListResponse : KalturaListResponse
    {
        /// <summary>
        /// Streaming devices
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaStreamingDevice> Objects { get; set; }
    }

    /// <summary>
    /// Watch history asset info
    /// </summary>
    [Serializable]
    public partial class KalturaStreamingDevice : KalturaOTTObject
    {
        /// <summary>
        /// Asset 
        /// </summary>
        [DataMember(Name = "asset")]
        [JsonProperty(PropertyName = "asset")]
        [XmlElement(ElementName = "asset")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaSlimAsset Asset { get; set; }

        /// <summary>
        ///User identifier
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty("userId")]
        [XmlElement(ElementName = "userId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public string UserId { get; set; }

        /// <summary>
        /// Device UDID
        /// </summary>
        [DataMember(Name = "udid")]
        [JsonProperty("udid")]
        [XmlElement(ElementName = "udid")]
        [SchemeProperty(InsertOnly = true)]
        public string Udid { get; set; }
    }
}