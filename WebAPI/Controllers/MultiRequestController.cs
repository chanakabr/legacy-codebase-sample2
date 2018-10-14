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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using TVinciShared;
using WebAPI.App_Start;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.MultiRequest;
using WebAPI.Reflection;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("multirequest")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiAuthorize]
    public class MultiRequestController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static private object translateToken(object parameter, List<string> tokens)
        {
            string token = tokens.ElementAt(0);
            tokens.RemoveAt(0);
            object result;

            if (parameter == null)
                throw new RequestParserException();

            Type parameterType = parameter.GetType();
            if (parameterType.IsArray)
            {
                int index;
                if (!int.TryParse(token, out index))
                {
                    throw new RequestParserException();
                }

                result = ((object[])parameter)[index];
            }
            else if (typeof(IList).IsAssignableFrom(parameterType))
            {
                Match matchCondition = Regex.Match(token, @"^\[[\w\d]+(=|<|>|!=)[^\]]+\]$", RegexOptions.IgnoreCase);
                List<object> parametersList = new List<object>(parameter as IEnumerable<object>);
                
                if (parametersList.FirstOrDefault() is KalturaOTTObject && (token.Equals("*") || matchCondition.Success))
                {
                    string propertyValueName = tokens.ElementAt(0);
                    tokens.RemoveAt(0);
                    
                    string operatorValue = string.Empty;
                    string conditionPropertyName = string.Empty;
                    string conditionValueToConvert = string.Empty;
                    if (matchCondition.Success)
                    {
                        operatorValue = matchCondition.Groups[1].Value;
                        int index = matchCondition.Groups[1].Index;
                        conditionPropertyName = token.Substring(1, index - 1);
                        conditionValueToConvert = token.Substring(index + 1, token.Length - index - 2);
                    }
                    
                    List<object> valueList = new List<object>();
                    // item1 = value, item2 = condition
                    Dictionary<Type, Tuple<PropertyInfo, PropertyInfo>> typesToPropertyInfosMap = new Dictionary<Type, Tuple<PropertyInfo, PropertyInfo>>();
                    Dictionary<Type, object> typesToConditionValueMap = new Dictionary<Type, object>();

                    foreach (var item in parametersList)
                    {
                        Type itemType = item.GetType();
                        if (!typesToPropertyInfosMap.ContainsKey(itemType))
                        {
                            PropertyInfo propertyInfoValue = GetPropertyInfo(itemType, propertyValueName);
                            PropertyInfo propertyInfoCoindition = null;
                            if (matchCondition.Success)
                            {
                                propertyInfoCoindition = GetPropertyInfo(itemType, conditionPropertyName);
                                if (propertyInfoCoindition != null)
                                {
                                    Type conditionType = Nullable.GetUnderlyingType(propertyInfoCoindition.PropertyType);
                                    if (conditionType == null)
                                    {
                                        conditionType = propertyInfoCoindition.PropertyType;
                                    }

                                    if ((operatorValue.Equals("<") || operatorValue.Equals(">")) && !CanUseGreaterOrLessThanOperator(conditionType))
                                    {
                                        throw new RequestParserException(RequestParserException.INVALID_OPERATOR, operatorValue, conditionType.Name);
                                    }

                                    object conditionConvertedValue = TryConvertTo(conditionType, conditionValueToConvert);

                                    if (conditionConvertedValue == null)
                                    {
                                        throw new RequestParserException(RequestParserException.INVALID_CONDITION_VALUE, conditionValueToConvert, conditionType.Name);
                                    }

                                    typesToConditionValueMap.Add(itemType, conditionConvertedValue);
                                }
                            }

                            typesToPropertyInfosMap.Add(itemType, new Tuple<PropertyInfo, PropertyInfo>(propertyInfoValue, propertyInfoCoindition));
                        }

                        if (typesToPropertyInfosMap[itemType].Item1 == null)
                        {
                            continue;
                        }
                        
                        object checkValue = null;
                        object conditionValue = null;
                        if (matchCondition.Success)
                        {
                            if (typesToPropertyInfosMap[itemType].Item2 == null)
                            {
                                continue;
                            }
                            
                            checkValue = typesToPropertyInfosMap[itemType].Item2.GetValue(item);
                            conditionValue = typesToConditionValueMap[itemType];
                        }

                        var value = typesToPropertyInfosMap[itemType].Item1.GetValue(item);
                        if (value != null)
                        {
                            if (!matchCondition.Success || CheckCondition(operatorValue, checkValue, conditionValue))
                            {
                                valueList.Add(value);
                            }
                        }
                    }

                    result = string.Join(",", valueList);
                }
                else if (parametersList.Count > 0)
                {
                    int index;
                    if (!int.TryParse(token, out index))
                    {
                        throw new RequestParserException();
                    }
                    
                    result = parametersList[index];
                }
                else
                {
                    // no items in the list
                    result = string.Empty;
                }
            }
            else if (parameterType.IsSubclassOf(typeof(KalturaOTTObject)))
            {
                var properties = parameterType.GetProperties();
                result = null;
                bool found = false;
                foreach (PropertyInfo property in properties)
                {
                    string name = DataModel.getApiName(property);
                    if (!token.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    result = property.GetValue(parameter);
                    found = true;
                    break;
                }
                if (!found)
                    throw new RequestParserException();
            }
            else if (parameterType == typeof(Dictionary<string, object>))
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

        private static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            PropertyInfo propertyInfo = type.GetProperties().FirstOrDefault
                (property => propertyName.Equals(DataModel.getApiName(property), StringComparison.CurrentCultureIgnoreCase));
            
            return propertyInfo;
        }

        private static bool CanUseGreaterOrLessThanOperator(Type type)
        {
            if (type == null)
                return false;

            TypeCode typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                    return true;
            }

            return false;
        }

        private static bool CheckCondition(string operatorValue, dynamic obj1, dynamic obj2)
        {
            if (obj1 == null)
            {
                return false;
            }

            switch (operatorValue)
            {
                case "=": return obj1.Equals(obj2);
                case "!=": return !obj1.Equals(obj2);
                case ">": return obj1 > obj2;
                case "<": return obj1 < obj2;
                default: throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE, "operator", "= , != , > , < ");
            }
        }

        private static object TryConvertTo(Type type, string value)
        {
            if (!string.IsNullOrEmpty(value) && type != null)
            {
                try
                {
                    return Convert.ChangeType(value, type);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return null;
        }

        static private object translateMultirequestTokens(object parameter, object[] responses)
        {
            if (parameter.GetType() == typeof(string))
            {
                string text = parameter as string;
                Match match = Regex.Match(text, @"{(\d):result(:[^}]+)?}", RegexOptions.IgnoreCase);

                while (match.Success)
                {
                    int index = int.Parse(match.Groups[1].Value) - 1;
                    if (index < 0)
                        throw new RequestParserException(RequestParserException.INDEX_NOT_ZERO_BASED);

                    if (index >= responses.Length)
                        throw new RequestParserException(RequestParserException.INVALID_INDEX);

                    object translatedValue;
                    if (match.Groups[2].Success)
                    {
                        List<string> tokens = new List<string>(match.Groups[2].Value.Split(':'));
                        tokens.RemoveAt(0);
                        translatedValue = translateToken(responses[index], tokens);
                    }
                    else
                    {
                        translatedValue = responses[index];
                        //parameter = responses[index];
                    }

                    text = text.Replace(match.Value, translatedValue.ToString());
                    match = match.NextMatch();
                }

                parameter = text;
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
        [Action("do")]
        [ApiExplorerSettings(IgnoreApi = true)]
        static public object[] Do(KalturaMultiRequestAction[] request)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            object[] responses = new object[request.Count()];
            bool needToAbortRequest = false;
            int abortingRequestIndex = 0;
            bool isPreviousErrorOccurred = false;
            bool isAnyErrorOccurred = false;

            for (int i = 0; i < request.Count(); i++)
            {
                object response;
                
                if (needToAbortRequest)
                {
                    var requestAbortException = new BadRequestException(BadRequestException.REQUEST_ABORTED, abortingRequestIndex + 1);
                    responses[i] = WrappingHandler.prepareExceptionResponse(requestAbortException.Code, requestAbortException.Message, requestAbortException.Args);
                    isAnyErrorOccurred = true;
                    continue;
                }

                if ((isPreviousErrorOccurred && request[i].SkipOnError == SkipOptions.Previous) || 
                    (isAnyErrorOccurred && request[i].SkipOnError == SkipOptions.Any))
                {
                    var requestSkippedException = new BadRequestException(BadRequestException.REQUEST_SKIPPED, abortingRequestIndex + 1);
                    responses[i] = WrappingHandler.prepareExceptionResponse(requestSkippedException.Code, requestSkippedException.Message, requestSkippedException.Args);
                    isAnyErrorOccurred = true;
                    continue;
                }

                isPreviousErrorOccurred = false;
                
                Type controller = asm.GetType(string.Format("WebAPI.Controllers.{0}Controller", request[i].Service), false, true);
                if (controller == null)
                {
                    response = new BadRequestException(BadRequestException.INVALID_SERVICE, request[i].Service);
                    isPreviousErrorOccurred = true;
                    isAnyErrorOccurred = true;
                }
                else
                {
                    try
                    {
                        Dictionary<string, object> parameters = request[i].Parameters;
                        if (i > 0)
                        {
                            parameters = (Dictionary<string, object>)translateMultirequestTokens(parameters, responses);
                        }
                        RequestParser.setRequestContext(parameters, request[i].Service, request[i].Action);
                        Dictionary<string, MethodParam> methodArgs = DataModel.getMethodParams(request[i].Service, request[i].Action);
                        List<Object> methodParams = RequestParser.buildActionArguments(methodArgs, parameters);
                        response = DataModel.execAction(request[i].Service, request[i].Action, methodParams);
                    }
                    catch (ApiException e)
                    {
                        response = e;
                        isPreviousErrorOccurred = true;
                        isAnyErrorOccurred = true;
                    }
                    catch (Exception e)
                    {
                        response = e.InnerException;
                        isPreviousErrorOccurred = true;
                        isAnyErrorOccurred = true;
                    }
                }
                
                if (response is ApiException)
                {
                    response = WrappingHandler.prepareExceptionResponse(((ApiException)response).Code, ((ApiException)response).Message, ((ApiException)response).Args);
                }

                responses[i] = response;

                if (!needToAbortRequest && request[i].AbortAllOnError && isPreviousErrorOccurred)
                {
                    needToAbortRequest = true;
                    abortingRequestIndex = i;
                }
            }

            return responses;
        }
    }
}
