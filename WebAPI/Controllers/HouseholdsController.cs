using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.API;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("households")]
    public class HouseholdsController : ApiController
    {
        #region Parental and Purchase rules

        /// <summary>
        /// Return the parental rules that applies to the household. 
        /// Can include rules that have been associated in account or household.
        /// Possible status codes: DomainNotExists = 1006
        /// </summary>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="partner_id">Partner identifier</param>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <returns>List of parental rules applied to the household</returns>
        [Route("{household_id}/parental/rules"), HttpGet]
        public List<ParentalRule> GetParentalRules([FromUri] string partner_id, [FromUri] int household_id)
        {
            List<ParentalRule> response = null;

            // parameters validation
            int groupId;
            if (!int.TryParse(partner_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "partner_id must be an integer");
            }

            if (household_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetDomainParentalRules(groupId, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Enabled a parental rule for a specific household.
        /// Possible status codes: DomainNotExists = 1006
        /// </summary>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="rule_id">Rule Identifier</param>
        /// <param name="partner_id">Partner identifier</param>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <returns>Success or failure and reason</returns>
        [Route("{household_id}/parental/rules/{rule_id}"), HttpPost]
        public bool EnableParentalRule([FromUri] string partner_id, [FromUri] int household_id, [FromUri] long rule_id)
        {
            bool success = false;

            // parameters validation
            int groupId;
            if (!int.TryParse(partner_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "partner_id must be an integer");
            }

            if (household_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "household_id cannot be empty");
            }

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetDomainParentalRules(groupId, household_id, rule_id, 1);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Disables a parental rule for a specific household.
        /// Possible status codes: DomainNotExists = 1006
        /// </summary>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="rule_id">Rule Identifier</param>
        /// <param name="partner_id">Partner identifier</param>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <returns>Success or failure and reason</returns>
        [Route("{household_id}/parental/rules/{rule_id}"), HttpDelete]
        public bool DisableParentalRule([FromUri] string partner_id, [FromUri] int household_id, [FromUri] long rule_id)
        {
            bool success = false;

            // parameters validation
            int groupId;
            if (!int.TryParse(partner_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "partner_id must be an integer");
            }


            if (household_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "household_id cannot be empty");
            }

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetDomainParentalRules(groupId, household_id, rule_id, 0);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Retrieve the parental PIN that applies for the household..
        /// Possible status codes: DomainNotExists = 1006
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <returns>The PIN that applies for the household</returns>
        [Route("{household_id}/parental/pin/"), HttpGet]
        public PinResponse GetParentalPIN([FromUri] string partner_id, [FromUri] int household_id)
        {
            PinResponse pinResponse = null;

            int groupId;
            if (!int.TryParse(partner_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "partner_id must be an integer");
            }

            if (household_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "household_id cannot be empty");
            }

            try
            {
                // call client
                pinResponse = ClientsManager.ApiClient().GetDomainParentalPIN(groupId, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return pinResponse;
        }

        /// <summary>
        /// Set the parental PIN that applies for the household.
        /// Possible status codes: DomainNotExists = 1006
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="pin">New PIN to set</param>
        /// <returns>Success / Fail</returns>
        [Route("{household_id}/parental/pin"), HttpPost]
        public bool SetParentalPIN([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string pin)
        {
            bool success = false;

            int groupId;
            if (!int.TryParse(partner_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "partner_id must be an integer");
            }

            if (household_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "household_id cannot be empty");
            }

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetDomainParentalRules(groupId, household_id, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Retrieve the purchase settings that applies for the household.
        /// Possible status codes:
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <returns>The purchase settings that apply for the user</returns>
        [Route("{household_id}/purchase/settings"), HttpGet]
        public PurchaseSettingsResponse GetPurchaseSettings([FromUri] string partner_id, [FromUri] int household_id)
        {
            PurchaseSettingsResponse purchaseResponse = null;

            int groupId;
            if (!int.TryParse(partner_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "partner_id must be an integer");
            }

            if (household_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "household_id cannot be empty");
            }

            try
            {
                // call client
                purchaseResponse = ClientsManager.ApiClient().GetDomainPurchaseSettings(groupId, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return purchaseResponse;
        }

        /// <summary>
        /// Set the purchase settings that applies for the household.
        /// Possible status codes: DomainNotExists = 1006
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="setting">New settings to apply</param>
        /// <returns>Success / Fail</returns>
        [Route("{household_id}/purchase/settings/"), HttpPost]
        public bool SetPurchaseSettings([FromUri] string partner_id, [FromUri] int household_id, [FromUri] int setting)
        {
            bool success = false;

            int groupId;
            if (!int.TryParse(partner_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "partner_id must be an integer");
            }

            if (household_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "household_id cannot be empty");
            }

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetDomainPurchaseSettings(groupId, household_id, setting);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Retrieve the purchase PIN that applies for the household.
        /// Possible status codes: 5001 = No PIN defined, DomainNotExists = 1006
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <returns>The PIN that applies for the household</returns>
        [Route("{household_id}/purchase/pin/"), HttpGet]
        public PinResponse GetPurchasePIN([FromUri] string partner_id, [FromUri] int household_id)
        {
            PinResponse pinResponse = null;

            int groupId;
            if (!int.TryParse(partner_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "partner_id must be an integer");
            }

            if (household_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "household_id cannot be empty");
            }

            try
            {
                // call client
                var response = ClientsManager.ApiClient().GetDomainPurchasePIN(groupId, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return pinResponse;
        }

        /// <summary>
        /// Set the purchase PIN that applies for the household.
        /// Possible status codes: DomainNotExists = 1006
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="pin">New PIN to apply</param>
        /// <returns>Success / Fail</returns>
        [Route("{household_id}/purchase/pin"), HttpPost]
        public bool SetPurchasePIN([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string pin)
        {
            bool success = false;

            int groupId;
            if (!int.TryParse(partner_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "partner_id must be an integer");
            }

            if (household_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "household_id cannot be empty");
            }

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetDomainPurchasePIN(groupId, household_id, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        #endregion


        #region ConditionalAccess
        /// <summary>
        /// Immediately cancel a household subscription. 
        /// Cancel immediately if within cancellation window and content not already consumed OR if force flag is provided.<br/>
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003,
        /// HouseholdNotExists = 1006, HouseholdSuspended = 1009, InvalidPurchase = 3000, CancelationWindowPeriodExpired = 3001, ContentAlreadyConsumed = 3005
        /// </summary>        
        /// <param name="partner_id">Group identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="sub_id">Subscription identifier</param>        
        ///  <param name="is_force">If 'true', cancels the service regardless of whether the service was used or not</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("{household_id}/subscriptions/{sub_id}"), HttpDelete]
        public bool CancelSubscriptionNow([FromUri] string partner_id, [FromUri] int household_id, [FromUri] int sub_id, [FromUri] bool is_force = false)
        {         
            TransactionType transaction_type = TransactionType.subscription;
            return CancelServiceNow(partner_id, household_id, sub_id, is_force, transaction_type);
        }

        /// <summary>
        /// Immediately cancel a household PPV. 
        /// Cancel immediately if within cancellation window and content not already consumed OR if force flag is provided.<br/>
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003,
        /// HouseholdNotExists = 1006, HouseholdSuspended = 1009, InvalidPurchase = 3000, CancelationWindowPeriodExpired = 3001, ContentAlreadyConsumed = 3005
        /// </summary>        
        /// <param name="partner_id">Group identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="ppv_id">PPV identifier</param>        
        ///  <param name="is_force">If 'true', cancels the service regardless of whether the service was used or not</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("{household_id}/ppvs/{ppv_id}"), HttpDelete]
        public bool CancelPPVNow([FromUri] string partner_id, [FromUri] int household_id, [FromUri] int ppv_id, [FromUri] bool is_force = false)
        {
            TransactionType transaction_type = TransactionType.ppv;
            return CancelServiceNow(partner_id, household_id, ppv_id, is_force, transaction_type);
        }

        /// <summary>
        /// Immediately cancel a household Collection. 
        /// Cancel immediately if within cancellation window and content not already consumed OR if force flag is provided.<br/>
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003,
        /// HouseholdNotExists = 1006, HouseholdSuspended = 1009, InvalidPurchase = 3000, CancelationWindowPeriodExpired = 3001, ContentAlreadyConsumed = 3005
        /// </summary>        
        /// <param name="partner_id">Partner ID</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="collection_id">Collection identifier</param>        
        ///  <param name="is_force">If 'true', cancels the service regardless of whether the service was used or not</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("{household_id}/collections/{collection_id}"), HttpDelete]
        public bool CancelCollectionNow([FromUri] string partner_id, [FromUri] int household_id, [FromUri] int collection_id, [FromUri] bool is_force = false)
        {
            TransactionType transaction_type = TransactionType.collection;
            return CancelServiceNow(partner_id, household_id, collection_id, is_force, transaction_type);
        }

        private static bool CancelServiceNow(string partner_id, int household_id, int asset_id, bool is_force, TransactionType transaction_type)
        {
            bool response = false;
            int groupId;
            if (!int.TryParse(partner_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "partner_id must be int");
            }
            if (household_id == 0 || asset_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "household_id or asset_id not valid");
            }
            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().CancelServiceNow(groupId, household_id, asset_id, transaction_type, is_force);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex, null, new List<int>() { 1006 });
            }

            if (response == false)
            {
                throw new InternalServerErrorException();
            }
            return response;
        }

        /// <summary>
        /// Cancel a household service subscription at the next renewal. The subscription stays valid till the next renewal.<br/>
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003,
        ///  HouseholdNotExists = 1006, HouseholdSuspended = 1009, InvalidPurchase = 3000, SubscriptionNotRenewable = 300
        /// </summary>        
        /// <param name="partner_id">Group identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="sub_id">Subscription Code</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("{household_id}/subscriptions/{sub_id}/renewal"), HttpDelete]
        public bool CancelSubscriptionRenewal([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string sub_id)
        {
            bool response = false;

            int groupId;
            if (!int.TryParse(partner_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "partner_id must be int");
            }
            if (household_id == 0 || string.IsNullOrEmpty(sub_id))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "household_id or subscription code not valid");
            }
            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().CancelSubscriptionRenewal(groupId, household_id, sub_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex, null, new List<int>() { 1006 });
            }

            if (response == false)
            {
                throw new InternalServerErrorException();
            }
            return response;
        }
        #endregion

        /// <summary>
        /// Returns the household model<br/>
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003, 
        /// MasterEmailAlreadyExists = 1004, HouseholdNotExists = 1006, HouseholdUserFailed = 1007
        /// </summary>        
        /// <param name="partner_id">Group identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. Possible values: "users_info"</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("{household_id}"), HttpGet]
        public Household GetHousehold([FromUri] string partner_id, [FromUri] int household_id, [FromUri] List<With> with = null)
        {
            Household response = null;

            int groupId;
            if (!int.TryParse(partner_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "partner_id must be int");
            }
            if (household_id <= 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "household_id not valid");
            }
            try
            {
                // call client
                response = ClientsManager.DomainsClient().GetDomainInfo(groupId, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex, null, new List<int>() { 1006 });
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }


            if (with != null && with.Contains(With.users_info))
            {
                // get users ids list
                List<int> userIds = new List<int>();
                userIds.AddRange(response.UsersIds);
                userIds.AddRange(response.MasterUsersIds);
                userIds.AddRange(response.DefaultUsersIds);
                userIds.AddRange(response.PendingUsersIds);
                
                //get users
                List<User> users = null;
                if (userIds.Count > 0)
                {
                    users = ClientsManager.UsersClient().GetUsersData(groupId, userIds);
                }

                if (users != null)
                {
                    response.Users = Mapper.Map<List<SlimUser>>(users.Where(u => response.UsersIds.Contains((int)u.Id)));
                    response.UsersIds = null;
                    response.MasterUsers = Mapper.Map<List<SlimUser>>(users.Where(u => response.MasterUsersIds.Contains((int)u.Id)));
                    response.MasterUsersIds = null;
                    response.DefaultUsers = Mapper.Map<List<SlimUser>>(users.Where(u => response.DefaultUsersIds.Contains((int)u.Id)));
                    response.DefaultUsersIds = null;
                    response.PendingUsers = Mapper.Map<List<SlimUser>>(users.Where(u => response.PendingUsersIds.Contains((int)u.Id)));
                    response.PendingUsersIds = null;
                }

            }
            return response;
        }

        /// <summary>
        /// Creats a household for the user<br/>
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003, 
        /// User exists in other household = 1018, Household already exists = 1000, Household user failed = 1007
        /// </summary>        
        /// <param name="partner_id">Group identifier</param>
        /// <param name="request">Request parameters</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route(""), HttpPost]
        public Household AddHousehold([FromUri] string partner_id, [FromBody] AddHousehold request)
        {
            Household response = null;

            int groupId;
            if (!int.TryParse(partner_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "partner_id must be int");
            }
            if (string.IsNullOrEmpty(request.MasterUserId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "master_user_id cannot be empty");
            }
            try
            {
                // call client
                response = ClientsManager.DomainsClient().AddDomain(groupId, request.Name, request.Description, request.MasterUserId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex, null, new List<int>() { 1006 });
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }

            return response;
        }
    }
}