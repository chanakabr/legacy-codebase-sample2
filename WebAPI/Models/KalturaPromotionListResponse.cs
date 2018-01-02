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
    /// Prices list
    /// </summary>
    [DataContract(Name = "KalturaPromotionListResponse", Namespace = "")]
    [XmlRoot("KalturaPromotionListResponse")]
    public class KalturaPromotionListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of promotions
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaPromotion> Promotions { get; set; }
    }

    public class KalturaPromotionFilter : KalturaFilter<KalturaPromotionOrderBy>
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

        public override KalturaPromotionOrderBy GetDefaultOrderByValue()
        {
            return KalturaPromotionOrderBy.none;
        }
    }

    public enum KalturaPromotionOrderBy
    {
        none
    }
}