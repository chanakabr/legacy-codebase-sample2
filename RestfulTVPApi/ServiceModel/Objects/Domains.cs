using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApiModule.Objects;
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

    //TODO: CONTINUEEEEEEEEE
    //[Route("/domains/{domain_id}", "GET", Summary = "Confirmation with token invoked by domain master to approve adding pending device to the domain", Notes = "Usually called from landing page set in the email that was sent to the master")]
    //public class ConfirmDeviceByDomainMaster : RequestBase, IReturn<DomainResponseObject>
    //{
    //    [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
    //    public int domain_id { get; set; }
    //}

    ////Move to api?
    //[Route("/domains/{operator_co_guid}", "GET", Summary = "Returns all domain IDs belonging to a specific Operator by its co guid", Notes = "Returns all domain IDs belonging to a specific Operator by its co guid")]
    //public class GetDomainIDsByOperatorCoGuidRequest : RequestBase, IReturn<List<int>>
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
    public class GetDeviceDomainsRequest : RequestBase, IReturn<List<DomainResponseObject>>
    {
        [ApiMember(Name = "udid", Description = "Device id", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
    }

    [Route("/devices/{id}/info", "GET", Summary = "Get device info by device id or udid", Notes = "")]
    public class GetDeviceInfoRequest : RequestBase, IReturn<DeviceResponseObject>
    {
        [ApiMember(Name = "id", Description = "Device id or device udid", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string id { get; set; }
        [ApiMember(Name = "is_udid", Description = "Boolean flag which defines if device id or udid is sent. False - Device Id is sent. True - UDID is sent", ParameterType = "query", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool is_udid { get; set; }
    }

    [Route("/domains/{domain_id}/rules", "GET", Summary = "returns the rules for the domain", Notes = "returns the rules for the domain")]
    public class GetDomainGroupRulesRequest : RequestBase, IReturn<List<GroupRule>>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    [Route("/domains/{domain_id}/medias/permitted", "GET", Summary = "Gets all the items permitted for the users in a given domain", Notes = "Gets all the items permitted for the users in a given domain")]
    public class GetDomainPermittedItemsRequest : RequestBase, IReturn<List<PermittedMediaContainer>>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    [Route("/domains/{domain_id}/subscriptions/permitted", "GET", Summary = "Gets all subscriptions permitted to the users in a given domain", Notes = "Gets all subscriptions permitted to the users in a given domain.")]
    public class GetDomainPermittedSubscriptionsRequest : RequestBase, IReturn<List<PermittedSubscriptionContainer>>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    [Route("/domains/{domain_id}/homenetwork", "GET", Summary = "Get all home networks related to a domain", Notes = "")]
    public class GetDomainHomeNetworksRequest : RequestBase, IReturn<HomeNetwork>
    {        
        [ApiMember(Name = "domain_id", Description = "Domain ID", ParameterType = "path", DataType = SwaggerType.Long, IsRequired = true)]
        public long domain_id { get; set; }
    }

    [Route("/domains/{domain_id}/collections/permitted", "GET", Summary = "Gets all collections permitted to the domain", Notes = "")]
    public class GetDomainPermittedCollectionsRequest : RequestBase, IReturn<List<PermittedCollectionContainer>>
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
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
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
    public class GetDomainsBillingHistoryRequest : RequestBase, IReturn<List<DomainBillingTransactionsResponse>>
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

    [Route("/devices/{udid}/domains", "POST", Summary = "If domain is device-restricted (2, 3) and user is not master, method will send email to domain master to approve adding the device to domain. If domain is not device-restricted or the user is master, method will directly add the device to domain.URI")]
    public class SubmitAddDeviceToDomainRequest : RequestBase, IReturn<DomainResponseObject>
    {        
        [ApiMember(Name = "udid", Description = "Device Id", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
        [ApiMember(Name = "site_guid", Description = "User Ideftifier", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int site_guid { get; set; }
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
        [ApiMember(Name = "device_name", Description = "Device Name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string device_name { get; set; }
        [ApiMember(Name = "brand_id", Description = "Brand Id", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int brand_id { get; set; }
    }

    [Route("/domains/{domain_id}/homenetwork/{network_id}", "POST", Summary = "Adds a home network to a domain", Notes = "")]
    public class AddHomeNetworkToDomainRequest : RequestBase, IReturn<NetworkResponseObject>
    {
        [ApiMember(Name = "network_id", Description = "The network's Id", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string network_id { get; set; }
        [ApiMember(Name = "domain_id", Description = "Domain ID", ParameterType = "path", DataType = SwaggerType.Long, IsRequired = true)]
        public long domain_id { get; set; }
        [ApiMember(Name = "network_name", Description = "The network's name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string network_name { get; set; }
        [ApiMember(Name = "network_description", Description = "The network's description", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string network_description { get; set; }
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

    [Route("/domains/{domain_id}/restriction/{restriction}", "PUT", Summary = "Sets a restriction on a given domain, usually from Self-Care", Notes = "(requiring master's approval on adding users/devices)")]
    public class SetDomainRestrictionRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
        [ApiMember(Name = "restriction", Description = "Restriction number", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int restriction { get; set; }        
    }

    [Route("/domains/{domain_id}/homenetwork/{network_id}", "PUT", Summary = "Updates the domain home network's values", Notes = "")]
    public class UpdateDomainHomeNetworkRequest : RequestBase, IReturn<NetworkResponseObject>
    {
        [ApiMember(Name = "network_id", Description = "The network's Id", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string network_id { get; set; }
        [ApiMember(Name = "domain_id", Description = "Domain ID", ParameterType = "path", DataType = SwaggerType.Long, IsRequired = true)]
        public long domain_id { get; set; }
        [ApiMember(Name = "network_name", Description = "The network's name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string network_name { get; set; }
        [ApiMember(Name = "network_description", Description = "The network's description", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string network_description { get; set; }
        [ApiMember(Name = "is_active", Description = "The network's state (true - active, false - inactive", ParameterType = "body", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool is_active { get; set; }
    }

    [Route("/domains/{domain_id}/currmaster/{current_master_id}/newmaster/{new_master_id}", "PUT", Summary = "Changes the domain master", Notes = "")]
    public class ChangeDomainMasterRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
        [ApiMember(Name = "current_master_id", Description = "Current master id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int current_master_id { get; set; }
        [ApiMember(Name = "new_master_id", Description = "New master id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int new_master_id { get; set; }
    }

    [Route("/domains/{domain_id}/frequency/{frequency_type}", "PUT", Summary = "Resets the domain's frequency", Notes = "")]
    public class ResetDomainFrequencyRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
        [ApiMember(Name = "frequency_type", Description = "Frequency type", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int frequency_type { get; set; }        
    }

    #endregion

    #region DELETE

    [Route("/domains/{domain_id}/devices", "DELETE", Summary = "Remove User Favorite", Notes = "Remove User Favorite")]
    public class RemoveDeviceFromDomainRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
        [ApiMember(Name = "udid", Description = "Device ud_id", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }        
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

    [Route("/domains/{domain_id}/homenetwork/{network_id}", "DELETE", Summary = "Removes a home network from a domain by given networkId", Notes = "")]
    public class RemoveDomainHomeNetworkRequest : RequestBase, IReturn<NetworkResponseObject>
    {
        [ApiMember(Name = "network_id", Description = "The network's Id", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string network_id { get; set; }
        [ApiMember(Name = "domain_id", Description = "Domain ID", ParameterType = "path", DataType = SwaggerType.Long, IsRequired = true)]
        public long domain_id { get; set; }        
    }

    #endregion
}