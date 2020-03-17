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
    public static class RequestResponseLoggerMiddleware
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "RequestResponseLogger");

        public static IApplicationBuilder UseRequestResponseLogger(this IApplicationBuilder app)
        {
            app.Use(async (context, _next) =>
            {
                await _next.Invoke();
                var requestUrl = context.Request.GetDisplayUrl();
                var requestHeaders = GetRequestHeadersStr(context.Request);
                var requestLogStr = await FormatRequestLogStr(context.Request);
                var responseLogStr = await FormatResponseLogStr(context.Response);
                _Logger.Info($"{requestLogStr} {context.Response.StatusCode} {responseLogStr}");
                var builder = new StringBuilder();

            });
            return app;
        }

        private static async Task<string> FormatResponseLogStr(HttpResponse response)
        {
            string bodyAsText = await TryGetBodyStr(response.Body, Convert.ToInt32(response.ContentLength));
            //TODO: Add response headers...
            return bodyAsText;
        }

        private static async Task<string> FormatRequestLogStr(HttpRequest request)
        {
            request.EnableBuffering();
            string bodyAsText = await TryGetBodyStr(request.Body, Convert.ToInt32(request.ContentLength));

            builder.AppendLine($" {bodyAsText}");

            return builder.ToString();
        }

        private static string GetRequestHeadersStr(HttpRequest request)
        {
            var headersStrs = request.Headers.Select(h => $"{h.Key}:{h.Value}");
            return string.Join(",", headersStrs);
        }

        private static async Task<string> TryGetBodyStr(Stream bodyStream, int contentLength)
        {
            try
            {
                var buffer = new byte[contentLength];
                await bodyStream.ReadAsync(buffer, 0, buffer.Length);
                bodyStream.Seek(0, SeekOrigin.Begin);
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
