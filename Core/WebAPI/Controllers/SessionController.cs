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
                Group group = GroupsManager.GetGroup(groupId);
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

    }
}