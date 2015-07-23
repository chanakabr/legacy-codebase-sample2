using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// User
    /// </summary>
    [DataContract(Name="user")]
    public class KalturaUser : KalturaOTTObject
    {
        /// <summary>
        /// User identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// Household identifier
        /// </summary>
        [DataMember(Name = "household_id")]
        [JsonProperty("household_id")]
        public int HouseholdID { get; set; }

        /// <summary>
        /// Basic data
        /// </summary>
        [DataMember(Name = "basic_data")]
        [JsonProperty("basic_data")]
        public KalturaUserBasicData BasicData { get; set; }

        /// <summary>
        /// Dynamic data
        /// </summary>
        [DataMember(Name = "dynamic_data")]
        [JsonProperty("dynamic_data")]
        public SerializableDictionary<string, string> DynamicData { get; set; }

        /// <summary>
        /// Is the user the household master
        /// </summary>
        [DataMember(Name = "is_household_master")]
        [JsonProperty("is_household_master")]
        public bool IsHouseholdMaster { get; set; }

        /// <summary>
        /// Suspention state
        /// </summary>
        [DataMember(Name = "suspention_state")]
        [JsonProperty("suspention_state")]
        public KalturaHouseholdSuspentionState SuspentionState { get; set; }

        /// <summary>
        /// User state
        /// </summary>
        [DataMember(Name = "user_state")]
        [JsonProperty("user_state")]
        public KalturaUserState UserState { get; set; }
    }
}