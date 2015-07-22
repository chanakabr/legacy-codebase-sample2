using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.Users;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Household details
    /// </summary>
    public class KalturaHousehold
    {
        /// <summary>
        /// Household identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// Household name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Household description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Household external identifier
        /// </summary>
        [DataMember(Name = "external_id")]
        [JsonProperty("external_id")]
        public string ExternalId { get; set; }

        /// <summary>
        /// Household limitation module identifier
        /// </summary>
        [DataMember(Name = "dlm_id")]
        [JsonProperty("dlm_id")]
        public int DlmId { get; set; }

        /// <summary>
        /// The max number of the devices that can be added to the household
        /// </summary>
        [DataMember(Name = "devices_limit")]
        [JsonProperty("devices_limit")]
        public int DevicesLimit { get; set; }

        /// <summary>
        /// The max number of the users that can be added to the household
        /// </summary>
        [DataMember(Name = "users_limit")]
        [JsonProperty("users_limit")]
        public int UsersLimit { get; set; }

        /// <summary>
        /// The max number of concurrent streams in the household
        /// </summary>
        [DataMember(Name = "concurrent_limit")]
        [JsonProperty("concurrent_limit")]
        public int ConcurrentLimit { get; set; }

        /// <summary>
        /// List of users identifiers 
        /// </summary>
        [DataMember(Name = "users")]
        [JsonProperty("users")]
        public List<KalturaSlimUser> Users { get; set; }

        /// <summary>
        /// List of master users identifiers 
        /// </summary>
        [DataMember(Name = "master_users")]
        [JsonProperty("master_users")]
        public List<KalturaSlimUser> MasterUsers { get; set; }

        /// <summary>
        /// List of default users identifiers 
        /// </summary>
        [DataMember(Name = "default_users")]
        [JsonProperty("default_users")]
        public List<KalturaSlimUser> DefaultUsers { get; set; }

        /// <summary>
        /// List of pending users identifiers 
        /// </summary>
        [DataMember(Name = "pending_users")]
        [JsonProperty("pending_users")]
        public List<KalturaSlimUser> PendingUsers { get; set; }

        /// <summary>
        /// The households region identifier
        /// </summary>
        [DataMember(Name = "region_id")]
        [JsonProperty("region_id")]
        public int RegionId { get; set; }

        /// <summary>
        /// Household state
        /// </summary>
        [DataMember(Name = "state")]
        [JsonProperty("state")]
        public KalturaHouseholdState State { get; set; }

        /// <summary>
        /// Is household frequency enabled
        /// </summary>
        [DataMember(Name = "is_frequency_enabled")]
        [JsonProperty("is_frequency_enabled")]
        public bool IsFrequencyEnabled { get; set; }

        /// <summary>
        /// The next time a device is allowed to be removed from the household (epoch)
        /// </summary>
        [DataMember(Name = "frequency_next_device_action")]
        [JsonProperty("frequency_next_device_action")]
        public DateTime FrequencyNextDeviceAction { get; set; }

        /// <summary>
        /// The next time a user is allowed to be removed from the household (epoch)
        /// </summary>
        [DataMember(Name = "frequency_next_user_action")]
        [JsonProperty("frequency_next_user_action")]
        public DateTime FrequencyNextUserAction { get; set; }

        /// <summary>
        /// Household restriction
        /// </summary>
        [DataMember(Name = "restriction")]
        [JsonProperty("restriction")]
        public KalturaHouseholdRestriction Restriction { get; set; }

        /// <summary>
        /// Household home networks
        /// </summary>
        [DataMember(Name = "home_networks")]
        [JsonProperty("home_networks")]
        public List<KalturaHomeNetwork> HomeNetworks{ get; set; }
        
        /// <summary>
        /// Household device families
        /// </summary>
        [DataMember(Name = "device_families")]
        [JsonProperty("device_families")]
        public List<KalturaDeviceFamily> DeviceFamilies { get; set; }
    }
}