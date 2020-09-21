using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HealthCheck
{
    public static class HealthCheck
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static void AddHealthCheckService(this IServiceCollection services, List<HealthCheckDefinition> definitions)
        {
            var healthCheckBuilder = services.AddHealthChecks();
            services.AddHttpClient();
            
            foreach (var definition in definitions)
            {
                switch (definition.Type)
                {
                    case HealthCheckType.SQL:
                        healthCheckBuilder = healthCheckBuilder.AddCheck<SQLServerHealthCheck>("sql");
                        break;
                    case HealthCheckType.CouchBase:
                        healthCheckBuilder = healthCheckBuilder.AddCheck<CouchbaseHealthCheck>("couchbase");
                        break;
                    case HealthCheckType.ElasticSearch:
                        healthCheckBuilder = healthCheckBuilder.AddCheck<ElasticSearchHealthCheck>("elastic_search");
                        break;
                    case HealthCheckType.RabbitMQ:
                        healthCheckBuilder = healthCheckBuilder.AddCheck<RabbitHealthCheck>("rabbitmq");
                        break;
                    case HealthCheckType.Redis:
                        healthCheckBuilder = healthCheckBuilder.AddCheck<RedisHealthCheck>("redis");
                        break;
                    case HealthCheckType.ThirdParty:
                        if (definition.Args.Length > 1 && definition.Args[0] != null && definition.Args[1] != null)
                        {
                            var thirdPartyName = definition.Args[0].ToString();
                            services.AddHttpClient(thirdPartyName, c =>
                            {
                                c.BaseAddress = new Uri(definition.Args[1].ToString());
                            });
                            healthCheckBuilder = healthCheckBuilder.AddTypeActivatedCheck<ThirdPartyHealthCheck>(thirdPartyName, definition.Args);
                        }
                        else
                        {
                            _Logger.Error("$Error when initializing third party health check: invalid arguments were defined. " +
                                "It should have name and URL as string arguments.");
                        }

                        break;
                    default:
                        break;
                }
            }
        }

        public static void UseHealthCheck(this IApplicationBuilder app, string path)
        {
            var options = new HealthCheckOptions();
            options.ResponseWriter = WriteResponse;

            app.UseHealthChecks(path, options);
        }

        public static void AddKalturaHealthCheckService(this IServiceCollection services)
        {
            var tcmHealthCheckDefinitions = ConfigurationManager.ApplicationConfiguration.Current.HealthCheckConfiguration.Value;
            List<HealthCheckDefinition> healthCheckDefinitions = tcmHealthCheckDefinitions?.Select(defintion =>
                new HealthCheckDefinition(defintion)).ToList();

            services.AddHealthCheckService(healthCheckDefinitions);
        }

        private static Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var jsonResponse = new JObject();

            var unHealthyComponents = new JArray();
            var degradedComponents = new JArray();

            jsonResponse["status"] = result.Status.ToString();

            foreach (var entry in result.Entries)
            {
                switch (entry.Value.Status)
                {
                    case HealthStatus.Unhealthy:
                        unHealthyComponents.Add(entry.Key);
                        break;
                    case HealthStatus.Degraded:
                        degradedComponents.Add(entry.Key);
                        break;
                    case HealthStatus.Healthy:
                        break;
                    default:
                        break;
                }
            }

            jsonResponse["unhealthy"] = unHealthyComponents;
            jsonResponse["degraded"] = degradedComponents;

            return context.Response.WriteAsync(jsonResponse.ToString());
        }
    }
}
