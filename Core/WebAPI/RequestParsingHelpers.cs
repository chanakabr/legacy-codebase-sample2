using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using KalturaRequestContext;
using KLogMonitor;
using Newtonsoft.Json.Linq;
using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Models.MultiRequest;
using WebAPI.Reflection;

namespace WebAPI
{
    public class RequestParsingHelpers
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static List<object> BuildActionArguments(IDictionary<string, MethodParam> methodArgs, IDictionary<string, object> reqParams)
        {
            List<Object> methodParams = new List<object>();
            foreach (KeyValuePair<string, MethodParam> methodArgItem in methodArgs)
            {
                string name = methodArgItem.Key;
                MethodParam methodArg = methodArgItem.Value;

                if (!reqParams.ContainsKey(name) || reqParams[name] == null)
                {
                    if (methodArg.NewName != null && reqParams.ContainsKey(methodArg.NewName) && reqParams[methodArg.NewName] != null)
                    {
                        name = methodArg.NewName;
                    }
                    else
                    {
                        if (methodArg.IsOptional)
                        {
                            methodParams.Add(methodArg.DefaultValue);
                            continue;
                        }
                        else if (methodArg.IsNullable)
                        {
                            methodParams.Add(null);
                            continue;
                        }

                        throw new RequestParserException(RequestParserException.MISSING_PARAMETER, name);
                    }
                }

                try
                {
                    object value = null;

                    // If it is an enum, newtonsoft's bad "ToObject" doesn't do this easily, 
                    // so we do it ourselves in this not so good looking way
                    if (methodArg.IsEnum)
                    {
                        string paramAsString = reqParams[name].ToString();
                        if (paramAsString != null)
                        {
                            value = Enum.Parse(methodArg.Type, paramAsString, true);

                            if (!Enum.IsDefined(methodArg.Type, value))
                            {
                                throw new ArgumentException(string.Format("Invalid enum parameter value {0} was sent for enum type {1}", value, methodArg.Type));
                            }
                        }
                    }

                    else if (methodArg.IsKalturaObject)
                    {
                        Dictionary<string, object> param;
                        string requestName = name;
                        JObject jObject;

                        if (methodArg.IsKalturaMultilingualString)
                        {
                            requestName = KalturaMultilingualString.GetMultilingualName(name);
                        }
                        if (reqParams[requestName].GetType() == typeof(JObject) || reqParams[requestName].GetType().IsSubclassOf(typeof(JObject)))
                        {
                            param = ((JObject)reqParams[requestName]).ToObject<Dictionary<string, object>>();
                            jObject = (JObject)reqParams[requestName];
                        }
                        else
                        {
                            param = (Dictionary<string, object>)reqParams[requestName];
                            jObject = new JObject();

                            foreach (var item in param)
                            {
                                jObject[item.Key] = JToken.FromObject(item.Value);
                            }
                        }

                        KalturaOTTObject res = Deserializer.deserialize(methodArg.Type, param);

                        string service = Convert.ToString(HttpContext.Current.Items[RequestContextConstants.REQUEST_SERVICE]);
                        string action = Convert.ToString(HttpContext.Current.Items[RequestContextConstants.REQUEST_ACTION]);
                        string language = Convert.ToString(HttpContext.Current.Items[RequestContextConstants.REQUEST_LANGUAGE]);
                        string userId = Convert.ToString(HttpContext.Current.Items[RequestContextConstants.REQUEST_USER_ID]);
                        string deviceId = KSUtils.ExtractKSPayload().UDID;
                        int groupId = Convert.ToInt32(HttpContext.Current.Items[RequestContextConstants.REQUEST_GROUP_ID]);

                        object ksObject = HttpContext.Current.Items[RequestContextConstants.REQUEST_KS];
                        KS ks = null;

                        if (ksObject != null)
                        {
                            ks = ksObject as KS;
                        }

                        if (ks != null)
                        {
                            if (string.IsNullOrEmpty(userId))
                            {
                                userId = ks.UserId;
                            }

                            if (groupId <= 0)
                            {
                                groupId = ks.GroupId;
                            }
                        }

                        res.AfterRequestParsed(service, action, language, groupId, userId, deviceId, jObject);

                        value = res;
                    }

                    else if (methodArg.IsNullable) // nullable
                    {
                        if (methodArg.IsDateTime)
                        {
                            long unixTimeStamp = (long)reqParams[name];
                            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                            value = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
                        }
                        else
                        {
                            value = Convert.ChangeType(reqParams[name], methodArg.Type);
                        }
                    }

                    else if (methodArg.IsList) // list
                    {
                        if (typeof(JArray).IsAssignableFrom(reqParams[name].GetType()))
                        {
                            value = KalturaOTTObject.buildList(methodArg.GenericType, (JArray)reqParams[name]);
                        }
                        else if (reqParams[name].GetType().IsArray)
                        {
                            value = KalturaOTTObject.buildList(methodArg.GenericType, reqParams[name] as object[]);
                        }
                    }

                    else if (methodArg.IsMap) // map
                    {
                        Dictionary<string, object> param;
                        if (reqParams[name].GetType() == typeof(JObject) || reqParams[name].GetType().IsSubclassOf(typeof(JObject)))
                        {
                            param = ((JObject)reqParams[name]).ToObject<Dictionary<string, object>>();
                        }
                        else
                        {
                            param = (Dictionary<string, object>)reqParams[name];
                        }
                        value = KalturaOTTObject.buildDictionary(methodArg.Type, param);
                    }

                    else if (reqParams[name] != null)
                    {
                        if (reqParams[name].GetType() == typeof(JObject) || reqParams[name].GetType().IsSubclassOf(typeof(JObject)))
                        {
                            value = ((JObject)reqParams[name]).ToObject(methodArg.Type);
                            if (methodArg.IsDateTime)
                            {
                                long unixTimeStamp = (long)value;
                                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                                value = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
                            }
                        }
                        else if (methodArg.IsDateTime)
                        {
                            long unixTimeStamp = (long)reqParams[name];
                            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                            value = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
                        }
                        else
                        {
                            value = Convert.ChangeType(reqParams[name], methodArg.Type);
                        }
                    }

                    if (methodArg.SchemeArgument != null)
                    {
                        methodArg.SchemeArgument.Validate(value);
                    }

                    methodParams.Add(value);
                }                
                catch (ApiException ex)
                {
                    log.Error("Invalid parameter", ex);
                    throw ex;
                }
                catch (Exception ex)
                {
                    log.Error("Invalid parameter format", ex);
                    throw new RequestParserException(RequestParserException.INVALID_ACTION_PARAMETER, name);
                }
            }

            return methodParams;
        }

