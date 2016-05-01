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
using WebAPI.ObjectsConvertor.Mapping.Utils;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/userAssetRule/action")]
    public class UserAssetRuleController : ApiController
    {
        /// <summary>
        /// Retrieve all the rules (parental, geo, device or user-type) that applies for this user and asset.        
        /// </summary>
        /// <remarks>Possible status codes: 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001, User not in household = 1005, Household does not exist = 1006</remarks>
        /// <param name="filter">Filter</param>
        /// <returns>All the rules that applies for a specific media and a specific user according to the user parental and userType settings.</returns>
        [Route("List"), HttpPost]
        [ApiAuthorize]
        public KalturaGenericRuleListResponse List(KalturaGenericRuleFilter filter)
        {
            List<KalturaGenericRule> response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            // parameters validation
            if (filter == null)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter cannot be null");
            }

            if (filter.AssetId == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_id cannot be empty");
            }

            if (!Enum.IsDefined(typeof(AssetType), filter.AssetType))
            {
                 throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_type value is not defined");
            }
            try
            {
                string userID = KS.GetFromRequest().UserId;

                if ((AssetType)filter.AssetType == AssetType.epg)
                {
                    // call client
                    response = ClientsManager.ApiClient().GetEpgRules(groupId, userID, filter.getAssetId(), (int)HouseholdUtils.GetHouseholdIDByKS(groupId));
                }
                else if ((AssetType)filter.AssetType == AssetType.media)
                {
                    // call client
                    response = ClientsManager.ApiClient().GetMediaRules(groupId, userID, filter.getAssetId(), (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid);
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