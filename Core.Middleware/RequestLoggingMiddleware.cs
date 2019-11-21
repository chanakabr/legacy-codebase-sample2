using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Core.Middleware
{
    public static class RequestLoggingMiddleware
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static IApplicationBuilder UseRequestLogger(this IApplicationBuilder app)
        {
            app.Use(async (context, _next) =>
            {
                var requestLogStr = await FormatRequestLogStr(context.Request);
                _Logger.DebugFormat("{0}", requestLogStr);

                await _next.Invoke();

            });
            return app;
        }

        private static async Task<string> FormatRequestLogStr(HttpRequest request)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Url: {request.GetDisplayUrl()}");
            builder.AppendLine($"Headers: {GetRequestHeadersStr(request)}");

            request.EnableBuffering();
            string bodyAsText = await TryGetRequestBodyStr(request);

            builder.AppendLine($"Body: {bodyAsText}");

            return builder.ToString();
        }

        private static string GetRequestHeadersStr(HttpRequest request)
        {
            var headersStrs = request.Headers.Select(h => $"{h.Key}:{h.Value}");
            return string.Join(",", headersStrs);
        }

        private static async Task<string> TryGetRequestBodyStr(HttpRequest request)
        {
            try
            {
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadAsync(buffer, 0, buffer.Length);
                request.Body.Seek(0, SeekOrigin.Begin);
                var bodyAsText = Encoding.UTF8.GetString(buffer);
                return bodyAsText;
            }
            catch (Exception e)
            {
                _Logger.Error("Erorr while reading request body", e);
                return "Error Reading Request Body, See Log for details";
            }
        }
    }
}
