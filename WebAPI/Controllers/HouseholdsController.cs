using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.API;
using WebAPI.Models.Billing;
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
        /// </summary>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="partner_id">Partner identifier</param>
        /// <returns>List of parental rules applied to the household</returns>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, Household does not exist = 1006</remarks>
        [Route("{household_id}/parental/rules"), HttpGet]
        public ParentalRulesList GetParentalRules([FromUri] string partner_id, [FromUri] int household_id)
        {
            List<ParentalRule> response = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetDomainParentalRules(groupId, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new ParentalRulesList() { ParentalRules = response };
        }

        /// <summary>
        /// Enabled a parental rule for a specific household.
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006, Invalid rule = 5003</remarks>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="rule_id">Rule Identifier</param>
        /// <param name="partner_id">Partner identifier</param>
        /// <returns>Success or failure and reason</returns>
        [Route("{household_id}/parental/rules/{rule_id}"), HttpPost]
        public bool EnableParentalRule([FromUri] string partner_id, [FromUri] int household_id, [FromUri] long rule_id)
        {
            bool success = false;

            int groupId = int.Parse(partner_id);

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
        /// </summary>        
        /// <param name="household_id">Household Identifier</param>
        /// <param name="rule_id">Rule Identifier</param>
        /// <param name="partner_id">Partner identifier</param>
        /// <returns>Success or failure and reason</returns>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008 
        /// Household does not exist = 1006, Invalid rule = 5003</remarks>
        [Route("{household_id}/parental/rules/{rule_id}"), HttpDelete]
        public bool DisableParentalRule([FromUri] string partner_id, [FromUri] int household_id, [FromUri] long rule_id)
        {
            bool success = false;

            int groupId = int.Parse(partner_id);

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
        /// Retrieve the parental PIN that applies for the household.
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006</remarks>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <returns>The PIN that applies for the household</returns>
        [Route("{household_id}/parental/pin"), HttpGet]
        public PinResponse GetParentalPIN([FromUri] string partner_id, [FromUri] int household_id)
        {
            PinResponse pinResponse = null;

            int groupId = int.Parse(partner_id);

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
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="pin">New PIN to set</param>
        /// <returns>Success / Fail</returns>
        [Route("{household_id}/parental/pin"), HttpPost]
        public bool SetParentalPIN([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string pin)
        {
            bool success = false;

            int groupId = int.Parse(partner_id);

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
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006
        /// </remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <returns>The purchase settings that apply for the user</returns>
        [Route("{household_id}/purchase/settings"), HttpGet]
        public PurchaseSettingsResponse GetPurchaseSettings([FromUri] string partner_id, [FromUri] int household_id)
        {
            PurchaseSettingsResponse purchaseResponse = null;

            int groupId = int.Parse(partner_id);

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
        /// </summary>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006</remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="setting">New settings to apply</param>
        /// <returns>Success / Fail</returns>
        [Route("{household_id}/purchase/settings"), HttpPost]
        public bool SetPurchaseSettings([FromUri] string partner_id, [FromUri] int household_id, [FromUri] int setting)
        {
            bool success = false;

            int groupId = int.Parse(partner_id);

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
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// 5001 = No PIN defined, Household does not exist = 1006
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <returns>The PIN that applies for the household</returns>
        [Route("{household_id}/purchase/pin"), HttpGet]
        public PinResponse GetPurchasePIN([FromUri] string partner_id, [FromUri] int household_id)
        {
            PinResponse pinResponse = null;

            int groupId = int.Parse(partner_id);

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
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006
        /// </remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="pin">New PIN to apply</param>
        /// <returns>Success / Fail</returns>
        [Route("{household_id}/purchase/pin"), HttpPost]
        public bool SetPurchasePIN([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string pin)
        {
            bool success = false;

            int groupId = int.Parse(partner_id);

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

        /// <summary>
        /// Disables the partner's default rule for this household        
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006
        /// </remarks>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <returns>Success / fail</returns>
        [Route("{household_id}/parental/rules/default"), HttpDelete]
        public bool DisableDefaultParentalRule([FromUri] string partner_id, [FromUri] int household_id)
        {
            bool success = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                success = ClientsManager.ApiClient().DisableDomainDefaultParentalRule(groupId, household_id);
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
        /// Cancel immediately if within cancellation window and content not already consumed OR if force flag is provided.        
        /// </summary>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="sub_id">Subscription identifier</param>        
        ///  <param name="is_force">If 'true', cancels the service regardless of whether the service was used or not</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006, Household suspended = 1009, Invalid purchase = 3000, Cancellation window period expired = 3001, Content already consumed = 3005</remarks>
        [Route("{household_id}/subscriptions/{sub_id}"), HttpDelete]
        public bool CancelSubscriptionNow([FromUri] string partner_id, [FromUri] int household_id, [FromUri] int sub_id, [FromUri] bool is_force = false)
        {
            TransactionType transaction_type = TransactionType.subscription;
            return CancelServiceNow(partner_id, household_id, sub_id, is_force, transaction_type);
        }

        /// <summary>
        /// Immediately cancel a household PPV. 
        /// Cancel immediately if within cancellation window and content not already consumed OR if force flag is provided.        
        /// </summary>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="ppv_id">PPV identifier</param>        
        /// <param name="is_force">If 'true', cancels the service regardless of whether the service was used or not</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006, Household suspended = 1009, Invalid purchase = 3000, Cancellation window period expired = 3001, Content already consumed = 3005</remarks>
        [Route("{household_id}/ppvs/{ppv_id}"), HttpDelete]
        public bool CancelPPVNow([FromUri] string partner_id, [FromUri] int household_id, [FromUri] int ppv_id, [FromUri] bool is_force = false)
        {
            TransactionType transaction_type = TransactionType.ppv;
            return CancelServiceNow(partner_id, household_id, ppv_id, is_force, transaction_type);
        }

        /// <summary>
        /// Immediately cancel a household Collection. 
        /// Cancel immediately if within cancellation window and content not already consumed OR if force flag is provided.       
        /// </summary>        
        /// <param name="partner_id">Partner ID</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="collection_id">Collection identifier</param>        
        /// <param name="is_force">If 'true', cancels the service regardless of whether the service was used or not</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        /// Household does not exist = 1006, Household suspended = 1009, Invalid purchase = 3000, Cancellation window period expired = 3001, Content already consumed = 3005</remarks>
        [Route("{household_id}/collections/{collection_id}"), HttpDelete]
        public bool CancelCollectionNow([FromUri] string partner_id, [FromUri] int household_id, [FromUri] int collection_id, [FromUri] bool is_force = false)
        {
            TransactionType transaction_type = TransactionType.collection;
            return CancelServiceNow(partner_id, household_id, collection_id, is_force, transaction_type);
        }

        private static bool CancelServiceNow(string partner_id, int household_id, int asset_id, bool is_force, TransactionType transaction_type)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            if (asset_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_id not valid");
            }
            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().CancelServiceNow(groupId, household_id, asset_id, transaction_type, is_force);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == false)
            {
                throw new InternalServerErrorException();
            }
            return response;
        }

        /// <summary>
        /// Cancel a household service subscription at the next renewal. The subscription stays valid till the next renewal.        
        /// </summary>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="sub_id">Subscription Code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008,
        ///  Household does not exist = 1006, Household suspended = 1009, Invalid purchase = 3000, SubscriptionNotRenewable = 300</remarks>
        [Route("{household_id}/subscriptions/{sub_id}/renewal"), HttpDelete]
        public void CancelSubscriptionRenewal([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string sub_id)
        {
            int groupId = int.Parse(partner_id);

            if (string.IsNullOrEmpty(sub_id))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "subscription code not valid");
            }
            try
            {
                // call client
                ClientsManager.ConditionalAccessClient().CancelSubscriptionRenewal(groupId, household_id, sub_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }
        #endregion

        /// <summary>
        /// Returns the household model       
        /// </summary>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. Possible values: "users_info"</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// Household does not exist = 1006, Household user failed = 1007</remarks>
        [Route("{household_id}"), HttpGet]
        public Household GetHousehold([FromUri] string partner_id, [FromUri] int household_id, [FromUri] List<With> with = null)
        {
            Household response = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.DomainsClient().GetDomainInfo(groupId, household_id);

                if (with != null && with.Contains(With.users_info))
                {
                    // get users ids lists
                    var userIds = response.Users != null ? response.Users.Select(u => u.Id) : new List<int>();
                    var masterUserIds = response.MasterUsers != null ? response.MasterUsers.Select(u => u.Id) : new List<int>();
                    var defaultUserIds = response.DefaultUsers != null ? response.DefaultUsers.Select(u => u.Id) : new List<int>();
                    var pendingUserIds = response.PendingUsers != null ? response.PendingUsers.Select(u => u.Id) : new List<int>();

                    // marge all user ids to one list
                    List<int> allUserIds = new List<int>();
                    allUserIds.AddRange(userIds);
                    allUserIds.AddRange(masterUserIds);
                    allUserIds.AddRange(defaultUserIds);
                    allUserIds.AddRange(pendingUserIds);

                    //get users
                    List<User> users = null;
                    if (allUserIds.Count > 0)
                    {
                        users = ClientsManager.UsersClient().GetUsersData(groupId, allUserIds);
                    }

                    if (users != null)
                    {
                        response.Users = Mapper.Map<List<SlimUser>>(users.Where(u => userIds.Contains((int)u.Id)));
                        response.MasterUsers = Mapper.Map<List<SlimUser>>(users.Where(u => masterUserIds.Contains((int)u.Id)));
                        response.DefaultUsers = Mapper.Map<List<SlimUser>>(users.Where(u => defaultUserIds.Contains((int)u.Id)));
                        response.PendingUsers = Mapper.Map<List<SlimUser>>(users.Where(u => pendingUserIds.Contains((int)u.Id)));
                    }
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }

            return response;
        }

        /// <summary>
        /// Creates a household for the user      
        /// </summary>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="request">Request parameters</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// User exists in other household = 1018, Household already exists = 1000, Household user failed = 1007</remarks>
        [Route(""), HttpPost]
        public Household AddHousehold([FromUri] string partner_id, [FromBody] AddHousehold request)
        {
            Household response = null;

            int groupId = int.Parse(partner_id);

            if (string.IsNullOrEmpty(request.MasterUserId))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "master_user_id cannot be empty");
            }
            try
            {
                // call client
                response = ClientsManager.DomainsClient().AddDomain(groupId, request.Name, request.Description, request.MasterUserId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }

            return response;
        }

        /// <summary>
        /// Removes a user from household   
        /// </summary>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// Household does not exists = 1006, Limitation period = 1014, User not exists in household = 1020, Invalid user = 1026, 
        /// Household suspended = 1009, No users in household = 1017, User not allowed = 1027</remarks>
        [Route("{household_id}/users/{user_id}"), HttpDelete]
        public bool RemoveUserFromHousehold([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string user_id)
        {
            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                return ClientsManager.DomainsClient().RemoveUserFromDomain(groupId, household_id, user_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Adds a user to household       
        /// </summary>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="master_user_id">Identifier of household master</param>
        /// <param name="is_master">True if the new user should be set to be master</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// Household suspended = 1009, No users in household = 1017, Action user not master = 1021, User Already In household = 1029
        /// </remarks>
        [Route("{household_id}/users/{user_id}"), HttpPost]
        public bool AddUserToHousehold([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string user_id, [FromUri] string master_user_id, [FromUri] bool is_master = false)
        {
            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                return ClientsManager.DomainsClient().AddUserToDomain(groupId, household_id, user_id, master_user_id, is_master);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Removes a device from household
        /// </summary>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="udid">device UDID</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// Household suspended = 1009, No users in household = 1017, Action user not master = 1021</remarks>
        [Route("{household_id}/devices/{udid}"), HttpDelete]
        public bool RemoveDeviceFromHousehold([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string udid)
        {
            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                return ClientsManager.DomainsClient().RemoveDeviceFromDomain(groupId, household_id, udid);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Registers a device to a household using pin code    
        /// </summary>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="device_name">Device name</param>
        /// <param name="pin">Pin code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// Exceeded limit = 1001, Duplicate pin = 1028, Device not exists = 1019</remarks>
        [Route("{household_id}/devices/pin"), HttpPost]
        public Device RegisterDeviceByPin([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string device_name, [FromUri] string pin)
        {
            Device device = null;

            if (string.IsNullOrEmpty(pin))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "pin cannot be empty");
            }

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                device = ClientsManager.DomainsClient().RegisterDeviceByPin(groupId, household_id, device_name, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return device;
        }

        #region payment gateway
        /// <summary>
        /// Returns payment gateway for household
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008, UserDoesNotExist = 2000, UserNotInDomain = 1005, UserWithNoDomain = 2024, UserSuspended = 2001, DomainNotExists = 1006
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="household_id">House Hold Identifier</param>
        /// <param name="user_id">User Identifier</param>
        [Route("{household_id}/payment_gateways/get"), HttpGet]
        public Models.Billing.PaymentGWResponse GetPaymentGW([FromUri] string partner_id, [FromUri] string household_id, [FromUri] string user_id)
        {
            Models.Billing.PaymentGWResponse response = null;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().GetHouseHoldPaymentGW(groupId, user_id, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete payment gateway from household
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008, UserDoesNotExist = 2000, UserNotInDomain = 1005, UserWithNoDomain = 2024, UserSuspended = 2001, DomainNotExists = 1006
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param>
        /// <param name="household_id">House Hold Identifier</param>
        /// <param name="user_id">User Identifier</param>
        [Route("{household_id}/payment_gateways/delete"), HttpPost]
        public bool DeletePaymentGWHouseHold([FromUri] string partner_id, [FromUri] int payment_gateway_id, [FromUri] string household_id, [FromUri] string user_id)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().DeletePaymentGWHouseHold(groupId, payment_gateway_id, user_id, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new payment gateway for household
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008, UserDoesNotExist = 2000, UserNotInDomain = 1005, UserWithNoDomain = 2024, UserSuspended = 2001, DomainNotExists = 1006
        /// </remarks>
        /// <param name="partner_id">Partner identifier</param>    
        /// <param name="payment_gateway_id">Payment Gateway Identifier</param> 
        /// <param name="household_id">House Hold Identifier</param>
        /// <param name="user_id">User Identifier</param>
        /// <param name="charge_id">The billing user account identifier for this household at the given payment gateway</param>
        [Route("{household_id}/payment_gateways/add"), HttpPost]
        public bool InsertPaymentGWHouseHold([FromUri] string partner_id, [FromUri] int payment_gateway_id, [FromUri] string household_id, [FromUri] string user_id, [FromUri] string charge_id)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().InsertPaymentGWHouseHold(groupId, payment_gateway_id, user_id, household_id, charge_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Set user billing account identifier (charge ID), for a specific household and a specific payment gateway
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008, UserDoesNotExist = 2000, UserNotInDomain = 1005, UserWithNoDomain = 2024, UserSuspended = 2001, DomainNotExists = 1006
        /// </remarks>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="id">External identifier for the payment gateway  </param>
        /// <param name="household_id">Household Identifier</param>
        /// <param name="user_id">User Identifier</param>
        /// <param name="charge_id">The billing user account identifier for this household at the given payment gateway</param>        
        [Route("{household_id}/payment_gateways/{*id}"), HttpPost]
        public bool SetChargeID([FromUri] string partner_id, [FromUri] string id, [FromUri] string household_id, [FromUri] string charge_id)
        {
            bool response = false;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().SetHouseholdChargeID(groupId, id, household_id, charge_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;

        }


        /// <summary>
        /// Get a household’s billing account identifier (charge ID) in a given payment gateway 
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008, UserDoesNotExist = 2000, UserNotInDomain = 1005, UserWithNoDomain = 2024, UserSuspended = 2001, DomainNotExists = 1006, PaymentGateWayNotExistForHH = 6007, PaymentGateWayNotExistForGroup = 6008
        /// </remarks>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="id">External identifier for the payment gateway  </param>
        /// <param name="household_id">Household Identifier</param>        
        [Route("{household_id}/payment_gateways/{*id}"), HttpGet]
        public Models.Billing.PaymentGWHouseholdResponse GetChargeID([FromUri] string partner_id, [FromUri] string id, [FromUri] string household_id)
        {
            Models.Billing.PaymentGWHouseholdResponse response = null;


            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                response = ClientsManager.BillingClient().GetHouseholdChargeID(groupId, id, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


        #endregion
    }
}