using ConfigurationManager;
using KLogMonitor;
using WebAPI.Managers.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Phoenix.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;
using WebAPI;
using WebAPI.Filters;
using WebAPI.Managers;
using WebAPI.Models.General;
using WebAPI.Reflection;

namespace Phoenix.Rest.Middleware
{
    public class PhoenixRequestContextBuilder
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly string _FileSystemUploaderSourcePath = ApplicationConfiguration.Current.RequestParserConfiguration.TempUploadFolder.Value;
        private static int _LegacyAccessTokenLength = ApplicationConfiguration.Current.RequestParserConfiguration.AccessTokenLength.Value;
        private readonly RequestDelegate _Next;

        public PhoenixRequestContextBuilder(RequestDelegate next)
        {
            _Next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var km = new KMonitor(Events.eEvent.EVENT_CLIENT_API_START);

            KS.ClearOnRequest();
            var phoenixContext = new PhoenixRequestContext();
            context.Items[PhoenixRequestContext.PHOENIX_REQUEST_CONTEXT_KEY] = phoenixContext;
            context.Items[RequestContextUtils.REQUEST_TIME] = DateTime.UtcNow;
            phoenixContext.SessionId = KLogger.GetRequestId();
            phoenixContext.RequestDate = DateTime.UtcNow;
            phoenixContext.ApiMonitorLog = km;
            if (phoenixContext == null) { throw new SystemException("Request Context is lost, something went wrong."); }

            var request = context.Request;
            phoenixContext.RawRequestUrl = request.GetDisplayUrl();

            phoenixContext.RouteData = GetRouteData(request);
            var action = phoenixContext.RouteData.Action;
            var service = phoenixContext.RouteData.Service;
            var pathData = phoenixContext.RouteData.PathData;
            string serviceAction = $"{service}.{action}";
            KLogger.LogContextData[KLogMonitor.Constants.ACTION] = serviceAction;
            System.Web.HttpContext.Current.Items[Constants.ACTION] = serviceAction;

            var parsedActionParams = await GetActionParams(context.Request.Method, request, phoenixContext);
            phoenixContext.RequestVersion = GetRequestVersion(parsedActionParams);
            SetCommonRequestContextItems(context, phoenixContext, parsedActionParams, service, action);

            var actionParams = GetDeserializedActionParams(parsedActionParams, phoenixContext.IsMultiRequest, service, action);
            context.Items[RequestContextUtils.REQUEST_METHOD_PARAMETERS] = actionParams;
            phoenixContext.ActionParams = actionParams;

            await _Next(context);
        }

        private void SetCommonRequestContextItems(HttpContext context, PhoenixRequestContext _PhoenixContext, IDictionary<string, object> parsedActionParams, string service, string action)
        {
            RequestContext.SetContext(parsedActionParams, service, action);
            _PhoenixContext.Format = context.Items[RequestContextUtils.REQUEST_FORMAT]?.ToString();
            _PhoenixContext.UserIpAdress = context.Items[RequestContextUtils.USER_IP]?.ToString();
            _PhoenixContext.Currency = context.Items[RequestContextUtils.REQUEST_GLOBAL_CURRENCY]?.ToString();
            _PhoenixContext.Language = context.Items[RequestContextUtils.REQUEST_GLOBAL_LANGUAGE]?.ToString();

            if (context.Items.TryGetValue(RequestContextUtils.REQUEST_RESPONSE_PROFILE, out var responseProfile))
            {
                _PhoenixContext.ResponseProfile = responseProfile as KalturaOTTObject;
            }

            if (context.Items.TryGetValue(RequestContextUtils.REQUEST_TYPE, out var reqType))
            {
                _PhoenixContext.RequestType = reqType as RequestType?;
            }

            if (context.Items.TryGetValue(RequestContextUtils.REQUEST_GLOBAL_USER_ID, out var userId))
            {
                _PhoenixContext.UserId = userId as int?;
            }

            if (context.Items.TryGetValue(RequestContextUtils.REQUEST_VERSION, out var version))
            {
                _PhoenixContext.RequestVersion = version as Version;
            }
        }

        private Version GetRequestVersion(IDictionary<string, object> parsedActionParams)
        {
            if (parsedActionParams.ContainsKey("apiVersion"))
            {
                Version versionFromParams;
                if (Version.TryParse((string)parsedActionParams["apiVersion"], out versionFromParams))
                {
                    return versionFromParams;
                }
                else
                {
                    throw new RequestParserException(RequestParserException.INVALID_VERSION, parsedActionParams["apiVersion"]);
                }
            }

            return null;
        }

