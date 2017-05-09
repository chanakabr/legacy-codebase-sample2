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
using WebAPI.Managers.Scheme;
using System.Runtime.Serialization;
using WebAPI.Models.Billing;
using WebAPI.Models.MultiRequest;
using System.Collections.Specialized;
using System.Threading.Tasks;
using WebAPI.Reflection;

namespace WebAPI.Filters
{
    public class RequestParserException : BadRequestException
    {
        public static ApiExceptionType INVALID_MULTIREQUEST_TOKEN = new ApiExceptionType(StatusCode.InvalidMultirequestToken, "Invalid multirequest token");

        public static ApiExceptionType ABSTRACT_PARAMETER = new ApiExceptionType(StatusCode.AbstractParameter, "Abstract parameter type [@type@]", "type");
        public static ApiExceptionType MISSING_PARAMETER = new ApiExceptionType(StatusCode.MissingParameter, StatusCode.InvalidActionParameters, "Missing parameter [@parameter@]", "parameter");
        public static ApiExceptionType INDEX_NOT_ZERO_BASED = new ApiExceptionType(StatusCode.MultirequestIndexNotZeroBased, StatusCode.InvalidMultirequestToken, "Invalid multirequest token, response index is not zero based");
        public static ApiExceptionType INVALID_INDEX = new ApiExceptionType(StatusCode.MultirequestInvalidIndex, StatusCode.InvalidMultirequestToken, "Invalid multirequest token, invalid response index");

        public RequestParserException()
            : this(INVALID_MULTIREQUEST_TOKEN)
        {
        }

        public RequestParserException(ApiExceptionType type, params object[] parameters)
            : base(type, parameters)
        {
        }

        public RequestParserException(ApiException ex) : base(ex)
        {
        }
    }

    public enum RequestType
    {
        READ = 1,
        INSERT = 2,
        UPDATE = 4,
        WRITE = 6,
        ALL = 7
    }

    public class RequestParser : ActionFilterAttribute
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const char PARAMS_PREFIX = ':';
        private const char PARAMS_NESTED_PREFIX = '.';
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
        public const string REQUEST_VERSION = "requestVersion";
        public const string REQUEST_USER_ID = "user_id";
        public const string REQUEST_GROUP_ID = "group_id";
        public const string REQUEST_KS = "KS";
        public const string REQUEST_LANGUAGE = "language";
        public const string REQUEST_CURRENCY = "currency";
        public const string REQUEST_FORMAT = "format";
        public const string REQUEST_GLOBAL_KS = "global_ks";
        public const string REQUEST_GLOBAL_USER_ID = "global_user_id";
        public const string REQUEST_GLOBAL_LANGUAGE = "global_language";
        public const string REQUEST_GLOBAL_CURRENCY = "global_currency";
        public const string REQUEST_SERVICE = "requestService";
        public const string REQUEST_ACTION = "requestAction";
        public const string REQUEST_TIME = "requestTime";
        public const string REQUEST_TYPE = "requestType";
        public const string REQUEST_SERVE_CONTENT_TYPE = "requestServeContentType";
        public const string REQUEST_PATH_DATA = "pathData";

        public static object GetRequestPayload()
        {
            return HttpContext.Current.Items[REQUEST_METHOD_PARAMETERS];
        }

