using System;
using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Services;
using Core.Catalog;
using Grpc.Core;
using LiveToVod;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;
using Phoenix.AsyncHandler.Kronos;
using Phx.Lib.Appconfig;

namespace Phoenix.AsyncHandler.Catalog
{
    public class LiveToVodTearDownHandler : IKronosTaskHandler
    {
        private readonly ILiveToVodManager _liveToVodManager;
        private readonly ILiveToVodService _liveToVodService;
        private readonly IAssetManager _assetManager;
        private readonly IIndexManagerFactory _indexManagerFactory;
        
        public LiveToVodTearDownHandler(
            ILiveToVodManager liveToVodManager,
            ILiveToVodService liveToVodService,
            IAssetManager assetManager,
            IIndexManagerFactory indexManagerFactory)
        {
            _liveToVodManager = liveToVodManager;
            _liveToVodService = liveToVodService;
            _assetManager = assetManager;
            _indexManagerFactory = indexManagerFactory;
        }
        
        public Task<ExecuteTaskResponse> ExecuteTask(ExecuteTaskRequest request, ServerCallContext context)
        {
            var finalEndDate = DateTime.UtcNow;
            var partnerIds = _liveToVodManager.GetPartnersForTearDown();
            foreach (var partnerId in partnerIds)
            {
                var liveToVodAssetStructResponse = _liveToVodService.GetLiveToVodAssetStruct((int)partnerId);
                if (!liveToVodAssetStructResponse.IsOkStatusCode())
                {
                    continue;
                }
                
                _indexManagerFactory.GetIndexManager((int)partnerId).DeleteMediaByTypeAndFinalEndDate(
                    liveToVodAssetStructResponse.Object.Id,
                    finalEndDate);
                _assetManager.DeleteAssetsByTypeAndDate(
                    partnerId,
                    liveToVodAssetStructResponse.Object.Id,
                    finalEndDate,
                    ApplicationConfiguration.Current.PhoenixAsyncHandlerConfiguration.UserId.Value);
            }

            return Task.FromResult(new ExecuteTaskResponse
            {
                IsSuccess = true,
                Message = "Live to vod tear down process is completed."
            });
        }
    }
}