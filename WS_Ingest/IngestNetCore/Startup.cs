using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SoapCore;
using Ingest;
using System.ServiceModel;
using Core.Middleware;
using System.Reflection;
using HealthCheck;
using ConfigurationManager;
using Core.Metrics.Web;

namespace IngetsNetCore
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddKalturaHealthCheckService();
            services.AddHttpContextAccessor();
            services.AddStaticHttpContextAccessor();
            services.TryAddSingleton<IService, IngestService>();            
            services.AddMvc(x => x.EnableEndpointRouting = false);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseHealthCheck("/health");

            // see explanation inside class regarding BOM
            app.UseBomKiller();
			app.UseKloggerSessionIdBuilder();
            app.UseRequestLogger();

            app.AddPrometheus();

            BasicHttpBinding ingestBinding = new BasicHttpBinding() { ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas() { MaxStringContentLength = int.MaxValue } };
            app.UseSoapEndpoint<IService>("/Service.svc", ingestBinding, SoapSerializer.DataContractSerializer, caseInsensitivePath: true);

            app.UseMvc();


            app.MapEndpoint("GetVersion", versionApp =>
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
        }
    }
}
