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
            app.UseMiddleware<TVPApiSessionId>();
            app.UseMiddleware<TVPApiExceptionHandler>();
            app.UseMiddleware<TVPApiRequestExecutor>();

            return app;
        }
    }
}
