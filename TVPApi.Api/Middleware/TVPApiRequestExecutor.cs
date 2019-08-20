using KLogMonitor;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TVPApi.Common;

namespace TVPApi.Web.Middleware
{
    public class TVPApiRequestExecutor
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly RequestDelegate _Next;
        private string _Response;
        private int _StatusCode;
        private readonly IHostingEnvironment _Host;

        public TVPApiRequestExecutor(RequestDelegate next, IHostingEnvironment host)
        {
            _Next = next;
            _Host = host;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                context.Items["ContentRootPath"] = _Host.ContentRootPath;
                context.Items["WebRootPath"] = _Host.WebRootPath;

                var gateway = new JsonPostGateway();
                var request = context.Request;

                context.Response.OnStarting(HandleResponse, context);

                using (var streamReader = new HttpRequestStreamReader(request.Body, Encoding.UTF8))
                {
                    var body = await streamReader.ReadToEndAsync();
                    _Response = gateway.ProcessRequest(body);
                    _StatusCode = 200;
                }
                
                await _Next(context);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error when processing request. error = {0}", ex);
                _StatusCode = 500;
            }
        }

        private async Task HandleResponse(object ctx)
        {
            var context = ctx as HttpContext;

            context.Response.StatusCode = _StatusCode;
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            
            //HttpContext.Current.Response.HeaderEncoding = HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
            //HttpContext.Current.Response.Charset = "utf-8";

            await context.Response.WriteAsync(_Response);
        }
    }
}