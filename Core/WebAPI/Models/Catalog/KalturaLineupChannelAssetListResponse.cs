using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaLineupChannelAssetListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of objects
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaLineupChannelAsset> Objects { get; set; }

        /// <summary>
        /// Lineup External Id
        /// </summary>
        [DataMember(Name = "lineupExternalId")]
        [JsonProperty("lineupExternalId")]
        [XmlElement(ElementName = "lineupExternalId", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string LineupExternalId { get; set; }

        /// <summary>
        /// Parent Lineup External Id
        /// </summary>
        [DataMember(Name = "parentLineupExternalId")]
        [JsonProperty("parentLineupExternalId")]
        [XmlElement(ElementName = "parentLineupExternalId", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string ParentLineupExternalId { get; set; }

        public KalturaLineupChannelAssetListResponse()
        {
        }

        public KalturaLineupChannelAssetListResponse(KalturaLineupChannelAssetListResponse response)
        {
            Objects = response.Objects;
            TotalCount = response.TotalCount;
            LineupExternalId = response.LineupExternalId;
            ParentLineupExternalId = response.ParentLineupExternalId;
        }
    }
}
