using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;
using WebAPI.Models.Users;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Household details
    /// </summary>
    public class KalturaHousehold : KalturaOTTObject
    {
        /// <summary>
        /// Household identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public long Id { get; set; }

        /// <summary>
        /// Household name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Household description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Household external identifier
        /// </summary>
        [DataMember(Name = "external_id")]
        [JsonProperty("external_id")]
        [XmlElement(ElementName = "external_id")]
        public string ExternalId { get; set; }

        /// <summary>
        /// Household limitation module identifier
        /// </summary>
        [DataMember(Name = "dlm_id")]
        [JsonProperty("dlm_id")]
        [XmlElement(ElementName = "dlm_id")]
        public int DlmId { get; set; }

        /// <summary>
        /// The max number of the devices that can be added to the household
        /// </summary>
        [DataMember(Name = "devices_limit")]
        [JsonProperty("devices_limit")]
        [XmlElement(ElementName = "devices_limit")]
        public int DevicesLimit { get; set; }

        /// <summary>
        /// The max number of the users that can be added to the household
        /// </summary>
        [DataMember(Name = "users_limit")]
        [JsonProperty("users_limit")]
        [XmlElement(ElementName = "users_limit")]
        public int UsersLimit { get; set; }

        /// <summary>
        /// The max number of concurrent streams in the household
        /// </summary>
        [DataMember(Name = "concurrent_limit")]
        [JsonProperty("concurrent_limit")]
        [XmlElement(ElementName = "concurrent_limit")]
        public int ConcurrentLimit { get; set; }

        /// <summary>
        /// List of users identifiers 
        /// </summary>
        [DataMember(Name = "users")]
        [JsonProperty("users")]
        [XmlArray(ElementName = "users")]
        [XmlArrayItem("item")] 
        public List<KalturaBaseOTTUser> Users { get; set; }

        /// <summary>
        /// List of master users identifiers 
        /// </summary>
        [DataMember(Name = "master_users")]
        [JsonProperty("master_users")]
        [XmlArray(ElementName = "master_users")]
        [XmlArrayItem("item")] 
        public List<KalturaBaseOTTUser> MasterUsers { get; set; }

        /// <summary>
        /// List of default users identifiers 
        /// </summary>
        [DataMember(Name = "default_users")]
        [JsonProperty("default_users")]
        [XmlArray(ElementName = "default_users")]
        [XmlArrayItem("item")] 
        public List<KalturaBaseOTTUser> DefaultUsers { get; set; }

        /// <summary>
        /// List of pending users identifiers 
        /// </summary>
        [DataMember(Name = "pending_users")]
        [JsonProperty("pending_users")]
        [XmlArray(ElementName = "pending_users")]
        [XmlArrayItem("item")] 
        public List<KalturaBaseOTTUser> PendingUsers { get; set; }

        /// <summary>
        /// The households region identifier
        /// </summary>
        [DataMember(Name = "region_id")]
        [JsonProperty("region_id")]
        [XmlElement(ElementName = "region_id")]
        public int RegionId { get; set; }

        /// <summary>
        /// Household state
        /// </summary>
        [DataMember(Name = "state")]
        [JsonProperty("state")]
        [XmlElement(ElementName = "state")]
        public KalturaHouseholdState State { get; set; }

        /// <summary>
        /// Is household frequency enabled
        /// </summary>
        [DataMember(Name = "is_frequency_enabled")]
        [JsonProperty("is_frequency_enabled")]
        [XmlElement(ElementName = "is_frequency_enabled")]
        public bool IsFrequencyEnabled { get; set; }

        /// <summary>
        /// The next time a device is allowed to be removed from the household (epoch)
        /// </summary>
        [DataMember(Name = "frequency_next_device_action")]
        [JsonProperty("frequency_next_device_action")]
        [XmlElement(ElementName = "frequency_next_device_action")]
        public DateTime FrequencyNextDeviceAction { get; set; }

        /// <summary>
        /// The next time a user is allowed to be removed from the household (epoch)
        /// </summary>
        [DataMember(Name = "frequency_next_user_action")]
        [JsonProperty("frequency_next_user_action")]
        [XmlElement(ElementName = "frequency_next_user_action")]
        public DateTime FrequencyNextUserAction { get; set; }

        /// <summary>
        /// Household restriction
        /// </summary>
        [DataMember(Name = "restriction")]
        [JsonProperty("restriction")]
        [XmlElement(ElementName = "restriction")]
        public KalturaHouseholdRestriction Restriction { get; set; }

        /// <summary>
        /// Household home networks
        /// </summary>
        [DataMember(Name = "home_networks")]
        [JsonProperty("home_networks")]
        [XmlArray(ElementName = "home_networks")]
        [XmlArrayItem("item")] 
        public List<KalturaHomeNetwork> HomeNetworks{ get; set; }
        
        /// <summary>
        /// Household device families
        /// </summary>
        [DataMember(Name = "device_families")]
        [JsonProperty("device_families")]
        [XmlArray(ElementName = "device_families")]
        [XmlArrayItem("item")] 
        public List<KalturaDeviceFamily> DeviceFamilies { get; set; }
    }
}