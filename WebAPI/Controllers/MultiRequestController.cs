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
        private static Dictionary<Type, Dictionary<string, PropertyInfo>> typesToPropertyInfosMap = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        static private object translateToken(object parameter, List<string> tokens, out Type propertyType)
        {
            string token = tokens.ElementAt(0);
            tokens.RemoveAt(0);
            object result;

            if (parameter == null)
                throw new RequestParserException();

            Type parameterType = parameter.GetType();
            propertyType = parameterType;

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
                        conditionValueToConvert = token.Substring(index + matchCondition.Groups[1].Length, token.Length - index - 1 - matchCondition.Groups[1].Length);
                    }
                    
                    List<object> valueList = new List<object>();
                    Dictionary<Type, object> typesToConditionValueMap = new Dictionary<Type, object>();
                    object conditionValue = null;
                    KalturaSkipOperators skipOperator = KalturaSkipOperators.Equal;

                    foreach (var item in parametersList)
                    {
                        Type itemType = item.GetType();

                        if (!typesToPropertyInfosMap.ContainsKey(itemType))
                        {
                            typesToPropertyInfosMap.Add(itemType, new Dictionary<string, PropertyInfo>());
                        }

                        if (!typesToPropertyInfosMap[itemType].ContainsKey(propertyValueName))
                        {
                            PropertyInfo propertyInfoValue = GetPropertyInfo(itemType, propertyValueName);
                            if (propertyInfoValue != null)
                            {
                                typesToPropertyInfosMap[itemType].Add(propertyValueName, propertyInfoValue);
                            }   
                        }

                        if (!typesToPropertyInfosMap[itemType].ContainsKey(propertyValueName))
                            continue;
                        
                        object checkValue = null;
                        
                        if (matchCondition.Success)
                        {
                            if (!typesToPropertyInfosMap[itemType].ContainsKey(conditionPropertyName))
                            {
                                PropertyInfo propertyInfoCoindition = GetPropertyInfo(itemType, conditionPropertyName);
                                if (propertyInfoCoindition != null)
                                {
                                    typesToPropertyInfosMap[itemType].Add(conditionPropertyName, propertyInfoCoindition);
                                }
                            }

                            if (!typesToPropertyInfosMap[itemType].ContainsKey(conditionPropertyName))
                                continue;

                            if (conditionValue == null)
                            {
                                skipOperator = ConvertToKalturaSkipOperators(operatorValue);
                                conditionValue = GetConvertedValue(itemType, conditionPropertyName, skipOperator, conditionValueToConvert);
                            }
                            
                            checkValue = typesToPropertyInfosMap[itemType][conditionPropertyName].GetValue(item);
                        }
                        
                        var value = typesToPropertyInfosMap[itemType][propertyValueName].GetValue(item);
                        if (value != null)
                        {
                            if (!matchCondition.Success || CheckCondition(skipOperator, checkValue, conditionValue))
                            {
                                valueList.Add(value);
                                if (valueList.Count == 1)
                                {
                                    Type realType = GetRealType(typesToPropertyInfosMap[itemType][propertyValueName].PropertyType);
                                    propertyType = typeof(List<>).MakeGenericType(realType);
                                }
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
                    
                    if (parametersList.Count <= index)
                    {
                        throw new RequestParserException(RequestParserException.INVALID_INDEX);
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
                bool found = false;
                result = null;

                if (typesToPropertyInfosMap.ContainsKey(parameterType) && typesToPropertyInfosMap[parameterType].ContainsKey(token))
                {
                    propertyType = GetRealType(typesToPropertyInfosMap[parameterType][token].PropertyType);
                    result = typesToPropertyInfosMap[parameterType][token].GetValue(parameter);
                    found = true;
                }
                else
                {
                    PropertyInfo propertyInfo = GetPropertyInfo(parameterType, token);
                    if (propertyInfo != null)
                    {
                        if (!typesToPropertyInfosMap.ContainsKey(parameterType))
                        {
                            typesToPropertyInfosMap.Add(parameterType, new Dictionary<string, PropertyInfo>());
                        }

                        if (!typesToPropertyInfosMap[parameterType].ContainsKey(token))
                        {
                            typesToPropertyInfosMap[parameterType].Add(token, propertyInfo);
                        }

                        propertyType = GetRealType(propertyInfo.PropertyType);
                        result = propertyInfo.GetValue(parameter);
                        found = true;
                    }
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
                return translateToken(result, tokens, out propertyType);
            }

            return result;
        }
        
        private static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            PropertyInfo propertyInfo = type.GetProperties().FirstOrDefault
                (property => propertyName.Equals(DataModel.getApiName(property), StringComparison.CurrentCultureIgnoreCase));
            
            return propertyInfo;
        }

        private static bool ValidateOperator(Type type, KalturaSkipOperators skipOperator)
        {
            if (type == null || typeof(IList).IsAssignableFrom(type))
                return false;
            
            if (skipOperator == KalturaSkipOperators.Equal || skipOperator == KalturaSkipOperators.UnEqual)
            {
                return true;
            }

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

        private static KalturaSkipOperators ConvertToKalturaSkipOperators(string operatorValue)
        {
            switch (operatorValue)
            {
                case "=": return KalturaSkipOperators.Equal;
                case "!=": return KalturaSkipOperators.UnEqual;
                case ">": return KalturaSkipOperators.GreaterThan;
                case "<": return KalturaSkipOperators.LessThan;
                default: throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE, "operator", "= , != , > , < ");
            }
        }

        private static bool CheckCondition(KalturaSkipOperators skipOperator, dynamic obj1, dynamic obj2)
        {
            if (obj1 == null)
            {
                if (obj2 == null)
                {
                    return false;
                }

                Type t = obj2.GetType();
                obj1 = Activator.CreateInstance(t);
            }

            switch (skipOperator)
            {
                case KalturaSkipOperators.Equal: return obj1.Equals(obj2);
                case KalturaSkipOperators.UnEqual: return !obj1.Equals(obj2);
                case KalturaSkipOperators.GreaterThan: return obj1 > obj2;
                case KalturaSkipOperators.LessThan: return obj1 < obj2;
            }

            return false;
        }

        private static Type GetRealType(Type itemType)
        {
            Type realType = Nullable.GetUnderlyingType(itemType);
            if (realType == null)
            {
                realType = itemType;
            }
            return realType;
        }

        private static object GetConvertedValue(Type itemType, string propertyName, KalturaSkipOperators skipOperator, string valueToConvert, Type propertyType = null)
        {
            if (propertyType != null || typesToPropertyInfosMap.ContainsKey(itemType) && typesToPropertyInfosMap[itemType].ContainsKey(propertyName))
            {
                if (propertyType == null)
                {
                    propertyType = GetRealType(typesToPropertyInfosMap[itemType][propertyName].PropertyType);
                }
                
                if (!ValidateOperator(propertyType, skipOperator))
                {
                    throw new RequestParserException(RequestParserException.INVALID_OPERATOR, skipOperator.ToString(), propertyType.Name);
                }

                object convertedValue = TryConvertTo(propertyType, valueToConvert);
                
                if (convertedValue == null)
                {
                    throw new RequestParserException(RequestParserException.INVALID_CONDITION_VALUE, valueToConvert, propertyType.Name);
                }

                return convertedValue;
            }

            return null;
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

        static private object translateMultirequestTokens(object parameter, object[] responses, out Type propertyType)
        {
            propertyType = null;
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
                        translatedValue = translateToken(responses[index], tokens, out propertyType);
                    }
                    else
                    {
                        translatedValue = responses[index];
                    }

                    log.DebugFormat("at MultiRequestController.translateMultirequestTokens. for parameter: {0} the translated value is: {1}.", match.Value, translatedValue);
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
                    list.Add(translateMultirequestTokens(item, responses, out propertyType));
                }
                parameter = list.ToArray();
            }
            else if (parameter.GetType() == typeof(JArray))
            {
                List<object> list = new List<object>();
                JArray array = (JArray)parameter;
                foreach (object item in array)
                {
                    list.Add(translateMultirequestTokens(item, responses, out propertyType));
                }
                parameter = list.ToArray();
            }
            else if (parameter.GetType() == typeof(Dictionary<string, object>) || parameter.GetType() == typeof(JObject))
            {
                Dictionary<string, object> dict = parameter.GetType() == typeof(JObject) ? ((JObject)parameter).ToObject<Dictionary<string, object>>() : (Dictionary<string, object>)parameter;
                Dictionary<string, object> result = new Dictionary<string, object>();

                foreach (KeyValuePair<string, object> item in dict)
                {
                    result.Add(item.Key, translateMultirequestTokens(item.Value, responses, out propertyType));
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
            bool globalAbortOnError = Utils.Utils.GetAbortOnErrorFromRequest();

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

                if (request[i].SkipCondition is KalturaSkipOnErrorCondition)
                {
                    KalturaSkipOnErrorCondition skipOnErrorCondition = request[i].SkipCondition as KalturaSkipOnErrorCondition;
                    if ((isPreviousErrorOccurred && skipOnErrorCondition.Condition == KalturaSkipOptions.Previous) ||
                        (isAnyErrorOccurred && skipOnErrorCondition.Condition == KalturaSkipOptions.Any))
                    {
                        var requestSkippedException = new BadRequestException(BadRequestException.REQUEST_SKIPPED, "because there was an error in request number " + (abortingRequestIndex + 1));
                        responses[i] = WrappingHandler.prepareExceptionResponse(requestSkippedException.Code, requestSkippedException.Message, requestSkippedException.Args);
                        isAnyErrorOccurred = true;
                        continue;
                    }
                }

                isPreviousErrorOccurred = false;
                Type propertyType;
                
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
                        if (request[i].SkipCondition is KalturaAggregatedPropertySkipCondition)
                        {
                            if (!ValidateAggregationSkipCondition(request[i].SkipCondition as KalturaAggregatedPropertySkipCondition, responses))
                            {
                                var requestSkippedException = new BadRequestException(BadRequestException.REQUEST_SKIPPED, string.Empty);
                                responses[i] = WrappingHandler.prepareExceptionResponse(requestSkippedException.Code, requestSkippedException.Message, requestSkippedException.Args);
                                continue;
                            }
                        }
                        else if (request[i].SkipCondition is KalturaPropertySkipCondition)
                        {
                            if (!ValidateSkipCondition(request[i].SkipCondition as KalturaPropertySkipCondition, responses))
                            {
                                var requestSkippedException = new BadRequestException(BadRequestException.REQUEST_SKIPPED, string.Empty);
                                responses[i] = WrappingHandler.prepareExceptionResponse(requestSkippedException.Code, requestSkippedException.Message, requestSkippedException.Args);
                                continue;
                            }
                        }

                        Dictionary<string, object> parameters = request[i].Parameters;
                        if (i > 0)
                        {
                            parameters = (Dictionary<string, object>)translateMultirequestTokens(parameters, responses, out propertyType);
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

                if (!needToAbortRequest && (request[i].AbortAllOnError || globalAbortOnError) && isPreviousErrorOccurred)
                {
                    needToAbortRequest = true;
                    abortingRequestIndex = i;
                }
            }

            return responses;
        }

        private static bool ValidateAggregationSkipCondition(KalturaAggregatedPropertySkipCondition aggregatedPropertySkipCondition, object[] responses)
        {
            Type propertyType;
            object propertyValue = translateMultirequestTokens(aggregatedPropertySkipCondition.PropertyPath, responses, out propertyType);
            if (propertyValue != null && typeof(IList).IsAssignableFrom(propertyType))
            {
                string[] stringValues = propertyValue.ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                dynamic list = Activator.CreateInstance(propertyType);
                Type realType = propertyType.GenericTypeArguments[0];

                foreach (var item in stringValues)
                {
                    var convertedItem = TryConvertTo(realType, item);
                    list.Add((dynamic)convertedItem);
                }
                
                dynamic aggregatedValue = Activator.CreateInstance(realType);
                //int aggregatedValue = 0;
                switch (aggregatedPropertySkipCondition.AggregationType)
                {
                    case KalturaAggregationType.Count:
                        aggregatedValue = Enumerable.Count(list);
                        break;
                    case KalturaAggregationType.Sum:
                        aggregatedValue = Enumerable.Sum(list);
                        break;
                    case KalturaAggregationType.Avg:
                        aggregatedValue =Enumerable.Average(list);
                        break;
                }

                
                Type conditionType;
                object conditionValue = translateMultirequestTokens(aggregatedPropertySkipCondition.Value, responses, out conditionType);
                if (conditionValue != null)
                {
                    var convertedConditionValue = GetConvertedValue(null, null, aggregatedPropertySkipCondition.Operator, conditionValue.ToString(), realType);
                    if (convertedConditionValue != null && CheckCondition(aggregatedPropertySkipCondition.Operator, aggregatedValue, convertedConditionValue))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool ValidateSkipCondition(KalturaPropertySkipCondition propertySkipCondition, object[] responses)
        {
            Type propertyType;
            object propertyValue = translateMultirequestTokens(propertySkipCondition.PropertyPath, responses, out propertyType);
            if (propertyValue != null)
            {
                var convertedPropValue = GetConvertedValue(null, null, propertySkipCondition.Operator, propertyValue.ToString(), propertyType);
                if (convertedPropValue != null)
                {
                    Type conditionType;
                    object conditionValue = translateMultirequestTokens(propertySkipCondition.Value, responses, out conditionType);
                    if (conditionValue != null)
                    {
                        var convertedConditionValue = GetConvertedValue(null, null, propertySkipCondition.Operator, conditionValue.ToString(), propertyType);
                        if (convertedConditionValue != null && CheckCondition(propertySkipCondition.Operator, convertedPropValue, convertedConditionValue))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
