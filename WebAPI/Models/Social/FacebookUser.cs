using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;

namespace WebAPI.Models.Social
{
    public class FacebookUser
    {
        /// <summary>
        /// Facebook identifier
        /// </summary>
        [DataMember(Name = "facebook_id")]
        [JsonProperty("facebook_id")]
        public string FacebookId { get; set; }

        /// <summary>
        /// Full name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        [DataMember(Name = "first_name")]
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [DataMember(Name = "last_name")]
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        /// <summary>
        /// User email
        /// </summary>
        [DataMember(Name = "email")]
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Gender
        /// </summary>
        [DataMember(Name = "gender")]
        [JsonProperty("gender")]
        public string Gender { get; set; }

        /// <summary>
        /// User identifier
        /// </summary>
        [DataMember(Name = "user_id")]
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        /// <summary>
        /// User birthday
        /// </summary>
        [DataMember(Name = "birthday")]
        [JsonProperty("birthday")]
        public string Birthday { get; set; }
    }
}