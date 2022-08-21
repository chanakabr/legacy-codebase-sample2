using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CachingProvider.LayeredCache;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcAPI.Services;
using Microsoft.Extensions.Logging;
using phoenix;

namespace Grpc.controllers
{
    public class PhoenixController : phoenix.Phoenix.PhoenixBase
    {
        private readonly ILogger<PhoenixController> _logger;
        private readonly IEntitlementService _entitlementService;
        private readonly IHouseholdService _householdService;
        private readonly IPricingService _pricingService;
        private readonly ICatalogService _catalogService;
        private readonly IAssetRuleService _assetRuleService;
        private readonly IGroupAndConfigurationService _groupAndConfigurationService;
        private const string InvalidationKey = "invalidationkey";

        public PhoenixController(ILogger<PhoenixController> logger, IEntitlementService entitlementService,
            IHouseholdService householdService, IPricingService pricingService, ICatalogService catalogService,
            IAssetRuleService assetRuleService, IGroupAndConfigurationService groupAndConfigurationService)
        {
            _logger = logger;
            _entitlementService = entitlementService;
            _householdService = householdService;
            _pricingService = pricingService;
            _catalogService = catalogService;
            _assetRuleService = assetRuleService;
            _groupAndConfigurationService = groupAndConfigurationService;
        }

        private static Metadata GetInvalidationKeysHeader(List<string> invalidationKeys)
        {
            var headers = new Metadata();
            invalidationKeys?.ForEach(x =>
            {
                if (!string.IsNullOrEmpty(x))
                    headers.Add(new Metadata.Entry(InvalidationKey, x));
            });
            return headers;
        }

