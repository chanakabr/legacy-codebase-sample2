using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;

namespace TVPWebApi.Models
{
    public class GenericExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is HttpResponseException)
            {
                // do nothing
            }
            else if (context.Exception is UnknownGroupException)
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent(context.Exception.Message)
                };
            }
            else
            {
                context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(context.Exception.Message)
                };
            }
        }
    }
}