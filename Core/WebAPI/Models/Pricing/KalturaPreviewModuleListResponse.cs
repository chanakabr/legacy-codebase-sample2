using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{

    public partial class KalturaPreviewModuleListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of Preview Module
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaPreviewModule> PreviewModule { get; set; }
    }
}