using KLogMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebAPI.ClientManagers;
using WebAPI.Controllers;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Managers;
using WebAPI.Managers.Schema;
using System.Runtime.Serialization;
using WebAPI.Models.Billing;
using WebAPI.Models.MultiRequest;

namespace WebAPI.Filters
{
    public class RequestParserException : Exception
    {
        public RequestParserException(int code, string message) : base(message)
        {
            Code = code;
        }

        public int Code { get; set; }
    }

    public class RequestParser : ActionFilterAttribute
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const char PARAMS_PREFIX = ':';
        private const string CB_SECTION_NAME = "tokens";

        private static int accessTokenLength = TCMClient.Settings.Instance.GetValue<int>("access_token_length");
        private static string accessTokenKeyFormat = TCMClient.Settings.Instance.GetValue<string>("access_token_key_format");

        private static string globalKs = null;
        private static string globalUserId = null;
        private static string globalLanguage = null;

        private static CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(CB_SECTION_NAME, true);

        private static Dictionary<string, Type> types = null;
        private static object locker = new object();

        private static Dictionary<string, Type> Types
        {
            get
            {
                if (types == null)
                {
                    lock (locker)
                    {
                        types = new Dictionary<string, Type>();
                        Assembly asm = Assembly.GetExecutingAssembly();
                        var allTypes = asm.GetTypes();

                        foreach (var type in allTypes)
                        {
                            types[type.Name] = type;
                        }
                    }
                }

                return types;
            }
        }

        public const string REQUEST_METHOD_PARAMETERS = "requestMethodParameters";
        public const string REQUEST_VERSION = "requestVersion";
        public const string REQUEST_USER_ID = "user_id";
        public const string REQUEST_LANGUAGE = "language";

        public static object GetRequestPayload()
        {
            return HttpContext.Current.Items[REQUEST_METHOD_PARAMETERS];
        }

