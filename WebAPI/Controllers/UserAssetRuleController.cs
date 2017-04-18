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
        [Route("listOldStandard"), HttpPost]
        [OldStandardAction("list")]
        [ApiAuthorize]
        [Obsolete]
        public KalturaGenericRuleListResponse ListOldStandard(KalturaGenericRuleFilter filter)
        {
            List<KalturaGenericRule> response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            if (filter.AssetId == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaGenericRuleFilter.assetId");
            }

            if (!filter.AssetType.HasValue || !Enum.IsDefined(typeof(AssetType), filter.AssetType))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaGenericRuleFilter.assetType");
            }
            try
            {
                string userID = KS.GetFromRequest().UserId;

                if ((AssetType)filter.AssetType == AssetType.epg)
                {
                    // call client
                    response = ClientsManager.ApiClient().GetEpgRulesOldStandard(groupId, userID, filter.getAssetId(), (int)HouseholdUtils.GetHouseholdIDByKS(groupId));
                }
                else if ((AssetType)filter.AssetType == AssetType.media)
                {
                    // call client
                    response = ClientsManager.ApiClient().GetMediaRulesOldStandard(groupId, userID, filter.getAssetId(), (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaGenericRuleListResponse() { GenericRules = response, TotalCount = response.Count };
        }

        /// <summary>
        /// Retrieve all the rules (parental, geo, device or user-type) that applies for this user and asset.        
        /// </summary>
        /// <remarks>Possible status codes: 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001, User not in household = 1005, Household does not exist = 1006</remarks>
        /// <param name="filter">Filter</param>
        /// <returns>All the rules that applies for a specific media and a specific user according to the user parental and userType settings.</returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.DomainNotExists)]
        public KalturaUserAssetRuleListResponse List(KalturaUserAssetRuleFilter filter)
        {
            List<KalturaUserAssetRule> response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            
            if (filter.AssetIdEqual == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaUserAssetRuleFilter.assetIdEqual");
            }

            if (!filter.AssetTypeEqual.HasValue || !Enum.IsDefined(typeof(AssetType), filter.AssetTypeEqual))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaUserAssetRuleFilter.assetTypeEqual");
            }
            try
            {
                string userID = KS.GetFromRequest().UserId;

                if ((AssetType)filter.AssetTypeEqual == AssetType.epg)
                {
                    // call client
                    response = ClientsManager.ApiClient().GetEpgRules(groupId, userID, filter.getAssetId(), (int)HouseholdUtils.GetHouseholdIDByKS(groupId));
                }
                else if ((AssetType)filter.AssetTypeEqual == AssetType.media)
                {
                    // call client
                    response = ClientsManager.ApiClient().GetMediaRules(groupId, userID, filter.getAssetId(), (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaUserAssetRuleListResponse() { Rules = response, TotalCount = response.Count };
        }
    }
}