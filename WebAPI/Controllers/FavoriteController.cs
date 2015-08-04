using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
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
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="request">Request parameters</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, 
        /// Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User suspended = 2001, Wrong username or password = 1011</remarks>
        [Route("add"), HttpPost]
        public void Add([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string user_id, [FromUri] string udid, [FromBody] KalturaAddUserFavoriteRequest request)
        {
            int groupId = int.Parse(partner_id);

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
                ClientsManager.UsersClient().AddUserFavorite(groupId, user_id, household_id, udid, request.MediaType, request.MediaId, request.ExtraData);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

        }

        /// <summary>
        /// Remove media from user's favorite list
        /// </summary>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="user_id">User identifier</param>        
        /// <param name="media_ids">Media identifiers</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, 
        /// Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User suspended = 2001, Wrong username or password = 1011</remarks>
        [Route("delete"), HttpPost]
        public void Delete([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string user_id, [ModelBinder(typeof(WebAPI.Utils.SerializationUtils.ConvertCommaDelimitedList<int>))] List<int> media_ids)
        {
            int groupId = int.Parse(partner_id);

            // parameters validation
            if (media_ids == null | media_ids.Count == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "media_id cannot be empty");
            }

            try
            {
                // call client
                ClientsManager.UsersClient().RemoveUserFavorite(groupId, user_id, household_id, media_ids.ToArray());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

        }

        /// <summary>
        /// Retrieving users' favorites
        /// </summary>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="user_id">User identifier</param>                
        /// <param name="media_type">Related media type </param>                
        /// <param name="household_id">Household identifier</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>        
        /// <param name="language">Language Code</param>                
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, 
        /// Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("list"), HttpPost]
        public KalturaFavoriteList List([FromUri] string partner_id, [FromUri] string user_id, [FromUri] string media_type = null,
            [FromUri] int household_id = 0, [FromUri] string udid = null,
            [ModelBinder(typeof(WebAPI.Utils.SerializationUtils.ConvertCommaDelimitedList<KalturaCatalogWith>))] List<KalturaCatalogWith> with = null,
            string language = null)
        {
            List<KalturaFavorite> favorites = null;
            List<KalturaFavorite> favoritesFinalList = null;
            
            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                favorites = ClientsManager.UsersClient().GetUserFavorites(groupId, user_id, household_id, udid, media_type);
                if (favorites != null && favorites.Count > 0)
                {

                    List<int> mediaIds = favorites.Where(m => (m.Asset.Id != 0) == true).Select(x => Convert.ToInt32(x.Asset.Id)).ToList();

                    KalturaAssetInfoWrapper assetInfoWrapper = ClientsManager.CatalogClient().GetMediaByIds(groupId, user_id, household_id, udid, language, 0, 0, mediaIds, with);

                    favoritesFinalList = new List<KalturaFavorite>();
                    for (int assertIndex = 0, favoriteIndex = 0; favoriteIndex < favorites.Count; favoriteIndex++)
                    {
                        if (favorites[favoriteIndex].Asset.Id == assetInfoWrapper.Assets[assertIndex].Id)
                        {
                            favorites[favoriteIndex].Asset = assetInfoWrapper.Assets[assertIndex];
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

            return new KalturaFavoriteList() { Favorites = favoritesFinalList };
        }
    }
}