        public static MethodInfo createMethodInvoker(string serviceName, string actionName, Assembly asm, bool validateAuthorization = true)
        {
            MethodInfo methodInfo = null;
            Type controller = asm.GetType(string.Format("WebAPI.Controllers.{0}Controller", serviceName), false, true);

            methodInfo = null;

            if (controller == null)
            {
                throw new RequestParserException(RequestParserException.INVALID_SERVICE, serviceName);
            }

            Dictionary<string, string> oldStandardActions = OldStandardAttribute.getOldMembers(controller);
            string action = actionName;

            if (serviceName.Equals("multirequest", StringComparison.CurrentCultureIgnoreCase))
            {
                action = "Do";
            }
            else
            {
                string lowerActionName = actionName.ToLower();
                if (oldStandardActions != null && oldStandardActions.ContainsValue(lowerActionName))
                {
                    action = oldStandardActions.FirstOrDefault(value => value.Value == lowerActionName).Key;
                }
            }

            methodInfo = controller.GetMethod(action, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (methodInfo == null)
            {
                throw new RequestParserException(RequestParserException.INVALID_ACTION, serviceName, actionName);
            }

            if (validateAuthorization)
            {
                DataModel.validateAuthorization(methodInfo, serviceName, actionName);
            }

            string contentType = DataModel.getServeActionContentType(methodInfo);
            if (contentType != null)
            {
                HttpContext.Current.Items[REQUEST_SERVE_CONTENT_TYPE] = contentType;
            }

            return methodInfo;
        }

        public static void setRequestContext(Dictionary<string, object> requestParams, string service, string action, bool globalScope = true)
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

                if (globalScope && HttpContext.Current.Items[REQUEST_GLOBAL_KS] == null)
                    HttpContext.Current.Items.Add(REQUEST_GLOBAL_KS, ks);
            }
            else if (HttpContext.Current.Items[REQUEST_GLOBAL_KS] != null)
            {
                InitKS((string)HttpContext.Current.Items[REQUEST_GLOBAL_KS]);
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
                if (globalScope && HttpContext.Current.Items[REQUEST_GLOBAL_USER_ID] == null)
                    HttpContext.Current.Items.Add(REQUEST_GLOBAL_USER_ID, userId);
            }
            else if (HttpContext.Current.Items[REQUEST_GLOBAL_USER_ID] != null)
            {
                HttpContext.Current.Items.Add(REQUEST_USER_ID, HttpContext.Current.Items[REQUEST_GLOBAL_USER_ID]);
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
                if (globalScope && HttpContext.Current.Items[REQUEST_GLOBAL_LANGUAGE] == null)
                    HttpContext.Current.Items.Add(REQUEST_GLOBAL_LANGUAGE, language);
            }
            else if (HttpContext.Current.Items[REQUEST_GLOBAL_LANGUAGE] != null)
            {
                HttpContext.Current.Items.Add(REQUEST_LANGUAGE, HttpContext.Current.Items[REQUEST_GLOBAL_LANGUAGE]);
            }

            // currency
            HttpContext.Current.Items.Remove(REQUEST_CURRENCY);
            if (requestParams.ContainsKey("currency") && requestParams["currency"] != null)
            {
                string currency;
                if (requestParams["currency"].GetType() == typeof(JObject) || requestParams["currency"].GetType().IsSubclassOf(typeof(JObject)))
                {
                    currency = ((JObject)requestParams["currency"]).ToObject<string>();
                }
                else
                {
                    currency = (string)Convert.ChangeType(requestParams["currency"], typeof(string));
                }

                HttpContext.Current.Items.Add(REQUEST_CURRENCY, currency);
                if (globalScope && HttpContext.Current.Items[REQUEST_GLOBAL_CURRENCY] == null)
                    HttpContext.Current.Items.Add(REQUEST_GLOBAL_CURRENCY, currency);
            }
            else if (HttpContext.Current.Items[REQUEST_GLOBAL_CURRENCY] != null)
            {
                HttpContext.Current.Items.Add(REQUEST_CURRENCY, HttpContext.Current.Items[REQUEST_GLOBAL_CURRENCY]);
            }

            // format                        
            if (!string.IsNullOrEmpty( HttpContext.Current.Request.QueryString[REQUEST_FORMAT]))
            {
                HttpContext.Current.Items.Add(REQUEST_FORMAT, HttpContext.Current.Request.QueryString[REQUEST_FORMAT]);                
            }            

            if (HttpContext.Current.Items[REQUEST_TYPE] != null)
                HttpContext.Current.Items.Remove(REQUEST_TYPE);

