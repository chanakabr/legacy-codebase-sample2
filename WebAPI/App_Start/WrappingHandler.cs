using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using KLogMonitor;
using WebAPI.Models;
using WebAPI.Models.General;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using System.Runtime.Serialization;

namespace WebAPI.App_Start
{
    public class WrappingHandler : DelegatingHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // get request ID
            HttpContext.Current.Items[Constants.REQUEST_ID_KEY] = request.GetCorrelationId();

            // log request body
            log.DebugFormat("API Request - {0} {1}",
                            request.RequestUri.OriginalString,            // 0
                            await request.Content.ReadAsStringAsync());   // 1

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_CLIENT_API_START))
            {
                //let other handlers process the request
                var response = await base.SendAsync(request, cancellationToken);
                var wrappedResponse = await BuildApiResponse(request, response, float.Parse(km.ExecutionTime));
                return wrappedResponse;
            }
        }

        private async static Task<HttpResponseMessage> BuildApiResponse(HttpRequestMessage request, HttpResponseMessage response, float executionTime)
        {
            if (request.GetRouteData().Route.RouteTemplate.ToLower().Contains("swagger"))
                return response;

            object content = null;
            string message = "";
            int subCode = (int)StatusCode.OK;
            response.TryGetContentValue(out content);

            if (content is ApiException.ExceptionPayload && ((ApiException.ExceptionPayload)content).code != 0)
            {
                WebAPI.Exceptions.ApiException.ExceptionPayload payload = content as WebAPI.Exceptions.ApiException.ExceptionPayload;

                subCode = payload.code;
                message = HandleError(payload.error.ExceptionMessage, payload.error.StackTrace);
                content = prepareExceptionResponse(payload.code, message);
            }
            else if (response.IsSuccessStatusCode)
            {
                message = "success";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                //Web API Bad Request global error
                content = prepareExceptionResponse((int)StatusCode.BadRequest, "Bad request");
                response.StatusCode = System.Net.HttpStatusCode.OK;
                message = HandleError("Bad Request", "");
            }
            else
            {
                content = prepareExceptionResponse((int)StatusCode.Error, "Unknown error");
                subCode = (int)StatusCode.Error;
                message = HandleError("Unknown error", "");
            }

            //We never return 500. even on errors/warning
            response.StatusCode = System.Net.HttpStatusCode.OK;
            Guid reqID = request.GetCorrelationId();
            var newResponse = request.CreateResponse(response.StatusCode, new StatusWrapper(subCode, reqID, executionTime, content, message));

            newResponse.Headers.Add("X-Kaltura-Session", reqID.ToString());
            newResponse.Headers.Add("X-Me", Environment.MachineName);

            foreach (var header in response.Headers)
            {
                newResponse.Headers.Add(header.Key, header.Value);
            }

            return newResponse;
        }

        [DataContract(Name = "error")]
        public class KalturaAPIExceptionWrapper
        {
            public KalturaAPIException error { get; set; }
        }

        public class KalturaAPIException
        {
            [DataMember(Name = "objectType")]
            public string objectType { get { return this.GetType().Name; } set { } }
            public string code { get; set; }
            public string message { get; set; }
            public string[] args { get; set; }
        }

        public static KalturaAPIExceptionWrapper prepareExceptionResponse(int statusCode, string msg)
        {
            return new KalturaAPIExceptionWrapper() { error = new KalturaAPIException() { message = msg, code = statusCode.ToString() } };
        }

        public static string HandleError(string errorMsg, string stack)
        {
            string message = errorMsg;
#if DEBUG
            message = string.Concat(message, stack);
            log.ErrorFormat("{0}", message);
#else
            log.ErrorFormat("{0} {1}", message, stack);
#endif


            return message;
        }
    }
}
