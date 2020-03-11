using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Permissions filter
    /// </summary>
    public partial class KalturaPermissionFilter : KalturaFilter<KalturaPermissionOrderBy>
    {

        /// <summary>
        /// Indicates whether the results should be filtered by userId using the current
        /// </summary>
        [DataMember(Name = "currentUserPermissionsContains")]
        [JsonProperty(PropertyName = "currentUserPermissionsContains")]
        [XmlElement(ElementName = "currentUserPermissionsContains", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool? CurrentUserPermissionsContains { get; set; }

        /// <summary>
        /// Return permissions by role ID
        /// </summary>
        [DataMember(Name = "roleIdIn")]
        [JsonProperty(PropertyName = "roleIdIn")]
        [XmlElement(ElementName = "roleIdIn", IsNullable = true)]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public long? RoleIDIn { get; set; }

        public override KalturaPermissionOrderBy GetDefaultOrderByValue()
        {
            return KalturaPermissionOrderBy.NONE;
        }

        internal void Validate()
        {
            if (CurrentUserPermissionsContains.HasValue && RoleIDIn.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaPermissionFilter.CurrentUserPermissionsContains, KalturaPermissionFilter.RoleIDIn");
            }
        }
    }

    public enum KalturaPermissionOrderBy
    {
        NONE
    }
}