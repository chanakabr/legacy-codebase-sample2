using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("user_transaction_history")]
    public class UserTransactionHistoryController : ApiController
    {
        /// <summary>
        /// Gets user transaction history.        
        /// </summary>        
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User Id</param>
        /// <param name="page_number">page number</param>
        /// <param name="page_size">page size</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("{user_id}/transactions"), HttpGet]
        public KalturaBillingTransactions GetUserTransactionHistory([FromUri] string partner_id, [FromUri] string user_id, [FromUri] int page_number, [FromUri] int page_size)
        {
            KalturaBillingTransactions response = new KalturaBillingTransactions();

            // validate group ID
            int groupId = 0;
            if (!int.TryParse(partner_id, out groupId) || groupId < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal partner ID");

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().GetUserTransactionHistory(groupId, user_id, page_number, page_size);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}