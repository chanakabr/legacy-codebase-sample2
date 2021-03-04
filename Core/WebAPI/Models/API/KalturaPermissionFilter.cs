using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects.Roles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public partial class KalturaBasePermissionFilter : KalturaFilter<KalturaPermissionOrderBy>
    {
        public override KalturaPermissionOrderBy GetDefaultOrderByValue()
        {
            return KalturaPermissionOrderBy.NONE;
        }

        public virtual KalturaPermissionListResponse GetPermissions(ContextData contextData)
        {
            throw new NotImplementedException();
        }

        public virtual void Validate(ContextData contextData)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Permissions filter
    /// </summary>
    public partial class KalturaPermissionFilter : KalturaBasePermissionFilter
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

        public override void Validate(ContextData contextData)
        {
            if (CurrentUserPermissionsContains.HasValue && RoleIDIn.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaPermissionFilter.CurrentUserPermissionsContains, KalturaPermissionFilter.RoleIDIn");
            }
        }

        public override KalturaPermissionListResponse GetPermissions(ContextData contextData)
        {
            long userId = contextData.UserId.Value;
            if ((!CurrentUserPermissionsContains.HasValue || !CurrentUserPermissionsContains.Value || RoleIDIn.HasValue))
            {
                userId = 0;
            }

            return ClientsManager.ApiClient().GetPermissions(contextData.GroupId, userId, RoleIDIn);
        }
    }

    public enum KalturaPermissionOrderBy
    {
        NONE
    }

    public partial class KalturaPermissionByIdInFilter : KalturaBasePermissionFilter
    {
        /// <summary>
        /// Category item identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = false)]
        public string IdIn { get; set; }

        public override void Validate(ContextData contextData)
        {
            if (string.IsNullOrEmpty(IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "idIn");
            }
        }
       
        public override KalturaPermissionListResponse GetPermissions(ContextData contextData)
        {
            var result = new KalturaPermissionListResponse();

            Func<GenericListResponse<Permission>> getPermissionListFunc = () =>
                 Core.Api.Module.GetGroupPermissionsByIds(contextData.GroupId, GetIdIn());

            KalturaGenericListResponse<KalturaPermission> response =
                ClientUtils.GetResponseListFromWS<KalturaPermission, Permission>(getPermissionListFunc);

            result.Permissions = new List<KalturaPermission>(response.Objects);
            result.TotalCount = response.TotalCount;
            return result;
        }

        private List<long> GetIdIn()
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(IdIn))
            {
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value) && !list.Contains(value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "idIn");
                    }
                }
            }

            return list;
        }

    }

}