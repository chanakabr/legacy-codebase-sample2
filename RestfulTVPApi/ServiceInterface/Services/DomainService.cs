using System.Net;
using ServiceStack.ServiceInterface;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using RestfulTVPApi.ServiceModel;
using System;
using System.Linq;
using ServiceStack.ServiceHost;

namespace RestfulTVPApi.ServiceInterface
{

    //[RequiresAuthentication]
    [RequiresInitializationObject]
    public class DomainService : Service
    {
        #region Members

        public IDomainRepository _repository { get; set; }  //Injected by IOC

        #endregion

        #region HTTPMethods

        #region POST

        public object Post(AddDeviceToDomainRequest request)
        {
            var response = _repository.AddDeviceToDomain(request.InitObj, request.device_name, request.device_brand_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Post(AddDomainRequest request)
        {
            var response = _repository.AddDomain(request.InitObj, request.device_name, request.domain_desc, request.master_guid_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Post(AddUserToDomainRequest request)
        {
            var response = _repository.AddUserToDomain(request.InitObj, request.site_guid, request.domain_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        //public object Post(RegisterDeviceByPINRequest request)
        //{
        //    var response = _repository.RegisterDeviceByPIN(request.InitObj, request.pin);

        //    if ((Nullable<TVPApiModule.Services.ApiDomainsService.DeviceRegistration>)response == null)
        //    {
        //        return new HttpResult(HttpStatusCode.InternalServerError);
        //    }

        //    var responseDTO = response.ToDto();

        //    return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        //}

        public object Post(SubmitAddUserToDomainRequest request)
        {
            var response = _repository.SubmitAddUserToDomainRequest(request.InitObj, request.site_guid, request.master_user_name);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Post(SetDomainGroupRuleRequest request)
        {
            var response = _repository.SetDomainGroupRule(request.InitObj, request.domain_id, request.rule_id, request.pin, request.is_active);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public object Post(AddDomainWithCoGuidRequest request)
        {
            var response = _repository.AddDomainWithCoGuid(request.InitObj, request.domain_name, request.domain_description, request.master_guid, request.co_guid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        #endregion

        #region GET

        public object Get(GetDeviceDomainsRequest request)
        {
            var response = _repository.GetDeviceDomains(request.InitObj, request.udid);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return base.RequestContext.ToPartialResponse(response);
        }

        //public object Get(GetDomainByCoGuidRequest request)
        //{
        //    var response = _repository.GetDomainByCoGuid(request.InitObj, request.co_guid);

        //    if (response == null)
        //    {
        //        return new HttpResult(HttpStatusCode.InternalServerError);
        //    }

        //    var responseDTO = response.ToDto();

        //    return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        //}

        public object Get(GetDomainInfoRequest request)
        {
            var response = _repository.GetDomainInfo(request.InitObj, request.domain_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        //public object Get(GetDomainIDsByOperatorCoGuidRequest request)
        //{
        //    var response = _repository.GetDomainIDsByOperatorCoGuid(request.InitObj, request.operator_co_guid);

        //    if (response == null)
        //    {
        //        return new HttpResult(HttpStatusCode.InternalServerError);
        //    }

        //    var responseDTO = response.ToDto();

        //    return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        //}

        public object Get(GetPINForDeviceRequest request)
        {
            var response = _repository.GetPINForDevice(request.InitObj, request.dev_brand_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public object Get(GetDomainGroupRulesRequest request)
        {
            var response = _repository.GetDomainGroupRules(request.InitObj, request.domain_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Get(GetDomainsBillingHistoryRequest request)
        {
            var response = _repository.GetDomainsBillingHistory(request.InitObj, request.domain_ids, request.start_date, request.end_date);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        //public object Get(GetDomainIDByCoGuidRequest request)
        //{
        //    var response = _repository.GetDomainIDByCoGuid(request.InitObj, request.co_guid);

        //    if (response == null)
        //    {
        //        return new HttpResult(HttpStatusCode.InternalServerError);
        //    }

        //    var responseDTO = response.ToDto();

        //    return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        //}

        public object Get(GetDomainPermittedItemsRequest request)
        {
            var response = _repository.GetDomainPermittedItems(request.InitObj, request.domain_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Get(GetDomainPermittedSubscriptionsRequest request)
        {
            var response = _repository.GetDomainPermittedSubscriptions(request.InitObj, request.domain_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        #endregion

        #region PUT

        public object Put(ChangeDeviceDomainStatusRequest request)
        {
            var response = _repository.ChangeDeviceDomainStatus(request.InitObj, request.is_active);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Put(SetDeviceInfoRequest request)
        {
            var response = _repository.SetDeviceInfo(request.InitObj, request.udid, request.device_name);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public object Put(SetDomainInfoRequest request)
        {
            var response = _repository.SetDomainInfo(request.InitObj, request.domain_id, request.domain_name, request.domain_description);

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Put(SetRuleStateRequest request)
        {
            var response = _repository.SetRuleState(request.InitObj,request.rule_id, request.is_active);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        #endregion

        #region DELETE

        public object Delete(RemoveDeviceFromDomainRequest request)
        {
            var response = _repository.RemoveDeviceFromDomain(request.InitObj, request.domain_id, request.udid, request.device_name, request.device_brand_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public object Delete(RemoveDomainRequest request)
        {
            var response = _repository.RemoveDomain(request.InitObj, request.domain_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public object Delete(RemoveUserFromDomainRequest request)
        {
            var response = _repository.RemoveUserFromDomain(request.InitObj, request.site_guid, request.domain_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }

        #endregion

        #endregion
    }
}