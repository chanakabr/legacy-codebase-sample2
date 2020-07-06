using ApiLogic.Base;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ApiLogic.Users.Managers
{
    public class PasswordPolicyManager : ICrudHandler<PasswordPolicy, long>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<PasswordPolicyManager> lazy = new Lazy<PasswordPolicyManager>(() => new PasswordPolicyManager());
        public static PasswordPolicyManager Instance { get { return lazy.Value; } }

        private PasswordPolicyManager() { }

        public GenericResponse<PasswordPolicy> Add(ContextData contextData, PasswordPolicy objectToAdd)
        {
            var response = new GenericResponse<PasswordPolicy>();
            try
            {
                response = ValidateCrudObject(contextData, 0, objectToAdd);
                if (!response.IsOkStatusCode())
                {
                    return response;
                }

                var couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);
                objectToAdd.Id = (long)couchbaseManager.Increment("password_policy_sequence", 1);
                if (objectToAdd.Id == 0)
                {
                    log.ErrorFormat("Error setting Password Policy id");
                    return response;
                }

                var modifiedRoleIdsResponse = GetModifiedRoleIds(contextData, objectToAdd.Id, null, out Dictionary<long, HashSet<long>> rolesToPasswordPolicyMap, objectToAdd.UserRoleIds);
                if (!modifiedRoleIdsResponse.HasObject())
                {
                    response.SetStatus(modifiedRoleIdsResponse.Status);
                    return response;
                }

                if (!UsersDal.SavePasswordPolicy(objectToAdd))
                {
                    log.ErrorFormat($"Error while saving Password Policy. Policy: {JsonConvert.SerializeObject(objectToAdd)}, Status: {response.Status}");
                    return response;
                }

                if (!UsersDal.SaveUserRolesToPasswordPolicy(contextData.GroupId, rolesToPasswordPolicyMap))
                {
                    log.ErrorFormat($"Error while saving Password Policy dictionary.");
                    return response;
                }

                SetInvalidationKeys(contextData.GroupId, objectToAdd.UserRoleIds);

                response.Object = objectToAdd;
                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in Add. contextData:{contextData}, passwordPolicy:{JsonConvert.SerializeObject(objectToAdd)}", ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        public GenericResponse<PasswordPolicy> Update(ContextData contextData, PasswordPolicy objectToUpdate)
        {
            var response = new GenericResponse<PasswordPolicy>();
            try
            {
                response = ValidateCrudObject(contextData, objectToUpdate.Id, objectToUpdate);
                if (!response.HasObject())
                {
                    return response;
                }

                var modifiedRoleIdsResponse = GetModifiedRoleIds(contextData, objectToUpdate.Id, response.Object.UserRoleIds,
                    out Dictionary<long, HashSet<long>> rolesToPasswordPolicyMap, objectToUpdate.UserRoleIds);

                if (!modifiedRoleIdsResponse.HasObject())
                {
                    response.SetStatus(modifiedRoleIdsResponse.Status);
                    return response;
                }

                var needToUpdate = objectToUpdate.CompareAndFill(response.Object); //update current instead

                if (modifiedRoleIdsResponse.Object.Count != 0)
                {
                    if (!UsersDal.SaveUserRolesToPasswordPolicy(contextData.GroupId, rolesToPasswordPolicyMap))
                    {
                        log.Error($"Couldn't save user role to password policy in CB, groupId: " +
                            $"{contextData.GroupId}, map: {JsonConvert.SerializeObject(rolesToPasswordPolicyMap)}");
                    }
                }
                else if (needToUpdate)
                {
                    modifiedRoleIdsResponse.Object = objectToUpdate.UserRoleIds;
                }

                response.SetStatus(eResponseStatus.OK);
                if (modifiedRoleIdsResponse.Object.Count > 0)//any change in object
                {
                    if (!UsersDal.SavePasswordPolicy(objectToUpdate))
                    {
                        log.Error($"SavePasswordPolicy");
                        response.SetStatus(eResponseStatus.Error);
                    }
                    else
                    {
                        SetInvalidationKeys(contextData.GroupId, modifiedRoleIdsResponse.Object);
                    }
                }

                response.Object = objectToUpdate;
            }
            catch (Exception ex)
            {
                log.Error($"Error updating password policy due to: {ex.Message}");
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            var response = Status.Error;

            try
            {
                var validationResponse = ValidateCrudObject(contextData, id);
                if (!validationResponse.HasObject())
                {
                    response.Set(validationResponse.Status);
                    return response;
                }

                var modifiedRoleIdsResponse = GetModifiedRoleIds(contextData, id, validationResponse.Object.UserRoleIds,
                    out Dictionary<long, HashSet<long>> roleIdsToPolicyMap);

                if (!modifiedRoleIdsResponse.HasObject())
                {
                    response.Set(modifiedRoleIdsResponse.Status);
                    return response;
                }

                if (!UsersDal.DeleteUserRolesToPasswordPolicy(contextData.GroupId, roleIdsToPolicyMap))
                {
                    log.ErrorFormat($"Error while deleting Password Policy dictionary.");
                    return response;
                }

                // Delete PasswordPolicy from CB
                if (!UsersDal.DeletePasswordPolicy(contextData.GroupId, id))
                {
                    log.Error($"Error while delete PasswordPolicy CB. groupId: {contextData.GroupId}, Id:{id}");
                    response.Set((int)eResponseStatus.Error, "FailedToDeletePasswordPolicy");
                    return response;
                }

                SetInvalidationKeys(contextData.GroupId, modifiedRoleIdsResponse.Object);
                response.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"DeletePasswordPolicy failed ex={ex}, groupId={contextData.GroupId}, AssetRuleId={id}");
                response.Set(eResponseStatus.Error);
            }

            return response;
        }

        public GenericResponse<PasswordPolicy> Get(ContextData contextData, long id)
        {
            var response = new GenericResponse<PasswordPolicy>
            {
                Object = UsersDal.GetPasswordPolicy(id)
            };

            if (response.Object == null)
            {
                response.SetStatus(eResponseStatus.PasswordPolicyDoesNotExist);
                return response;
            }

            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        public GenericListResponse<PasswordPolicy> List(ContextData contextData, PasswordPolicyFilter filter)
        {
            var response = new GenericListResponse<PasswordPolicy>();

            try
            {
                if (filter.RoleIdsIn == null || filter.RoleIdsIn.Count == 0)
                {
                    var roles = RolesPermissionsManager.GetRolesByGroupId(contextData.GroupId);
                    if (roles != null && roles.Count > 0)
                    {
                        filter.RoleIdsIn = new List<long>(roles.Select(x => x.Id));
                    }
                }

                Dictionary<string, List<PasswordPolicy>> passwordPolicies = null;
                var keyToOriginalValueMap = GetPasswordPolicyKeyMap(filter.RoleIdsIn);
                var invalidationKeysMap = GetPasswordPolicyInvalidationKeysMap(filter.RoleIdsIn);

                if (!LayeredCache.Instance.GetValues(keyToOriginalValueMap,
                                                     ref passwordPolicies,
                                                     GetPasswordPolicyList,
                                                     new Dictionary<string, object>()
                                                     {
                                                        { "contextData", contextData },
                                                        { "filter", filter }
                                                     },
                                                     contextData.GroupId,
                                                     LayeredCacheConfigNames.GET_EPG_ASSETS_CACHE_CONFIG_NAME,
                                                     invalidationKeysMap))
                {
                    log.ErrorFormat("PasswordPolicyManager.List - GetPasswordPolicyList - Failed get data from cache. groupId: {0}", contextData.GroupId);
                    return response;
                }

                if (passwordPolicies != null && passwordPolicies.Count > 0)
                {
                    response.Objects.AddRange(passwordPolicies.Values.SelectMany(x => x));
                    response.Objects = response.Objects.Distinct().ToList();
                }

                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                response.SetStatus(eResponseStatus.Error);
                log.ErrorFormat("An Exception was occurred in PasswordPolicyManager.List. contextData:{0}, ex:{1}.", contextData.ToString(), ex);
            }

            return response;
        }

        public Status ValidatePassword(string password, int groupId, long userId, List<long> userRoleIds)
        {
            var response = new Status(eResponseStatus.OK);

            try
            {
                var contextData = new ContextData(groupId);
                var filter = new PasswordPolicyFilter() { RoleIdsIn = userRoleIds };
                var passwordPolicyResponse = List(contextData, filter);

                if (passwordPolicyResponse.HasObjects())
                {
                    int? historyCount = null;
                    HashSet<string> passwordsHistory = UsersDal.GetPasswordsHistory(userId);
                    var invalidPasswordComplexities = new HashSet<string>();
                    var isPasswordReused = false;

                    foreach (var passwordPolicy in passwordPolicyResponse.Objects)
                    {
                        if (!isPasswordReused && passwordPolicy.HistoryCount.HasValue && passwordPolicy.HistoryCount.Value > 0)
                        {
                            historyCount = passwordPolicy.HistoryCount;
                            if (passwordsHistory != null && passwordsHistory.Contains(password))
                            {
                                isPasswordReused = true;
                                response.AddArg(eResponseStatus.PasswordCannotBeReused, 
                                    $"The password shall be different from the last {passwordPolicy.HistoryCount} " +
                                    $"passwords used by the user");
                            }
                        }

                        if (passwordPolicy.Complexities != null)
                        {
                            for (int i = 0; i < passwordPolicy.Complexities.Count; i++)
                            {
                                var expression = passwordPolicy.Complexities[i].Expression;
                                if (!invalidPasswordComplexities.Contains(expression) && !Regex.IsMatch(password, expression))
                                {
                                    invalidPasswordComplexities.Add(expression);

                                    var description = passwordPolicy.Complexities[i].Description;

                                    AddPasswordComplexityError(response, description);
                                }
                            }
                        }
                    }

                    if (response.Args != null && response.Args.Count > 0)
                    {
                        var args = response.Args;
                        response.Set(eResponseStatus.PasswordPolicyViolation, eResponseStatus.PasswordPolicyViolation.ToString(), args);
                    }
                    else
                    {
                        UpdatePasswordHistory(userId, password, historyCount, passwordsHistory);
                    }
                }
            }
            catch (Exception ex)
            {
                response.Set(eResponseStatus.Error);
                log.Error($"An Exception was occurred in ValidateNewPassword. groupId:{groupId}, userRoleIds:{string.Join(",", userRoleIds)}. ex:{ex}");
            }

            return response;
        }

        public GenericResponse<PasswordPolicy> ValidateCrudObject(ContextData contextData, long id = 0, PasswordPolicy objectToValidate = null)
        {
            var response = new GenericResponse<PasswordPolicy>();
            try
            {
                if (id != 0)
                {
                    response.Object = UsersDal.GetPasswordPolicy(id);
                    if (response.Object == null)
                    {
                        response.SetStatus(eResponseStatus.PasswordPolicyDoesNotExist);
                        return response;
                    }
                }

                if (objectToValidate != null && objectToValidate.UserRoleIds != null)
                {
                    var allExistingRoles = RolesPermissionsManager.GetRolesByGroupId(contextData.GroupId);
                    if (!(objectToValidate.UserRoleIds.IsSubsetOf(allExistingRoles.Select(r => r.Id))))
                    {
                        response.SetStatus(eResponseStatus.RoleDoesNotExists);
                        return response;
                    }
                }

                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                response.SetStatus(eResponseStatus.Error);
                log.Error($"Validate Error: {ex}");
            }
            return response;
        }

        private void SetInvalidationKeys(int groupId, HashSet<long> roleIds)
        {
            var mapKey = LayeredCacheKeys.GetUserRolesToPasswordPolicyInvalidationKey(groupId);
            if (!LayeredCache.Instance.SetInvalidationKey(mapKey))
            {
                log.Error($"Error setting InvalidationKey: {mapKey} (UserRolesToPasswordPolicyInvalidationKey)");
            }

            foreach (var roleId in roleIds)
            {
                var key = LayeredCacheKeys.GetPasswordPolicyInvalidationKey(roleId);
                if (!LayeredCache.Instance.SetInvalidationKey(key))
                {
                    log.Error($"Error setting: {key}");
                }
            }
        }

        private Tuple<Dictionary<string, List<PasswordPolicy>>, bool> GetPasswordPolicyList(Dictionary<string, object> funcParams)
        {
            bool res = false;
            var result = new Dictionary<string, List<PasswordPolicy>>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("contextData") && funcParams.ContainsKey("filter"))
                {
                    Dictionary<long, HashSet<long>> userRolesToPasswordPolicy = null;
                    var filter = funcParams["filter"] as PasswordPolicyFilter;
                    if (funcParams["contextData"] is ContextData contextData && filter != null)
                    {
                        if (!LayeredCache.Instance.Get(LayeredCacheKeys.GetUserRolesToPasswordPolicyKey(contextData.GroupId),
                                                       ref userRolesToPasswordPolicy,
                                                       GetUserRolesToPasswordPolicy,
                                                       new Dictionary<string, object>()
                                                       {
                                                            { "groupId", contextData.GroupId }
                                                       },
                                                       contextData.GroupId,
                                                       LayeredCacheConfigNames.GET_USER_ROLES_TO_PASSWORD_POLICY,
                                                       new List<string>() { LayeredCacheKeys.GetUserRolesToPasswordPolicyInvalidationKey(contextData.GroupId) }))
                        {
                            log.ErrorFormat("GetPasswordPolicyList - GetUserRolesToPasswordPolicy - Failed get data from cache. groupId: {0}", contextData.GroupId);
                            return new Tuple<Dictionary<string, List<PasswordPolicy>>, bool>(result, false);
                        }
                    }

                    res = true;
                    if (userRolesToPasswordPolicy != null && userRolesToPasswordPolicy.Count > 0)
                    {
                        List<long> roleIds = null;
                        if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                        {
                            roleIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => long.Parse(x)).ToList();
                        }
                        else
                        {
                            roleIds = filter.RoleIdsIn;
                        }

                        var relevantPasswordPolicies = new List<long>();
                        if (roleIds != null && roleIds.Count > 0)
                        {
                            foreach (var roleId in roleIds)
                            {
                                if (userRolesToPasswordPolicy.ContainsKey(roleId))
                                {
                                    relevantPasswordPolicies.AddRange(userRolesToPasswordPolicy[roleId]);
                                }
                            }
                        }
                        else
                        {
                            relevantPasswordPolicies.AddRange(userRolesToPasswordPolicy.Values.SelectMany(x => x));
                        }

                        var existingPolicies = new HashSet<long>();
                        if (relevantPasswordPolicies.Count > 0)
                        {
                            relevantPasswordPolicies = relevantPasswordPolicies.Distinct().ToList();
                            foreach (var passwordPolicyId in relevantPasswordPolicies)
                            {
                                if (!existingPolicies.Contains(passwordPolicyId))
                                {
                                    var passwordPolicy = UsersDal.GetPasswordPolicy(passwordPolicyId);
                                    if (passwordPolicy != null)
                                    {
                                        existingPolicies.Add(passwordPolicyId);
                                        foreach (var roleId in passwordPolicy.UserRoleIds)
                                        {
                                            var passwordPolicykey = LayeredCacheKeys.GetPasswordPolicyKey(roleId);
                                            if (result.ContainsKey(passwordPolicykey))
                                            {
                                                result[passwordPolicykey].Add(passwordPolicy);
                                            }
                                            else
                                            {
                                                result.Add(passwordPolicykey, new List<PasswordPolicy>() { passwordPolicy });
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        res = existingPolicies.Count == relevantPasswordPolicies.Count;
                    }
                }
            }
            catch (Exception ex)
            {
                res = false;
                log.Error(string.Format("GetPasswordPolicyList failed, funcParams: {0}.", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, List<PasswordPolicy>>, bool>(result, res);
        }

        private Tuple<Dictionary<long, HashSet<long>>, bool> GetUserRolesToPasswordPolicy(Dictionary<string, object> funcParams)
        {
            Dictionary<long, HashSet<long>> userRolesToPasswordPolicy = null;
            bool res = false;

            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                    {
                        userRolesToPasswordPolicy = UsersDal.GetUserRolesToPasswordPolicy(groupId.Value);
                        res = true;
                    }
                }
            }
            catch (Exception ex)
            {
                res = false;
                log.Error(string.Format("GetUserRolesToPasswordPolicy failed, funcParams: {0}.", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<long, HashSet<long>>, bool>(userRolesToPasswordPolicy, res);
        }

        private void UpdatePasswordHistory(long userId, string password, int? policyHistoryCount, HashSet<string> passwordsHistory)
        {
            if (policyHistoryCount.HasValue)
            {
                if (passwordsHistory == null)
                {
                    passwordsHistory = new HashSet<string>();
                }

                while (passwordsHistory.Count > 0 && passwordsHistory.Count >= policyHistoryCount.Value)
                {
                    var oldPassword = passwordsHistory.FirstOrDefault();
                    passwordsHistory.Remove(oldPassword);
                }

                passwordsHistory.Add(password);
                UsersDal.SavePasswordsHistory(userId, passwordsHistory);
            }
        }

        private HashSet<long> UpdatePolicyMap(long passwordPolicyId, HashSet<long> roleIdsToUpdate, HashSet<long> originalRoleIds, Dictionary<long, HashSet<long>> roleIdsToPasswordPolicyIdsMap)
        {
            var modifiedRole = new HashSet<long>();
            //if map changed roles
            if (roleIdsToUpdate == null || originalRoleIds == null || !originalRoleIds.SequenceEqual(roleIdsToUpdate))
            {
                if (originalRoleIds != null)
                {
                    //all roles with that policy
                    var rolesWithPolicy = roleIdsToPasswordPolicyIdsMap.Where
                        (x => x.Value.Contains(passwordPolicyId)).Select(x => x.Key).ToList();

                    foreach (var role in rolesWithPolicy)
                    {
                        roleIdsToPasswordPolicyIdsMap[role].Remove(passwordPolicyId);
                        modifiedRole.Add(role);
                    }
                }

                if (roleIdsToUpdate != null)
                {
                    foreach (var role in roleIdsToUpdate)//update for all relevant
                    {
                        if (roleIdsToPasswordPolicyIdsMap.ContainsKey(role))
                        {
                            roleIdsToPasswordPolicyIdsMap[role].Add(passwordPolicyId);
                        }
                        else
                        {
                            roleIdsToPasswordPolicyIdsMap.Add(role, new HashSet<long> { passwordPolicyId });
                        }

                        if (!modifiedRole.Contains(role))
                        {
                            modifiedRole.Add(role);
                        }
                    }
                }
            }

            return modifiedRole;
        }

        private GenericResponse<HashSet<long>> GetModifiedRoleIds(ContextData contextData, long passwordPolicyId,
            HashSet<long> originalRoleIds, out Dictionary<long, HashSet<long>> rolesToPasswordPolicyMap, HashSet<long> roleIdsToUpdate = null)
        {
            var response = new GenericResponse<HashSet<long>>();
            rolesToPasswordPolicyMap = null;
            if (!LayeredCache.Instance.Get(LayeredCacheKeys.GetUserRolesToPasswordPolicyKey(contextData.GroupId),
                                                       ref rolesToPasswordPolicyMap,
                                                       GetUserRolesToPasswordPolicy,
                                                       new Dictionary<string, object>()
                                                       {
                                                            { "groupId", contextData.GroupId }
                                                       },
                                                       contextData.GroupId,
                                                       LayeredCacheConfigNames.GET_USER_ROLES_TO_PASSWORD_POLICY,
                                                       new List<string>() { LayeredCacheKeys.GetUserRolesToPasswordPolicyInvalidationKey(contextData.GroupId) }))
            {
                log.ErrorFormat("GetModifiedRoleIds - GetUserRolesToPasswordPolicy - Failed get data from cache. groupId: {0}", contextData.GroupId);
                return response;
            }

            if (rolesToPasswordPolicyMap == null)
            {
                rolesToPasswordPolicyMap = new Dictionary<long, HashSet<long>>();
            }

            //logic - validate group and role in policy
            response.Object = UpdatePolicyMap(passwordPolicyId, roleIdsToUpdate, originalRoleIds, rolesToPasswordPolicyMap);
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        private static Dictionary<string, string> GetPasswordPolicyKeyMap(List<long> roleIds)
        {
            var result = new Dictionary<string, string>();
            if (roleIds != null && roleIds.Count > 0)
            {
                foreach (var id in roleIds)
                {
                    var key = LayeredCacheKeys.GetPasswordPolicyKey(id);
                    if (!result.ContainsKey(key))
                    {
                        result.Add(key, id.ToString());
                    }
                }
            }

            return result;
        }

        private static Dictionary<string, List<string>> GetPasswordPolicyInvalidationKeysMap(List<long> roleIds)
        {
            var result = new Dictionary<string, List<string>>();
            if (roleIds != null && roleIds.Count > 0)
            {
                foreach (long id in roleIds)
                {
                    var key = LayeredCacheKeys.GetPasswordPolicyKey(id);
                    if (!result.ContainsKey(key))
                    {
                        result.Add(key, new List<string>() { LayeredCacheKeys.GetPasswordPolicyInvalidationKey(id) });
                    }
                }
            }

            return result;
        }

        private static void AddPasswordComplexityError(Status response, string complexityDescription)
        {
            if (response.Args == null)
            {
                response.Args = new List<ApiObjects.KeyValuePair>();
            }

            // Add args as Args.Add(..) (not as AddArg(..)) to allow multiple InvalidPasswordComplexity errors.
            response.Args.Add(new ApiObjects.KeyValuePair(
                eResponseStatus.InvalidPasswordComplexity.ToString(),
                $"Your password needs to be more complex. It requires: {complexityDescription}. Please enter a new password in accordance with these requirements"));
        }
    }
}