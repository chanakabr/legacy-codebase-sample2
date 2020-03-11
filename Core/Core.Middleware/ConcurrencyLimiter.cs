using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;


namespace Core.Middleware
{
    public static class ConcurrencyLimiter
    {
        private const string CONCURRENCY_LIMITER_POLICY_QUEUE = "QUEUE";
        private const string CONCURRENCY_LIMITER_POLICY_STACK = "STACK";
        private const int CONCURRENCY_LIMITER_MAX_LENGTH_DEFAULT = 1000; // This is the default iis queue length
        private static readonly int CONCURRENCY_LIMITER_MAX_CONCURRENT_DEFAULT = Environment.ProcessorCount * 2;

        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static void AddCoreConcurrencyLimiter(this IServiceCollection services)
        {
            var concurrencyLimiterPolicy = GetEnvironmnetVariableStr("CONCURRENCY_LIMITER_POLICY", defaultVal: CONCURRENCY_LIMITER_POLICY_QUEUE);
            var concurrencyLimiterMaxConcurrent = GetEnvironmnetVariableInt("CONCURRENCY_LIMITER_MAX_CONCURRENT", defaultVal: CONCURRENCY_LIMITER_MAX_CONCURRENT_DEFAULT);
            var concurrencyLimiterMaxLength = GetEnvironmnetVariableInt("CONCURRENCY_LIMITER_MAX_LENGTH", defaultVal: CONCURRENCY_LIMITER_MAX_LENGTH_DEFAULT);

            _Logger.Info($"Setting concurrency Limiter Policy:[{concurrencyLimiterPolicy}] maxConcurrent:[{concurrencyLimiterMaxConcurrent}], maxLength:[{concurrencyLimiterMaxLength}]");

            if (concurrencyLimiterPolicy.Equals(CONCURRENCY_LIMITER_POLICY_QUEUE, StringComparison.OrdinalIgnoreCase))
            {
                services.AddQueuePolicy(conf =>
                {
                    conf.MaxConcurrentRequests = concurrencyLimiterMaxConcurrent;
                    conf.RequestQueueLimit = concurrencyLimiterMaxLength;

                });
            }
            else
            {
                services.AddStackPolicy(conf =>
                {
                    conf.MaxConcurrentRequests = concurrencyLimiterMaxConcurrent;
                    conf.RequestQueueLimit = concurrencyLimiterMaxLength;
                });
            }

              // add some additional threads to facilitate the async pipeline and the parallel foreach we use
            var minThreadsBuffer = Environment.ProcessorCount * 5; 
            var minThreadsDefault = concurrencyLimiterMaxConcurrent + minThreadsBuffer;

            var minWorkerThreadsToSet = GetEnvironmnetVariableInt("MIN_WORKER_THREADS", defaultVal: minThreadsDefault);
            var minIOWorkerThreadsToSet = GetEnvironmnetVariableInt("MIN_IO_WORKER_THREADS", defaultVal: minThreadsDefault);
            
            System.Threading.ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionThreads);
            _Logger.Info($"Current Min workThreads/IO Threads :{minWorkerThreads} / {minCompletionThreads}");
            
            var setMinResult = System.Threading.ThreadPool.SetMinThreads(minWorkerThreadsToSet, minIOWorkerThreadsToSet);
            _Logger.Info($"Setting Min workThreads/IO Threads :{minWorkerThreadsToSet} / {minIOWorkerThreadsToSet} result:[{setMinResult}]");

        }

        public static void UseCoreConcurrencyLimiter(this IApplicationBuilder app)
        {
            app.UseConcurrencyLimiter();
        }

        private static int GetEnvironmnetVariableInt(string key, int defaultVal)
        {
            if (int.TryParse(Environment.GetEnvironmentVariable(key), out var outValue))
            {
                return outValue;
            }
            else
            {
                return defaultVal;
            }
        }

        private static string GetEnvironmnetVariableStr(string key, string defaultVal)
        {
            var result = Environment.GetEnvironmentVariable(key);
            return result ?? defaultVal;
        }

    }
}
