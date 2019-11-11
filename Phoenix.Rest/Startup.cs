using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Phoenix.Rest.Middleware;

namespace Phoenix.Rest
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigurePhoenix();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // support multiple prefix slashes
            app.MapWhen(context =>
            {
                var arr = context.Request.Path.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries);
                return arr != null && arr.Length > 0 && arr[0].Equals("api_v3", StringComparison.OrdinalIgnoreCase);
            }, apiApp =>
            {
                apiApp.UsePhoenix();
            });
            app.Use(async (context, next) =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await context.Response.WriteAsync("");
            });

            WebAPI.Utils.LogReloader.GetInstance().Initiate();
        }
    }
}
