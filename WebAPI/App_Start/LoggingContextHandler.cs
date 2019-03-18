using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using KlogMonitorHelper;
using KLogMonitor;


namespace WebAPI
{
    public class LoggingContextHandler : ActionFilterAttribute
    {
        
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var contextData = new ContextData();
            contextData.Load();
        }
    }
}