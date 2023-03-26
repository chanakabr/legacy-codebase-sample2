using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects.Roles;
using CachingProvider.LayeredCache;
using Core.Users;
using DAL;
using KalturaRequestContext;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using Status = ApiObjects.Response.Status;
using UserModule = Core.Users.Module;

namespace APILogic.Api.Managers
{
    public interface IRolesPermissionsManager
    {
        bool IsPermittedPermission(int groupId, string userId, RolePermissions rolePermission);
        bool IsPermittedPermissionItem(int groupId, string userId, string permissionItem);
        bool AllowActionInSuspendedDomain(int groupId, long userId, bool isDefaultInheritanceType);
    }
    
    public class RolesPermissionsManager: IRolesPermissionsManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<RolesPermissionsManager> lazy = new Lazy<RolesPermissionsManager>(() => new RolesPermissionsManager(LayeredCache.Instance, RequestContextUtilsInstance.Get(), GeneralPartnerConfigManager.Instance, UserModule.Instance), LazyThreadSafetyMode.PublicationOnly);
        public static RolesPermissionsManager Instance => lazy.Value;

        private readonly ILayeredCache _layeredCache;
        private readonly IRequestContextUtils _requestContextUtils;
        private readonly IGeneralPartnerConfigManager _generalPartnerConfigManager;
        private readonly IUserModule _userModule;

        public RolesPermissionsManager(ILayeredCache layeredCache, IRequestContextUtils requestContextUtils, IGeneralPartnerConfigManager generalPartnerConfigManager, IUserModule userModule)
        {
            _layeredCache = layeredCache;
            _requestContextUtils = requestContextUtils;
            _generalPartnerConfigManager = generalPartnerConfigManager;
            _userModule = userModule;
        }

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

