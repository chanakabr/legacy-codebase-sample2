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
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/UserAssetsList/action")]
    public class UserAssetsListController : ApiController
    {
        /// <summary>
        /// Returns all the items added to a list.
        /// </summary>
        /// <param name="user_ids">Users identifiers</param>
        /// <param name="list_type">The requested list type</param>
        /// <param name="asset_type">The requested asset type</param>
        /// <remarks>Possible status codes: 
        /// Invalid user = 1026</remarks>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public List<KalturaUserAssetsList> List(List<KalturaStringValue> user_ids, KalturaUserAssetsListType list_type,
            KalturaUserAssetsListItemType asset_type)
        {
            List<KalturaUserAssetsList> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.UsersClient().GetItemFromList(groupId, user_ids.Select(x => x.value).ToList(), list_type, asset_type);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }

            return response;
        }
    }
}