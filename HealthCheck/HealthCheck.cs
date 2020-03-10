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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HealthCheck
{
    public static class HealthCheck
    {
        public static void AddHealthCheckService(this IServiceCollection services, List<HealthCheckDefinition> definitions)
        {
            var healthCheckBuilder = services.AddHealthChecks();

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
                    case HealthCheckType.ThirdParty:
                        healthCheckBuilder = healthCheckBuilder.AddTypeActivatedCheck<ThirdPartyHealthCheck>(definition.Args[0].ToString(), definition.Args);
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
