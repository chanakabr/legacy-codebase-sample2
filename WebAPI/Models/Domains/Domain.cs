using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Domains
{
    public class Domain
    {
        /// <summary>
        /// Domain identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// Domain name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Domain description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// The max number of the devices that can be added to the domain
        /// </summary>
        [DataMember(Name = "devices_limit")]
        [JsonProperty("devices_limit")]
        public int DevicesLimit { get; set; }

        /// <summary>
        /// The max number of the users that can be added to the domain
        /// </summary>
        [DataMember(Name = "users_limit")]
        [JsonProperty("users_limit")]
        public int UsersLimit { get; set; }

        /// <summary>
        /// List of users identifiers that are in the domain
        /// </summary>
        [DataMember(Name = "users")]
        [JsonProperty("users")]
        public List<int> Users { get; set; }

        /// <summary>
        /// List of pending users identifiers for the domain
        /// </summary>
        [DataMember(Name = "pending_users")]
        [JsonProperty("pending_users")]
        public List<int> PendingUsers { get; set; }

        /// <summary>
        /// The domains region identifier
        /// </summary>
        [DataMember(Name = "region_id")]
        [JsonProperty("region_id")]
        public int RegionId { get; set; }
    }
}