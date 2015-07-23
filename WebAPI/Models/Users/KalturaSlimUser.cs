using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Slim user data
    /// </summary>
    public class KalturaSlimUser : KalturaOTTObject
    {
        /// <summary>
        /// User identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        [DataMember(Name = "username")]
        [JsonProperty("username")]
        public string Username { get; set; }

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
    }
}