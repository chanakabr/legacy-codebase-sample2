using System;
using System.Linq;
using System.Reflection;
using ApiLogic.Api.Managers;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Segmentation;
using AutoMapper;
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
using ObjectVirtualAssetInfoType = ApiObjects.ObjectVirtualAssetInfoType;
using Status = phoenix.Status;

namespace GrpcAPI.Services
{
    public interface ICatalogService
    {
        string GetEpgChannelId(GetEpgChannelIdRequest request);
        bool HasVirtualAssetType(HasVirtualAssetTypeRequest request);
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

        public GetRecordingLinkByFileTypeResponse GetRecordingLinkByFileType(GetRecordingLinkByFileTypeRequest request);
        public GetGroupMediaFileTypesResponse GetGroupMediaFileTypes(GetGroupMediaFileTypesRequest request);
        public GetProgramsByChannelIdResponse GetProgramsByChannelId(GetProgramsByChannelIdRequest request);
        public GetChannelIdsResponse GetChannelIds(GetChannelIdsRequest request);
    }

    public class CatalogService : ICatalogService
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.ToString());

        public string GetEpgChannelId(GetEpgChannelIdRequest request)
        {
            return APILogic.Api.Managers.EpgManager.GetEpgChannelId(request.MediaId, request.GroupId);
        }

        public bool HasVirtualAssetType(HasVirtualAssetTypeRequest request)
        {
            var objectVirtualAssetInfo =
                VirtualAssetPartnerConfigManager.Instance.GetObjectVirtualAssetInfo(request.GroupId,
                    (ObjectVirtualAssetInfoType)request.VirtualAssetInfoType);
            return objectVirtualAssetInfo != null;
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
                        (ApiObjects.ObjectVirtualAssetInfoType)request.VirtualAssetInfoType,
                        request.SubscriptionId);
                return new HandleBlockingSegmentResponse()
                {
                    Status = Mapper.Map<Status>(status)
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
                    (ApiObjects.eAssetTypes)getAssetsForValidationRequest.AssetType,
                    getAssetsForValidationRequest.GroupId,
                    getAssetsForValidationRequest.AssetId);

                return new GetAssetsForValidationResponse()
                {
                    SlimAsset =
                    {
                        Mapper.Map<RepeatedField<SlimAsset>>(SlimAsset)
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
                    mediaFiles =
                        Core.ConditionalAccess.Utils.ValidateMediaFilesUponSecurity(mediaFiles, request.GroupId);
                    return new GetMediaFilesResponse()
                    {
                        MediaFiles =
                        {
                            Mapper.Map<RepeatedField<MediaFile>>(mediaFiles)
                        }
                    };
                }

                return new GetMediaFilesResponse();
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
                    new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
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
                        (ApiObjects.eAssetTypes)request.AssetType, request.UserId.ToString(), domain, request.Udid,
                        out mediaId, out recording, out program);
                }
                else
                {
                    mediaId = long.Parse(request.AssetId);
                }

                // Allow to continue for external recording (and asset type = NPVR) since we may not be updated on them in real time
                if (status.Code != (int)eResponseStatus.OK)
                {
                    if (isExternalRecordingIgnoreMode && request.MediaFileId > 0)
                    {
                        status = new ApiObjects.Response.Status((int)eResponseStatus.OK,
                            eResponseStatus.OK.ToString());
                        mediaId = Core.ConditionalAccess.Utils.GetMediaIdByFileId(request.GroupId, request.MediaFileId);

                        if (request.ProgramId > 0)
                        {
                            recording = new ApiObjects.TimeShiftedTv.Recording() { EpgId = request.ProgramId };
                        }
                    }
                }

                return new GetMediaInfoResponse()
                {
                    MediaId = mediaId,
                    Recording = recording != null ? Mapper.Map<Recording>(recording) : null,
                    Program = program != null
                        ? phoenix.EPGChannelProgrammeObject.Parser.ParseFrom(GrpcSerialize.ProtoSerialize(program))
                        : null,
                    Status = Mapper.Map<Status>(status),
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
                return mediaById != null ? Mapper.Map<GetMediaByIdResponse>(mediaById) : new GetMediaByIdResponse();
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
                return program != null
                    ? Mapper.Map<GetProgramScheduleResponse>(program)
                    : new GetProgramScheduleResponse();
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
                return recording != null
                    ? Mapper.Map<GetDomainRecordingsResponse>(recording)
                    : new GetDomainRecordingsResponse();
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
                return epgIds != null ? Mapper.Map<GetEpgsByIdsResponse>(epgIds) : new GetEpgsByIdsResponse();
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
                            Mapper.Map<RepeatedField<MediaMapper>>(mapMediaFilesResponse)
                        }
                    };
                }

                return new MapMediaFilesResponse();
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

        public GetRecordingLinkByFileTypeResponse GetRecordingLinkByFileType(GetRecordingLinkByFileTypeRequest request)
        {
            try
            {
                var recording =
                    RecordingsDAL.GetRecordingLinkByFileType(request.GroupId, request.RecordingId, request.FileType);
                return new GetRecordingLinkByFileTypeResponse
                {
                    RecordingUrl = recording.Url
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while calling GetRecordingLinkByFileType GRPC service {e.Message}");
                return null;
            }
        }

        public GetGroupMediaFileTypesResponse GetGroupMediaFileTypes(GetGroupMediaFileTypesRequest request)
        {
            try
            {
                var mediaFileType =
                    Core.Catalog.CatalogManagement.FileManager.Instance.GetMediaFileTypes(request.GroupId);
                return new GetGroupMediaFileTypesResponse
                {
                    MediaFileType =
                    {
                        new RepeatedField<MediaFileType>
                        {
                            mediaFileType.Objects.Select(x =>
                                MediaFileType.Parser.ParseFrom(GrpcSerialize.ProtoSerialize(x)))
                        }
                    },
                    Status = Mapper.Map<phoenix.Status>(mediaFileType.Status),
                    TotalCount = mediaFileType.TotalItems
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while calling GetGroupMediaFileTypes GRPC service {e.Message}");
                return null;
            }
        }

        public GetProgramsByChannelIdResponse GetProgramsByChannelId(GetProgramsByChannelIdRequest request)
        {
            var programs = EpgManager.GetPrograms(request.GroupId, request.ChannelId, 0);
            return new GetProgramsByChannelIdResponse
            {
                Programs =
                {
                    programs.Select(x => new Program
                    {
                        Id = x.AssetId,
                        StartDate = Timestamp.FromDateTime(DateTime.SpecifyKind(x.StartDate, DateTimeKind.Utc)),
                        EndDate = Timestamp.FromDateTime(DateTime.SpecifyKind(x.EndDate, DateTimeKind.Utc)),
                    })
                }
            };
        }

        public GetChannelIdsResponse GetChannelIds(GetChannelIdsRequest request)
        {
            return new GetChannelIdsResponse
            {
                Channels = { EpgManager.GetChannelIds(request.GroupId) }
            };
        }
    }
}