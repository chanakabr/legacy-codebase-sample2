using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Phoenix.Rest.Services;
using System.Reflection;
using WebAPI.Filters;
using Core.Middleware;
using HealthCheck;
using Phoenix.Rest.Middleware.Metrics;

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
            services.AddKalturaHealthCheckService();
            services.AddCoreConcurrencyLimiter();
            services.AddHttpContextAccessor();
            services.AddStaticHttpContextAccessor();
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
            EventNotificationsConfig.SubscribeConsumers();

            app.UseRequestResponseLogger();
            app.UseHealthCheck("/api_v3/service/system/action/health");
            app.UseCoreConcurrencyLimiter();
            app.UseHttpPrometheusMetrics();
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

