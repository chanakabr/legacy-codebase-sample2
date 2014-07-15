using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.WebHost.Endpoints.Support;
using ServiceStack.WebHost.Endpoints.Extensions;

namespace RestfulTVPApi.ServiceInterface
{
    public class CustomNotFoundHttpHandler : IServiceStackHttpHandler, IHttpHandler
    {
        public void ProcessRequest(IHttpRequest request, IHttpResponse response, string operationName)
        {
            HttpError dto = new HttpError(new ResponseStatus("NotFoundException", "Please check your url."), HttpStatusCode.NotFound, HttpStatusCode.NotFound.ToString(), string.Empty);
            response.StatusCode = 404;
            response.End();
                //ServiceStack.WebHost.Endpoints.Extensions.HttpResponseExtensions.WriteToResponse‌(response, request, dto);
        }

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(context.Request.ToRequest(), context.Response.ToResponse(), null);
        }

        public bool IsReusable
        {
            get { return true; }
        }   
    }
}