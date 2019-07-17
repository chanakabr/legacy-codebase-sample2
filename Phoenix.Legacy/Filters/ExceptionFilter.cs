using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebAPI.Models;

namespace WebAPI.Filters
{
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            //if (context.Exception is BadRequestException)
            //    context.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            //else
            //    context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }
    }
}