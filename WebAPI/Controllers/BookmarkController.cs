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
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/Bookmark/action")]
    public class BookmarkController : ApiController
    {
        /// <summary>
        /// Returns the last position (in seconds) in a media or nPVR asset until which a user in the household watched
        /// </summary>
        /// <param name="filter">Filter option for the last position</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetsBookmarksResponse List(KalturaAssetsBookmarksFilter filter)
        {
            KalturaAssetsBookmarksResponse response = null;

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

                List<AssetBookmarkRequest> assetsToRequestPositions = new List<AssetBookmarkRequest>();

                foreach (KalturaSlimAsset asset in filter.Assets)
                {
                    AssetBookmarkRequest assetInfo = new AssetBookmarkRequest();
                    assetInfo.AssetID = asset.Id;
                    bool addToRequest = true;
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
                            addToRequest = false;
                            break;
                    }
                    if(addToRequest)
                    {
                        assetsToRequestPositions.Add(assetInfo);
                    }
                }

                response = ClientsManager.CatalogClient().GetAssetsBookmarks(userID, groupId, domain, udid, assetsToRequestPositions);
                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Report player position and action for the user on the watched asset. Player position is used to later allow resume watching.
        /// </summary>
        /// <param name="assetId">Internal identifier of the asset </param>
        /// <param name="assetType">The type of the asset. Possible values <VOD, nPVR, Catch-Up> </param>
        /// <param name="fileId">Identifier of the file</param>
        /// <param name="PlayerAssetData">Data regarding players status for the asset</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, Bad search request = 4002, ConcurrencyLimitation = 4001</remarks>
        [Route("set"), HttpPost]
        [ApiAuthorize(true)]
        public bool Set(string assetId, eAssetTypes assetType, long fileId, KalturaPlayerAssetData PlayerAssetData)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                int householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);
                string siteGuid = KS.GetFromRequest().UserId;
                response = ClientsManager.CatalogClient().SetBookmark(groupId, siteGuid, householdId, udid, assetId, assetType, fileId, PlayerAssetData);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}