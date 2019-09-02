using ApiLogic.Base;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ApiLogic.Users.Managers
{
    public class PasswordPolicyManager : ICrudHandler<PasswordPolicy, long, PasswordPolicyFilter>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<PasswordPolicyManager> lazy = new Lazy<PasswordPolicyManager>(() => new PasswordPolicyManager());
        public static PasswordPolicyManager Instance { get { return lazy.Value; } }

        private PasswordPolicyManager() { }

        public GenericResponse<PasswordPolicy> Add(ContextData contextData, PasswordPolicy objectToAdd)
        {
            var response = new GenericResponse<PasswordPolicy>();

            var userRoleToPasswordPolicy = UsersDal.GetUserRolesToPasswordPolicy(contextData.GroupId);

            if (userRoleToPasswordPolicy == null)
            {
                userRoleToPasswordPolicy = new Dictionary<long, List<long>>();
            }

            var couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);
            objectToAdd.Id = (long)couchbaseManager.Increment("password_policy_sequence", 1);
            if (objectToAdd.Id == 0)
            {
                log.ErrorFormat("Error setting Password Policy id");
                return null;
            }

            foreach (var roleId in objectToAdd.UserRoleIds)
            {
                if (userRoleToPasswordPolicy.ContainsKey(roleId))
                {
                    userRoleToPasswordPolicy[roleId].Add(objectToAdd.Id);
                }
                else
                {
                    userRoleToPasswordPolicy.Add(roleId, new List<long> { objectToAdd.Id });
                }
            }

            if (!UsersDal.SaveUserRolesToPasswordPolicy(contextData.GroupId, userRoleToPasswordPolicy))
            {
                log.ErrorFormat($"Error while saving Password Policy dictionary.");
                return response;
            }

            if (!UsersDal.SavePasswordPolicy(objectToAdd))
            {
                log.ErrorFormat($"Error while saving Password Policy. " +
                    $"Policy: {Newtonsoft.Json.JsonConvert.SerializeObject(objectToAdd)}," +
                    $"Status: {response.Status}");
                return response;
            }

            SetInvalidationKeys(contextData.GroupId);

            response.Object = objectToAdd;
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        private void SetInvalidationKeys(int groupId, List<long> roleIds = null)
        {
            if (!LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetUserRolesToPasswordPolicyInvalidationKey(groupId)))
            {
                log.Error($"Error setting InvalidationKey (UserRolesToPasswordPolicyInvalidationKey) for group id: {groupId}");
            }

            if (roleIds != null && roleIds.Count > 0)
            {
                foreach (var roleId in roleIds)
                {
                    if (!LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetPasswordPolicyInvalidationKey(roleId)))
                    {
                        log.Error($"Error setting InvalidationKey for role id: {roleId}");
                    }
                }
            }
        }

        public GenericResponse<PasswordPolicy> Update(ContextData contextData, PasswordPolicy objectToUpdate)
        {
            var response = new GenericResponse<PasswordPolicy>();
            try
            {
                var orgPolicy = UsersDal.GetPasswordPolicy(objectToUpdate.Id);
                if (orgPolicy == null)
                {
                    response.SetStatus(eResponseStatus.PasswordPolicyDoesNotExist);
                    return response;
                }

                var urtp = UsersDal.GetUserRolesToPasswordPolicy(contextData.GroupId);
                if (urtp == null)
                {
                    log.ErrorFormat($"Error getting UserRolesToPasswordPolicy by the supplied group id: {contextData.GroupId}");
                    response.SetStatus(eResponseStatus.PasswordPolicyDoesNotExist);
                    return response;
                }

                //logic - validate group and role in policy
                CompareAndFillPolicy(orgPolicy, objectToUpdate);

                //update relevant 
                var isSuccess = UsersDal.SavePasswordPolicy(objectToUpdate);
                response.SetStatus(isSuccess ? eResponseStatus.OK : eResponseStatus.Error);
                SetInvalidationKeys(contextData.GroupId, objectToUpdate.UserRoleIds);

                response.Object = objectToUpdate;
                //response.SetStatus(eResponseStatus.OK);

                return response;
            }
            catch (Exception ex)
            {
                log.Error($"Error updating password policy due to: {ex.Message}");
                response.SetStatus(eResponseStatus.Error);
            }
            return response;
        }

        /// <summary>
        /// fill empty policies
        /// </summary>
        /// <param name="orgPolicy"></param>
        /// <param name="objectToUpdate"></param>
        private void CompareAndFillPolicy(PasswordPolicy orgObject, PasswordPolicy objectToUpdate)
        {
            //skip id
            objectToUpdate.Name = objectToUpdate.Name ?? orgObject.Name;
            objectToUpdate.Complexities = objectToUpdate.Complexities ?? orgObject.Complexities;
            objectToUpdate.Expiration = objectToUpdate.Expiration ?? orgObject.Expiration;
            objectToUpdate.LockoutFailuresCount = objectToUpdate.LockoutFailuresCount ?? orgObject.LockoutFailuresCount;
            objectToUpdate.HistoryCount = objectToUpdate.HistoryCount ?? orgObject.HistoryCount;
            objectToUpdate.UserRoleIds = objectToUpdate.UserRoleIds ?? orgObject.UserRoleIds;
        }

        public Status Delete(ContextData contextData, long id)
        {
            var response = Status.Error;

            try
            {
                //check AssetRule exists
                var passwordPolicy = UsersDal.GetPasswordPolicy(id);
                if (passwordPolicy == null || passwordPolicy.Id == 0)
                {
                    response.Set((int)eResponseStatus.PasswordPolicyDoesNotExist, "PasswordPolicyDoesNotExist");
                    return response;
                }

                // Delete PasswordPolicy from CB
                if (!UsersDal.DeletePasswordPolicy(contextData.GroupId, id))
                {
                    log.Error($"Error while delete PasswordPolicy CB. groupId: {contextData.GroupId}, Id:{id}");
                    response.Set((int)eResponseStatus.Error, "FailedToDeletePasswordPolicy");
                    return response;
                }

                var idsList = new List<long> { id };
                SetInvalidationKeys(contextData.GroupId, idsList);
                response = Status.Ok;
            }
            catch (Exception ex)
            {
                log.Error($"DeletePasswordPolicy failed ex={ex}, groupId={contextData.GroupId}, AssetRuleId={id}");
            }

            return response;
        }

        public GenericResponse<PasswordPolicy> Get(ContextData contextData, long id)
        {
            throw new NotImplementedException();
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
                var keyToOriginalValueMap = LayeredCacheKeys.GetPasswordPolicyKeyMap(filter.RoleIdsIn);
                var invalidationKeysMap = LayeredCacheKeys.GetPasswordPolicyInvalidationKeysMap(filter.RoleIdsIn);

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

                response.Objects.AddRange(passwordPolicies.Values.SelectMany(x => x));
                response.Objects = response.Objects.Distinct().ToList();
                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                response.SetStatus(eResponseStatus.Error);
                log.ErrorFormat("An Exception was occurred in PasswordPolicyManager.List. contextData:{0}, ex:{1}.", contextData.ToString(), ex);
            }

            return response;
        }

        public Status ValidateNewPassword(string password, int groupId, long userId, List<long> userRoleIds)
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
                    HashSet<string> passwordsHistory = null;
                    var errors = new HashSet<eResponseStatus>();

                    foreach (var passwordPolicy in passwordPolicyResponse.Objects)
                    {
                        if (!errors.Contains(eResponseStatus.ReusedPassword) && passwordPolicy.HistoryCount.HasValue && passwordPolicy.HistoryCount.Value > 0)
                        {
                            historyCount = passwordPolicy.HistoryCount;
                            passwordsHistory = UsersDal.GetPasswordsHistory(userId);
                            if (passwordsHistory != null && passwordsHistory.Contains(password))
                            {
                                response.AddArg(eResponseStatus.ReusedPassword, "password cannot be Reused");
                                errors.Add(eResponseStatus.ReusedPassword);
                            }
                        }
                        // TODO SHIR - ValidateNewPassword
                    }

                    if (response.Args != null && response.Args.Count > 0)
                    {
                        response.Set(eResponseStatus.InvalidPassword);
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
                log.Error($"An Exception was occurred in ValidateNewPassword. groupId:{groupId}, password:{password}, userRoleIds:{string.Join(",", userRoleIds)}. ex:{ex}");
            }

            return response;
        }

        private Tuple<Dictionary<string, List<PasswordPolicy>>, bool> GetPasswordPolicyList(Dictionary<string, object> funcParams)
        {
            bool res = false;
            var result = new Dictionary<string, List<PasswordPolicy>>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("contextData") && funcParams.ContainsKey("filter"))
                {
                    Dictionary<long, List<long>> userRolesToPasswordPolicy = null;
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
                        }
                    }

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
                log.Error(string.Format("GetPasswordPolicyList failed, funcParams: {0}.", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, List<PasswordPolicy>>, bool>(result, res);
        }

        private Tuple<Dictionary<long, List<long>>, bool> GetUserRolesToPasswordPolicy(Dictionary<string, object> funcParams)
        {
            Dictionary<long, List<long>> userRolesToPasswordPolicy = null;

            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                    {
                        userRolesToPasswordPolicy = UsersDal.GetUserRolesToPasswordPolicy(groupId.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetUserRolesToPasswordPolicy failed, funcParams: {0}.", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<long, List<long>>, bool>(userRolesToPasswordPolicy, userRolesToPasswordPolicy != null);
        }

        private void UpdatePasswordHistory(long userId, string password, int? historyCount, HashSet<string> passwordsHistory)
        {
            if (historyCount.HasValue)
            {
                if (passwordsHistory == null)
                {
                    passwordsHistory = new HashSet<string>();
                }

                if (passwordsHistory.Count == historyCount.Value)
                {
                    var oldPassword = passwordsHistory.FirstOrDefault();
                    passwordsHistory.Remove(oldPassword);
                }

                passwordsHistory.Add(password);
                UsersDal.SavePasswordsHistory(userId, passwordsHistory);
            }
        }
    }
}