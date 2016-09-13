using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.General;

namespace WebAPI.Managers
{
    public class RolesManager
    {       
        private const string PARTNER_WILDCARD = "partner*";
        private const string HOUSEHOLD_WILDCARD = "household*";

        public const long ANONYMOUS_ROLE_ID = 0;
        public const long USER_ROLE_ID = 1;
        public const long MASTER_ROLE_ID = 2;
        public const long OPERATOR_ROLE_ID = 3;
        public const long MANAGER_ROLE_ID = 4;
        public const long ADMINISTRATOR_ROLE_ID = 5;

        /// <summary>
        /// Builds a dictionary representing the schema of roles, permissions and action permission items for the group.
        /// </summary>
        /// <param name="roles">List of roles</param>
        /// <returns>Dictionary of dictionaries, where the key of the first dictionary is a string representing a service action pair (format: {service}_{action}) 
        /// and the value is a dictionary representing all the role IDs containing the permission item of the service action pair, and the users group list that is relevant for the action, 
        /// the second's dictionary key is the role ID and the value is a ';' separated list of users allowed in a group permission</returns>
        internal static Dictionary<string, Dictionary<long, string>> BuildPermissionItemsDictionary(List<KalturaUserRole> roles)
        {
            Dictionary<string, Dictionary<long, string>> dictionary = new Dictionary<string, Dictionary<long, string>>();

            string key = null;
            KalturaApiActionPermissionItem apiActionPermissionItem;
            KalturaApiParameterPermissionItem apiParameterPermissionItem;
            KalturaApiArgumentPermissionItem apiArgumentPermissionItem;
            string usersGroup;

            foreach (var role in roles)
            {
                foreach (var permission in role.Permissions)
                {
                    // if the permission is group permission, get the users group list to append later
                    if (permission is KalturaGroupPermission)
                    {
                        usersGroup = ((KalturaGroupPermission)permission).Group;
                    }
                    else
                    {
                        usersGroup = string.Empty;
                    }

                    foreach (var permissionItem in permission.PermissionItems)
                    {
                        // the dictionary is relevant only for action permission items
                        if (permissionItem is KalturaApiActionPermissionItem)
                        {
                            apiActionPermissionItem = (KalturaApiActionPermissionItem)permissionItem;
                            key = string.Format("{0}_{1}", apiActionPermissionItem.Service, apiActionPermissionItem.Action).ToLower();
                        }
                        else if (permissionItem is KalturaApiParameterPermissionItem)
                        {
                            apiParameterPermissionItem = (KalturaApiParameterPermissionItem)permissionItem;
                            key = string.Format("{0}_{1}_{2}", apiParameterPermissionItem.Object, apiParameterPermissionItem.Parameter, apiParameterPermissionItem.Action).ToLower();
                        }
                        else if (permissionItem is KalturaApiArgumentPermissionItem)
                        {
                            apiArgumentPermissionItem = (KalturaApiArgumentPermissionItem)permissionItem;
                            key = string.Format("{0}_{1}_{2}", apiArgumentPermissionItem.Service, apiArgumentPermissionItem.Action, apiArgumentPermissionItem.Parameter).ToLower();
                        }
                        else
                        {
                            continue;
                        }


                        // if the dictionary already contains the action, try to append the role and /or the users group
                        if (dictionary.ContainsKey(key))
                        {
                            if (!dictionary[key].ContainsKey(role.getId()))
                            {
                                dictionary[key].Add(role.getId(), usersGroup);
                            }
                            else
                            {
                                dictionary[key][role.getId()] = string.Format("{0};{1}", usersGroup, dictionary[key][role.getId()]);
                            }
                        }
                        // add the action to the dictionary
                        else
                        {
                            dictionary.Add(key, new Dictionary<long, string>() { { role.getId(), usersGroup } });
                        }
                    }
                }
            }

            return dictionary;
        }

