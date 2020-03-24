using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("parentalRuleProfile")]
    [Obsolete]
    public class ParentalRuleProfileController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Return all of the parental rules defined for the account 
        /// </summary>
        /// <returns>The parental rules defined for the account</returns>
        /// <remarks></remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaParentalRuleListResponse List()
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