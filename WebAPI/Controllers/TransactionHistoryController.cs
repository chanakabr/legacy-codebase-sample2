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
    [RoutePrefix("_service/transactionHistory/action")]
    public class TransactionHistoryController : ApiController
    {
        /// <summary>
        /// Gets user or household transaction history.        
        /// </summary>        
        /// <param name="filter">Page size and index, filter by household or user</param>        
        /// <param name="household_id">If getting transactions of household - household id</param>        
        /// <param name="start_date">Filter transactions later than specific date</param>        
        /// <param name="end_date">Filter transactions earlier than specific date</param>        
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaBillingTransactionListResponse List(KalturaTransactionsFilter filter = null, int household_id = 0, 
            DateTime? start_date = null, DateTime? end_date = null)
        {
            KalturaBillingTransactionListResponse response = new KalturaBillingTransactionListResponse();

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                filter = new KalturaTransactionsFilter();
            }

            try
            {
                switch (filter.By)
                {
                    case KalturaReferenceType.user:
                    {
                        response = ClientsManager.ConditionalAccessClient().GetUserTransactionHistory(groupId, KS.GetFromRequest().UserId, filter.PageIndex, filter.PageSize);
                        break;
                    }
                    case KalturaReferenceType.household:
                    {
                        DateTime startDate = new DateTime(1753, 1, 1);
                        DateTime endDate = DateTime.MaxValue;

                        if (start_date.HasValue)
                        {
                            startDate = start_date.Value;
                        }

                        if (end_date.HasValue)
                        {
                            endDate = end_date.Value;
                        }

                        response = ClientsManager.ConditionalAccessClient().GetDomainBillingHistory(
                            groupId, household_id, startDate, endDate, filter.PageIndex, filter.PageSize);
                        break;
                    }
                    default:
                    break;
                }
                // call client
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}