        public static List<object> BuildMultirequestActions(IDictionary<string, object> requestParams)
        {
            List<KalturaMultiRequestAction> requests = new List<KalturaMultiRequestAction>();
            Dictionary<string, object> currentRequestParams;

            // multi request abort on error
            HttpContext.Current.Items.Remove(RequestContextConstants.MULTI_REQUEST_GLOBAL_ABORT_ON_ERROR);
            if (requestParams.ContainsKey("abortOnError") && requestParams["abortOnError"] != null)
            {
                bool abortOnError;
                if (requestParams["abortOnError"].GetType() == typeof(JObject) || requestParams["abortOnError"].GetType().IsSubclassOf(typeof(JObject)))
                {
                    abortOnError = ((JObject)requestParams["abortOnError"]).ToObject<bool>();
                }
                else
                {
                    abortOnError = (bool)Convert.ChangeType(requestParams["abortOnError"], typeof(bool));
                }

                HttpContext.Current.Items.Add(RequestContextConstants.MULTI_REQUEST_GLOBAL_ABORT_ON_ERROR, abortOnError);
            }

            int requestIndex = 0;
            foreach (var param in requestParams)
            {
                if (!int.TryParse(param.Key, out requestIndex))
                    continue;

                if (param.Value.GetType() == typeof(JObject) || param.Value.GetType().IsSubclassOf(typeof(JObject)))
                {
                    currentRequestParams = ((JObject)param.Value).ToObject<Dictionary<string, object>>();
                }
                else
                {
                    currentRequestParams = (Dictionary<string, object>)param.Value;
                }

                bool abortAllOnError = false;
                if (currentRequestParams.ContainsKey("abortAllOnError"))
                {
                    BoolUtils.TryConvert(currentRequestParams["abortAllOnError"], out abortAllOnError);
                }

                KalturaSkipCondition skipCondition = null;
                if (currentRequestParams.ContainsKey("skipCondition"))
                {
                    Dictionary<string, object> skipConditionParams;
                    if (currentRequestParams["skipCondition"].GetType() == typeof(JObject) || currentRequestParams["skipCondition"].GetType().IsSubclassOf(typeof(JObject)))
                    {
                        skipConditionParams = ((JObject)currentRequestParams["skipCondition"]).ToObject<Dictionary<string, object>>();
                    }
                    else
                    {
                        skipConditionParams = (Dictionary<string, object>)currentRequestParams["skipCondition"];
                    }

                    if (skipConditionParams.ContainsKey("objectType"))
                    {
                        Type skipConditionType = null;
                        switch (skipConditionParams["objectType"].ToString())
                        {
                            case "KalturaSkipOnErrorCondition":
                                skipConditionType = typeof(KalturaSkipOnErrorCondition);
                                break;
                            case "KalturaPropertySkipCondition":
                                skipConditionType = typeof(KalturaPropertySkipCondition);
                                break;
                            case "KalturaAggregatedPropertySkipCondition":
                                skipConditionType = typeof(KalturaAggregatedPropertySkipCondition);
                                break;
                        }

                        if (skipConditionType != null)
                        {
                            skipCondition = Deserializer.deserialize(skipConditionType, skipConditionParams) as KalturaSkipCondition;
                            skipCondition.Validate();
                        }
                    }
                }

                KalturaMultiRequestAction currentRequest = new KalturaMultiRequestAction()
                {
                    Service = currentRequestParams["service"].ToString(),
                    Action = currentRequestParams["action"].ToString(),
                    Parameters = currentRequestParams,
                    AbortAllOnError = abortAllOnError,
                    SkipCondition = skipCondition
                };

                requests.Add(currentRequest);
                requestIndex++;
            }

            List<object> serviceArguments = new List<object>();
            serviceArguments.Add(requests.ToArray());
            return serviceArguments;
        }
    }
}