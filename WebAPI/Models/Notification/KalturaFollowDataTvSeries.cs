using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    [Serializable]
    [Obsolete]
    public class KalturaFollowDataTvSeries : KalturaFollowDataBase
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

    public class KalturaFollowTvSeries : KalturaFollowDataBase
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

    public class KalturaFollowTvSeriesFilter : KalturaFilter<KalturaFollowTvSeriesOrderBy>
    {
        public override KalturaFollowTvSeriesOrderBy GetDefaultOrderByValue()
        {
            return KalturaFollowTvSeriesOrderBy.START_DATE_DESC;
        }
    }
}