using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
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

    /// <summary>
    /// Filter for segmentation types
    /// </summary>
    public partial class KalturaSegmentationTypeFilter : KalturaFilter<KalturaSegmentationTypeOrder>
    {
        public override KalturaSegmentationTypeOrder GetDefaultOrderByValue()
        {
            return KalturaSegmentationTypeOrder.NONE;
        }
    }

    /// <summary>
    /// Segmentation types order
    /// </summary>
    public enum KalturaSegmentationTypeOrder
    {
        NONE
    }
}