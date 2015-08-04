using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.Billing;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/household/action")]
    public class HouseholdController : ApiController
    {
        #region Parental and Purchase rules

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
        [Route("disableDefaultParentalRule"), HttpPost]
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

        /// <summary>
        /// Returns the household model       
        /// </summary>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. Possible values: "users_info"</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// Household does not exist = 1006, Household user failed = 1007</remarks>        
        [ApiAuthorize(AllowAnonymous: false)]
        [Route("get"), HttpPost]
        public KalturaHousehold Get(string partner_id, int household_id, List<KalturaHouseholdWith> with = null)
        {
            var ks = KS.GetFromRequest();
            KalturaHousehold response = null;

            int groupId = int.Parse(partner_id);

            var user = ClientsManager.UsersClient().GetUsersData(groupId, new int[] { ks.UserId }.ToList<int>());

            if (user.First().HouseholdID != household_id)
                throw new ForbiddenException((int)WebAPI.Managers.Models.StatusCode.ServiceForbidden, "Households mismatch");

            try
            {
                // call client
                response = ClientsManager.DomainsClient().GetDomainInfo(groupId, user.First().HouseholdID);

                if (with != null && with.Contains(KalturaHouseholdWith.users_info))
                {
                    // get users ids lists
                    var userIds = response.Users != null ? response.Users.Select(u => u.Id) : new List<int>();
                    var masterUserIds = response.MasterUsers != null ? response.MasterUsers.Select(u => u.Id) : new List<int>();
                    var defaultUserIds = response.DefaultUsers != null ? response.DefaultUsers.Select(u => u.Id) : new List<int>();
                    var pendingUserIds = response.PendingUsers != null ? response.PendingUsers.Select(u => u.Id) : new List<int>();

                    // merge all user ids to one list
                    List<int> allUserIds = new List<int>();
                    allUserIds.AddRange(userIds);
                    allUserIds.AddRange(masterUserIds);
                    allUserIds.AddRange(defaultUserIds);
                    allUserIds.AddRange(pendingUserIds);

                    //get users
                    List<KalturaUser> users = null;
                    if (allUserIds.Count > 0)
                    {
                        users = ClientsManager.UsersClient().GetUsersData(groupId, allUserIds);
                    }

                    if (users != null)
                    {
                        response.Users = Mapper.Map<List<KalturaSlimUser>>(users.Where(u => userIds.Contains((int)u.Id)));
                        response.MasterUsers = Mapper.Map<List<KalturaSlimUser>>(users.Where(u => masterUserIds.Contains((int)u.Id)));
                        response.DefaultUsers = Mapper.Map<List<KalturaSlimUser>>(users.Where(u => defaultUserIds.Contains((int)u.Id)));
                        response.PendingUsers = Mapper.Map<List<KalturaSlimUser>>(users.Where(u => pendingUserIds.Contains((int)u.Id)));
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
        [Route("add"), HttpPost]
        public KalturaHousehold Add([FromUri] string partner_id, [FromBody] KalturaAddHouseholdRequest request)
        {
            KalturaHousehold response = null;

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

        #region payment gateway

        /// <summary>
        /// Set user billing account identifier (charge ID), for a specific household and a specific payment gateway
        /// </summary>
        /// <remarks>
        /// Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, 
        /// Not found = 500007, Partner is invalid = 500008, UserDoesNotExist = 2000, UserNotInDomain = 1005, UserWithNoDomain = 2024, UserSuspended = 2001, DomainNotExists = 1006, 
        /// Payment Gateway Not Exist = 6008, Payment Gateway Charge Id Missing = 6009, External Idntifier Missing = 6016, Error Saving PaymentGateway Household = 6017,
        /// Charge Id Already Set To Household Payment Gateway = 6025
        /// </remarks>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="id">External identifier for the payment gateway  </param>
        /// <param name="household_id">Household Identifier</param>        
        /// <param name="charge_id">The billing user account identifier for this household at the given payment gateway</param>        
        [Route("setChargeID"), HttpPost]
        public bool SetChargeID([FromUri] string partner_id, [FromUri] string id, [FromUri] int household_id, [FromUri] string charge_id)
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
        /// Not found = 500007, Partner is invalid = 500008, UserDoesNotExist = 2000, UserNotInDomain = 1005, UserWithNoDomain = 2024, UserSuspended = 2001, DomainNotExists = 1006,
        /// Payment Gateway Not Exist For Household = 6007, Payment GateWay Not Exist For Group = 6008, External Idntifier Missing  = 6016, Charge Id Not Set To Household = 6024
        /// </remarks>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="id">External identifier for the payment gateway  </param>
        /// <param name="household_id">Household Identifier</param>        
        [Route("getChargeID"), HttpPost]
        public string GetChargeID([FromUri] string partner_id, [FromUri] string id, [FromUri] string household_id)
        {
            string chargeId = string.Empty;

            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                chargeId = ClientsManager.BillingClient().GetHouseholdChargeID(groupId, id, household_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return chargeId;
        }

        #endregion
    }
}