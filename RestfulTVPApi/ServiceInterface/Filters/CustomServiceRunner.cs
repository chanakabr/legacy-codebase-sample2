using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Web;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.PartialResponse.ServiceModel;
using System.Collections;

namespace RestfulTVPApi.ServiceInterface
{
    public class CustomServiceRunner<TRequest> : ServiceRunner<TRequest>
    {
        public CustomServiceRunner(IAppHost appHost, ActionContext actionContext)
            : base(appHost, actionContext)
        {
        }

        public override void OnBeforeExecute(IRequestContext requestContext, TRequest request)
        {
            base.OnBeforeExecute(requestContext, request);
        }

        public override object OnAfterExecute(IRequestContext requestContext, object response)
        {
            if (response != null)                
                response = requestContext.ToOptimizedResult(requestContext.ToPartialResponse(response));

            return base.OnAfterExecute(requestContext, response);
        }

        public override object HandleException(IRequestContext requestContext, TRequest request, Exception ex)
        {
            return base.HandleException(requestContext, request, ex);
        }
    }
}