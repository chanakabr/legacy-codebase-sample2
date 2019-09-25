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
using System.ServiceModel;
using WebAPI.WebServices;
using WS_Notification;

namespace IngetsNetCore
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.TryAddSingleton<WS_Catalog.Iservice, CatalogService>();
            services.TryAddSingleton<WS_Notification.INotificationService, NotificationService>();
            services.TryAddSingleton<ISocialService, SocialService>();
            services.TryAddSingleton<IPricingService, PricingService>();
            services.TryAddSingleton<IApiService, ApiService>();
            services.TryAddSingleton<IBillingService, BillingService>();
            services.TryAddSingleton<IConditionalAccessService, ConditionalAccessService>();
            services.TryAddSingleton<IDomainsService, DomainsService>();
            services.TryAddSingleton<IUsersService, UsersService>();

            var provider = services.BuildServiceProvider();
            var htttpContextAccessor = provider.GetService<IHttpContextAccessor>();
            System.Web.HttpContext.Configure(htttpContextAccessor);

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSoapEndpoint<WS_Catalog.Iservice>("/ws_catalog_service.svc", 
                new BasicHttpBinding(), SoapSerializer.DataContractSerializer, caseInsensitivePath: true);

            app.UseSoapEndpoint<WS_Catalog.Iservice>("/catalog.svc", 
                new BasicHttpBinding(), SoapSerializer.DataContractSerializer, caseInsensitivePath: true);

            app.UseSoapEndpoint<INotificationService>("/notification.svc", 
                new BasicHttpBinding(), SoapSerializer.DataContractSerializer, caseInsensitivePath: true);

            app.UseSoapEndpoint<INotificationService>("/ws_notification_service.svc", 
                new BasicHttpBinding(), SoapSerializer.DataContractSerializer, caseInsensitivePath: true);

            app.UseSoapEndpoint<ISocialService>("/ws_social_module.asmx", 
                new BasicHttpsBinding(), SoapSerializer.XmlSerializer, caseInsensitivePath: true);

            app.UseSoapEndpoint<IApiService>("/api.asmx", 
                new BasicHttpsBinding(), SoapSerializer.XmlSerializer, caseInsensitivePath: true);

            app.UseSoapEndpoint<IBillingService>("/ws_billing_module.asmx", 
                new BasicHttpsBinding(), SoapSerializer.XmlSerializer, caseInsensitivePath: true);

            app.UseSoapEndpoint<IConditionalAccessService>("/ws_cas_module.asmx", 
                new BasicHttpsBinding(), SoapSerializer.XmlSerializer, caseInsensitivePath: true);

            app.UseSoapEndpoint<IDomainsService>("/ws_domains_module.asmx", 
                new BasicHttpsBinding(), SoapSerializer.XmlSerializer, caseInsensitivePath: true);

            app.UseSoapEndpoint<IUsersService>("/ws_users_module.asmx", 
                new BasicHttpsBinding(), SoapSerializer.XmlSerializer, caseInsensitivePath: true);

            app.UseSoapEndpoint<IPricingService>("/ws_pricing_module.asmx", 
                new BasicHttpsBinding(), SoapSerializer.XmlSerializer, caseInsensitivePath: true);

            app.UseMvc();
        }
    }
}
