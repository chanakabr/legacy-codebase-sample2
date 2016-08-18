using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/household/action")]
    [OldStandardAction("getOldStandard", "get")]
    [OldStandardAction("addOldStandard", "add")]
    [OldStandardAction("updateOldStandard", "update")]
    public class HouseholdController : ApiController
    {
        /// <summary>
        /// Returns the household model       
        /// </summary>
        /// <param name="id">Household identifier</param>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, Household user failed = 1007</remarks>        
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [SchemeArgument("id", RequiresPermission = true)]
        public KalturaHousehold Get(int? id = null)
        {
            var ks = KS.GetFromRequest();
            KalturaHousehold response = null;

            int groupId = KS.GetFromRequest().GroupId;
            if (!id.HasValue)
                id = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                // call client
                response = ClientsManager.DomainsClient().GetDomainInfo(groupId, id.Value);
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
        /// Returns the household model       
        /// </summary>        
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. Possible values: "users_base_info", "users_full_info"</param>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, Household user failed = 1007</remarks>        
        [Route("getOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaHousehold GetOldStandard(List<KalturaHouseholdWithHolder> with = null)
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
                response = ClientsManager.DomainsClient().GetDomainInfo(groupId, user.First().getHouseholdID());

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
        [Obsolete]
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
        /// <param name="household">Household object</param>
        /// <remarks>Possible status codes: 
        /// User exists in other household = 1018, Household user failed = 1007</remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaHousehold Add(KalturaHousehold household)
        {
            KalturaHousehold response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                // call client
                response = ClientsManager.DomainsClient().AddDomain(groupId, household.Name, household.Description, userId, household.ExternalId);
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
        [Route("addOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaHousehold AddOldStandard(string name, string description, string external_id = null)
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
        [Obsolete]
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
        [Obsolete]
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

        /// <summary>
        /// Set user billing payment method identifier (payment method external id), for a specific household and a specific payment gateway
        /// </summary>
        /// <remarks>
        /// Possible status codes:         
        /// Payment gateway not set for household = 6007, Payment gateway not exist = 6008, Payment method not exist = 6049,  Error saving payment gateway household payment method = 6052, 
        /// Payment method already set to household payment gateway = 6054, Payment gateway not support payment method = 6056
        /// </remarks>        
        /// <param name="payment_gateway_id">External identifier for the payment gateway  </param>
        /// <param name="payment_method_name"></param>      
        /// <param name="payment_details"></param>      
        /// <param name="payment_method_external_id"></param>        
        [Route("setPaymentMethodExternalId"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public bool SetPaymentMethodExternalId(string payment_gateway_id, string payment_method_name, string payment_details, string payment_method_external_id)
        {
            bool response = false;

            if (string.IsNullOrEmpty(payment_gateway_id))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "payment_gateway_id cannot be empty");
            }

            if (string.IsNullOrEmpty(payment_method_name))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "payment_method_name cannot be empty");
            }

            if (string.IsNullOrEmpty(payment_method_external_id))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "payment_method_external_id cannot be empty");
            }

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // get domain id      
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // call client
                response = ClientsManager.BillingClient().SetPaymentGatewayHouseholdPaymentMethod(groupId, payment_gateway_id, (int)domainId, payment_method_name, payment_details, payment_method_external_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;

        }

        #endregion

        /// <summary>
        /// Reset a household’s time limitation for removing user or device
        /// </summary>
        /// <remarks>
        /// Possible status codes: 
        /// </remarks>        
        /// <param name="frequencyType">Possible values: devices – reset the device change frequency. 
        /// users – reset the user add/remove frequency</param>        
        [Route("resetFrequency"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [OldStandard("frequencyType", "household_frequency_type")]
        public KalturaHousehold ResetFrequency(KalturaHouseholdFrequencyType frequencyType)
        {
            KalturaHousehold household = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                household = ClientsManager.DomainsClient().ResetFrequency(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), frequencyType);
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
        /// <param name="household">Household object</param>
        /// <remarks></remarks>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public KalturaHousehold Update(KalturaHousehold household)
        {
            int groupId = KS.GetFromRequest().GroupId;

            var householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            // no household to update - return forbidden
            if (householdId == 0)
            {
                throw new UnauthorizedException((int)WebAPI.Managers.Models.StatusCode.ServiceForbidden, "Service Forbidden");
            }

            try
            {
                // call client
                household = ClientsManager.DomainsClient().SetDomainInfo(groupId, (int)householdId, household.Name, household.Description);
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

        /// <summary>
        /// Update the household name and description    
        /// </summary>        
        /// <param name="name">Name for the household</param>
        /// <param name="description">Description for the household</param>
        /// <remarks></remarks>
        [Route("updateOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaHousehold UpdateOldStandard(string name, string description)
        {
            KalturaHousehold household = null;

            int groupId = KS.GetFromRequest().GroupId;

            var householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            // no household to update - return forbidden
            if (householdId == 0)
            {
                throw new UnauthorizedException((int)WebAPI.Managers.Models.StatusCode.ServiceForbidden, "Service Forbidden");
            }

            try
            {
                // call client
                household = ClientsManager.DomainsClient().SetDomainInfo(groupId, (int)householdId, name, description);
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

        /// <summary>
        /// Fully delete a household. Delete all of the household information, including users, devices, transactions and assets.
        /// </summary>
        /// <param name="id">Household identifier</param>
        /// <remarks>Possible status codes: 
        ///</remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int id)
        {
            var ks = KS.GetFromRequest();

            int groupId = KS.GetFromRequest().GroupId;
            
            try
            {
                return ClientsManager.DomainsClient().RemoveDomain(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Fully delete a household per specified internal or external ID. Delete all of the household information, including users, devices, transactions and assets.
        /// </summary>                
        /// <param name="filter">Household ID by which to delete a household. Possible values: internal – internal ID ; external – external ID</param>
        /// <remarks>Possible status codes: 
        ///</remarks>
        [Route("deleteByOperator"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public bool DeleteByOperator(KalturaIdentifierTypeFilter filter)
        {
            var ks = KS.GetFromRequest();

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(filter.Identifier))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "identifier cannot be empty");
            }

            try
            {

                if (filter.By == KalturaIdentifierTypeBy.internal_id)
                {
                    int householdId = 0;
                    if (int.TryParse(filter.Identifier, out householdId))
                    {
                        // call client
                        return ClientsManager.DomainsClient().RemoveDomain(groupId, householdId);
                    }

                }

                else if (filter.By == KalturaIdentifierTypeBy.external_id)
                {
                    // call client
                    return ClientsManager.DomainsClient().RemoveDomain(groupId, filter.Identifier);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Suspend a given household service. Sets the household status to “suspended".The household service settings are maintained for later resume
        /// </summary>                
        /// <remarks>Possible status codes: Domain already suspended = 1012
        ///</remarks>
        [Route("suspend"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public bool Suspend()
        {
            var ks = KS.GetFromRequest();
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);
                // call client
                return ClientsManager.DomainsClient().Suspend(groupId, (int)domainId);

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Resumed a given household service to its previous service settings
        /// </summary>                
        /// <remarks>Possible status codes: 
        /// Domain already active = 1013
        ///</remarks>
        [Route("resume"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public bool Resume()
        {
            var ks = KS.GetFromRequest();
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                var domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);
                // call client
                return ClientsManager.DomainsClient().Resume(groupId, (int)domainId);

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }
    }
}