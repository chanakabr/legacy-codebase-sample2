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
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("parental_rule_profile")]
    public class ParentalRuleProfileController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Return all of the parental rules defined for the account 
        /// </summary>
        /// <param name="partner_id">Partner identifier</param>
        /// <returns>The parental rules defined for the account</returns>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("{partner_id}/parental/rules"), HttpGet]
        public KalturaParentalRulesList List([FromUri] string partner_id)
        {
            List<KalturaParentalRule> response = null;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetGroupParentalRules(int.Parse(partner_id));
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaParentalRulesList() { ParentalRules = response };
        }
    }
}