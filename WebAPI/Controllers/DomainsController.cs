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
    [RoutePrefix("domains")]
    public class DomainsController : ApiController
    {
        /// <summary>
        /// Return the parental rules that applies to the domain. Can include rules that have been associated in account or domain
        /// </summary>
        /// <param name="domain_id">Domain Identifier</param>
        /// <param name="group_id">Partner identifier</param>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <returns>List of parental rules applied to the domain</returns>
        [Route("{domain_id}/parental/rules"), HttpGet]
        public List<ParentalRule> GetParentalRules([FromUri] string group_id, [FromUri] int domain_id)
        {
            List<ParentalRule> response = null;

            // parameters validation
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (domain_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "user_id cannot be empty");
            }

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetDomainParentalRules(groupId, domain_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Enabled a parental rule for a specific domain
        /// </summary>
        /// <param name="domain_id">Domain Identifier</param>
        /// <param name="rule_id">Rule Identifier</param>
        /// <param name="group_id">Partner identifier</param>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <returns>Success or failure and reason</returns>
        [Action("{domain_id}/parental/rules/{rule_id}")]
        public bool EnableParentalRule([FromUri] string group_id, [FromUri] int domain_id, [FromUri] long rule_id)
        {
            bool success = false;

            // parameters validation
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (domain_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "domain_id cannot be empty");
            }

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetDomainParentalRules(groupId, domain_id, rule_id, 1);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Disables a parental rule for a specific Domain
        /// </summary>
        /// <param name="domain_id">Domain Identifier</param>
        /// <param name="rule_id">Rule Identifier</param>
        /// <param name="group_id">Partner identifier</param>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <returns>Success or failure and reason</returns>
        [Route("{domain_id}/parental/rules/{rule_id}"), HttpDelete]
        public bool DisableParentalRule([FromUri] string group_id, [FromUri] int domain_id, [FromUri] long rule_id)
        {
            bool success = false;

            // parameters validation
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }


            if (domain_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "domain_id cannot be empty");
            }

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetDomainParentalRules(groupId, domain_id, rule_id, 0);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Retrieve the parental PIN that applies for the Domain.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner identifier</param>
        /// <param name="domain_id">Domain identifier</param>
        /// <returns>The PIN that applies for the domain</returns>
        [Route("{domain_id}/parental/pin/"), HttpGet]
        public PinResponse GetParentalPIN([FromUri] string group_id, [FromUri] int domain_id)
        {
            PinResponse pinResponse = null;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (domain_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "domain_id cannot be empty");
            }

            try
            {
                // call client
                pinResponse = ClientsManager.ApiClient().GetDomainParentalPIN(groupId, domain_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return pinResponse;
        }

        /// <summary>
        /// Set the parental PIN that applies for the Domain.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="domain_id">Domain identifier</param>
        /// <param name="pin">New PIN to set</param>
        /// <returns>Success / Fail</returns>
        [Action("{domain_id}/parental/pin")]
        public bool SetParentalPIN([FromUri] string group_id, [FromUri] int domain_id, [FromUri] string pin)
        {
            bool success = false;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (domain_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "domain_id cannot be empty");
            }

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetDomainParentalRules(groupId, domain_id, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Retrieve the purchase settings that applies for the domain.
        /// Possible status codes:
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="domain_id">Domain identifier</param>
        /// <returns>The purchase settings that apply for the user</returns>
        [Route("{domain_id}/purchase/settings"), HttpGet]
        public PurchaseSettingsResponse GetPurchaseSettings([FromUri] string group_id, [FromUri] int domain_id)
        {
            PurchaseSettingsResponse purchaseResponse = null;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (domain_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "domain_id cannot be empty");
            }

            try
            {
                // call client
                purchaseResponse = ClientsManager.ApiClient().GetDomainPurchaseSettings(groupId, domain_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return purchaseResponse;
        }

        /// <summary>
        /// Set the purchase settings that applies for the Domain.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="domain_id">Domain identifier</param>
        /// <param name="setting">New settings to apply</param>
        /// <returns>Success / Fail</returns>
        [Action("{domain_id}/purchase/settings/")]
        public bool SetPurchaseSettings([FromUri] string group_id, [FromUri] int domain_id, [FromUri] int setting)
        {
            bool success = false;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (domain_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "domain_id cannot be empty");
            }

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetDomainPurchaseSettings(groupId, domain_id, setting);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }

        /// <summary>
        /// Retrieve the purchase PIN that applies for the Domain.
        /// Possible status codes: 5001 = No PIN defined
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner identifier</param>
        /// <param name="domain_id">Domain identifier</param>
        /// <returns>The PIN that applies for the domain</returns>
        [Route("{domain_id}/purchase/pin/"), HttpGet]
        public PinResponse GetPurchasePIN([FromUri] string group_id, [FromUri] int domain_id)
        {
            PinResponse pinResponse = null;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (domain_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "domain_id cannot be empty");
            }

            try
            {
                // call client
                var response = ClientsManager.ApiClient().GetDomainPurchasePIN(groupId, domain_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return pinResponse;
        }

        /// <summary>
        /// Set the purchase PIN that applies for the domain.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="domain_id">Domain identifier</param>
        /// <param name="pin">New PIN to apply</param>
        /// <returns>Success / Fail</returns>
        [Action("{domain_id}/purchase/pin")]
        public bool SetPurchasePIN([FromUri] string group_id, [FromUri] int domain_id, [FromUri] string pin)
        {
            bool success = false;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (domain_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "domain_id cannot be empty");
            }

            try
            {
                // call client
                success = ClientsManager.ApiClient().SetDomainPurchasePIN(groupId, domain_id, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return success;
        }




        #region ConditionalAccess
        /// <summary>
        /// Immediately cancel a household service. Cancel immediately if within cancellation window and content not already consumed OR if force flag is provided.<br/>
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003,
        /// DomainNotExists = 1006, DomainSuspended = 1009, InvalidPurchase = 3000, CancelationWindowPeriodExpired = 3001, ContentAlreadyConsumed = 3005
        /// </summary>        
        /// <param name="group_id">Group ID</param>
        /// <param name="domain_id">Domain Id</param>
        /// <param name="asset_id">Asset Id</param>
        /// <param name="transaction_type">TransactionType Enum</param>
        ///  <param name="bIsForce"Bbool parameter</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("{domain_id}/subscriptions/{sub_id}"), HttpDelete]
        public bool CancelServiceNow([FromUri] string group_id, [FromUri] int domain_id, [FromUri] int asset_id, [FromUri] TransactionType transaction_type, [FromUri] bool is_force = false)
        {
            bool response = false;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be int");
            }
            if (domain_id == 0 || asset_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "domain_id or asset_id not valid");
            }
            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().CancelServiceNow(groupId, domain_id, asset_id, transaction_type, is_force);
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
        /// Cancel a household service subscription at the next renewal. The subscription stays valid till the next renewal.<br/>
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003,
        ///  DomainNotExists = 1006, DomainSuspended = 1009, InvalidPurchase = 3000, SubscriptionNotRenewable = 300
        /// </summary>        
        /// <param name="group_id">Group ID</param>
        /// <param name="domain_id">Domain Id</param>
        /// <param name="subscription_code">Subscription Code</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("{domain_id}/subscriptions/{sub_id}/renewal"), HttpDelete]
        public bool CancelSubscriptionRenewal([FromUri] string group_id, [FromUri] int domain_id, [FromUri] string subscription_code)
        {
            bool response = false;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be int");
            }
            if (domain_id == 0 || string.IsNullOrEmpty(subscription_code))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "domain_id or subscription code not valid");
            }
            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().CancelSubscriptionRenewal(groupId, domain_id, subscription_code);
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
        #endregion

        /// <summary>
        /// Returns the Domain model<br/>
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003, 
        /// DomainAlreadyExists = 1000, ExceededLimit = 1001, DeviceTypeNotAllowed = 1002, DeviceNotInDomin = 1003, MasterEmailAlreadyExists = 1004, UserNotInDomain = 1005, DomainNotExists = 1006, 
        /// HouseholdUserFailed = 1007, UserExistsInOtherDomains = 1018
        /// </summary>        
        /// <param name="group_id">Group ID</param>
        /// <param name="domain_id">Domain Id</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("{domain_id}"), HttpGet]
        public Domain GetDomain([FromUri] string group_id, [FromUri] int domain_id, [FromUri] List<With> with)
        {
            Domain response = null;

            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be int");
            }
            if (domain_id <= 0)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "domain_id not valid");
            }
            try
            {
                // call client
                response = ClientsManager.DomainsClient().GetDomainInfo(groupId, domain_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }


            if (with != null && with.Contains(With.users))
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

    }
}