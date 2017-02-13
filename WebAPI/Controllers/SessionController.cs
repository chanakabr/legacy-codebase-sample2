using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/session/action")]
    [OldStandardAction("getOldStandard", "get")]
    public class SessionController : ApiController
    {
        /// <summary>
        /// Parses KS
        /// </summary>
        /// <param name="session">Additional KS to parse, if not passed the user's KS will be parsed</param>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [SchemeArgument("session", RequiresPermission = true)]
        
        public KalturaSession Get(string session = null)
        {
            KS ks;

            if (session != null)
            {
                ks = KS.ParseKS(session);
            }
            else
            {
                ks = KS.GetFromRequest();
            }

            var payload = KSUtils.ExtractKSPayload(ks);

            return new KalturaSession()
            {
                ks = ks.ToString(),
                expiry = (int)SerializationUtils.ConvertToUnixTimestamp(ks.Expiration),
                partnerId = ks.GroupId,
                privileges = ks.Privileges != null && ks.Privileges.Count > 0 ? string.Join(",", ks.Privileges.Select(p => string.Join(":", p.key, p.value))) : string.Empty,
                sessionType = ks.SessionType,
                userId = ks.UserId,
                udid = payload.UDID,
                createDate = payload.CreateDate,
            };
        }

        /// <summary>
        /// Parses KS
        /// </summary>
        /// <param name="session">Additional KS to parse, if not passed the user's KS will be parsed</param>
        [Route("getOldStandard"), HttpPost]
        [ApiAuthorize]
        [OldStandard("session", "ks_to_parse")]
        [Obsolete]
        public KalturaSessionInfo GetOldStandard(string session = null)
        {
            KS ks;

            if (session != null)
            {
                ks = KS.ParseKS(session);
            }
            else
            {
                ks = KS.GetFromRequest();
            }

            return new KalturaSessionInfo()
            {
                ks = ks.ToString(),
                expiry = (int)SerializationUtils.ConvertToUnixTimestamp(ks.Expiration),
                partnerId = ks.GroupId,
                privileges = ks.Privileges != null && ks.Privileges.Count > 0 ? string.Join(",", ks.Privileges.Select(p => string.Join(":", p.key, p.value))) : string.Empty,
                sessionType = ks.SessionType,
                userId = ks.UserId,
                udid = KSUtils.ExtractKSPayload(ks).UDID,
                createDate = payload.CreateDate,
            };
        }

        /// <summary>
        /// Switching the user in the session by generating a new session for a new user within the same household
        /// </summary>
        /// <param name="userIdToSwitch">The identifier of the user to change</param>
        [Route("switchUser"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public KalturaLoginSession SwitchUser(string userIdToSwitch)
        {
            KS ks = KS.GetFromRequest();
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

            string udid = KSUtils.ExtractKSPayload().UDID;

            AuthorizationManager.LogOut(ks);
            return AuthorizationManager.GenerateSession(userIdToSwitch, groupId, false, false, udid);
        }

        /// <summary>
        /// Revokes all the sessions (KS) of a given user 
        /// </summary>
        /// <param name="userId">The identifier of the user to change</param>
        [Route("revoke"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public bool Revoke()
        {
            KS ks = KS.GetFromRequest();
            int groupId = ks.GroupId;
            string userId = ks.UserId;

            return AuthorizationManager.RevokeSessions(groupId, userId);
        }

    }
}