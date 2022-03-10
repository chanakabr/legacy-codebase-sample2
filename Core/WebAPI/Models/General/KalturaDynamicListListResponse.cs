using ApiObjects.BulkUpload;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.App_Start;
using WebAPI.Exceptions;

namespace WebAPI.Models.General
{
    public partial class KalturaDynamicListListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of KalturaDynamicList
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaDynamicList> Objects { get; set; }
    }
}
