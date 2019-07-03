using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Phoenix.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Phoenix.Rest.Middleware
{
    public class PhoenixCors
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string ALLOWED_ORIGINS = "*";
        private const string ALLOWED_HEADERS = "Origin, X-Requested-With, Content-Type, Accept, Range, Cache-Control";
        private const string ALLOWED_METHODS = "POST, GET, HEAD, OPTIONS";
        private const string EXPOSE_HEADERS = "Server, Content-Length, Content-Range, Date";

        private static readonly HashSet<string> ALLOWED_METHODS_HASHSET = ALLOWED_METHODS.Split(",").Select(m => m.Trim()).ToHashSet();

        
        private readonly RequestDelegate _Next;

        public PhoenixCors(RequestDelegate next)
        {
            _Next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", ALLOWED_ORIGINS);

            if (!ALLOWED_METHODS_HASHSET.Contains(context.Request.Method))
            {
                // TODO: arthir Add error response
                throw new Exception("HTTP Method not supports");
            }

            // IF method is not OPTIONS then pass to the next middleware.
            if (context.Request.Method == HttpMethod.Options.Method)
            {
                _Logger.Debug($"request method is OPTIONS, returing response, stopping piepline");
                context.Response.Headers.Add("Access-Control-Allow-Headers", ALLOWED_HEADERS);
                context.Response.Headers.Add("Access-Control-Allow-Methods", ALLOWED_METHODS);
                context.Response.Headers.Add("Access-Control-Expose-Headers", EXPOSE_HEADERS);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync("");
                return;
            }

            await _Next(context);
        }
    }
}
