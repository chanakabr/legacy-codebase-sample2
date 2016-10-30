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
    [RoutePrefix("_service/parentalRule/action")]
    [OldStandardAction("listOldStandard", "list")]
    public class ParentalRuleController : ApiController 
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
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.HouseholdNotExists)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserWithNoHousehold)]
        [Throws(eResponseStatus.UserSuspended)]
        public KalturaParentalRuleListResponse ListOldStandard(KalturaRuleFilter filter)
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
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.HouseholdNotExists)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserWithNoHousehold)]
        [Throws(eResponseStatus.UserSuspended)]
        public KalturaParentalRuleListResponse List(KalturaParentalRuleFilter filter)
        {
            List<KalturaParentalRule> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (!filter.EntityReferenceEqual.HasValue)
                {
                    response = ClientsManager.ApiClient().GetGroupParentalRules(groupId);
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
        [Route("enable"), HttpPost]
        [ApiAuthorize]
        [OldStandard("entityReference", "by")]
        [OldStandard("ruleId", "rule_id")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.HouseholdNotExists)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserWithNoHousehold)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.RuleNotExists)]
        public bool Enable(long ruleId, KalturaEntityReferenceBy entityReference)
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
        [Route("disable"), HttpPost]
        [ApiAuthorize]
        [OldStandard("entityReference", "by")]
        [OldStandard("ruleId", "rule_id")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.HouseholdNotExists)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserWithNoHousehold)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.RuleNotExists)]
        [Throws(eResponseStatus.UserParentalRuleNotExists)]
        public bool Disable(long ruleId, KalturaEntityReferenceBy entityReference)
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
        [Route("disableDefault"), HttpPost]
        [ApiAuthorize]
        [OldStandard("entityReference", "by")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.HouseholdNotExists)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserWithNoHousehold)]
        [Throws(eResponseStatus.UserSuspended)]
        public bool DisableDefault(KalturaEntityReferenceBy entityReference)
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
    }
}