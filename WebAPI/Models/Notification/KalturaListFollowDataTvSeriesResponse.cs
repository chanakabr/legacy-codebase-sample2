using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// List of message follow data.
    /// </summary>
    [DataContract(Name = "KalturaListFollowDataResponse", Namespace = "")]
    [XmlRoot("KalturaListFollowDataResponse")]
    [Obsolete]
    public class KalturaListFollowDataTvSeriesResponse : KalturaListResponse
    {
        /// <summary>
        /// Follow data list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaFollowDataTvSeries> FollowDataList { get; set; }
    }
    
    /// <summary>
    /// List of message follow data.
    /// </summary>
    [DataContract(Name = "KalturaFollowTvSeriesListResponse", Namespace = "")]
    [XmlRoot("KalturaFollowTvSeriesListResponse")]
    public class KalturaFollowTvSeriesListResponse : KalturaListResponse
    {
        /// <summary>
        /// Follow data list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaFollowTvSeries> FollowDataList { get; set; }
    }
    
}