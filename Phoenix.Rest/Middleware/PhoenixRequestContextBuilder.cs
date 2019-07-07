using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Phoenix.Context;
using System;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Reflection;
using KLogMonitor;
using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Http.Extensions;
using System.Linq;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.IO;
using ConfigurationManager;
using WebAPI.Models.General;

namespace Phoenix.Rest.Middleware
{

    public class PhoenixRequestContextBuilder
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly string _FileSystemUploaderSourcePath = ApplicationConfiguration.RequestParserConfiguration.TempUploadFolder.Value;

        private readonly RequestDelegate _Next;

        public PhoenixRequestContextBuilder(RequestDelegate next)
        {
            _Next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var phoenixCtx = context.Items[PhoenixRequestContext.PHOENIX_REQUEST_CONTEXT_KEY] as PhoenixRequestContext;
            var request = context.Request;
            phoenixCtx.RouteData = GetRouteData(request);

            if (context.Request.Method == HttpMethods.Post)
            {
                await GetRequestBody(request);
            }
            else
            {
                GetRequestBodyFromQueryString(request);
            }

            phoenixCtx.SetHttpContextForBackwardCompatibility();

            context.Response.OnStarting(async () =>
            {
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(phoenixCtx));
            });

            await _Next(context);
        }





        private RequestRouteData GetRouteData(HttpRequest request)
        {
            if (TryGetRouteDataFromUrl(request, out var routeDataFromUrl)) { return routeDataFromUrl; }
            else if (TryGetRoutDataFromQueryString(request, out var routDataFromQS)) { return routDataFromQS; }
            else if (TryGetRouteDataFromFormBody(request, out var routeDataFromFormBody)) { return routeDataFromFormBody; }
            else
            {
                // TODO: arthur, add error response handling
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

            routeData.PathData = string.Join('/', urlSegments.Skip(4));

            return isRoutDataFoundInUrl;
        }


        private async Task<IDictionary<string, object>> GetRequestBody(HttpRequest request)
        {

            if (request.HasFormContentType)
            {
                return await ParseFormDataBody(request);
            }
            else if (request.ContentType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return await ParseJsonBody(request);
            }
            else
            {
                // TODO: Arthur, handle error
                throw new Exception("Unsupported content type");
            }


        }

        private IDictionary<string, object> GetRequestBodyFromQueryString(HttpRequest request)
        {
            var queryStringDictionary = request.Query.ToDictionary(k => k.Key, v => v.Value.Count == 1 ? v.Value.First() : v.Value as object);
            var nestedDictionary = GetNestedDictionary(queryStringDictionary);
            return nestedDictionary;

        }

        private async Task<IDictionary<string, object>> ParseFormDataBody(HttpRequest request)
        {
            var uploadedFilesDictionary = await ParseUploadedFiles(request);

            var actionParamsDictionary = ParseRequestBodyFromFormData(request);
            return uploadedFilesDictionary.Concat(actionParamsDictionary).ToDictionary(k => k.Key, v => v.Value);

        }

        private IDictionary<string, object> ParseRequestBodyFromFormData(HttpRequest request)
        {
            if (request.Form.TryGetValue("json", out var jsonFromData))
            {
                var body = JObject.Parse(jsonFromData.First());
                return body.ToObject<IDictionary<string, object>>();
            }
            else
            {
                var queryStringDictionary = request.Form.ToDictionary(k => k.Key, v => v.Value.Count == 1 ? v.Value.First() : v.Value as object);
                var nestedDictionary = GetNestedDictionary(queryStringDictionary);
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

        private static async Task<IDictionary<string, object>> ParseJsonBody(HttpRequest request)
        {

            using (var streamReader = new HttpRequestStreamReader(request.Body, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var body = await JObject.LoadAsync(jsonReader);
                return body.ToObject<Dictionary<string, object>>();
            }
        }

        private Dictionary<string, object> GetNestedDictionary(Dictionary<string, object> tokens)
        {
            var result = new Dictionary<string, object>();

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
    }

}

