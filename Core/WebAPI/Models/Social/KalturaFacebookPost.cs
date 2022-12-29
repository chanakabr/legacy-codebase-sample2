using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Social
{
    public partial class KalturaFacebookPost : KalturaSocialNetworkComment
    {
        /// <summary>
        /// List of comments on the post
        /// </summary>
        [DataMember(Name = "comments")]
        [JsonProperty(PropertyName = "comments")]
        [XmlElement(ElementName = "comments")]
        public List<KalturaSocialNetworkComment> Comments { get; set; }

        /// <summary>
        /// A link associated to the post
        /// </summary>
        [DataMember(Name = "link")]
        [JsonProperty(PropertyName = "link")]
        [XmlElement(ElementName = "link")]
        public string Link { get; set; }
    }
}