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

namespace WebAPI.Filters
{
    public class RequestParser : ActionFilterAttribute
    {
        private const char PARAMS_PREFIX = ':';
        private const string CB_SECTION_NAME = "tokens";

        private static int accessTokenLength = TCMClient.Settings.Instance.GetValue<int>("access_token_length");
        private static string accessTokenKeyFormat = TCMClient.Settings.Instance.GetValue<string>("access_token_key_format");

        private static CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(CB_SECTION_NAME);

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

        public static object GetRequestPayload()
        {
            return HttpContext.Current.Items[REQUEST_METHOD_PARAMETERS];
        }

        private bool createMethodInvoker(HttpActionContext actionContext, string serviceName, string actionName, Assembly asm, out MethodInfo methodInfo, out object classInstance)
        {
            Type controller = asm.GetType(string.Format("WebAPI.Controllers.{0}Controller", serviceName), false, true);

            classInstance = null;
            methodInfo = null;

            if (controller == null)
            {
                createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidService, "Service doesn't exist");
                return false;
            }

            methodInfo = controller.GetMethod(actionName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (methodInfo == null)
            {
                createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidAction, "Action doesn't exist");
                return false;
            }

            classInstance = Activator.CreateInstance(controller, null);

            return true;
        }

        public async override void OnActionExecuting(HttpActionContext actionContext)
        {
            string currentAction;
            string currentController;
            
            // version request
            if (actionContext.Request.GetRouteData().Route.RouteTemplate == "api_v3/version" && (actionContext.Request.Method == HttpMethod.Post) || (actionContext.Request.Method == HttpMethod.Get))
            {
                base.OnActionExecuting(actionContext);
                return;
            }

            if (actionContext.Request.Method == HttpMethod.Post)
            {
                var rd = actionContext.ControllerContext.RouteData;
                currentAction = rd.Values["action_name"].ToString();
                currentController = rd.Values["service_name"].ToString();
            }
            else if (actionContext.Request.Method == HttpMethod.Get)
            {
                currentAction = actionContext.Request.GetQueryNameValuePairs().Where(x => x.Key.ToLower() == "action").FirstOrDefault().Value;
                currentController = actionContext.Request.GetQueryNameValuePairs().Where(x => x.Key.ToLower() == "service").FirstOrDefault().Value;
            }
            else
            {
                createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.BadRequest, "HTTP Method not supported");
                return;
            }

            MethodInfo methodInfo = null;
            object classInstance = null;
            Assembly asm = Assembly.GetExecutingAssembly();

            if (!createMethodInvoker(actionContext, currentController, currentAction, asm, out methodInfo, out classInstance))
                return;

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

                            // ks
                            if (reqParams["ks"] != null)
                                InitKS(actionContext, reqParams["ks"].ToObject<string>());

                            // additional user_id
                            if (reqParams["user_id"] != null)
                                HttpContext.Current.Items.Add("user_id", reqParams["user_id"]);

                            //Running on the expected method parameters
                            ParameterInfo[] parameters = methodInfo.GetParameters();

                            List<Object> methodParams = new List<object>();
                            foreach (var p in parameters)
                            {
                                if (reqParams[p.Name] == null && p.IsOptional)
                                {
                                    methodParams.Add(Type.Missing);
                                    continue;
                                }
                                else if (reqParams[p.Name] == null && p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    methodParams.Add(null);
                                    continue;
                                }
                                else if (reqParams[p.Name] == null)
                                {
                                    createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidActionParameters, string.Format("Missing parameter {0}", p.Name));
                                    return;
                                }

