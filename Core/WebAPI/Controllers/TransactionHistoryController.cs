using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("transactionHistory")]
    public class TransactionHistoryController : IKalturaController
    {
        /// <summary>
        /// Gets user or household transaction history.        
        /// </summary>        
        /// <param name="filter">Filter by household or user</param>   
        /// <param name="pager">Page size and index</param>   
        /// <remarks></remarks>
        [Action("list")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        static public KalturaBillingTransactionListResponse List(KalturaTransactionHistoryFilter filter = null, KalturaFilterPager pager = null)
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

                switch (filter.EntityReferenceEqual)
                {
                    case KalturaEntityReferenceBy.user:
                        {
                            response = ClientsManager.ConditionalAccessClient().GetUserTransactionHistory(groupId, userID, pager.getPageIndex(), pager.getPageSize(), filter, startDate, endDate);
                            break;
                        }
                    case KalturaEntityReferenceBy.household:
                        {
                            bool isDeprecated = !DeprecatedAttribute.IsDeprecated("4.8.0.0", (Version)HttpContext.Current.Items[RequestContextUtils.REQUEST_VERSION]); // fix for userFullName and userId disapearing from response since 4.8.0.0

                            response = ClientsManager.ConditionalAccessClient().GetDomainBillingHistory(
                                groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), startDate, endDate, pager.getPageIndex(), pager.getPageSize(), filter, isDeprecated);
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
        [Action("listOldStandard")]
        [OldStandardAction("list")]
        [ApiAuthorize]
        [Obsolete]
        static public KalturaBillingTransactionListResponse ListOldStandard(KalturaTransactionsFilter filter = null)
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

                switch (filter.By)
                {
                    case KalturaEntityReferenceBy.user:
                        {
                            response = ClientsManager.ConditionalAccessClient().GetUserTransactionHistory(groupId, userID, filter.getPageIndex(), filter.getPageSize(),
                                                                                        KalturaTransactionHistoryOrderBy.CREATE_DATE_DESC, startDate, endDate);
                            break;
                        }
                    case KalturaEntityReferenceBy.household:
                        {


                            response = ClientsManager.ConditionalAccessClient().GetDomainBillingHistory(
                                groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), startDate, endDate, filter.getPageIndex(), filter.getPageSize(), KalturaTransactionHistoryOrderBy.CREATE_DATE_DESC, true);
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