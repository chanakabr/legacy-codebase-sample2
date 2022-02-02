using Microsoft.AspNetCore.Builder;
using Phx.Lib.Log;

namespace Core.Middleware
{
    public static class KloggerSessionBuilder
    {
        // TODO: this value has been removed from Klogger and PHx.Lib.Log, need to replace with RequestContextConstants.SESSION_ID_KEY
        // after bringing Phx.Lib.Rest
        public const string SESSION_HEADER_KEY = "x-kaltura-session-id";
        public const string LEGACY_SESSION_HEADER_KEY = Constants.REQUEST_ID_KEY;

        public static IApplicationBuilder UseKloggerSessionIdBuilder(this IApplicationBuilder app)
        {
            return app.Use(async (context, _next) =>
            {
                string sessionId;
                if (context.Request.Headers.TryGetValue(SESSION_HEADER_KEY, out var sessionHeader))
                {
                    sessionId = sessionHeader;
                }
                else if (context.Request.Headers.TryGetValue(LEGACY_SESSION_HEADER_KEY, out var legacySessionHeader))
                {
                    sessionId = legacySessionHeader;
                }
                else
                {
                    sessionId = context.TraceIdentifier;
                }

                context.Items[SESSION_HEADER_KEY] = sessionId;

                context.Items[Constants.HOST_IP] = context.Connection.RemoteIpAddress;

                KLogger.SetRequestId(sessionId);
                KLogger.LogContextData[Constants.HOST_IP] = context.Connection.RemoteIpAddress;

                context.Response.Headers["X-Kaltura-Session"] = sessionId;

                await _next.Invoke();
            });
        }
    }
}
