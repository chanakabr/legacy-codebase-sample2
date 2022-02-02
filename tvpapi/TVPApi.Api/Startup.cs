using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Phx.Lib.Appconfig;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TVPApi.Web.Middleware;
using Core.Middleware;
using System.Reflection;
using Core.Metrics.Web;
using HealthCheck;

namespace TVPApi.Web
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
            services.ConfigureTvpapi();
            services.AddKalturaHealthCheckService();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            
            Tvinci.Data.Loaders.CatalogRequestManager.SignatureKey = ApplicationConfiguration.Current.WebServicesConfiguration.Catalog.SignatureKey.Value;

            app.UseHealthCheck("/health");
            
            app.AddPrometheus();

            app.MapEndpoint("Gateways/JsonPostGW.aspx", apiApp =>
            {
                apiApp.UseTvpApi();
            });

            app.MapEndpoint("Gateways/GetVersion", versionApp =>
            {
                versionApp.Run((ctx) =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentType = "application/json; charset=utf-8";
                    ctx.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                    var currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    return ctx.Response.WriteAsync("{\"result\":\"" + currentVersion + "\"}");
                });
            });

            app.Use(async (context, next) =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await context.Response.WriteAsync("");
            });

            KLogMonitor.ConfigurationReloader.LogReloader.GetInstance().Initiate("tvpapi_log_configuration");
        }
    }
}
