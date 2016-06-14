using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;
using WebAPI.Models.Users;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Household details
    /// </summary>
    public class KalturaHouseholdUser : KalturaOTTObject
    {
        /// <summary>
        /// The identifier of the user
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty("userId")]
        [XmlElement(ElementName = "userId")]
        public string UserId { get; set; }

        /// <summary>
        /// True if the user added as master use
        /// </summary>
        [DataMember(Name = "isMaster")]
        [JsonProperty("isMaster")]
        [XmlElement(ElementName = "isMaster")]
        public bool? IsMaster { get; set; }

        internal bool getIsMaster()
        {
            return IsMaster.HasValue ? (bool)IsMaster : false;
        }
    }
}