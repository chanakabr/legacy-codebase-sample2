using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models
{
    /// <summary>
    /// KalturaCuePointListResponse
    /// </summary>
    [DataContract(Name = "KalturaCuePointListResponse", Namespace = "")]
    [XmlRoot("KalturaCuePointListResponse")]
    public class KalturaCuePointListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of promotions
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaCuePoint> CuePoints { get; set; }
    }

    public class KalturaCuePointFilter : KalturaFilter<KalturaCuePointOrderBy>
    {
        /// <summary>
        /// Asset ID  
        /// </summary>
        [DataMember(Name = "assetIdEqual")]
        [JsonProperty("assetIdEqual")]
        [XmlElement(ElementName = "assetIdEqual")]
        public long AssetIdEqual { get; set; }

        /// <summary>
        /// SavedEqual  
        /// </summary>
        [DataMember(Name = "savedEqual")]
        [JsonProperty("savedEqual")]
        [XmlElement(ElementName = "savedEqual")]
        public bool SavedEqual { get; set; }

        public override KalturaCuePointOrderBy GetDefaultOrderByValue()
        {
            return KalturaCuePointOrderBy.none;
        }
    }

    public enum KalturaCuePointOrderBy
    {
        none
    }
}