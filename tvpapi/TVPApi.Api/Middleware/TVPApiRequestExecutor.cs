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
using System.IO;
using System.Net;

namespace TVPApi.Web.Middleware
{
    public class TVPApiRequestExecutor
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly RequestDelegate _Next;
        private readonly IWebHostEnvironment _Host;

        public TVPApiRequestExecutor(RequestDelegate next, IWebHostEnvironment host)
        {
            _Next = next;
            _Host = host;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // get action name
                var queryString = context.Request.GetQueryString();

                if (queryString != null && queryString["m"] != null)
                {
                    var m = queryString["m"];
                    context.Items[KLogMonitor.Constants.ACTION] = m;
                    KLogger.SetAction(m);
                }
                // get user agent
                var userAgent = context.Request.GetUserAgentString();

                if (userAgent != null)
                    context.Items[KLogMonitor.Constants.CLIENT_TAG] = userAgent;

                // get host IP
                if (context.Connection.RemoteIpAddress != null)
                {
                    KLogger.LogContextData[KLogMonitor.Constants.HOST_IP] = context.Connection.RemoteIpAddress;
                    context.Items[KLogMonitor.Constants.HOST_IP] = context.Connection.RemoteIpAddress;
                }

                context.Items["ContentRootPath"] = _Host.ContentRootPath;
                context.Items["WebRootPath"] = _Host.WebRootPath;

                var gateway = new JsonPostGateway();
                var request = context.Request;

                context.Response.OnStarting(HandleResponse, context);

                using (var streamReader = new StreamReader(request.Body, Encoding.UTF8))
                {
                    var body = await streamReader.ReadToEndAsync();
                    var response = gateway.ProcessRequest(body);
                    context.Items["TVPApi_Response"] = response;
                }

                await _Next(context);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error when processing request. error = {0}", ex);
                context.Items["TVPApi_Response"] = "Error";
            }
        }

        private async Task HandleResponse(object ctx)
        {
            var context = ctx as HttpContext;
            var contentType = context.Response.ContentType;
            var response = context.Items["TVPApi_Response"]?.ToString();

            // always mark response as 200, that's how it worked on old tvpapi
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");

            // TODO : Understand if it is meaningful in .net core response 
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
                    log.Error("HandleResponse, exception was thrown during the request execution", error as Exception);
                    sError = "Unknown error ";
                }
                // Error occurred - write log
                else
                {
                    sError = error as string;
                }
                // Return an error message to client
                if (contentType != null && contentType.Contains("xml"))
                {
                    string xml = string.Format("<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/' " +
                        "xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'><soap:Body><Error error='{0}'/>" +
                        "</soap:Body></soap:Envelope>", sError);
                    response = xml;
                }
                else
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(new { Error = sError });
                    response = json;
                }

                // clear error
                System.Web.HttpContext.Current.Items.Remove("Error");
            }

            // Check for Status code
            if (context.Items.ContainsKey("StatusCode"))
            {
                context.Response.Body = new MemoryStream();
                context.Response.StatusCode = (int)context.Items["StatusCode"];
            }

            if (response == null)
            {
                response = string.Empty;
            }

            await context.Response.WriteAsync(response);
        }
    }
}