        public override Task<BoolValue> IsPermittedPermission(IsPermittedPermissionRequest request,
            ServerCallContext context)
        {
            var response = _householdService.IsPermittedPermission(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(new BoolValue {Value = response});
        }

        public override Task<GetSuspensionStatusResponse> GetSuspensionStatus(GetSuspensionStatusRequest request,
            ServerCallContext context)
        {
            var response = _householdService.GetSuspensionStatus(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<BoolValue> IsServiceAllowed(IsServiceAllowedRequest request, ServerCallContext context)
        {
            return Task.FromResult(new BoolValue()
            {
                Value = _entitlementService.IsServiceAllowed(request),
            });
        }

        public override Task<GetDomainDataResponse> GetDomainData(GetDomainDataRequest request,
            ServerCallContext context)
        {
            var response = _householdService.GetDomainData(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetDomainAdsControlResponse> GetDomainAdsControl(GetDomainAdsControlRequest request,
            ServerCallContext context)
        {
            var response = _entitlementService.GetDomainAdsControl(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetItemsPricesResponse> GetItemsPrices(GetItemsPricesRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(_pricingService.GetItemsPrices(request));
        }

        public override Task<GetPPVModuleDataResponse> GetPPVModuleData(GetPPVModuleDataRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(_entitlementService.GetPPVModuleData(request));
        }

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

        public override Task<GetAssetMediaRuleIdsResponse> GetAssetMediaRuleIds(GetAssetMediaRuleIdsRequest request,
            ServerCallContext context)
        {
            var response = _assetRuleService.GetAssetMediaRuleIds(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetAssetEpgRuleIdsResponse> GetAssetEpgRuleIds(GetAssetEpgRuleIdsRequest request,
            ServerCallContext context)
        {
            var response = _assetRuleService.GetAssetEpgRuleIds(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetMediaConcurrencyRulesResponse> GetMediaConcurrencyRules(
            GetMediaConcurrencyRulesRequest request, ServerCallContext context)
        {
            var response = _assetRuleService.GetMediaConcurrencyRules(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetMediaConcurrencyRulesByDomainLimitationModuleResponse>
            GetMediaConcurrencyRulesByDomainLimitationModule(
                GetMediaConcurrencyRulesByDomainLimitationModuleRequest request, ServerCallContext context)
        {
            var response = _householdService.GetMediaConcurrencyRulesByDomainLimitationModule(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
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

        public override Task<BoolValue>
            HasAssetRules(
                HasAssetRulesRequest request, ServerCallContext context)
        {
            var response = _assetRuleService.HasAssetRules(request);
            var invalidationKeyFromRequest = new List<string>{LayeredCacheKeys.GetAllAssetRulesGroupInvalidationKey(request.GroupId)};
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(new BoolValue{Value = response});
        }

        
        public override Task<CheckNetworkRulesResponse>
            CheckNetworkRules(
                CheckNetworkRulesRequest request, ServerCallContext context)
        {
            var response = _assetRuleService.CheckNetworkRules(request);
            return Task.FromResult(response);
        }

        public override Task<GetAssetRulesResponse>
            GetAssetRules(
                GetAssetRulesRequest request, ServerCallContext context)
        {
            var response = _assetRuleService.GetAssetRules(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
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

        public override Task<GetGroupMediaConcurrencyRulesResponse>
            GetGroupMediaConcurrencyRules(
                GetGroupMediaConcurrencyRulesRequest request, ServerCallContext context)
        {
            var response = _assetRuleService.GetGroupMediaConcurrencyRules(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetMediaConcurrencyByIdResponse>
            GetMediaConcurrencyRule(
                GetMediaConcurrencyByIdRequest request, ServerCallContext context)
        {
            var response = _assetRuleService.GetMediaConcurrencyRule(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
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

        public override Task<Empty> HandlePlayUses(
            HandlePlayUsesRequest request, ServerCallContext context)
        {
            return Task.FromResult(_entitlementService.HandlePlayUses(request));
        }

        public override Task<MapMediaFilesResponse> MapMediaFiles(
            MapMediaFilesRequest request, ServerCallContext context)
        {
            var response = _catalogService.MapMediaFiles(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(response);
        }

        public override Task<GetGroupSecretAndCountryCodeResponse> GetGroupSecretAndCountryCode(
            GetGroupSecretAndCountryCodeRequest request, ServerCallContext context)
        {
            return Task.FromResult(_groupAndConfigurationService.GetGroupSecretAndCountryCode(request));
        }

        public override Task<StringValue> GetEPGChannelCDVRId(
            GetEPGChannelCDVRIdRequest request, ServerCallContext context)
        {
            return Task.FromResult(new StringValue {Value = _catalogService.GetEPGChannelCDVRId(request)});
        }

        public override Task<Int32Value> IsDevicePlayValid(
            IsDevicePlayValidRequest request, ServerCallContext context)
        {
            var response = _householdService.IsDevicePlayValid(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(new Int32Value {Value = response});
        }

        public override Task<BoolValue> AllowActionInSuspendedDomain(
            AllowActionInSuspendedDomainRequest request, ServerCallContext context)
        {
            var response = _householdService.AllowActionInSuspendedDomain(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(new BoolValue {Value = response});
        }
        
        public override Task<GetPaymentGatewayProfileResponse> GetPaymentGatewayProfile(
            GetPaymentGatewayProfileRequest request, ServerCallContext context)
        { 
            return Task.FromResult(_pricingService.GetPaymentGatewayProfile(request));
        }
        
        public override Task<BoolValue> GetGroupHasSubWithAds(
            GetGroupHasSubWithAdsRequest request, ServerCallContext context)
        {
            var response = _pricingService.GetGroupHasSubWithAds(request);
            return Task.FromResult(new BoolValue {Value = response});
        }
        
        //TODO add invalidation keys for client cache cache
        public override Task<GetCDVRAdapterResponse> GetCDVRAdapter(GetCDVRAdapterRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(_groupAndConfigurationService.GetCDVRAdapter(request));
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
        
        public override Task<BoolValue> CheckProgramAssetGroupExistence(CheckProgramAssetGroupExistenceRequest request,
            ServerCallContext context)
        {
            var response = _entitlementService.CheckProgramAssetGroupExistence(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest));
            return Task.FromResult(new BoolValue{Value = response});        
        }
        
        public override Task<GetEntitledPagoWindowResponse> GetEntitledPagoWindow(GetEntitledPagoWindowRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(_entitlementService.GetEntitledPagoWindow(request));
        }

        public override Task<BoolValue> IsRegionalization(IsRegionalizationRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(new BoolValue {Value = _groupAndConfigurationService.IsRegionalization(request)});
        }

        public override Task<GetDefaultRegionIdResponse> GetDefaultRegionId(GetDefaultRegionIdRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(_groupAndConfigurationService.GetDefaultRegionId(request));
        }

        public override Task<GetNotificationPartnerSettingsResponse> GetNotificationPartnerSettings(
            GetNotificationPartnerSettingsRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(_groupAndConfigurationService.GetNotificationPartnerSettings(request));
        }
        
        public override Task<BoolValue> IsValidDeviceFamily(
            IsValidDeviceFamilyRequest request,
            ServerCallContext context)
        {
            return Task.FromResult(new BoolValue {Value = _householdService.IsValidDeviceFamily(request)});
        }
       
        public override Task<BoolValue> IsMediaFileFree(IsMediaFileFreeRequest request, ServerCallContext context)
        {
            var response = _pricingService.IsMediaFileFree(request);
            var invalidationKeyFromRequest = LayeredCache.GetInvalidationKeyFromRequest();
            context.WriteResponseHeadersAsync(GetInvalidationKeysHeader(invalidationKeyFromRequest)); 
            return Task.FromResult(new BoolValue { Value =  response});
        }
    }
}