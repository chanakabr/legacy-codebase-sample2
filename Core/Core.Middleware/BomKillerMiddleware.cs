using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Phx.Lib.Log;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Core.Middleware
{
    public static class BomKillerMiddleware
    {
        /// <summary>
        /// Please read this page: https://www.freecodecamp.org/news/a-quick-tale-about-feff-the-invisible-character-cd25cd4630e7/
        /// SoapCore adds a hidden character to the response: FEFF, an invisible UTF-8 that ruins XML parsing. It causes several erros
        /// and the only way we found to solve it is like this: rewriting the response to the stream with UTF8 encoding. This omits the hidden character
        /// from the response.
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
