using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("domains")]
    public class DomainsController : ApiController
    {
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
        [Route("{domain_id}/parental_rules/{rule_id}"), HttpPost]
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
                var response = ClientsManager.ApiClient().SetDomainParentalRules(groupId, domain_id, rule_id, 1);
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
        [Route("{domain_id}/parental_rules/{rule_id}"), HttpDelete]
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
                var response = ClientsManager.ApiClient().SetDomainParentalRules(groupId, domain_id, rule_id, 0);
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
        [Route("{domain_id}/parental_pin/"), HttpGet]
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
                var response = ClientsManager.ApiClient().GetDomainParentalPIN(groupId, domain_id);
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
        [Route("{domain_id}/parental_pin/"), HttpPost]
        public bool SetParentalPIN([FromUri] string group_id, [FromUri] int domain_id, string pin)
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
        /// Set the purchase settings that applies for the Domain.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="domain_id">Domain identifier</param>
        /// <param name="setting">New settings to apply</param>
        /// <returns>Success / Fail</returns>
        [Route("{domain_id}/purchase_settings/"), HttpPost]
        public bool SetPurchaseSettings([FromUri] string group_id, [FromUri] int domain_id, int setting)
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
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner identifier</param>
        /// <param name="domain_id">Domain identifier</param>
        /// <returns>The PIN that applies for the domain</returns>
        [Route("{domain_id}/purchase_pin/"), HttpGet]
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
        /// Set the purchase PIN that applies for the Domain.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        /// <param name="group_id">Partner Identifier</param>
        /// <param name="domain_id">Domain identifier</param>
        /// <param name="pin">New PIN to apply</param>
        /// <returns>Success / Fail</returns>
        [Route("{domain_id}/purchase_pin/"), HttpPost]
        public bool SetPurchaseSettings([FromUri] string group_id, [FromUri] int domain_id, string pin)
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
        [Route("{domain_id}/purchase_pin/"), HttpPost]
        public bool SetPurchasePIN([FromUri] string group_id, [FromUri] int domain_id, string pin)
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
    }
}