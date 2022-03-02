using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
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

        public KalturaLineupChannelAssetListResponse(IEnumerable<KalturaLineupChannelAsset> channelAssets, int totalCount)
        {
            Objects = channelAssets.ToList();
            TotalCount = totalCount;
        }
    }
}