using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.Catalog;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/epgChannel/action")]
    public class EPGChannelController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Returns media by ID
        /// </summary>
        /// <param name="filter">Filtering the epg channel request</param>        
        /// <param name="order_by">Ordering the channel</param>
        /// <param name="pager">Paging the request</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="language">Language code</param>        
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaEPGChannelAssets List(KalturaAssetInfoFilter filter, List<KalturaCatalogWithHolder> with = null, KalturaOrder? order_by = null,
             string language = null)
        {
            KalturaEPGChannelAssets response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            try
            {
                string userID = KS.GetFromRequest().UserId;

                response = ClientsManager.CatalogClient().GetEPGByChannelIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), string.Empty, language,
              0, 1, new List<int>(filter.IDs.Select(x => x.value).ToList()), filter.StartTime, filter.EndTime, with.Select(x => x.type).ToList());

                //// if no response - return not found status 
                //if (response == null || response.Objects == null || response.Objects.Count == 0)
                //    throw new NotFoundException();


            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}