using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/transactionHistory/action")]
    [OldStandardAction("listOldStandard", "list")]
    public class TransactionHistoryController : ApiController
    {
        /// <summary>
        /// Gets user or household transaction history.        
        /// </summary>        
        /// <param name="filter">Filter by household or user</param>   
        /// <param name="pager">Page size and index</param>   
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        public KalturaBillingTransactionListResponse List(KalturaTransactionHistoryFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaBillingTransactionListResponse response = new KalturaBillingTransactionListResponse();

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                filter = new KalturaTransactionHistoryFilter();
            }

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            try
            {
                string userID = KS.GetFromRequest().UserId;

                switch (filter.EntityReferenceEqual)
                {
                    case KalturaEntityReferenceBy.user:
                    {
                        response = ClientsManager.ConditionalAccessClient().GetUserTransactionHistory(groupId, userID, pager.getPageIndex(), pager.getPageSize(), filter.OrderBy);
                        break;
                    }
                    case KalturaEntityReferenceBy.household:
                    {
                        DateTime startDate = new DateTime(1753, 1, 1);
                        DateTime endDate = DateTime.MaxValue;

                        if (filter.StartDateGreaterThanOrEqual.HasValue)
                        {
                            startDate = filter.StartDateGreaterThanOrEqual.Value;
                        }

                        if (filter.EndDateLessThanOrEqual.HasValue)
                        {
                            endDate = filter.EndDateLessThanOrEqual.Value;
                        }

                        response = ClientsManager.ConditionalAccessClient().GetDomainBillingHistory(
                            groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), startDate, endDate, pager.getPageIndex(), pager.getPageSize(), filter.OrderBy);
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

        /// <summary>
        /// Gets user or household transaction history.        
        /// </summary>        
        /// <param name="filter">Page size and index, filter by household or user</param>   
        /// <remarks></remarks>
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaBillingTransactionListResponse ListOldStandard(KalturaTransactionsFilter filter = null)
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
                            response = ClientsManager.ConditionalAccessClient().GetUserTransactionHistory(groupId, userID, filter.getPageIndex(), filter.getPageSize(), KalturaTransactionHistoryOrderBy.CREATE_DATE_DESC);
                            break;
                        }
                    case KalturaEntityReferenceBy.household:
                        {
                            DateTime startDate = new DateTime(1753, 1, 1);
                            DateTime endDate = DateTime.MaxValue;

                            if (filter.StartDate.HasValue)
                            {
                                startDate = filter.StartDate.Value;
                            }

                            if (filter.EndDate.HasValue)
                            {
                                endDate = filter.EndDate.Value;
                            }

                            response = ClientsManager.ConditionalAccessClient().GetDomainBillingHistory(
                                groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), startDate, endDate, filter.getPageIndex(), filter.getPageSize(), KalturaTransactionHistoryOrderBy.CREATE_DATE_DESC);
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