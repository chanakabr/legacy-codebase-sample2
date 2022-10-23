using AutoMapper.Configuration;
using Core.Middleware;
using Grpc.controllers;
using GrpcAPI.controllers;
using GrpcAPI.Services;
using GrpcAPI.Utils;
using HealthCheck;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OTT.Lib.Metrics.Extensions;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Phoenix.Grpc
{
    public class Startup
    {
        public IConfiguration Configuration { get;}
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            MapperConfigurationExpression cfg = new MapperConfigurationExpression();
            GrpcMapping.RegisterMappings(cfg);
            AutoMapper.Mapper.Initialize(cfg);
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddMetricHttpEndpoint(8080);
            services.AddKalturaHealthCheckService();
            services.AddCoreConcurrencyLimiter();
            // this can be transient or scoped as well depending on your needs
            services.AddHttpContextAccessor();
            services.AddStaticHttpContextAccessor();
            services.AddSingleton<IEntitlementService, EntitlementService>();
            services.AddSingleton<IHouseholdService, HouseholdService>();
            services.AddSingleton<IPricingService, PricingService>();
            services.AddSingleton<ICatalogService, CatalogService>();
            services.AddSingleton<IAssetRuleService, AssetRuleService>();
            services.AddSingleton<IGroupAndConfigurationService, GroupAndConfigurationService>();
            services.AddSingleton<ISegmentService, SegmentService>();
        }
        

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCoreConcurrencyLimiter();
            // this will connect request metrics middleware to your existing webhost
            app.UseRequestMetricsMiddleware();            
            // this will connect a /metrics endpoint to your existing webHost 
            app.UseMetrics();
            app.UseRouting();
            app.UseKloggerSessionIdBuilder();
            
            //mapping of grpc controllers
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<HealthCheckController>();
                endpoints.MapGrpcService<PhoenixController>();
            });
        }
    }
}
