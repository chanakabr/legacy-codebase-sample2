using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// List of segmentation types
    /// </summary>
    [DataContract(Name = "KalturaSegmentationTypeListResponse", Namespace = "")]
    [XmlRoot("KalturaSegmentationTypeListResponse")]
    public partial class KalturaSegmentationTypeListResponse : KalturaListResponse
    {
        /// <summary>
        /// Segmentation Types
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [SchemeProperty()]
        public List<KalturaSegmentationType> SegmentationTypes { get; set; }
    }    
}