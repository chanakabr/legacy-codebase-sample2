using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TVPApi.Common;
using Core.Middleware;
using Microsoft.Extensions.DependencyInjection;
using WebAPI.Filters;

namespace TVPApi.Web.Middleware
{
    public static class TVPApiMiddleware
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static void ConfigureTvpapi(this IServiceCollection services)
        {
            services.AddCoreConcurrencyLimiter();
            services.AddStaticHttpContextAccessor();
            services.AddApiExceptionHandler<TVPApiExceptionHandler>();
        }

        /// <summary>
        /// Using custom middleware for Phoenix Api convention
        /// </summary>
        public static IApplicationBuilder UseTvpApi(this IApplicationBuilder app)
        {
            AutoMapperConfig.RegisterMappings();
            EventNotificationsConfig.SubscribeConsumers();

            app.UseCoreConcurrencyLimiter();
            app.UseApiExceptionHandler();
            app.UseKloggerSessionIdBuilder();
            app.UseKlogerMonitor();
            app.UseRequestLogger();
            app.EnablePublicCors();
            app.UseMiddleware<TVPApiRequestExecutor>();

            return app;
        }
    }
}
