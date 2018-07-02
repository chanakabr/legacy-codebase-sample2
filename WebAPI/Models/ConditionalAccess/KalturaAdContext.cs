using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaAdsContext : KalturaOTTObject
    {
        /// <summary>
        /// Sources
        /// </summary>
        [DataMember(Name = "sources")]
        [JsonProperty("sources")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAdsSource> Sources { get; set; }
    }

    public partial class KalturaAdsSource : KalturaOTTObject
    {
        /// <summary>
        /// File unique identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        /// <summary>
        /// Device types as defined in the system
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Ads policy 
        /// </summary>
        [DataMember(Name = "adsPolicy")]
        [JsonProperty("adsPolicy")]
        [XmlElement(ElementName = "adsPolicy")]
        public KalturaAdsPolicy? AdsPolicy { get; set; }

        /// <summary>
        /// The parameters to pass to the ads server 
        /// </summary>
        [DataMember(Name = "adsParam")]
        [JsonProperty("adsParam")]
        [XmlElement(ElementName = "adsParam")]
        public string AdsParams { get; set; }
    }
}