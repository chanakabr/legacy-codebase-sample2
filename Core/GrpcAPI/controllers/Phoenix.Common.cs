using Grpc.Core;
using GrpcAPI.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using CachingProvider.LayeredCache;
using MoreLinq.Extensions;

namespace Grpc.controllers
{
    public partial class PhoenixController : phoenix.Phoenix.PhoenixBase
    {
        private readonly ILogger<PhoenixController> _logger;
        private readonly IEntitlementService _entitlementService;
        private readonly IHouseholdService _householdService;
        private readonly IPricingService _pricingService;
        private readonly ICatalogService _catalogService;
        private readonly IAssetRuleService _assetRuleService;
        private readonly IGroupAndConfigurationService _groupAndConfigurationService;
        private readonly ISegmentService _segmentService;
        private readonly IAssetUserRuleService _assetUserRuleService;
        private const string InvalidationKey = "invalidationkey";

        public PhoenixController(ILogger<PhoenixController> logger, IEntitlementService entitlementService,
            IHouseholdService householdService, IPricingService pricingService, ICatalogService catalogService,
            IAssetRuleService assetRuleService, IGroupAndConfigurationService groupAndConfigurationService,
            ISegmentService segmentService, IAssetUserRuleService assetUserRuleService)
        {
            _logger = logger;
            _entitlementService = entitlementService;
            _householdService = householdService;
            _pricingService = pricingService;
            _catalogService = catalogService;
            _assetRuleService = assetRuleService;
            _groupAndConfigurationService = groupAndConfigurationService;
            _segmentService = segmentService;
            _assetUserRuleService = assetUserRuleService;
        }

        private static Metadata GetInvalidationKeysHeader(HashSet<string> invalidationKeys)
        {
            var headers = new Metadata();
            invalidationKeys?.ForEach(x =>
            {
                if (!string.IsNullOrEmpty(x) && !x.Equals(LayeredCacheKeys.GetProceduresRoutingInvalidationKey()))
                    headers.Add(new Metadata.Entry(InvalidationKey, x));
            });
            return headers;
        }
    }
}