using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// User
    /// </summary>
    public class KalturaClientUser : KalturaOTTObject
    {
        /// <summary>
        /// ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public string ID { get; set; }

        /// <summary>
        /// Household ID
        /// </summary>
        [DataMember(Name = "household_id")]
        [JsonProperty("household_id")]
        public string HouseholdID { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        [DataMember(Name = "first_name")]
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
    }
}