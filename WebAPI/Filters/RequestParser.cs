using ConfigurationManager;
using KLogMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebAPI.ClientManagers;
using WebAPI.Controllers;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Billing;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.MultiRequest;
using WebAPI.Reflection;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace WebAPI.Filters
{
    public class RequestParserException : BadRequestException
    {
        public static ApiExceptionType INVALID_MULTIREQUEST_TOKEN = new ApiExceptionType(StatusCode.InvalidMultirequestToken, "Invalid multirequest token");

        public static ApiExceptionType INVALID_OBJECT_TYPE = new ApiExceptionType(StatusCode.InvalidObjectType, "Invalid object type [@type@]", "type");
        public static ApiExceptionType ABSTRACT_PARAMETER = new ApiExceptionType(StatusCode.AbstractParameter, "Abstract parameter type [@type@]", "type");
        public static ApiExceptionType MISSING_PARAMETER = new ApiExceptionType(StatusCode.MissingParameter, StatusCode.InvalidActionParameters, "Missing parameter [@parameter@]", "parameter");
        public static ApiExceptionType INDEX_NOT_ZERO_BASED = new ApiExceptionType(StatusCode.MultirequestIndexNotZeroBased, StatusCode.InvalidMultirequestToken, "Invalid multirequest token, response index is not zero based");
        public static ApiExceptionType INVALID_INDEX = new ApiExceptionType(StatusCode.MultirequestInvalidIndex, StatusCode.InvalidMultirequestToken, "Invalid multirequest token, invalid response index");
        public static ApiExceptionType GENERIC_METHOD = new ApiExceptionType(StatusCode.MultirequestGenericMethod, StatusCode.InvalidService, "Invalid multirequest service, invalid service: [@service@], action: [@action@]", "service", "action");

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
        private const string UPLOAD_FOLDER = "C:\\tmp\\tvp-api\\"; // TODO take from configuration

        private static int accessTokenLength = ApplicationConfiguration.RequestParserConfiguration.AccessTokenLength.IntValue;
        private static string accessTokenKeyFormat = ApplicationConfiguration.RequestParserConfiguration.AccessTokenKeyFormat.Value;

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

        // same key as in REST solution KLogMonitor.Constants
        // in-case changing this  - you must change there  as well
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
        public const string REQUEST_RESPONSE_PROFILE = "responseProfile";


        public static object GetRequestPayload()
        {
            return HttpContext.Current.Items[REQUEST_METHOD_PARAMETERS];
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
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString[REQUEST_FORMAT]))
            {
                HttpContext.Current.Items.Add(REQUEST_FORMAT, HttpContext.Current.Request.QueryString[REQUEST_FORMAT]);
            }

            if (HttpContext.Current.Items[REQUEST_TYPE] != null)
                HttpContext.Current.Items.Remove(REQUEST_TYPE);

            if (action != null)
            {
                switch (action)
                {
                    case "register":
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
            // response profile
            if (requestParams.ContainsKey("responseProfile") && requestParams["responseProfile"] != null)
            {

                //If object
                KalturaOTTObject responseProfile = null;
                Type type = typeof(KalturaBaseResponseProfile);
                if (requestParams["responseProfile"] is JObject)
                {
                    responseProfile = Deserializer.deserialize(type,
                        ((JObject)requestParams["responseProfile"]).ToObject<Dictionary<string, object>>());
                }
                else if (requestParams["responseProfile"] is Dictionary<string, object>)
                {                    
                     responseProfile = Deserializer.deserialize(type,
                        ((JObject)requestParams["responseProfile"]).ToObject<Dictionary<string, object>>());
                }

                if (globalScope && HttpContext.Current.Items[REQUEST_RESPONSE_PROFILE] == null)
                    HttpContext.Current.Items.Add(REQUEST_RESPONSE_PROFILE, responseProfile);
            }
            else if (HttpContext.Current.Items[REQUEST_RESPONSE_PROFILE] != null)
            {
                HttpContext.Current.Items.Add(REQUEST_RESPONSE_PROFILE, HttpContext.Current.Items[REQUEST_RESPONSE_PROFILE]);
            }
        }

        private bool Equals(byte[] source, byte[] separator, int index)
        {
            for (int i = 0; i < separator.Length; ++i)
            {
                if (index + i >= source.Length || source[index + i] != separator[i])
                {
                    return false;
                }
            }
            return true;
        }

        public KeyValuePair<string, object> ParseFormField(byte[] fieldBytes)
        {
            Regex rgxName = new Regex("Content-Disposition: form-data; name=\"?([^\" ]+)\"?", RegexOptions.IgnoreCase);
            Regex rgxFileName = new Regex("filename=\"?([^\" ]+)\"?", RegexOptions.IgnoreCase);

            string fieldStr = Encoding.UTF8.GetString(fieldBytes);
            string[] fieldLines = fieldStr.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string name = null;
            string fileName = null;
            
            int index = 0;
            foreach (string fieldLine in fieldLines)
            {
                if (fieldLine.StartsWith("Content-"))
                {
                    MatchCollection matches = rgxName.Matches(fieldLine);
                    if (matches.Count > 0)
                    {
                        name = matches[0].Groups[1].Value;

                        matches = rgxFileName.Matches(fieldLine);
                        if (matches.Count > 0)
                        {
                            fileName = matches[0].Groups[1].Value;
                        }
                    }
                    index += fieldLine.Length + 2; // \r\n
                }
                else
                {
                    break;
                }
            }
            index += 2; // \r\n

            if (fileName == null)
            {
                return new KeyValuePair<string, object>(name, Encoding.UTF8.GetString(fieldBytes, index, fieldBytes.Length - index));
            }
            else
            {
                string filePath = String.Format("{0}\\{1}", UPLOAD_FOLDER, Path.GetRandomFileName());
                byte[] bytes = new byte[fieldBytes.Length - index - 2];  // \r\n
                Array.Copy(fieldBytes, index, bytes, 0, bytes.Length);
                File.WriteAllBytes(filePath, bytes);

                return new KeyValuePair<string, object>(name, new KalturaOTTFile(filePath));
            }

            return new KeyValuePair<string, object>("aaa", "bb");
        }

        public async Task<Dictionary<string, object>> ParseFormData(HttpActionContext actionContext)
        {
            if (actionContext.Request.Content.IsMimeMultipartContent())
            {
                if (!Directory.Exists(UPLOAD_FOLDER))
                {
                    Directory.CreateDirectory(UPLOAD_FOLDER);
                }

                Dictionary<string, object> ret = new Dictionary<string, object>();

                byte[] requestBody = (byte[]) HttpContext.Current.Items["body"];
                string body = Encoding.UTF8.GetString(requestBody);
                string boundery = body.Substring(0, body.IndexOf("\r") + 2);
                byte[] bounderyBytes = Encoding.UTF8.GetBytes(boundery);
                int index = 0;
                int length;
                byte[] fieldBytes;
                for (int i = 0; i < requestBody.Length; ++i)
                {
                    if (Equals(requestBody, bounderyBytes, i))
                    {
                        length = i - index;
                        if (length > 0)
                        {
                            fieldBytes = new byte[length];
                            Array.Copy(requestBody, index, fieldBytes, 0, fieldBytes.Length);
                            KeyValuePair<string, object> keyValue = ParseFormField(fieldBytes);
                            ret.Add(keyValue.Key, keyValue.Value);
                        }
                        index = i + bounderyBytes.Length;
                        i += bounderyBytes.Length - 1;
                    }
                }
                length = requestBody.Length - index;
                if (length > 0)
                {
                    fieldBytes = new byte[requestBody.Length - index];
                    Array.Copy(requestBody, index, fieldBytes, 0, fieldBytes.Length);
                    KeyValuePair<string, object> keyValue = ParseFormField(fieldBytes);
                    ret.Add(keyValue.Key, keyValue.Value);
                }

                /*
                 * This code works perfect against noed.js, php and postman clients but fails against C# client
                 * 
                MultipartMemoryStreamProvider streamProvider = await actionContext.Request.Content.ReadAsMultipartAsync();
                foreach (StreamContent field in streamProvider.Contents)
                {
                    string name = field.Headers.ContentDisposition.Name.Trim(new char[]{'"'});

                    if (field.Headers.ContentDisposition.FileName == null)
                    {
                        ret.Add(name, await field.ReadAsStringAsync());
                    }
                    else
                    {
                        string filePath = String.Format("{0}\\{1}", UPLOAD_FOLDER, Path.GetRandomFileName());

                        Stream stream = await field.ReadAsStreamAsync();
                        var fileStream = File.Create(filePath);
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(fileStream);
                        fileStream.Close();

                        //byte[] bytes = await field.ReadAsByteArrayAsync();
                        //File.WriteAllBytes(filePath, bytes);

                        ret.Add(name, new KalturaOTTFile(filePath));
                    }
                }
                 * */

                return ret;
            }
            else
            {   
                string query = await actionContext.Request.Content.ReadAsStringAsync();
                NameValueCollection values = HttpUtility.ParseQueryString(query);

                Dictionary<string, object> ret = new Dictionary<string, object>();
                foreach (string key in values.Keys)
                {
                    if (key != null)
                    {
                        ret.Add(key, values[key]);
                    }
                }
                if (ret.Count > 0)
                {
                    return ret;
                }
            }

            return null;
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

            Dictionary<string, object> formData = null;

            if (actionContext.Request.Method == HttpMethod.Options)
            {
                actionContext.Response = actionContext.Request.CreateResponse();
                actionContext.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Range, Cache-Control");
                actionContext.Response.Headers.Add("Access-Control-Allow-Methods", " POST, GET, HEAD, OPTIONS");
                actionContext.Response.Headers.Add("Access-Control-Expose-Headers", "Server, Content-Length, Content-Range, Date");

                return;
            }

            if (actionContext.Request.Method == HttpMethod.Post)
            {
                formData = await ParseFormData(actionContext);
            }

            var rd = actionContext.ControllerContext.RouteData;
            if (rd.Values.ContainsKey("service_name"))
            {
                currentController = rd.Values["service_name"].ToString();
                if (rd.Values.ContainsKey("action_name"))
                {
                    currentAction = rd.Values["action_name"].ToString();
                }

                if (rd.Values.ContainsKey("pathData") && rd.Values["pathData"] != null)
                {
                    pathData = rd.Values["pathData"].ToString();
                }
            }
            else if(formData != null)
            {
                currentController = formData["service"].ToString();
                currentAction = formData["action"].ToString();
                pathData = formData["pathData"].ToString();
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
            
            if (actionContext.Request.Method == HttpMethod.Post)
            {
                string json = null;

                if (actionContext.Request.Content.IsMimeMultipartContent() && formData != null && formData.ContainsKey("json"))
                {
                    json = formData["json"].ToString();
                }
                else if (HttpContext.Current.Request.ContentType == "application/json" && HttpContext.Current.Request.ContentLength > 0)
                {
                    string result = await actionContext.Request.Content.ReadAsStringAsync();
                    using (var input = new StringReader(result))
                    {
                        json = input.ReadToEnd();
                    }
                }

                if(json != null)
                {
                    try
                    {
                        JObject reqParams = JObject.Parse(json);

                        if (string.IsNullOrEmpty((string)HttpContext.Current.Items[Constants.CLIENT_TAG]))
                        {
                            //For logging
                            HttpContext.Current.Items.Remove(Constants.CLIENT_TAG);
                            if (reqParams["clientTag"] != null)
                            {
                                HttpContext.Current.Items.Add(Constants.CLIENT_TAG, reqParams["clientTag"]);
                            }
                            else if (HttpContext.Current.Request.QueryString.Count > 0 && HttpContext.Current.Request.QueryString["clientTag"] != null)
                            {
                                HttpContext.Current.Items.Add(Constants.CLIENT_TAG, HttpContext.Current.Request.QueryString["clientTag"]);
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

                        List<Object> methodParams;
                        if (currentController == "multirequest")
                        {
                            methodParams = buildMultirequestActions(requestParams);
                        }
                        else
                        {
                            Dictionary<string, MethodParam> methodArgs = DataModel.getMethodParams(currentController, currentAction);
                            methodParams = RequestParser.buildActionArguments(methodArgs, requestParams);
                        }
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

                        List<Object> methodParams;
                        if (currentController == "multirequest")
                        {
                            methodParams = buildMultirequestActions(groupedParams);
                        }
                        else
                        {
                            Dictionary<string, MethodParam> methodArgs = DataModel.getMethodParams(currentController, currentAction);
                            methodParams = RequestParser.buildActionArguments(methodArgs, groupedParams);
                        }
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
                if (string.IsNullOrEmpty((string)HttpContext.Current.Items[Constants.CLIENT_TAG]))
                {
                    if (HttpContext.Current.Request.QueryString.Count > 0 && HttpContext.Current.Request.QueryString["clientTag"] != null)
                    {
                        HttpContext.Current.Items[Constants.CLIENT_TAG] = HttpContext.Current.Request.QueryString["clientTag"];
                    }
                }

                if (string.IsNullOrEmpty((string)HttpContext.Current.Items["playSessionId"]))
                {
                    if (HttpContext.Current.Request.QueryString.Count > 0 && HttpContext.Current.Request.QueryString["playSessionId"] != null)
                    {
                        HttpContext.Current.Items[Constants.CLIENT_TAG] = HttpContext.Current.Request.QueryString["playSessionId"];
                    }
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

                        List<Object> methodParams;
                        if (currentController == "multirequest")
                        {
                            methodParams = buildMultirequestActions(groupedParams);
                        }
                        else
                        {
                            Dictionary<string, MethodParam> methodArgs = DataModel.getMethodParams(currentController, currentAction);
                            methodParams = RequestParser.buildActionArguments(methodArgs, groupedParams);
                        }
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

        public static List<object> buildActionArguments(Dictionary<string, MethodParam> methodArgs, Dictionary<string, object> reqParams)
        {
            List<Object> methodParams = new List<object>();
            foreach (KeyValuePair<string, MethodParam> methodArgItem in methodArgs)
            {
                string name = methodArgItem.Key;
                MethodParam methodArg = methodArgItem.Value;

                if (!reqParams.ContainsKey(name) || reqParams[name] == null)
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

                try
                {
                    object value = null;

                    // If it is an enum, newtonsoft's bad "ToObject" doesn't do this easily, 
                    // so we do it ourselves in this not so good looking way
                    if (methodArg.IsEnum)
                    {
                        if (reqParams[name] != null)
                        {
                            string paramAsString = reqParams[name].ToString();
                            if (paramAsString != null)
                            {
                                value = Enum.Parse(methodArg.Type, paramAsString, true);
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

                        string service = Convert.ToString(HttpContext.Current.Items[REQUEST_SERVICE]);
                        string action = Convert.ToString(HttpContext.Current.Items[REQUEST_ACTION]);
                        string language = Convert.ToString(HttpContext.Current.Items[REQUEST_LANGUAGE]);
                        string userId = Convert.ToString(HttpContext.Current.Items[REQUEST_USER_ID]);
                        string deviceId = KSUtils.ExtractKSPayload().UDID;
                        int groupId = Convert.ToInt32(HttpContext.Current.Items[REQUEST_GROUP_ID]);

                        object ksObject = HttpContext.Current.Items[REQUEST_KS];
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

        private Dictionary<string, object> groupParams(Dictionary<string, object> tokens)
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

            KS ks = KS.CreateKSFromApiToken(token, ksVal);

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