using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/parentalRuleProfile/action")]
    public class ParentalRuleProfileController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Return all of the parental rules defined for the account 
        /// </summary>
        /// <returns>The parental rules defined for the account</returns>
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaParentalRuleListResponse List()
        {
            List<KalturaParentalRule> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetGroupParentalRules(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaParentalRuleListResponse() { ParentalRule = response, TotalCount = response.Count };
        }
    }
}