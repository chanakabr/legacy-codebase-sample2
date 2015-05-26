using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;
using WebAPI.Models;

namespace WebAPI.App_Start
{
    public class WrappingHandler : DelegatingHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //logging request body
            string requestBody = await request.Content.ReadAsStringAsync();
            log.Debug(requestBody);

            //let other handlers process the request
            var response = await base.SendAsync(request, cancellationToken);           
            var wrapped = await BuildApiResponse(request, response);

            //await (wrapped.Content ?? new StringContent("")).ReadAsStringAsync().ContinueWith(x =>
            //{
            //    log.Debug(x.Result);
            //});
            
            return wrapped;
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
