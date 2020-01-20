using ApiObjects.Response;
using System;
using System.Collections.Generic;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("userAssetsList")]
    [Obsolete]
    public class UserAssetsListController : IKalturaController
    {
        /// <summary>
        /// Retrieve the user’s private asset lists 
        /// </summary>
        /// <remarks>Possible status codes: 
        /// Invalid user = 1026</remarks>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.InvalidUser)]
        static public List<KalturaUserAssetsList> List(KalturaUserAssetsListFilter filter)
        {
            List<KalturaUserAssetsList> response = null;

            int groupId = KSManager.GetKSFromRequest().GroupId;

            try
            {
                // call client
                switch (filter.By)
                {
                    case KalturaEntityReferenceBy.user:
                        response = ClientsManager.UsersClient().GetItemFromList(groupId, new List<string>() { KSManager.GetKSFromRequest().UserId}, filter.ListTypeEqual, filter.AssetTypeEqual);
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