        private static KS getKS(bool silent)
        {
            KS ks = KS.GetFromRequest();

            if (ks == null)
                throw new UnauthorizedException(UnauthorizedException.SERVICE_FORBIDDEN);

            if (!ks.IsValid && !silent)
                throw new UnauthorizedException(UnauthorizedException.KS_EXPIRED);

            return ks;
        }

        public static List<long> GetRoleIds(KS ks)
        {
            List<long> roleIds = new List<long>() { RolesManager.ANONYMOUS_ROLE_ID };

            if (ks.UserId != "0")
            {
                // not anonymous user - get user's roles
                var userRoleIds = ClientsManager.UsersClient().GetUserRoleIds(ks.GroupId, ks.UserId);
                if (userRoleIds != null && userRoleIds.Count > 0)
                {
                    roleIds.AddRange(userRoleIds);
                }
            }
            return roleIds;
        }

        /// <summary>
        /// Checks if an action from a service is allowed
        /// </summary>
        /// <param name="service">Service name</param>
        /// <param name="action">Action name</param>
        /// <param name="silent">Fail silently</param>
        internal static void ValidateActionPermitted(string service, string action, bool silent = false)
        {
            KS ks = getKS(silent);
            List<long> roleIds = GetRoleIds(ks);

            // no roles found for the user
            if (roleIds == null || roleIds.Count == 0)
                throw new UnauthorizedException(UnauthorizedException.SERVICE_FORBIDDEN);

            string allowedUsersGroup = null;

            // user not permitted
            if (!IsActionPermittedForRoles(ks.GroupId, service, action, roleIds, out allowedUsersGroup))
                throw new UnauthorizedException(UnauthorizedException.SERVICE_FORBIDDEN);

            // allowed group users (additional user_id) handling:
            // get user_id additional parameter
            string userId = null;
            if (HttpContext.Current.Items.Contains(RequestParser.REQUEST_USER_ID))
            {
                var extraUserId = HttpContext.Current.Items[RequestParser.REQUEST_USER_ID];
                userId = extraUserId != null ? extraUserId.ToString() : null;
            }
            // if exists and is in the allowed group users list - override the user id in ks (HOUSEHOLD_WILDCARD = everyone in the domain is allowed, PARTNER_WILDCARD = everyone in the group is allowed)
            if (!string.IsNullOrEmpty(userId))
            {
                if (!string.IsNullOrEmpty(allowedUsersGroup) && (
                    allowedUsersGroup.Contains(userId) ||
                    (allowedUsersGroup.Contains(RolesManager.PARTNER_WILDCARD) && AuthorizationManager.IsUserInGroup(userId, ks.GroupId)) ||
                    (allowedUsersGroup.Contains(RolesManager.HOUSEHOLD_WILDCARD) && AuthorizationManager.IsUserInHousehold(userId, ks.GroupId))))
                {
                    ks.UserId = userId;
                    KS.SaveOnRequest(ks);
                }
                else
                {
                    throw new UnauthorizedException(UnauthorizedException.SERVICE_FORBIDDEN);
                }
            }
        }

        /// <summary>
        /// Checks if a property of an object is allowed
        /// </summary>
        /// <param name="type">Type name</param>
        /// <param name="property">Property name</param>
        /// <param name="action">Required action</param>
        /// <param name="silent">Fail silently</param>
        /// <returns>True if the property is permitted, false otherwise</returns>
        internal static void ValidatePropertyPermitted(string type, string property, RequestType action, bool silent = false)
        {
            KS ks = getKS(silent);
            List<long> roleIds = GetRoleIds(ks);

            // no roles found for the user
            if (roleIds == null || roleIds.Count == 0)
                throw new UnauthorizedException(UnauthorizedException.PROPERTY_ACTION_FORBIDDEN, Enum.GetName(typeof(RequestType), action), type, property);

            string allowedUsersGroup = null;

            // user not permitted
            string actionName = Enum.GetName(typeof(RequestType), action);
            if (!IsPropertyPermittedForRoles(ks.GroupId, type, property, actionName, roleIds, out allowedUsersGroup))
                throw new UnauthorizedException(UnauthorizedException.PROPERTY_ACTION_FORBIDDEN, Enum.GetName(typeof(RequestType), action), type, property);
        }

