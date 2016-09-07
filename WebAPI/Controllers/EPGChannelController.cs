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
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/epgChannel/action")]
    [Obsolete]
    public class EpgChannelController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Returns EPG channel programs filtered by channel identifiers and dates
        /// </summary>
        /// <param name="filter">Filtering the epg channel request</param>        
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaEPGChannelAssetsListResponse List(KalturaEpgChannelFilter filter, List<KalturaCatalogWithHolder> with = null)
        {
            List<KalturaEPGChannelAssets> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();
            
            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                string language = Utils.Utils.GetLanguageFromRequest();

                response = ClientsManager.CatalogClient().GetEPGByChannelIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language, 0, 0, 
                    new List<int>(filter.IDs.Select(x => x.value).ToList()), 
                    SerializationUtils.ConvertFromUnixTimestamp(filter.getStartTime()),
                    SerializationUtils.ConvertFromUnixTimestamp(filter.getEndTime()), with.Select(x => x.type).ToList());

                // if no response - return not found status 
                if (response == null || response.Count == 0)
                    throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "EPG-Channel");
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaEPGChannelAssetsListResponse() { Channels = response, TotalCount = response.Count };
        }
    }
}