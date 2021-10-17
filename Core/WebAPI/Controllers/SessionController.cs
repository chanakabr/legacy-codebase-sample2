using ApiObjects.Response;
using System;
using System.Collections.Generic;
using ApiLogic.Users.Managers;
using ApiObjects.User.SessionProfile;
using AutoMapper;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Users;
using WebAPI.Utils;
using WebAPI.ClientManagers.Client;
using TVinciShared;
using WebAPI.Models.General;
using ApiLogic.Users;
using Core.Users;

namespace WebAPI.Controllers
{
    [Service("session")]
    public class SessionController : IKalturaController
    {
        /// <summary>
        /// Parses KS
        /// </summary>
        /// <param name="session">Additional KS to parse, if not passed the user's KS will be parsed</param>
        [Action("get")]
        [ApiAuthorize]
        [SchemeArgument("session", RequiresPermission = true)]
        static public KalturaSession Get(string session = null)
        {
            KS ks, ksFromRequest = KS.GetFromRequest();

            if (session != null)
            {
                ks = KS.ParseKS(session);

                if (ks.GroupId != ksFromRequest.GroupId)
                {
                    throw new ForbiddenException(ForbiddenException.GROUP_MISS_MATCH);
                }
            }
            else
            {
                ks = ksFromRequest;
            }

            var payload = KSUtils.ExtractKSPayload(ks);
            
            return new KalturaSession()
            {
                ks = ks.ToString(),
                expiry = (int)DateUtils.DateTimeToUtcUnixTimestampSeconds(ks.Expiration),
                partnerId = ks.GroupId,
                privileges = KS.JoinPrivileges(ks.Privileges, ",", ":"),
                sessionType = ks.SessionType,
                userId = ks.UserId,
                udid = payload.UDID,
                createDate = payload.CreateDate
            };
        }

        /// <summary>
        /// Parses KS
        /// </summary>
        /// <param name="session">Additional KS to parse, if not passed the user's KS will be parsed</param>
        [Action("getOldStandard")]
        [OldStandardAction("get")]
        [ApiAuthorize]
        [OldStandardArgument("session", "ks_to_parse")]
        [Obsolete]
        static public KalturaSessionInfo GetOldStandard(string session = null)
        {
            KS ks, ksFromRequest = KS.GetFromRequest();

            if (session != null)
            {
                ks = KS.ParseKS(session);

                if (ks.GroupId != ksFromRequest.GroupId)
                {
                    throw new ForbiddenException(ForbiddenException.GROUP_MISS_MATCH);
                }
            }
            else
            {
                ks = ksFromRequest;
            }

            var payload = KSUtils.ExtractKSPayload(ks);
            return new KalturaSessionInfo()
            {
                ks = ks.ToString(),
                expiry = (int)DateUtils.DateTimeToUtcUnixTimestampSeconds(ks.Expiration),
                partnerId = ks.GroupId,
                privileges = KS.JoinPrivileges(ks.Privileges, ",", ":"),
                sessionType = ks.SessionType,
                userId = ks.UserId,
                udid = payload.UDID,
                createDate = payload.CreateDate
            };
        }

        /// <summary>
        /// Switching the user in the session by generating a new session for a new user within the same household
        /// </summary>
        /// <param name="userIdToSwitch">The identifier of the user to change</param>
        [Action("switchUser")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserNotActivated)]
        static public KalturaLoginSession SwitchUser(string userIdToSwitch)
        {
            KalturaLoginSession loginSession = null;
            KS ks = KS.GetFromRequest();
            int groupId = ks.GroupId;

            try
            {
                // switch notification users
                var payload = KSUtils.ExtractKSPayload();
                ClientsManager.UsersClient().SwitchUsers(groupId, ks.UserId, userIdToSwitch, payload.UDID);
                Group group = GroupsManager.Instance.GetGroup(groupId);
                loginSession = AuthorizationManager.SwitchUser(userIdToSwitch, groupId, payload, ks.Privileges, group);
                AuthorizationManager.LogOut(ks);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return loginSession;
        }

        /// <summary>
        /// Revokes all the sessions (KS) of a given user 
        /// </summary>
        /// <param name="userId">The identifier of the user to change</param>
        [Action("revoke")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public bool Revoke()
        {
            KS ks = KS.GetFromRequest();
            int groupId = ks.GroupId;
            string userId = ks.UserId;

            return AuthorizationManager.RevokeSessions(groupId, userId);
        }

        /// <summary>
        /// Create session characteristic
        /// </summary>
        /// <param name="userId">user identifier</param>
        /// <param name="householdId">household identifier</param>
        /// <param name="udid">device UDID</param>
        /// <param name="regionId">region identifier</param>
        /// <param name="sessionCharacteristicParams">session characteristic dynamic params</param>
        /// <param name="expiration">relative expiration(TTL) in seconds, should be equal or greater than KS expiration</param>
        /// <returns>session characteristic entity</returns>
        [Action("createSessionCharacteristic")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public KalturaSessionCharacteristic CreateSessionCharacteristic(
            string userId,
            long householdId,
            string udid,
            long expiration,
            int? regionId = null,
            SerializableDictionary<string, KalturaStringValueArray> sessionCharacteristicParams = null)
        {
            // could be validation here, but it should be used by Auth MS only...
            
            var groupId = KS.GetFromRequest().GroupId;
            var region = regionId ?? Core.Catalog.CatalogLogic.GetRegionIdOfDomain(groupId, (int)householdId, userId);
            var userSegments = Core.Api.Module.GetUserAndHouseholdSegmentIds(groupId, userId, householdId);
            var userRoles = ClientsManager.UsersClient().GetUserRoleIds(groupId, userId);
            var sessionCharacteristics = Mapper.Map<Dictionary<string, List<string>>>(sessionCharacteristicParams) 
                                         ?? new Dictionary<string, List<string>>(0);

            var userSessionProfileIds = GetUserSessionProfileIds(groupId, udid, userSegments, sessionCharacteristics);

            var sessionCharacteristicKey = SessionCharacteristicManager.Instance.GetOrAdd(groupId,
                new SessionCharacteristic(region, userSegments, userRoles, userSessionProfileIds), (uint)expiration);
            return new KalturaSessionCharacteristic
            {
                Id = sessionCharacteristicKey,
                RegionId = region,
                UserSegmentsIds = string.Join(",", userSegments),
                UserRolesIds = string.Join(",", userRoles),
                UserSessionProfilesIds = string.Join(",", userSessionProfileIds)
            };
        }

        private static List<long> GetUserSessionProfileIds(int groupId, string udid, List<long> userSegments, Dictionary<string, List<string>> sessionCharacteristics)
        {
            var device = new Device(groupId); // TODO could be from cache
            if (!device.Initialize(udid) || device.m_state != DeviceState.Activated) return new List<long>(0); // TODO is it correct?

            var userScope = new UserSessionConditionScope
            {
                BrandId = device.m_deviceBrandID,
                FamilyId = device.m_deviceFamilyID,
                SegmentIds = userSegments,
                FilterBySegments = true,
                ManufacturerId = device.ManufacturerId,
                Model = device.Model,
                SessionCharacteristics = sessionCharacteristics,
                DeviceDynamicData = device.DynamicData,
                RuleId = 0 // not used
            };

            return UserSessionProfileManager.Instance.GetMatchedUserSessionProfiles(groupId, userScope);
        }
    }
}