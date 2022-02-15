using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Phx.Lib.Log;


namespace WebAPI.Filters
{
    public class LoggingContextHandler : ActionFilterAttribute
    {
        
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var contextData = new LogContextData();
            contextData.Load();
        }
    }
}