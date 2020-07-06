using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var envEnableLogging = Environment.GetEnvironmentVariable("ENABLE_REQUEST_RESPONSE_LOGGING") ?? "false";
            var isRequestResponseLoggingEnabled = envEnableLogging.Equals("true", StringComparison.OrdinalIgnoreCase) || envEnableLogging.Equals("1", StringComparison.OrdinalIgnoreCase);
            if (!isRequestResponseLoggingEnabled)
            {
                // Logging is disabled, do not register middleware and return the app as is
                return app;
            }

            app.Use(async (context, _next) =>
            {
                
                // Get Request Information
                context.Request.EnableBuffering();
                var traceId = context.Request.Headers["X-Trace-Id"];
                var requestUrl = context.Request.GetDisplayUrl();
                var requestHeadersStr = GetHeadersStr(context.Request.Headers);
                var requestBodyStr = await TryGetBodyStr(context.Request.Body, Convert.ToInt32(context.Request.ContentLength));

                // Prepare Response Capturing (Replace the response stream with a memeory stream so that we can re-read it later)
                var originalResponseStream = context.Response.Body;
                var memoryResponseStream = new MemoryStream();
                context.Response.Body = memoryResponseStream;

                // On Repsonse starting delegate will read the memory stream and log it, then will redirect the memory copy stream of the response
                // To the origianl response stream the client is waiting for
                context.Response.OnStarting(async () =>
                {
                    memoryResponseStream.Seek(0, SeekOrigin.Begin);
                    await memoryResponseStream.CopyToAsync(originalResponseStream);

                    // Logging req\resp async after the response returned
                    _ = Task.Run(async () =>
                    {
                        requestBodyStr = requestBodyStr.Replace("\n", string.Empty).Replace("\t", string.Empty).Replace("\r", string.Empty);
                        var responseBodyStr = await TryGetBodyStr(context.Response.Body, Convert.ToInt32(context.Response.Body.Length));
                        var responseHeadersStr = GetHeadersStr(context.Response.Headers);
                        var responseStatusStr = context.Response.StatusCode.ToString();
                        await memoryResponseStream.DisposeAsync();
                        _Logger.Info($"{traceId} | {requestUrl} | {requestHeadersStr} | {requestBodyStr} | {responseStatusStr} | {responseHeadersStr} | {responseBodyStr}");
                    });

                });

                await _next.Invoke();
            });
            return app;
        }

        private static string GetHeadersStr(IHeaderDictionary headers)
        {
            var headersStrs = headers.Select(h => $"{h.Key}:{h.Value}");
            return string.Join(",", headersStrs);
        }

        private static async Task<string> TryGetBodyStr(Stream bodyStream, int contentLength)
        {
            try
            {

                var buffer = new byte[contentLength];
                bodyStream.Seek(0, SeekOrigin.Begin);
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
