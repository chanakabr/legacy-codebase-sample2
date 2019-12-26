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

namespace IngetsNetCore
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<IService, IngestService>();            
            services.AddMvc(x => x.EnableEndpointRouting = false);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSoapEndpoint<IService>("/Service.svc", new BasicHttpBinding(), SoapSerializer.DataContractSerializer, caseInsensitivePath: true);
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
