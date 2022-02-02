using Phx.Lib.Log;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Core.Middleware
{
    public class ApiExceptionHandlerResponse
    {
        public int HttpStatusCode { get; set; }
        public string ContentType { get; set; }
        public string Reponse { get; set; }
    }

    public interface IApiExceptionHandler
    {
        Task<ApiExceptionHandlerResponse> FormatErrorResponse(HttpContext context, Exception e);
    }

    public class ApiExceptionHandlerMiddleware
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly RequestDelegate _Next;
        private readonly IApiExceptionHandler _ApiExceptionHandler;

        public ApiExceptionHandlerMiddleware(RequestDelegate next, IApiExceptionHandler apiExceptionHandler)
        {
            _Next = next;
            _ApiExceptionHandler = apiExceptionHandler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _Next(context);
            }
            catch (Exception e)
            {
                try
                {
                    var handleErrorResponse = await _ApiExceptionHandler.FormatErrorResponse(context, e);
                    context.Response.StatusCode = handleErrorResponse.HttpStatusCode;
                    context.Response.ContentType = handleErrorResponse.ContentType;
                    await context.Response.WriteAsync(handleErrorResponse.Reponse);
                }
                catch (Exception innerEx)
                {
                    _Logger.Error($"Error while trying to generate an API Error response from APIException:[{e.ToString()}], innerEx:[{innerEx}]", innerEx);
                    throw e;
                }
            }
        }
    }


    public static class ApiExceptionHandlerMiddlewareExtentions
    {
        public static IServiceCollection AddApiExceptionHandler<T>(this IServiceCollection services) where T : class, IApiExceptionHandler
        {
            services.AddSingleton<IApiExceptionHandler, T>();
            return services;
        }

        public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<ApiExceptionHandlerMiddleware>();
            return app;
        }
    }
}
