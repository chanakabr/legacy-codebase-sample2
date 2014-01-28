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
    public class NotificationsService : Service
    {
        public INotificationsRepository _repository { get; set; }  //Injected by IOC

        #region GET
        #endregion

        #region PUT
        #endregion

        #region POST

        public HttpResult Post(SubscribeByTagRequest request)
        {
            var response = _repository.SubscribeByTag(request.InitObj, request.site_guid, request.tags);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Post(UnSubscribeByTagRequest request)
        {
            var response = _repository.UnsubscribeFollowUpByTag(request.InitObj, request.site_guid, request.tags);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        #endregion

        #region DELETE
        #endregion
        
    }
}
