using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using OTT.Lib.Kafka;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;
using Phoenix.AsyncHandler.Kafka;
using Phx.Lib.Log;

namespace Phoenix.AsyncHandler.Kronos
{
    internal class KronosTaskHandlerTyped<T> : TaskHandler.TaskHandlerBase
        where T : class, IKronosTaskHandler
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public KronosTaskHandlerTyped(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override Task<ExecuteTaskResponse> ExecuteTask(ExecuteTaskRequest request, ServerCallContext context)
        {
            SetKLogger(request, context);
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                SetKafkaContext(request, context, serviceScope);
                var handler = (IKronosTaskHandler) serviceScope.ServiceProvider.GetService<T>();
                if (handler == null)
                {
                    return Task.FromResult(new ExecuteTaskResponse
                    {
                        IsSuccess = false,
                        Message = $"Service is subscribed for {request.TaskMetaData.QualifiedName} event, but doesn't implement handler."
                    });
                }

                return handler.ExecuteTask(request, context);
            }
        }

        private static void SetKafkaContext(ExecuteTaskRequest request, ServerCallContext context, IServiceScope serviceScope)
        {
            var contextProvider = serviceScope.ServiceProvider.GetRequiredService<IKafkaContextProvider>() as AsyncHandlerKafkaContextProvider;
            // TODO: set "traceId" after https://kaltura.atlassian.net/browse/BEO-12447 is fixed
            contextProvider?.Populate(string.Empty, request.PartnerId, 0);
        }

        private static void SetKLogger(ExecuteTaskRequest request, ServerCallContext context)
        {
            // Currently it's not possible to get traceId for KLogger
            // TODO: set "traceId" for KLogger after https://kaltura.atlassian.net/browse/BEO-12447 is fixed
            // KLogger.SetRequestId(context.RequestHeaders.GetValue("traceId"));
            if (request.PartnerId > 0) KLogger.SetGroupId(request.PartnerId.ToString());
        }
    }
}
