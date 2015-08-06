using Jil;
using KLogMonitor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPI.App_Start;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Models.MultiRequest;

namespace WebAPI.Controllers
{
    [RoutePrefix("multirequest")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MultiRequestController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private object Invoke(Type type, string methodName, params object[] parameters)
        {
            Object obj = Activator.CreateInstance(type);
            MethodInfo methodInfo = type.GetMethod(methodName);

            //Do we need to convert types?            
            var methodParams = methodInfo.GetParameters();
            object[] convParams = new object[methodParams.Count()];

            Dictionary<string, string> pp = new Dictionary<string, string>();
            foreach (var p in parameters)
            {
                string[] kv = ((string)p).Split('=');
                pp.Add(kv[0], kv[1]);
            }

            //Running on expected parameters
            for (int i = 0; i < methodParams.Count(); i++)
            {
                //Skipping (null) if missing from request
                if (!pp.ContainsKey(methodParams[i].Name))
                    continue;

                string val = pp[methodParams[i].Name];

                if (methodParams[i].ParameterType == typeof(int))
                {
                    try
                    {
                        convParams[i] = int.Parse(pp[methodParams[i].Name]);
                    }
                    catch (Exception)
                    {
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest,
                            string.Format("Parameters has incorrect type {0}", methodParams[i].Name));
                    }
                }
                else
                    convParams[i] = pp[methodParams[i].Name];
            }

            try
            {
                return new StatusWrapper(0, Request.GetCorrelationId(), 0, methodInfo.Invoke(obj, convParams), "success");
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is ApiException)
                {
                    ApiException.ExceptionPayload content = null;
                    var e = (ApiException)ex.InnerException;
                    e.Response.TryGetContentValue(out content);

                    return new StatusWrapper(
                        content.code,
                        Request.GetCorrelationId(), 0, null, WrappingHandler.HandleError(content.error.ExceptionMessage,
                        ex.InnerException.StackTrace));
                }

                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest,
                           string.Format("Method invocation failed - {0}", methodName));
            }
        }

        /// <summary>
        /// Run a multi request call 
        /// </summary>
        ///
        /// <remarks>Example:<br />
        ///<![CDATA[ [{"service": "Users", "action": "GetUsersData", "parameters": ["partner_id=215", "user_id=425421"] }, {"service": "Households", "action": "GetHousehold", "parameters": ["partner_id=215", "household_id=0:household_id"] }, {"service": "Households", "action": "GetParentalPIN", "parameters": ["partner_id=215", "household_id=1:id"] } ] ]]>
        ///</remarks>
        /// <param name="request">Sequential API calls' definitions</param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        public object[] Do(KalturaMultiRequest[] request)
        {
            object[] responses = new object[request.Count()];
            for (int i = 0; i < request.Count(); i++)
            {
                Type t = Type.GetType(string.Format("WebAPI.Controllers.{0}Controller", request[i].service));

                for (int j = 0; j < request[i].parameters.Count(); j++)
                {
                    try
                    {
                        string[] kv = request[i].parameters[j].Split('=');
                        string p = kv[1];
                        string[] tokens = null;
                        if (p != null && (tokens = p.Split(':')).Count() > 1)
                        {
                            int respIdx = int.Parse(tokens[0]);
                            dynamic response = responses[respIdx];

                            //TODO: Fix if array or not
                            var innerResult = response.Result;
                            if (response.Result is IEnumerable)
                                innerResult = response.Result[0];

                            Type respType = innerResult.GetType();
                            var properties = innerResult.GetType().GetProperties();
                            bool found = false;
                            foreach (var property in properties)
                            {
                                if (property.GetCustomAttributes(typeof(DataMemberAttribute), true)[0].Name == tokens[1])
                                {
                                    request[i].parameters[j] = string.Format("{0}={1}", kv[0],
                                        respType.GetProperty(property.Name).GetValue(innerResult, null).ToString());

                                    found = true;
                                    break;
                                }
                            }

                            if (found)
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest,
                            string.Format("The method {0} has a parameter ({1}) which its syntax is incorrect", request[i].action,
                            request[i].parameters[j]));
                    }
                }

                responses[i] = Invoke(t, request[i].action, request[i].parameters);
            }

            return responses;
        }
    }
}