        private List<object> GetDeserializedActionParams(IDictionary<string, object> parsedActionParams, bool isMultiRequest, string service, string action)
        {
            var actionParams = new List<object>();
            if (isMultiRequest)
            {
                actionParams = RequestParsingHelpers.BuildMultirequestActions(parsedActionParams);
            }
            else
            {
                var methodArgs = DataModel.getMethodParams(service, action);
                actionParams = RequestParsingHelpers.BuildActionArguments(methodArgs, parsedActionParams);
            }
            return actionParams;
        }

        private async Task<IDictionary<string, object>> GetActionParams(string httpMethod, HttpRequest request, PhoenixRequestContext context)
        {
            IDictionary<string, object> parsedActionParams;
            if (httpMethod == HttpMethods.Post)
            {
                parsedActionParams = await GetActionParamsFromPostBody(request, context);
            }
            // previous code would have done it for all none POST request so keeping the same behavior
            else
            {
                parsedActionParams = GetActionParamsFromQueryString(request);
                if (httpMethod == HttpMethods.Get && context.RouteData.UrlParams != null && context.RouteData.UrlParams.Count > 0)
                {
                    if (parsedActionParams != null && parsedActionParams.Count > 0)
                    {
                        foreach (KeyValuePair<string, object> item in context.RouteData.UrlParams)
                        {
                            parsedActionParams[item.Key] = context.RouteData.UrlParams[item.Key];
                        }
                    }
                    else
                    {
                        parsedActionParams = context.RouteData.UrlParams;
                    }
                }
            }

            return new Dictionary<string, object>(parsedActionParams, StringComparer.OrdinalIgnoreCase);
        }

        private RequestRouteData GetRouteData(HttpRequest request)
        {
            if (TryGetRouteDataFromUrl(request, out var routeDataFromUrl)) { return routeDataFromUrl; }
            else if (TryGetRoutDataFromQueryString(request, out var routDataFromQS)) { return routDataFromQS; }
            else if (TryGetRouteDataFromFormBody(request, out var routeDataFromFormBody)) { return routeDataFromFormBody; }
            else
            {
                throw new Exception("Unknown service");
            }
        }

        private bool TryGetRouteDataFromFormBody(HttpRequest request, out RequestRouteData routeData)
        {
            routeData = new RequestRouteData();
            if (!request.HasFormContentType) { return false; }

            if (request.Form.TryGetValue("service", out var serviceQsVal))
            {
                routeData.Service = serviceQsVal.First();
                if (request.Form.TryGetValue("action", out var actionQsVal))
                {
                    routeData.Action = actionQsVal.First();
                }

                if (request.Form.TryGetValue("pathData", out var pathDataQsVal))
                {
                    routeData.PathData = pathDataQsVal.First();
                }

                return true;
            }

            return false;
        }

        private bool TryGetRoutDataFromQueryString(HttpRequest request, out RequestRouteData routeData)
        {
            routeData = new RequestRouteData();

            if (request.Method != HttpMethods.Get) { return false; }
            if (request.Query.TryGetValue("service", out var serviceQsVal))
            {
                routeData.Service = serviceQsVal.First();
                if (request.Query.TryGetValue("action", out var actionQsVal))
                {
                    routeData.Action = actionQsVal.First();
                }

                if (request.Query.TryGetValue("pathData", out var pathDataQsVal))
                {
                    routeData.PathData = pathDataQsVal.First();
                }

                return true;
            }

            return false;
        }

        private bool TryGetRouteDataFromUrl(HttpRequest request, out RequestRouteData routeData)
        {
            routeData = new RequestRouteData();

            var urlSegments = request.Path.Value.Split("/", StringSplitOptions.RemoveEmptyEntries);

            routeData.Service = urlSegments.ElementAtOrDefault(2);
            var isRoutDataFoundInUrl = routeData.Service != null;
            if (isRoutDataFoundInUrl)
            {
                routeData.Action = urlSegments.ElementAtOrDefault(4);
            }

            routeData.UrlParams = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            // in case url contains params
            if (urlSegments.Length <= 4) return isRoutDataFoundInUrl;

            string parameterKey = string.Empty;
            // url params starts after action, so we start on i = 5
            for (int i = 5; i < urlSegments.Length; i++)
            {
                /*odd numbers are the keys*/
                if (i % 2 ==1)
                {
                    parameterKey = urlSegments.ElementAtOrDefault(i);
                }
                else if (!routeData.UrlParams.ContainsKey(parameterKey))
                {
                    routeData.UrlParams.Add(parameterKey, urlSegments.ElementAtOrDefault(i));
                }
            }

            routeData.PathData = string.Join('/', urlSegments.Skip(5));

            return isRoutDataFoundInUrl;
        }

