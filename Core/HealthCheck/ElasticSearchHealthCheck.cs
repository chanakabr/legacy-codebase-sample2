using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using ConfigurationManager;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace HealthCheck
{
    class ElasticSearchHealthCheck : IHealthCheck
    {
        private readonly HttpClient httpClient = null;

        public ElasticSearchHealthCheck(IHttpClientFactory factory)
        {
            httpClient = factory.CreateClient("ElasticSearchHealthCheck");
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            bool healthCheckResultHealthy = this.HealthCheck();

            if (healthCheckResultHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("ElasticSearch is healthy"));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("ElasticSearch is unhealthy"));
            }
        }

        private bool HealthCheck()
        {
            bool result = false;
            try
            {
                var url = $"{ApplicationConfiguration.Current.ElasticSearchConfiguration.URL.Value}/{"_cluster/health"}";
                var status = 0;
                var response = httpClient.GetAsync(url).ExecuteAndWait();

                status = GetResponseCode(response.StatusCode);
                var ret = response.Content.ReadAsStringAsync().ExecuteAndWait();

                if (status != 200)
                {
                    return false;
                }

                var json = JObject.Parse(ret);

                if (json != null)
                {
                    var healthStatus = json["status"];

                    if (healthStatus != null)
                    {
                        var healthStatusValue = healthStatus.Value<string>().ToLower();

                        if (healthStatusValue == "yellow" || healthStatusValue == "green")
                        {
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        private Int32 GetResponseCode(HttpStatusCode theCode)
        {
            if (theCode == HttpStatusCode.OK || theCode == HttpStatusCode.Created || theCode == HttpStatusCode.Accepted)
                return (int)HttpStatusCode.OK;
            if (theCode == HttpStatusCode.NotFound)
                return (int)HttpStatusCode.NotFound;
            return (int)HttpStatusCode.InternalServerError;
        }

    }

    internal static class AsyncHelper
    {
        public static T ExecuteAndWait<T>(this Task<T> taskToRun)
        {
            var result = Task.Run(() => taskToRun).ConfigureAwait(false).GetAwaiter().GetResult();
            return result;
        }

        public static void ExecuteAndWait(this Task taskToRun)
        {
            Task.Run(() => taskToRun).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }

}
