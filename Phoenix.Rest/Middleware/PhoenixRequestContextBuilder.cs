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
using Phoenix.Rest.Helpers;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Phoenix.Rest.Middleware
{
    public class PhoenixRequestContextBuilder
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly RequestDelegate _Next;

        public PhoenixRequestContextBuilder(RequestDelegate next)
        {
            _Next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var phoenixCtx = context.Items[PhoenixRequestContext.PHOENIX_REQUEST_CONTEXT_KEY] as PhoenixRequestContext;
            var request = context.Request;
            GetRouteData(phoenixCtx, request);

            if (context.Request.Method == HttpMethods.Post)
            {
                await GetRequestBody(phoenixCtx, request);
            }
            else
            {
                GetRequestBodyFromQueryString(phoenixCtx, request);
            }


            context.Response.OnStarting(async () =>
            {
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(phoenixCtx));
            });



            await _Next(context);

        }





        private void GetRouteData(PhoenixRequestContext phoenixCtx, HttpRequest request)
        {
            if (!TryGetRouteDataFromUrl(phoenixCtx, request))
            {
                if (!TryGetRoutDataFromQueryString(phoenixCtx, request))
                {
                    if (!TryGetRouteDataFromFormBody(phoenixCtx, request))
                    {
                        // TODO: arthur, add error response handling
                        throw new Exception("Unknown service");
                    }
                };
            }
        }

        private bool TryGetRouteDataFromFormBody(PhoenixRequestContext phoenixCtx, HttpRequest request)
        {
            if (!request.HasFormContentType) { return false; }

            if (request.Form.TryGetValue("service", out var serviceQsVal))
            {
                phoenixCtx.Service = serviceQsVal.First();
                if (request.Form.TryGetValue("action", out var actionQsVal))
                {
                    phoenixCtx.Action = actionQsVal.First();
                }

                if (request.Form.TryGetValue("pathData", out var pathDataQsVal))
                {
                    phoenixCtx.PathData = pathDataQsVal.First();
                }

                return true;
            }

            return false;
        }

        private bool TryGetRoutDataFromQueryString(PhoenixRequestContext phoenixCtx, HttpRequest request)
        {
            if (request.Method != HttpMethods.Get) { return false; }
            if (request.Query.TryGetValue("service", out var serviceQsVal))
            {
                phoenixCtx.Service = serviceQsVal.First();
                if (request.Query.TryGetValue("action", out var actionQsVal))
                {
                    phoenixCtx.Action = actionQsVal.First();
                }

                if (request.Query.TryGetValue("pathData", out var pathDataQsVal))
                {
                    phoenixCtx.PathData = pathDataQsVal.First();
                }

                return true;
            }

            return false;
        }

        private bool TryGetRouteDataFromUrl(PhoenixRequestContext phoenixCtx, HttpRequest request)
        {
            var urlSegments = request.Path.Value.Split("/", StringSplitOptions.RemoveEmptyEntries);

            phoenixCtx.Service = urlSegments.ElementAtOrDefault(2);
            var isRoutDataFoundInUrl = phoenixCtx.Service != null;
            if (isRoutDataFoundInUrl)
            {
                phoenixCtx.Action = urlSegments.ElementAtOrDefault(4);
            }

            phoenixCtx.PathData = string.Join('/', urlSegments.Skip(4));

            return isRoutDataFoundInUrl;
        }


        private async Task GetRequestBody(PhoenixRequestContext phoenixCtx, HttpRequest request)
        {

            if (request.HasFormContentType)
            {
                await ParseFormDataBody(phoenixCtx, request);
            }
            else if (request.ContentType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
            {
                await ParseJsonBody(phoenixCtx, request);
            }
            else
            {
                // TODO: Arthur, handle error
                throw new Exception("Unsupported content type");
            }


        }

        private void GetRequestBodyFromQueryString(PhoenixRequestContext phoenixCtx, HttpRequest request)
        {
            var queryStringDictionary = request.Query.ToDictionary(k => k.Key, v => v.Value.Count == 1 ? v.Value.First() : v.Value as object);
            var nestedDictionary = GetNestedDictionary(queryStringDictionary);
            phoenixCtx.ActionParams = nestedDictionary;

        }

        private async Task ParseFormDataBody(PhoenixRequestContext phoenixCtx, HttpRequest request)
        {
            foreach (var file in request.Form.Files)
            {
                // TODO: Parse Kaltura OTT Files and upload
                throw new NotImplementedException("File upload is not implementd");
            }

            if (request.Form.TryGetValue("json", out var jsonFromData))
            {
                var body = JObject.Parse(jsonFromData.First());
            }
            else
            {
                var queryStringDictionary = request.Form.ToDictionary(k => k.Key, v => v.Value.Count == 1 ? v.Value.First() : v.Value as object);
                var nestedDictionary = GetNestedDictionary(queryStringDictionary);
                phoenixCtx.ActionParams = nestedDictionary;
            }

        }

        private static async Task ParseJsonBody(PhoenixRequestContext phoenixCtx, HttpRequest request)
        {

            using (var streamReader = new HttpRequestStreamReader(request.Body, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var body = await JObject.LoadAsync(jsonReader);
                phoenixCtx.ActionParams = body.ToObject<Dictionary<string, object>>();
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
    }

}

