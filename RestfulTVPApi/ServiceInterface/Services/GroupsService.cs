using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System.Net;
using ServiceStack.Api.Swagger;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using System.Collections.Generic;
using RestfulTVPApi.ServiceModel;
using System.Linq;
using TVPApi;
using ServiceStack;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using System;

namespace RestfulTVPApi.ServiceInterface
{

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class GroupsService : Service
    {
        public IGroupsRepository _repository { get; set; }  //Injected by IOC

        #region GET

        public HttpResult Get(GetGroupOperatorsRequest request)
        {
            var response = _repository.GetGroupOperators(request.InitObj, request.scope);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetGroupRulesRequest request)
        {
            var response = _repository.GetGroupRules(request.InitObj);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(FBConfigRequest request)
        {
            var response = _repository.FBConfig(request.InitObj);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetFBUserDataRequest request)
        {
            var response = _repository.GetFBUserData(request.InitObj, request.token);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        #endregion

        #region PUT

        public HttpResult Put(FBUserMergeRequest request)
        {
            var response = _repository.FBUserMerge(request.InitObj, request.token, request.facebook_id, request.user_name, request.password);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        #endregion

        #region POST

        public HttpResult Post(FBUserRegisterRequest request)
        {
            var response = _repository.FBUserRegister(request.InitObj, request.token, request.create_new_domain, request.get_newsletter);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        #endregion

        #region DELETE
        #endregion
        
    }
}
