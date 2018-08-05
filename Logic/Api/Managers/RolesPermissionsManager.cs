using ApiObjects.Response;
using ApiObjects.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachingProvider.LayeredCache;
using DAL;
using KLogMonitor;
using System.Reflection;
using ApiObjects;

namespace APILogic.Api.Managers
{
    public static class RolesPermissionsManager
    {
        
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public const long ANONYMOUS_ROLE_ID = 0;

        private static Dictionary<string, List<KeyValuePair<long, bool>>> BuildPermissionItemsDictionary(List<Role> roles)
        {
            Dictionary<string, List<KeyValuePair<long, bool>>> dictionary = new Dictionary<string, List<KeyValuePair<long, bool>>>();

            string key = null;
            try
            {
                if (roles == null)
                {
                    return dictionary;
                }

                foreach (Role role in roles)
                {
                    if (role.Permissions == null)
                    {
                        continue;
                    }
                    foreach (Permission permission in role.Permissions)
                    {
                        key = permission.Name.ToLower();

                        // if the dictionary already contains the action, try to append the role and /or the users group                   
                        if (dictionary.ContainsKey(key))
                        {
                            dictionary[key].Add(new KeyValuePair<long, bool>(role.Id, permission.isExcluded));
                        }
                        else
                        {
                            dictionary.Add(key, new List<KeyValuePair<long, bool>>() { new KeyValuePair<long, bool>(role.Id, permission.isExcluded) });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("BuildPermissionItemsDictionary failed ex:{0}", ex);
            }
            return dictionary;
        }
        
        private static List<long> GetRoleIds(int groupId, string userId)
        {
            // ??????? we set list to be only anonymous at first but it is never used. something here smells fishy!
            List<long> roleIds = new List<long>() { ANONYMOUS_ROLE_ID };

            ApiObjects.Response.LongIdsResponse response = Core.Users.Module.GetUserRoleIds(groupId, userId);
            if (response == null || response.Status == null || response.Status.Code != (int)eResponseStatus.OK)
            {
                return new List<long>();
            }

            if (response.Ids != null)
            {
                roleIds.AddRange(response.Ids);
            }

            return roleIds;
        }


      
        public static bool IsPermittedPermission(int groupId, string userId, ApiObjects.RolePermissions rolePermission)
        {
            try
            {
                // Looks like this code was wrong. it should be reviewed by someone else
                //if (string.IsNullOrEmpty(userId) || userId == "0")// anonymouse
                //{
                //    return true;
                //}
              
                Dictionary<string, List<KeyValuePair<long, bool>>> rolesPermission = GetPermissionsRolesByGroup(groupId);
                if (rolesPermission != null && rolesPermission.Any() && rolesPermission.ContainsKey(rolePermission.ToString().ToLower()))
                {   
                    List<long> userRoleIDs = GetRoleIds(groupId, userId);
                    if (userRoleIDs != null && userRoleIDs.Any())
                    {
                        if (rolesPermission.ContainsKey(rolePermission.ToString().ToLower()))
                        {
                            var userRoles = rolesPermission[rolePermission.ToString().ToLower()].Where(x => userRoleIDs.Contains(x.Key));
                            if (userRoles != null && userRoles.Any() && userRoles.Where(x => x.Value).Count() == 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        internal static bool IsPermittedPermissionItem(int groupId, string userId, string permissionItem)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || userId == "0")// anonymouse
                {
                    return true;
                }

                List<Role> roles = null;
                string key = LayeredCacheKeys.GetPermissionsRolesIdsKey(groupId);
                string invalidationKey = LayeredCacheKeys.GetPermissionsRolesIdsInvalidationKey(groupId);
                if (!LayeredCache.Instance.Get<List<Role>>(key, ref roles, GetRolesByGroupId, new Dictionary<string, object>() { { "groupId", groupId } },
                                                        groupId, LayeredCacheConfigNames.GET_ROLES_BY_GROUP_ID, new List<string>() { invalidationKey }))
                {
                    log.ErrorFormat("Failed getting GetRolesByGroupId from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
                
                if (roles != null && roles.Any())
                {
                    List<long> userRoleIDs = GetRoleIds(groupId, userId);
                    if (userRoleIDs != null && userRoleIDs.Any())
                    {
                        // get list of all user permissions 
                        List<Permission> permissions = roles.Where(x=> userRoleIDs.Contains(x.Id)).SelectMany(x => x.Permissions).ToList();
                        if (permissions != null && permissions.Any())
                        {
                            var permissionItems = permissions.Where(x => x.PermissionItems != null).SelectMany(x => x.PermissionItems).ToList();

                            if (permissionItems != null && permissionItems.Where(x => x.Name.ToLower() == permissionItem.ToLower() && x.IsExcluded).Any())
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return true;
        }

        private static Dictionary<string, List<KeyValuePair<long, bool>>> GetPermissionsRolesByGroup(int groupId)
        {
            try
            {
                List<Role> roles = null;
                string key = LayeredCacheKeys.GetPermissionsRolesIdsKey(groupId);
                string invalidationKey = LayeredCacheKeys.GetPermissionsRolesIdsInvalidationKey(groupId);
                if (!LayeredCache.Instance.Get<List<Role>>(key, ref roles, GetRolesByGroupId, new Dictionary<string, object>() { { "groupId", groupId } },
                                                        groupId, LayeredCacheConfigNames.GET_ROLES_BY_GROUP_ID, new List<string>() { invalidationKey }))
                {
                    log.ErrorFormat("Failed getting GetPermissionRoleByGroup from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }

                if (roles != null && roles.Any())
                {
                    return BuildPermissionItemsDictionary(roles);
                }                
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetPermissionRoleByGroup failed, groupId : {0}, ex : {1}", groupId, ex);
            }
            return null;
        }

        private static Tuple<List<Role>, bool> GetRolesByGroupId(Dictionary<string, object> funcParams)
        {
            List<Role> roles = new List<Role>();
            bool res = false;
            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;

                    if (groupId.HasValue)
                    {
                        roles = ApiDAL.GetRoles(groupId.Value, null); // get all roles for this group
                        if (roles != null && roles.Any())
                        {
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetRolesByGroupId failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<Role>, bool>(roles, res);
        }

        public static Status GetSuspentionStatus(int groupId, int householdId)
        {
            Status status;
            Core.Users.Domain domain = null;
            if (householdId > 0)
            {
                var domainResponse = Core.Domains.Module.GetDomainInfo(groupId, householdId);
                if (domainResponse != null && domainResponse.Status != null && domainResponse.Status.Code == (int)eResponseStatus.OK)
                {
                    domain = domainResponse.Domain;
                }
            }
            if (domain != null && domain.roleId > 0)
            {
                status = new ApiObjects.Response.Status((int)eResponseStatus.NotAllowed, "Not allowed for role id [@roleId@]", new List<KeyValuePair>()
                        { new KeyValuePair("roleId", domain.roleId.ToString()) });
            }
            else
            {
                status = new ApiObjects.Response.Status((int)eResponseStatus.NotAllowed, "Action not allowed");
            }

            return status;
        }

    }
}
