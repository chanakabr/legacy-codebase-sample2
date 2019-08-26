using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TVPApi.Common;

namespace TVPApi.Web.Middleware
{
    public static class TVPApiMiddleware
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        /// <summary>
        /// Using custom middleware for Phoenix Api convention
        /// </summary>
        public static IApplicationBuilder UseTvpApi(this IApplicationBuilder app)
        {
            var test = new CommonTest().Test;
            app.UseMiddleware<TVPApiExceptionHandler>();
            app.UseMiddleware<TVPApiRequestExecutor>();
            //app.UseMiddleware<PhoenixExceptionHandler>();
            //AutoMapperConfig.RegisterMappings();
            //app.UseMiddleware<PhoenixSessionId>();
            //app.UseMiddleware<PhoenixCors>();
            //app.UseMiddleware<PhoenixRequestContextBuilder>();
            //app.UseMiddleware<PhoenixRequestExecutor>();
            return app;
        }
    }
}
