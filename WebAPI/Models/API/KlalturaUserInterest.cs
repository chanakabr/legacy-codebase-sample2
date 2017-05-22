using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Asset meta
    /// </summary>
    public class KlalturaUserInterest : KalturaOTTObject
    {
        /// <summary>
        /// Meta name for the partner
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty("userId")]
        [XmlElement(ElementName = "userId")]
        public string UserId { get; set; }


    }

    /// <summary>
    /// Asset meta
    /// </summary>
    public class KlalturaUserInterestTag : KlalturaUserInterest
    {
        /// <summary>
        /// Meta name for the partner
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
    }

    /// <summary>
    /// Asset meta
    /// </summary>
    public class KlalturaUserInterestMeta : KlalturaUserInterest
    {
        /// <summary>
        /// Meta name for the partner
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
    }
}