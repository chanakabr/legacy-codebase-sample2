using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/Statistics/action")]
    public class StatisticsController : ApiController
    {
        /// <summary>
        /// Returns list of asset statistics
        /// </summary>
        /// <param name="assetsIds">array of asset identifier's</param>
        /// <param name="assetType">type of the assets</param>
        /// <param name="startTime">start time in epoch</param>
        /// <param name="endTime">end time in epoch</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003</remarks>     
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetStatisticsListResponse List(int[] assetsIds, KalturaAssetType assetType, long startTime, long endTime)
        {            
            KalturaAssetStatisticsListResponse response = null;
            List<KalturaAssetStatistics> assetsStatistics = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                assetsStatistics = ClientsManager.CatalogClient().GetAssetsStats(groupId, userId, assetsIds.ToList(), CatalogMappings.ConvertToStatsType(assetType), startTime, endTime);
                if (assetsStatistics != null)
                {
                    response.Objects = assetsStatistics;
                    response.TotalCount = assetsStatistics.Count;
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}