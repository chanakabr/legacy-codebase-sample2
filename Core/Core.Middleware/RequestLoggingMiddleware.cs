using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Core.Middleware
{
    public static class RequestLoggingMiddleware
    {
        private const string MASK = "*****";
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static IApplicationBuilder UseRequestLogger(this IApplicationBuilder app)
        {
            app.Use(async (context, _next) =>
            {
                var logLevel = KLogger.GetLogLevel();

                if (logLevel <= log4net.Core.Level.Debug)
                {
                    var requestLogStr = await FormatRequestLogStr(context.Request);
                    _Logger.DebugFormat("{0}", requestLogStr);
                }

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
            string result = string.Empty;

            try
            {
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadAsync(buffer, 0, buffer.Length);
                request.Body.Seek(0, SeekOrigin.Begin);
                var bodyAsText = Encoding.UTF8.GetString(buffer);

                // with regex find all json fields that END with "password", "pass", "email" or "emailfield" -
                // then after they're found, replace only the VALUE of the field to be masked
                bodyAsText = Regex.Replace(bodyAsText, "(password|email|pass|emailfield)\"\\s*:\\s*(\"(?:\\\\\"|[^\"])*?\")", (match) =>
                {
                    // match.Groups:
                    // [0] = entire match. e.g. "key" : "value"
                    // [1] = key e.g. "password"
                    // [2] = value 

                    string replaceResult = string.Empty;

                    if (match != null && match.Groups.Count > 2)
                    {
                        replaceResult = $"{match.Groups[1].Value}\" : \"{MASK}\"";
                    }
                    else
                    {
                        replaceResult = match.ToString().Replace(match.Groups[match.Groups.Count - 1].Value, MASK);
                    }

                    return replaceResult;
                },
                RegexOptions.IgnoreCase);

                result = bodyAsText;
            }
            catch (Exception e)
            {
                _Logger.Error("Erorr while reading request body", e);
                result = "Error Reading Request Body, See Log for details";
            }

            return result;
        }
    }
}
