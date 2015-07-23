using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Premium service
    /// </summary>
    public class KalturaPremiumService : KalturaOTTObject
    {
        /// <summary>
        /// Service identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// Service name / description
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}