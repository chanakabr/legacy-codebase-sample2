using System.Collections.Generic;
using ApiLogic;
using ApiLogic.Pricing.Handlers;
using AutoMapper;
using Core.ConditionalAccess;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using GrpcAPI.Utils;
using phoenix;
using AdsPolicy = phoenix.AdsPolicy;
using eService = ApiObjects.eService;
using MediaFile = ApiObjects.MediaFile;
using Module = Core.ConditionalAccess.Module;

namespace GrpcAPI.Services
{
    public interface IEntitlementService
    {
        bool IsServiceAllowed(IsServiceAllowedRequest request);
        GetDomainAdsControlResponse GetDomainAdsControl(GetDomainAdsControlRequest request);
        GetPPVModuleDataResponse GetPPVModuleData(GetPPVModuleDataRequest request);
        Empty HandlePlayUses(HandlePlayUsesRequest request);
        bool CheckProgramAssetGroupExistence(CheckProgramAssetGroupExistenceRequest request);
        GetEntitledPagoWindowResponse GetEntitledPagoWindow(GetEntitledPagoWindowRequest request);
    }

    public class EntitlementService : IEntitlementService
    {
        public bool IsServiceAllowed(IsServiceAllowedRequest request)
        {
            return Module.IsServiceAllowed(request.GroupId, (int) request.DomainId,
                (eService) request.Service);
        }

        public GetDomainAdsControlResponse GetDomainAdsControl(GetDomainAdsControlRequest request)
        {
            var defaultAdsData =
                PlaybackManager.GetDomainAdsControl(request.GroupId, request.DomainId);
            if (defaultAdsData != null)
            {
                return new GetDomainAdsControlResponse()
                {
                    AdsControlData = new AdsControlData()
                    {
                        AdsParam = defaultAdsData.AdsParam,
                        FileId = defaultAdsData.FileId,
                        FileType = defaultAdsData.FileType,
                        AdsPolicy = defaultAdsData.AdsPolicy.HasValue
                            ? (AdsPolicy) defaultAdsData.AdsPolicy.Value
                            : AdsPolicy.None
                    }
                };
            }

            return new GetDomainAdsControlResponse();
        }

        public GetPPVModuleDataResponse GetPPVModuleData(GetPPVModuleDataRequest request)
        {
            var ppv = Core.Pricing.Module.GetPPVModuleData(request.GroupId,
                request.SPPVCode, string.Empty, string.Empty, request.Udid);
            if (ppv != null && ppv.AdsPolicy != null)
            {
                return new GetPPVModuleDataResponse()
                {
                    AdsControlData = new AdsControlData()
                    {
                        AdsPolicy = (AdsPolicy) ppv.AdsPolicy,
                        AdsParam = ppv.AdsParam,
                    }
                };
            }

            return new GetPPVModuleDataResponse();
        }

        public Empty HandlePlayUses(HandlePlayUsesRequest request)
        {
            BaseConditionalAccess t = null;
            Core.ConditionalAccess.Utils.GetBaseConditionalAccessImpl(ref t, request.GroupId);
            if (t != null)
            {
                var playbackEntitlementContainer = new PlaybackContextOut
                {
                    MediaFileItemPrices = request.Price != null ?
                        Mapper.Map<Core.ConditionalAccess.MediaFileItemPricesContainer>(request.Price) : null,
                    PagoProgramAvailability = request.PagoProgramAvailability != null ?
                        GrpcSerialize.ProtoDeserialize<ApiObjects.PagoProgramAvailability>(
                            request.PagoProgramAvailability.ToByteArray()) : null
                };
                PlaybackManager.HandlePlayUsesAndDevicePlayData(t, request.UserId.ToString(), request.DomainId,
                    request.MediaFileId, request.Ip, request.Udid, playbackEntitlementContainer, null, request.IsLive);

            }

            return new Empty();
        }

        public bool CheckProgramAssetGroupExistence(CheckProgramAssetGroupExistenceRequest request)
        {
            return PagoManager.Instance.GetProgramAssetGroupOfferIds((long) request.GroupId, false).Count > 0;
        }

        public GetEntitledPagoWindowResponse GetEntitledPagoWindow(GetEntitledPagoWindowRequest request)
        {
            var pagoProgramAvailability = Core.ConditionalAccess.Utils.GetEntitledPagoWindow(request.GroupId,
                (int) request.DomainId,
                request.AssetId, (ApiObjects.eAssetTypes) request.AssetType,
                Mapper.Map<List<MediaFile>>(request.Files),
                GrpcSerialize.ProtoDeserialize<ApiObjects.EPGChannelProgrammeObject>(request.EpgProgram.ToByteArray()));
            return new GetEntitledPagoWindowResponse
            {
                PagoResponse = pagoProgramAvailability != null ?
                    phoenix.PagoProgramAvailability.Parser.ParseFrom(
                        GrpcSerialize.ProtoSerialize(pagoProgramAvailability)) : null
            };
        }
    }
}