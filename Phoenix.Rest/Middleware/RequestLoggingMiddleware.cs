using KLogMonitor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Phoenix.Rest.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestLogStr = await FormatRequestLogStr(context.Request);
            _Logger.Debug(requestLogStr);

            await _next(context);
        }

        private async Task<string> FormatRequestLogStr(HttpRequest request)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Url: {request.GetDisplayUrl()}");
            builder.Append("Headers:");
            
            request.EnableBuffering();

            foreach (var header in request.Headers)
            {
                builder.Append($"{header.Key}:{header.Value}, ");
            }
            
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            request.Body.Seek(0, SeekOrigin.Begin);
            var bodyAsText = Encoding.UTF8.GetString(buffer);


            builder.AppendLine($"Body: {bodyAsText}");

            return builder.ToString();
        }
    }
}