        /// <summary>
        /// Checks if a property of an object is allowed
        /// </summary>
        /// <param name="service">Controller service name</param>
        /// <param name="action">Method action name</param>
        /// <param name="argument">Argument name</param>
        /// <param name="silent">Fail silently</param>
        /// <returns>True if the method argument is permitted, false otherwise</returns>
        internal static void ValidateArgumentPermitted(string service, string action, string argument, bool silent = false)
        {
            KS ks = getKS(silent);
            List<long> roleIds = GetRoleIds(ks);

            // no roles found for the user
            if (roleIds == null || roleIds.Count == 0)
                throw new UnauthorizedException(UnauthorizedException.ACTION_ARGUMENT_FORBIDDEN, argument, service, action);

            string allowedUsersGroup = null;

            // user not permitted
            if (!IsArgumentPermittedForRoles(ks.GroupId, service, action, argument, roleIds, out allowedUsersGroup))
                throw new UnauthorizedException(UnauthorizedException.ACTION_ARGUMENT_FORBIDDEN, argument, service, action);
        }

        private static bool IsPermittedForRoles(int groupId, string objectPropertyKey, List<long> roleIds, out string usersGroup)
        {
            usersGroup = null;
            StringBuilder usersGroupStringBuilder = new StringBuilder();

            // get group's roles schema
            Dictionary<string, Dictionary<long, string>> permissionItemsDictionary = GroupsManager.GetGroup(groupId).PermissionItemsRolesMapping;

            // if the permission for the property is not defined in the schema - return false
            if (!permissionItemsDictionary.ContainsKey(objectPropertyKey))
            {
                return false;
            }

            Dictionary<long, string> roles = permissionItemsDictionary[objectPropertyKey];
            bool isPermitted = false;


            foreach (var roleId in roleIds)
            {
                // if the permission item for the action is part of one of the supplied roles - return true
                if (roles.ContainsKey(roleId))
                {
                    isPermitted = true;

                    // the action is permitted for the role, append the users group of the permission if defined
                    if (usersGroupStringBuilder.Length == 0)
                    {
                        usersGroupStringBuilder.Append(roles[roleId]);
                    }
                    else
                    {
                        usersGroupStringBuilder.AppendFormat(";{0}", roles[roleId]);
                    }
                }
            }

            usersGroup = usersGroupStringBuilder.ToString();
            return isPermitted;
        }

        private static bool IsArgumentPermittedForRoles(int groupId, string service, string action, string argument, List<long> roleIds, out string usersGroup)
        {
            usersGroup = null;
            StringBuilder usersGroupStringBuilder = new StringBuilder();

            // build the key for the service action key for roles schema (permission items - roles dictionary)
            string methodArgumentKey = string.Format("{0}_{1}_{2}", service, action, argument).ToLower();
            return IsPermittedForRoles(groupId, methodArgumentKey, roleIds, out usersGroup);
        }

        private static bool IsPropertyPermittedForRoles(int groupId, string type, string property, string action, List<long> roleIds, out string usersGroup)
        {
            usersGroup = null;
            StringBuilder usersGroupStringBuilder = new StringBuilder();

            // build the key for the service action key for roles schema (permission items - roles dictionary)
            string objectPropertyKey = string.Format("{0}_{1}_{2}", type, property, action).ToLower();
            return IsPermittedForRoles(groupId, objectPropertyKey, roleIds, out usersGroup);
        }

        private static bool IsActionPermittedForRoles(int groupId, string service, string action, List<long> roleIds, out string usersGroup)
        {
            usersGroup = null;
            StringBuilder usersGroupStringBuilder = new StringBuilder();

            // build the key for the service action key for roles schema (permission items - roles dictionary)
            string serviceActionKey = string.Format("{0}_{1}", service, action).ToLower();
            return IsPermittedForRoles(groupId, serviceActionKey, roleIds, out usersGroup);
        }
    }
}