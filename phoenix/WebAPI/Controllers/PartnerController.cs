using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models;
using WebAPI.Models.Partner;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("partner")]
    public class PartnerController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Returns a login session for external system (like OVP)
        /// </summary>
        /// <returns></returns>
        [Action("externalLogin")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize]
        static public KalturaLoginSession ExternalLogin()
        {
            KalturaLoginSession response = null;
            int groupId = KS.GetFromRequest().GroupId;
            
            try
            {
                response = AuthorizationManager.GenerateOvpSession(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }

}