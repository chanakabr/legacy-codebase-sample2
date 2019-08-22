using KLogMonitor;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TVPApi.Common;
using TVinciShared;

namespace TVPApi.Web.Middleware
{
    public class TVPApiRequestExecutor
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly RequestDelegate _Next;
        private string _Response;
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_CLIENT_API_START, null, null, null, null))
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
                    }
                }

                await _Next(context);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error when processing request. error = {0}", ex);
                _Response = "Error";
            }
        }

        private async Task HandleResponse(object ctx)
        {
            var context = ctx as HttpContext;
            var contentType = context.Response.ContentType;
            // always mark response as 200, that's how it worked on old tvpapi
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");

            //HttpContext.Current.Response.HeaderEncoding = HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
            //HttpContext.Current.Response.Charset = "utf-8";

            // Check if exception or error occurred
            object error = System.Web.HttpContext.Current.Items["Error"];
            string sError = null;

            if (error != null)
            {
                // Exception was thrown - write log
                if (error is Exception)
                {
                    sError = "Unknown error ";
                }
                // Error occurred - write log
                else
                {
                    sError = error as string;
                }
                // Return an error message to client
                if (contentType.Contains("xml"))
                {
                    string xml = string.Format("<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/' " + 
                        "xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><soap:Body><Error error='{0}'/>" + 
                        "</soap:Body></soap:Envelope>", sError);
                    _Response = xml;
                }
                else
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(new { Error = sError });
                    _Response = json;
                }
            }

            await context.Response.WriteAsync(_Response);
        }
    }
}