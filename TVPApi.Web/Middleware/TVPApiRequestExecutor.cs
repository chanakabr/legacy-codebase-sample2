using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Text;
using System.Threading.Tasks;
using TVPApi.Common;

namespace TVPApi.Web.Middleware
{
    public class TVPApiRequestExecutor
    {
        private readonly RequestDelegate _Next;
        private string _Response;

        public TVPApiRequestExecutor(RequestDelegate next)
        {
            _Next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var gateway = new JsonPostGateway();
                var request = context.Request;

                context.Response.OnStarting(HandleResponse, context);

                using (var streamReader = new HttpRequestStreamReader(request.Body, Encoding.UTF8))
                {
                    var body = await streamReader.ReadToEndAsync();
                    _Response = gateway.ProcessRequest(body);
                }
                
                await _Next(context);
            }
            catch (Exception ex)
            {
                
            }
        }

        private async Task HandleResponse(object ctx)
        {
            var context = ctx as HttpContext;

            await context.Response.WriteAsync(_Response);
        }
    }
}