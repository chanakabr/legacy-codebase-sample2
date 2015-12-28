using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/session/action")]
    public class SessionController : ApiController
    {
        /// <summary>
        /// Parses KS
        /// </summary>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaSessionInfo Get(string ks_to_parse = null)
        {
            KS ks;
            
            if (ks_to_parse != null)
            {
                StringBuilder sb = new StringBuilder(ks_to_parse);
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
                catch (Exception ex)
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
                ks = KS.CreateKSFromEncoded(encryptedData, groupId, adminSecret, ks_to_parse, KS.KSVersion.V2);
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
    }
}