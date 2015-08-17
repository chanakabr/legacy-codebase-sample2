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

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/assetView/action")]
    public class AssetViewController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Get recently watched media for user, ordered by recently watched first.    
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <param name="pager"><![CDATA[Page size and index. Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted - will be set to 25. If a value > 50 provided – will set to 50]]></param>
        /// <param name="language">Language code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaWatchHistoryAssetWrapper List(KalturaAssetViewFilter filter = null, KalturaFilterPager pager = null, string language = null)
        {
            KalturaWatchHistoryAssetWrapper response = null;
            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

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
                filter = new KalturaAssetViewFilter();
            }

            // validate and convert filter status
            eWatchStatus filterStatusHelper = eWatchStatus.All;
            if (filter.filter_status != null)
                Enum.TryParse<eWatchStatus>(filter.filter_status.ToString(), out filterStatusHelper);

            // days - default value 7
            if (filter.days == 0)
                filter.days = 7;
            try
            {
                // call client
                response = ClientsManager.CatalogClient().WatchHistory(groupId, userId.ToString(),
                    language, pager.PageIndex, pager.PageSize, filterStatusHelper, filter.days, filter.filter_types != null ?
                    filter.filter_types.Select(x => x.value).ToList() : null, filter.with);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}
