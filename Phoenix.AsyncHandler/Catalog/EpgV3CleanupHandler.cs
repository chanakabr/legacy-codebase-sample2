using System;
using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiObjects;
using Core.Catalog;
using Core.GroupManagers;
using Grpc.Core;
using LiveToVod;
using Microsoft.Extensions.Logging;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;
using Phoenix.AsyncHandler.Kronos;
using Phx.Lib.Appconfig;

namespace Phoenix.AsyncHandler.Catalog
{
    public class EpgV3CleanupHandler : IKronosTaskHandler
    {
        private readonly ILogger<EpgV3CleanupHandler> _logger;
        private readonly IIndexManagerFactory _indexManagerFactory;
        private readonly IGroupSettingsManager _groupSettingsManager;

        public EpgV3CleanupHandler(
            ILogger<EpgV3CleanupHandler> logger,
            IGroupSettingsManager groupSettingsManager,
            IIndexManagerFactory indexManagerFactory)
        {
            _logger = logger;
            _indexManagerFactory = indexManagerFactory;
            _groupSettingsManager = groupSettingsManager;
        }
        
        public Task<ExecuteTaskResponse> ExecuteTask(ExecuteTaskRequest request, ServerCallContext context)
        {
            var isSuccess = true;
            var partnerIdsToCleanup = _groupSettingsManager.GetPartnersByEpgFeatureVersion(EpgFeatureVersion.V3);
            foreach (var partnerId in partnerIdsToCleanup)
            {
                var idxManager = _indexManagerFactory.GetIndexManager(partnerId);
                try
                {
                    idxManager.CleanupEpgV3Index();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "failed to cleanup epg v3 index");
                    isSuccess = false;
                }
            }

            return Task.FromResult(new ExecuteTaskResponse
            {
                IsSuccess = isSuccess,
                Message = "Epg V3 cleanup process is completed."
            });
        }
    }
}