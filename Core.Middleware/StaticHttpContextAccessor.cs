using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Middleware
{
    public static class StaticHttpContextAccessor
    {

        public static IServiceCollection AddStaticHttpContextAccessor(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            var provider = services.BuildServiceProvider();
            var httpContextAccessor = provider.GetService<IHttpContextAccessor>();

            // Confgure the shim of System.Web.HttpContext, this is actualy the project StaticHttpContextForNetCore
            System.Web.HttpContext.Configure(httpContextAccessor);

            return services;
        }
    }
}
