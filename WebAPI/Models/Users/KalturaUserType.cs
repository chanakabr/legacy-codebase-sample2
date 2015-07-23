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
    /// User type
    /// </summary>
    public class KalturaUserType : KalturaOTTObject
    {
        /// <summary>
        /// User type identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        public int? Id { get; set; }

        /// <summary>
        /// User type description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}