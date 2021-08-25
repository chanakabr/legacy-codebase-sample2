using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Phoenix.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Phoenix.WebServices
{
    public class WebServicesSessionId
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public const string NEW_SESSION_HEADER_KEY = Constants.SESSION_ID_KEY;
        public const string SESSION_HEADER_KEY = Constants.REQUEST_ID_KEY;
        private readonly RequestDelegate _Next;

        public WebServicesSessionId(RequestDelegate next)
        {
            _Next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (var km = new KMonitor(Events.eEvent.EVENT_CLIENT_API_START))
            {
                string sessionId;
                if (context.Request.Headers.TryGetValue(NEW_SESSION_HEADER_KEY, out var sessionHeader))
                {
                    sessionId = sessionHeader;
                }
                else if (context.Request.Headers.TryGetValue(SESSION_HEADER_KEY, out var legacySessionHeader))
                {
                    sessionId = legacySessionHeader;
                }
                else
                {
                    sessionId = context.TraceIdentifier;
                }

                context.Items[SESSION_HEADER_KEY] = sessionId;
                KLogger.SetRequestId(sessionId);

                var phoenixCtx = new PhoenixRequestContext();
                context.Items[PhoenixRequestContext.PHOENIX_REQUEST_CONTEXT_KEY] = phoenixCtx;
                phoenixCtx.SessionId = sessionId;
                phoenixCtx.RequestDate = DateTime.UtcNow;
                context.Response.Headers["X-Kaltura-Session"] = sessionId;
                phoenixCtx.ApiMonitorLog = km;
                await _Next(context);
            }
        }
    }
}
