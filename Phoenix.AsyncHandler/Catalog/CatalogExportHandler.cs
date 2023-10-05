using System;
using System.Threading.Tasks;
using ApiObjects;
using Core.Catalog;
using Core.GroupManagers;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;
using Phoenix.AsyncHandler.Kronos;
using System.Text;
using Newtonsoft.Json;
using Phoenix.Generated.Tasks.Scheduled.CatalogExport;


namespace Phoenix.AsyncHandler.Catalog
{
    public class CatalogExportHandler : IKronosTaskHandler
    {
        private readonly ILogger<CatalogExportHandler> _logger;
        private readonly IIndexManagerFactory _indexManagerFactory;
        private readonly IGroupSettingsManager _groupSettingsManager;

        public CatalogExportHandler(
            ILogger<CatalogExportHandler> logger,
            IGroupSettingsManager groupSettingsManager,
            IIndexManagerFactory indexManagerFactory)
        {
            _logger = logger;
            _indexManagerFactory = indexManagerFactory;
            _groupSettingsManager = groupSettingsManager;
        }
        
        public Task<ExecuteTaskResponse> ExecuteTask(ExecuteTaskRequest request, ServerCallContext context)
        {
            CatalogExport catalogExport =
                JsonConvert.DeserializeObject<CatalogExport>(Encoding.UTF8.GetString(request.TaskBody.ToByteArray()));

            if (!catalogExport.PartnerId.HasValue || !catalogExport.Id.HasValue)
            {
                _logger.LogError("Renew - needed information is missing");
                return Task.FromResult(new ExecuteTaskResponse
                {
                    IsSuccess = false,
                    Message = "Catalog export failed - needed information is missing."
                });
            }
            
            var isSuccess = Core.Api.Module.Export((int)catalogExport.PartnerId, catalogExport.Id.Value, catalogExport.Version);

            return Task.FromResult(new ExecuteTaskResponse
            {
                IsSuccess = isSuccess,
                Message = "Catalog export process is completed."
            });
        }
    }
}