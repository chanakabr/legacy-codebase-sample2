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

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class DomainService : Service
    {
        #region Members

        public IDomainRepository _repository { get; set; }  //Injected by IOC

        #endregion

        #region Methods

        #region GET

        public object Get(GetDeviceDomainsRequest request)
        {
            return _repository.GetDeviceDomains(request.InitObj, request.udid);
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
            return _repository.GetDomainInfo(request.InitObj, request.domain_id);
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
            return _repository.GetPINForDevice(request.InitObj, request.dev_brand_id);
        }

        public object Get(GetDomainGroupRulesRequest request)
        {
            return _repository.GetDomainGroupRules(request.InitObj, request.domain_id);
        }

        public object Get(GetDomainsBillingHistoryRequest request)
        {
            return _repository.GetDomainsBillingHistory(request.InitObj, request.domain_ids, request.start_date, request.end_date);
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
            return _repository.GetDomainPermittedItems(request.InitObj, request.domain_id);
        }

        public object Get(GetDomainPermittedSubscriptionsRequest request)
        {
            return _repository.GetDomainPermittedSubscriptions(request.InitObj, request.domain_id);
        }

        #endregion

        #region PUT

        public object Put(ChangeDeviceDomainStatusRequest request)
        {
            return _repository.ChangeDeviceDomainStatus(request.InitObj, request.is_active);
        }

        public object Put(SetDeviceInfoRequest request)
        {
            return _repository.SetDeviceInfo(request.InitObj, request.udid, request.device_name);
        }

        public object Put(SetDomainInfoRequest request)
        {
            return _repository.SetDomainInfo(request.InitObj, request.domain_id, request.domain_name, request.domain_description);
        }

        public object Put(SetRuleStateRequest request)
        {
            return _repository.SetRuleState(request.InitObj, request.rule_id, request.is_active);
        }

        #endregion

        #region POST

        public object Post(AddDeviceToDomainRequest request)
        {
            return _repository.AddDeviceToDomain(request.InitObj, request.device_name, request.device_brand_id);
        }

        public object Post(AddDomainRequest request)
        {
            return _repository.AddDomain(request.InitObj, request.device_name, request.domain_desc, request.master_guid_id);
        }

        public object Post(AddUserToDomainRequest request)
        {
            return _repository.AddUserToDomain(request.InitObj, request.site_guid, request.domain_id);
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
            return _repository.SubmitAddUserToDomainRequest(request.InitObj, request.site_guid, request.master_user_name);
        }

        public object Post(SetDomainGroupRuleRequest request)
        {
            return _repository.SetDomainGroupRule(request.InitObj, request.domain_id, request.rule_id, request.pin, request.is_active);
        }

        public object Post(AddDomainWithCoGuidRequest request)
        {
            return _repository.AddDomainWithCoGuid(request.InitObj, request.domain_name, request.domain_description, request.master_guid, request.co_guid);
        }

        #endregion

        #region DELETE

        public object Delete(RemoveDeviceFromDomainRequest request)
        {
            return _repository.RemoveDeviceFromDomain(request.InitObj, request.domain_id, request.udid, request.device_name, request.device_brand_id);
        }

        public object Delete(RemoveDomainRequest request)
        {
            return _repository.RemoveDomain(request.InitObj, request.domain_id);
        }

        public object Delete(RemoveUserFromDomainRequest request)
        {
            return _repository.RemoveUserFromDomain(request.InitObj, request.site_guid, request.domain_id);
        }

        #endregion

        #endregion
    }
}