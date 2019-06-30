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

namespace Phoenix.Rest.Infrastructure
{
    public class PhoenixRequestContextBuilder
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly RequestDelegate _Next;

        public PhoenixRequestContextBuilder(RequestDelegate next)
        {
            _Next = next;
        }

        public async Task InvokeAsync(HttpContext context, IPhoenixRequestContext phoenixCtx)
        {
            var request = context.Request;
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

            await GetJsonBody(phoenixCtx, request);



            context.Response.OnStarting(async () =>
            {
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(phoenixCtx));
            });



            await _Next(context);

        }

        private bool TryGetRouteDataFromFormBody(IPhoenixRequestContext phoenixCtx, HttpRequest request)
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

        private bool TryGetRoutDataFromQueryString(IPhoenixRequestContext phoenixCtx, HttpRequest request)
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

        private static bool TryGetRouteDataFromUrl(IPhoenixRequestContext phoenixCtx, HttpRequest request)
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

        private static async Task GetJsonBody(IPhoenixRequestContext phoenixCtx, HttpRequest request)
        {
            using (var streamReader = new HttpRequestStreamReader(request.Body, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var body = await JObject.LoadAsync(jsonReader);
                phoenixCtx.RequestBody = body;
            }
        }
    }

}

