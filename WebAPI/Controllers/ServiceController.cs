using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPI.Exceptions;

namespace WebAPI.Controllers
{
    [RoutePrefix("service")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ServiceController : ApiController
    {
        private static void createMethodInvoker(string serviceName, string actionName, out MethodInfo methodInfo, out object classInstance)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Type controller = asm.GetType(string.Format("WebAPI.Controllers.{0}Controller", serviceName), false, true);

            if (controller == null)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.InvalidService, "Service doesn't exist");

            methodInfo = controller.GetMethod(actionName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (methodInfo == null)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.InvalidAction, "Action doesn't exist");

            classInstance = Activator.CreateInstance(controller, null);
        }

        [Route("{service_name}/action/{action_name}"), HttpGet]
        public async Task<object> _Action([FromUri] string service_name, [FromUri] string action_name)
        {
            return await Action(service_name, action_name);
        }

        [Route("{service_name}/action/{action_name}"), HttpPost]
        public async Task<object> Action([FromUri] string service_name, [FromUri] string action_name)
        {
            MethodInfo methodInfo = null;
            object classInstance = null;

            createMethodInvoker(service_name, action_name, out methodInfo, out classInstance);

            string result = await Request.Content.ReadAsStringAsync();
            using (var input = new StringReader(result))
            {
                try
                {
                    JObject reqParams = JObject.Parse(input.ReadToEnd());

                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    List<Object> methodParams = new List<object>();
                    foreach (var p in parameters)
                    {
                        if (reqParams[p.Name] == null && p.IsOptional)
                        {
                            methodParams.Add(Type.Missing);
                            continue;
                        }

                        methodParams.Add(reqParams[p.Name].ToObject(p.ParameterType));
                    }

                    try
                    {
                        return methodInfo.Invoke(classInstance, methodParams.ToArray());
                    }
                    catch (TargetParameterCountException ex)
                    {
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.InvalidActionParameters,
                            "Mismatch in action parameters");
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is ApiException)
                    {
                        throw ex.InnerException;
                    }

                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.InvalidJSONRequest,
                        "Invalid JSON request");
                }
            }
        }
    }
}