        public static MethodInfo createMethodInvoker(string serviceName, string actionName, Assembly asm)
        {
            MethodInfo methodInfo = null;
            Type controller = asm.GetType(string.Format("WebAPI.Controllers.{0}Controller", serviceName), false, true);

            methodInfo = null;

            if (controller == null)
            {
                throw new RequestParserException((int)WebAPI.Managers.Models.StatusCode.InvalidService, "Service doesn't exist");
            }

            Dictionary<string, string> oldStandardActions = OldStandardAttribute.getOldMembers(controller);
            string action = actionName;
            if (oldStandardActions != null && oldStandardActions.ContainsValue(actionName))
            {
                action = oldStandardActions.FirstOrDefault(value => value.Value == actionName).Key;
            }

            if (serviceName.Equals("multirequest", StringComparison.CurrentCultureIgnoreCase))
            {
                action = "Do";
            }

            methodInfo = controller.GetMethod(action, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (methodInfo == null)
            {
                throw new RequestParserException((int)WebAPI.Managers.Models.StatusCode.InvalidAction, "Action doesn't exist");
            }

            ApiAuthorizeAttribute authorization = methodInfo.GetCustomAttribute<ApiAuthorizeAttribute>(true);
            if (authorization != null)
            {
                authorization.IsAuthorized(serviceName, actionName);
            }

            return methodInfo;
        }

        public static void setRequestContext(Dictionary<string, object> requestParams, bool globalScope = true)
        {
            // ks
            KS.ClearOnRequest();
            if (requestParams.ContainsKey("ks") && requestParams["ks"] != null)
            {
                string ks;
                if (requestParams["ks"].GetType() == typeof(JObject) || requestParams["ks"].GetType().IsSubclassOf(typeof(JObject)))
                {
                    ks = ((JObject)requestParams["ks"]).ToObject<string>();
                }
                else
                {
                    ks = (string)Convert.ChangeType(requestParams["ks"], typeof(string));
                }

                InitKS(ks);

                if (globalScope)
                    globalKs = ks;
            }
            else if (globalKs != null)
            {
                InitKS(globalKs);
            }

            // impersonated user_id
            HttpContext.Current.Items.Remove(REQUEST_USER_ID);
            if ((requestParams.ContainsKey("user_id") && requestParams["user_id"] != null) || (requestParams.ContainsKey("userId") && requestParams["userId"] != null))
            {
                object userIdObject = requestParams.ContainsKey("userId") ? requestParams["userId"] : requestParams["user_id"];
                string userId;
                if (userIdObject.GetType() == typeof(JObject) || userIdObject.GetType().IsSubclassOf(typeof(JObject)))
                {
                    userId = ((JObject)userIdObject).ToObject<string>();
                }
                else
                {
                    userId = (string)Convert.ChangeType(userIdObject, typeof(string));
                }

                HttpContext.Current.Items.Add(REQUEST_USER_ID, userId);
                if (globalScope)
                    globalUserId = userId;
            }
            else if (globalUserId != null)
            {
                HttpContext.Current.Items.Add(REQUEST_USER_ID, globalUserId);
            }

            // language
            HttpContext.Current.Items.Remove(REQUEST_LANGUAGE);
            if (requestParams.ContainsKey("language") && requestParams["language"] != null)
            {
                string language;
                if (requestParams["language"].GetType() == typeof(JObject) || requestParams["language"].GetType().IsSubclassOf(typeof(JObject)))
                {
                    language = ((JObject)requestParams["language"]).ToObject<string>();
                }
                else
                {
                    language = (string)Convert.ChangeType(requestParams["language"], typeof(string));
                }

                HttpContext.Current.Items.Add(REQUEST_LANGUAGE, language);
                if (globalScope)
                    globalLanguage = language;
            }
            else if (globalLanguage != null)
            {
                HttpContext.Current.Items.Add(REQUEST_LANGUAGE, globalLanguage);
            }
        }

        public async override void OnActionExecuting(HttpActionContext actionContext)
        {
            string currentAction = null;
            string currentController = null;
            
            // version request
            if (actionContext.Request.GetRouteData().Route.RouteTemplate == "api_v3/version" && (actionContext.Request.Method == HttpMethod.Post) || (actionContext.Request.Method == HttpMethod.Get))
            {
                base.OnActionExecuting(actionContext);
                return;
            }

            if (actionContext.Request.Method == HttpMethod.Post)
            {
                var rd = actionContext.ControllerContext.RouteData;
                currentController = rd.Values["service_name"].ToString();
                if (rd.Values.ContainsKey("action_name"))
                {
                    currentAction = rd.Values["action_name"].ToString();
                }
            }
            else if (actionContext.Request.Method == HttpMethod.Get)
            {
                currentController = actionContext.Request.GetQueryNameValuePairs().Where(x => x.Key.ToLower() == "service").FirstOrDefault().Value;
                var pair = actionContext.Request.GetQueryNameValuePairs().Where(x => x.Key.ToLower() == "action");
                if (pair != null)
                {
                    currentAction = pair.FirstOrDefault().Value;
                }
            }
            else
            {
                createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.BadRequest, "HTTP Method not supported");
                return;
            }

            MethodInfo methodInfo = null;
            Assembly asm = Assembly.GetExecutingAssembly();

            if (actionContext.Request.Method == HttpMethod.Post)
            {
                string result = await actionContext.Request.Content.ReadAsStringAsync();
                if (HttpContext.Current.Request.ContentType == "application/json")
                {
                    using (var input = new StringReader(result))
                    {
                        try
                        {
                            JObject reqParams = JObject.Parse(input.ReadToEnd());

                            if (string.IsNullOrEmpty((string)HttpContext.Current.Items[Constants.CLIENT_TAG]) &&
                                reqParams["clientTag"] != null)
                            {
                                //For logging
                                HttpContext.Current.Items[Constants.CLIENT_TAG] = reqParams["clientTag"];
                            }

                            if (reqParams["apiVersion"] != null)
                            {
                                //For logging and to parse old standard
                                HttpContext.Current.Items[REQUEST_VERSION] = (string) reqParams["apiVersion"];
                            }

                            Dictionary<string, object> requestParams = reqParams.ToObject<Dictionary<string, object>>();
                            setRequestContext(requestParams);

                            methodInfo = createMethodInvoker(currentController, currentAction, asm);
                            List<Object> methodParams = buildActionArguments(methodInfo, requestParams);

                            HttpContext.Current.Items.Add(REQUEST_METHOD_PARAMETERS, methodParams);
                        }
                        catch (UnauthorizedException e)
                        {
                            createErrorResponse(actionContext, (int)e.Code, e.Message);
                            return;
                        }
                        catch (RequestParserException e)
                        {
                            createErrorResponse(actionContext, e.Code, e.Message);
                            return;
                        }
                        catch (JsonReaderException)
                        {
                            createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidJSONRequest, "Invalid JSON");
                            return;
                        }
                    }
                }
                else if (HttpContext.Current.Request.ContentType == "text/xml" ||
                    HttpContext.Current.Request.ContentType == "application/xml")
                {
                    //TODO
                    createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.BadRequest, "XML is currently not supported");
                    return;
                }
                else
                {
                    createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.BadRequest, "Content type is invalid");
                    return;
                }
            }
            else if (actionContext.Request.Method == HttpMethod.Get)
            {
                var tokens = actionContext.Request.GetQueryNameValuePairs().ToDictionary((keyItem) => keyItem.Key,
                    (valueItem) => valueItem.Value);

                if (tokens["apiVersion"] != null)
                {
                    //For logging and to parse old standard
                    HttpContext.Current.Items[REQUEST_VERSION] = tokens["apiVersion"];
                }

                try
                {
                    //Running on the expected method parameters
                    var groupedParams = groupParams(tokens);

                    setRequestContext(groupedParams);
                    methodInfo = createMethodInvoker(currentController, currentAction, asm);

                    List<Object> methodParams = buildActionArguments(methodInfo, groupedParams);

                    HttpContext.Current.Items.Add(REQUEST_METHOD_PARAMETERS, methodParams);
                }
                catch (UnauthorizedException e)
                {
                    createErrorResponse(actionContext, (int)e.Code, e.Message);
                    return;
                }
                catch (RequestParserException e)
                {
                    createErrorResponse(actionContext, e.Code, e.Message);
                    return;
                }

            }

            base.OnActionExecuting(actionContext);
        }

        private static List<object> buildMultirequestActions(Dictionary<string, object> requestParams)
        {
            List<KalturaMultiRequestAction> requests = new List<KalturaMultiRequestAction>();
            Dictionary<string, object> currentRequestParams;
            
            int requestIndex = 0;
            while (requestParams.ContainsKey(requestIndex.ToString()))
            {
                var requestItem = requestParams[requestIndex.ToString()];
                if (requestItem.GetType() == typeof(JObject) || requestItem.GetType().IsSubclassOf(typeof(JObject)))
                {
                    currentRequestParams = ((JObject)requestItem).ToObject<Dictionary<string, object>>();
                }
                else
                {
                    currentRequestParams = (Dictionary<string, object>)requestItem;
                }

                KalturaMultiRequestAction currentRequest = new KalturaMultiRequestAction(); ;
                currentRequest.Service = currentRequestParams["service"].ToString();
                currentRequest.Action = currentRequestParams["action"].ToString();
                currentRequest.Parameters = currentRequestParams;
                requests.Add(currentRequest);
                requestIndex++;
            }

            List<object> serviceArguments = new List<object>();
            serviceArguments.Add(requests.ToArray());
            return serviceArguments;
        }

        public static List<object> buildActionArguments(MethodInfo methodInfo, Dictionary<string, object> reqParams)
        {
            if (methodInfo.ReflectedType == typeof(MultiRequestController))
                return buildMultirequestActions(reqParams);

            //Running on the expected method parameters
            ParameterInfo[] parameters = methodInfo.GetParameters();
            Dictionary<string, string> oldStandardParameters = OldStandardAttribute.getOldMembers(methodInfo);

            List<Object> methodParams = new List<object>();
            foreach (var p in parameters)
            {
                string name = p.Name;
                if (!reqParams.ContainsKey(name) && oldStandardParameters != null && oldStandardParameters.ContainsKey(name))
                    name = oldStandardParameters[name];

                if (!reqParams.ContainsKey(name))
                {
                    if (p.IsOptional)
                    {
                        methodParams.Add(Type.Missing);
                        continue;
                    }
                    else if (p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        methodParams.Add(null);
                        continue;
                    }

                    throw new RequestParserException((int)WebAPI.Managers.Models.StatusCode.InvalidActionParameters, string.Format("Missing parameter {0}", p.Name));
                }

                try
                {
                    Type t = p.ParameterType;
                    
                    // If it is an enum, newtonsoft's bad "ToObject" doesn't do this easily, 
                    // so we do it ourselves in this not so good looking way
                    Type u = Nullable.GetUnderlyingType(t);
                    if (t.IsEnum)
                    {
                        var paramAsString = reqParams[name].ToString();
                        var names = t.GetEnumNames().ToList();
                        if (names.Contains(paramAsString))
                        {
                            methodParams.Add(Enum.Parse(t, paramAsString, true));
                        }
                    }
                    // nullable enum
                    else if (u != null && u.IsEnum)
                    {
                        var paramAsString = reqParams[name] != null ? reqParams[name].ToString() : null;
                        if (paramAsString != null)
                        {
                            var names = u.GetEnumNames().ToList();
                            if (names.Contains(paramAsString))
                            {
                                methodParams.Add(Enum.Parse(u, paramAsString, true));
                            }
                        }
                        else
                        {
                            methodParams.Add(null);
                        }
                    }
                    else if (OldStandardAttribute.isCurrentRequestOldVersion())
                    {
                        if (t.IsSubclassOf(typeof(KalturaOTTObject)))
                        {
                            Dictionary<string, object> param;
                            if (reqParams[name].GetType() == typeof(JObject) || reqParams[name].GetType().IsSubclassOf(typeof(JObject)))
                            {
                                param = ((JObject)reqParams[name]).ToObject<Dictionary<string, object>>();
                            }
                            else
                            {
                                param = (Dictionary<string, object>) reqParams[name];
                            }

                            KalturaOTTObject res = buildObject(t, param);
                            methodParams.Add(res);
                        }
                        else if (t.IsArray || t.IsGenericType) // array or list
                        {
                            Type dictType = typeof(SerializableDictionary<,>);
                            object res = null;

                            if (t.GetGenericArguments().Count() == 1)
                            {
                                // if nullable
                                if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    Type underlyingType = Nullable.GetUnderlyingType(t);
                                    if (underlyingType.IsEnum)
                                    {
                                        res = Enum.Parse(underlyingType, reqParams[name].ToString(), true);
                                    }
                                    else
                                    {
                                        res = Convert.ChangeType(reqParams[name], underlyingType);
                                    }
                                }
                                else // list
                                {
                                    res = buildList(t, (JArray)reqParams[name]);
                                }
                            }

                            //if Dictionary
                            else if (t.GetGenericArguments().Count() == 2)
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

                                res = buildDictionary(t, param);
                            }
                            methodParams.Add(res);
                        }
                        else
                        {
                            object param;
                            if (reqParams[name].GetType() == typeof(JObject) || reqParams[name].GetType().IsSubclassOf(typeof(JObject)))
                            {
                                param = ((JObject)reqParams[name]).ToObject(t);
                            }
                            else
                            {
                                param = Convert.ChangeType(reqParams[name], t);
                            }

                            methodParams.Add(param);
                        }
                    }
                    else
                    {
                        object param;
                        if (reqParams[name].GetType() == typeof(JObject) || reqParams[name].GetType().IsSubclassOf(typeof(JObject)))
                        {
                            param = ((JObject)reqParams[name]).ToObject(t);
                        }
                        else
                        {
                            param = Convert.ChangeType(reqParams[name], t);
                        }

                        methodParams.Add(param);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Invalid parameter format", ex);
                    throw new RequestParserException((int)WebAPI.Managers.Models.StatusCode.InvalidActionParameters, string.Format("Invalid parameter format {0}", p.Name));
                }
            }

            return methodParams;
        }

        private Dictionary<string, object> groupParams(Dictionary<string, string> tokens)
        {
            Dictionary<string, object> paramsDic = new Dictionary<string, object>();

            // group the params by prefix
            foreach (var kv in tokens)
            {
                string[] path = kv.Key.Split(PARAMS_PREFIX);
                setElementByPath(paramsDic, path.ToList(), kv.Value);
            }

            return paramsDic;
        }

        private static string getApiName(PropertyInfo property)
        {
            DataMemberAttribute dataMember = property.GetCustomAttribute<DataMemberAttribute>();
            if (dataMember == null)
                return null;

            return dataMember.Name;
        }

        private static KalturaOTTObject buildObject(Type type, Dictionary<string, object> parameters)
        {
            // if objectType was specified, we will use it only if the anotation type is it's base type
            if (parameters.ContainsKey("objectType"))
            {
                var possibleTypeName = parameters["objectType"].ToString();
                var possibleType = Type.GetType(string.Format("{0},WebAPI", possibleTypeName));
                if (possibleType == null)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    var possibleTypes = assembly.GetTypes().Where(myType => myType.Name == possibleTypeName);
                    possibleType = possibleTypes.First();
                }
                if (possibleType.Name.ToLower() != type.Name.ToLower()) // reflect only if type is different
                {
                    if (possibleType.IsSubclassOf(type)) // we know that the objectType that came from the user is right, and we can use it to initiate the object\
                    {
                        type = possibleType;
                    }
                }
            }

            if (type.IsAbstract)
            {
                throw new RequestParserException((int)WebAPI.Managers.Models.StatusCode.AbstractParameter, "Parameter is abstract");
            }

            var classProperties = type.GetProperties();
            Dictionary<string, string> oldStandardProperties = OldStandardAttribute.getOldMembers(type);
            KalturaOTTObject instance = (KalturaOTTObject)Activator.CreateInstance(type);
            foreach (PropertyInfo property in classProperties)
            {
                var parameterName = getApiName(property);
                if (!parameters.ContainsKey(parameterName) && oldStandardProperties != null && oldStandardProperties.ContainsKey(parameterName))
                    parameterName = oldStandardProperties[parameterName];

                if (!parameters.ContainsKey(parameterName) || !property.CanWrite)
                {
                    continue;
                }

                if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(string))
                {
                    property.SetValue(instance, Convert.ChangeType(parameters[parameterName], property.PropertyType), null);
                    continue;
                }

                if (property.PropertyType.IsEnum)
                {
                    var eValue = Enum.Parse(property.PropertyType, parameters[parameterName].ToString(), true);
                    property.SetValue(instance, eValue, null);
                    continue;
                }

                if (property.PropertyType.IsArray || property.PropertyType.IsGenericType) // array or list
                {
                    Type dictType = typeof(SerializableDictionary<,>);
                    object res = null;

                    if (property.PropertyType.GetGenericArguments().Count() == 1)
                    {
                        // if nullable
                        if (property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            Type underlyingType = Nullable.GetUnderlyingType(property.PropertyType);

                            if (underlyingType.IsEnum)
                            {
                                res = Enum.Parse(underlyingType, parameters[parameterName].ToString(), true);
                            }
                            else
                            {
                                res = Convert.ChangeType(parameters[parameterName], underlyingType);
                            }
                        }
                        else // list
                        {
                            res = buildList(property.PropertyType, (JArray)parameters[parameterName]);
                        }
                    }

                    //if Dictionary
                    else if (property.PropertyType.GetGenericArguments().Count() == 2)
                    {
                        res = buildDictionary(property.PropertyType, ((JObject)parameters[parameterName]).ToObject<Dictionary<string, object>>());
                    }

                    property.SetValue(instance, res, null);
                    continue;
                }

                //If object
                var classRes = buildObject(property.PropertyType, ((JObject)parameters[parameterName]).ToObject<Dictionary<string, object>>());
                property.SetValue(instance, classRes, null);
                continue;

            }

            return instance;
        }

        private static dynamic buildList(Type type, JArray array)
        {
            Type itemType = type.GetGenericArguments()[0];
            Type listType = typeof(List<>).MakeGenericType(itemType);
            dynamic list = Activator.CreateInstance(listType);

            foreach (JToken item in array)
            {
                if (itemType.IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    var itemObject = buildObject(itemType, item.ToObject<Dictionary<string, object>>());
                    list.Add((dynamic)Convert.ChangeType(itemObject, itemType));
                }
                else
                {
                    list.Add((dynamic)Convert.ChangeType(item, itemType));
                }
            }

            return list;
        }

        private static dynamic buildDictionary(Type type, Dictionary<string, object> dictionary)
        {
            dynamic res = Activator.CreateInstance(type);

            Type itemType = type.GetGenericArguments()[1];
            foreach (string key in dictionary.Keys)
            {
                JToken item = (JToken)dictionary[key];

                if (itemType.IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    var itemObject = buildObject(itemType, item.ToObject<Dictionary<string, object>>());
                    res.Add(key, (dynamic)Convert.ChangeType(itemObject, itemType));
                }
                else
                {
                    res.Add(key, (dynamic)Convert.ChangeType(dictionary[key], itemType));
                }
            }

            return res;
        }

        private void setElementByPath(Dictionary<string, object> array, List<string> path, object value)
        {
            Dictionary<string, object> tmpArr = array;

            string key;
            while (path.Count > 0 && (key = path.ElementAt(0)) != null)
            {
                path.Remove(key);

                if (key == "-" && path.Count == 0)
                    break;

                if (!tmpArr.ContainsKey(key) || !(tmpArr[key] is Dictionary<string, object>))
                    tmpArr[key] = new Dictionary<string, object>();

                if (path.Count == 0)
                    tmpArr[key] = value;
                else
                    tmpArr = (Dictionary<string, object>)tmpArr[key];
            }

            array = tmpArr;
        }

        private static void InitKS(string ksVal)
        {
            // the supplied ks is in KS forma (project phoenix's)
            if (IsKsFormat(ksVal))
            {
                parseKS(ksVal);
            }
            // the supplied is in access token format (TVPAPI's)
            else
            {
                GetUserDataFromCB(ksVal);
            }
        }

        private static void GetUserDataFromCB(string ksVal)
        {
            // get token from CB
            string tokenKey = string.Format(accessTokenKeyFormat, ksVal);
            ApiToken token = cbManager.Get<ApiToken>(tokenKey, true);

            if (token == null)
            {
                throw new RequestParserException((int)WebAPI.Managers.Models.StatusCode.InvalidKS, "Invalid KS");
            }

            KS ks = KS.CreateKSFromApiToken(token);

            if (!ks.IsValid)
            {
                throw new RequestParserException((int)WebAPI.Managers.Models.StatusCode.InvalidKS, "Invalid KS");
            }

            ks.SaveOnRequest();

        }

        private static void createErrorResponse(HttpActionContext actionContext, int errorCode, string msg)
        {
            //We cannot use the ApiException* concept in Filters, so we manually invoke exceptions here.
            actionContext.Response = actionContext.Request.CreateResponse(new ApiException.ExceptionPayload()
            {
                code = errorCode,
                error = new HttpError()
                {
                    ExceptionMessage = msg
                }
            });
        }

        private static void parseKS(string ksVal)
        {
            StringBuilder sb = new StringBuilder(ksVal);
            sb = sb.Replace("-", "+");
            sb = sb.Replace("_", "/");

            int groupId = 0;
            byte[] encryptedData = null;
            string encryptedDataStr = null;
            string[] ksParts = null;

            try
            {
                encryptedData = System.Convert.FromBase64String(sb.ToString());
                encryptedDataStr = System.Text.Encoding.ASCII.GetString(encryptedData);
                ksParts = encryptedDataStr.Split('|');
            }
            catch (Exception)
            {
                throw new RequestParserException((int)WebAPI.Managers.Models.StatusCode.InvalidKS, "Invalid KS format");
            }

            if (ksParts.Length < 3 || ksParts[0] != "v2" || !int.TryParse(ksParts[1], out groupId))
            {
                throw new RequestParserException((int)WebAPI.Managers.Models.StatusCode.InvalidKS, "Invalid KS format");
            }

            Group group = null;
            try
            {
                // get group secret
                group = GroupsManager.GetGroup(groupId);
            }
            catch (ApiException ex)
            {
                throw new RequestParserException((int)ex.Code, ex.Message);
            }

            string adminSecret = group.UserSecret;

            // build KS
            KS ks = KS.CreateKSFromEncoded(encryptedData, groupId, adminSecret, ksVal, KS.KSVersion.V2);

            ks.SaveOnRequest();
        }

        private static bool IsKsFormat(string ksVal)
        {
            return ksVal.Length > accessTokenLength;
        }
    }
}