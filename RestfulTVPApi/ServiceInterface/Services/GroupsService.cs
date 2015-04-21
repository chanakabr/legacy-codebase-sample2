using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System.Net;
using ServiceStack.Api.Swagger;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using System.Collections.Generic;
using RestfulTVPApi.ServiceModel;
using System.Linq;
using ServiceStack;
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
            return _repository.GetGroupOperators(request);
        }

        public object Get(GetGroupRulesRequest request)
        {
            return _repository.GetGroupRules(request);
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
