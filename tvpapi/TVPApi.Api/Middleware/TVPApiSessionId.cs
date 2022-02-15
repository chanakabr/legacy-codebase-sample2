using Phx.Lib.Log;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TVPApi.Web.Middleware
{
    public class TVPApiSessionId
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public const string SESSION_HEADER_KEY = Phx.Lib.Log.Constants.REQUEST_ID_KEY;
        private readonly RequestDelegate _Next;

        public TVPApiSessionId(RequestDelegate next)
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
            context.Items[SESSION_HEADER_KEY] = sessionId.ToString();
            KLogger.SetRequestId(sessionId.ToString());

            context.Response.Headers["X-Kaltura-Session"] = sessionId.ToString();
            await _Next(context);
        }
    }
}
