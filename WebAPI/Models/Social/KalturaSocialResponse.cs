using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;
using WebAPI.Models.General;
using System.Xml.Serialization;

namespace WebAPI.Models.Social
{
    public class KalturaSocialResponse : KalturaOTTObject
    {
        /// <summary>
        /// User model status
        /// Possible values: UNKNOWN, OK, ERROR, NOACTION, NOTEXIST, CONFLICT, MERGE, MERGEOK, NEWUSER, MINFRIENDS, INVITEOK, INVITEERROR, ACCESSDENIED, WRONGPASSWORDORUSERNAME, UNMERGEOK
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        public string Status { get; set; }

        /// <summary>
        /// User identifier
        /// </summary>
        [DataMember(Name = "user_id")]
        [JsonProperty("user_id")]
        [XmlElement(ElementName = "user_id")]
        public string UserId { get; set; }

        /// <summary>
        /// Kaltura username
        /// </summary>
        [DataMember(Name = "kaltura_username")]
        [JsonProperty("kaltura_username")]
        [XmlElement(ElementName = "kaltura_username")]
        public string KalturaName { get; set; }

        /// <summary>
        /// Facebook username
        /// </summary>
        [DataMember(Name = "social_username")]
        [JsonProperty("social_username")]
        [XmlElement(ElementName = "social_username")]
        public string SocialNetworkUsername { get; set; }

        /// <summary>
        /// Facebook profile picture
        /// </summary>
        [DataMember(Name = "pic")]
        [JsonProperty("pic")]
        [XmlElement(ElementName = "pic")]
        public string Pic { get; set; }

        /// <summary>
        /// Data
        /// </summary>
        [DataMember(Name = "data")]
        [JsonProperty("data")]
        [XmlElement(ElementName = "data")]
        public string Data { get; set; }

        /// <summary>
        /// Minimum number of friends limitation
        /// </summary>
        [DataMember(Name = "min_friends_limitation")]
        [JsonProperty("min_friends_limitation")]
        [XmlElement(ElementName = "min_friends_limitation")]
        public string MinFriends { get; set; }

        /// <summary>
        /// Facebook encrypted token
        /// </summary>
        [DataMember(Name = "token")]
        [JsonProperty("token")]
        [XmlElement(ElementName = "token")]
        public string Token { get; set; }

        /// <summary>
        /// Facebook user object
        /// </summary>
        [DataMember(Name = "social_user")]
        [JsonProperty("social_user")]
        [XmlElement(ElementName = "social_user", IsNullable = true)]
        public KalturaSocialUser SocialUser { get; set; }
    }
}