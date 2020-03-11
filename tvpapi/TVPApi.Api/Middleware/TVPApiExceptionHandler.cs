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
using Core.Middleware;
using System.Net;

namespace TVPApi.Web.Middleware
{
    public class TVPApiExceptionHandler : IApiExceptionHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public Task<ApiExceptionHandlerResponse> FormatErrorResponse(HttpContext context, Exception ex)
        {
            log.Error("Error when processing request.", ex);
            var response = new ApiExceptionHandlerResponse
            {
                ContentType = "application/json",
                HttpStatusCode = (int)HttpStatusCode.OK,
                Reponse = "Error",
            };
            return Task.FromResult(response);

        }
    }
}