using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.IO;
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

            var options = new JsonWriterOptions
            {
                Indented = true
            };

            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, options))
                {
                    writer.WriteStartObject();
                    writer.WriteString("status", result.Status.ToString());
                    writer.WriteStartObject("results");
                    foreach (var entry in result.Entries)
                    {
                        writer.WriteStartObject(entry.Key);
                        writer.WriteString("status", entry.Value.Status.ToString());
                        writer.WriteString("description", entry.Value.Description);
                        writer.WriteStartObject("data");
                        foreach (var item in entry.Value.Data)
                        {
                            writer.WritePropertyName(item.Key);
                            JsonSerializer.Serialize(
                                writer, item.Value, item.Value?.GetType() ??
                                typeof(object));
                        }
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                    }
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }

                var json = Encoding.UTF8.GetString(stream.ToArray());

                return context.Response.WriteAsync(json);
            }
        }
    }
}
