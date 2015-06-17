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
    [RoutePrefix("partners")]
    public class PartnersController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Return all of the parental rules defined for the account 
        /// </summary>
        /// <param name="partner_id">Partner identifier</param>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <returns>The parental rules defined for the account</returns>
        [Route("{partner_id}/parental/rules"), HttpGet]
        [PartnerFilter]
        public List<ParentalRule> GetParentalRules([FromUri] string partner_id)
        {
            List<ParentalRule> response = null;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetGroupParentalRules(int.Parse(partner_id));
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

    }
}