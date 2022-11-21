using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiObjects.Cloudfront;
using Microsoft.Extensions.Logging;

namespace CloudfrontInvalidatorGrpcClientWrapper
{
    public class CloudfrontTtlInvalidator : ICloudfrontInvalidator
    {
        private static readonly TimeSpan CloudfrontInvalidationTtl = TimeSpan.FromMinutes(3);
        private readonly ILogger<CloudfrontTtlInvalidator> _logger;

        public CloudfrontTtlInvalidator(ILogger<CloudfrontTtlInvalidator> logger)
        {
            _logger = logger;
        }
        
        public async Task<(bool success, IEnumerable<string> failedPaths)> InvalidateAndWaitAsync(int partnerId, string[] path, WaitConfig waitConfig)
        {
            _logger.LogInformation("Wait [{Duration}], so Cloudfront will be invalidated by TTL", CloudfrontInvalidationTtl);
            await Task.Delay(CloudfrontInvalidationTtl);
            return (true, Enumerable.Empty<string>());
        }
    }
}