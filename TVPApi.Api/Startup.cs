using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ConfigurationManager;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TVPApi.Web.Middleware;
using Core.Middleware;

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
            services.AddCoreConcurrencyLimiter();
            services.AddHttpContextAccessor();

            var provider = services.BuildServiceProvider();
            var httpContextAccessor = provider.GetService<IHttpContextAccessor>();
            System.Web.HttpContext.Configure(httpContextAccessor);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Tvinci.Data.Loaders.CatalogRequestManager.SignatureKey = ApplicationConfiguration.WebServicesConfiguration.Catalog.SignatureKey.Value;
            app.MapEndpoint("Gateways/JsonPostGW.aspx", apiApp =>
            {
                apiApp.UseConcurrencyLimiter();
                apiApp.UseTvpApi();
            });

            app.Use(async (context, next) =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await context.Response.WriteAsync("");
            });
        }
    }
}
