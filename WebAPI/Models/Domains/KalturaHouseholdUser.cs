using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Filters;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Users;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Household users list
    /// </summary>
    public class KalturaHouseholdUserListResponse : KalturaListResponse
    {
        /// <summary>
        /// Household users
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaHouseholdUser> Objects { get; set; }
    }

    /// <summary>
    /// Household user 
    /// </summary>
    public class KalturaHouseholdUser : KalturaOTTObject
    {
        /// <summary>
        /// The identifier of the household
        /// </summary>
        [DataMember(Name = "householdId")]
        [JsonProperty("householdId")]
        [XmlElement(ElementName = "householdId")]
        [SchemeProperty(RequiresPermission = (int)RequestType.WRITE)]
        public int? HouseholdId { get; set; }

        /// <summary>
        /// The identifier of the user
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty("userId")]
        [XmlElement(ElementName = "userId")]
        [SchemeProperty(RequiresPermission = (int)RequestType.WRITE)]
        public string UserId { get; set; }

        /// <summary>
        /// True if the user added as master use
        /// </summary>
        [DataMember(Name = "isMaster")]
        [JsonProperty("isMaster")]
        [XmlElement(ElementName = "isMaster")]
        public bool? IsMaster { get; set; }

        /// <summary>
        /// The username of the household master for adding a user in status pending for the household master to approve
        /// </summary>
        [DataMember(Name = "householdMasterUsername")]
        [JsonProperty("householdMasterUsername")]
        [XmlElement(ElementName = "householdMasterUsername")]
        [SchemeProperty(InsertOnly = true)]
        public string HouseholdMasterUsername { get; set; }

        /// <summary>
        /// The status of the user in the household 
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaHouseholdUserStatus Status { get; set; }

        internal bool getIsMaster()
        {
            return IsMaster.HasValue ? (bool)IsMaster : false;
        }
    }
}