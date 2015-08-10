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
        /// <param name="filter_types">List of asset types to search within. The list is a string separated be comma.
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.</param>
        /// <param name="filter_status">Which type of recently watched media to include in the result – those that finished watching, those that are in progress or both.
        /// If omitted or specified filter = all – return all types.
        /// Allowed values: progress – return medias that are in-progress, done – return medias that finished watching.</param>
        /// <param name="days">How many days back to return the watched media. If omitted, default to 7 days</param>        
        /// <param name="pager"><![CDATA[Page size and index. Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted - will be set to 25. If a value > 50 provided – will set to 50]]></param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="language">Language code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaWatchHistoryAssetWrapper List(string filter_types = null, KalturaWatchStatus? filter_status = null,
            int days = 0, KalturaFilterPager pager = null, List<KalturaCatalogWith> with = null, string language = null)
        {
            KalturaWatchHistoryAssetWrapper response = null;
            int groupId = KS.GetFromRequest().GroupId;
            int userId = KS.GetFromRequest().UserId;

            if (pager == null)
                pager = new KalturaFilterPager();

            // page size - 5 <= size <= 50
            if (pager.PageSize == null || pager.PageSize == 0)
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

            // validate and convert filter status
            eWatchStatus filterStatusHelper = eWatchStatus.All;
            if (filter_status != null)
                Enum.TryParse<eWatchStatus>(filter_status.ToString(), out filterStatusHelper);

            List<int> filterTypes = null;
            if (!string.IsNullOrEmpty(filter_types))
            {
                try
                {
                    filterTypes = filter_types.Split(',').Select(x => int.Parse(x)).ToList();
                }
                catch
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "invalid filter types");
                }
            }

            // days - default value 7
            if (days == 0)
                days = 7;
            try
            {
                // call client
                response = ClientsManager.CatalogClient().WatchHistory(groupId, userId.ToString(),
                    language, pager.PageIndex, pager.PageSize, filterStatusHelper, days, filterTypes, with);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}
