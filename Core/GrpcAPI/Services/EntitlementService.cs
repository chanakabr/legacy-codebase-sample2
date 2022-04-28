using System;
using System.Reflection;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using Phx.Lib.Appconfig;
using Core.ConditionalAccess;
using Google.Protobuf.WellKnownTypes;
using GrpcAPI.Utils;
using phoenix;
using Phx.Lib.Log;
using AdsPolicy = phoenix.AdsPolicy;
using eService = ApiObjects.eService;
using Module = Core.ConditionalAccess.Module;
using RolePermissions = ApiObjects.RolePermissions;
using Status = phoenix.Status;

namespace GrpcAPI.Services
{
    public interface IEntitlementService
    {
        bool IsServiceAllowed(IsServiceAllowedRequest request);
        GetDomainAdsControlResponse GetDomainAdsControl(GetDomainAdsControlRequest request);
        GetPPVModuleDataResponse GetPPVModuleData(GetPPVModuleDataRequest request);
        Empty HandlePlayUses(HandlePlayUsesRequest request);
    }

    public class EntitlementService : IEntitlementService
    {
        public bool IsServiceAllowed(IsServiceAllowedRequest request)
        {
            return Module.IsServiceAllowed(request.GroupId, (int)request.DomainId,
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

            return null;
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

            return null;
        }
        
        public Empty HandlePlayUses(HandlePlayUsesRequest request)
        {
            BaseConditionalAccess t = null;
            Core.ConditionalAccess.Utils.GetBaseConditionalAccessImpl(ref t, request.GroupId);
            if (t != null)
            {
                MediaFileItemPricesContainer filePrice =
                    GrpcMapping.Mapper.Map<MediaFileItemPricesContainer>(request.Price);
                if (Core.ConditionalAccess.Utils.IsItemPurchased(filePrice))
                {
                    PlayUsesManager.HandlePlayUses(t, filePrice, request.UserId.ToString(), request.MediaFileId, request.Ip, request.CountryCode, request.LanguageCode, request.Udid,
                        request.CouponCode, request.DomainId, request.GroupId, request.IsLive);
                }
                // item must be free otherwise we wouldn't get this far
                else if (ApplicationConfiguration.Current.LicensedLinksCacheConfiguration.ShouldUseCache.Value
                         && filePrice?.m_oItemPrices?.Length > 0)
                {
                   Core.ConditionalAccess.Utils.InsertOrSetCachedEntitlementResults(request.DomainId, request.MediaFileId,
                        new CachedEntitlementResults(0, 0, DateTime.UtcNow, true, false,
                            eTransactionType.PPV, null, filePrice.m_oItemPrices[0].m_dtEndDate, request.IsLive));
                }
            }
            return new Empty();
        }
    }
}