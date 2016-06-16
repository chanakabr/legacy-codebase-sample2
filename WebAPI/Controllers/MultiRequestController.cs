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
using System.Web.Http;
using System.Web.Http.Description;
using WebAPI.App_Start;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Models.MultiRequest;

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
            System.Runtime.Serialization.DataMemberAttribute dataMember = property.GetCustomAttribute<System.Runtime.Serialization.DataMemberAttribute>();
            if (dataMember == null)
                return null;

            return dataMember.Name;
        }

        private object translateToken(object parameter, List<string> tokens)
        {
            string token = tokens.ElementAt(0);
            tokens.RemoveAt(0);
            object result;

            if (parameter.GetType().IsArray)
            {
                int index;
                if (!int.TryParse(token, out index))
                {
                    throw new RequestParserException((int)WebAPI.Managers.Models.StatusCode.InvalidMultirequestToken, "Invalid multirequest token");
                }

                result = parameter.GetType().IsArray ? ((object[])parameter)[index] : ((JArray)parameter)[index];
            }
            else if (parameter.GetType().IsSubclassOf(typeof(KalturaOTTObject)))
            {
                Type type = parameter.GetType();
                var properties = type.GetProperties();
                result = null;
                foreach (PropertyInfo property in properties)
                {
                    string name = getApiName(property);
                    if (!token.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    result = property.GetValue(parameter);
                    break;
                }
            }
            else if (parameter.GetType() == typeof(Dictionary<string, object>))
            {
                Dictionary<string, object> dict = (Dictionary<string, object>)parameter;
                if (!dict.ContainsKey(token))
                {
                    throw new RequestParserException((int)WebAPI.Managers.Models.StatusCode.InvalidMultirequestToken, "Invalid multirequest token");
                }

                result = dict[token];
            }
            else
            {
                throw new RequestParserException((int)WebAPI.Managers.Models.StatusCode.InvalidMultirequestToken, "Invalid multirequest token");
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
                Match match = Regex.Match((string)parameter, @"^(\d):result(:.+)?$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int index = int.Parse(match.Groups[1].Value);
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
                    response = new BadRequestException((int)WebAPI.Managers.Models.StatusCode.InvalidService, "Service doesn't exist");
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
                        MethodInfo methodInfo = RequestParser.createMethodInvoker(request[i].Service, request[i].Action, asm);
                        RequestParser.setRequestContext(parameters);
                        List<Object> methodParams = RequestParser.buildActionArguments(methodInfo, parameters);
                        object controllerInstance = Activator.CreateInstance(controller, null);
                        response = methodInfo.Invoke(controllerInstance, methodParams.ToArray());
                    }
                    catch(Exception e)
                    {
                        response = e;
                    }
                }

                responses[i] = response;
            }

            return responses;
        }
    }
}
