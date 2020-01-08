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
using KlogMonitorHelper;
using TVinciShared;
using HttpMultipartParser;
//using EventManager;

namespace WebAPI.Filters
{

    public class RequestParser : ActionFilterAttribute
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const char PARAMS_PREFIX = ':';
        private const char PARAMS_NESTED_PREFIX = '.';

        private static readonly string fileSystemUploaderSourcePath = ApplicationConfiguration.Current.RequestParserConfiguration.TempUploadFolder.Value;

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


        public static object GetRequestPayload()
        {
            return HttpContext.Current.Items[RequestContextUtils.REQUEST_METHOD_PARAMETERS];
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

        private object CreateRandomFileName(string fileName)
        {
            var randomFileName = Path.GetRandomFileName();
            return Path.GetFileNameWithoutExtension(randomFileName) + Path.GetExtension(fileName);
        }

        public async Task<Dictionary<string, object>> ParseFormData(HttpActionContext actionContext)
        {
            if (actionContext.Request.Content.IsMimeMultipartContent())
            {
                if (!Directory.Exists(fileSystemUploaderSourcePath))
                {
                    Directory.CreateDirectory(fileSystemUploaderSourcePath);
                }

                var ret = new Dictionary<string, object>();

                byte[] requestBody = (byte[])HttpContext.Current.Items["body"];
                using (Stream stream = new MemoryStream(requestBody))
                {
                    var parser = new MultipartFormDataParser(stream);
                    foreach (var param in parser.Parameters)
                    {
                        ret.Add(param.Name, param.Data);
                    };

                    foreach (var uploadedFile in parser.Files)
                    {
                        var filePath = $@"{fileSystemUploaderSourcePath}\{CreateRandomFileName(uploadedFile.FileName)}";
                        using (Stream tempFile = File.Create(filePath))
                        {
                            uploadedFile.Data.CopyTo(tempFile);
                        }
                        ret.Add(uploadedFile.Name, new KalturaOTTFile(filePath, uploadedFile.FileName));
                    }

                }

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

            HttpContext.Current.Items[RequestContextUtils.REQUEST_TIME] = DateTime.UtcNow;

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
            else if (formData != null)
            {
                currentController = formData["service"].ToString();
                currentAction = formData["action"].ToString();
                if (formData.ContainsKey("pathData"))
                {
                    pathData = formData["pathData"].ToString();
                }

                if (formData.ContainsKey("apiVersion"))
                {
                    Version version;
                    if (!Version.TryParse((string)formData["apiVersion"], out version))
                        throw new RequestParserException(RequestParserException.INVALID_VERSION, formData["apiVersion"]);
                    
                    HttpContext.Current.Items[RequestContextUtils.REQUEST_VERSION] = version;
                }
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

            if (currentController == null && pathData == null)
            {
                createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidService, "Unknown Service");
                return;
            }

            if (currentController != null)
            {
                HttpContext.Current.Items[RequestContextUtils.REQUEST_SERVICE] = currentController;
                if (currentAction != null)
                {
                    HttpContext.Current.Items[RequestContextUtils.REQUEST_ACTION] = currentAction;
                }
            }

            if (pathData != null)
            {
                HttpContext.Current.Items[RequestContextUtils.REQUEST_PATH_DATA] = pathData;
            }

            if (actionContext.Request.Method == HttpMethod.Post)
            {
                JObject JObj = null;

                if (actionContext.Request.Content.IsMimeMultipartContent() && formData != null && formData.ContainsKey("json"))
                {
                    var str = formData["json"].ToString();
                    JObj = JObject.Parse(str);

                    if (formData.ContainsKey("fileData"))
                    {
                        var obj = JObject.FromObject(formData["fileData"]);
                        JObj.Add(new JProperty("fileData", obj));
                    }
                }
                else if (HttpContext.Current.Request.ContentType == "application/json" && HttpContext.Current.Request.ContentLength > 0)
                {
                    string result = await actionContext.Request.Content.ReadAsStringAsync();
                    using (var input = new StringReader(result))
                    {
                        var str = input.ReadToEnd();
                        JObj = JObject.Parse(str);
                    }
                }

                if (JObj != null)
                {
                    try
                    {
                        if (string.IsNullOrEmpty((string)HttpContext.Current.Items[Constants.CLIENT_TAG]))
                        {
                            //For logging
                            HttpContext.Current.Items.Remove(Constants.CLIENT_TAG);
                            if (JObj["clientTag"] != null)
                            {
                                HttpContext.Current.Items.Add(Constants.CLIENT_TAG, JObj["clientTag"]);
                            }
                            else if (HttpContext.Current.Request.QueryString.Count > 0 && HttpContext.Current.Request.QueryString["clientTag"] != null)
                            {
                                HttpContext.Current.Items.Add(Constants.CLIENT_TAG, HttpContext.Current.Request.QueryString["clientTag"]);
                            }
                        }

                        if (JObj["apiVersion"] != null)
                        {
                            //For logging and to parse old standard
                            Version version;
                            if (!Version.TryParse((string)JObj["apiVersion"], out version))
                                throw new RequestParserException(RequestParserException.INVALID_VERSION, JObj["apiVersion"]);

                            HttpContext.Current.Items[RequestContextUtils.REQUEST_VERSION] = version;
                        }

                        Dictionary<string, object> requestParams;
                        if (HttpContext.Current.Items[RequestContextUtils.REQUEST_PATH_DATA] != null)
                        {
                            requestParams = groupPathDataParams((string)HttpContext.Current.Items[RequestContextUtils.REQUEST_PATH_DATA]);
                        }
                        else
                        {
                            requestParams = JObj.ToObject<Dictionary<string, object>>();
                        }
                        RequestContext.SetContext(requestParams, currentController, currentAction);

                        List<Object> methodParams;
                        if (currentController.ToLower() == "multirequest")
                        {
                            methodParams = RequestParsingHelpers.BuildMultirequestActions(requestParams);
                        }
                        else
                        {
                            Dictionary<string, MethodParam> methodArgs = DataModel.getMethodParams(currentController, currentAction);
                            methodParams = RequestParsingHelpers.BuildActionArguments(methodArgs, requestParams);
                        }
                        HttpContext.Current.Items.Add(RequestContextUtils.REQUEST_METHOD_PARAMETERS, methodParams);
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
                        if (HttpContext.Current.Items[RequestContextUtils.REQUEST_PATH_DATA] != null)
                        {
                            groupedParams = groupPathDataParams((string)HttpContext.Current.Items[RequestContextUtils.REQUEST_PATH_DATA]);
                        }
                        else
                        {
                            //Running on the expected method parameters
                            groupedParams = groupParams(formData);
                        }

                        RequestContext.SetContext(groupedParams, currentController, currentAction);

                        List<Object> methodParams;
                        if (currentController == "multirequest")
                        {
                            methodParams = RequestParsingHelpers.BuildMultirequestActions(groupedParams);
                        }
                        else
                        {
                            Dictionary<string, MethodParam> methodArgs = DataModel.getMethodParams(currentController, currentAction);
                            methodParams = RequestParsingHelpers.BuildActionArguments(methodArgs, groupedParams);
                        }
                        HttpContext.Current.Items.Add(RequestContextUtils.REQUEST_METHOD_PARAMETERS, methodParams);
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

                    HttpContext.Current.Items[RequestContextUtils.REQUEST_VERSION] = version;
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
                        if (HttpContext.Current.Items[RequestContextUtils.REQUEST_PATH_DATA] != null)
                        {
                            groupedParams = groupPathDataParams((string)HttpContext.Current.Items[RequestContextUtils.REQUEST_PATH_DATA]);
                        }
                        else
                        {
                            //Running on the expected method parameters
                            groupedParams = groupParams(tokens);
                        }

                        RequestContext.SetContext(groupedParams, currentController, currentAction);

                        List<Object> methodParams;
                        if (currentController == "multirequest")
                        {
                            methodParams = RequestParsingHelpers.BuildMultirequestActions(groupedParams);
                        }
                        else
                        {
                            Dictionary<string, MethodParam> methodArgs = DataModel.getMethodParams(currentController, currentAction);
                            methodParams = RequestParsingHelpers.BuildActionArguments(methodArgs, groupedParams);
                        }
                        HttpContext.Current.Items.Add(RequestContextUtils.REQUEST_METHOD_PARAMETERS, methodParams);
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

        

    }
}