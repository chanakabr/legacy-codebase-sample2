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
        /// <remarks>Possible status codes: 
        /// Invalid user = 1026</remarks>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public List<KalturaUserAssetsList> List(KalturaUserAssetsListFilter filter)
        {
            List<KalturaUserAssetsList> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.UsersClient().GetItemFromList(groupId, filter.usersIDs.Select(x => x.value).ToList(), 
                    filter.ListType, filter.AssetType);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}