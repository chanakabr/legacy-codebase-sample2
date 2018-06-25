using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    /// <summary>
    /// Asset wrapper
    /// </summary>
    [Serializable]
    public class KalturaBulkUploadListResponse : KalturaListResponse
    {
        /// <summary>
        /// bulk uploads
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaBulkUpload> Objects { get; set; }
    }
}