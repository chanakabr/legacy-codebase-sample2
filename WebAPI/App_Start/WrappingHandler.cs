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
            log.DebugFormat("API Request - \n{0}\n{1}",
                            request.RequestUri.OriginalString,            // 0
                            await request.Content.ReadAsStringAsync());   // 1

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_API_START))
            {
                //let other handlers process the request
                var response = await base.SendAsync(request, cancellationToken);
                var wrappedResponse = await BuildApiResponse(request, response);
                return wrappedResponse;
            }
        }

        private async static Task<HttpResponseMessage> BuildApiResponse(HttpRequestMessage request, HttpResponseMessage response)
        {
            if (request.GetRouteData().Route.RouteTemplate.ToLower().Contains("swagger"))
                return response;

            object content = null;
            string message = "";
            int subCode = (int)StatusCode.OK;

            if (response.TryGetContentValue(out content) && content is HttpError && !response.IsSuccessStatusCode)
            {
                //This is a global unintentional error

                HttpError error = content as HttpError;
                subCode = (int)StatusCode.Error;

                if (error != null)
                {
                    content = null;
                    message = handleError(error.ExceptionMessage, error.StackTrace);
                }
            }
            else if (!response.IsSuccessStatusCode && content != null)
            {
                WebAPI.Exceptions.ApiException.ExceptionPayload payload = content as WebAPI.Exceptions.ApiException.ExceptionPayload;

                subCode = payload.code;
                message = handleError(payload.error.ExceptionMessage, payload.error.StackTrace);
                content = null;
            }
            else if (response.IsSuccessStatusCode)
            {
                message = "success";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                //Web API Bad Request global error
                content = null;
                subCode = (int)StatusCode.BadRequest;
                message = handleError("Bad Request", "");
            }
            else
            {
                content = null;
                subCode = (int)StatusCode.Error;
                message = handleError("Unknown error", "");
            }

            Guid reqID = request.GetCorrelationId();
            var newResponse = request.CreateResponse(response.StatusCode, new StatusWrapper(subCode, reqID, content, message));

            foreach (var header in response.Headers)
            {
                newResponse.Headers.Add(header.Key, header.Value);
            }

            return newResponse;
        }

        private static string handleError(string errorMsg, string stack)
        {
            string message = "";
            string errMsg = string.Concat(errorMsg, stack);
#if DEBUG
            message = errMsg;
#endif
            log.ErrorFormat("{0}", errMsg);

            return message;
        }
    }
}
