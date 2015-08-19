using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/favorite/action")]
    public class FavoriteController : ApiController
    {
        /// <summary>
        /// Add media to user's favorite list
        /// </summary>
        /// <param name="household_id">Household identifier</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="request">Request parameters</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, 
        /// Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User suspended = 2001, Wrong username or password = 1011</remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public bool Add(int household_id, string udid, KalturaAddUserFavoriteRequest request)
        {
            bool res = false;
            int groupId = KS.GetFromRequest().GroupId;

            // parameters validation
            if (request.MediaType.Trim().Length == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "media_id cannot be empty");
            }

            if (request.MediaType.Trim().Length == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "media_type cannot be empty");
            }

            if (udid.Trim().Length == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "UDID cannot be empty");
            }

            try
            {
                // call client
                res = ClientsManager.UsersClient().AddUserFavorite(groupId, KS.GetFromRequest().UserId, household_id, udid, request.MediaType,
                    request.MediaId, request.ExtraData);
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
        /// <param name="household_id">Household identifier</param>
        /// <param name="media_ids">Media identifiers</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, 
        /// Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User suspended = 2001, Wrong username or password = 1011</remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public void Delete(int household_id, List<KalturaIntegerValue> media_ids)
        {
            int groupId = KS.GetFromRequest().GroupId;

            // parameters validation
            if (media_ids == null || media_ids.Count == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "media_id cannot be empty");
            }

            try
            {
                // call client
                ClientsManager.UsersClient().RemoveUserFavorite(groupId, KS.GetFromRequest().UserId, household_id, media_ids.Select(x=> x.value).ToArray());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

        }

        /// <summary>
        /// Retrieving users' favorites
        /// </summary>            
        /// <param name="filter">Request filter</param>                
        /// <param name="household_id">Household identifier</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>        
        /// <param name="language">Language Code</param>                
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, 
        /// Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008
        /// User does not exist = 2000, User suspended = 2001</remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaFavoriteListResponse List(KalturaFavoriteFilter filter, int household_id = 0, List<KalturaCatalogWithHolder> with = null, string language = null)
        {
            List<KalturaFavorite> favorites = null;
            List<KalturaFavorite> favoritesFinalList = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            try
            {
                // call client
                // no media ids to filter from - use the regular favorites function
                if (filter.MediaIds == null || filter.MediaIds.Count == 0)
                {
                    favorites = ClientsManager.UsersClient().GetUserFavorites(groupId, KS.GetFromRequest().UserId, household_id, filter.UDID, filter.MediaType);
                }
                else
                {
                    favorites = ClientsManager.UsersClient().FilterFavoriteMedias(groupId, KS.GetFromRequest().UserId, filter.MediaIds.Select(id => id.value).ToList());
                }
                if (favorites != null && favorites.Count > 0)
                {
                    List<int> mediaIds = favorites.Where(m => (m.Asset.Id != 0) == true).Select(x => Convert.ToInt32(x.Asset.Id)).ToList();

                    KalturaAssetInfoListResponse assetInfoWrapper = ClientsManager.CatalogClient().GetMediaByIds(groupId, KS.GetFromRequest().UserId, household_id,
                        filter.UDID, language, 0, 0, mediaIds, with.Select(x => x.type).ToList());

                    favoritesFinalList = new List<KalturaFavorite>();
                    for (int assertIndex = 0, favoriteIndex = 0; favoriteIndex < favorites.Count; favoriteIndex++)
                    {
                        if (favorites[favoriteIndex].Asset.Id == assetInfoWrapper.Objects[assertIndex].Id)
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

            return new KalturaFavoriteListResponse() { Favorites = favoritesFinalList, TotalCount = favoritesFinalList.Count };
        }
    }
}