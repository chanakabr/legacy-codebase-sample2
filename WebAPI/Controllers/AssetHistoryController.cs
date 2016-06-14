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
using WebAPI.Catalog;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Managers.Schema;
using System.Web;
using WebAPI.Filters;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/assetHistory/action")]
    public class AssetHistoryController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Get recently watched media for user, ordered by recently watched first.    
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <param name="pager"><![CDATA[Page size and index. Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted - will be set to 25. If a value > 50 provided – will set to 50]]></param>
        /// <param name="language">Language code</param>
        /// <remarks>Possible status codes: 
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaWatchHistoryAssetWrapper List(KalturaAssetHistoryFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaWatchHistoryAssetWrapper response = null;
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
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "page_size range can be between 5 and 50");
            }

            if (filter == null)
            {
                filter = new KalturaAssetHistoryFilter();
            }

            // validate and set filter status value if not provided
            if (!filter.filter_status.HasValue)
            {
                filter.filter_status = KalturaWatchStatus.all;
            }

            // days - default value 7
            if (filter.days == null || (filter.days.HasValue && filter.days.Value == 0))
                filter.days = 7;

            string language = Utils.Utils.GetLanguageFromRequest();

            try
            {
                // call client
                response = ClientsManager.CatalogClient().WatchHistory(groupId, userId.ToString(), udid,
                    language, pager.getPageIndex(), pager.PageSize, filter.filter_status.Value, filter.getDays(), filter.filter_types != null ?
                    filter.filter_types.Select(x => x.value).ToList() : null, filter.with.Select(x=> x.type).ToList());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Clean the user’s viewing history
        /// </summary>
        /// <param name="filter">List of assets identifier</param>
        /// <returns></returns>
        [Route("clean"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_NAME)]
        public bool Clean(KalturaAssetsFilter filter = null)
        {
            var ks = KS.GetFromRequest();
            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);
                // call client
                return ClientsManager.ApiClient().CleanUserHistory(groupId, userId, filter.Assets);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }
    }
}
