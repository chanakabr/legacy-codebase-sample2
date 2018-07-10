using ApiObjects.Response;
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
using WebAPI.Reflection;

namespace WebAPI.Controllers
{
    [RoutePrefix("api_v3")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ServiceController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route(""), HttpGet, HttpOptions]
        public async Task<object> __Action([FromUri]string service, [FromUri]string action)
        {
            return await Action(service, action);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("service/{service_name}"), HttpGet, HttpOptions]
        public async Task<object> _Multirequest(string service_name)
        {
            if (service_name.Equals("multirequest", StringComparison.CurrentCultureIgnoreCase))
            {
                return await Action("multirequest", "Do");
            }

            throw new BadRequestException(BadRequestException.ACTION_NOT_SPECIFIED);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("service/{service_name}"), HttpPost]
        public async Task<object> Multirequest(string service_name)
        {
            if (service_name.Equals("multirequest", StringComparison.CurrentCultureIgnoreCase))
            {
                return await Action("multirequest", "Do");
            }

            throw new BadRequestException(BadRequestException.ACTION_NOT_SPECIFIED);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("service/{service_name}/action/{action_name}"), HttpGet, HttpOptions]
        public async Task<object> _Action(string service_name, string action_name)
        {
            return await Action(service_name, action_name);
        }

        public static object ExecGeneric(MethodInfo methodInfo, List<object> methodParams)
        {
            Type genericType = GetGenericParam(methodParams);
            if (genericType != null)
            {
                methodInfo = methodInfo.MakeGenericMethod(genericType.GetGenericArguments());
            }
            return methodInfo.Invoke(null, methodParams.ToArray());
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("service/{service_name}/action/{action_name}"), HttpPost]
        public async Task<object> Action(string service_name, string action_name)
        {
            object response = null;

            try
            {
                List<object> methodParams = (List<object>)HttpContext.Current.Items[RequestParser.REQUEST_METHOD_PARAMETERS];

                // add action to log
                HttpContext.Current.Items[Constants.ACTION] = string.Format("{0}.{1}",
                    string.IsNullOrEmpty(service_name) ? "null" : service_name,
                    string.IsNullOrEmpty(action_name) ? "null" : action_name);

                response = DataModel.execAction(service_name, action_name, methodParams);
            }
            catch (ApiException ex)
            {
                throw ex;
            }
            catch (TargetParameterCountException ex)
            {
                throw new BadRequestException(BadRequestException.INVALID_ACTION_PARAMETERS);
            }
            catch (Exception ex)
            {
                log.Error("Failed to perform action", ex);

                if (ex.InnerException is ApiException)
                {
                    throw ex.InnerException;
                }

                throw new InternalServerErrorException();
            }

            return response;
        }

        static private Type GetGenericParam(List<object> methodParams)
        {
            foreach (var param in methodParams)
            {
                var type = param.GetType();
                while (type != typeof(object))
                {
                    if (type.IsGenericType)
                        return type;

                    type = type.BaseType;
                }
            }

            return null;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route(""), HttpGet, HttpOptions]
        public async Task<object> NoRoute()
        {
            throw new BadRequestException(BadRequestException.ACTION_NOT_SPECIFIED);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route(""), HttpPost]
        public async Task<object> _NoRoute()
        {
            string service = (string)HttpContext.Current.Items[RequestParser.REQUEST_SERVICE];
            string action = (string)HttpContext.Current.Items[RequestParser.REQUEST_ACTION];
            return await Action(service, action);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("service/{service_name}/action/{action_name}/{*pathData}"), HttpPost, HttpGet, HttpOptions]
        [FailureHttpCode(System.Net.HttpStatusCode.NotFound)]
        public async Task<object> ActionWithParams(string service_name, string action_name, string pathData)
        {
            object response = null;

            try
            {
                List<object> methodParams = (List<object>)HttpContext.Current.Items[WebAPI.Filters.RequestParser.REQUEST_METHOD_PARAMETERS];
                response = DataModel.execAction(service_name, action_name, methodParams);
            }
            catch (ApiException ex)
            {
                ApiException apiEx = new ApiException(ex, System.Net.HttpStatusCode.NotFound);
                throw apiEx;
            }
            catch (TargetParameterCountException ex)
            {
                ApiException apiEx = new ApiException(new BadRequestException(BadRequestException.INVALID_ACTION_PARAMETERS), System.Net.HttpStatusCode.NotFound);
                throw apiEx;
            }
            catch (Exception ex)
            {
                log.Error("Failed to perform action", ex);
                HttpStatusCode? failureHttpCode = DataModel.getFailureHttpCode(service_name, action_name);

                if (ex.InnerException is ApiException)
                {
                    if (failureHttpCode.HasValue)
                    {
                        throw new ApiException((ApiException)ex.InnerException, failureHttpCode.Value);
                    }
                    throw (ApiException)ex.InnerException;
                }

                if (failureHttpCode.HasValue)
                {
                    throw new ApiException(ex, failureHttpCode.Value);
                }
                throw ex;
            }

            return response;
        }
    }
}
