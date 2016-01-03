using System;
using System.Collections.Generic;
using System.Linq;
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
        public KalturaAssetsPositionsResponse List(KalturaSlimAssetsFilter filter)
        {
            KalturaAssetsPositionsResponse response = null;

            if (filter.Assets == null || filter.Assets.Count == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Assets cannot be empty");
            }

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                int domain = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);
                eUserType userType;

                switch (filter.By)
                {
                    case KalturaEntityReferenceBy.user:
                        userType = eUserType.PERSONAL;
                        break;
                    case KalturaEntityReferenceBy.household:
                        userType = eUserType.HOUSEHOLD;
                        break;                        
                    default:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.NotImplemented, "Not implemented");
                }                    

                List<AssetPositionRequestInfo> assetsToRequestPositions = new List<AssetPositionRequestInfo>();

                foreach (KalturaSlimAsset asset in filter.Assets)
                {
                    AssetPositionRequestInfo assetInfo = new AssetPositionRequestInfo();
                    assetInfo.AssetID = asset.Id;
                    switch (asset.Type)
                    {
                        case KalturaAssetType.media:
                            assetInfo.AssetType = eAssetTypes.MEDIA;
                            break;
                        case KalturaAssetType.recording:
                            assetInfo.AssetType = eAssetTypes.NPVR;
                            break;
                        case KalturaAssetType.epg:
                            assetInfo.AssetType = eAssetTypes.EPG;
                            break;
                        default:
                            assetInfo.AssetType = eAssetTypes.UNKNOWN;
                            break;
                    }
                    assetsToRequestPositions.Add(assetInfo);
                }

                response = ClientsManager.CatalogClient().GetAssetsPositions(userID, groupId, domain, udid, userType, assetsToRequestPositions);
                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}