using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using ApiObjects.Response;
using Core.Middleware;
using KalturaRequestContext;
using Phx.Lib.Log;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Phoenix.Context;
using Phoenix.Rest.Services;
using WebAPI;
using WebAPI.App_Start;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Reflection;

namespace Phoenix.Rest.Middleware
{
    public class PhoenixExceptionHandler : IApiExceptionHandler
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly IResponseFromatterProvider _FormatterProvider;

        public const string INTERNAL_ERROR_CODE = "INTERNAL_ERROR_CODE";

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
            KalturaApiExceptionArg[] args;
            var apiException = ex as ApiException;
            if (apiException != null)
            {
                code = apiException.Code;
                message = apiException.Message;
                args = apiException.Args;
            }
            else if (ex is ClientException clientEx)
            {
                var apiFromClientEx = new ApiException(clientEx);
                code = apiFromClientEx.Code;
                message = apiFromClientEx.Message;
                args = apiFromClientEx.Args;
            }
            else
            {
                code = (int)StatusCode.Error;
                message = "Unknown error";
                args = null;
            }

            context.Items[INTERNAL_ERROR_CODE] = code;
            context.Response.Headers.Add("X-Kaltura-App", $"exiting on error {code} - {message}");
            context.Response.Headers.Add("X-Kaltura", $"error-{code}");
            
            context.Response.Headers.Add("x-kaltura-error-code", code.ToString());
            context.Response.Headers.Add("x-kaltura-error-msg", message);
            string enumName = "";
            if(Enum.IsDefined(typeof(eResponseStatus), code))
            {
                enumName = Enum.GetName(typeof(eResponseStatus), code);
            }
            else if(Enum.IsDefined(typeof(StatusCode), code))
            {
                enumName = Enum.GetName(typeof(StatusCode), code);
            }
            if (!string.IsNullOrEmpty(enumName))
            {
                context.Response.Headers.Add("x-kaltura-error-name", enumName);
            }
            
            // get proper response formatter but make sure errors should be only xml or json ...
            context.Request.Headers.TryGetValue("accept", out var acceptHeader);
            ctx.Format ??= context.Items[RequestContextConstants.REQUEST_FORMAT]?.ToString();
            var format = ctx.Format != "1" || ctx.Format != "2" ? "1" : ctx.Format;
            var formatter = _FormatterProvider.GetFormatter(acceptHeader.ToArray(), format);

            var content = KalturaApiExceptionHelpers.prepareExceptionResponse(code, message, args);
            var errorResponse = new StatusWrapper
            {
                ExecutionTime = float.Parse(ctx.ApiMonitorLog.ExecutionTime),
                Result = content
            };
            var stringResponse = await formatter.GetStringResponse(errorResponse);
            var phoenixContextMasked = _Logger.MaskPersonalInformation(JsonConvert.SerializeObject(ctx));
            _Logger.Error($"Error while calling api:[{ctx.RouteData}] response:[{stringResponse}]{Environment.NewLine}PhoenixContext:[{phoenixContextMasked}]{Environment.NewLine}", ex);

            var httpStatusCode = HttpStatusCode.OK;
            if (ctx.Format == "31")
            {
                httpStatusCode = HttpStatusCode.InternalServerError;
            }
            else if (apiException != null)
            {
                if (apiException is UnauthorizedException
                    && DataModel.UnauthorizedResponseEnabled(ctx.RouteData.Service, ctx.RouteData.Action))
                {
                    httpStatusCode = HttpStatusCode.Unauthorized;
                }
                else if (apiException.FailureHttpCode != 0)
                {
                    httpStatusCode = apiException.FailureHttpCode;
                }
            }

            var response = new ApiExceptionHandlerResponse
            {
                HttpStatusCode = (int)httpStatusCode,
                ContentType = formatter.AcceptContentTypes[0],
                Reponse = stringResponse
            };

            return response;
        }
    }
}