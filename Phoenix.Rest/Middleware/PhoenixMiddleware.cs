using KLogMonitor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Phoenix.Context;
using Phoenix.Rest.Services;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using WebAPI.App_Start;
using WebAPI.Filters;

namespace Phoenix.Rest.Middleware
{


    public static class PhoenixMiddleware
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Adding required services to use the Phoenix middleware
        /// </summary>
        public static IServiceCollection ConfigurePhoenix(this IServiceCollection services)
        {

            System.Threading.ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionThreads);
            System.Threading.ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionThreads);

            _Logger.Info($"Current ServicePointManager.DefaultConnectionLimit:{ServicePointManager.DefaultConnectionLimit}");
            _Logger.Info($"Current Max workThreads/IO Threads :{maxWorkerThreads} / {maxCompletionThreads}");
            _Logger.Info($"Current Min workThreads/IO Threads :{minWorkerThreads} / {minCompletionThreads}");

            var minWorkerThreadsToSet = GetEnvironmnetVariableInt("MIN_WORKER_THREADS", defaultVal: minWorkerThreads);
            var minIOWorkerThreadsToSet = GetEnvironmnetVariableInt("MIN_IO_WORKER_THREADS", defaultVal: maxCompletionThreads);
            var maxWorkerThreadsToSet = GetEnvironmnetVariableInt("MAX_WORKER_THREADS", defaultVal: maxWorkerThreads);
            var maxIOWorkerThreadsToSet = GetEnvironmnetVariableInt("MAX_IO_WORKER_THREADS", defaultVal: minCompletionThreads);
            var connectionLimitToSet = GetEnvironmnetVariableInt("MAX_CONN_LIMIT", defaultVal: ServicePointManager.DefaultConnectionLimit);

            var setMaxResult = System.Threading.ThreadPool.SetMaxThreads(maxWorkerThreadsToSet, maxIOWorkerThreadsToSet); // or higher
            var setMinResult = System.Threading.ThreadPool.SetMinThreads(minWorkerThreadsToSet, minIOWorkerThreadsToSet); // or higher
            System.Net.ServicePointManager.DefaultConnectionLimit = connectionLimitToSet; // Max concurrent outbound requests

            _Logger.Info($"Current ServicePointManager.DefaultConnectionLimit:{connectionLimitToSet}");
            _Logger.Info($"Current Max workThreads/IO Threads :{maxWorkerThreadsToSet} / {maxIOWorkerThreadsToSet} result:[{setMaxResult}]");
            _Logger.Info($"Current Min workThreads/IO Threads :{minWorkerThreadsToSet} / {minIOWorkerThreadsToSet} result:[{setMinResult}]");

            services.AddHttpContextAccessor();
            services.AddKalturaApplicationSessionContext();
            services.AddSingleton<IResponseFromatterProvider, ResponseFromatterProvider>();

            return services;
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

        /// <summary>
        /// Using custom middleware for Phoenix Api convention
        /// </summary>
        public static IApplicationBuilder UsePhoenix(this IApplicationBuilder app)
        {
            app.UseMiddleware<PhoenixExceptionHandler>();
            AutoMapperConfig.RegisterMappings();
            app.UseMiddleware<PhoenixSessionId>();
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<PhoenixCors>();
            app.UseMiddleware<PhoenixRequestContextBuilder>();
            app.UseMiddleware<PhoenixRequestExecutor>();
            return app;
        }
    }
}

