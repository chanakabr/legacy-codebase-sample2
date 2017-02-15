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
    [Obsolete]
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
                throw new BadRequestException(new WebAPI.Exceptions.ApiException.ApiExceptionType(WebAPI.Managers.Models.StatusCode.BadRequest, "ids cannot be empty", null));
            }

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;

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
                                    throw new BadRequestException(new WebAPI.Exceptions.ApiException.ApiExceptionType(WebAPI.Managers.Models.StatusCode.BadRequest, "ids must be numeric when type is media", null));
                                }

                                response = new KalturaLastPositionListResponse()
                                {
                                    LastPositions = new List<KalturaLastPosition>()
                                };

                                List<KalturaSlimAsset> assets = new List<KalturaSlimAsset>();
                                foreach (var id in mediaIds)
                                {
                                    assets.Add(new KalturaSlimAsset(){ Id = id.ToString(), Type = KalturaAssetType.media });
                                }

                                var household = HouseholdUtils.GetHouseholdFromRequest();
                                if (household != null && household.DefaultUsers != null && household.DefaultUsers.Count > 0)
                                {
                                    response = ClientsManager.CatalogClient().GetAssetsLastPositionBookmarks(household.DefaultUsers[0].Id, groupId, 
                                        (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, assets);
                                }
                            }
                            else
                            {
                                throw new BadRequestException(new WebAPI.Exceptions.ApiException.ApiExceptionType(WebAPI.Managers.Models.StatusCode.NotImplemented, "Not implemented", null));
                            }

                        }
                        break;
                    case KalturaLastPositionAssetType.recording:
                        {
                            if (filter.By == KalturaEntityReferenceBy.household)
                            {
                                throw new BadRequestException(new WebAPI.Exceptions.ApiException.ApiExceptionType(WebAPI.Managers.Models.StatusCode.NotImplemented, "Not implemented", null));
                            }
                            else
                            {
                                throw new BadRequestException(new WebAPI.Exceptions.ApiException.ApiExceptionType(WebAPI.Managers.Models.StatusCode.NotImplemented, "Not implemented", null));
                            }
                        }
                    default:
                        throw new BadRequestException(new WebAPI.Exceptions.ApiException.ApiExceptionType(WebAPI.Managers.Models.StatusCode.NotImplemented, "Not implemented", null));
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