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
            HttpContext.Current.Items.Add(Constants.REQUEST_ID_KEY, request.GetCorrelationId());

            // log request body
            log.DebugFormat("API Request - {0}, {1}", true,
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
            StatusCode subCode = StatusCode.OK;

            if (response.TryGetContentValue(out content) && !response.IsSuccessStatusCode)
            {
                //This is a global unintentional error

                HttpError error = content as HttpError;

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    subCode = StatusCode.Forbidden;
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    subCode = StatusCode.Unauthorized;
                else
                    subCode = StatusCode.Error;

                if (error != null)
                {
                    log.ErrorFormat("Request ID: {0}, exception: {1}", true, null,
                    request.GetCorrelationId().ToString(),                                   // 0
                    string.Concat(message, error.ExceptionMessage, error.StackTrace));       // 1

                    content = null;
                    message = error.ExceptionMessage;
#if DEBUG
                    message = string.Concat(message, error.ExceptionMessage, error.StackTrace);
#endif
                }
            }
            else if (!response.IsSuccessStatusCode)
            {
                message = response.ReasonPhrase;
                string status = await response.Content.ReadAsStringAsync();

                subCode = (StatusCode)Enum.Parse(typeof(StatusCode), status);
            }
            else
                message = "success";

            Guid reqID = request.GetCorrelationId();
            var newResponse = request.CreateResponse(response.StatusCode, new StatusWrapper(subCode, reqID, content, message));

            foreach (var header in response.Headers)
            {
                newResponse.Headers.Add(header.Key, header.Value);
            }

            return newResponse;
        }
    }
}
