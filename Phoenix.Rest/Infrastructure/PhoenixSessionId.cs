using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Phoenix.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Phoenix.Rest.Infrastructure
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

        public async Task InvokeAsync(HttpContext context, IPhoenixRequestContext requestContext)
        {
            var sessionID = context.Request.Headers.TryGetValue(SESSION_HEADER_KEY, out var SessionHeader) ? SessionHeader.ToString() : Guid.NewGuid().ToString();
            context.Items[SESSION_HEADER_KEY] = sessionID;
            KLogger.SetRequestId(sessionID);
            requestContext.SessionId = sessionID;
            requestContext.RequestDate = DateTime.UtcNow;
            
            context.Response.Headers["X-Kaltura-Session"] = sessionID;

            await _Next(context);
        }
    }
}
