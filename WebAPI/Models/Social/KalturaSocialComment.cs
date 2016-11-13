using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Social
{
    public class KalturaSocialComment : KalturaOTTObject
    {
        /// <summary>
        /// Comment header
        /// </summary>
        [DataMember(Name = "header")]
        [JsonProperty(PropertyName = "header")]
        [XmlElement(ElementName = "header")]
        public string Header { get; set; }

        /// <summary>
        /// Comment body
        /// </summary>
        [DataMember(Name = "text")]
        [JsonProperty(PropertyName = "text")]
        [XmlElement(ElementName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Comment creation date
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        public long CreateDate { get; set; }

        /// <summary>
        /// The writer of the comment
        /// </summary>
        [DataMember(Name = "writer")]
        [JsonProperty(PropertyName = "writer")]
        [XmlElement(ElementName = "writer")]
        public string Writer { get; set; }
    }

    public class KalturaSocialNetworkComment : KalturaSocialComment
    {
        /// <summary>
        /// Number of likes
        /// </summary>
        [DataMember(Name = "likeCounter")]
        [JsonProperty(PropertyName = "likeCounter")]
        [XmlElement(ElementName = "likeCounter")]
        public string LikeCounter { get; set; }

        /// <summary>
        /// The URL of the profile picture of the writer
        /// </summary>
        [DataMember(Name = "writerImageUrl")]
        [JsonProperty(PropertyName = "writerImageUrl")]
        [XmlElement(ElementName = "writerImageUrl")]
        public string WriterImageUrl { get; set; }
    }

    public class KalturaTwitterTwit : KalturaSocialNetworkComment
    {
    }

    public class KalturaFacebookPost : KalturaSocialNetworkComment
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

    public class KalturaSocialCommentListResponse : KalturaListResponse
    {
        /// <summary>
        /// Social comments list
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaSocialComment> Objects { get; set; }
    }
}