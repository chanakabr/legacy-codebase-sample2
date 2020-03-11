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
        /// <summary>
        /// Setting Static Http Context Accessor to allow compatability with all other services that are still using System.Web HttpContext
        /// This will no longer be required if we remove usage of static HTTP context and use Dependency Injection of IHttpContextAccessor instead
        /// </summary>
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
