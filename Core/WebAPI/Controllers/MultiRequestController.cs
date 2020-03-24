using KLogMonitor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.MultiRequest;
using WebAPI.Reflection;
using WebAPI.Utils;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Mvc;
#endif
#if NET461
using System.Web.Http.Description;
#endif

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
                                    Type realType = typesToPropertyInfosMap[itemType][propertyValueName].PropertyType.GetRealType();
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
                    propertyType = typesToPropertyInfosMap[parameterType][token].PropertyType.GetRealType();
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

                        propertyType = propertyInfo.PropertyType.GetRealType();
                        result = propertyInfo.GetValue(parameter);
                        found = true;
                    }
                }
               
                if (!found)
                    throw new RequestParserException();
            }
            else
            {
                var genericTypeDefinition = parameterType.GetGenericTypeDefinition();
                if (parameterType.IsGenericType && (genericTypeDefinition == typeof(Dictionary<,>) || genericTypeDefinition == typeof(SerializableDictionary<,>)))
                {
                    var dynamicParameter = (parameter as dynamic);
                    if (dynamicParameter != null && dynamicParameter.ContainsKey(token))
                    {
                        result = dynamicParameter[token];
                    }
                    else
                    {
                        throw new RequestParserException();
                    }
                }
                else
                {
                    throw new RequestParserException();
                }
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
        
        private static object GetConvertedValue(Type itemType, string propertyName, KalturaSkipOperators skipOperator, string valueToConvert, Type propertyType = null)
        {
            if (propertyType != null || (typesToPropertyInfosMap.ContainsKey(itemType) && typesToPropertyInfosMap[itemType].ContainsKey(propertyName)))
            {
                if (propertyType == null)
                {
                    propertyType = typesToPropertyInfosMap[itemType][propertyName].PropertyType.GetRealType();
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
                    if (type.IsEnum)
                    {
                        return Enum.Parse(type, value);
                    }

                    return Convert.ChangeType(value, type);
                }
                catch (Exception ex)
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
            else if (parameter.GetType() == typeof(Dictionary<string, object>))
            {
                Dictionary<string, object> dict = parameter.GetType() == typeof(JObject) ? ((JObject)parameter).ToObject<Dictionary<string, object>>() : (Dictionary<string, object>)parameter;
                Dictionary<string, object> result = new Dictionary<string, object>();

                foreach (KeyValuePair<string, object> item in dict)
                {
                    result.Add(item.Key, translateMultirequestTokens(item.Value, responses, out propertyType));
                }

                parameter = result;
            }
            else if(parameter.GetType() == typeof(JObject))
            {
                Dictionary<string, object> dict = parameter.GetType() == typeof(JObject) ? ((JObject)parameter).ToObject<Dictionary<string, object>>() : (Dictionary<string, object>)parameter;
                JObject result = new JObject();

                foreach (KeyValuePair<string, object> item in dict)
                {
                    result.Add(new JProperty(item.Key, translateMultirequestTokens(item.Value, responses, out propertyType)));
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
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public object[] Do(KalturaMultiRequestAction[] request)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            object[] responses = new object[request.Count()];
            bool needToAbortRequest = false;
            int abortingIndex = 0;
            int lastFaildIndex = 0;
            bool isPreviousErrorOccurred = false;
            bool isAnyErrorOccurred = false;
            bool globalAbortOnError = Utils.Utils.GetAbortOnErrorFromRequest();

            for (int i = 0; i < request.Count(); i++)
            {
                object response;

                if (needToAbortRequest)
                {
                    response = new BadRequestException(BadRequestException.REQUEST_ABORTED, abortingIndex + 1);
                }
                else
                {
                    try
                    {
                        HttpContext.Current.Items[CachingProvider.LayeredCache.LayeredCache.IS_READ_ACTION] = false;

                        if (!string.IsNullOrEmpty(request[i].Action))
                        {
                            bool isReadAction = CachingProvider.LayeredCache.LayeredCache.readActions.Contains(request[i].Action);
                            HttpContext.Current.Items[CachingProvider.LayeredCache.LayeredCache.IS_READ_ACTION] = isReadAction;
                            if (!string.IsNullOrEmpty(request[i].Service))
                            {
                                HttpContext.Current.Items[Constants.ACTION] = string.Format("{0}.{1}", request[i].Service, request[i].Action);
                                HttpContext.Current.Items[Constants.MULTIREQUEST] = "1";
                            }
                        }

                        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_CLIENT_API_START))
                        {
                            BadRequestException badRequest;
                            if (!ValidateSkipCondition(request[i].SkipCondition, isAnyErrorOccurred, lastFaildIndex, isPreviousErrorOccurred, i, responses, out badRequest))
                            {
                                response = badRequest;
                                isPreviousErrorOccurred = false;
                            }
                            else
                            {
                                isPreviousErrorOccurred = false;
                                Type controller = executingAssembly.GetType(string.Format("WebAPI.Controllers.{0}Controller", request[i].Service), false, true);
                                if (controller == null)
                                {
                                    throw new BadRequestException(BadRequestException.INVALID_SERVICE, request[i].Service);
                                }

                                Dictionary<string, object> parameters = request[i].Parameters;
                                if (i > 0)
                                {
                                    Type propertyType;
                                    parameters = (Dictionary<string, object>)translateMultirequestTokens(parameters, responses, out propertyType);
                                }
                                RequestContext.SetContext(parameters, request[i].Service, request[i].Action);
                                Dictionary<string, MethodParam> methodArgs = DataModel.getMethodParams(request[i].Service, request[i].Action);
                                List<Object> methodParams = RequestParsingHelpers.BuildActionArguments(methodArgs, parameters);
                                response = DataModel.execAction(request[i].Service, request[i].Action, methodParams);
                            }
                        }
                    }
                    catch (ApiException e)
                    {
                        isPreviousErrorOccurred = true;
                        isAnyErrorOccurred = true;
                        lastFaildIndex = i;
                        response = e;
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat("Exception received while calling MultiRequestController.Do , exception: {0}", e);
                        isPreviousErrorOccurred = true;
                        isAnyErrorOccurred = true;
                        lastFaildIndex = i;

                        if (e.InnerException is ApiException)
                        {
                            response = e.InnerException;
                        }
                        else
                        {
                            response = new ApiException(ErrorUtils.GetClientException(e));
                        }
                    }
                }
                
                if (response is ApiException)
                {
                    response = KalturaApiExceptionHelpers.prepareExceptionResponse(((ApiException)response).Code, ((ApiException)response).Message, ((ApiException)response).Args);
                }

                responses[i] = response;

                if (!needToAbortRequest && isPreviousErrorOccurred && (request[i].AbortAllOnError || globalAbortOnError))
                {
                    needToAbortRequest = true;
                    abortingIndex = i;
                }
            }

            return responses;
        }

        private static bool ValidateSkipCondition(KalturaSkipCondition skipCondition, bool isAnyErrorOccurred, int lastFaildIndex, bool isPreviousErrorOccurred, 
                                           int currentIndex, object[] responses, out BadRequestException badRequest)
        {
            badRequest = null;
            
            if (skipCondition is KalturaSkipOnErrorCondition)
            {
                KalturaSkipOnErrorCondition skipOnErrorCondition = skipCondition as KalturaSkipOnErrorCondition;

                if (isAnyErrorOccurred && skipOnErrorCondition.Condition == KalturaSkipOptions.Any)
                {
                    badRequest = new BadRequestException(BadRequestException.REQUEST_SKIPPED, "because there was an error in request number " + (lastFaildIndex + 1));
                    return false;
                }
                else if (isPreviousErrorOccurred && skipOnErrorCondition.Condition == KalturaSkipOptions.Previous)
                {
                    badRequest = new BadRequestException(BadRequestException.REQUEST_SKIPPED, "because there was an error in request number " + currentIndex);
                    return false;
                }
            }
            else if (skipCondition is KalturaAggregatedPropertySkipCondition)
            {
                KalturaAggregatedPropertySkipCondition aggregatedPropertySkipCondition = skipCondition as KalturaAggregatedPropertySkipCondition;
                Tuple<double, double> translatedConditionValues;
                if (!ValidateAggregationSkipCondition(aggregatedPropertySkipCondition, responses, out translatedConditionValues))
                {
                    // FORMAT: count({2:result:objects:0:id}=1) lessthan ({2:result:totalCount}=3)
                    string reason = string.Format("{0}({1}={2}) {3} ({4}={5})",
                                                   aggregatedPropertySkipCondition.AggregationType,
                                                   aggregatedPropertySkipCondition.PropertyPath,
                                                   translatedConditionValues.Item1,
                                                   aggregatedPropertySkipCondition.Operator,
                                                   aggregatedPropertySkipCondition.Value,
                                                   translatedConditionValues.Item2);
                    badRequest = new BadRequestException(BadRequestException.REQUEST_SKIPPED, reason);
                    return false;
                }
            }
            else if (skipCondition is KalturaPropertySkipCondition)
            {
                KalturaPropertySkipCondition propertySkipCondition = skipCondition as KalturaPropertySkipCondition;
                Tuple<object, object> translatedConditionValues;
                if (!ValidateSkipCondition(propertySkipCondition, responses, out translatedConditionValues))
                {
                    // FORMAT: ({2:result:objects:0:id}=1) lessthan ({2:result:totalCount}=3)
                    string reason = string.Format("({0}={1}) {2} ({3}={4})",
                                                   propertySkipCondition.PropertyPath,
                                                   translatedConditionValues.Item1,
                                                   propertySkipCondition.Operator,
                                                   propertySkipCondition.Value,
                                                   translatedConditionValues.Item2);
                    badRequest = new BadRequestException(BadRequestException.REQUEST_SKIPPED, reason);
                    return false;
                }
            }
            
            return true;
        }

        private static bool ValidateAggregationSkipCondition(KalturaAggregatedPropertySkipCondition aggregatedPropertySkipCondition, object[] responses, out Tuple<double, double> translatedConditionValues)
        {
            Type propertyType;
            object propertyValue = translateMultirequestTokens(aggregatedPropertySkipCondition.PropertyPath, responses, out propertyType);
            if (propertyValue != null && typeof(IList).IsAssignableFrom(propertyType))
            {
                string[] stringValues = propertyValue.ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<long> list = new List<long>();
                foreach (var item in stringValues)
                {
                    long convertedItem;
                    if (long.TryParse(item, out convertedItem))
                    {
                        list.Add(convertedItem);
                    }
                    else
                    {
                        throw new RequestParserException(RequestParserException.INVALID_CONDITION_VALUE, item, "Numeric (int, long, etc)");
                    }
                }

                double aggregatedValue = 0;
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
                    var convertedConditionValue = GetConvertedValue(null, null, aggregatedPropertySkipCondition.Operator, conditionValue.ToString(), typeof(double));
                    if (convertedConditionValue != null && CheckCondition(aggregatedPropertySkipCondition.Operator, aggregatedValue, convertedConditionValue))
                    {
                        translatedConditionValues = new Tuple<double, double>(aggregatedValue, (double)convertedConditionValue);
                        return false;
                    }
                }
            }

            translatedConditionValues = new Tuple<double, double>(0, 0);
            return true;
        }

        private static bool ValidateSkipCondition(KalturaPropertySkipCondition propertySkipCondition, object[] responses, out Tuple<object, object> translatedConditionValues)
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
                            translatedConditionValues = new Tuple<object, object>(convertedPropValue, convertedConditionValue);
                            return false;
                        }
                    }
                }
            }

            translatedConditionValues = new Tuple<object, object>(0, 0);
            return true;
        }
    }
}
