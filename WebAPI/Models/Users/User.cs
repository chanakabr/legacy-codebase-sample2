using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// User
    /// </summary>
    public class User
    {
        /// <summary>
        /// User identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Domain identifier
        /// </summary>
        [DataMember(Name = "domain_id")]
        [JsonProperty("domain_id")]
        public int DomainId { get; set; }

        /// <summary>
        /// User basic data
        /// </summary>
        [DataMember(Name = "basic_data")]
        [JsonProperty("basic_data")]
        public UserBasicData BasicDate { get; set; }

        /// <summary>
        /// User dynamic data
        /// </summary>
        [DataMember(Name = "dynamic_data")]
        [JsonProperty("dynamic_data")]
        public Dictionary<string,string> DynamicDate { get; set; }

        /// <summary>
        /// Is the user the domain master
        /// </summary>
        [DataMember(Name = "is_domain_master")]
        [JsonProperty("is_domain_master")]
        public bool IsDomainMaster { get; set; }

        /// <summary>
        /// Suspention state
        /// </summary>
        [DataMember(Name = "suspention_state")]
        [JsonProperty("suspention_state")]
        public DomainSuspentionState SuspentionState { get; set; }
    }
}