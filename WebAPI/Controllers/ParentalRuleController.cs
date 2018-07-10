using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("parentalRule")]
    public class ParentalRuleController : IKalturaController
    {
        /// <summary>
        /// Return the parental rules that applies for the user or household. Can include rules that have been associated in account, household, or user level.
        /// Association level is also specified in the response.
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <remarks>
        /// Possible status codes: 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001
        /// </remarks>
        /// <returns>List of parental rules applied to the user</returns>
        [Action("listOldStandard")]
        [OldStandardAction("list")]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.UserSuspended)]
        static public KalturaParentalRuleListResponse ListOldStandard(KalturaRuleFilter filter)
        {
            List<KalturaParentalRule> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (filter.By == KalturaEntityReferenceBy.user)
                {
                    string userId = KS.GetFromRequest().UserId;

                    // call client
                    response = ClientsManager.ApiClient().GetUserParentalRules(groupId, userId);
                }
                else if (filter.By == KalturaEntityReferenceBy.household)
                {
                    // call client
                    response = ClientsManager.ApiClient().GetDomainParentalRules(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId));
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaParentalRuleListResponse() { ParentalRule = response, TotalCount = response != null ? response.Count : 0 };
        }

        /// <summary>
        /// Return the parental rules that applies for the user or household. Can include rules that have been associated in account, household, or user level.
        /// Association level is also specified in the response.
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <remarks>
        /// Possible status codes: 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001
        /// </remarks>
        /// <returns>List of parental rules applied to the user</returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.UserSuspended)]
        static public KalturaParentalRuleListResponse List(KalturaParentalRuleFilter filter)
        {
            List<KalturaParentalRule> response = null;
            KS ks = KS.GetFromRequest();
            int groupId = ks.GroupId;

            try
            {
                if (!filter.EntityReferenceEqual.HasValue)
                {
                    bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(groupId, ks.UserId);
                    response = ClientsManager.ApiClient().GetGroupParentalRules(groupId, isAllowedToViewInactiveAssets);
                }
                else if (filter.EntityReferenceEqual.Value == KalturaEntityReferenceBy.user)
                {
                    string userId = KS.GetFromRequest().UserId;

                    // call client
                    response = ClientsManager.ApiClient().GetUserParentalRules(groupId, userId);
                }
                else if (filter.EntityReferenceEqual.Value == KalturaEntityReferenceBy.household)
                {
                    // call client
                    response = ClientsManager.ApiClient().GetDomainParentalRules(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId));
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaParentalRuleListResponse() { ParentalRule = response, TotalCount = response != null ? response.Count : 0 };
        }

        /// <summary>
        /// Enable a parental rules for a user  
        /// </summary>
        /// <param name="entityReference">Reference type to filter by</param>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001, Invalid rule = 5003</remarks>
        /// <param name="ruleId">Rule Identifier</param>
        /// <returns>Success or failure and reason</returns>
        [Action("enable")]
        [ApiAuthorize]
        [OldStandardArgument("entityReference", "by")]
        [OldStandardArgument("ruleId", "rule_id")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.RuleNotExists)]
        static public bool Enable(long ruleId, KalturaEntityReferenceBy entityReference)
        {
            bool success = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (entityReference == KalturaEntityReferenceBy.user)
                {
                    string userId = KS.GetFromRequest().UserId;

                    // call client
                    success = ClientsManager.ApiClient().SetUserParentalRule(groupId, userId, ruleId, 1);
                }
                else if (entityReference == KalturaEntityReferenceBy.household)
                {
                    // call client
                    success = ClientsManager.ApiClient().SetDomainParentalRules(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), ruleId, 1);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Disables a parental rule that was previously defined by the household master. Disable can be at specific user or household level.  
        /// </summary>
        /// <param name="entityReference">Reference type to filter by</param>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001, Invalid rule = 5003, 
        /// Cannot disable a default rule that was not specifically enabled previously = 5021 </remarks>
        /// <param name="ruleId">Rule Identifier</param>
        /// <returns>Success or failure and reason</returns>
        [Action("disable")]
        [ApiAuthorize]
        [OldStandardArgument("entityReference", "by")]
        [OldStandardArgument("ruleId", "rule_id")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.RuleNotExists)]
        [Throws(eResponseStatus.UserParentalRuleNotExists)]
        static public bool Disable(long ruleId, KalturaEntityReferenceBy entityReference)
        {
            bool success = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (entityReference == KalturaEntityReferenceBy.user)
                {
                    string userId = KS.GetFromRequest().UserId;

                    // call client
                    success = ClientsManager.ApiClient().SetUserParentalRule(groupId, userId, ruleId, 0);
                }
                else if (entityReference == KalturaEntityReferenceBy.household)
                {
                    // call client
                    success = ClientsManager.ApiClient().SetDomainParentalRules(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), ruleId, 0);
                }

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Disables a parental rule that was defined at account level. Disable can be at specific user or household level.
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006, User does not exist = 2000, User with no household = 2024, User suspended = 2001
        /// </remarks>
        /// <param name="entityReference">Reference type to filter by</param>
        /// <returns>Success / fail</returns>
        [Action("disableDefault")]
        [ApiAuthorize]
        [OldStandardArgument("entityReference", "by")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.UserSuspended)]
        static public bool DisableDefault(KalturaEntityReferenceBy entityReference)
        {
            bool success = false;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (entityReference == KalturaEntityReferenceBy.household)
                {
                    // call client
                    success = ClientsManager.ApiClient().DisableDomainDefaultParentalRule(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId));
                }
                else if (entityReference == KalturaEntityReferenceBy.user)
                {
                    string userId = KS.GetFromRequest().UserId;

                    // call client
                    success = ClientsManager.ApiClient().DisableUserDefaultParentalRule(groupId, userId);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Add a new parentalRule
        /// </summary>
        /// <param name="parentalRule">parentalRule object</param>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.ParentalRuleNameAlreadyInUse)]
        [Throws(eResponseStatus.TagDoesNotExist)]
        public KalturaParentalRule Add(KalturaParentalRule parentalRule)
        {
            KalturaParentalRule response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();
            if (string.IsNullOrEmpty(parentalRule.name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            if (!parentalRule.order.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "order");
            }

            bool isMediaTag = parentalRule.mediaTagTypeId.HasValue;
            bool isEpgTag = parentalRule.epgTagTypeId.HasValue;
            if (!isMediaTag && !isEpgTag)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "mediaTagTypeId", "epgTagTypeId");
            }

            if (isMediaTag && (parentalRule.mediaTagValues == null || parentalRule.mediaTagValues.Count == 0))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "mediaTagTypeId", "mediaTagValues");
            }

            if (isEpgTag && (parentalRule.epgTagTypeId == null || parentalRule.epgTagValues.Count == 0))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "epgTagTypeId", "epgTagValues");
            }

            if (!parentalRule.ruleType.HasValue)
            {
                // set ruleType default value to ALL if not sent, same as existing behavior on old TVM
                parentalRule.ruleType = KalturaParentalRuleType.ALL;                
            }

            if (!parentalRule.blockAnonymousAccess.HasValue)
            {
                // set blockAnonymousAccess default value to FALSE if not sent, same as existing behavior on old TVM
                parentalRule.blockAnonymousAccess = false;
            }

            try
            {
                response = ClientsManager.ApiClient().AddParentalRule(groupId, parentalRule, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update an existing parentalRule
        /// </summary>
        /// <param name="id">parentalRule identifier</param>
        /// <param name="parentalRule">parentalRule object</param>
        /// <returns></returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.ParentalRuleNameAlreadyInUse)]
        [Throws(eResponseStatus.ParentalRuleDoesNotExist)]
        [Throws(eResponseStatus.TagDoesNotExist)]
        [SchemeArgument("id", MinLong = 1)]
        public KalturaParentalRule Update(long id, KalturaParentalRule parentalRule)
        {
            KalturaParentalRule response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();
            if (parentalRule.name != null && parentalRule.name == string.Empty)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            bool isMediaTag = parentalRule.mediaTagTypeId.HasValue;
            bool isEpgTag = parentalRule.epgTagTypeId.HasValue;
            if (!isMediaTag && !isEpgTag)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "mediaTagTypeId", "epgTagTypeId");
            }

            if (isMediaTag && (parentalRule.mediaTagValues == null || parentalRule.mediaTagValues.Count == 0))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "mediaTagTypeId", "mediaTagValues");
            }

            if (isEpgTag && (parentalRule.epgTagTypeId == null || parentalRule.epgTagValues.Count == 0))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "epgTagTypeId", "epgTagValues");
            }

            try
            {
                response = ClientsManager.ApiClient().UpdateParentalRule(groupId, id, parentalRule, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Get an existing parentalRule by identifier
        /// </summary>
        /// <param name="id">parentalRule identifier</param>        
        /// <returns></returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.ParentalRuleDoesNotExist)]
        [SchemeArgument("id", MinLong = 1)]
        public KalturaParentalRule Get(long id)
        {
            KalturaParentalRule response = null;
            KS ks = KS.GetFromRequest();
            int groupId = ks.GroupId;

            try
            {
                bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(groupId, ks.UserId);
                response = ClientsManager.ApiClient().GetParentalRule(groupId, id, isAllowedToViewInactiveAssets);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete an existing parentalRule
        /// </summary>
        /// <param name="id">parentalRule identifier</param>        
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.ParentalRuleDoesNotExist)]
        [Throws(eResponseStatus.CanNotDeleteDefaultParentalRule)]
        [SchemeArgument("id", MinLong = 1)]
        public bool Delete(long id)
        {
            bool result = false;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                result = ClientsManager.ApiClient().DeleteParentalRule(groupId, id, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

    }
}