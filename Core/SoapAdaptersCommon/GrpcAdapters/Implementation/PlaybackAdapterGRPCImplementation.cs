using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration;
using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SoapAdaptersCommon.Contracts.PlaybackAdapter.Models;

namespace SoapAdaptersCommon.GrpcAdapters.Implementation
{
    public class PlaybackAdapterGRPCImplementation : PlaybackAdapterGRPC.PlaybackAdapterGRPCBase
    {
        private readonly ILogger<PlaybackAdapterGRPCImplementation> _logger;
        private readonly PlaybackAdapter.IService _PlaybackService;
        private readonly IMemoryCache _MemoryCache;

        public PlaybackAdapterGRPCImplementation(ILogger<PlaybackAdapterGRPCImplementation> logger,
            PlaybackAdapter.IService PlaybackService, IMemoryCache memoryCache)
        {
            _logger = logger;
            _PlaybackService = PlaybackService;
            _MemoryCache = memoryCache;
        }

        public override Task<PlaybackAdapterGetConfigurationResponse> GetConfiguration(
            PlaybackAdapterGetConfigurationRequest request, ServerCallContext context)
        {
            var result = _PlaybackService.GetConfiguration(request.AdapterId);
            var response = new PlaybackAdapterGetConfigurationResponse();
            response.AdapterStatus = new AdapterStatus
            {
                Code = result.Status.Code,
                Message = result.Status.Message
            };

            response.ImplementedMethods.AddRange(result.ImplementedMethods.Select(m => (PlaybackMethods) m).ToList());
            return Task.FromResult(response);
        }

        public override Task<PlaybackAdapterSetConfigurationResponse> SetConfiguration(
            PlaybackAdapterSetConfigurationRequest request, ServerCallContext context)
        {
            var result = _PlaybackService.SetConfiguration(request.AdapterId, request.Settings, request.PartnerId,
                request.TimeStamp, request.Signature);
            var response = new PlaybackAdapterSetConfigurationResponse
            {
                AdapterStatus = new AdapterStatus
                {
                    Code = result.Code,
                    Message = result.Message
                }
            };

            return Task.FromResult(response);
        }

        public override Task<PlaybackAdapterResponse> GetPlaybackContext(PlaybackAdapterRequest request,
            ServerCallContext context)
        {
            var adapterPlaybackContext = request.AdapterPlaybackContextOptions == null
                ? null
                : JsonConvert.DeserializeObject<PlaybackAdapter.AdapterPlaybackContextOptions>(
                    JsonConvert.SerializeObject(request.AdapterPlaybackContextOptions));
            var requestPlaybackContextOptions = request.RequestPlaybackContextOptions == null
                ? null
                : JsonConvert.DeserializeObject<PlaybackAdapter.RequestPlaybackContextOptions>(
                    JsonConvert.SerializeObject(request.RequestPlaybackContextOptions));
            var response = _PlaybackService.GetPlaybackContext(adapterPlaybackContext, requestPlaybackContextOptions);

            return Task.FromResult(response == null
                ? null
                : JsonConvert.DeserializeObject<PlaybackAdapterResponse>(
                    JsonConvert.SerializeObject(response)));
        }

        public override Task<PlaybackAdapterResponse> GetPlaybackManifest(PlaybackAdapterRequest request,
            ServerCallContext context)
        {
            var adapterPlaybackContext = request.AdapterPlaybackContextOptions == null
                ? null
                : JsonConvert.DeserializeObject<PlaybackAdapter.AdapterPlaybackContextOptions>(
                    JsonConvert.SerializeObject(request.AdapterPlaybackContextOptions));
            var requestPlaybackContextOptions = request.RequestPlaybackContextOptions == null
                ? null
                : JsonConvert.DeserializeObject<PlaybackAdapter.RequestPlaybackContextOptions>(
                    JsonConvert.SerializeObject(request.RequestPlaybackContextOptions));
            var response = _PlaybackService.GetPlaybackManifest(adapterPlaybackContext, requestPlaybackContextOptions);

            return Task.FromResult(response == null
                ? null
                : JsonConvert.DeserializeObject<PlaybackAdapterResponse>(
                    JsonConvert.SerializeObject(response)));
        }

        public override Task<PlaybackAdapterConcurrencyCheckResponse> ConcurrencyCheck(
            PlaybackAdapterConcurrencyCheckRequest request, ServerCallContext context)
        {
            var concurrencyCheckRequest = JsonConvert.DeserializeObject<ConcurrencyCheckRequest>(
                JsonConvert.SerializeObject(request));
            var concurrencyCheckResponse = _PlaybackService.ConcurrencyCheck(concurrencyCheckRequest);
            return Task.FromResult(new PlaybackAdapterConcurrencyCheckResponse()
            {
                AllowedToPlay = concurrencyCheckResponse.AllowedToPlay,
                CancelDeviceUdid = concurrencyCheckResponse.CancelDeviceUdid
            });
        }
    }
}