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
            object content = null;
            string errorMessage = "";
            StatusCode subCode = StatusCode.OK;

            if (!response.IsSuccessStatusCode)
            {
                errorMessage = response.ReasonPhrase;
                string status = await response.Content.ReadAsStringAsync();

                subCode = (StatusCode) Enum.Parse(typeof(StatusCode), status);
            }
            else if (response.TryGetContentValue(out content) && !response.IsSuccessStatusCode)
            {
                //This is a global unintentional 500 error

                HttpError error = content as HttpError;
                subCode = StatusCode.Error;

                if (error != null)
                {
                    content = null;
                    errorMessage = error.ExceptionMessage;
#if DEBUG
                    errorMessage = string.Concat(errorMessage, error.ExceptionMessage, error.StackTrace);
#endif
                }
            }

            var newResponse = request.CreateResponse(response.StatusCode, new StatusWrapper(subCode, content, errorMessage));

            foreach (var header in response.Headers)
            {
                newResponse.Headers.Add(header.Key, header.Value);
            }

            return newResponse;
        }
    }
}
