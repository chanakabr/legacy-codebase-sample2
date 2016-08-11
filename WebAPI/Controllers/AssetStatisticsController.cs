using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/assetStatistics/action")]
    public class AssetStatisticsController : ApiController
    {
        /// <summary>
        /// Returns statistics for given list of assets by type and / or time period
        /// </summary>
        /// <param name="query">Query for assets statistics</param>
        /// <remarks></remarks>
        [Route("query"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public KalturaAssetStatisticsListResponse Query(KalturaAssetStatisticsQuery query)
        {
            List<KalturaAssetStatistics> response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;
            
            query.Validate();

            try
            {
                response = ClientsManager.CatalogClient().GetAssetsStats(groupId, userID, query.getAssetIdIn(), query.AssetTypeEqual, query.StartDateGreaterThanOrEqual, query.EndDateGreaterThanOrEqual);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaAssetStatisticsListResponse() { AssetsStatistics = response, TotalCount = response != null ? response.Count : 0 };
        }
    }
}