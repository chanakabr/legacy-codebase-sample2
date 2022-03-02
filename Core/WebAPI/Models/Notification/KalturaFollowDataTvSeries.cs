using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    [Serializable]
    [Obsolete]
    public partial class KalturaFollowDataTvSeries : KalturaFollowDataBase
    {
        /// <summary>
        /// Asset Id
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty(PropertyName = "assetId")]
        [XmlElement(ElementName = "assetId")]
        [OldStandardProperty("asset_id")]
        public int AssetId { get; set; }
    }

    public partial class KalturaFollowTvSeries : KalturaFollowDataBase
    {
        /// <summary>
        /// Asset Id
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty(PropertyName = "assetId")]
        [XmlElement(ElementName = "assetId")]
        [SchemeProperty(MinInteger = 1)]
        public int AssetId { get; set; }
    }

    public enum KalturaFollowTvSeriesOrderBy
    {
        START_DATE_DESC,
        START_DATE_ASC
    }

    public partial class KalturaFollowTvSeriesFilter : KalturaFilter<KalturaFollowTvSeriesOrderBy>
    {
        public override KalturaFollowTvSeriesOrderBy GetDefaultOrderByValue()
        {
            return KalturaFollowTvSeriesOrderBy.START_DATE_DESC;
        }
    }
}