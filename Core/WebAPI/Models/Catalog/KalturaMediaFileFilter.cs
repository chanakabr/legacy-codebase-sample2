using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaMediaFileFilter : KalturaFilter<KalturaMediaFileOrderBy>
    {
        /// <summary>
        /// Asset identifier to filter by
        /// </summary>
        [DataMember(Name = "assetIdEqual")]
        [JsonProperty("assetIdEqual")]
        [XmlElement(ElementName = "assetIdEqual")]
        public long AssetIdEqual { get; set; }

        /// <summary>
        /// Asset file identifier to filter by
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual")]
        public long IdEqual { get; set; }

        public override KalturaMediaFileOrderBy GetDefaultOrderByValue()
        {
            return KalturaMediaFileOrderBy.NONE;
        }
    }
}