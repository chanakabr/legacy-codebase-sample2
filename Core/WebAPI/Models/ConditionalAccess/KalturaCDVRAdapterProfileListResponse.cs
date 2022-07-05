using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// C-DVR adapter profiles
    /// </summary>
    [Serializable]
    public partial class KalturaCDVRAdapterProfileListResponse : KalturaListResponse
    {
        /// <summary>
        /// C-DVR adapter profiles
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaCDVRAdapterProfile> Objects { get; set; }
    }
}
