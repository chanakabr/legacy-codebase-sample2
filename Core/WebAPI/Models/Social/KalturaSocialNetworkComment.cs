using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Social
{
    public partial class KalturaSocialNetworkComment : KalturaSocialComment
    {
        /// <summary>
        /// Number of likes
        /// </summary>
        [DataMember(Name = "likeCounter")]
        [JsonProperty(PropertyName = "likeCounter")]
        [XmlElement(ElementName = "likeCounter")]
        public string LikeCounter { get; set; }

        /// <summary>
        /// The URL of the profile picture of the author of the comment
        /// </summary>
        [DataMember(Name = "authorImageUrl")]
        [JsonProperty(PropertyName = "authorImageUrl")]
        [XmlElement(ElementName = "authorImageUrl")]
        public string AuthorImageUrl { get; set; }
    }
}