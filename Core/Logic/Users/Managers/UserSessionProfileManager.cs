using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects.User.SessionProfile;
using CachingProvider.LayeredCache;
using Core.Api.Managers;
using DAL.Users;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ApiLogic.Users.Managers
{
    public interface IUserSessionProfileManager
    {
        List<long> GetMatchedUserSessionProfiles(int groupId, UserSessionConditionScope userScope);
        IReadOnlyCollection<UserSessionProfile> List(int groupId, long? userSessionProfileId = null);
        GenericResponse<UserSessionProfile> Add(ContextData contextData, UserSessionProfile userSessionProfile);
        GenericResponse<UserSessionProfile> Update(ContextData contextData, UserSessionProfile userSessionProfile);
        Status Delete(ContextData contextData, long userSessionProfileId);
    }

    public class UserSessionProfileManager : IUserSessionProfileManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<UserSessionProfileManager> LazyInstance = new Lazy<UserSessionProfileManager>(() =>
            new UserSessionProfileManager(new UserSessionProfileRepository(),
                                          LayeredCache.Instance,
                                          UserSessionProfileExpressionValidator.Instance,
                                          AssetRuleManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IUserSessionProfileManager Instance => LazyInstance.Value;

        private const int MAX_USER_SESSION_PROFILES = 50;

        private readonly IUserSessionProfileRepository _repository;
        private readonly ILayeredCache _layeredCache;
        private readonly IUserSessionProfileExpressionValidator _expressionValidator;
        private readonly IAssetRuleManager _assetRuleManager;

        public UserSessionProfileManager(IUserSessionProfileRepository repository,
                                         ILayeredCache layeredCache,
                                         IUserSessionProfileExpressionValidator expressionValidator,
                                         IAssetRuleManager assetRuleManager)
        {
            _repository = repository;
            _layeredCache = layeredCache;
            _expressionValidator = expressionValidator;
            _assetRuleManager = assetRuleManager;
        }

        public IReadOnlyCollection<UserSessionProfile> List(int groupId, long? userSessionProfileId = null)
        {
            var allUserSessionProfiles = _repository.GetUserSessionProfiles(groupId);
            if (userSessionProfileId.HasValue)
            {
                var curr = allUserSessionProfiles.FirstOrDefault(x => x.Id == userSessionProfileId.Value);
                allUserSessionProfiles.Clear();
                if (curr != null)
                {
                    allUserSessionProfiles.Add(curr);
                }
            }
            return allUserSessionProfiles;
        }

        private IReadOnlyCollection<UserSessionProfile> ListFromCache(int groupId)
        {
            var response = new List<UserSessionProfile>();
            var key = LayeredCacheKeys.GetUserSessionProfiles(groupId);
            var cacheResult = _layeredCache.Get(
                key,
                ref response,
                arg => Tuple.Create(_repository.GetUserSessionProfiles(groupId), true),
                null,
                groupId,
                LayeredCacheConfigNames.GET_USER_SESSION_PROFILES,
                new List<string> { LayeredCacheKeys.GetUserSessionProfilesInvalidationKey(groupId) },
                true);

            if (!cacheResult)
            {
                log.Warn($"could not get {key} from LayeredCache");
                response.Clear();
                return response;
            }

            return response;
        }

        public Status Delete(ContextData contextData, long userSessionProfileId)
        {
            var allAssetRules = _assetRuleManager.GetAssetRules(RuleConditionType.UserSessionProfile, contextData.GroupId);
            if (allAssetRules.HasObjects())
            {
                return new Status(eResponseStatus.CannotDeleteUserSessionProfile, 
                    $"UserSessionProfile {userSessionProfileId} cannot be deleted if becuase AssetRule {string.Join(", ", allAssetRules.Objects.Select(x => x.Id))} refers to it.");
            }

            if (!_repository.DeleteUserSessionProfile(contextData.GroupId, contextData.UserId.Value, userSessionProfileId))
            {
                log.Warn($"faild Delete UserSessionProfile to DB, id:{userSessionProfileId}, contextData:{contextData}");
                return new Status(eResponseStatus.UserSessionProfileDoesNotExist, $"User session profile {userSessionProfileId} does not exist");
            }

            _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetUserSessionProfilesInvalidationKey(contextData.GroupId));

            return Status.Ok;
        }

        public GenericResponse<UserSessionProfile> Add(ContextData contextData, UserSessionProfile userSessionProfile)
        {
            var response = new GenericResponse<UserSessionProfile>();

            var allUserSessionProfile = _repository.GetUserSessionProfiles(contextData.GroupId);
            if (allUserSessionProfile.Count >= MAX_USER_SESSION_PROFILES)
            {
                response.SetStatus(eResponseStatus.ExceededMaxCapacity, "partner UserSessionProfiles Exceeded Max Size");
                return response;
            }

            Status validationStatus = _expressionValidator.Validate(contextData.GroupId, userSessionProfile.Expression);
            if (!validationStatus.IsOkStatusCode())
            {
                response.SetStatus(validationStatus);
                return response;
            }

            var id = _repository.InsertUserSessionProfile(contextData.GroupId, contextData.UserId.Value, userSessionProfile);
            _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetUserSessionProfilesInvalidationKey(contextData.GroupId));

            userSessionProfile.Id = id;
            response.Object = GetUserSessionProfile(contextData.GroupId, userSessionProfile.Id);
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        public GenericResponse<UserSessionProfile> Update(ContextData contextData, UserSessionProfile userSessionProfile)
        {
            var response = new GenericResponse<UserSessionProfile>();
            
            var needToUpdate = false;
            if (userSessionProfile.Expression != null)
            {
                needToUpdate = true;
                Status validationStatus = _expressionValidator.Validate(contextData.GroupId, userSessionProfile.Expression);
                if (!validationStatus.IsOkStatusCode())
                {
                    response.SetStatus(validationStatus);
                    return response;
                }
            }

            if (userSessionProfile.Name != null)
            {
                needToUpdate = true;
            }

            if (needToUpdate)
            {
                if (!_repository.UpdateUserSessionProfile(contextData.GroupId, contextData.UserId.Value, userSessionProfile))
                {
                    log.Warn($"failed Update UserSessionProfile to DB, id:{userSessionProfile.Id}, contextData:{contextData}");
                    response.SetStatus(eResponseStatus.UserSessionProfileDoesNotExist, $"User session profile {userSessionProfile.Id} does not exist");
                    return response;
                }

                _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetUserSessionProfilesInvalidationKey(contextData.GroupId));
            }

            response.Object = GetUserSessionProfile(contextData.GroupId, userSessionProfile.Id);
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        private UserSessionProfile GetUserSessionProfile(int groupId, long id)
        {
            return _repository.GetUserSessionProfiles(groupId).First(x => x.Id == id);
        }

        public List<long> GetMatchedUserSessionProfiles(int groupId, UserSessionConditionScope userScope)
        {
            return ListFromCache(groupId)
                .Where(_ => _.Expression.Evaluate(userScope))
                .Select(_ => _.Id)
                .ToList();
        }
    }
}