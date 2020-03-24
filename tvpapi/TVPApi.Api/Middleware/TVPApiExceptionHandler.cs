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
    public class TVPApiExceptionHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly RequestDelegate _Next;
        private string _Response;
        private readonly IHostingEnvironment _Host;

        public TVPApiExceptionHandler(RequestDelegate next, IHostingEnvironment host)
        {
            _Next = next;
            _Host = host;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _Next(context);
            }
            catch (Exception ex)
            {
                log.Error("Error when processing request.", ex);
                _Response = "Error";
            }
        }
    }
}