        private List<long> GetRoleIds(int groupId, string userId)
        {
            // ??????? we set list to be only anonymous at first but it is never used. something here smells fishy!
            List<long> roleIds = new List<long>() { PredefinedRoleId.ANONYMOUS };

            LongIdsResponse response = _userModule.GetUserRoleIds(groupId, long.Parse(userId));
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

        private static Tuple<List<Role>, bool> GetRolesByGroupId(Dictionary<string, object> funcParams)
        {
            List<Role> roles = new List<Role>();
            bool res = false;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
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

        public bool IsPermittedPermission(int groupId, string userId, RolePermissions rolePermission)
        {
            try
            {
                // Looks like this code was wrong. it should be reviewed by someone else
                //if (string.IsNullOrEmpty(userId) || userId == "0")// anonymous
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
                            if (userRoles.Any() && !userRoles.Any(x => x.Value))
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

        public bool IsPermittedPermissionItem(int groupId, string userId, string permissionItem)
        {
            bool result = false;
            try
            {
                if (string.IsNullOrEmpty(userId) || userId == "0")// anonymouse
                {
                    result = true;
                }
                else
                {
                    // get list of all user permissions 
                    List<Permission> permissions = GetUserPermissions(groupId, userId);
                    if (permissions != null && permissions.Any())
                    {
                        List<PermissionItem> permissionItems = permissions.Where(x => x.PermissionItems != null).SelectMany(x => x.PermissionItems).ToList();
                        if (permissionItems != null)
                        {
                            result = !permissionItems.Any(x => x.Name.ToLower() == permissionItem.ToLower() && x.IsExcluded);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed IsPermittedPermissionItem, groupId: {0}, userId: {1}, permissionItem: {2}", groupId, userId, permissionItem), ex);
            }

            return result;
        }

        private Dictionary<string, List<KeyValuePair<long, bool>>> GetPermissionsRolesByGroup(int groupId)
        {
            Dictionary<string, List<KeyValuePair<long, bool>>> result = null;
            try
            {
                List<Role> roles = GetRolesByGroupId(groupId);
                if (roles != null && roles.Any())
                {
                    result = BuildPermissionItemsDictionary(roles);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetPermissionRoleByGroup failed, groupId : {0}, ex : {1}", groupId, ex);
            }

            return result;
        }

        public List<Role> GetRolesByGroupId(int groupId)
        {
            List<Role> roles = null;
            try
            {
                string key = LayeredCacheKeys.GetPermissionsRolesIdsKey(groupId);
                string invalidationKey = LayeredCacheKeys.GetPermissionsRolesIdsInvalidationKey(groupId);
                string permissionsManagerInvalidationKey = LayeredCacheKeys.PermissionsManagerInvalidationKey();
                if (!_layeredCache.Get(
                    key,
                    ref roles,
                    GetRolesByGroupId,
                    new Dictionary<string, object> { { "groupId", groupId } },
                    groupId,
                    LayeredCacheConfigNames.GET_ROLES_BY_GROUP_ID,
                    new List<string> { invalidationKey, permissionsManagerInvalidationKey }))
                {
                    log.ErrorFormat("Failed getting GetRolesByGroupId from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetRolesByGroupId failed, groupId : {0}, ex : {1}", groupId, ex);
            }

            return roles;
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
                status = new ApiObjects.Response.Status((int)eResponseStatus.NotAllowed, "Not allowed for role id [@roleId@]", new List<ApiObjects.KeyValuePair>()
                        { new ApiObjects.KeyValuePair("roleId", domain.roleId.ToString()) });
            }
            else
            {
                status = new ApiObjects.Response.Status((int)eResponseStatus.NotAllowed, "Action not allowed");
            }

            return status;
        }

        public static Dictionary<long, Permission> GetGroupPermissions(int groupId, long? roleIdIn)
        {
            Dictionary<long, Permission> result = null;
            try
            {
                List<Role> roles = null;
                string key = LayeredCacheKeys.GetPermissionsRolesIdsKey(groupId);
                string invalidationKey = LayeredCacheKeys.GetPermissionsRolesIdsInvalidationKey(groupId);
                string permissionsManagerInvalidationKey = LayeredCacheKeys.PermissionsManagerInvalidationKey();
                if (!LayeredCache.Instance.Get<List<Role>>(key, ref roles, GetRolesByGroupId, new Dictionary<string, object>() { { "groupId", groupId } },
                                                        groupId, LayeredCacheConfigNames.GET_ROLES_BY_GROUP_ID, new List<string>() { invalidationKey, permissionsManagerInvalidationKey }))
                {
                    log.ErrorFormat("Failed getting GetGroupPermissions from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }

                if (roles != null && roles.Any())
                {
                    if (roleIdIn.HasValue)
                    {
                        roles = roles.Where(r => r.Id.Equals(roleIdIn.Value)).ToList();
                    }

                    result = roles.Where(x => x.Permissions != null).SelectMany(x => x.Permissions).GroupBy(x => x.Name).Select(x => new KeyValuePair<long, Permission>(x.First().Id, x.First())).ToDictionary(x => x.Key, x => x.Value);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGroupPermissions for groupId: {0}", groupId), ex);

            }

            return result;
        }

        public static List<Permission> GetUserPermissions(int groupId, string userId)
        {
            List<Permission> result = null;
            try
            {
                List<Role> roles = Instance.GetRolesByGroupId(groupId);

                if (roles != null && roles.Any())
                {
                    // get list of all user permissions 
                    List<long> userRoleIDs = Instance.GetRoleIds(groupId, userId.ToString());
                    if (userRoleIDs != null && userRoleIDs.Count > 0 && roles.Any(x => userRoleIDs.Contains(x.Id)))
                    {
                        result = roles.Where(x => userRoleIDs.Contains(x.Id)).SelectMany(x => x.Permissions).GroupBy(x => x.Name).Select(x => x.First()).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetUserPermissions for groupId: {0}, userId: {1}", groupId, userId), ex);

            }

            return result;
        }

        public static string GetCurrentUserPermissions(int groupId, string userId)
        {
            string result = null;
            try
            {
                List<Permission> permissions = GetUserPermissions(groupId, userId);
                if (permissions != null)
                {
                    result = string.Join(",", permissions.Where(x => !string.IsNullOrEmpty(x.Name)).OrderBy(x => x.Name).Select(x => x.Name));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetCurrentUserPermissions for groupId: {0}, userId: {1}", groupId, userId), ex);

            }

            return result;
        }

        public static bool SetAllInvalidaitonKeysRelatedPermissions(int groupId)
        {
            bool result = false;
            try
            {
                string permissionRolesInvalidationKey = LayeredCacheKeys.GetPermissionsRolesIdsInvalidationKey(groupId);
                string permissionItemsDictionaryInvalidationKey = LayeredCacheKeys.GetGroupPermissionItemsDictionaryInvalidationKey(groupId);
                result = LayeredCache.Instance.SetInvalidationKey(permissionRolesInvalidationKey) && LayeredCache.Instance.SetInvalidationKey(permissionItemsDictionaryInvalidationKey);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("SetAllInvalidaitonKeysRelatedPermissions failed, groupId : {0}", groupId), ex);
            }

            return result;
        }

        public static Dictionary<string, Permission> GetGroupFeatures(int groupId)
        {
            Dictionary<string, Permission> result = new Dictionary<string, Permission>();

            try
            {
                string key = LayeredCacheKeys.GetGroupFeaturesKey(groupId);
                string invalidationKey = LayeredCacheKeys.GetGroupPermissionItemsDictionaryInvalidationKey(groupId);
                if (!LayeredCache.Instance.Get<Dictionary<string, Permission>>(key, ref result, BuildGroupFeatures,
                                                                                                                new Dictionary<string, object>() { { "groupId", groupId } }, groupId,
                                                                                                                LayeredCacheConfigNames.GET_GROUP_FEATURES,
                                                                                                                new List<string>() { invalidationKey }))
                {
                    log.ErrorFormat("Failed getting GetGroupFeatures from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGroupFeatures, groupId: {0}", groupId), ex);
            }

            return result;

        }

        private static Tuple<Dictionary<string, Permission>, bool> BuildGroupFeatures(Dictionary<string, object> funcParams)
        {
            Dictionary<string, Permission> result = new Dictionary<string, Permission>();
            bool success = false;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                    {
                        result = GetGroupPermissionFeatures(groupId.Value);
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("BuildPermissionItemsToFeaturesDictionary failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, Permission>, bool>(result, success);
        }

        public static Dictionary<string, Permission> GetGroupPermissionFeatures(int groupId)
        {
            Dictionary<string, Permission> features = new Dictionary<string, Permission>();
            Permission permission = null;

            try
            {
                DataTable dt = ApiDAL.GetGroupFeatures(groupId);
                if (dt?.Rows?.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        permission = new Permission()
                        {
                            Name = ODBCWrapper.Utils.GetSafeStr(dr, "name"),
                            DependsOnPermissionNames = ODBCWrapper.Utils.GetSafeStr(dr, "DEPENDS_ON_PERMISSION_NAMES"),
                            FriendlyName = ODBCWrapper.Utils.GetSafeStr(dr, "FRIENDLY_NAME"),
                            Id = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID"),
                            Type = ePermissionType.SpecialFeature
                        };

                        if (!string.IsNullOrEmpty(permission.Name) && !features.ContainsKey(permission.Name.ToLower()))
                        {
                            features.Add(permission.Name.ToLower(), permission);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while getting Group features. group id = {0}", groupId), ex);
            }

            return features;
        }

        public static bool IsAllowedToViewInactiveAssets(int groupId, string userId, bool ignoreDoesGroupUsesTemplates = false)
        {
            return Instance.IsPermittedPermission(groupId, userId, RolePermissions.VIEW_INACTIVE_ASSETS) &&
                (DoesGroupUsesTemplates(groupId) || ignoreDoesGroupUsesTemplates);
        }
        private static bool DoesGroupUsesTemplates(int groupId)
        {
            return Core.Catalog.CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
        }

        public static GenericListResponse<PermissionItem> GetPermissionItemList(PermissionItemFilter filter, CorePager pager)
        {
            GenericListResponse<PermissionItem> response = new GenericListResponse<PermissionItem>();
            response.Objects = GetAllPermissionItems();
            response.TotalItems = response.Objects.Count;
            response.Objects = response.Objects.Skip(pager.PageIndex * pager.PageSize).Take(pager.PageSize).ToList();
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        public static GenericListResponse<PermissionItem> GetPermissionItemList(PermissionItemByIdInFilter filter, CorePager pager)
        {
            GenericListResponse<PermissionItem> response = new GenericListResponse<PermissionItem>();
            response.Objects = GetAllPermissionItems();
            response.Objects = response.Objects.Where(pi => filter.IdIn.Contains(pi.Id)).ToList();
            response.TotalItems = response.Objects.Count;
            response.Objects = response.Objects.Skip(pager.PageIndex * pager.PageSize).Take(pager.PageSize).ToList();
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        public static GenericListResponse<PermissionItem> GetPermissionItemList(PermissionItemByApiActionFilter filter, CorePager pager)
        {
            GenericListResponse<PermissionItem> response = new GenericListResponse<PermissionItem>();
            response.Objects = GetAllPermissionItems();
            response.Objects = response.Objects.Where(pi => pi.GetPermissionItemType() == ePermissionItemType.Action &&
                (string.IsNullOrEmpty(filter.Action) || ((ApiActionPermissionItem)pi).Action.ToLower() == filter.Action.ToLower()) &&
                ((string.IsNullOrEmpty(filter.Service) || ((ApiActionPermissionItem)pi).Service.ToLower() == filter.Service.ToLower()))).ToList();
            response.TotalItems = response.Objects.Count;
            response.Objects = response.Objects.Skip(pager.PageIndex * pager.PageSize).Take(pager.PageSize).ToList();
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        public static GenericListResponse<PermissionItem> GetPermissionItemList(PermissionItemByArgumentFilter filter, CorePager pager)
        {
            GenericListResponse<PermissionItem> response = new GenericListResponse<PermissionItem>();
            response.Objects = GetAllPermissionItems();
            response.Objects = response.Objects.Where(pi => pi.GetPermissionItemType() == ePermissionItemType.Argument &&
                (string.IsNullOrEmpty(filter.Parameter) || ((ApiArgumentPermissionItem)pi).Parameter.ToLower() == filter.Parameter.ToLower()) &&
                (string.IsNullOrEmpty(filter.Action) || ((ApiArgumentPermissionItem)pi).Action.ToLower() == filter.Action.ToLower()) &&
                (string.IsNullOrEmpty(filter.Service) || ((ApiArgumentPermissionItem)pi).Service.ToLower() == filter.Service.ToLower())).ToList();
            response.TotalItems = response.Objects.Count;
            response.Objects = response.Objects.Skip(pager.PageIndex * pager.PageSize).Take(pager.PageSize).ToList();
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        public static GenericListResponse<PermissionItem> GetPermissionItemList(PermissionItemByParameterFilter filter, CorePager pager)
        {
            GenericListResponse<PermissionItem> response = new GenericListResponse<PermissionItem>();
            response.Objects = GetAllPermissionItems();
            response.Objects = response.Objects.Where(pi => pi.GetPermissionItemType() == ePermissionItemType.Parameter &&
                (string.IsNullOrEmpty(filter.Parameter) || ((ApiParameterPermissionItem)pi).Parameter.ToLower() == filter.Parameter.ToLower()) &&
                (string.IsNullOrEmpty(filter.Object) || ((ApiParameterPermissionItem)pi).Object.ToLower() == filter.Object.ToLower())).ToList();
            response.TotalItems = response.Objects.Count;
            response.Objects = response.Objects.Skip(pager.PageIndex * pager.PageSize).Take(pager.PageSize).ToList();
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        public static ApiObjects.Response.Status AddPermissionItemToPermission(int groupId, long permissionId, long permissionItemId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status(eResponseStatus.Error);

            try
            {
                PermissionItem permissionItem = RolesPermissionsManager.GetPermissionItem(permissionItemId);
                if (permissionItem == null)
                {
                    response.Set(eResponseStatus.PermissionItemNotFound);
                    return response;
                }

                Permission permission = RolesPermissionsManager.GetPermission(groupId, permissionId);
                if (permission == null)
                {
                    response.Set(eResponseStatus.PermissionNotFound);
                    return response;
                }

                if (permission.GroupId == 0)
                {
                    response.Set(eResponseStatus.PermissionReadOnly);
                    return response;
                }

                if (permission.PermissionItemsIds?.Count > 0 && permission.PermissionItemsIds.Contains(permissionItemId))
                {
                    response.Set(eResponseStatus.PermissionPermissionItemAlreadyExists);
                    return response;
                }

                int id = DAL.ApiDAL.InsertPermissionPermissionItem(groupId, permissionId, permissionItemId);
                if (id > 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }

                if (!APILogic.Api.Managers.RolesPermissionsManager.SetAllInvalidaitonKeysRelatedPermissions(groupId))
                {
                    log.DebugFormat("Failed to set AllInvalidaitonKeysRelatedPermissions, groupId: {0}", groupId);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while adding permission item to permission. group id = {0}", groupId), ex);
            }

            return response;
        }

        public static ApiObjects.Response.Status RemovePermissionItemFromPermission(int groupId, long permissionId, long permissionItemId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status(eResponseStatus.Error);

            try
            {
                PermissionItem permissionItem = RolesPermissionsManager.GetPermissionItem(permissionItemId);
                if (permissionItem == null)
                {
                    response.Set(eResponseStatus.PermissionItemNotFound);
                    return response;
                }

                Permission permission = RolesPermissionsManager.GetPermission(groupId, permissionId);
                if (permission == null)
                {
                    response.Set(eResponseStatus.PermissionNotFound);
                    return response;
                }

                if (permission.GroupId == 0)
                {
                    response.Set(eResponseStatus.PermissionReadOnly);
                    return response;
                }

                if (permission.PermissionItemsIds?.Count == 0 && !permission.PermissionItemsIds.Contains(permissionItemId))
                {
                    response.Set(eResponseStatus.PermissionPermissionItemNotFound);
                    return response;
                }

                int rowCount = DAL.ApiDAL.DeletePermissionItemFromPermission(groupId, permissionId, permissionItemId);
                if (rowCount > 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }

                if (!APILogic.Api.Managers.RolesPermissionsManager.SetAllInvalidaitonKeysRelatedPermissions(groupId))
                {
                    log.DebugFormat("Failed to set AllInvalidaitonKeysRelatedPermissions, groupId: {0}", groupId);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while deleting permission item from permission. group id = {0}", groupId), ex);
            }

            return response;
        }

        private static List<PermissionItem> GetAllPermissionItems()
        {
            List<PermissionItem> items = new List<PermissionItem>();
            var roles = Instance.GetRolesByGroupId(0);

            HashSet<long> ids = new HashSet<long>();

            foreach (var role in roles)
            {
                foreach (var permisson in role.Permissions)
                {
                    if (permisson.PermissionItems?.Count > 0)
                    {
                        foreach (var permissonItem in permisson.PermissionItems)
                        {
                            if (!ids.Contains(permissonItem.Id))
                            {
                                ids.Add(permissonItem.Id);
                                items.Add(permissonItem);
                            }
                        }
                    }
                }
            }

            return items.OrderBy(x => x.Id).ToList();
        }

        internal static Permission GetPermission(int groupId, long permissionId)
        {
            Permission permission = null;

            var customPermissions = GetGroupPermissions(groupId);
            if (customPermissions?.Count > 0 && customPermissions.ContainsKey(permissionId))
            {
                return customPermissions[permissionId];
            }

            var otherPermissions = GetGroupPermissions(groupId, null);
            if (otherPermissions?.Count > 0 && otherPermissions.ContainsKey(permissionId))
            {
                return otherPermissions[permissionId];
            }

            return permission;
        }

        internal static PermissionItem GetPermissionItem(long permissionItemId)
        {
            var allPermissionItems = GetAllPermissionItems();
            PermissionItem permissionItem = allPermissionItems.FirstOrDefault(x => x.Id == permissionItemId);

            return permissionItem;
        }

        public static Dictionary<long, Permission> GetGroupPermissions(int groupId)
        {
            Dictionary<long, Permission> result = new Dictionary<long, Permission>();

            try
            {
                string key = LayeredCacheKeys.GetGroupPermissionsKey(groupId);
                string invalidationKey = LayeredCacheKeys.GetGroupPermissionItemsDictionaryInvalidationKey(groupId);
                if (!LayeredCache.Instance.Get<Dictionary<long, Permission>>(key, ref result, BuildGroupPermissions,
                                                                                                                new Dictionary<string, object>() { { "groupId", groupId } }, groupId,
                                                                                                                LayeredCacheConfigNames.GET_GROUP_PERMISSIONS,
                                                                                                                new List<string>() { invalidationKey }))
                {
                    log.ErrorFormat("Failed getting GetGroupPermissions from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGroupPermissions, groupId: {0}", groupId), ex);
            }

            return result;
        }

        private static Tuple<Dictionary<long, Permission>, bool> BuildGroupPermissions(Dictionary<string, object> funcParams)
        {
            Dictionary<long, Permission> result = new Dictionary<long, Permission>();
            bool success = false;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                    {
                        result = BuildGroupPermissions(groupId.Value);
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("BuildPermissionItemsToFeaturesDictionary failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<long, Permission>, bool>(result, success);
        }

        public static Dictionary<long, Permission> BuildGroupPermissions(int groupId)
        {
            Dictionary<long, Permission> permissions = new Dictionary<long, Permission>();
            Permission permission;
            PermissionItem permissionItem;

            try
            {
                DataTable dt = ApiDAL.GetGroupPermissions(groupId);
                if (dt?.Rows?.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var id = ODBCWrapper.Utils.GetIntSafeVal(dr, "PERMISSION_ID");

                        if (id > 0)
                        {
                            if (!permissions.ContainsKey(id))
                            {
                                permission = new Permission()
                                {
                                    Name = ODBCWrapper.Utils.GetSafeStr(dr, "PERMISSION_NAME"),
                                    DependsOnPermissionNames = ODBCWrapper.Utils.GetSafeStr(dr, "DEPENDS_ON_PERMISSION_NAMES"),
                                    FriendlyName = ODBCWrapper.Utils.GetSafeStr(dr, "FRIENDLY_NAME"),
                                    Id = id,
                                    GroupId = ODBCWrapper.Utils.GetIntSafeVal(dr, "GROUP_ID"),
                                    Type = (ePermissionType)ODBCWrapper.Utils.GetIntSafeVal(dr, "TYPE"),
                                    PermissionItemsIds = new List<long>()
                                };

                                permissions.Add(permission.Id, permission);
                            }

                            var permissionItemId = ODBCWrapper.Utils.GetIntSafeVal(dr, "PERMISSION_ITEM_ID");
                            if (permissionItemId > 0)
                            {
                                if (ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_EXCLUDED") == 1)
                                {
                                    if (permissions[id].PermissionItemsIds.Contains(permissionItemId))
                                    {
                                        permissions[id].PermissionItemsIds.Remove(permissionItemId);
                                    }
                                }
                                else
                                {
                                    permissions[id].PermissionItemsIds.Add(permissionItemId);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while getting Group permissions. group id = {0}", groupId), ex);
            }

            return permissions;
        }

        public static GenericListResponse<Permission> GetPermissions(int groupId, List<long> permissionIds)
        {

            GenericListResponse<Permission> response = new GenericListResponse<Permission>();
            response.Objects = GetGroupPermissions(groupId, null).Values.ToList();
            response.Objects = response.Objects.Where(pi => permissionIds.Contains(pi.Id)).ToList();
            response.TotalItems = response.Objects.Count;
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        public bool AllowActionInSuspendedDomain(int groupId, long userId, bool isDefaultInheritanceType = false)
        {
            return AllowActionToPartner(groupId, isDefaultInheritanceType)
                   || AllowActionToUser(groupId, userId);
        }

        private bool AllowActionToPartner(int groupId, bool isDefault)
        {
            var result = false;
            
            if (_requestContextUtils.IsPartnerRequest())
            {
                var inheritanceType = _generalPartnerConfigManager.GetGeneralPartnerConfig(groupId)?.SuspensionProfileInheritanceType;
                result = inheritanceType == SuspensionProfileInheritanceType.Default && isDefault
                         || inheritanceType == SuspensionProfileInheritanceType.Never;
            }

            return result;
        }

        private bool AllowActionToUser(int groupId, long userId)
        {
            var result = false;

            var userData = _userModule.GetUserData(groupId, userId, string.Empty);
            if (userData.m_RespStatus == ResponseStatus.OK && userData.m_user.m_domianID != 0)
            {
                result = IsPermittedPermission(groupId, userId.ToString(), RolePermissions.ALLOW_ACTION_IN_SUSPENDED_DOMAIN);
            }

            return result;
        }
    }
}
