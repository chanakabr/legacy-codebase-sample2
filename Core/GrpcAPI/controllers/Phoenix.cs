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
    public class PhoneixController : phoenix.Phoenix.PhoenixBase
    {
        private readonly ILogger<PhoneixController> _logger;
        private readonly IEntitlementService _entitlementService;
        private readonly IHouseholdService _householdService;
        private readonly IPricingService _pricingService;
        private readonly ICatalogService _catalogService;
        private readonly IAssetRuleService _assetRuleService;
        private readonly IGroupAndConfigurationService _groupAndConfigurationService;
        private const string InvalidationKey = "invalidationkey";

        public PhoneixController(ILogger<PhoneixController> logger, IEntitlementService entitlementService,
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
            return Task.FromResult(_catalogService.GetMediaFiles(request));
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
            return Task.FromResult(_catalogService.GetMediaById(request));
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
    }
}