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
            //bool result = false;

            //this.Status = 1;
            //this.Version = 1;
            //this.CreateDate = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            //CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

            //long newId = (long)couchbaseManager.Increment(GetSegmentationTypeSequenceDocument(), 1);

            //if (newId == 0)
            //{
            //    log.ErrorFormat("Error setting segmentation type id");
            //    return false;
            //}

            //this.Id = newId;

            //string segmentationTypesKey = GetGroupSegmentationTypeDocumentKey(this.GroupId);

            //if (this.Value == null)
            //{
            //    this.Value = new SegmentDummyValue();
            //}

            //if (this.Value != null)
            //{
            //    bool addSegmentIdsResult = this.Value.AddSegmentsIds(this.Id);

            //    if (!addSegmentIdsResult)
            //    {
            //        log.ErrorFormat("error setting segment Ids");
            //        return false;
            //    }
            //}
            //GroupSegmentationTypes groupSegmentationTypes = couchbaseManager.Get<GroupSegmentationTypes>(segmentationTypesKey);

            //if (groupSegmentationTypes == null)
            //{
            //    groupSegmentationTypes = new GroupSegmentationTypes();
            //}

            //if (groupSegmentationTypes.segmentationTypes == null)
            //{
            //    groupSegmentationTypes.segmentationTypes = new List<long>();
            //}

            //groupSegmentationTypes.segmentationTypes.Add(newId);

            //bool setResult = couchbaseManager.Set<GroupSegmentationTypes>(segmentationTypesKey, groupSegmentationTypes);

            //if (!setResult)
            //{
            //    log.ErrorFormat("Error updating list of segment types in group.");
            //    return false;
            //}

            //string newDocumentKey = GetSegmentationTypeDocumentKey(this.GroupId, this.Id);

            //setResult = couchbaseManager.Set<SegmentationType>(newDocumentKey, this);

            //if (!setResult)
            //{
            //    log.ErrorFormat("Error adding new segment type to Couchbase.");
            //}

            //result = setResult;

            //return result;
            throw new NotImplementedException();
        }

        public GenericResponse<PasswordPolicy> Update(ContextData contextData, PasswordPolicy objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public Status Delete(ContextData contextData, long id)
        {
            throw new NotImplementedException();
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

        public Status ValidateNewPassword(string password, ContextData contextData, List<long> userRoleIds)
        {
            var response = new Status(eResponseStatus.OK);

            try
            {
                var filter = new PasswordPolicyFilter() { RoleIdsIn = userRoleIds };
                var passwordSettingsResponse = List(contextData, filter);
                if (passwordSettingsResponse.HasObjects())
                {
                    foreach (var passwordSettings in passwordSettingsResponse.Objects)
                    {
                        // TODO SHIR - ValidateNewPassword
                    }
                }

                if (response.Args != null && response.Args.Count > 0)
                {
                    response.Set(eResponseStatus.InvalidPassword);
                }
            }
            catch (Exception ex)
            {
                response.Set(eResponseStatus.Error);
                log.ErrorFormat("An Exception was occurred in ValidatePassword. contextData:{0}, password:{1}, userRoleIds:{2}. ex: {3}",
                                contextData.ToString(), password, string.Join(",", userRoleIds), ex);
            }

            return response;
        }

        public Status ValidateExistingPassword(string password, ContextData contextData, List<long> userRoleIds, DateTime passwordUpdateDate)
        {
            var response = new Status(eResponseStatus.OK);

            try
            {
                var filter = new PasswordPolicyFilter() { RoleIdsIn = userRoleIds };
                var passwordSettingsResponse = List(contextData, filter);
                if (passwordSettingsResponse.HasObjects())
                {
                    foreach (var passwordSettings in passwordSettingsResponse.Objects)
                    {
                        // TODO SHIR -ValidateExistingPassword
                        if (passwordSettings.PasswordAge.HasValue && passwordUpdateDate.AddDays(passwordSettings.PasswordAge.Value) > DateTime.UtcNow)
                        {
                            response.AddArg(eResponseStatus.PasswordExpired.ToString(), "some error");
                        }
                    }
                }

                if (response.Args != null && response.Args.Count > 0)
                {
                    response.Set(eResponseStatus.InvalidPassword);
                }
            }
            catch (Exception ex)
            {
                response.Set(eResponseStatus.Error);
                log.ErrorFormat("An Exception was occurred in ValidatePassword. contextData:{0}, password:{1}, userRoleIds:{2}. ex: {3}",
                                contextData.ToString(), password, string.Join(",", userRoleIds), ex);
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
                                        foreach (var roleId in passwordPolicy.RoleIds)
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
    }
}
