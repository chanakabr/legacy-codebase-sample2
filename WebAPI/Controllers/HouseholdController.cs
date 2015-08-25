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
        /// <summary>
        /// Returns the household model       
        /// </summary>        
        /// <param name="household_id">Household identifier</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. Possible values: "users_info"</param>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, Household user failed = 1007</remarks>        
        [ApiAuthorize(AllowAnonymous: false)]
        [Route("get"), HttpPost]
        public KalturaHousehold Get(int household_id, List<KalturaHouseholdWithHolder> with = null)
        {
            var ks = KS.GetFromRequest();
            KalturaHousehold response = null;

            int groupId = KS.GetFromRequest().GroupId;

            var user = ClientsManager.UsersClient().GetUsersData(groupId, new List<string>() { ks.UserId });

            if (user.First().HouseholdID != household_id)
                throw new ForbiddenException((int)WebAPI.Managers.Models.StatusCode.ServiceForbidden, "Households mismatch");

            if (with == null)
                with = new List<KalturaHouseholdWithHolder>();

            try
            {
                // call client
                response = ClientsManager.DomainsClient().GetDomainInfo(groupId, user.First().HouseholdID);

                if (with != null && with.Where(x => x.type == KalturaHouseholdWith.users_base_info || x.type == KalturaHouseholdWith.users_full_info).Count() > 0)
                {
                    // get users ids lists
                    var userIds = response.Users != null ? response.Users.Select(u => u.Id) : new List<string>();
                    var masterUserIds = response.MasterUsers != null ? response.MasterUsers.Select(u => u.Id) : new List<string>();
                    var defaultUserIds = response.DefaultUsers != null ? response.DefaultUsers.Select(u => u.Id) : new List<string>();
                    var pendingUserIds = response.PendingUsers != null ? response.PendingUsers.Select(u => u.Id) : new List<string>();

                    // merge all user ids to one list
                    List<string> allUserIds = new List<string>();
                    allUserIds.AddRange(userIds);
                    allUserIds.AddRange(masterUserIds);
                    allUserIds.AddRange(defaultUserIds);
                    allUserIds.AddRange(pendingUserIds);

                    //get users
                    List<KalturaOTTUser> users = null;
                    if (allUserIds.Count > 0)
                    {
                        users = ClientsManager.UsersClient().GetUsersData(groupId, allUserIds);
                    }

                    if (users != null)
                    {
                        if (with.Where(x => x.type == KalturaHouseholdWith.users_base_info).FirstOrDefault() != null)
                        {
                            response.Users = Mapper.Map<List<KalturaBaseOTTUser>>(users.Where(u => userIds.Contains(u.Id)));
                            response.MasterUsers = Mapper.Map<List<KalturaBaseOTTUser>>(users.Where(u => masterUserIds.Contains(u.Id)));
                            response.DefaultUsers = Mapper.Map<List<KalturaBaseOTTUser>>(users.Where(u => defaultUserIds.Contains(u.Id)));
                            response.PendingUsers = Mapper.Map<List<KalturaBaseOTTUser>>(users.Where(u => pendingUserIds.Contains(u.Id)));
                        }
                        if (with.Where(x => x.type == KalturaHouseholdWith.users_full_info).FirstOrDefault() != null)
                        {
                            response.Users = Mapper.Map<List<KalturaOTTUser>>(users.Where(u => userIds.Contains(u.Id))).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                            response.MasterUsers = Mapper.Map<List<KalturaOTTUser>>(users.Where(u => masterUserIds.Contains(u.Id))).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                            response.DefaultUsers = Mapper.Map<List<KalturaOTTUser>>(users.Where(u => defaultUserIds.Contains(u.Id))).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                            response.PendingUsers = Mapper.Map<List<KalturaOTTUser>>(users.Where(u => pendingUserIds.Contains(u.Id))).Select(usr => (KalturaBaseOTTUser)usr).ToList();
                        }
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
        /// <param name="name">Name for the household</param>
        /// <param name="description">Description for the household</param>
        /// <param name="master_user_id">Identifier of the user that will become the master of the created household</param>
        /// <remarks>Possible status codes: 
        /// User exists in other household = 1018, Household already exists = 1000, Household user failed = 1007</remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaHousehold Add(string name, string description, string master_user_id)
        {
            KalturaHousehold response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(master_user_id))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "master_user_id cannot be empty");
            }
            try
            {
                // call client
                response = ClientsManager.DomainsClient().AddDomain(groupId, name, description, master_user_id);
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
        /// Possible status codes:       
        ///   
        /// Domain not exists = 1006, Payment gateway not exist = 6008, Payment gateway charge id required = 6009, External idntifier required = 6016, Error saving paymentgateway household = 6017, 
        /// Charge id already set to household payment gateway = 6025
        /// </remarks>        
        /// <param name="id">External identifier for the payment gateway  </param>
        /// <param name="household_id">Household for which to return the Charge ID</param>        
        /// <param name="charge_id">The billing user account identifier for this household at the given payment gateway</param>        
        [Route("setChargeID"), HttpPost]
        [ApiAuthorize]
        public bool SetChargeID(string id, int household_id, string charge_id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

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
        /// Possible status codes:       
        ///   
        /// Domain not exists = 1006, Payment gateway not exist for group = 6008, External idntifier is required = 6016, Charge id not set to household = 6026
        /// </remarks>        
        /// <param name="id">External identifier for the payment gateway  </param>
        /// <param name="household_id">Household for which to return the Charge ID</param>        
        [Route("getChargeID"), HttpPost]
        [ApiAuthorize]
        public string GetChargeID(string id, int household_id)
        {
            string chargeId = string.Empty;

            int groupId = KS.GetFromRequest().GroupId;

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