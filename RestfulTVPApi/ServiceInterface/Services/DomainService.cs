using ServiceStack.PartialResponse.ServiceModel;
using RestfulTVPApi.ServiceModel;
using ServiceStack.Api.Swagger;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{

    #region Objects

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

    [Route("/devices/{udid}/domains", "GET", Summary = "Get the domains to which a device is associated", Notes = "Get a device's domains")]
    public class GetDeviceDomainsRequest : RequestBase, IReturn<IEnumerable<DomainResponseObject>>
    {
        [ApiMember(Name = "udid", Description = "Device id", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
    }

    //move to api?
    [Route("/domains/{co_guid}", "GET", Summary = "Get a specific domain by a coguid", Notes = "Get a domain by coguid")]
    public class GetDomainByCoGuidRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "co_guid", Description = "CoGuid", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string co_guid { get; set; }
    }

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

    [Route("/domains/{domain_id}", "GET", Summary = "Get domain info", Notes = "Get domain info")]
    public class GetDomainInfoRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    //Ofir - /devices/{udid}/domains/{domain_id}/status
    [Route("/devices/{udid}/domains", "PUT", Summary = "Change Device Domain status", Notes = "Change device domain status")]
    public class ChangeDeviceDomainStatusRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "udid", Description = "Device ID", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
        [ApiMember(Name = "is_active", Description = "indicates the new device domain status", ParameterType = "body", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool is_active { get; set; }
    }

    [Route("/domains/{domain_id}/users/{added_user_guid}", "POST", Summary = "Adds a user to a domain", Notes = "Adds a user to a domain")]
    public class AddUserToDomainRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "added_user_guid", Description = "The new user to add to domain", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int added_user_guid { get; set; }
        [ApiMember(Name = "domain_id", Description = "Domain ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    //Move to api?
    [Route("/domains/{operator_co_guid}", "GET", Summary = "Returns all domain IDs belonging to a specific Operator by its co guid", Notes = "Returns all domain IDs belonging to a specific Operator by its co guid")]
    public class GetDomainIDsByOperatorCoGuidRequest : RequestBase, IReturn<IEnumerable<int>>
    {
        [ApiMember(Name = "operator_co_guid", Description = "The operator coguid", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string operator_co_guid { get; set; }
    }

    //Ofir - /devices/{udid}/pin
    [Route("/devices/{dev_brand_id}/pin", "GET", Summary = "generates a PIN code for adding a new device to a domain", Notes = "generates a PIN code for adding a new device to a domain")]
    public class GetPINForDeviceRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "dev_brand_id", Description = "Device brand ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int dev_brand_id { get; set; }
    }

    //Ofir - move to api?
    [Route("/devices/{udid}/pin", "POST", Summary = "registers a new device to a domain from an input PIN code", Notes = "registers a new device to a domain from an input PIN code")]
    public class RegisterDeviceByPINRequest : RequestBase, IReturn<TVPApiModule.Services.ApiDomainsService.DeviceRegistration>
    {
        [ApiMember(Name = "pin", Description = "Pin code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string pin { get; set; }
    }

    [Route("/domains/{domain_id}", "DELETE", Summary = "Remove Domain", Notes = "Remove domain")]
    public class RemoveDomainRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    //Ofir - /domains/{domain_id}/users/{removed_user_guid}
    [Route("/domains/{domain_id}/user/{removed_user_guid}", "DELETE", Summary = "Removes a user from a domain", Notes = "Removes a user from a domain")]
    public class RemoveUserFromDomainRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "removed_user_guid", Description = "The user to remove from domain", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string removed_user_guid { get; set; }
        [ApiMember(Name = "domain_id", Description = "Domain ID", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    //Ofir - /devices/{udid}
    [Route("/devices/{udid}", "PUT", Summary = "Sets device name", Notes = "Sets device name")]
    public class SetDeviceInfoRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "udid", Description = "Device ID", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
        [ApiMember(Name = "device_name", Description = "The new device name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string device_name { get; set; }
    }

    [Route("/domains/{domain_id}", "PUT", Summary = "Sets domain description", Notes = "Sets domain description")]
    public class SetDomainInfoRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
        [ApiMember(Name = "domain_name", Description = "Domain Name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string domain_name { get; set; }
        [ApiMember(Name = "domain_description", Description = "Domain description", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string domain_description { get; set; }
    }

    //Ofir - Should siteguid be a prarm?
    [Route("/domains/{domain_id}/users", "POST", Summary = "A master user request to add another user to the domain", Notes = "Invokes AddUserToDomain")]
    public class SubmitAddUserToDomainRequest : RequestBase, IReturn<DomainResponseObject>
    {
        [ApiMember(Name = "domain_id", Description = "Is active", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
        [ApiMember(Name = "master_user_name", Description = "Master user name", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string master_user_name { get; set; }
    }

    [Route("/domains/{domain_id}/rules", "GET", Summary = "returns the rules for the domain", Notes = "returns the rules for the domain")]
    public class GetDomainGroupRulesRequest : RequestBase, IReturn<GroupRule[]>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    [Route("/domains/{domain_id}/rules", "PUT", Summary = "returns the rules for the domain", Notes = "returns the rules for the domain")]
    public class SetDomainGroupRuleRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
        [ApiMember(Name = "rule_id", Description = "Rule Id", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int rule_id { get; set; }
        [ApiMember(Name = "pin", Description = "pin code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string pin { get; set; }
        [ApiMember(Name = "is_active", Description = "Is active", ParameterType = "body", DataType = SwaggerType.Int, IsRequired = true)]
        public int is_active { get; set; }
    }

    //ofir - /domains/{domain_ids}/billing_history
    [Route("/domains/{domain_ids}/billings/{start_date}/{end_date}", "POST", Summary = "Returns the billing history for an array of domains in a given time range", Notes = "Returns the billing history for an array of domains in a given time range")]
    public class GetDomainsBillingHistoryRequest : RequestBase, IReturn<TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.DomainBillingTransactionsResponse[]>
    {
        [ApiMember(Name = "domain_ids", Description = "Domain ids", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public int[] domain_ids { get; set; }
        [ApiMember(Name = "start_date", Description = "Start date", ParameterType = "path", DataType = SwaggerType.Date, IsRequired = true)]
        public DateTime start_date { get; set; }
        [ApiMember(Name = "end_date", Description = "End date", ParameterType = "path", DataType = SwaggerType.Date, IsRequired = true)]
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

    //Move to api?
    [Route("/domains/{co_guid}", "GET", Summary = "Returns a domain ID using a 3rd party Co-GUID", Notes = "used when a device has an existing 3rd party association and identification number outside of the Tvinci system")]
    public class GetDomainIDByCoGuidRequest : RequestBase, IReturn<int>
    {
        [ApiMember(Name = "co_guid", Description = "Domain Master Co-GUID", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string co_guid { get; set; }
    }

    [Route("/domains/{domain_id}/medias/permitted", "GET", Summary = "Gets all the items permitted for the users in a given domain", Notes = "Gets all the items permitted for the users in a given domain")]
    public class GetDomainPermittedItemsRequest : RequestBase, IReturn<PermittedMediaContainer[]>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    [Route("/domains/{domain_id}/subscriptions/permitted", "GET", Summary = "Gets all subscriptions permitted to the users in a given domain", Notes = "Gets all subscriptions permitted to the users in a given domain.")]
    public class GetDomainPermittedSubscriptionsRequest : RequestBase, IReturn<PermittedSubscriptionContainer[]>
    {
        [ApiMember(Name = "domain_id", Description = "Domain Id", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int domain_id { get; set; }
    }

    #endregion

    //[RequiresAuthentication]
    [RequiresInitializationObject]
    public class DomainService : Service
    {
        #region Members

        public IDomainRepository _repository { get; set; }  //Injected by IOC

        #endregion

        #region HTTPMethods

        #region Post

        public HttpResult Post(AddDeviceToDomainRequest request)
        {
            var response = _repository.AddDeviceToDomain(request.InitObj, request.device_name, request.device_brand_id);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Post(AddDomainRequest request)
        {
            var response = _repository.AddDomain(request.InitObj, request.device_name, request.domain_desc, request.master_guid_id);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Post(AddUserToDomainRequest request)
        {
            var response = _repository.AddUserToDomain(request.InitObj, request.added_user_guid, request.domain_id);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Post(RegisterDeviceByPINRequest request)
        {
            var response = _repository.RegisterDeviceByPIN(request.InitObj, request.pin);

            if ((Nullable<TVPApiModule.Services.ApiDomainsService.DeviceRegistration>)response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Post(SubmitAddUserToDomainRequest request)
        {
            var response = _repository.SubmitAddUserToDomainRequest(request.InitObj, request.master_user_name);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Post(SetDomainGroupRuleRequest request)
        {
            var response = _repository.SetDomainGroupRule(request.InitObj, request.domain_id, request.rule_id, request.pin, request.is_active);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Post(AddDomainWithCoGuidRequest request)
        {
            var response = _repository.AddDomainWithCoGuid(request.InitObj, request.domain_name, request.domain_description, request.master_guid, request.co_guid);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        #endregion

        #region GET

        public HttpResult Get(GetDeviceDomainsRequest request)
        {
            var response = _repository.GetDeviceDomains(request.InitObj, request.udid);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetDomainByCoGuidRequest request)
        {
            var response = _repository.GetDomainByCoGuid(request.InitObj, request.co_guid);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetDomainInfoRequest request)
        {
            var response = _repository.GetDomainInfo(request.InitObj, request.domain_id);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetDomainIDsByOperatorCoGuidRequest request)
        {
            var response = _repository.GetDomainIDsByOperatorCoGuid(request.InitObj, request.operator_co_guid);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetPINForDeviceRequest request)
        {
            var response = _repository.GetPINForDevice(request.InitObj, request.dev_brand_id);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetDomainGroupRulesRequest request)
        {
            var response = _repository.GetDomainGroupRules(request.InitObj, request.domain_id);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetDomainsBillingHistoryRequest request)
        {
            var response = _repository.GetDomainsBillingHistory(request.InitObj, request.domain_ids, request.start_date, request.end_date);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetDomainIDByCoGuidRequest request)
        {
            var response = _repository.GetDomainIDByCoGuid(request.InitObj, request.co_guid);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetDomainPermittedItemsRequest request)
        {
            var response = _repository.GetDomainPermittedItems(request.InitObj, request.domain_id);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetDomainPermittedSubscriptionsRequest request)
        {
            var response = _repository.GetDomainPermittedSubscriptions(request.InitObj, request.domain_id);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        #endregion

        #region Put

        public HttpResult Put(ChangeDeviceDomainStatusRequest request)
        {
            var response = _repository.ChangeDeviceDomainStatus(request.InitObj, request.is_active);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Put(SetDeviceInfoRequest request)
        {
            var response = _repository.SetDeviceInfo(request.InitObj, request.udid, request.device_name);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Put(SetDomainInfoRequest request)
        {
            var response = _repository.SetDomainInfo(request.InitObj, request.domain_id, request.domain_name, request.domain_description);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.ToDto();

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        #endregion

        #region Delete

        public HttpResult Delete(RemoveDeviceFromDomainRequest request)
        {
            var response = _repository.RemoveDeviceFromDomain(request.InitObj, request.domain_id, request.udid, request.device_name, request.device_brand_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Delete(RemoveDomainRequest request)
        {
            var response = _repository.RemoveDomain(request.InitObj, request.domain_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Delete(RemoveUserFromDomainRequest request)
        {
            var response = _repository.RemoveUserFromDomain(request.InitObj, request.removed_user_guid, request.domain_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        #endregion

        #endregion
    }
}