using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog.GroupRepresentatives
{
    public partial class KalturaTopRsp : KalturaRepresentativeSelectionPolicy
    {
        /// <summary>
        /// order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaBaseAssetOrder OrderBy { get; set; }
    }
}