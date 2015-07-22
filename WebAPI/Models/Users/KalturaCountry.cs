using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Country details
    /// </summary>
    public class KalturaCountry
    {
        /// <summary>
        /// Country identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Country name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public int Name { get; set; }

        /// <summary>
        /// Country code
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        public int Code { get; set; }
    }
}