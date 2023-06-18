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
    /// Asset info wrapper
    /// </summary>
    [Serializable]
    [Obsolete]
    public partial class KalturaAssetInfoListResponse : KalturaListResponse
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetInfo> Objects { get; set; }

        [DataMember(Name = "requestId")]
        [JsonProperty(PropertyName = "requestId")]
        [XmlElement("requestId", IsNullable = true)]
        [OldStandardProperty("request_id")]
        public string RequestId { get; set; }
    }
}
