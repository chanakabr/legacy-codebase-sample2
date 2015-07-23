using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;

namespace WebAPI.Models.Social
{
    public class ClientFacebookResponse
    {
        /// <summary>
        /// User model status
        /// Possible values: UNKNOWN, OK, ERROR, NOACTION, NOTEXIST, CONFLICT, MERGE, MERGEOK, NEWUSER, MINFRIENDS, INVITEOK, INVITEERROR, ACCESSDENIED, WRONGPASSWORDORUSERNAME, UNMERGEOK
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// User identifier
        /// </summary>
        [DataMember(Name = "user_id")]
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        /// <summary>
        /// Kaltura username
        /// </summary>
        [DataMember(Name = "kaltura_username")]
        [JsonProperty("kaltura_username")]
        public string KalturaName { get; set; }

        /// <summary>
        /// Facebook username
        /// </summary>
        [DataMember(Name = "facebook_username")]
        [JsonProperty("facebook_username")]
        public string FacebookName { get; set; }

        /// <summary>
        /// Facebook profile picture
        /// </summary>
        [DataMember(Name = "pic")]
        [JsonProperty("pic")]
        public string Pic { get; set; }

        /// <summary>
        /// Data
        /// </summary>
        [DataMember(Name = "data")]
        [JsonProperty("data")]
        public string Data { get; set; }

        /// <summary>
        /// Minimum number of friends limitation
        /// </summary>
        [DataMember(Name = "min_friends_limitation")]
        [JsonProperty("min_friends_limitation")]
        public string MinFriends { get; set; }

        /// <summary>
        /// Facebook encrypted token
        /// </summary>
        [DataMember(Name = "token")]
        [JsonProperty("token")]
        public string Token { get; set; }

        /// <summary>
        /// Facebook user object
        /// </summary>
        [DataMember(Name = "facebook_user")]
        [JsonProperty("facebook_user")]
        public FacebookUser FacebookUser { get; set; }
    }
}