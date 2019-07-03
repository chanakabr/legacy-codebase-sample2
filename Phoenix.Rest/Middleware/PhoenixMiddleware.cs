using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Phoenix.Context;
using System.Linq;

namespace Phoenix.Rest.Middleware
{


    public static class PhoenixMiddleware
    {
        /// <summary>
        /// Adding required services to use the Phoenix middleware
        /// </summary>
        public static IServiceCollection ConfigurePhoenix(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddKalturaApplicationSessionContext();
            return services;
        }

        /// <summary>
        /// Using custom middleware for Phoenix Api convention
        /// </summary>
        public static IApplicationBuilder UsePhoenix(this IApplicationBuilder app)
        {
            app.UseMiddleware<PhoenixSessionId>();
            app.UseMiddleware<PhoenixCors>();
            app.UseMiddleware<PhoenixRequestContextBuilder>();
            app.UseMiddleware<PhoenixRequestParser>();
            return app;
        }
    }
}

