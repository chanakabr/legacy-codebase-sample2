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
        /// Domain external identifier
        /// </summary>
        [DataMember(Name = "external_id")]
        [JsonProperty("external_id")]
        public string ExternalId { get; set; }

        /// <summary>
        /// Domain limitation module identifier
        /// </summary>
        [DataMember(Name = "dlm_id")]
        [JsonProperty("dlm_id")]
        public int DlmId { get; set; }

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
        /// The max number of concurrent streams in the domain
        /// </summary>
        [DataMember(Name = "concurrent_limit")]
        [JsonProperty("concurrent_limit")]
        public int ConcurrentLimit { get; set; }

        /// <summary>
        /// List of users identifiers 
        /// </summary>
        [DataMember(Name = "users")]
        [JsonProperty("users")]
        public List<int> Users { get; set; }

        /// <summary>
        /// List of master users identifiers 
        /// </summary>
        [DataMember(Name = "master_users")]
        [JsonProperty("master_users")]
        public List<int> MasterUsers { get; set; }

        /// <summary>
        /// List of default users identifiers 
        /// </summary>
        [DataMember(Name = "default_users")]
        [JsonProperty("default_users")]
        public List<int> DefaultUsers { get; set; }

        /// <summary>
        /// List of pending users identifiers 
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

        /// <summary>
        /// Domain state
        /// </summary>
        [DataMember(Name = "state")]
        [JsonProperty("state")]
        public DomainState State { get; set; }

        /// <summary>
        /// Domain restriction
        /// </summary>
        [DataMember(Name = "restriction")]
        [JsonProperty("restriction")]
        public DomainRestriction Restriction { get; set; }

        /// <summary>
        /// Domain home networks
        /// </summary>
        [DataMember(Name = "home_networks")]
        [JsonProperty("home_networks")]
        public List<HomeNetwork> HomeNetworks{ get; set; }
    }
}