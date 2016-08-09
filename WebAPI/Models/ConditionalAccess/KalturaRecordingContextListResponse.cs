using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Recordings context info wrapper
    /// </summary>
    [Serializable]
    [Obsolete]
    public class KalturaRecordingContextListResponse : KalturaListResponse
    {
        /// <summary>
        /// Recording contexts
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaRecordingContext> Objects { get; set; }
    }
}