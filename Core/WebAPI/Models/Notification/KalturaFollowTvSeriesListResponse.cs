using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// List of message follow data.
    /// </summary>
    [DataContract(Name = "KalturaFollowTvSeriesListResponse", Namespace = "")]
    [XmlRoot("KalturaFollowTvSeriesListResponse")]
    public partial class KalturaFollowTvSeriesListResponse : KalturaListResponse
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