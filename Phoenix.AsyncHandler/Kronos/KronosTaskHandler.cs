using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using OTT.Lib.Kafka;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;
using Phoenix.AsyncHandler.Kafka;
using Phx.Lib.Log;

namespace Phoenix.AsyncHandler.Kronos
{
    internal class KronosTaskHandler : TaskHandler.TaskHandlerBase
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly KronosConfigurationProvider _configurationProvider;

        public KronosTaskHandler(
            IServiceScopeFactory serviceScopeFactory, KronosConfigurationProvider configurationProvider)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configurationProvider = configurationProvider;
        }

        public override Task<ExecuteTaskResponse> ExecuteTask(ExecuteTaskRequest request, ServerCallContext context)
        {
            SetKLogger(request, context);
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                if (_configurationProvider.TryGetHandler(request.TaskMetaData.QualifiedName, out var implementation))
                {
                    SetKafkaContext(request, context, serviceScope);
                    var handler = (IKronosTaskHandler)serviceScope.ServiceProvider.GetRequiredService(implementation);

                    return handler.ExecuteTask(request, context);
                }

                return Task.FromResult(new ExecuteTaskResponse
                {
                    IsSuccess = false,
                    Message = $"Service is subscribed for {request.TaskMetaData.QualifiedName} event, but doesn't implement handler."
                });
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