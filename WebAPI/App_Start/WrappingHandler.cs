using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using WebAPI.Filters.Exceptions;
using WebAPI.Models;

namespace WebAPI.App_Start
{
    public class WrappingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            return await BuildApiResponse(request, response);
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
                //This is a global unintentional 500 error

                HttpError error = content as HttpError;
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
