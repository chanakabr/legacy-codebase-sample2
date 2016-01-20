using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
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
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. Possible values: "users_base_info", "users_full_info"</param>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, Household user failed = 1007</remarks>        
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaHousehold Get(List<KalturaHouseholdWithHolder> with = null)
        {
            var ks = KS.GetFromRequest();
            KalturaHousehold response = null;

            int groupId = KS.GetFromRequest().GroupId;

            var user = ClientsManager.UsersClient().GetUsersData(groupId, new List<string>() { ks.UserId });

            if (with == null)
                with = new List<KalturaHouseholdWithHolder>();

            try
            {
                // call client
                response = ClientsManager.DomainsClient().GetDomainInfo(groupId, user.First().HouseholdID);

                if (with != null && with.Where(x => x.type == KalturaHouseholdWith.users_base_info || x.type == KalturaHouseholdWith.users_full_info).Count() > 0)
                {
                    ClientsManager.DomainsClient().EnrichHouseHold(with, response, groupId);
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
        /// Retrieve household information according to internal or external ID      
        /// </summary>        
        /// <param name="filter">Specify how to retrieve the household. Possible values: internal – internal ID ; external – external ID</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. Possible values: "users_base_info", "users_full_info"</param>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, Household user failed = 1007</remarks>                
        [Route("getByOperator"), HttpPost]
        [ApiAuthorize]
        public KalturaHousehold GetByOperator(KalturaIdentifierTypeFilter filter, List<KalturaHouseholdWithHolder> with = null)
        {
            var ks = KS.GetFromRequest();
            KalturaHousehold response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(filter.Identifier))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "identifier cannot be empty");
            }

            if (with == null)
                with = new List<KalturaHouseholdWithHolder>();

            try
            {
                if (filter.By == KalturaIdentifierTypeBy.internal_id)
                {
                    int householdId = 0;
                    if (int.TryParse(filter.Identifier, out householdId))
                    {
                        // call client
                        response = ClientsManager.DomainsClient().GetDomainInfo(groupId, householdId);
                    }

                }

                else if (filter.By == KalturaIdentifierTypeBy.external_id)
                {
                    // call client
                    response = ClientsManager.DomainsClient().GetDomainByCoGuid(groupId, filter.Identifier);
                }

                if (with != null && with.Where(x => x.type == KalturaHouseholdWith.users_base_info || x.type == KalturaHouseholdWith.users_full_info).Count() > 0)
                {
                    ClientsManager.DomainsClient().EnrichHouseHold(with, response, groupId);
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
        /// <param name="external_id">Unique external ID to identify the household</param>
        /// <remarks>Possible status codes: 
        /// User exists in other household = 1018, Household user failed = 1007</remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaHousehold Add(string name, string description, string external_id = null)
        {
            KalturaHousehold response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                // call client
                response = ClientsManager.DomainsClient().AddDomain(groupId, name, description, userId, external_id);
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
        /// Payment gateway not exist = 6008, Payment gateway charge id required = 6009, External identifier required = 6016, Error saving payment gateway household = 6017, 
        /// Charge id already set to household payment gateway = 6025
        /// </remarks>        
        /// <param name="pg_id">External identifier for the payment gateway  </param>
        /// <param name="charge_id">The billing user account identifier for this household at the given payment gateway</param>        
        [Route("setChargeID"), HttpPost]
        [ApiAuthorize]
        public bool SetChargeID(string pg_id, string charge_id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // get domain id      
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                response = ClientsManager.BillingClient().SetHouseholdChargeID(groupId, pg_id, (int)domainId, charge_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;

        }

        /// <summary>
        /// Get a household’s billing account identifier (charge ID) for a given payment gateway
        /// </summary>
        /// <remarks>
        /// Possible status codes: Payment gateway not exist for group = 6008, External identifier is required = 6016, Charge id not set to household = 6026
        /// </remarks>        
        /// <param name="pg_id">External identifier for the payment gateway  </param>        
        [Route("getChargeID"), HttpPost]
        [ApiAuthorize]
        public string GetChargeID(string pg_id)
        {
            string chargeId = string.Empty;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // get domain id       
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                chargeId = ClientsManager.BillingClient().GetHouseholdChargeID(groupId, pg_id, (int)domainId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return chargeId;
        }

        #endregion

        /// <summary>
        /// Reset a household’s business limitations -device change frequency or the user add/remove frequency
        /// </summary>
        /// <remarks>
        /// Possible status codes: 
        /// </remarks>        
        /// <param name="household_frequency_type">Possible values: devices – reset the device change frequency. 
        /// users – reset the user add/remove frequency</param>        
        [Route("resetFrequency"), HttpPost]
        [ApiAuthorize]
        public KalturaHousehold ResetFrequency(KalturaHouseholdFrequencyType household_frequency_type)
        {
            KalturaHousehold household = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                household = ClientsManager.DomainsClient().ResetFrequency(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), household_frequency_type);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return household;
        }

        /// <summary>
        /// Update the household name and description    
        /// </summary>        
        /// <param name="name">Name for the household</param>
        /// <param name="description">Description for the household</param>
        /// <remarks></remarks>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public KalturaHousehold Update(string name, string description)
        {
            KalturaHousehold household = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                household = ClientsManager.DomainsClient().SetDomainInfo(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), name, description);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (household == null)
            {
                throw new InternalServerErrorException();
            }

            return household;
        }
    }
}