        private async Task<IDictionary<string, object>> GetActionParamsFromPostBody(HttpRequest request, PhoenixRequestContext context)
        {
            if (request.HasFormContentType)
            {
                return await ParseFormDataBody(request, context);
            }
            else if (request.ContentType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return await ParseJsonBody(request, context);
            }
            else
            {
                // TODO: Arthur, handle error
                throw new Exception("Unsupported content type");
            }
        }

        private IDictionary<string, object> GetActionParamsFromQueryString(HttpRequest request)
        {
            var queryStringDictionary = request.Query.ToDictionary(k => k.Key, v => v.Value.Count == 1 ? v.Value.First() : v.Value as object);
            var nestedDictionary = GetNestedDictionary(queryStringDictionary);
            return nestedDictionary;

        }

        private async Task<IDictionary<string, object>> ParseFormDataBody(HttpRequest request, PhoenixRequestContext context)
        {
            var uploadedFilesDictionary = await ParseUploadedFiles(request);

            var actionParamsDictionary = ParseRequestBodyFromFormData(request, context);
            return uploadedFilesDictionary.Concat(actionParamsDictionary).ToDictionary(k => k.Key, v => v.Value);

        }

        private IDictionary<string, object> ParseRequestBodyFromFormData(HttpRequest request, PhoenixRequestContext context)
        {
            if (request.Form.TryGetValue("json", out var jsonFromData))
            {
                var body = JObject.Parse(jsonFromData.First());
                context.RawRequestBody = body;
                return body.ToObject<IDictionary<string, object>>();
            }
            else
            {
                var queryStringDictionary = request.Form.ToDictionary(k => k.Key, v => v.Value.Count == 1 ? v.Value.First() : v.Value as object);
                var nestedDictionary = GetNestedDictionary(queryStringDictionary);
                context.RawRequestBody = JObject.FromObject(nestedDictionary);
                return nestedDictionary;
            }
        }

        private async Task<IDictionary<string, object>> ParseUploadedFiles(HttpRequest request)
        {
            var uploadedFiles = new Dictionary<string, object>();
            if (!Directory.Exists(_FileSystemUploaderSourcePath))
            {
                Directory.CreateDirectory(_FileSystemUploaderSourcePath);
            }

            foreach (var uploadedFile in request.Form.Files)
            {
                var filePath = $@"{_FileSystemUploaderSourcePath}\{CreateRandomFileName(uploadedFile.FileName)}";
                using (Stream tempFile = File.Create(filePath))
                {
                    await uploadedFile.CopyToAsync(tempFile);
                }

                uploadedFiles.Add(uploadedFile.Name, new KalturaOTTFile(filePath, uploadedFile.FileName));
            }

            return uploadedFiles;
        }

        private async Task<IDictionary<string, object>> ParseJsonBody(HttpRequest request, PhoenixRequestContext context)
        {
            using (var streamReader = new HttpRequestStreamReader(request.Body, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var body = await JObject.LoadAsync(jsonReader);
                context.RawRequestBody = body;
                return body.ToObject<Dictionary<string, object>>();
            }
        }

        private Dictionary<string, object> GetNestedDictionary(Dictionary<string, object> tokens)
        {
            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            // group the params by prefix
            foreach (var kv in tokens)
            {
                var splittedKey = kv.Key.Replace('[', '.').Replace("]", "").Split(".");
                SetElementByPath(result, splittedKey, kv.Value);
            }

            return result;
        }

        private void SetElementByPath(Dictionary<string, object> sourceDict, IEnumerable<string> splittedKey, object valueToSet)
        {
            var nestedDict = sourceDict;
            var keysStack = new Queue<string>(splittedKey);
            while (keysStack.TryDequeue(out var key))
            {
                if (key == "-" && keysStack.Count == 0) { break; }

                // If this is the last key in path .. then just place the value
                if (keysStack.Count == 0)
                {
                    nestedDict[key] = valueToSet;
                }
                // if there are more keys keep creating new nested dictionaries if they dont exist
                else
                {

                    if (!sourceDict.TryGetValue(key, out var currentDictValue))
                    {
                        sourceDict[key] = new Dictionary<string, object>();
                    }

                    nestedDict = nestedDict[key] as Dictionary<string, object>;
                }
            }
        }

        private string CreateRandomFileName(string fileName)
        {
            var randomFileName = Path.GetRandomFileName();
            return Path.GetFileNameWithoutExtension(randomFileName) + Path.GetExtension(fileName);
        }

        private static bool IsKsV2Format(string ksVal)
        {
            return ksVal.Length > _LegacyAccessTokenLength;
        }
    }
}