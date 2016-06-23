using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/favorite/action")]
    [OldStandard("addOldStandard", "add")]
    [OldStandard("deleteOldStandard", "delete")]
    [OldStandard("listOldStandard", "list")]
    public class FavoriteController : ApiController
    {
        /// <summary>
        /// Add media to user's favorite list
        /// </summary>        
        /// <param name="favorite">Favorite details.</param>
        /// <remarks>Possible status codes: User does not exist = 2000, User suspended = 2001, Wrong username or password = 1011</remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaFavorite Add(KalturaFavorite favorite)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;
            int domainId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);
            string udid = KSUtils.ExtractKSPayload().UDID;
            
            KalturaAssetInfo asset;
            try
            {

                KalturaAssetInfoListResponse assetInfoListResponse = ClientsManager.CatalogClient().GetMediaByIds(groupId, userId, domainId, udid, null, 0, 1, new List<int>() { (int)favorite.AssetId }, null);
                asset = assetInfoListResponse.Objects.First();

                // call client
                ClientsManager.UsersClient().AddUserFavorite(groupId, userId, domainId, udid, asset.getType().ToString(), favorite.AssetId.ToString(), favorite.ExtraData);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            KalturaFavorite response = new KalturaFavorite();
            response.AssetId = favorite.AssetId;
            response.ExtraData = favorite.ExtraData;
            return response;
        }

        /// <summary>
        /// Add media to user's favorite list
        /// </summary>        
        /// <param name="media_type">Media Type ID (according to media type IDs defined dynamically in the system).</param>
        /// <param name="media_id">Media id</param>
        /// <param name="extra_data">Extra data</param>        
        /// <remarks>Possible status codes: User does not exist = 2000, User suspended = 2001, Wrong username or password = 1011</remarks>
        [Route("addOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public bool AddOldStandard(string media_id, string media_type = null, string extra_data = null)
        {
            bool res = false;
            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            // parameters validation
            if (media_type.Trim().Length == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "media_id cannot be empty");
            }

            if (media_type.Trim().Length == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "media_type cannot be empty");
            }

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // call client
                res = ClientsManager.UsersClient().AddUserFavorite(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, media_type,
                    media_id, extra_data);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }

        /// <summary>
        /// Remove media from user's favorite list
        /// </summary>        
        /// <param name="id">Media identifier</param>
        /// <remarks>Possible status codes: User does not exist = 2000, User suspended = 2001, Wrong username or password = 1011</remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int id)
        {
            bool res = false;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;

                // call client
                res = ClientsManager.UsersClient().RemoveUserFavorite(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), new int[] {id});
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }

        /// <summary>
        /// Remove media from user's favorite list
        /// </summary>        
        /// <param name="media_ids">Media identifiers</param>
        /// <remarks>Possible status codes: User does not exist = 2000, User suspended = 2001, Wrong username or password = 1011</remarks>
        [Route("deleteOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public bool DeleteOldStandard(List<KalturaIntegerValue> media_ids)
        {
            bool res = false;
            int groupId = KS.GetFromRequest().GroupId;

            // parameters validation
            if (media_ids == null || media_ids.Count == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "media_id cannot be empty");
            }

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;

                // call client
                res = ClientsManager.UsersClient().RemoveUserFavorite(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), media_ids.Select(x => x.value).ToArray());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }

        /// <summary>
        /// Retrieving users' favorites
        /// </summary>            
        /// <param name="filter">Request filter</param>
        /// <remarks>Possible status codes: User does not exist = 2000, User suspended = 2001</remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaFavoriteListResponse List(KalturaFavoriteFilter filter = null)
        {
            List<KalturaFavorite> favorites = null;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            if (filter == null)
                filter = new KalturaFavoriteFilter();

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // no media ids to filter from - use the regular favorites function
                List<int> mediaIds = filter.getMediaIdIn();
                if (mediaIds == null || mediaIds.Count == 0)
                {
                    favorites = ClientsManager.UsersClient().GetUserFavorites(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, filter.MediaTypeIn != 0 ? filter.MediaTypeIn.ToString() : string.Empty, filter.OrderBy);
                }
                else
                {
                    favorites = ClientsManager.UsersClient().FilterFavoriteMedias(groupId, userID, mediaIds, udid, filter.MediaTypeIn != 0 ? filter.MediaTypeIn.ToString() : null, filter.OrderBy);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaFavoriteListResponse() { Favorites = favorites, TotalCount = favorites != null ? favorites.Count : 0 };
        }

        /// <summary>
        /// Retrieving users' favorites
        /// </summary>            
        /// <param name="filter">Request filter</param>                        
        /// <param name="udid">device identifier</param>                        
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>        
        /// <remarks>Possible status codes: User does not exist = 2000, User suspended = 2001</remarks>
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaFavoriteListResponse ListOldStandard(KalturaFavoriteFilter filter = null, List<KalturaCatalogWithHolder> with = null, string udid = null)
        {
            List<KalturaFavorite> favorites = null;
            List<KalturaFavorite> favoritesFinalList = null;

            int groupId = KS.GetFromRequest().GroupId;
            string language = Utils.Utils.GetLanguageFromRequest();

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (filter == null)
                filter = new KalturaFavoriteFilter();

            try
            {
                string userID = KS.GetFromRequest().UserId;
                List<int> mediaIds = filter.getMediaIdIn();

                // no media ids to filter from - use the regular favorites function
                if (mediaIds == null || mediaIds.Count == 0)
                {
                    favorites = ClientsManager.UsersClient().GetUserFavorites(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), filter.UDID,
                        filter.MediaTypeIn != 0 ? filter.MediaTypeIn.ToString() : string.Empty, filter.OrderBy);
                }
                else
                {
                    favorites = ClientsManager.UsersClient().FilterFavoriteMedias(groupId, userID, mediaIds, udid, filter.MediaTypeIn != 0 ? filter.MediaTypeIn.ToString() : null, filter.OrderBy);
                }

                // get assets
                if (favorites != null && favorites.Count > 0)
                {
                    mediaIds = favorites.Where(m => (m.AssetId != 0) == true).Select(x => Convert.ToInt32(x.AssetId)).Distinct().ToList();

                    KalturaAssetInfoListResponse assetInfoWrapper = ClientsManager.CatalogClient().GetMediaByIds(groupId, KS.GetFromRequest().UserId,
                        (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language, 0, 0, mediaIds, with.Select(x => x.type).ToList());

                    favoritesFinalList = new List<KalturaFavorite>();
                    for (int assertIndex = 0, favoriteIndex = 0; favoriteIndex < favorites.Count; favoriteIndex++)
                    {
                        if (favorites[favoriteIndex].AssetId == assetInfoWrapper.Objects[assertIndex].Id)
                        {
                            favorites[favoriteIndex].Asset = assetInfoWrapper.Objects[assertIndex];
                            favoritesFinalList.Add(favorites[favoriteIndex]);
                            assertIndex++;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaFavoriteListResponse() { Favorites = favoritesFinalList, TotalCount = favoritesFinalList != null ? favoritesFinalList.Count : 0 };
        }
    }
}