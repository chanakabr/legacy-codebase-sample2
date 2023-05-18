using Amazon.S3.Model;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using KalturaRequestContext;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Controllers;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.General;
using ApiObjects.Roles;

namespace WebAPI.Managers
{
    public class RolesManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string PARTNER_WILDCARD = "partner*";
        private const string HOUSEHOLD_WILDCARD = "household*";
        private const string IS_PROPERTY_PERMITTED = "{0}_{1}_{2}_{3}";
        public const string EXTERNAL_EDITOR_ROLE_NAME = "ExternalEditor";
        public const int PROVISIONING_SYSTEM_ROLE_ID = 18;

        #region Private Methods

        private static KS getKS(eKSValidation validationState)
        {
            KS ks = KS.GetFromRequest();

            if (ks == null)
                throw new UnauthorizedException(UnauthorizedException.SERVICE_FORBIDDEN);

            if (!AuthorizationManager.IsAuthorized(ks, validationState))
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
                            KalturaApiPriviligesPermissionItem apiPriviligesPermissionItem;
                            string usersGroup;

                            foreach (KalturaUserRole role in groupUserRoles)
                            {
                                foreach (KalturaPermission permission in role.Permissions)
                                {
                                    bool isExcluded = role.ExcludedPermissionNames.Contains(permission.Name);

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
                                        else if (permissionItem is KalturaApiPriviligesPermissionItem)
                                        {
                                            apiPriviligesPermissionItem = (KalturaApiPriviligesPermissionItem)permissionItem;
                                            keyToExcludeMap.Add(string.Format("{0}_{1}", apiPriviligesPermissionItem.Object, apiPriviligesPermissionItem.Parameter).ToLower(), apiPriviligesPermissionItem.IsExcluded);
                                        }
                                        else
                                        {
                                            log.DebugFormat("Failed to get permission item type");
                                            continue;
                                        }

                                        foreach (KeyValuePair<string, bool> pair in keyToExcludeMap)
                                        {
                                            isExcluded |= pair.Value;

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
                if (!LayeredCache.Instance.Get<Dictionary<string, Dictionary<long, KeyValuePair<string, bool>>>>(key, ref result, BuildGroupPermissionItemsDictionary,
                                                                                                                new Dictionary<string, object>() { { "groupId", groupId } }, groupId,
                                                                                                                LayeredCacheConfigNames.GET_GROUP_PERMISSION_ITEMS_BY_GROUP_ID,
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
            // TODO: this if should be deleted when all "OldStandard" methods will be removed
            var oldStandardIndex = action.IndexOf("OldStandard");
            if (oldStandardIndex > -1) 
            {
                action = action.Remove(oldStandardIndex);
            }

            // build the key for the service action key for roles schema (permission items - roles dictionary)
            string methodArgumentKey = string.Format("{0}_{1}_{2}", service, action, argument).ToLower();
            return IsPermittedForRoles(groupId, methodArgumentKey, roleIds, out usersGroup) && IsPermittedForFeature(groupId, methodArgumentKey);
        }

        private static bool IsPropertyPermittedForRoles(int groupId, string type, string property, string action, List<long> roleIds, out string usersGroup)
        {
            usersGroup = null;
            StringBuilder usersGroupStringBuilder = new StringBuilder();

            // build the key for the service action key for roles schema (permission items - roles dictionary)
            string objectPropertyKey = string.Format("{0}_{1}_{2}", type, property, action).ToLower();
            return IsPermittedForRoles(groupId, objectPropertyKey, roleIds, out usersGroup) && IsPermittedForFeature(groupId, objectPropertyKey);
        }

        private static bool IsActionPermittedForRoles(int groupId, string service, string action, List<long> roleIds, out string usersGroup)
        {
            usersGroup = null;
            StringBuilder usersGroupStringBuilder = new StringBuilder();

            // build the key for the service action key for roles schema (permission items - roles dictionary)
            string serviceActionKey = string.Format("{0}_{1}", service, action).ToLower();
            return IsPermittedForRoles(groupId, serviceActionKey, roleIds, out usersGroup) && IsPermittedForFeature(groupId, serviceActionKey);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Checks if an action from a service is allowed
        /// </summary>
        /// <param name="service">Service name</param>
        /// <param name="action">Action name</param>
        /// <param name="validationState">All, Expiration, None - level of KS validation</param>
        internal static void ValidateActionPermitted(string service, string action, eKSValidation validationState = eKSValidation.All)
        {
            KS ks = getKS(validationState);
            List<long> roleIds = GetRoleIds(ks);

            // no roles found for the user
            if (roleIds == null || roleIds.Count == 0)
                throw new UnauthorizedException(UnauthorizedException.SERVICE_FORBIDDEN);

            //BEO-7703 - No cache for operator+
            if (IsPartner(ks.GroupId, roleIds))
            {
                RequestContextUtilsInstance.Get().SetIsPartnerRequest();
            }

            string allowedUsersGroup = null;

            // user not permitted
            if (!IsActionPermittedForRoles(ks.GroupId, service, action, roleIds, out allowedUsersGroup))
                throw new UnauthorizedException(UnauthorizedException.SERVICE_FORBIDDEN);

            // allowed group users (additional user_id) handling:
            // get user_id additional parameter
            string userId = null;
            if (RequestContextUtilsInstance.Get().IsImpersonateRequest() && HttpContext.Current.Items.ContainsKey(RequestContextConstants.REQUEST_USER_ID))
            {
                var extraUserId = HttpContext.Current.Items[RequestContextConstants.REQUEST_USER_ID];
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

        public static bool IsPartner(int groupId, List<long> roleIds)
            => IsAnyRoleWithProfileType(groupId, roleIds, KalturaUserRoleProfile.PARTNER, KalturaUserRoleProfile.SYSTEM);

        public static bool IsProvisioningSystemRole(IEnumerable<long> roleIds)
            => roleIds?.Any(x => x == PROVISIONING_SYSTEM_ROLE_ID) == true;

        private static bool IsAnyRoleWithProfileType(
            int groupId,
            List<long> roleIds,
            params KalturaUserRoleProfile[] profileTypes)
        {
            var roles = ClientsManager.ApiClient().GetRoles(groupId, roleIds);

            return roles != null
                && roles.Any(x => Array.Exists(profileTypes, p => p == x.Profile));
        }

        /// <summary>
        /// Checks if a property of an object is allowed
        /// </summary>
        /// <param name="type">Type name</param>
        /// <param name="property">Property name</param>
        /// <param name="action">Required action</param>
        /// <param name="validationState">All, Expiration, None - level of KS validation</param>
        /// <returns>True if the property is permitted, false otherwise</returns>
        internal static void ValidatePropertyPermitted(string type, string property, RequestType action, eKSValidation validationState = eKSValidation.All)
        {
            KS ks;

            try
            {
                ks = getKS(validationState);
            }
            catch (UnauthorizedException ex)
            {
                if (ex.Code == (int)StatusCode.ServiceForbidden)
                {
                    throw new UnauthorizedException(UnauthorizedException.PROPERTY_ACTION_FORBIDDEN, Enum.GetName(typeof(RequestType), action), type, property);
                }

                throw;
            }
            
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
        /// <param name="type">Type name</param>
        /// <param name="property">Property name</param>
        /// <param name="action">Required action</param>
        /// <param name="validationState">All, Expiration, None - level of KS validation</param>
        /// <returns>True if the property is permitted, false otherwise</returns>
        internal static bool IsPropertyPermitted(string type, string property, RequestType action, eKSValidation validationState = eKSValidation.All)
        {
            bool result = false;
            string key = string.Format(IS_PROPERTY_PERMITTED, type, property, action, validationState);
            if (HttpContext.Current != null && HttpContext.Current.Items != null && HttpContext.Current.Items.ContainsKey(key))
            {
                return (bool)HttpContext.Current.Items[key];
            }
            else
            {
                try
                {
                    ValidatePropertyPermitted(type, property, action, validationState);
                    result = true;
                }
                catch
                {
                    result = false;
                }

                HttpContext.Current.Items[key] = result;
                return result;
            }            
        }

        /// <summary>
        /// Checks if a property of an object is allowed
        /// </summary>
        /// <param name="service">Controller service name</param>
        /// <param name="action">Method action name</param>
        /// <param name="argument">Argument name</param>
        /// <param name="validationState">All, Expiration, None - level of KS validation</param>
        /// <returns>True if the method argument is permitted, false otherwise</returns>
        internal static void ValidateArgumentPermitted(string service, string action, string argument, eKSValidation validationState = eKSValidation.All)
        {
            KS ks = getKS(validationState);
            List<long> roleIds = GetRoleIds(ks);

            // no roles found for the user
            if (roleIds == null || roleIds.Count == 0)
                throw new UnauthorizedException(UnauthorizedException.ACTION_ARGUMENT_FORBIDDEN, argument, service, action);

            string allowedUsersGroup = null;

            // user not permitted
            if (!IsArgumentPermittedForRoles(ks.GroupId, service, action, argument, roleIds, out allowedUsersGroup))
                throw new UnauthorizedException(UnauthorizedException.ACTION_ARGUMENT_FORBIDDEN, argument, service, action);
        }

        internal static bool IsPermittedForFeature(int groupId, string key)
        {
            // get group's permission items to Features schema            
            Dictionary<string, List<string>> permissionItemsToFeaturesMap = GetPermissionItemsToFeaturesDictionary(groupId);

            // if the permission for the property is not defined in the schema - return false
            if (!permissionItemsToFeaturesMap.ContainsKey(key))
            {
                return true;
            }

            List<string> groupFeatures = ClientsManager.ApiClient().GetGroupFeatures(groupId);
            if (groupFeatures?.Count > 0)
            {
                bool result = groupFeatures.Count(x => permissionItemsToFeaturesMap[key].Contains(x)) > 0;

                if (!result)
                {
                    log.Debug($"Group {groupId} does not have relevant feature for {key}");
                }

                return result;
            }
            else
            {
                log.Debug($"Group {groupId} has no features.");
            }

            return false;
        }

        private static Dictionary<string, List<string>> GetPermissionItemsToFeaturesDictionary(int groupId)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            try
            {
                string key = LayeredCacheKeys.GetPermissionItemsToFeaturesDictionaryKey(groupId);
                string invalidationKey = LayeredCacheKeys.GetGroupPermissionItemsDictionaryInvalidationKey(groupId);
                if (!LayeredCache.Instance.Get<Dictionary<string, List<string>>>(key, ref result, BuildPermissionItemsToFeaturesDictionary,
                                                                                                                new Dictionary<string, object>() { { "groupId", groupId } }, groupId,
                                                                                                                LayeredCacheConfigNames.GET_PERMISSION_ITEMS_TO_FEATURES,
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

        private static Tuple<Dictionary<string, List<string>>, bool> BuildPermissionItemsToFeaturesDictionary(Dictionary<string, object> funcParams)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            bool success = false;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                    {
                        result = ClientsManager.ApiClient().GetPermissionItemsToFeatures(groupId.Value);
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("BuildPermissionItemsToFeaturesDictionary failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, List<string>>, bool>(result, success);
        }

        #endregion

        #region Public Methods

        public static List<long> GetRoleIds(KS ks, bool appendOriginalRoles = true)
        {
            List<long> roleIds = new List<long>() { PredefinedRoleId.ANONYMOUS };

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
                    var originalRoleIds = ClientsManager.UsersClient().GetUserRoleIds(ks.GroupId, ks.OriginalUserId);
                    if (originalRoleIds != null && originalRoleIds.Count > 0)
                    {
                        roleIds.AddRange(originalRoleIds);
                    }
                }
            }
            return roleIds;
        }

        public static bool IsManagerAllowedAction(KS ks, List<long> roleIds)
        {
            // check role's hierarchy
            var isManager = GetRoleIds(ks).Any(ur => ur == PredefinedRoleId.MANAGER);

            if (isManager)
            {
                //BEO-13980 - Allow manager to create sis users & operator
                var validRoleIdsForManager = PredefinedRoleId.GetManagerAllowedRoleIds();

                // Get External editor Role Id. (manager should be able to update it's role)
                var externalEditorRole = GetEERole(ks);

                if (externalEditorRole.HasValue)
                    validRoleIdsForManager.Add(externalEditorRole.Value);
                
                return roleIds.All(x => validRoleIdsForManager.Contains(x));
            }

            return true;
        }
        
        public static bool IsOperatorAllowedAction(KS ks, List<long> roleIds)
        {
            //BEO-13980 - Allow operator to create sis users
            var isOperator = GetRoleIds(ks).Any(ur => ur == PredefinedRoleId.OPERATOR);

            if (isOperator)
            {
                var shopManagerRoleIds = PredefinedRoleId.GetShopManagerRoleIds();
                if (!roleIds.All(x => shopManagerRoleIds.Contains(x)))
                    return false;
            }

            return true;
        }

        public static bool IsAllowedDeleteAction()
        {
            // check role's hierarchy 
            var ks = KS.GetFromRequest();
            string originalUserId = string.IsNullOrEmpty(ks.OriginalUserId) ? ks.UserId : ks.OriginalUserId;

            if (ks.UserId != "0")
            {
                if (originalUserId != ks.UserId)
                {
                    // get original user's roles
                    var userRoleIds = ClientsManager.UsersClient().GetUserRoleIds(ks.GroupId, originalUserId);
                    if (userRoleIds?.Count > 0)
                    {
                        long maxRole = userRoleIds.Max();

                        if (userRoleIds.Any(ur => ur == PredefinedRoleId.MANAGER || ur == PredefinedRoleId.OPERATOR))
                        {
                            userRoleIds = ClientsManager.UsersClient().GetUserRoleIds(ks.GroupId, ks.UserId);

                            // Get External editor Role Id. ( manager should be able to update it's role)
                            long? externalEditorRole = GetEERole(ks);

                            if (externalEditorRole.HasValue)
                            {
                                if (userRoleIds?.Count > 0 && userRoleIds.Any(x => x > maxRole && x != externalEditorRole.Value))
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                if (userRoleIds?.Count > 0 && userRoleIds.Any(x => x > maxRole))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsManagerAllowedUpdateAction(string userId, List<long> roleIds)
        {
            if (roleIds == null || roleIds.Count == 0)
            {
                return true;
            }

            // check role's hierarchy 
            var ks = KS.GetFromRequest();
            var ksRoleIds = GetRoleIds(ks);

            if (ksRoleIds == null)
            {
                return false;
            }

            bool isSysAdmin = ksRoleIds.Any(ur => ur == PredefinedRoleId.SYSTEM_ADMINISTRATOR);
            bool isAdmin = ksRoleIds.Any(ur => ur == PredefinedRoleId.ADMINISTRATOR);
            bool isManager = ksRoleIds.Any(ur => ur == PredefinedRoleId.MANAGER);

            if (isManager)
            {
                List<long> userRoleIds = ClientsManager.UsersClient().GetUserRoleIds(ks.GroupId, userId);

                if (!isSysAdmin && userRoleIds?.Count > 0 && userRoleIds.Any(x => x == PredefinedRoleId.SYSTEM_ADMINISTRATOR))
                {
                    return false;
                }

                if (!isAdmin && userRoleIds?.Count > 0 && userRoleIds.Any(x => x == PredefinedRoleId.ADMINISTRATOR))
                {
                    return false;
                }

                //check if update needed
                if (userRoleIds?.Count > 0 && userRoleIds.Count == roleIds.Count)
                {
                    int intersect = roleIds.Intersect(userRoleIds).Count<long>();
                    if (intersect == roleIds.Count)
                    {
                        //no changes
                        return true;
                    }
                }

                if (!isSysAdmin && roleIds.Any(x => x == PredefinedRoleId.SYSTEM_ADMINISTRATOR))
                {
                    return false;
                }

                if (!isAdmin && roleIds.Any(x => x == PredefinedRoleId.ADMINISTRATOR))
                {
                    return false;
                }
                
                if (!isAdmin && roleIds.Any(x => x == PredefinedRoleId.DMS_OPERATOR))
                {
                    return false;
                }
            }

            return true;
        }

        public static long? GetEERole(KS ks)
        {
            long? externalEditorRole = null;

            if (ks != null)
            {
                List<KalturaUserRole> list = ClientsManager.ApiClient().GetRoles(ks.GroupId);
                if (list?.Count > 0)
                {
                    externalEditorRole = list.FirstOrDefault(x => x.Name == EXTERNAL_EDITOR_ROLE_NAME).Id;
                }
            }

            return externalEditorRole;
        }

        #endregion

    }
}