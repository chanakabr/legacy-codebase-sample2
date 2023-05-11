using ApiObjects.Roles;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// User-roles list
    /// </summary>
    [DataContract(Name = "UserRoles", Namespace = "")]
    [XmlRoot("UserRoles")]
    public partial class KalturaUserRoleListResponse : KalturaListResponse
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

    public partial class KalturaUserRole : KalturaOTTObject
    {
        /// <summary>
        /// User role identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
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
        [Deprecated("4.6.0.0")]
        public List<KalturaPermission> Permissions { get; set; }

        /// <summary>
        /// permissions associated with the user role
        /// </summary>
        [DataMember(Name = "permissionNames")]
        [JsonProperty("permissionNames")]
        [XmlElement(ElementName = "permissionNames")]
        public string PermissionNames { get; set; }

        /// <summary>
        /// permissions associated with the user role in is_exclueded = true
        /// </summary>
        [DataMember(Name = "excludedPermissionNames")]
        [JsonProperty("excludedPermissionNames")]
        [XmlElement(ElementName = "excludedPermissionNames")]
        public string ExcludedPermissionNames { get; set; }

        /// <summary>
        /// Role type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaUserRoleType Type { get; set; }

        /// <summary>
        /// Role profile
        /// </summary>
        [DataMember(Name = "profile")]
        [JsonProperty("profile")]
        [XmlElement(ElementName = "profile")]
        public KalturaUserRoleProfile? Profile { get; set; }

        internal long getId()
        {
            return Id.HasValue ? (long)Id : 0;
        }
    }

    public enum KalturaUserRoleProfile
    {
        USER,
        PARTNER,
        PROFILE,
        SYSTEM,
        PERMISSION_EMBEDDED
    }

}