using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/userAssetsList/action")]
    [Obsolete]
    public class UserAssetsListController : ApiController
    {
        /// <summary>
        /// Retrieve the user’s private asset lists 
        /// </summary>
        /// <remarks>Possible status codes: 
        /// Invalid user = 1026</remarks>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.InvalidUser)]
        public List<KalturaUserAssetsList> List(KalturaUserAssetsListFilter filter)
        {
            List<KalturaUserAssetsList> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                switch (filter.By)
                {
                    case KalturaEntityReferenceBy.user:
                        response = ClientsManager.UsersClient().GetItemFromList(groupId, new List<string>() { KS.GetFromRequest().UserId}, filter.ListTypeEqual, filter.AssetTypeEqual);
                        break;
                    case KalturaEntityReferenceBy.household:
                        List<string> householdUserIds = HouseholdUtils.GetHouseholdUserIds(groupId);
                        if (householdUserIds != null && householdUserIds.Count > 0)
                        {
                            response = ClientsManager.UsersClient().GetItemFromList(groupId, householdUserIds, filter.ListTypeEqual, filter.AssetTypeEqual);
                        }
                        else
                        {
                            throw new ClientException((int)WebAPI.Managers.Models.StatusCode.HouseholdInvalid, "Household not found");
                        }
                        break;
                    default:
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