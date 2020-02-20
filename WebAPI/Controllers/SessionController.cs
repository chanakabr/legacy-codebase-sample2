using ApiObjects.Response;
using System;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Users;
using WebAPI.Utils;
using WebAPI.ClientManagers.Client;
using TVinciShared;
using KSWrapper;
using WebAPI.Models.General;

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
            KS ks, ksFromRequest = KSManager.GetKSFromRequest();

            if (session != null)
            {
                ks = KSManager.ParseKS(session);

                if (ks.GroupId != ksFromRequest.GroupId)
                {
                    throw new ForbiddenException(ForbiddenException.GROUP_MISS_MATCH);
                }
            }
            else
            {
                ks = ksFromRequest;
            }

            var payload = ks.ExtractKSData();
            
            return new KalturaSession()
            {
                ks = ks.ToString(),
                expiry = (int)DateUtils.DateTimeToUtcUnixTimestampSeconds(ks.Expiration),
                partnerId = ks.GroupId,
                privileges = ks.JoinPrivileges(",", ":"),
                sessionType = (KalturaSessionType)ks.SessionType,
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
            KS ks;

            if (session != null)
            {
                ks = KSManager.ParseKS(session);
            }
            else
            {
                ks = KSManager.GetKSFromRequest();
            }

            var payload = ks.ExtractKSData();
            return new KalturaSessionInfo()
            {
                ks = ks.ToString(),
                expiry = (int)DateUtils.DateTimeToUtcUnixTimestampSeconds(ks.Expiration),
                partnerId = ks.GroupId,
                privileges = ks.JoinPrivileges(",", ":"),
                sessionType = (KalturaSessionType)ks.SessionType,
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
            var ks = KSManager.GetKSFromRequest();
            int groupId = ks.GroupId;

            Group group = GroupsManager.GetGroup(groupId);
            if (!group.IsSwitchingUsersAllowed)
            {
                throw new ForbiddenException(ForbiddenException.SWITCH_USER_NOT_ALLOWED_FOR_PARTNER);
            }

            if (string.IsNullOrEmpty(userIdToSwitch))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "userIdToSwitch");
            }

            if (!AuthorizationManager.IsUserInHousehold(userIdToSwitch, groupId))
            {
                throw new NotFoundException(NotFoundException.OBJECT_ID_NOT_FOUND, "OTT-User", userIdToSwitch);
            }

            try
            {
                // switch notification users
                var payload = ks.ExtractKSData();
                ClientsManager.UsersClient().SwitchUsers(groupId, ks.UserId, userIdToSwitch, payload.UDID);
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
            var ks = KSManager.GetKSFromRequest();
            int groupId = ks.GroupId;
            string userId = ks.UserId;

            return AuthorizationManager.RevokeSessions(groupId, userId);
        }

    }
}