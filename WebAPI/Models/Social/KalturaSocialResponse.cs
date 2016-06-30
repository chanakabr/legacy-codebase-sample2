using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;
using WebAPI.Models.General;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;

namespace WebAPI.Models.Social
{
    [OldStandard("userId", "user_id")]
    [OldStandard("kalturaUsername", "kaltura_username")]
    [OldStandard("socialUsername", "social_username")]
    [OldStandard("minFriendsLimitation", "min_friends_limitation")]
    [OldStandard("socialUser", "social_user")]
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
        [DataMember(Name = "userId")]
        [JsonProperty("userId")]
        [XmlElement(ElementName = "userId")]
        public string UserId { get; set; }

        /// <summary>
        /// Kaltura username
        /// </summary>
        [DataMember(Name = "kalturaUsername")]
        [JsonProperty("kalturaUsername")]
        [XmlElement(ElementName = "kalturaUsername")]
        public string KalturaName { get; set; }

        /// <summary>
        /// Facebook username
        /// </summary>
        [DataMember(Name = "socialUsername")]
        [JsonProperty("socialUsername")]
        [XmlElement(ElementName = "socialUsername")]
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
        [DataMember(Name = "minFriendsLimitation")]
        [JsonProperty("minFriendsLimitation")]
        [XmlElement(ElementName = "minFriendsLimitation")]
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
        [DataMember(Name = "socialUser")]
        [JsonProperty("socialUser")]
        [XmlElement(ElementName = "socialUser", IsNullable = true)]
        public KalturaSocialUser SocialUser { get; set; }
    }
}