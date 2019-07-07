using KLogMonitor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Reflection;

namespace WebAPI
{
    public class RequestParsingHelpers
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static List<object> BuildActionArguments(Dictionary<string, MethodParam> methodArgs, IDictionary<string, object> reqParams)
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

                        string service = Convert.ToString(HttpContext.Current.Items[RequestContext.REQUEST_SERVICE]);
                        string action = Convert.ToString(HttpContext.Current.Items[RequestContext.REQUEST_ACTION]);
                        string language = Convert.ToString(HttpContext.Current.Items[RequestContext.REQUEST_LANGUAGE]);
                        string userId = Convert.ToString(HttpContext.Current.Items[RequestContext.REQUEST_USER_ID]);
                        string deviceId = KSUtils.ExtractKSPayload().UDID;
                        int groupId = Convert.ToInt32(HttpContext.Current.Items[RequestContext.REQUEST_GROUP_ID]);

                        object ksObject = HttpContext.Current.Items[RequestContext.REQUEST_KS];
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
                            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
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
                                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                                value = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
                            }
                        }
                        else if (methodArg.IsDateTime)
                        {
                            long unixTimeStamp = (long)reqParams[name];
                            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
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
    }
}