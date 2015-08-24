using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/householdLastPosition/action")]
    public class HouseholdLastPositionController : ApiController
    {
       /// <summary>
        /// Returns the last position (in seconds) in a media or nPVR asset until which a user in the household watched
       /// </summary>
       /// <param name="user_id">User identifier</param>
       /// <param name="household_id">Household identifier</param>
       /// <param name="udid">Device UDID</param>
       /// <param name="media_id">media identifier</param>
       /// <param name="npvr_id">nPVR identifier</param>
       /// <returns></returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public List<KalturaUserLastPosition> Get(string udid = null, int media_id = 0, string npvr_id = null)
        {
            List<KalturaUserLastPosition> response = null;

            if (media_id == 0 && string.IsNullOrEmpty(npvr_id))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "at least one of media_id and npvr_id must not be empty");
            }

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // call client
                response = ClientsManager.CatalogClient().GetDomainLastPosition(groupId, userID, (int)HouseholdUtils.getHouseholdIDByKS(groupId), udid, media_id, npvr_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}