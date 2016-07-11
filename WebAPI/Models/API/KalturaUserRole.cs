using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// User-roles list
    /// </summary>
    [DataContract(Name = "UserRoles", Namespace = "")]
    [XmlRoot("UserRoles")]
    public class KalturaUserRoleListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of generic rules
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaUserRole> UserRoles { get; set; }
    }

    public class KalturaUserRole : KalturaOTTObject
    {
        /// <summary>
        /// User role identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public long? Id { get; set; }

        /// <summary>
        /// User role name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// List of permissions associated with the user role
        /// </summary>
        [DataMember(Name = "permissions")]
        [JsonProperty("permissions")]
        [XmlArray(ElementName = "permissions", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaPermission> Permissions { get; set; }

        internal long getId()
        {
            return Id.HasValue ? (long)Id : 0;
        }
    }
}