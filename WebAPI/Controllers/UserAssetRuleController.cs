using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.ObjectsConvertor.Utils;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/UserAssetRule/action")]
    public class UserAssetRuleController : ApiController
    {
        /// <summary>
        /// Disables the partner's default rule for this user        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001</remarks>
        /// <returns>Success / fail</returns>
        [Route("DisableDefault"), HttpPost]
        [ApiAuthorize]
        public bool DisableDefault()
        {
            bool success = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                success = ClientsManager.ApiClient().DisableUserDefaultParentalRule(groupId, KS.GetFromRequest().UserId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Retrieve all the rules (parental, geo, device or user-type) that applies for this user and media.        
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001, User not in household = 1005, Household does not exist = 1006</remarks>
        /// <param name="asset_id">Asset identifier</param>
        /// <param name="asset_type">Asset type</param>
        /// <param name="household_id">Media identifier</param>
        /// <param name="channel_media_id">Linear channel's media identifier</param>  
        /// <param name="udid">Device UDID</param>
        /// <returns>All the rules that applies for a specific media and a specific user according to the user parental and userType settings.</returns>
        [Route("List"), HttpPost]
        [ApiAuthorize]
        public KalturaGenericRuleListResponse List(long asset_id, int asset_type, long channel_media_id = 0, string udid = null, int household_id = 0)
        {
            List<KalturaGenericRule> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            // parameters validation
            if (asset_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_id cannot be empty");
            }

            if (!Enum.IsDefined(typeof(AssetType), asset_type))
            {
                 throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_type value is not defined");
            }
            try
            {

                if ((AssetType)asset_type == AssetType.epg)
                {
                    // call client
                    response = ClientsManager.ApiClient().GetEpgRules(groupId, KS.GetFromRequest().UserId, asset_id, household_id, channel_media_id);
                }
                else if ((AssetType)asset_type == AssetType.media)
                {
                    // call client
                    response = ClientsManager.ApiClient().GetMediaRules(groupId, KS.GetFromRequest().UserId, asset_id, household_id, udid);
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