                                try
                                {
                                    Type t = p.ParameterType;

                                    var objType = reqParams[p.Name].SelectToken("objectType");

                                    if (objType != null)
                                    {
                                        string objectTypeName = objType.ToString();

                                        if (Types.ContainsKey(objectTypeName))
                                        {
                                            t = RequestParser.Types[objectTypeName];
                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }

                                    methodParams.Add(reqParams[p.Name].ToObject(t));
                                }
                                catch (Exception)
                                {
                                    createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidActionParameters, string.Format("Invalid parameter format {0}", p.Name));
                                    return;
                                }
                            }

                            HttpContext.Current.Items.Add(REQUEST_METHOD_PARAMETERS, methodParams);
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

                if (tokens["ks"] != null)
                    InitKS(actionContext, tokens["ks"]);

                //Running on the expected method parameters
                ParameterInfo[] parameters = methodInfo.GetParameters();
                var groupedParams = groupParams(tokens);

                List<Object> methodParams = buildActionArguments(parameters, groupedParams, actionContext);

                HttpContext.Current.Items.Add(REQUEST_METHOD_PARAMETERS, methodParams);
            }

            base.OnActionExecuting(actionContext);
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

        public List<object> buildActionArguments(ParameterInfo[] actionParams, Dictionary<string, object> paramsGrouped, HttpActionContext actionContext)
        {
            List<object> serviceArguments = new List<object>();
            foreach (var actionParam in actionParams)
            {
                var type = actionParam.ParameterType;
                var name = actionParam.Name;

                //$this->disableRelativeTime = $actionParam->getDisableRelativeTime();

                if (type.IsPrimitive || type == typeof(string))
                {
                    if (paramsGrouped.ContainsKey(name))
                    {
                        var obj = Convert.ChangeType(paramsGrouped[name], type);
                        serviceArguments.Add(obj);

                        continue;
                    }

                    if (actionParam.IsOptional)
                    {
                        serviceArguments.Add(Type.Missing);
                        continue;
                    }
                }

                if (actionParam.ParameterType.IsEnum) // enum
                {
                    if (paramsGrouped.ContainsKey(name))
                    {
                        //XXX: We support only string enums here...

                        var eValue = Enum.Parse(actionParam.ParameterType, paramsGrouped[name].ToString());
                        serviceArguments.Add(eValue);
                        continue;
                    }

                    if (actionParam.IsOptional)
                    {
                        serviceArguments.Add(Type.Missing);
                        continue;
                    }
                }

                if (actionParam.ParameterType.IsArray || actionParam.ParameterType.IsGenericType) // array or list
                {
                    if (paramsGrouped.ContainsKey(name))
                    {
                        Type dictType = typeof(SerializableDictionary<,>);
                        object res = null;

                        //if list
                        if (type.GetGenericArguments().Count() == 1)
                        {
                            var d1 = typeof(List<>);
                            Type[] typeArgs = { type.GetGenericArguments()[0] };
                            var makeme = d1.MakeGenericType(typeArgs);
                            res = Activator.CreateInstance(makeme);

                            foreach (var kv in (Dictionary<string, object>)paramsGrouped[name])
                            {
                                ((IList)res).Add(buildObject(type.GetGenericArguments()[0], (Dictionary<string, object>)kv.Value, name, actionContext));
                            }
                        }
                        //if Dictionary
                        else if (type.GetGenericArguments().Count() == 2 &&
                            dictType.GetGenericArguments().Length == type.GetGenericArguments().Length &&
                            dictType.MakeGenericType(type.GetGenericArguments()) == type)
                        {
                            var d1 = typeof(Dictionary<,>);
                            Type[] typeArgs = { typeof(string), type.GetGenericArguments()[0] };
                            var makeme = d1.MakeGenericType(typeArgs);
                            res = Activator.CreateInstance(makeme);
                            foreach (var kv in (Dictionary<string, object>)paramsGrouped[name])
                            {
                                ((IDictionary)res).Add(name, buildObject(type.GetGenericArguments()[0], (Dictionary<string, object>)kv.Value, name, actionContext));
                            }
                        }

                        serviceArguments.Add(res);
                        continue;
                    }

                    if (actionParam.IsOptional)
                    {
                        serviceArguments.Add(Type.Missing);
                        continue;
                    }

                    if (actionParam.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        serviceArguments.Add(null);
                        continue;
                    }

                    continue;
                }

                if (paramsGrouped.ContainsKey(name)) // object 
                {
                    var res = buildObject(actionParam.ParameterType, (Dictionary<string, object>)paramsGrouped[name], name, actionContext);
                    serviceArguments.Add(res);
                    continue;
                }

                if (actionParam.IsOptional)
                {
                    serviceArguments.Add(Type.Missing);
                    continue;
                }

                //createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidActionParameters, "Missing required parameter");
            }

            return serviceArguments;
        }

        private object buildObject(Type type, Dictionary<string, object> parameters, string name, HttpActionContext actionContext)
        {
            // if objectType was specified, we will use it only if the anotation type is it's base type
            if (parameters.ContainsKey("objectType"))
            {
                var possibleType = Type.GetType(parameters["objectType"].ToString());
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
                createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.AbstractParameter, "Parameter is abstract");
                return null;
            }

            var classProperties = type.GetProperties();
            object instance = Activator.CreateInstance(type);
            foreach (var kv in parameters)
            {
                var property = classProperties.Where(x => x.Name == kv.Key).FirstOrDefault();
                if (property == null)
                {
                    continue;
                }

                PropertyInfo prop = instance.GetType().GetProperty(kv.Key, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null || !prop.CanWrite)
                {
                    //XXX: See if we want to throw something here
                    continue;
                }

                if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(string))
                {
                    prop.SetValue(instance, Convert.ChangeType(kv.Value, property.PropertyType), null);
                    continue;
                }

                if (property.PropertyType.IsEnum)
                {
                    var eValue = Enum.Parse(property.PropertyType, kv.Value.ToString());
                    prop.SetValue(instance, eValue, null);
                    continue;
                }

                if (property.PropertyType.IsArray || property.PropertyType.IsGenericType) // array or list
                {
                    Type dictType = typeof(SerializableDictionary<,>);
                    object res = null;

                    //if list
                    if (property.PropertyType.GetGenericArguments().Count() == 1)
                    {
                        res = buildObject(property.PropertyType.GetGenericArguments()[0], (Dictionary<string, object>)kv.Value, name, actionContext);

                    }
                    //if Dictionary
                    else if (property.PropertyType.GetGenericArguments().Count() == 2 &&
                        dictType.GetGenericArguments().Length == type.GetGenericArguments().Length &&
                        dictType.MakeGenericType(type.GetGenericArguments()) == property.PropertyType)
                    {
                        res = buildObject(property.PropertyType.GetGenericArguments()[0], (Dictionary<string, object>)kv.Value, name, actionContext);
                    }

                    prop.SetValue(instance, res, null);
                    continue;
                }

                //If object
                var classRes = buildObject(property.PropertyType, (Dictionary<string, object>)kv.Value, name, actionContext);
                prop.SetValue(instance, classRes, null);
                continue;

            }

            return instance;
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

        private void InitKS(HttpActionContext actionContext, string ksVal)
        {
            // the supplied ks is in KS forma (project phoenix's)
            if (IsKsFormat(ksVal))
            {
                parseKS(actionContext, ksVal);
            }
            // the supplied is in access token format (TVPAPI's)
            else
            {
                GetUserDataFromCB(actionContext, ksVal);
            }
        }

        private void GetUserDataFromCB(HttpActionContext actionContext, string ksVal)
        {
            // get token from CB
            string tokenKey = string.Format(accessTokenKeyFormat, ksVal);
            ApiToken token = cbManager.Get<ApiToken>(tokenKey);

            if (token == null)
            {
                createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidKS, "Invalid KS");
                return;
            }

            KS ks = KS.CreateKSFromApiToken(token);

            if (!ks.IsValid)
            {
                createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidKS, "Invalid KS");
                return;
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

        private static void parseKS(HttpActionContext actionContext, string ksVal)
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
                createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidKS, "Invalid KS format");
                return;
            }

            if (ksParts.Length < 3 || ksParts[0] != "v2" || !int.TryParse(ksParts[1], out groupId))
            {
                createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidKS, "Invalid KS format");
                return;
            }

            Group group = null;
            try
            {
                // get group secret
                group = GroupsManager.GetGroup(groupId);
            }
            catch (ApiException ex)
            {
                createErrorResponse(actionContext, (int)ex.Code, ex.Message);
                return;
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