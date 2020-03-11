using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    /// <summary>
    /// Asset wrapper
    /// </summary>
    [Serializable]
    public partial class KalturaBulkUploadListResponse : KalturaListResponse
    {
        /// <summary>
        /// bulk upload items
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaBulkUpload> Objects { get; set; }
    }
}