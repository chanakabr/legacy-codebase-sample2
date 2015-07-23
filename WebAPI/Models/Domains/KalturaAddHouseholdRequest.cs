using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// AddHousehold request
    /// </summary>
    public class KalturaAddHouseholdRequest : KalturaOTTObject
    {
        /// <summary>
        /// Name for the household
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Description for the household
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Identifier of the user that will become the master of the created household
        /// </summary>
        [DataMember(Name = "master_user_id")]
        [JsonProperty("master_user_id")]
        public string MasterUserId { get; set; }
    }
}