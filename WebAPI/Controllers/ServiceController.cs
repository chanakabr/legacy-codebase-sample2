using KLogMonitor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Routing;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;

namespace WebAPI.Controllers
{
    [RoutePrefix("api")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ServiceController : ApiController
    {
        private void createMethodInvoker(string serviceName, string actionName, out MethodInfo methodInfo, out object classInstance)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Type controller = asm.GetType(string.Format("WebAPI.Controllers.{0}Controller", serviceName), false, true);

            if (controller == null)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.InvalidService, "Service doesn't exist");

            methodInfo = controller.GetMethod(actionName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (methodInfo == null)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.InvalidAction, "Action doesn't exist");

            var authorization = methodInfo.CustomAttributes.Where(x => x.AttributeType == typeof(ApiAuthorizeAttribute)).FirstOrDefault();

            if (authorization != null)
            {
                ApiAuthorizeAttribute auth = (ApiAuthorizeAttribute)authorization.Constructor
                    .Invoke(authorization.ConstructorArguments.Select(x => x.Value).ToArray());

                auth.OnAuthorization(ActionContext);
            }

            classInstance = Activator.CreateInstance(controller, null);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route(""), HttpGet]
        public async Task<object> __Action([FromUri]string service, [FromUri]string action)
        {
            return await Action(service, action);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("service/{service_name}/action/{action_name}"), HttpGet]
        public async Task<object> _Action(string service_name, string action_name)
        {
            return await Action(service_name, action_name);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("service/{service_name}/action/{action_name}"), HttpPost]
        public async Task<object> Action(string service_name, string action_name)
        {
            MethodInfo methodInfo = null;
            object classInstance = null;
            object response = null;

            createMethodInvoker(service_name, action_name, out methodInfo, out classInstance);

            try
            {                
                List<object> methodParams = (List<object>)HttpContext.Current.Items[RequestParser.REQUEST_METHOD_PARAMETERS];
                response = methodInfo.Invoke(classInstance, methodParams.ToArray());
            }
            catch (Exception ex)
            {
                if (ex.InnerException is ApiException)
                {
                    throw ex.InnerException;
                }

                if (ex is TargetParameterCountException)
                {
                    throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.InvalidActionParameters, "Mismatch in parameters");
                }

                throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.Error, "Unable to perform action");
            }

            return response;
        }
    }
}
