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
        public KalturaSession Get(string session = null)
        {
            KS ks;

            if (session != null)
            {
                StringBuilder sb = new StringBuilder(session);
                sb = sb.Replace("-", "+");
                sb = sb.Replace("_", "/");

                int groupId = 0;
                byte[] encryptedData = null;
                string encryptedDataStr = null;
                string[] ksParts = null;

                try
                {
                    encryptedData = System.Convert.FromBase64String(sb.ToString());
                    encryptedDataStr = System.Text.Encoding.ASCII.GetString(encryptedData);
                    ksParts = encryptedDataStr.Split('|');
                }
                catch (Exception)
                {
                    throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.InvalidKS, "Invalid KS format");
                }

                if (ksParts.Length < 3 || ksParts[0] != "v2" || !int.TryParse(ksParts[1], out groupId))
                {
                    throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.InvalidKS, "Invalid KS format");
                }

                Group group = null;
                try
                {
                    // get group secret
                    group = GroupsManager.GetGroup(groupId);
                }
                catch (ApiException ex)
                {
                    throw new InternalServerErrorException((int)ex.Code, ex.Message);
                }

                string adminSecret = group.UserSecret;

                // build KS
                ks = KS.CreateKSFromEncoded(encryptedData, groupId, adminSecret, session, KS.KSVersion.V2);
            }
            else
            {
                ks = KS.GetFromRequest();
            }

            return new KalturaSession()
            {
                ks = ks.ToString(),
                expiry = (int)SerializationUtils.ConvertToUnixTimestamp(ks.Expiration),
                partnerId = ks.GroupId,
                privileges = ks.Privilege,
                sessionType = ks.SessionType,
                userId = ks.UserId,
                udid = KSUtils.ExtractKSPayload(ks).UDID
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
                StringBuilder sb = new StringBuilder(session);
                sb = sb.Replace("-", "+");
                sb = sb.Replace("_", "/");

                int groupId = 0;
                byte[] encryptedData = null;
                string encryptedDataStr = null;
                string[] ksParts = null;

                try
                {
                    encryptedData = System.Convert.FromBase64String(sb.ToString());
                    encryptedDataStr = System.Text.Encoding.ASCII.GetString(encryptedData);
                    ksParts = encryptedDataStr.Split('|');
                }
                catch (Exception)
                {
                    throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.InvalidKS, "Invalid KS format");
                }

                if (ksParts.Length < 3 || ksParts[0] != "v2" || !int.TryParse(ksParts[1], out groupId))
                {
                    throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.InvalidKS, "Invalid KS format");
                }

                Group group = null;
                try
                {
                    // get group secret
                    group = GroupsManager.GetGroup(groupId);
                }
                catch (ApiException ex)
                {
                    throw new InternalServerErrorException((int)ex.Code, ex.Message);
                }

                string adminSecret = group.UserSecret;

                // build KS
                ks = KS.CreateKSFromEncoded(encryptedData, groupId, adminSecret, session, KS.KSVersion.V2);
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
                privileges = ks.Privilege,
                sessionType = ks.SessionType,
                userId = ks.UserId,
                udid = KSUtils.ExtractKSPayload(ks).UDID
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
            int groupId = KS.GetFromRequest().GroupId;

            Group group = GroupsManager.GetGroup(groupId);
            if (!group.IsSwitchingUsersAllowed)
            {
                throw new ForbiddenException((int)WebAPI.Managers.Models.StatusCode.SwitchingUsersIsNotAllowedForPartner, "Switching users is not allowed for partner");
            }

            if (string.IsNullOrEmpty(userIdToSwitch))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "userIdToSwitch cannot be empty");
            }

            if (!AuthorizationManager.IsUserInHousehold(userIdToSwitch, groupId))
            {
                throw new ForbiddenException((int)WebAPI.Managers.Models.StatusCode.ServiceForbidden, "userIdToSwitch is not in household");
            }

            string udid = KSUtils.ExtractKSPayload().UDID;

            return AuthorizationManager.GenerateSession(userIdToSwitch, groupId, false, false, udid);
        }
    }
}