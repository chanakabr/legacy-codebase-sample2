using System;
using System.Linq;
using System.Reflection;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Segmentation;
using Core.Api;
using Core.Api.Managers;
using Core.Users;
using DAL;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using GrpcAPI.Utils;
using Microsoft.Extensions.Logging;
using phoenix;
using Phx.Lib.Log;
using Tvinci.Core.DAL;
using eAssetTypes = phoenix.eAssetTypes;
using EPGChannelProgrammeObject = ApiObjects.EPGChannelProgrammeObject;
using MediaFile = phoenix.MediaFile;
using Status = phoenix.Status;

namespace GrpcAPI.Services
{
    public interface ICatalogService
    {
        string GetEpgChannelId(GetEpgChannelIdRequest request);
        HandleBlockingSegmentResponse HandleBlockingSegment(HandleBlockingSegmentRequest request);
        GetAssetsForValidationResponse GetAssetsForValidation(GetAssetsForValidationRequest request);

        GetMediaFilesResponse GetMediaFiles(GetMediaFilesRequest request);
        GetMediaByIdResponse GetMediaById(GetMediaByIdRequest request);
        GetMediaInfoResponse GetMediaInfo(GetMediaInfoRequest request);
        public GetProgramScheduleResponse GetProgramSchedule(GetProgramScheduleRequest request);
        public GetDomainRecordingsResponse GetDomainRecordings(GetDomainRecordingsRequest request);
        public GetEpgsByIdsResponse GetEpgsByIds(GetEpgsByIdsRequest request);
        public MapMediaFilesResponse MapMediaFiles(MapMediaFilesRequest request);

        public GetLinearMediaInfoByEpgChannelIdAndFileTypeResponse GetLinearMediaInfoByEpgChannelIdAndFileType(
            GetLinearMediaInfoByEpgChannelIdAndFileTypeRequest request);

        public string GetEPGChannelCDVRId(GetEPGChannelCDVRIdRequest request);
    }

