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
        /// <param name="start_date">Filter transactions later than specific date</param>        
        /// <param name="end_date">Filter transactions earlier than specific date</param>        
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaBillingTransactionListResponse List(KalturaTransactionsFilter filter = null, DateTime? start_date = null, DateTime? end_date = null)
        {
            KalturaBillingTransactionListResponse response = new KalturaBillingTransactionListResponse();

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                filter = new KalturaTransactionsFilter();
            }

            try
            {
                string userID = KS.GetFromRequest().UserId;

                switch (filter.By)
                {
                    case KalturaEntityReferenceBy.user:
                    {
                        response = ClientsManager.ConditionalAccessClient().GetUserTransactionHistory(groupId, userID, filter.PageIndex, filter.PageSize);
                        break;
                    }
                    case KalturaEntityReferenceBy.household:
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
                            groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), startDate, endDate, filter.PageIndex, filter.PageSize);
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