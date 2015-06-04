using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// User type
    /// </summary>
    public class UserType
    {
        /// <summary>
        /// Type identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Type description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}