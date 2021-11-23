using KLogMonitor;
using Microsoft.AspNetCore.Http;
using Phoenix.Context;
using Phoenix.Rest.Services;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using WebAPI.App_Start;
using WebAPI.Controllers;
using WebAPI.Managers.Models;
using WebAPI.Reflection;
using WebAPI.Utils;

namespace Phoenix.Rest.Middleware
{
    public class PhoenixRequestExecutor
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly RequestDelegate _Next;
        private readonly IResponseFromatterProvider _FormatterProvider;
        private static readonly ServiceController _ServiceController = new ServiceController();

        public PhoenixRequestExecutor(RequestDelegate next, IResponseFromatterProvider formatterProvider)
        {
            _Next = next;
            _FormatterProvider = formatterProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var phoenixContext = context.Items[PhoenixRequestContext.PHOENIX_REQUEST_CONTEXT_KEY] as PhoenixRequestContext;
            if (phoenixContext == null)
            {
                throw new Exception("Phoenix Context was lost on the way :/ this should never happen. if you see this message... hopefully...");
            }

            object response = phoenixContext.IsMultiRequest
                ? await _ServiceController.Multirequest(phoenixContext.RouteData.Service)
                : await _ServiceController.Action(phoenixContext.RouteData.Service, phoenixContext.RouteData.Action);

            phoenixContext.Response = response;
            PhoenixResponseContext phoenixResponseContext = new PhoenixResponseContext { StatusCode = context.Response.StatusCode };
            context.Items[PhoenixResponseContext.PHOENIX_RESPONSE_CONTEXT_KEY] = phoenixResponseContext;

            context.Response.OnStarting(HandleResponse, context);

            await _Next(context);
        }

        private async Task HandleResponse(object ctx)
        {
            var context = ctx as HttpContext;
            var phoenixContext = context.Items[PhoenixRequestContext.PHOENIX_REQUEST_CONTEXT_KEY] as PhoenixRequestContext;
            var phoenixResponseContext = context.Items[PhoenixResponseContext.PHOENIX_RESPONSE_CONTEXT_KEY] as PhoenixResponseContext;
            // These should never happen at this point, but better be safe than sorry
            if (phoenixContext == null || phoenixResponseContext == null)
            {
                throw new Exception("Phoenix Context was lost on the way :/ this should never happen. if you see this message... hopefully...");
            }

            if (context == null) throw new SystemException("HttpContext lost");

            var isNotModifiedResponse = await IsNotModifiedResponseAsync(phoenixContext, context);
            if (!isNotModifiedResponse)
            {
                await ProcessResponse(phoenixContext, context, phoenixResponseContext);
            }
        }

        private async Task ProcessResponse(PhoenixRequestContext phoenixContext, HttpContext context, PhoenixResponseContext phoenixResponseContext)
        {
            context.Request.Headers.TryGetValue("accept", out var acceptHeader);
            var formatter = _FormatterProvider.GetFormatter(acceptHeader.ToArray(), phoenixContext.Format);

            context.Response.StatusCode = phoenixResponseContext.StatusCode;
            context.Response.ContentType = formatter.AcceptContentTypes[0];

            var wrappedResponse = new StatusWrapper
            {
                ExecutionTime = float.Parse(phoenixContext.ApiMonitorLog.ExecutionTime),
                Result = phoenixContext.Response,
            };

            try
            {
                if (formatter is ExcelFormatter)
                {
                    await formatter.WriteToStreamAsync(wrappedResponse.GetType(), wrappedResponse, context.Response.Body, null, null);
                }
                else if (formatter is JsonPFormatter)
                {
                    var stringResponse = await formatter.GetStringResponse(wrappedResponse);
                    await formatter.WriteToStreamAsync(wrappedResponse.GetType(), stringResponse, context.Response.Body, null, null);
                }
                else
                {
                    var stringResponse = await formatter.GetStringResponse(wrappedResponse);
                    await context.Response.WriteAsync(stringResponse);
                }
            }
            catch (Exception e)
            {
                _Logger.Error($"error while writing response stream, exception: {e}.", e);
                throw;
            }
        }

        private async Task<bool> IsNotModifiedResponseAsync(PhoenixRequestContext phoenixContext, HttpContext context)
        {
            if (phoenixContext.IsMultiRequest // Not Modified response is not supported for MultiRequest.
                || !DataModel.ContentNotModifiedResponseEnabled(phoenixContext.RouteData.Service, phoenixContext.RouteData.Action))
            {
                return false;
            }

            var stringResponse = await new JilFormatter().GetStringResponse(phoenixContext.Response);
            var responseHash = EncryptionUtils.HashMD5(stringResponse, Encoding.UTF8);
            var eTag = $"W/{responseHash}";
            context.Response.Headers[HeaderNames.ETag] = eTag;

            if (context.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var etagValues) && eTag.Equals(etagValues.First()))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotModified;

                return true;
            }

            return false;
        }
    }
}