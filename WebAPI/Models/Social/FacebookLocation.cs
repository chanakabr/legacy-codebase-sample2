using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;

namespace WebAPI.Models.Social
{
    public class FacebookLocation
    {
        /// <summary>
        /// name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}