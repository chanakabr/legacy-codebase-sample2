using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Social
{
    public partial class KalturaSocialFriendActivity : KalturaOTTObject
    {
        /// <summary>
        /// The full name of the user who did the social action
        /// </summary>
        [DataMember(Name = "userFullName")]
        [JsonProperty("userFullName")]
        [XmlElement(ElementName = "userFullName")]
        public string UserFullName { get; set; }

        /// <summary>
        /// The URL of the profile picture of the user who did the social action
        /// </summary>
        [DataMember(Name = "userPictureUrl")]
        [JsonProperty("userPictureUrl")]
        [XmlElement(ElementName = "userPictureUrl")]
        public string UserPictureUrl { get; set; }

        /// <summary>
        /// The social action
        /// </summary>
        [DataMember(Name = "socialAction")]
        [JsonProperty("socialAction")]
        [XmlElement(ElementName = "socialAction")]
        public KalturaSocialAction SocialAction { get; set; }
    }

    public partial class KalturaSocialFriendActivityListResponse: KalturaListResponse
    {
        /// <summary>
        /// Social friends activity
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaSocialFriendActivity> Objects { get; set; }
    }
}