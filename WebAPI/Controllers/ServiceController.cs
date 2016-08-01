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
using WebAPI.Managers.Scheme;

namespace WebAPI.Controllers
{
    [RoutePrefix("api_v3")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ServiceController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private void createMethodInvoker(string serviceName, string actionName, out MethodInfo methodInfo, out object classInstance)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Type controller = asm.GetType(string.Format("WebAPI.Controllers.{0}Controller", serviceName), false, true);

            if (controller == null)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.InvalidService, "Service doesn't exist");

            Dictionary<string, string> oldStandardActions = OldStandardAttribute.getOldMembers(controller);
            string lowerActionName = actionName.ToLower();
            if (oldStandardActions != null && oldStandardActions.ContainsValue(lowerActionName))
                actionName = oldStandardActions.FirstOrDefault(value => value.Value == lowerActionName).Key;
            
            methodInfo = controller.GetMethod(actionName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (methodInfo == null)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.InvalidAction, "Action doesn't exist");
            
            classInstance = Activator.CreateInstance(controller, null);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route(""), HttpGet]
        public async Task<object> __Action([FromUri]string service, [FromUri]string action)
        {
            return await Action(service, action);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("service/{service_name}"), HttpGet]
        public async Task<object> _Multirequest(string service_name)
        {
            if (service_name.Equals("multirequest", StringComparison.CurrentCultureIgnoreCase))
            {
                return await Action("multirequest", "Do");
            }

            throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.Error, "No action specified");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("service/{service_name}"), HttpPost]
        public async Task<object> Multirequest(string service_name)
        {
            if (service_name.Equals("multirequest", StringComparison.CurrentCultureIgnoreCase))
            {
                return await Action("multirequest", "Do");
            }

            throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.Error, "No action specified");
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
                log.Error("Failed to perform action", ex);

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

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route(""), HttpGet]
        public async Task<object> NoRoute()
        {
            throw new InternalServerErrorException((int)WebAPI.Managers.Models.StatusCode.Error, "No action specified");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route(""), HttpPost]
        public async Task<object> _NoRoute()
        {
            string service = (string) HttpContext.Current.Items[RequestParser.REQUEST_SERVICE];
            string action = (string) HttpContext.Current.Items[RequestParser.REQUEST_ACTION];
            return await Action(service, action);
        }
    }
}
