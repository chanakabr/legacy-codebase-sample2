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
        public override Task<BoolValue> HasVirtualAssetType(HasVirtualAssetTypeRequest request,
            ServerCallContext context)
        {
            var response = _catalogService.HasVirtualAssetType(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(new BoolValue{Value = response});
        }

        public override Task<HandleBlockingSegmentResponse> HandleBlockingSegment(HandleBlockingSegmentRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(_catalogService.HandleBlockingSegment(request));
        }

        public override Task<StringValue>
            GetEpgChannelId(
                GetEpgChannelIdRequest request, ServerCallContext context)
        {
            var response = _catalogService.GetEpgChannelId(request) ?? String.Empty;
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(new StringValue{Value = response});
        }

        public override Task<GetAssetsForValidationResponse>
            GetAssetsForValidation(
                GetAssetsForValidationRequest request, ServerCallContext context)
        {
            return Task.FromResult(_catalogService.GetAssetsForValidation(request));
        }

        public override Task<GetMediaFilesResponse> GetMediaFiles(GetMediaFilesRequest request,
            ServerCallContext context)
        {
            var response = _catalogService.GetMediaFiles(request);
            var invalidationKeyFromRequest = new List<string>
                {LayeredCacheKeys.GetMediaInvalidationKey(request.GroupId, request.MediaId)};
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetMediaByIdResponse>
            GetMediaById(
                GetMediaByIdRequest request, ServerCallContext context)
        {
            
            var response =_catalogService.GetMediaById(request);
            var invalidationKeyFromRequest = new List<string>
                {LayeredCacheKeys.GetMediaInvalidationKey(request.GroupId, request.MediaId)};
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetMediaInfoResponse>
            GetMediaInfo(
                GetMediaInfoRequest request, ServerCallContext context)
        {
            var response = _catalogService.GetMediaInfo(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetProgramScheduleResponse>
            GetProgramSchedule(
                GetProgramScheduleRequest request, ServerCallContext context)
        {
            return Task.FromResult(_catalogService.GetProgramSchedule(request));
        }

        public override Task<GetDomainRecordingsResponse>
            GetDomainRecordings(
                GetDomainRecordingsRequest request, ServerCallContext context)
        {
            var response = _catalogService.GetDomainRecordings(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
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

        public override Task<MapMediaFilesResponse> MapMediaFiles(
            MapMediaFilesRequest request, ServerCallContext context)
        {
            var response = _catalogService.MapMediaFiles(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
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
        
        public override Task<GetGroupMediaFileTypesResponse> GetGroupMediaFileTypes(GetGroupMediaFileTypesRequest request,
            ServerCallContext context)
        {
            var response = _catalogService.GetGroupMediaFileTypes(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);        
        }
    }
}