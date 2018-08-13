using CachingProvider.LayeredCache;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string PARTNER_WILDCARD = "partner*";
        private const string HOUSEHOLD_WILDCARD = "household*";

        public const long ANONYMOUS_ROLE_ID = 0;
        public const long USER_ROLE_ID = 1;
        public const long MASTER_ROLE_ID = 2;
        public const long OPERATOR_ROLE_ID = 3;
        public const long MANAGER_ROLE_ID = 4;
        public const long ADMINISTRATOR_ROLE_ID = 5;

        #region Private Methods

        private static KS getKS(bool silent)
        {
            KS ks = KS.GetFromRequest();

            if (ks == null)
                throw new UnauthorizedException(UnauthorizedException.SERVICE_FORBIDDEN);

            if (!ks.IsValid && !silent)
                throw new UnauthorizedException(UnauthorizedException.KS_EXPIRED);

            return ks;
        }

        /// <summary>
        /// Builds a dictionary representing the schema of roles, permissions and action permission items for the group.
        /// </summary>
        /// <param name="roles">List of roles</param>
        /// <returns>Dictionary of dictionaries, where the key of the first dictionary is a string representing a service action pair (format: {service}_{action}) 
        /// and the value is a dictionary representing all the role IDs containing the permission item of the service action pair, and the users group list that is relevant for the action, 
        /// the second's dictionary key is the role ID and the value is a ';' separated list of users allowed in a group permission</returns>
        private static Tuple<Dictionary<string, Dictionary<long, KeyValuePair<string, bool>>>, bool> BuildGroupPermissionItemsDictionary(Dictionary<string, object> funcParams)
        {
            Dictionary<string, Dictionary<long, KeyValuePair<string, bool>>> result = new Dictionary<string, Dictionary<long, KeyValuePair<string, bool>>>();            
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                    {
                        List<KalturaUserRole> groupUserRoles = ClientsManager.ApiClient().GetRoles(groupId.Value);
                        if (groupUserRoles != null && groupUserRoles.Count > 0)
                        {
                            KalturaApiActionPermissionItem apiActionPermissionItem;
                            KalturaApiParameterPermissionItem apiParameterPermissionItem;
                            KalturaApiArgumentPermissionItem apiArgumentPermissionItem;
                            string usersGroup;

                            foreach (KalturaUserRole role in groupUserRoles)
                            {
                                foreach (KalturaPermission permission in role.Permissions)
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

                                    foreach (KalturaPermissionItem permissionItem in permission.PermissionItems)
                                    {
                                        // in case we have the following actions on KalturaApiParameterPermissionItem we need to create multiple items: ALL\WRITE
                                        Dictionary<string, bool> keyToExcludeMap = new Dictionary<string, bool>();
                                        // the dictionary is relevant only for action permission items
                                        if (permissionItem is KalturaApiActionPermissionItem)
                                        {
                                            apiActionPermissionItem = (KalturaApiActionPermissionItem)permissionItem;
                                            keyToExcludeMap.Add(string.Format("{0}_{1}", apiActionPermissionItem.Service, apiActionPermissionItem.Action).ToLower(), apiActionPermissionItem.IsExcluded);
                                        }
                                        else if (permissionItem is KalturaApiParameterPermissionItem)
                                        {
                                            apiParameterPermissionItem = (KalturaApiParameterPermissionItem)permissionItem;
                                            string key = string.Empty;
                                            switch (apiParameterPermissionItem.Action)
                                            {
                                                case KalturaApiParameterPermissionItemAction.READ:
                                                case KalturaApiParameterPermissionItemAction.INSERT:
                                                case KalturaApiParameterPermissionItemAction.UPDATE:
                                                    keyToExcludeMap.Add(string.Format("{0}_{1}_{2}", apiParameterPermissionItem.Object, apiParameterPermissionItem.Parameter,
                                                                                                    apiParameterPermissionItem.Action).ToLower(), apiParameterPermissionItem.IsExcluded);
                                                    break;
                                                // add both insert and update
                                                case KalturaApiParameterPermissionItemAction.WRITE:
                                                    keyToExcludeMap.Add(string.Format("{0}_{1}_{2}", apiParameterPermissionItem.Object, apiParameterPermissionItem.Parameter,
                                                                        KalturaApiParameterPermissionItemAction.INSERT).ToLower(), apiParameterPermissionItem.IsExcluded);
                                                    keyToExcludeMap.Add(string.Format("{0}_{1}_{2}", apiParameterPermissionItem.Object, apiParameterPermissionItem.Parameter,
                                                                        KalturaApiParameterPermissionItemAction.UPDATE).ToLower(), apiParameterPermissionItem.IsExcluded);
                                                    break;
                                                // add read, insert and update
                                                case KalturaApiParameterPermissionItemAction.ALL:
                                                    keyToExcludeMap.Add(string.Format("{0}_{1}_{2}", apiParameterPermissionItem.Object, apiParameterPermissionItem.Parameter,
                                                                        KalturaApiParameterPermissionItemAction.INSERT).ToLower(), apiParameterPermissionItem.IsExcluded);
                                                    keyToExcludeMap.Add(string.Format("{0}_{1}_{2}", apiParameterPermissionItem.Object, apiParameterPermissionItem.Parameter,
                                                                        KalturaApiParameterPermissionItemAction.UPDATE).ToLower(), apiParameterPermissionItem.IsExcluded);
                                                    keyToExcludeMap.Add(string.Format("{0}_{1}_{2}", apiParameterPermissionItem.Object, apiParameterPermissionItem.Parameter,
                                                                        KalturaApiParameterPermissionItemAction.READ).ToLower(), apiParameterPermissionItem.IsExcluded);
                                                    break;
                                                default:
                                                    continue;
                                                    break;
                                            }
                                        }
                                        else if (permissionItem is KalturaApiArgumentPermissionItem)
                                        {
                                            apiArgumentPermissionItem = (KalturaApiArgumentPermissionItem)permissionItem;
                                            keyToExcludeMap.Add(string.Format("{0}_{1}_{2}", apiArgumentPermissionItem.Service, apiArgumentPermissionItem.Action, apiArgumentPermissionItem.Parameter).ToLower(), apiArgumentPermissionItem.IsExcluded);
                                        }
                                        else
                                        {
                                            continue;
                                        }

                                        foreach (KeyValuePair<string, bool> pair in keyToExcludeMap)
                                        {
                                            bool isExcluded = pair.Value;

                                            // if the dictionary already contains the action, try to append the role and /or the users group
                                            if (result.ContainsKey(pair.Key))
                                            {
                                                if (!result[pair.Key].ContainsKey(role.getId()))
                                                {
                                                    result[pair.Key].Add(role.getId(), new KeyValuePair<string, bool>(usersGroup, isExcluded)); // usersGroup);
                                                }
                                                else
                                                {
                                                    result[pair.Key][role.getId()] = new KeyValuePair<string, bool>(string.Format("{0};{1}", usersGroup, result[pair.Key][role.getId()]), isExcluded);
                                                }
                                            }
                                            // add the action to the dictionary
                                            else
                                            {
                                                Dictionary<long, KeyValuePair<string, bool>> roleIdToKeyValuePairMap = new Dictionary<long, KeyValuePair<string, bool>>();
                                                roleIdToKeyValuePairMap.Add(role.getId(), new KeyValuePair<string, bool>(usersGroup, isExcluded));
                                                result.Add(pair.Key, roleIdToKeyValuePairMap);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("BuildGroupPermissionItemsDictionary failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, Dictionary<long, KeyValuePair<string, bool>>>, bool>(result, result != null && result.Count > 0);
        }

        private static Dictionary<string, Dictionary<long, KeyValuePair<string, bool>>> GetGroupPermissionItemsDictionary(int groupId)
        {
            Dictionary<string, Dictionary<long, KeyValuePair<string, bool>>> result = new Dictionary<string, Dictionary<long, KeyValuePair<string, bool>>>();
            try
            {
                string key = LayeredCacheKeys.GetGroupPermissionItemsDictionaryKey(groupId);
                string invalidationKey = LayeredCacheKeys.GetGroupPermissionItemsDictionaryInvalidationKey(groupId);
                if (!LayeredCache.Instance.GetWithAppDomainCache<Dictionary<string, Dictionary<long, KeyValuePair<string, bool>>>>(key, ref result, BuildGroupPermissionItemsDictionary,
                                                                                                                new Dictionary<string, object>() { { "groupId", groupId } }, groupId,
                                                                                                                LayeredCacheConfigNames.GET_GROUP_PERMISSION_ITEMS_BY_GROUP_ID,
                                                                                                                ConfigurationManager.ApplicationConfiguration.GroupsManagerConfiguration.CacheTTLSeconds.DoubleValue,
                                                                                                                new List<string>() { invalidationKey }))
                {
                    log.ErrorFormat("Failed getting GetGroupPermissionItemsDictionary from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGroupPermissionItemsDictionary, groupId: {0}", groupId), ex);
            }

            return result;
        }

        private static bool IsPermittedForRoles(int groupId, string objectPropertyKey, List<long> roleIds, out string usersGroup)
        {
            usersGroup = null;
            StringBuilder usersGroupStringBuilder = new StringBuilder();

            // get group's roles schema            
            Dictionary<string, Dictionary<long, KeyValuePair<string, bool>>> permissionItemsDictionary = GetGroupPermissionItemsDictionary(groupId);

            // if the permission for the property is not defined in the schema - return false
            if (!permissionItemsDictionary.ContainsKey(objectPropertyKey))
            {
                return false;
            }

            Dictionary<long, KeyValuePair<string, bool>> roles = permissionItemsDictionary[objectPropertyKey];
            bool isPermitted = false;


            foreach (var roleId in roleIds)
            {
                // if the permission item for the action is part of one of the supplied roles - return true
                if (roles.ContainsKey(roleId))
                {
                    if (roles[roleId].Value)
                    {
                        isPermitted = false; // is excluded ! 
                        break;
                    }

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

        #endregion

        #region Internal Methods

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
                    ks.OriginalUserId = ks.UserId;
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

        #endregion

        #region Public Methods

        public static List<long> GetRoleIds(KS ks, bool appendOriginalRoles = true)
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

                // if the ks was originally of operator - get he's roles too
                if (appendOriginalRoles && !string.IsNullOrEmpty(ks.OriginalUserId))
                {
                    userRoleIds = ClientsManager.UsersClient().GetUserRoleIds(ks.GroupId, ks.OriginalUserId);
                    if (userRoleIds != null && userRoleIds.Count > 0)
                    {
                        roleIds.AddRange(userRoleIds);
                    }
                }
            }
            return roleIds;
        }

        #endregion

    }
}