using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;

namespace WebAPI.Models.Social
{
    public class FacebookInterestData
    {
        /// <summary>
        /// name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// category
        /// </summary>
        [DataMember(Name = "category")]
        [JsonProperty("category")]
        public string Category { get; set; }

        /// <summary>
        /// identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// created time
        /// </summary>
        [DataMember(Name = "created_time")]
        [JsonProperty("created_time")]
        public string CreatedTime { get; set; }
    }
}