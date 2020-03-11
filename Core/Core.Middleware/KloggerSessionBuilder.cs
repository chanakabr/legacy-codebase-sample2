using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using KLogMonitor;

namespace Core.Middleware
{
    public static class KloggerSessionBuilder
    {
        public const string SESSION_HEADER_KEY = KLogMonitor.Constants.REQUEST_ID_KEY;

        public static IApplicationBuilder UseKloggerSessionIdBuilder(this IApplicationBuilder app)
        {

            return app.Use(async (context, _next) =>
            {
                string sessionId;
                if (context.Request.Headers.TryGetValue(SESSION_HEADER_KEY, out var sessionHeader))
                {
                    sessionId = sessionHeader;
                }
                else
                {
                    sessionId = context.TraceIdentifier;
                }
                context.Items[SESSION_HEADER_KEY] = sessionId.ToString();
                
                context.Items[KLogMonitor.Constants.HOST_IP] = context.Connection.RemoteIpAddress;

                KLogger.SetRequestId(sessionId.ToString());
                KLogger.LogContextData[KLogMonitor.Constants.HOST_IP] = context.Connection.RemoteIpAddress;
                
                context.Response.Headers["X-Kaltura-Session"] = sessionId.ToString();
                
                await _next.Invoke();

            });
        }
    }
}
