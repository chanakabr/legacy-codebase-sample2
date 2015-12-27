using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
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
        public KalturaSessionInfo Get()
        {
            var ks = KS.GetFromRequest();
            return new KalturaSessionInfo()
            {
                ks = ks.ToString(),
                expiry = (int)SerializationUtils.ConvertToUnixTimestamp(ks.Expiration),
                partnerId = ks.GroupId,
                privileges = ks.Privilege,
                sessionType = ks.SessionType,
                userId = ks.UserId,
                udid = KSUtils.ExtractKSPayload(KS.GetFromRequest()).UDID
            };
        }
    }
}