using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Phoenix.Rest.Infrastructure
{
    public static class StaticHttpContextConfiguration
    {

        /// <summary>
        /// Setting Static Http Context Accessor to allow compatability with all other services that are still using System.Web HttpContext
        /// This will no longer be required if we remove usage of static HTTP context and use Dependency Injection of IHttpContextAccessor instead
        /// </summary>
        public static IServiceCollection AddKalturaApplicationSessionContext(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var htttpContextAccessor  = provider.GetService<IHttpContextAccessor>();
            System.Web.HttpContext.Configure(htttpContextAccessor);
            return services;
        }
    }
}
