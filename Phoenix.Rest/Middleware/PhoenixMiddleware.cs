using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Phoenix.Context;
using Phoenix.Rest.Services;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using WebAPI.App_Start;
using WebAPI.Filters;

namespace Phoenix.Rest.Middleware
{


    public static class PhoenixMiddleware
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Adding required services to use the Phoenix middleware
        /// </summary>
        public static IServiceCollection ConfigurePhoenix(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddKalturaApplicationSessionContext();
            services.AddSingleton<IResponseFromatterProvider, ResponseFromatterProvider>();

            return services;
        }

        /// <summary>
        /// Using custom middleware for Phoenix Api convention
        /// </summary>
        public static IApplicationBuilder UsePhoenix(this IApplicationBuilder app)
        {
            app.UseMiddleware<PhoenixExceptionHandler>();
            AutoMapperConfig.RegisterMappings();
            app.UseMiddleware<PhoenixSessionId>();
            app.UseMiddleware<PhoenixCors>();
            app.UseMiddleware<PhoenixRequestContextBuilder>();
            app.UseMiddleware<PhoenixRequestExecutor>();
            return app;
        }

        private static async Task ExcpetionResponseHandler(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (contextFeature != null)
            {
                _Logger.Error($"{contextFeature.Error}");

                //await context.Response.WriteAsync(JsonConvert.SerializeObject(contextFeature.Error));



            }
        }
    }
}

