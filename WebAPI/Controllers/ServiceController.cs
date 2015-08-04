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
using WebAPI.Managers.Models;

namespace WebAPI.Controllers
{
    [RoutePrefix("api/service")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ServiceController : ApiController
    {
        private void createMethodInvoker(string serviceName, string actionName, out MethodInfo methodInfo, out object classInstance)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Type controller = asm.GetType(string.Format("WebAPI.Controllers.{0}Controller", serviceName), false, true);

            if (controller == null)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.InvalidService, "Service doesn't exist");

            methodInfo = controller.GetMethod(actionName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

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
        [Route("getks"), HttpPost]
        [ApiAuthorize(AllowAnonymous: true)]
        public string GetKS(int group_id, string user_id)
        {
            string userSecret = GroupsManager.GetGroup(group_id).UserSecret;
            KS ks = new KS(userSecret, group_id.ToString(), user_id, 32982398, KS.eUserType.USER, "", string.Empty);
            return ks.ToString();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("{service_name}/action/{action_name}"), HttpGet]
        public async Task<object> _Action([FromUri] string service_name, [FromUri] string action_name)
        {
            return await Action(service_name, action_name);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("{service_name}/action/{action_name}"), HttpPost]
        public async Task<object> Action([FromUri] string service_name, [FromUri] string action_name)
        {
            MethodInfo methodInfo = null;
            object classInstance = null;
            object response = null;

            createMethodInvoker(service_name, action_name, out methodInfo, out classInstance);

            string result = await Request.Content.ReadAsStringAsync();

            //XXX: currently we know how to get only JSON response ---- if (HttpContext.Current.Request.ContentType == "application/json")
            {
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
                            //If we got the clientTag from the JSON - override if does not exist
                            if (string.IsNullOrEmpty((string)HttpContext.Current.Items[Constants.CLIENT_TAG]) && reqParams["clientTag"] != null)
                                HttpContext.Current.Items[Constants.CLIENT_TAG] = reqParams["clientTag"];

                            response = methodInfo.Invoke(classInstance, methodParams.ToArray());
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
            //else if (HttpContext.Current.Request.ContentType == "text/xml" ||
            //    HttpContext.Current.Request.ContentType == "application/xml")
            //{
            //    //TODO
            //}
            //else
            //    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Content type is invalid or missing");

            return response;
        }
    }
}
