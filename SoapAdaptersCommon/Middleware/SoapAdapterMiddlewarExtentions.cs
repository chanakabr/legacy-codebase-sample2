using Core.Middleware;
using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SoapCore;
using SoapCore.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace SoapAdaptersCommon.Middleware
{
    public static class SoapAdapterMiddlewarExtentions
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly BasicHttpBinding DEFAULT_SOAP_ADAPTER_BINDING = new BasicHttpBinding()
        {
            ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
            {
                MaxStringContentLength = int.MaxValue
            },
        };

        /// <summary>
        /// Adds all the required servoces to use a SOAP adpater
        ///  - HtppContextAccessort and configure the StaticHttpContext
        ///  - Adds MVC (to map SOAP endpoints)
        ///  - Adds AdapterRequestContextExtractor as and IMessageInspector
        ///  - Adds SimpleFaultExceptionTransformer as the IFaultExceptionTransformer
        ///  - Adds AdapterRequestContextAccessor as the IAdapterRequestContextAccessor
        ///  - Adds all AdapterSettings that defined in the provided SoapAdapaterOptions
        /// </summary>
        public static IServiceCollection ConfigureSoapAdapaters(this IServiceCollection services, Action<SoapAdapatersOptions> configure = null)
        {
            services.AddHttpContextAccessor();
            services.AddStaticHttpContextAccessor();
            services.AddMvc(x => x.EnableEndpointRouting = false);
            services.TryAddSingleton<IMessageInspector, AdapterRequestContextExtractor>();
            services.TryAddSingleton<IFaultExceptionTransformer, SimpleFaultExceptionTransformer>();
            services.TryAddSingleton<IAdapterRequestContextAccessor, AdapterRequestContextAccessor>();

            var options = new SoapAdapatersOptions { };
            configure?.Invoke(options);

            services.TryAddSingleton<SoapAdapatersOptions>(options);

            foreach (var adapterConfig in options.Adapters)
            {
                _Logger.Info($"Configuring adapter: {adapterConfig}");
                services.TryAddSingleton(adapterConfig.AdapaterInterface, adapterConfig.AdapaterType);
            }

            return services;
        }

        /// <summary>
        /// Uses the following middlewares 
        ///  - BOM Killer - to avoid BOM in respons for JAVA clients
        ///  - KloggerSessionIdBuilder - init klogger context with TraceIdentifier 
        ///  - RequestLoggerMiddleware - log incomming requests
        ///  - Maps a GetVersion endpoint that returns a json result with current version of the executing assembly
        ///  - Maps all the injected AdapaterSettings in SoapAdapaterOptions when using ConfigureSoapAdapaters
        /// </summary>
        public static IApplicationBuilder UseSoapAdapaters(this IApplicationBuilder app)
        {
            app.UseBomKiller();
            app.UseKloggerSessionIdBuilder();
            app.UseRequestLogger();
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

            var adapterOptions = app.ApplicationServices.GetService<SoapAdapatersOptions>();
            if (adapterOptions != null)
            {
                foreach (var adapterConfig in adapterOptions.Adapters)
                {
                    _Logger.Info($"Setting SOAP endpoint adapter: {adapterConfig}");
                    app.MapAdapter(adapterConfig.AdapaterInterface, adapterConfig.EndpointUrl, adapterConfig.Binding, adapterConfig.SoapSerializer, adapterConfig.CaseInsensitivePath);
                }
            }

            return app;
        }

        /// <summary>
        /// Maps an adapter endpoint to a given url.
        /// </summary>
        /// <typeparam name="T">Adapter service contract interface</typeparam>
        /// <param name="url">Url endpoint of the Soap adapater service</param>
        /// <param name="binding">Binding to use, default null will use BasicHttpBinding with MaxStringContentLength=int.max</param>
        /// <param name="soapSerializer">serilizer to use</param>
        /// <param name="caseInsensitivePath">case sensativity match for url</param>
        public static IApplicationBuilder MapAdapter<T>(this IApplicationBuilder app, string url, Binding binding = null, SoapSerializer soapSerializer = SoapSerializer.DataContractSerializer, bool caseInsensitivePath = true)
        {
            app.MapAdapter(typeof(T), url, binding, soapSerializer, caseInsensitivePath);
            return app;
        }

        /// <summary>
        /// Maps an adapter endpoint to a given url.
        /// </summary>
        /// <param name="type">Adapter service contract interface</typeparam>
        /// <param name="url">Url endpoint of the Soap adapater service</param>
        /// <param name="binding">Binding to use, default null will use BasicHttpBinding with MaxStringContentLength=int.max</param>
        /// <param name="soapSerializer">serilizer to use</param>
        /// <param name="caseInsensitivePath">case sensativity match for url</param>
        public static IApplicationBuilder MapAdapter(this IApplicationBuilder app, Type type, string url, Binding binding = null, SoapSerializer soapSerializer = SoapSerializer.DataContractSerializer, bool caseInsensitivePath = true)
        {
            binding = binding ?? DEFAULT_SOAP_ADAPTER_BINDING;
            app.UseSoapEndpoint(type, url, binding, soapSerializer, caseInsensitivePath);
            return app;
        }
    }

    public class AdapterEndpointConfiguration
    {
        public Type AdapaterInterface { get; set; }
        public Type AdapaterType { get; set; }
        public string EndpointUrl { get; set; }
        public bool CaseInsensitivePath { get; set; }
        public Binding Binding { get; set; }
        public SoapSerializer SoapSerializer { get; set; }

        public override string ToString()
        {
            return $"{{{nameof(AdapaterInterface)}={AdapaterInterface}, {nameof(AdapaterType)}={AdapaterType}, {nameof(EndpointUrl)}={EndpointUrl}, {nameof(CaseInsensitivePath)}={CaseInsensitivePath.ToString()}, {nameof(Binding)}={Binding}, {nameof(SoapSerializer)}={SoapSerializer.ToString()}}}";
        }
    }

    public class SoapAdapatersOptions
    {
        internal List<AdapterEndpointConfiguration> Adapters { get; set; }
        public SoapAdapatersOptions()
        {
            Adapters = new List<AdapterEndpointConfiguration>();
        }

        /// <summary>
        /// Add an adapter implementation to the current Soap Adapater middelware
        /// </summary>
        /// <typeparam name="TService">adapater soap innterface (operation contract)</typeparam>
        /// <typeparam name="TImplementation">adapter implementation</typeparam>
        /// <param name="endpointUrl">url enpoint that will expose the service</param>
        /// <param name="binding">binding to use</param>
        /// <param name="soapSerializer">envelope serializer type</param>
        /// <param name="caseInsensitivePath">should url path be case insensitive</param>
        /// <returns></returns>
        public SoapAdapatersOptions AddAdapter<TService, TImplementation>(string endpointUrl, Binding binding = null, SoapSerializer soapSerializer = SoapSerializer.DataContractSerializer, bool caseInsensitivePath = true)
            where TService : class
            where TImplementation : class, TService
        {
            // TODO: Arthur - should we verify that Tservice is one of the Adapter interfaces ?
            var adapter = new AdapterEndpointConfiguration
            {
                AdapaterInterface = typeof(TService),
                AdapaterType = typeof(TImplementation),
                Binding = binding,
                CaseInsensitivePath = caseInsensitivePath,
                EndpointUrl = endpointUrl,
                SoapSerializer = soapSerializer,
            };

            Adapters.Add(adapter);

            return this;
        }


    }
}
