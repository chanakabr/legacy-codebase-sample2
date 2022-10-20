using WebAPI.Models.General;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Segmentation
{
    public abstract partial class KalturaBaseSegmentationTypeFilter : KalturaFilter<KalturaSegmentationTypeOrderBy>
    {
        /// <summary>
        /// order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(IsNullable = true)]
        public new KalturaSegmentationTypeOrderBy? OrderBy { get; set; }

        public override KalturaSegmentationTypeOrderBy GetDefaultOrderByValue()
        {
            return KalturaSegmentationTypeOrderBy.UPDATE_DATE_DESC;
        }
    }
}
