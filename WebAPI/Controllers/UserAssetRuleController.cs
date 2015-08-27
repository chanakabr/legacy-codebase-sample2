using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/UserAssetRule/action")]
    public class UserAssetRuleController : ApiController
    {
        /// <summary>
        /// Retrieve all the rules (parental, geo, device or user-type) that applies for this user and media.        
        /// </summary>
        /// <remarks>Possible status codes: 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001, User not in household = 1005, Household does not exist = 1006</remarks>
        /// <param name="asset_id">Asset identifier</param>
        /// <param name="asset_type">Asset type</param>        
        /// <param name="udid">Device UDID</param>
        /// <returns>All the rules that applies for a specific media and a specific user according to the user parental and userType settings.</returns>
        [Route("List"), HttpPost]
        [ApiAuthorize]
        public KalturaGenericRuleListResponse List(long asset_id, int asset_type, string udid = null)
        {
            List<KalturaGenericRule> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            // parameters validation
            if (asset_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_id cannot be empty");
            }

            if (!Enum.IsDefined(typeof(KalturaAssetType), asset_type))
            {
                 throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_type value is not defined");
            }
            try
            {
                string userID = KS.GetFromRequest().UserId;

                if ((KalturaAssetType)asset_type == KalturaAssetType.epg)
                {
                    // call client
                    response = ClientsManager.ApiClient().GetEpgRules(groupId, userID, asset_id, (int)HouseholdUtils.GetHouseholdIDByKS(groupId));
                }
                else if ((KalturaAssetType)asset_type == KalturaAssetType.media)
                {
                    // call client
                    response = ClientsManager.ApiClient().GetMediaRules(groupId, userID, asset_id, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaGenericRuleListResponse() { GenericRules = response, TotalCount = response.Count };
        }
    }
}