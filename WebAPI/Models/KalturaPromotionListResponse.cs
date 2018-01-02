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
        /// EntryId  
        /// </summary>
        [DataMember(Name = "entryId")]
        [JsonProperty("entryId")]
        [XmlElement(ElementName = "entryId")]
        public int EntryId { get; set; }

        /// <summary>
        /// PartnerId  
        /// </summary>
        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        public int PartnerId { get; set; }

        /// <summary>
        /// UiConfId  
        /// </summary>
        [DataMember(Name = "uiConfId")]
        [JsonProperty("uiConfId")]
        [XmlElement(ElementName = "uiConfId")]
        public int UiConfId { get; set; }

        /// <summary>
        /// A list of promotions
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaPromotion> Promotions { get; set; }
    }
}