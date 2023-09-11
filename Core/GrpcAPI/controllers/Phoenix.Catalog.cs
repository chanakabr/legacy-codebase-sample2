using CachingProvider.LayeredCache;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using phoenix;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grpc.controllers
{
    public partial class PhoenixController : phoenix.Phoenix.PhoenixBase
    {
        public override async Task<BoolValue> HasVirtualAssetType(HasVirtualAssetTypeRequest request,
            ServerCallContext context)
        {
            var response = _catalogService.HasVirtualAssetType(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return new BoolValue{Value = response};
        }

        public override Task<HandleBlockingSegmentResponse> HandleBlockingSegment(HandleBlockingSegmentRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(_catalogService.HandleBlockingSegment(request));
        }

        public override async Task<StringValue>
            GetEpgChannelId(
                GetEpgChannelIdRequest request, ServerCallContext context)
        {
            var response = _catalogService.GetEpgChannelId(request) ?? String.Empty;
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return new StringValue{Value = response};
        }

        public override Task<GetAssetsForValidationResponse>
            GetAssetsForValidation(
                GetAssetsForValidationRequest request, ServerCallContext context)
        {
            return Task.FromResult(_catalogService.GetAssetsForValidation(request));
        }

        public override async Task<GetMediaFilesResponse> GetMediaFiles(GetMediaFilesRequest request,
            ServerCallContext context)
        {
            var response = _catalogService.GetMediaFiles(request);
            var invalidationKeyFromRequest = new HashSet<string>
                {LayeredCacheKeys.GetMediaInvalidationKey(request.GroupId, request.MediaId)};
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return response;
        }

        public override async Task<GetMediaByIdResponse>
            GetMediaById(
                GetMediaByIdRequest request, ServerCallContext context)
        {
            
            var response =_catalogService.GetMediaById(request);
            var invalidationKeyFromRequest = new HashSet<string>
                {LayeredCacheKeys.GetMediaInvalidationKey(request.GroupId, request.MediaId)};
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return response;
        }

        public override async Task<GetMediaInfoResponse>
            GetMediaInfo(
                GetMediaInfoRequest request, ServerCallContext context)
        {
            var response = _catalogService.GetMediaInfo(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return response;
        }

        public override Task<GetProgramScheduleResponse>
            GetProgramSchedule(
                GetProgramScheduleRequest request, ServerCallContext context)
        {
            return Task.FromResult(_catalogService.GetProgramSchedule(request));
        }

        public override async Task<GetDomainRecordingsResponse>
            GetDomainRecordings(
                GetDomainRecordingsRequest request, ServerCallContext context)
        {
            var response = _catalogService.GetDomainRecordings(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return response;
        }

        public override Task<GetEpgsByIdsResponse>
            GetEpgsByIds(
                GetEpgsByIdsRequest request, ServerCallContext context)
        {
            return Task.FromResult(_catalogService.GetEpgsByIds(request));
        }

        public override Task<GetLinearMediaInfoByEpgChannelIdAndFileTypeResponse>
            GetLinearMediaInfoByEpgChannelIdAndFileType(
                GetLinearMediaInfoByEpgChannelIdAndFileTypeRequest request, ServerCallContext context)
        {
            return Task.FromResult(_catalogService.GetLinearMediaInfoByEpgChannelIdAndFileType(request));
        }

        public override async Task<MapMediaFilesResponse> MapMediaFiles(
            MapMediaFilesRequest request, ServerCallContext context)
        {
            var response = _catalogService.MapMediaFiles(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return response;
        }

        public override Task<StringValue> GetEPGChannelCDVRId(
            GetEPGChannelCDVRIdRequest request, ServerCallContext context)
        {
            return Task.FromResult(new StringValue {Value = _catalogService.GetEPGChannelCDVRId(request)});
        }

        public override Task<GetRecordingLinkByFileTypeResponse> GetRecordingLinkByFileType(GetRecordingLinkByFileTypeRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(_catalogService.GetRecordingLinkByFileType(request));
        }
        
        public override async Task<GetGroupMediaFileTypesResponse> GetGroupMediaFileTypes(GetGroupMediaFileTypesRequest request,
            ServerCallContext context)
        {
            var response = _catalogService.GetGroupMediaFileTypes(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return response;        
        }
        
        public override async Task<GetProgramsByChannelIdResponse> GetProgramsByChannelId(GetProgramsByChannelIdRequest request,
            ServerCallContext context)
        {
            var response = _catalogService.GetProgramsByChannelId(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return response;        
        }
        
        public override async Task<GetChannelIdsResponse> GetChannelIds(GetChannelIdsRequest request,
            ServerCallContext context)
        {
            var response = _catalogService.GetChannelIds(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            await context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return response;        
        }
        
    }
}