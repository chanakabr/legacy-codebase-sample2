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

        public object Get(GetGroupOperatorsRequest request)
        {
            var response = _repository.GetGroupOperators(request.InitObj, request.scope);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Get(GetGroupRulesRequest request)
        {
            var response = _repository.GetGroupRules(request.InitObj);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        #endregion

        #region PUT
        #endregion

        #region POST
        #endregion

        #region DELETE
        #endregion
        
    }
}
