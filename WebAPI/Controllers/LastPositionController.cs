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

            if (filter.Ids == null || filter.Ids.Count == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "ids cannot be empty");
            }

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                switch (filter.Type)
                {
                    case KalturaLastPositionAssetType.media:
                        {
                            if (filter.By == KalturaEntityReferenceBy.household)
                            {
                                List<int> mediaIds;
                                try
                                {
                                    mediaIds = filter.Ids.Select(id => int.Parse(id.value)).ToList();
                                }
                                catch (Exception ex)
                                {
                                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "ids must be numeric when type is media");
                                }

                                response = new KalturaLastPositionListResponse()
                                {
                                    LastPositions = new List<KalturaLastPosition>()
                                };

                                foreach (var id in mediaIds)
                                {
                                    var res = ClientsManager.CatalogClient().GetDomainLastPosition(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId),
                                    filter.UDID, id, null);

                                    response.LastPositions.AddRange(res.LastPositions);
                                    response.TotalCount += res.TotalCount;
                                }
                                
                            }
                            else
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.NotImplemented, "Not implemented");
                            }

                        }
                        break;
                    case KalturaLastPositionAssetType.recording:
                        {
                            if (filter.By == KalturaEntityReferenceBy.household)
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.NotImplemented, "Not implemented");                                
                            }
                            else
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.NotImplemented, "Not implemented");
                            }
                        }
                    default:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.NotImplemented, "Not implemented");
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