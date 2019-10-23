using KLogMonitor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Phoenix.Context;
using Phoenix.Rest.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebAPI.App_Start;
using WebAPI.Controllers;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace Phoenix.Rest.Middleware
{
    public class PhoenixRequestExecutor
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly RequestDelegate _Next;
        private object _Response;
        private PhoenixRequestContext _PhoenixCtx;
        private readonly IResponseFromatterProvider _FormatterProvider;

        public PhoenixRequestExecutor(RequestDelegate next, IResponseFromatterProvider formatterProvider)
        {
            _Next = next;
            _FormatterProvider = formatterProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _PhoenixCtx = context.Items[PhoenixRequestContext.PHOENIX_REQUEST_CONTEXT_KEY] as PhoenixRequestContext;
            var ctr = new ServiceController();
            
            if (_PhoenixCtx.IsMultiRequest)
            {
                _Response = await ctr.Multirequest(_PhoenixCtx.RouteData.Service);

            }
            else
            {
                _Response = await ctr.Action(_PhoenixCtx.RouteData.Service, _PhoenixCtx.RouteData.Action);

            }

            context.Response.OnStarting(HandleResponse, context);

            await _Next(context);
        }

        private async Task HandleResponse(object ctx)
        {
            var context = ctx as HttpContext;

            // These should never happen at this point, but better be safe than sorry
            if (context == null) throw new SystemException("HttpContext lost");
            if (!_PhoenixCtx.SessionId.HasValue) throw new SystemException("Session lost");

            var wrappedResponse = new StatusWrapper((int)StatusCode.OK, _PhoenixCtx.SessionId.Value, float.Parse(_PhoenixCtx.ApiMonitorLog.ExecutionTime), _Response);

            context.Request.Headers.TryGetValue("accept", out var acceptHeader);
            var formatter = _FormatterProvider.GetFormatter(acceptHeader.ToArray(), _PhoenixCtx.Format);

            context.Response.ContentType = formatter.AcceptContentTypes[0];
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            try
            {
                var stringResponse = await formatter.GetStringResponse(wrappedResponse);
                await context.Response.WriteAsync(stringResponse);
            }
            catch (Exception e)
            {
                _Logger.Error("error while writing response stream", e);
                throw;
            }
        }
    }
}

