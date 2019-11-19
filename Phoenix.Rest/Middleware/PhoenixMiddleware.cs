using KLogMonitor;
using Microsoft.AspNetCore.Builder;
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
using Microsoft.AspNetCore.ConcurrencyLimiter;
using Core.Middleware;

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
            services.AddCoreConcurrencyLimiter();
            services.AddHttpContextAccessor();
            services.AddKalturaApplicationSessionContext();
            services.AddSingleton<IResponseFromatterProvider, ResponseFromatterProvider>();
            services.AddApiExceptionHandler<PhoenixExceptionHandler>();
            return services;
        }

        
        /// <summary>
        /// Using custom middleware for Phoenix Api convention
        /// </summary>
        public static IApplicationBuilder UsePhoenix(this IApplicationBuilder app)
        {
            AutoMapperConfig.RegisterMappings();

            app.UseCoreConcurrencyLimiter();
            app.UseApiExceptionHandler();
            app.UseKloggerSessionIdBuilder();
            app.UseRequestLogger();
            app.EnablePublicCors();
            app.UseMiddleware<PhoenixRequestContextBuilder>();
            app.UseMiddleware<PhoenixRequestExecutor>();
            return app;
        }

    }
}