            if (action != null)
            {
                switch (action)
                {
                    case "add":
                        HttpContext.Current.Items[REQUEST_TYPE] = RequestType.INSERT;
                        break;

                    case "update":
                        HttpContext.Current.Items[REQUEST_TYPE] = RequestType.UPDATE;
                        break;

                    case "get":
                    case "list":
                        HttpContext.Current.Items[REQUEST_TYPE] = RequestType.READ;
                        break;

                    default:
                        break;
                }
            }
        }

        public async Task<NameValueCollection> ParseFormData(HttpActionContext actionContext)
        {
            if (!actionContext.Request.Content.IsMimeMultipartContent("form-data"))
            {
                string query = await actionContext.Request.Content.ReadAsStringAsync();
                return HttpUtility.ParseQueryString(query);
            }

            string uploadFolder = "C:\\"; // TODO take from configuration
            MultipartFormDataStreamProvider streamProvider = new MultipartFormDataStreamProvider(uploadFolder);
            MultipartFileStreamProvider multipartFileStreamProvider = await actionContext.Request.Content.ReadAsMultipartAsync(streamProvider);

            // Get the file names.
            foreach (MultipartFileData file in streamProvider.FileData)
            {
                //Do something awesome with the files..
            }

            return streamProvider.FormData;
        }

        public async override void OnActionExecuting(HttpActionContext actionContext)
        {
            string currentAction = null;
            string currentController = null;
            string pathData = null;

            // version request
            if (actionContext.Request.GetRouteData().Route.RouteTemplate.Equals("api_v3/version"))
            {
                base.OnActionExecuting(actionContext);
                return;
            }

            HttpContext.Current.Items[REQUEST_TIME] = DateTime.UtcNow;

            NameValueCollection formData = null;

            if (actionContext.Request.Method == HttpMethod.Options)
            {
                actionContext.Response = actionContext.Request.CreateResponse();
                actionContext.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Range, Cache-Control");
                actionContext.Response.Headers.Add("Access-Control-Allow-Methods", " POST, GET, HEAD, OPTIONS");
                actionContext.Response.Headers.Add("Access-Control-Expose-Headers", "Server, Content-Length, Content-Range, Date");

                return;
            }

            var rd = actionContext.ControllerContext.RouteData;
            if (actionContext.Request.Method == HttpMethod.Options)
            {
                actionContext.Response = actionContext.Request.CreateResponse();
                actionContext.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Range, Cache-Control");
                actionContext.Response.Headers.Add("Access-Control-Allow-Methods", " POST, GET, HEAD, OPTIONS");
                actionContext.Response.Headers.Add("Access-Control-Expose-Headers", "Server, Content-Length, Content-Range, Date");

                return;
            }

            if (rd.Values.ContainsKey("service_name"))
            {
                currentController = rd.Values["service_name"].ToString();
                if (rd.Values.ContainsKey("action_name"))
                {
                    currentAction = rd.Values["action_name"].ToString();
                }

                if (rd.Values.ContainsKey("pathData"))
                {
                    pathData = rd.Values["pathData"].ToString();
                }
            }
            else if (actionContext.Request.Method == HttpMethod.Post)
            {
                formData = await ParseFormData(actionContext);
                currentController = formData.Get("service");
                currentAction = formData.Get("action");
                pathData = formData.Get("pathData");
            }
            else if (actionContext.Request.Method == HttpMethod.Get)
            {
                var pair = actionContext.Request.GetQueryNameValuePairs().Where(x => x.Key.ToLower() == "service");
                if (pair != null)
                {
                    currentController = pair.FirstOrDefault().Value;
                    pair = actionContext.Request.GetQueryNameValuePairs().Where(x => x.Key.ToLower() == "action");
                    if (pair != null)
                    {
                        currentAction = pair.FirstOrDefault().Value;
                    }
                }
                pair = actionContext.Request.GetQueryNameValuePairs().Where(x => x.Key.ToLower() == "pathData");
                if (pair != null)
                {
                    pathData = pair.FirstOrDefault().Value;
                }
            }
            else
            {
                createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.BadRequest, "HTTP Method not supported");
                return;
            }

            if(currentController == null && pathData == null)
            {
                createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidService, "Unknown Service");
                return;
            }

            if (currentController != null)
            {
                HttpContext.Current.Items[REQUEST_SERVICE] = currentController;
                if (currentAction != null)
                {
                    HttpContext.Current.Items[REQUEST_ACTION] = currentAction;
                }
            }

            if (pathData != null)
            {
                HttpContext.Current.Items[REQUEST_PATH_DATA] = pathData;
            }

            MethodInfo methodInfo = null;
            Assembly asm = Assembly.GetExecutingAssembly();

            if (actionContext.Request.Method == HttpMethod.Post)
            {
                string result = await actionContext.Request.Content.ReadAsStringAsync();
                if (HttpContext.Current.Request.ContentType == "application/json" && HttpContext.Current.Request.ContentLength > 0)
                {
                    using (var input = new StringReader(result))
                    {
                        try
                        {
                            JObject reqParams = JObject.Parse(input.ReadToEnd());

                            if (string.IsNullOrEmpty((string)HttpContext.Current.Items[Constants.CLIENT_TAG])) 
                            {
                                //For logging
                                if (reqParams["clientTag"] != null)
                                {
                                    HttpContext.Current.Items[Constants.CLIENT_TAG] = reqParams["clientTag"];
                                }
                                else if (HttpContext.Current.Request.QueryString.Count > 0 && HttpContext.Current.Request.QueryString["clientTag"] != null)
                                {
                                    HttpContext.Current.Items[Constants.CLIENT_TAG] = HttpContext.Current.Request.QueryString["clientTag"];
                                }
                            }

                            if (string.IsNullOrEmpty((string)HttpContext.Current.Items["playSessionId"]))
                            {
                                if (reqParams["playSessionId"] != null)
                                {
                                    HttpContext.Current.Items["playSessionId"] = reqParams["playSessionId"];
                                }
                                else if (HttpContext.Current.Request.QueryString.Count > 0 && HttpContext.Current.Request.QueryString["playSessionId"] != null)
                                {
                                    HttpContext.Current.Items["playSessionId"] = HttpContext.Current.Request.QueryString["playSessionId"];
                                }
                            }

                            if (reqParams["apiVersion"] != null)
                            {
                                //For logging and to parse old standard
                                Version version;
                                if (!Version.TryParse((string)reqParams["apiVersion"], out version))
                                    throw new RequestParserException(RequestParserException.INVALID_VERSION, reqParams["apiVersion"]);

                                HttpContext.Current.Items[REQUEST_VERSION] = version;
                            }

                            Dictionary<string, object> requestParams;
                            if (HttpContext.Current.Items[REQUEST_PATH_DATA] != null)
                            {
                                requestParams = groupPathDataParams((string)HttpContext.Current.Items[REQUEST_PATH_DATA]);
                            }
                            else
                            {
                                requestParams = reqParams.ToObject<Dictionary<string, object>>();
                            }
                            setRequestContext(requestParams, currentController, currentAction);
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
                        catch (ApiException e)
                        {
                            createErrorResponse(actionContext, (int)e.Code, e.Message);
                            return;
                        }
                        catch (JsonReaderException)
                        {
                            createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidJSONRequest, "Invalid JSON");
                            return;
                        }
                        catch (FormatException)
                        {
                            createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidJSONRequest, "Invalid JSON");
                            return;
                        }
                    }
                }
                else if ((HttpContext.Current.Request.ContentType == "text/xml" || HttpContext.Current.Request.ContentType == "application/xml") 
                    && HttpContext.Current.Request.ContentLength > 0)
                {
                    //TODO
                    createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.BadRequest, "XML is currently not supported");
                    return;
                }
                else if (formData != null)
                {
                    try
                    {
                        Dictionary<string, object> groupedParams;
                        if (HttpContext.Current.Items[REQUEST_PATH_DATA] != null)
                        {
                            groupedParams = groupPathDataParams((string)HttpContext.Current.Items[REQUEST_PATH_DATA]);
                        }
                        else
                        {
                            //Running on the expected method parameters
                            groupedParams = groupParams(formData);
                        }

                        setRequestContext(groupedParams, currentController, currentAction);
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
                    catch (ApiException e)
                    {
                        createErrorResponse(actionContext, (int)e.Code, e.Message);
                        return;
                    }
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

                if (tokens.ContainsKey("apiVersion"))
                {
                    Version version;
                    if (!Version.TryParse(tokens["apiVersion"], out version))
                        throw new RequestParserException(RequestParserException.INVALID_VERSION, tokens["apiVersion"]);

                    HttpContext.Current.Items[REQUEST_VERSION] = version;
                }
                if (currentController != null)
                {
                    try
                    {
                        Dictionary<string, object> groupedParams;
                        if (HttpContext.Current.Items[REQUEST_PATH_DATA] != null)
                        {
                            groupedParams = groupPathDataParams((string)HttpContext.Current.Items[REQUEST_PATH_DATA]);
                        }
                        else
                        {
                            //Running on the expected method parameters
                            groupedParams = groupParams(tokens);
                        }

                        setRequestContext(groupedParams, currentController, currentAction);
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
                    catch (ApiException e)
                    {
                        createErrorResponse(actionContext, (int)e.Code, e.Message);
                        return;
                    }
                }
            }

            base.OnActionExecuting(actionContext);
        }

        private static List<object> buildMultirequestActions(Dictionary<string, object> requestParams)
        {
            List<KalturaMultiRequestAction> requests = new List<KalturaMultiRequestAction>();
            Dictionary<string, object> currentRequestParams;
            
            int requestIndex = 0;
            foreach (string index in requestParams.Keys)
            {
                if (!int.TryParse(index, out requestIndex))
                    continue;

                var requestItem = requestParams[index];
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

            IEnumerable<SchemeArgumentAttribute> schemaArguments = methodInfo.GetCustomAttributes<SchemeArgumentAttribute>();
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

                    throw new RequestParserException(RequestParserException.MISSING_PARAMETER, p.Name);
                }

                try
                {
                    Type t = p.ParameterType;
                    object value = null;
                    
                    // If it is an enum, newtonsoft's bad "ToObject" doesn't do this easily, 
                    // so we do it ourselves in this not so good looking way
                    Type u = Nullable.GetUnderlyingType(t);
                    if (t.IsEnum)
                    {
                        var paramAsString = reqParams[name].ToString();
                        var names = t.GetEnumNames().ToList();
                        value = Enum.Parse(t, paramAsString, true);
                    }
                    // nullable enum
                    else if (u != null && u.IsEnum)
                    {
                        var paramAsString = reqParams[name] != null ? reqParams[name].ToString() : null;
                        if (paramAsString != null)
                        {
                            value = Enum.Parse(u, paramAsString, true);
                        }
                    }
                    else if (t.IsSubclassOf(typeof(KalturaOTTObject)))
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
                        value = res;
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
                                else if (t == typeof(DateTime))
                                {
                                    long unixTimeStamp = (long) reqParams[name];
                                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                                    res = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
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
                        value = res;
                    }
                    else if (reqParams[name] != null)
                    {
                        if (reqParams[name].GetType() == typeof(JObject) || reqParams[name].GetType().IsSubclassOf(typeof(JObject)))
                        {
                            value = ((JObject)reqParams[name]).ToObject(t);
                            if (t == typeof(DateTime))
                            {
                                long unixTimeStamp = (long) value;
                                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                                value = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
                            }
                        }
                        else if (t == typeof(DateTime))
                        {
                            long unixTimeStamp = (long) reqParams[name];
                            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                            value = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
                        }
                        else
                        {
                            value = Convert.ChangeType(reqParams[name], t);
                        }
                    }

                    foreach (SchemeArgumentAttribute schemaArgument in schemaArguments)
                    {
                        if (schemaArgument.Name.Equals(name))
                            schemaArgument.Validate(methodInfo, name, value);
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
                    throw new RequestParserException(RequestParserException.INVALID_ACTION_PARAMETER, p.Name);
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
                string[] path = kv.Key.Replace(PARAMS_NESTED_PREFIX, PARAMS_PREFIX).Split(PARAMS_PREFIX);
                setElementByPath(paramsDic, path.ToList(), kv.Value);
            }

            return paramsDic;
        }

        private Dictionary<string, object> groupParams(NameValueCollection tokens)
        {
            Dictionary<string, object> paramsDic = new Dictionary<string, object>();

            // group the params by prefix
            foreach (string key in tokens.Keys)
            {
                string[] path = key.Replace(PARAMS_NESTED_PREFIX, PARAMS_PREFIX).Split(PARAMS_PREFIX);
                setElementByPath(paramsDic, path.ToList(), tokens.Get(key));
            }

            return paramsDic;
        }

        private Dictionary<string, object> groupPathDataParams(string pathData)
        {
            Dictionary<string, object> paramsDic = new Dictionary<string, object>();

            string[] pathDataTokens = pathData.Split(new char[] { '/' }, StringSplitOptions.None);
            string key = null;
            for (int i = 0; i < pathDataTokens.Length; i++)
            {
                if (i % 2 == 0)
                {
                    key = pathDataTokens[i];
                }
                else
                {
                    paramsDic.Add(key, pathDataTokens[i]);
                }
            }

            return paramsDic;
        }

        private static string getApiName(PropertyInfo property)
        {
            return DataModel.getApiName(property);
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
                throw new RequestParserException(RequestParserException.ABSTRACT_PARAMETER, type.Name);
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

                SchemePropertyAttribute schemaProperty = property.GetCustomAttribute<SchemePropertyAttribute>();
                if (schemaProperty != null)
                    schemaProperty.Validate(type.Name, parameterName, parameters[parameterName]);

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
                            else if (underlyingType == typeof(DateTime))
                            {
                                long unixTimeStamp = (long) parameters[parameterName];
                                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                                res = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
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
                KalturaOTTObject classRes = null;
                if (parameters[parameterName] is JObject)
                {
                    classRes = buildObject(property.PropertyType, ((JObject)parameters[parameterName]).ToObject<Dictionary<string, object>>());
                }
                else if (parameters[parameterName] is Dictionary<string, object>)
                {
                    classRes = buildObject(property.PropertyType, (Dictionary<string, object>)parameters[parameterName]);
                }
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

                    list.Add((dynamic)itemObject);
                }
                else
                {
                    //list.Add((dynamic)item);
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
                throw new RequestParserException(RequestParserException.INVALID_KS_FORMAT);
            }

            KS ks = KS.CreateKSFromApiToken(token);

            if (!ks.IsValid)
            {
                throw new RequestParserException(RequestParserException.INVALID_KS_FORMAT);
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

            actionContext.Response.StatusCode = HttpStatusCode.OK;

            IEnumerable<FailureHttpCodeAttribute> failureHttpCode = actionContext.ActionDescriptor.GetCustomAttributes<FailureHttpCodeAttribute>();
            if (failureHttpCode != null && failureHttpCode.Count() > 0)
            {
                actionContext.Response.Headers.Add("X-Kaltura-App", string.Format("exiting on error {0} - {1}", errorCode, msg));
                actionContext.Response.Headers.Add("X-Kaltura", string.Format("error-{0}", errorCode));
                actionContext.Response.StatusCode = failureHttpCode.First().HttpStatusCode;
            }
        }

        private static void parseKS(string ksVal)
        {
            KS ks = KS.ParseKS(ksVal);
            ks.SaveOnRequest();
        }

        private static bool IsKsFormat(string ksVal)
        {
            return ksVal.Length > accessTokenLength;
        }
    }
}