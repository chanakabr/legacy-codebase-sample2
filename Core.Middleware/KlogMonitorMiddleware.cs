using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using KLogMonitor;

namespace Core.Middleware
{
    public static class KlogMonitorMiddleware
    {
        public const string SESSION_HEADER_KEY = KLogMonitor.Constants.REQUEST_ID_KEY;

        public static IApplicationBuilder UseKlogerMonitor(this IApplicationBuilder app)
        {

            return app.Use(async (context, _next) =>
            {
                using (var km = new KMonitor(Events.eEvent.EVENT_CLIENT_API_START))
                {
                    await _next.Invoke();
                }

            });
        }
    }
}
