using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.Utils;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Managers.Scheme;
using System.Web;
using WebAPI.Filters;
using WebAPI.Models.API;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/searchHistory/action")]
    public class SearchHistoryController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Get user's last search requests
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <param name="pager">Page size and index. Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted - will be set to 25. If a value > 50 provided – will set to 50></param>
        /// <remarks>Possible status codes: 
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaSearchHistoryListResponse List(KalturaSearchHistoryFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaSearchHistoryListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            if (pager == null)
                pager = new KalturaFilterPager();

            // page size - 5 <= size <= 50
            if (pager.PageSize == 0)
            {
                pager.PageSize = 25;
            }
            else if (pager.PageSize > 50)
            {
                pager.PageSize = 50;
            }
            else if (pager.PageSize < 5)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "KalturaAssetHistoryFilter.pageSize", "5");
            }

            string language = Utils.Utils.GetLanguageFromRequest();

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetSearchHistory(groupId, userId.ToString(), udid,
                    language, pager.getPageIndex(), pager.PageSize);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}