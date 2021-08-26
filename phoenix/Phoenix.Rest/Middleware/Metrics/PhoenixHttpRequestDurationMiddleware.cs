using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ApiObjects.Response;
using Microsoft.AspNetCore.Http;
using Prometheus;
using WebAPI.Managers.Models;

namespace Phoenix.Rest.Middleware.Metrics
{
    public class PhoenixHttpRequestDurationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICollector<IHistogram> _metric;
        private static readonly int Ok = (int) eResponseStatus.OK;

        public PhoenixHttpRequestDurationMiddleware(RequestDelegate next, CollectorRegistry registry)
        {
            _next = next;
            _metric = Prometheus.Metrics.WithCustomRegistry(registry)
                .CreateHistogram("http_request_duration_seconds",
                    "The duration of HTTP requests processed by an ASP.NET Core application.",
                    new HistogramConfiguration
                    {
                        Buckets = Histogram.ExponentialBuckets(0.001, 2, 16),
                        LabelNames = new[] {"code_http", "code", "code_message", "method", "path"}
                    });
        }
        
        public async Task Invoke(HttpContext context)
        {
            var stopWatch = Stopwatch.StartNew();
            
            try
            {
                await _next(context);
            }
            finally
            {
                _metric.WithLabels(ExtractContextLabels(context)).Observe(stopWatch.Elapsed.TotalSeconds);
            }
        }

        private static string[] ExtractContextLabels(HttpContext context)
        {
            // Please, note that we're not parsing MultiRequest inner structure.
            // So, it'll look like code="200" method="POST" service="multirequest" action="".
            var (code, message) = ExtractCode(context);
            var path = ExtractPath(context);
            return new[]
            {
                context.Response.StatusCode.ToString().ToLowerInvariant(),
                code.ToString(),
                message,
                (context.Request.Method ?? string.Empty).ToLowerInvariant(),
                path
            };
        }

        private static string ExtractPath(HttpContext context)
        {
            // it's possible to send custom "pathData"
            // e.g. "/api_v3/service/system/action/getVersion/customId/1483/anotherOne/20210820"
            // it'll generate a lot of unique paths.
            // that's why only main part is taken /service/{xxx}/action/{yyy} or /service/{xxx} 
            const int phoenixPathMainParts = 5;
            var parts = context.Request.Path.Value
                .ToLowerInvariant()
                .Split("/", StringSplitOptions.RemoveEmptyEntries);
            return "/" + string.Join('/', parts.Take(Math.Min(parts.Length, phoenixPathMainParts)));
        }

        private static (int code, string message) ExtractCode(HttpContext context)
        {
            var isCodeParsed = context.Items.TryGetValue(PhoenixExceptionHandler.INTERNAL_ERROR_CODE, out var code);
            if (!isCodeParsed)
            {
                return (Ok, "OK");
            }

            if (ParseEnum(typeof(eResponseStatus), code, out var result))
            {
                return result;
            }
            
            if (ParseEnum(typeof(StatusCode), code, out result))
            {
                return result;
            }

            return result;
        }

        private static bool ParseEnum(Type enumType, object code, out (int code, string message) errorCode)
        {
            if (Enum.TryParse(enumType, code.ToString(), out var status) && Enum.IsDefined(enumType, status))
            {
                errorCode = ((int) status, status.ToString());
                return true;
            }

            errorCode = (-1, null);
            return false;
        }
    }
}