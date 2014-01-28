using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceModel
{
    #region GET

    ////move to api?
    //[Route("/domains/{co_guid}", "GET", Summary = "Get a specific domain by a coguid", Notes = "Get a domain by coguid")]
    //public class GetDomainByCoGuidRequest : RequestBase, IReturn<DomainResponseObject>
    //{
    //    [ApiMember(Name = "co_guid", Description = "CoGuid", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
    //    public string co_guid { get; set; }
    //}

    [Route("/domains/{domain_id}", "GET", Summary = "Get domain info", Notes = "Get domain info")]
    public class GetDomainInfoRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    ////Move to api?
    //[Route("/domains/{operator_co_guid}", "GET", Summary = "Returns all domain IDs belonging to a specific Operator by its co guid", Notes = "Returns all domain IDs belonging to a specific Operator by its co guid")]
    //public class GetDomainIDsByOperatorCoGuidRequest : RequestBase, IReturn<IEnumerable<int>>
    //{
    //    [ApiMember(Name = "operator_co_guid", Description = "The operator coguid", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
    //    public string operator_co_guid { get; set; }
    //}

    //Ofir - /devices/{udid}/pin
    [Route("/devices/{udid}/pin", "GET", Summary = "generates a PIN code for adding a new device to a domain", Notes = "generates a PIN code for adding a new device to a domain")]
    public class GetPINForDeviceRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "dev_brand_id", Description = "Device brand ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int dev_brand_id { get; set; }
    }

    [Route("/devices/{udid}/domains", "GET", Summary = "Get the domains to which a device is associated", Notes = "Get a device's domains")]
    public class GetDeviceDomainsRequest : RequestBase, IReturn<IEnumerable<DomainResponseObject>>
    {
        [ApiMember(Name = "udid", Description = "Device id", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
    }

    [Route("/domains/{domain_id}/rules", "GET", Summary = "returns the rules for the domain", Notes = "returns the rules for the domain")]
    public class GetDomainGroupRulesRequest : RequestBase, IReturn<IEnumerable<GroupRule>>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    [Route("/domains/{domain_id}/medias/permitted", "GET", Summary = "Gets all the items permitted for the users in a given domain", Notes = "Gets all the items permitted for the users in a given domain")]
    public class GetDomainPermittedItemsRequest : RequestBase, IReturn<IEnumerable<PermittedMediaContainer>>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    [Route("/domains/{domain_id}/subscriptions/permitted", "GET", Summary = "Gets all subscriptions permitted to the users in a given domain", Notes = "Gets all subscriptions permitted to the users in a given domain.")]
    public class GetDomainPermittedSubscriptionsRequest : RequestBase, IReturn<IEnumerable<PermittedSubscriptionContainer>>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    #endregion

    ////Move to api?
    //[Route("/domains/{co_guid}", "GET", Summary = "Returns a domain ID using a 3rd party Co-GUID", Notes = "used when a device has an existing 3rd party association and identification number outside of the Tvinci system")]
    //public class GetDomainIDByCoGuidRequest : RequestBase, IReturn<int>
    //{
    //    [ApiMember(Name = "co_guid", Description = "Domain Master Co-GUID", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
    //    public string co_guid { get; set; }
    //}

    #region POST

    [Route("/domains/{domain_id}/devices", "POST", Summary = "Add Device To Domain", Notes = "Add device")]
    public class AddDeviceToDomainRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string domain_id { get; set; }
        [ApiMember(Name = "device_name", Description = "Device Name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string device_name { get; set; }
        [ApiMember(Name = "device_brand_id", Description = "Device Brand Id", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int device_brand_id { get; set; }
    }

    [Route("/domains", "POST", Summary = "Add Domain", Notes = "Add domain to master site user")]
    public class AddDomainRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "domain_name", Description = "Domain Name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string device_name { get; set; }
        [ApiMember(Name = "domain_desc", Description = "Domain desc", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string domain_desc { get; set; }
        [ApiMember(Name = "master_guid_id", Description = "Master Guid Id", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int master_guid_id { get; set; }
    }

    //Ofir - Should siteguid be a prarm?
    [Route("/domains/{domain_id}/users", "POST", Summary = "A master user request to add another user to the domain", Notes = "Invokes AddUserToDomain")]
    public class SubmitAddUserToDomainRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "domain_id", Description = "Is active", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "master_user_name", Description = "Master user name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string master_user_name { get; set; }
    }

    [Route("/domains/{domain_id}/users/{site_guid}", "POST", Summary = "Adds a user to a domain", Notes = "Adds a user to a domain")]
    public class AddUserToDomainRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "site_guid", Description = "The new user to add to domain", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "domain_id", Description = "Domain ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    ////Ofir - move to api?
    //[Route("/devices/{udid}/pin", "POST", Summary = "registers a new device to a domain from an input PIN code", Notes = "registers a new device to a domain from an input PIN code")]
    //public class RegisterDeviceByPINRequest : RequestBase, IReturn<TVPApiModule.Services.ApiDomainsService.DeviceRegistration>
    //{
    //    [ApiMember(Name = "udid", Description = "Device ID", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
    //    public string udid { get; set; }
    //    [ApiMember(Name = "pin", Description = "Pin code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
    //    public string pin { get; set; }
    //}

    [Route("/domains/{domain_ids}/billing_history", "POST", Summary = "Returns the billing history for an array of domains in a given time range", Notes = "Returns the billing history for an array of domains in a given time range")]
    public class GetDomainsBillingHistoryRequest : RequestBase, IReturn<IEnumerable<DomainBillingTransactionsResponse>>
    {
        [ApiMember(Name = "domain_ids", Description = "Domain ids", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public int[] domain_ids { get; set; }
        [ApiMember(Name = "start_date", Description = "Start date", ParameterType = "body", DataType = SwaggerType.Date, IsRequired = true)]
        public DateTime start_date { get; set; }
        [ApiMember(Name = "end_date", Description = "End date", ParameterType = "body", DataType = SwaggerType.Date, IsRequired = true)]
        public DateTime end_date { get; set; }
    }

    //Ofir - merge with adddomain, do co_guid as optional 
    [Route("/domains", "POST", Summary = "Adds a new domain by Co-GUID", Notes = "used when a device has an existing 3rd party association and identification number outside of the Tvinci system")]
    public class AddDomainWithCoGuidRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "domain_name", Description = "Domain name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string domain_name { get; set; }
        [ApiMember(Name = "domain_description", Description = "Domain description", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string domain_description { get; set; }
        [ApiMember(Name = "master_guid", Description = "Domain master user id", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int master_guid { get; set; }
        [ApiMember(Name = "co_guid", Description = "Domain master co guid", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string co_guid { get; set; }
    }

    #endregion

    #region PUT

    [Route("/devices/{udid}/domains", "PUT", Summary = "Change Device Domain status", Notes = "Change device domain status")]
    public class ChangeDeviceDomainStatusRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "udid", Description = "Device ID", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
        [ApiMember(Name = "is_active", Description = "indicates the new device domain status", ParameterType = "body", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool is_active { get; set; }
    }

    [Route("/devices/{udid}", "PUT", Summary = "Sets device name", Notes = "Sets device name")]
    public class SetDeviceInfoRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "udid", Description = "Device ID", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
        [ApiMember(Name = "device_name", Description = "The new device name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string device_name { get; set; }
    }

    [Route("/domains/{domain_id}", "PUT", Summary = "Sets domain description", Notes = "Sets domain description")]
    public class SetDomainInfoRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
        [ApiMember(Name = "domain_name", Description = "Domain Name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string domain_name { get; set; }
        [ApiMember(Name = "domain_description", Description = "Domain description", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string domain_description { get; set; }
    }

    [Route("/domains/{domain_id}/rules/{rule_id}", "PUT", Summary = "returns the rules for the domain", Notes = "returns the rules for the domain")]
    public class SetDomainGroupRuleRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
        [ApiMember(Name = "rule_id", Description = "Rule Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int rule_id { get; set; }
        [ApiMember(Name = "pin", Description = "pin code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string pin { get; set; }
        [ApiMember(Name = "is_active", Description = "Is active", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int is_active { get; set; }
    }
    
    //Gilad: Method description might be changed - check
    [Route("/domains/{domain_id}/rules/{rule_id}/state", "PUT", Summary = "This method sets a rule state for a domain", Notes = "This method sets a rule state for a domain")]
    public class SetRuleStateRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
        [ApiMember(Name = "rule_id", Description = "Rule Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int rule_id { get; set; }        
        [ApiMember(Name = "is_active", Description = "Is active", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int is_active { get; set; }
    }

    #endregion

    #region DELETE

    [Route("/domains/{domain_id}/devices/{udid}", "DELETE", Summary = "Remove User Favorite", Notes = "Remove User Favorite")]
    public class RemoveDeviceFromDomainRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
        [ApiMember(Name = "udid", Description = "Device Id", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
        [ApiMember(Name = "device_name", Description = "Device Name", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string device_name { get; set; }
        [ApiMember(Name = "device_brand_id", Description = "Device brand Id", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int device_brand_id { get; set; }
    }

    [Route("/domains/{domain_id}", "DELETE", Summary = "Remove Domain", Notes = "Remove domain")]
    public class RemoveDomainRequest : RequestBase, IReturn<DomainResponseStatus>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    [Route("/domains/{domain_id}/users/{site_guid}", "DELETE", Summary = "Removes a user from a domain", Notes = "Removes a user from a domain")]
    public class RemoveUserFromDomainRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "site_guid", Description = "The user to remove from domain", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "domain_id", Description = "Domain ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    #endregion
}