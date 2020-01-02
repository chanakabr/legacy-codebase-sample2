using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using KLogMonitor;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Core.Middleware
{
    public static class BomKillerMiddleware
    {
        /// <summary>
        /// LATERS
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseBomKiller(this IApplicationBuilder app)
        {
            return app.Use(async (context, _next) =>
            {
                using (var inMemoryResponse = new MemoryStream())
                {
                    var originalResponseStream = context.Response.Body;
                    context.Response.Body = inMemoryResponse;

                    await _next.Invoke();

                    inMemoryResponse.Seek(0, SeekOrigin.Begin);

                    using (var streamReader = new StreamReader(inMemoryResponse))
                    {
                        var bodyAsText = await streamReader.ReadToEndAsync();

                        context.Response.Body = originalResponseStream;
                        await context.Response.WriteAsync(bodyAsText, Encoding.UTF8);
                    }
                }
            });
        }
    }
}
