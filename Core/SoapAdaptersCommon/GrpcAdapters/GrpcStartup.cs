using Core.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SoapAdaptersCommon.GrpcAdapters.Implementation;
using SoapAdaptersCommon.Middleware;

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

            services.AddGrpc(o => { o.Interceptors.Add<AdapterRequestInterceptor>(); });
            services.TryAddSingleton<AdapterRequestContextAccessor>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseKloggerSessionIdBuilder();
            app.UseAdapterRequestContextAccessor();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<SSOAdapterGRPCImplementation>();
                endpoints.MapGrpcService<PlaybackAdapterGRPCImplementation>();
            });
        }
    }
}