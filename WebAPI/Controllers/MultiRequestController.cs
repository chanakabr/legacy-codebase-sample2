using Jil;
using KLogMonitor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPI.App_Start;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Models.MultiRequest;
using WebAPI.Reflection;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/multirequest")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiAuthorize]
    public class MultiRequestController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private string getApiName(PropertyInfo property)
        {
            return DataModel.getApiName(property);
        }

        private object translateToken(object parameter, List<string> tokens)
        {
            string token = tokens.ElementAt(0);
            tokens.RemoveAt(0);
            object result;

            if (parameter == null)
                throw new RequestParserException();

            if (parameter.GetType().IsArray)
            {
                int index;
                if (!int.TryParse(token, out index))
                {
                    throw new RequestParserException();
                }

                result = parameter.GetType().IsArray ? ((object[])parameter)[index] : ((JArray)parameter)[index];
            }
            else if (parameter.GetType().IsSubclassOf(typeof(KalturaOTTObject)))
            {
                Type type = parameter.GetType();
                var properties = type.GetProperties();
                result = null;
                bool found = false;
                foreach (PropertyInfo property in properties)
                {
                    string name = getApiName(property);
                    if (!token.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    result = property.GetValue(parameter);
                    found = true;
                    break;
                }
                if (!found)
                    throw new RequestParserException();
            }
            else if (parameter.GetType() == typeof(Dictionary<string, object>))
            {
                Dictionary<string, object> dict = (Dictionary<string, object>)parameter;
                if (!dict.ContainsKey(token))
                {
                    throw new RequestParserException();
                }

                result = dict[token];
            }
            else
            {
                throw new RequestParserException();
            }

            if (tokens.Count > 0)
            {
                return translateToken(result, tokens);
            }

            return result;
        }

        private object translateMultirequestTokens(object parameter, object[] responses)
        {
            if (parameter.GetType() == typeof(string))
            {
                Match match = Regex.Match((string)parameter, @"^{(\d):result(:.+)?}$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int index = int.Parse(match.Groups[1].Value) - 1;
                    if (index < 0)
                        throw new RequestParserException(RequestParserException.INDEX_NOT_ZERO_BASED);

                    if (index >= responses.Length)
                        throw new RequestParserException(RequestParserException.INVALID_INDEX);

                    if (match.Groups[2].Success)
                    {
                        List<string> tokens = new List<string>(match.Groups[2].Value.Split(':'));
                        tokens.RemoveAt(0);
                        parameter = translateToken(responses[index], tokens);
                    }
                    else
                    {
                        parameter = responses[index];
                    }
                }
            }
            else if (parameter.GetType().IsArray)
            {
                List<object> list = new List<object>();
                object[] array = (object[])parameter;
                foreach (object item in array)
                {
                    list.Add(translateMultirequestTokens(item, responses));
                }
                parameter = list.ToArray();
            }
            else if (parameter.GetType() == typeof(JArray))
            {
                List<object> list = new List<object>();
                JArray array = (JArray)parameter;
                foreach (object item in array)
                {
                    list.Add(translateMultirequestTokens(item, responses));
                }
                parameter = list.ToArray();
            }
            else if (parameter.GetType() == typeof(Dictionary<string, object>) || parameter.GetType() == typeof(JObject))
            {
                Dictionary<string, object> dict = parameter.GetType() == typeof(JObject) ? ((JObject)parameter).ToObject<Dictionary<string, object>>() : (Dictionary<string, object>)parameter;
                Dictionary<string, object> result = new Dictionary<string, object>();

                foreach (KeyValuePair<string, object> item in dict)
                {
                    result.Add(item.Key, translateMultirequestTokens(item.Value, responses));
                }
                parameter = result;
            }

            return parameter;
        }

        /// <summary>
        /// Run a multi request call 
        /// </summary>
        ///
        /// <remarks>Example:<br />
        ///<![CDATA[ [{"service": "Users", "action": "GetUsersData", "parameters": ["partnerId=215", "user_id=425421"] }, {"service": "Households", "action": "GetHousehold", "parameters": ["partnerId=215", "household_id=0:household_id"] }, {"service": "Households", "action": "GetParentalPIN", "parameters": ["partnerId=215", "household_id=1:id"] } ] ]]>
        ///</remarks>
        /// <param name="request">Sequential API calls' definitions</param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        public object[] Do(KalturaMultiRequestAction[] request)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            object[] responses = new object[request.Count()];
            for (int i = 0; i < request.Count(); i++)
            {
                object response;
                Type controller = asm.GetType(string.Format("WebAPI.Controllers.{0}Controller", request[i].Service), false, true);
                if (controller == null)
                {
                    response = new BadRequestException(BadRequestException.INVALID_SERVICE, request[i].Service);
                }
                else
                {
                    try
                    {
                        Dictionary<string, object> parameters = request[i].Parameters;
                        if (i > 0)
                        {
                            parameters = (Dictionary<string, object>) translateMultirequestTokens(parameters, responses);
                        }
                        RequestParser.setRequestContext(parameters, request[i].Service, request[i].Action);
                        MethodInfo methodInfo = RequestParser.createMethodInvoker(request[i].Service, request[i].Action, asm);
                        List<Object> methodParams = RequestParser.buildActionArguments(methodInfo, parameters);
                        object controllerInstance = Activator.CreateInstance(controller, null);
                        response = methodInfo.Invoke(controllerInstance, methodParams.ToArray());
                    }
                    catch (ApiException e)
                    {
                        response = e;
                    }
                    catch (Exception e)
                    {
                        response = e.InnerException;
                    }
                }

                if(response is ApiException) 
                {
                    response = WrappingHandler.prepareExceptionResponse(((ApiException)response).Code, ((ApiException)response).Message);
                }
                responses[i] = response;
            }

            return responses;
        }
    }
}
