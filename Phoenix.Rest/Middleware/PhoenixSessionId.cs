using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Phoenix.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Phoenix.Rest.Middleware
{
    public class PhoenixSessionId
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public const string SESSION_HEADER_KEY = KLogMonitor.Constants.REQUEST_ID_KEY;
        private readonly RequestDelegate _Next;

        public PhoenixSessionId(RequestDelegate next)
        {
            _Next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Guid sessionId;
            if (context.Request.Headers.TryGetValue(SESSION_HEADER_KEY, out var sessionHeader))
            {
                sessionId = new Guid(sessionHeader);
            }
            else
            {
                sessionId = Guid.NewGuid();
            }

            _Logger.Debug($"API Start SessionId:[{sessionId}]");
           
            context.Items[SESSION_HEADER_KEY] = sessionId.ToString();
            KLogger.SetRequestId(sessionId.ToString());

            var phoenixCtx = new PhoenixRequestContext();
            context.Items[PhoenixRequestContext.PHOENIX_REQUEST_CONTEXT_KEY] = phoenixCtx;
            phoenixCtx.SessionId = sessionId;
            phoenixCtx.RequestDate = DateTime.UtcNow;
            context.Response.Headers["X-Kaltura-Session"] = sessionId.ToString();
            
            await _Next(context);

            _Logger.Debug($"API Start SessionId:[{sessionId}], execution time:[{phoenixCtx.ExecutionTime:0.0000}]");
        }
    }
}
