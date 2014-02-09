using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Collections;
using System.Web.Http;
using TVPWebApi.Models;

namespace TVPWebApi.Models
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PartialResponseAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            string fields = string.Empty;

            if (actionContext.ActionArguments.ContainsKey("fields"))
            {
                fields = actionContext.ActionArguments["fields"].ToString();
            }

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new ParitalResponseContractResolver(fields)
            {
                IgnoreSerializableAttribute = true
            };

            base.OnActionExecuting(actionContext);
        }
    }
}