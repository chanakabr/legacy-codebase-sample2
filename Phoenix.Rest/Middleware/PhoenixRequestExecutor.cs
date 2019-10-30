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
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

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
            if (phoenixContext == null) { throw new Exception("Phoenix Context was lost on the way :/ this should never happen. if you see this message... hopefully..."); }


            _Logger.Info($"PhoenixRequestExecutor >_PhoenixCtx.ActionParams:[{JsonConvert.SerializeObject(phoenixContext.ActionParams)}]");
            _Logger.Info($"PhoenixRequestExecutor >        HttpContext:[{JsonConvert.SerializeObject(context.Items)}]");
            _Logger.Info($"PhoenixRequestExecutor > Static.HttpContext:[{JsonConvert.SerializeObject(System.Web.HttpContext.Current.Items)}]");

            object response;
            if (phoenixContext.IsMultiRequest)
            {
                response = _ServiceController.Multirequest(phoenixContext.RouteData.Service).Result;

            }
            else
            {
                _Logger.Info($"Execution ServiceController.Action > sending {phoenixContext.RouteData.Service}.{phoenixContext.RouteData.Action}");
                response = _ServiceController.Action(phoenixContext.RouteData.Service, phoenixContext.RouteData.Action).Result;
            }

            phoenixContext.Response = response;

            context.Response.OnStarting(HandleResponse, context);

            await _Next(context);
        }

        private async Task HandleResponse(object ctx)
        {
            var context = ctx as HttpContext;
            var phoenixContext = context.Items[PhoenixRequestContext.PHOENIX_REQUEST_CONTEXT_KEY] as PhoenixRequestContext;
            if (phoenixContext == null) { throw new Exception("Phoenix Context was lost on the way :/ this should never happen. if you see this message... hopefully..."); }

            // These should never happen at this point, but better be safe than sorry
            if (context == null) throw new SystemException("HttpContext lost");

            var wrappedResponse = new StatusWrapper
            {
                ExecutionTime = float.Parse(phoenixContext.ApiMonitorLog.ExecutionTime),
                Result = phoenixContext.Response,
            };

            context.Request.Headers.TryGetValue("accept", out var acceptHeader);
            var formatter = _FormatterProvider.GetFormatter(acceptHeader.ToArray(), phoenixContext.Format);

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

