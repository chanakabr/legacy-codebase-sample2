using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Phoenix.Context;
using Phoenix.Rest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using WebAPI;
using WebAPI.App_Start;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using Core.Middleware;

namespace Phoenix.Rest.Middleware
{
    public class PhoenixExceptionHandler : IApiExceptionHandler
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly IResponseFromatterProvider _FormatterProvider;

        public PhoenixExceptionHandler(IResponseFromatterProvider formatterProvider)
        {
            _FormatterProvider = formatterProvider;
        }

        public async Task<ApiExceptionHandlerResponse> FormatErrorResponse(HttpContext context, Exception ex)
        {
            var ctx = context.Items[PhoenixRequestContext.PHOENIX_REQUEST_CONTEXT_KEY] as PhoenixRequestContext;
            // for some reason klogger looses the reqId at this point
            KLogger.SetRequestId(ctx.SessionId);
            int code;
            string message;
            string stackTrace;
            KalturaApiExceptionArg[] args;
            if (ex is ApiException apiEx)
            {
                code = apiEx.Code;
                message = apiEx.Message;
                stackTrace = apiEx.StackTrace;
                args = apiEx.Args;
            }
            else
            {
                code = (int)StatusCode.Error;
                message = "Unknown error";
                stackTrace = ex.StackTrace;
                args = null;
            }

            var content = KalturaApiExceptionHelpers.prepareExceptionResponse(code, message, args);
            var errorResponse = new StatusWrapper
            {
                ExecutionTime = float.Parse(ctx.ApiMonitorLog.ExecutionTime),
                Result = content,
            };


            // get proper response formatter but make sure errors should be only xml or json ...
            context.Request.Headers.TryGetValue("accept", out var acceptHeader);
            var format = ctx.Format != "1" || ctx.Format != "2" ? "1" : ctx.Format;
            var formatter = _FormatterProvider.GetFormatter(acceptHeader.ToArray(), ctx.Format);

            context.Response.Headers.Add("X-Kaltura-App", $"exiting on error {code} - {message}");
            context.Response.Headers.Add("X-Kaltura", $"error-{code}");

            var stringResponse = await formatter.GetStringResponse(errorResponse);
                   
            _Logger.Error($"Error while calling url:[{ctx.RawRequestUrl}], body:[{ctx.RawRequestBody}]{Environment.NewLine}reqId:[{ctx.SessionId}]{Environment.NewLine}error response:[{stringResponse}]{Environment.NewLine}PhoenixContext:[{JsonConvert.SerializeObject(ctx)}]{Environment.NewLine}", ex);
            return new ApiExceptionHandlerResponse
            {
                HttpStatusCode = (int)HttpStatusCode.OK,
                ContentType = formatter.AcceptContentTypes[0],
                Reponse = stringResponse,
            };
        }
    }
}