    public class CatalogService : ICatalogService
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.ToString());

        public string GetEpgChannelId(GetEpgChannelIdRequest request)
        {
            return APILogic.Api.Managers.EpgManager.GetEpgChannelId(request.MediaId, request.GroupId);
        }

        public HandleBlockingSegmentResponse HandleBlockingSegment(HandleBlockingSegmentRequest request)
        {
            try
            {
                var status =
                    api.HandleBlockingSegment<SegmentBlockPlaybackSubscriptionAction>(request.GroupId,
                        request.UserId.ToString(),
                        request.Udid,
                        request.Ip, (int)request.DomainId,
                        (ApiObjects.ObjectVirtualAssetInfoType) request.VirtualAssetInfoType,
                        request.SubscriptionId);
                return new HandleBlockingSegmentResponse()
                {
                    Status = GrpcMapping.Mapper.Map<Status>(status)
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping HandleBlockingSegment GRPC service {e.Message}");
                return null;
            }
        }

        public GetAssetsForValidationResponse GetAssetsForValidation(
            GetAssetsForValidationRequest getAssetsForValidationRequest)
        {
            try
            {
                var SlimAsset = AssetRuleManager.GetAssetsForValidation(
                    (ApiObjects.eAssetTypes) getAssetsForValidationRequest.AssetType,
                    getAssetsForValidationRequest.GroupId,
                    getAssetsForValidationRequest.AssetId);

                return new GetAssetsForValidationResponse()
                {
                    SlimAsset =
                    {
                        GrpcMapping.Mapper.Map<RepeatedField<SlimAsset>>(SlimAsset)
                    }
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping GetAssetsForValidation GRPC service {e.Message}");
                return null;
            }
        }

        public GetMediaFilesResponse GetMediaFiles(
            GetMediaFilesRequest request)
        {
            try
            {
                var mediaFiles = ApiDAL.GetMediaFiles(
                    request.GroupId, request.MediaId);

                if (mediaFiles != null)
                {
                    return new GetMediaFilesResponse()
                    {
                        MediaFiles =
                        {
                            GrpcMapping.Mapper.Map<RepeatedField<MediaFile>>(mediaFiles)
                        }
                    };
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping GetMediaFiles GRPC service {e.Message}");
                return null;
            }
        }

        public GetMediaInfoResponse GetMediaInfo(GetMediaInfoRequest request)
        {
            try
            {
                ApiObjects.Response.Status status =
                    new ApiObjects.Response.Status((int) eResponseStatus.OK, eResponseStatus.OK.ToString());
                long mediaId;
                ApiObjects.TimeShiftedTv.Recording recording = null;
                EPGChannelProgrammeObject program = null;
                bool isExternalRecordingIgnoreMode = request.AssetType == eAssetTypes.Npvr &&
                                                     TvinciCache.GroupsFeatures.GetGroupFeatureStatus(request.GroupId,
                                                         GroupFeature.EXTERNAL_RECORDINGS);
                if (request.AssetType != eAssetTypes.Media)
                {
                    Domain domain = null;
                    long domainId = 0;
                    if (request.DomainId == 0)
                    {
                        Core.ConditionalAccess.Utils.ValidateUserAndDomain(request.GroupId, request.UserId.ToString(),
                            ref domainId, out domain);
                    }
                    else
                    {
                        Core.ConditionalAccess.Utils.ValidateDomain(request.GroupId, (int)request.DomainId,
                            out domain);
                    }

                    status = Core.ConditionalAccess.Utils.GetMediaIdForAsset(request.GroupId, request.AssetId,
                        (ApiObjects.eAssetTypes) request.AssetType, request.UserId.ToString(), domain, request.Udid,
                        out mediaId, out recording, out program);
                }
                else
                {
                    mediaId = long.Parse(request.AssetId);
                }

                // Allow to continue for external recording (and asset type = NPVR) since we may not be updated on them in real time
                if (status.Code != (int) eResponseStatus.OK)
                {
                    if (isExternalRecordingIgnoreMode && request.MediaFileId > 0)
                    {
                        status = new ApiObjects.Response.Status((int) eResponseStatus.OK,
                            eResponseStatus.OK.ToString());
                        mediaId = Core.ConditionalAccess.Utils.GetMediaIdByFileId(request.GroupId, request.MediaFileId);

                        if (request.ProgramId > 0)
                        {
                            recording = new ApiObjects.TimeShiftedTv.Recording() {EpgId = request.ProgramId};
                        }
                    }
                }

                return new GetMediaInfoResponse()
                {
                    MediaId = mediaId,
                    Recording = recording != null ? GrpcMapping.Mapper.Map<Recording>(recording) : null,
                    Program = program != null
                        ? GrpcMapping.Mapper.Map<phoenix.EPGChannelProgrammeObject>(program)
                        : null,
                    Status = GrpcMapping.Mapper.Map<Status>(status),
                    IsExternalRecordingIgnoreMode = isExternalRecordingIgnoreMode
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping GetMediaInfo GRPC service {e.Message}");
                return null;
            }
        }

        public GetMediaByIdResponse GetMediaById(GetMediaByIdRequest request)
        {
            try
            {
                var mediaById = Core.ConditionalAccess.Utils.GetMediaById(request.GroupId, request.MediaId);
                return mediaById != null ? GrpcMapping.Mapper.Map<GetMediaByIdResponse>(mediaById) : null;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping GetMediaById GRPC service {e.Message}");
                return null;
            }
        }

        public GetProgramScheduleResponse GetProgramSchedule(GetProgramScheduleRequest request)
        {
            try
            {
                var program = Core.Api.Module.GetProgramSchedule(request.GroupId, request.ProgramId);
                return program != null ? GrpcMapping.Mapper.Map<GetProgramScheduleResponse>(program) : null;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping GetProgramSchedule GRPC service {e.Message}");
                return null;
            }
        }

        public GetDomainRecordingsResponse GetDomainRecordings(GetDomainRecordingsRequest request)
        {
            try
            {
                var recording = Core.ConditionalAccess.Utils.GetDomainRecordings(request.GroupId,
                    request.DomainId, request.ShouldFilterViewableRecordingsOnly);
                return recording != null ? GrpcMapping.Mapper.Map<GetDomainRecordingsResponse>(recording) : null;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping GetDomainRecordings GRPC service {e.Message}");
                return null;
            }
        }

        public GetEpgsByIdsResponse GetEpgsByIds(GetEpgsByIdsRequest request)
        {
            try
            {
                var epgIds = Core.ConditionalAccess.Utils.GetEpgsByIds(request.GroupId,
                    request.EpgIds.ToList());
                return epgIds != null ? GrpcMapping.Mapper.Map<GetEpgsByIdsResponse>(epgIds) : null;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping GetEpgsByIds GRPC service {e.Message}");
                return null;
            }
        }

        public GetLinearMediaInfoByEpgChannelIdAndFileTypeResponse GetLinearMediaInfoByEpgChannelIdAndFileType(
            GetLinearMediaInfoByEpgChannelIdAndFileTypeRequest request)
        {
            int linearMediaId = 0, mediaFileId = 0;
            var getLinearMediaInfoByEpgChannelIdAndFileTypeResponse =
                Core.ConditionalAccess.Utils.GetLinearMediaInfoByEpgChannelIdAndFileType(request.GroupId,
                    request.EpgChannelId, request.FileType, ref linearMediaId, ref mediaFileId);

            return new GetLinearMediaInfoByEpgChannelIdAndFileTypeResponse()
            {
                IsSuccess = getLinearMediaInfoByEpgChannelIdAndFileTypeResponse,
                MediaFileId = mediaFileId,
                LinearMediaId = linearMediaId
            };
        }

        public MapMediaFilesResponse MapMediaFiles(MapMediaFilesRequest request)
        {
            try
            {
                var mapMediaFilesResponse =
                    Core.Api.Module.MapMediaFiles(request.GroupId, request.MediaFileIDs.ToArray());
                if (mapMediaFilesResponse != null)
                {
                    return new MapMediaFilesResponse()
                    {
                        MediaMappers =
                        {
                            GrpcMapping.Mapper.Map<RepeatedField<MediaMapper>>(mapMediaFilesResponse)
                        }
                    };
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping MapMediaFiles GRPC service {e.Message}");
                return null;
            }
        }

        public string GetEPGChannelCDVRId(GetEPGChannelCDVRIdRequest request)
        {
            return CatalogDAL.GetEPGChannelCDVRId(request.GroupId, request.EpgChannelId);
        }
    }
}