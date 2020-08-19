using Core.Middleware;
using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SoapAdaptersCommon.GrpcAdapters.Implementation;
using SoapAdaptersCommon.Middleware;
using System;
using System.Linq;
using System.Reflection;

namespace SoapAdaptersCommon.GrpcAdapters
{
    public class GrpcStartup
    {

        public static IServiceCollection ParentServiceCollection { get; internal set; }

        public void ConfigureServices(IServiceCollection services)
        {
            foreach (var parentService in ParentServiceCollection)
            {
                services.TryAdd(parentService);
            }

            services.AddGrpc();
            services.TryAddSingleton<AdapterRequestContextAccessor>();

            //services.TryAddSingleton<IGRPCNormlizedSSOAdapterService, SSOAdapterGRPCResponseNormalizer>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseKloggerSessionIdBuilder();
            app.UseAdapterRequestContextAccessor();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<SSOAdapterGRPCImplementation>();
            });
        }
    }
}