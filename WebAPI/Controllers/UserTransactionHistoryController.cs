using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/userTransactionHistory/action")]
    public class UserTransactionHistoryController : ApiController
    {
        /// <summary>
        /// Gets user transaction history.        
        /// </summary>        
        /// <param name="pager">Page size and index</param>        
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaBillingTransactionListResponse List(KalturaFilterPager pager = null)
        {
            KalturaBillingTransactionListResponse response = new KalturaBillingTransactionListResponse();

            int groupId = KS.GetFromRequest().GroupId;

            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().GetUserTransactionHistory(groupId, KS.GetFromRequest().UserId, pager.PageIndex, pager.PageSize);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}