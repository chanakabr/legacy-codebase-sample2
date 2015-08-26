using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/lastPosition/action")]
    public class LastPositionController : ApiController
    {
        /// <summary>
        /// Returns the last position (in seconds) in a media or nPVR asset until which a user in the household watched
        /// </summary>
        /// <param name="filter">Filter option for the last position</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaLastPositionListResponse List(KalturaLastPositionFilter filter)
        {
            KalturaLastPositionListResponse response = null;

            if (filter.MediaID == null && string.IsNullOrEmpty(filter.NPVRID))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "at least one of media_id and npvr_id must not be empty");
            }

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                switch (filter.By)
                {
                    case Models.ConditionalAccess.KalturaEntityReferenceBy.household:
                        response = ClientsManager.CatalogClient().GetDomainLastPosition(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId),
                            filter.UDID, filter.MediaID, filter.NPVRID);
                        break;
                    default:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Not implemented");
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