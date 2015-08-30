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

            if (string.IsNullOrEmpty(filter.AssetID))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_id cannot be empty");
            }

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                switch (filter.AssetType)
                {
                    case KalturaAssetType.media:
                        {
                            if (filter.By == KalturaEntityReferenceBy.household)
                            {
                                int mediaId;
                                if (!int.TryParse(filter.AssetID, out mediaId))
                                {
                                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_id must be numeric for type media");
                                }
                                response = ClientsManager.CatalogClient().GetDomainLastPosition(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId),
                                    filter.UDID, mediaId, null);
                            }
                            else
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.NotImplemented, "Not implemented");
                            }

                        }
                        break;
                    case KalturaAssetType.recording:
                        {
                            if (filter.By == KalturaEntityReferenceBy.household)
                            {
                                response = ClientsManager.CatalogClient().GetDomainLastPosition(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId),
                                    filter.UDID, null, filter.AssetID);
                            }
                            else
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.NotImplemented, "Not implemented");
                            }
                        }
                        break;
                    default:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.NotImplemented, "Not implemented");
                        break;
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