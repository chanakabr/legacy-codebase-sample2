using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
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
        [SchemeProperty(ReadOnly = true)]
        public long? Id { get; set; }

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
        [DataMember(Name = "externalId")]
        [JsonProperty("externalId")]
        [XmlElement(ElementName = "externalId")]
        [SchemeProperty(InsertOnly = true)]
        [OldStandardProperty("external_id")]
        public string ExternalId { get; set; }

        /// <summary>
        /// Household limitation module identifier
        /// </summary>
        [DataMember(Name = "householdLimitationsId")]
        [JsonProperty("householdLimitationsId")]
        [XmlElement(ElementName = "householdLimitationsId")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("household_limitations_id")]
        public int? HouseholdLimitationsId { get; set; }

        /// <summary>
        /// The max number of the devices that can be added to the household
        /// </summary>
        [DataMember(Name = "devicesLimit")]
        [JsonProperty("devicesLimit")]
        [XmlElement(ElementName = "devicesLimit")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("devices_limit")]
        public int? DevicesLimit { get; set; }

        /// <summary>
        /// The max number of the users that can be added to the household
        /// </summary>
        [DataMember(Name = "usersLimit")]
        [JsonProperty("usersLimit")]
        [XmlElement(ElementName = "usersLimit")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("users_limit")]
        public int? UsersLimit { get; set; }

        /// <summary>
        /// The max number of concurrent streams in the household
        /// </summary>
        [DataMember(Name = "concurrentLimit")]
        [JsonProperty("concurrentLimit")]
        [XmlElement(ElementName = "concurrentLimit")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("concurrent_limit")]
        public int? ConcurrentLimit { get; set; }

        /// <summary>
        /// List of users identifiers 
        /// </summary>
        [DataMember(Name = "users")]
        [JsonProperty("users")]
        [XmlArray(ElementName = "users", IsNullable = true)]
        [XmlArrayItem("item")]
        [Obsolete]
        public List<KalturaBaseOTTUser> Users { get; set; }

        /// <summary>
        /// List of master users identifiers 
        /// </summary>
        [DataMember(Name = "masterUsers")]
        [JsonProperty("masterUsers")]
        [XmlArray(ElementName = "masterUsers", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("master_users")]
        [Obsolete]
        public List<KalturaBaseOTTUser> MasterUsers { get; set; }

        /// <summary>
        /// List of default users identifiers 
        /// </summary>
        [DataMember(Name = "defaultUsers")]
        [JsonProperty("defaultUsers")]
        [XmlArray(ElementName = "defaultUsers", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("default_users")]
        [Obsolete]
        public List<KalturaBaseOTTUser> DefaultUsers { get; set; }

        /// <summary>
        /// List of pending users identifiers 
        /// </summary>
        [DataMember(Name = "pendingUsers")]
        [JsonProperty("pendingUsers")]
        [XmlArray(ElementName = "pendingUsers", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("pending_users")]
        [Obsolete]
        public List<KalturaBaseOTTUser> PendingUsers { get; set; }

        /// <summary>
        /// The households region identifier
        /// </summary>
        [DataMember(Name = "regionId")]
        [JsonProperty("regionId")]
        [XmlElement(ElementName = "regionId")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("region_id")]
        public int? RegionId { get; set; }

        /// <summary>
        /// Household state
        /// </summary>
        [DataMember(Name = "state")]
        [JsonProperty("state")]
        [XmlElement(ElementName = "state", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public KalturaHouseholdState? State { get; set; }

        /// <summary>
        /// Is household frequency enabled
        /// </summary>
        [DataMember(Name = "isFrequencyEnabled")]
        [JsonProperty("isFrequencyEnabled")]
        [XmlElement(ElementName = "isFrequencyEnabled")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("is_frequency_enabled")]
        public bool? IsFrequencyEnabled { get; set; }

        /// <summary>
        /// The next time a device is allowed to be removed from the household (epoch)
        /// </summary>
        [DataMember(Name = "frequencyNextDeviceAction")]
        [JsonProperty("frequencyNextDeviceAction")]
        [XmlElement(ElementName = "frequencyNextDeviceAction")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("frequency_next_device_action")]
        public long? FrequencyNextDeviceAction { get; set; }

        /// <summary>
        /// The next time a user is allowed to be removed from the household (epoch)
        /// </summary>
        [DataMember(Name = "frequencyNextUserAction")]
        [JsonProperty("frequencyNextUserAction")]
        [XmlElement(ElementName = "frequencyNextUserAction")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("frequency_next_user_action")]
        public long? FrequencyNextUserAction { get; set; }

        /// <summary>
        /// Household restriction
        /// </summary>
        [DataMember(Name = "restriction")]
        [JsonProperty("restriction")]
        [XmlElement(ElementName = "restriction", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public KalturaHouseholdRestriction? Restriction { get; set; }

        /// <summary>
        /// Household device families
        /// </summary>
        [DataMember(Name = "deviceFamilies")]
        [JsonProperty("deviceFamilies")]
        [XmlArray(ElementName = "deviceFamilies", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("device_families")]
        [Obsolete]
        public List<KalturaDeviceFamily> DeviceFamilies { get; set; }

        internal long getId()
        {
            return Id.HasValue ? (long)Id : 0;
        }
    }
}