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
    public class NotificationsService : Service
    {
        public INotificationsRepository _repository { get; set; }  //Injected by IOC

        #region GET
        #endregion

        #region PUT
        #endregion

        #region POST

        public object Post(SubscribeByTagRequest request)
        {
            return _repository.SubscribeByTag(request);
        }

        public object Post(UnSubscribeByTagRequest request)
        {
            return _repository.UnsubscribeFollowUpByTag(request);
        }

        #endregion

        #region DELETE
        #endregion
        
    }
}
