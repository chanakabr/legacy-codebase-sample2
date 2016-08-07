using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/region/action")]
    public class RegionController : ApiController
    {
        /// <summary>
        /// Returns all regions for the partner
        /// </summary>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaRegionListResponse List(KalturaRegionFilter filter)
        {
            KalturaRegionListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            long householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                response = ClientsManager.ApiClient().GetRegions(groupId, filter.GetExternalIdIn(), filter.GetDefaultOrderByValue());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}