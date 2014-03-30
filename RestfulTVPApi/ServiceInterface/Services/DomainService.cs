using ServiceStack.ServiceInterface;
using RestfulTVPApi.ServiceModel;

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
            return _repository.GetDeviceDomains(request);
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
            return _repository.GetDomainInfo(request);
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
            return _repository.GetPINForDevice(request);
        }

        public object Get(GetDomainGroupRulesRequest request)
        {
            return _repository.GetDomainGroupRules(request);
        }

        public object Get(GetDomainsBillingHistoryRequest request)
        {
            return _repository.GetDomainsBillingHistory(request);
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
            return _repository.GetDomainPermittedItems(request);
        }

        public object Get(GetDomainPermittedSubscriptionsRequest request)
        {
            return _repository.GetDomainPermittedSubscriptions(request);
        }

        #endregion

        #region PUT

        public object Put(ChangeDeviceDomainStatusRequest request)
        {
            return _repository.ChangeDeviceDomainStatus(request);
        }

        public object Put(SetDeviceInfoRequest request)
        {
            return _repository.SetDeviceInfo(request);
        }

        public object Put(SetDomainInfoRequest request)
        {
            return _repository.SetDomainInfo(request);
        }

        public object Put(SetRuleStateRequest request)
        {
            return _repository.SetRuleState(request);
        }

        #endregion

        #region POST

        public object Post(AddDeviceToDomainRequest request)
        {
            return _repository.AddDeviceToDomain(request);
        }

        public object Post(AddDomainRequest request)
        {
            return _repository.AddDomain(request);
        }

        public object Post(AddUserToDomainRequest request)
        {
            return _repository.AddUserToDomain(request);
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
            return _repository.SubmitAddUserToDomainRequest(request);
        }

        public object Post(SetDomainGroupRuleRequest request)
        {
            return _repository.SetDomainGroupRule(request);
        }

        public object Post(AddDomainWithCoGuidRequest request)
        {
            return _repository.AddDomainWithCoGuid(request);
        }

        #endregion

        #region DELETE

        public object Delete(RemoveDeviceFromDomainRequest request)
        {
            return _repository.RemoveDeviceFromDomain(request);
        }

        public object Delete(RemoveDomainRequest request)
        {
            return _repository.RemoveDomain(request);
        }

        public object Delete(RemoveUserFromDomainRequest request)
        {
            return _repository.RemoveUserFromDomain(request);
        }

        #endregion        

        #endregion
    }
}