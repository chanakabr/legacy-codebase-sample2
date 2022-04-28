using Core.Middleware;
using Grpc.controllers;
using Grpc.HealthCheck;
using GrpcAPI.controllers;
using GrpcAPI.Services;
using GrpcAPI.Utils;
using HealthCheck;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Phoenix.Grpc
{
    public class Startup
    {
        public IConfiguration Configuration { get;}
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            GrpcMapping.RegisterMappings();
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddKalturaHealthCheckService();
            // this can be transient or scoped as well depending on your needs
            services.AddHttpContextAccessor();
            services.AddStaticHttpContextAccessor();
            services.AddSingleton<IEntitlementService, EntitlementService>();
            services.AddSingleton<IHouseholdService, HouseholdService>();
            services.AddSingleton<IPricingService, PricingService>();
            services.AddSingleton<ICatalogService, CatalogService>();
            services.AddSingleton<IAssetRuleService, AssetRuleService>();
            services.AddSingleton<IGroupAndConfigurationService, GroupAndConfigurationService>();
        }
        

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseKloggerSessionIdBuilder();
            
            //mapping of grpc controllers
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<HealthCheckController>();
                endpoints.MapGrpcService<PhoneixController>();
            });
        }
    }
}