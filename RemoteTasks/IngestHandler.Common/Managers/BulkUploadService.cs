using System;
using System.Threading;
using System.Threading.Tasks;
using FeatureFlag;
using IngestHandler.Common.Domain.Exceptions;
using IngestHandler.Common.Infrastructure;
using IngestHandler.Common.Managers.Abstractions;
using IngestHandler.Common.Repositories;
using IngestHandler.Common.Repositories.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace IngestHandler.Common.Managers
{
    public class BulkUploadService : IBulkUploadService
    {
        private readonly IBulkUploadRepository _bulkUploadRepository;
        private readonly IBulkCompletedRetryPolicyConfiguration _bulkCompletedRetryPolicyConfiguration;
        private readonly ILogger<BulkUploadService> _logger;
        private readonly IPhoenixFeatureFlag _phoenixFeatureFlag;
        private readonly Lazy<AsyncRetryPolicy> _bulkCompletedRetryPolicy;

        public BulkUploadService(
            IBulkUploadRepository bulkUploadRepository,
            IBulkCompletedRetryPolicyConfiguration bulkCompletedRetryPolicyConfiguration,
            ILogger<BulkUploadService> logger,
            IPhoenixFeatureFlag phoenixFeatureFlag)
        {
            _bulkUploadRepository = bulkUploadRepository;
            _bulkCompletedRetryPolicyConfiguration = bulkCompletedRetryPolicyConfiguration;
            _logger = logger;
            _phoenixFeatureFlag = phoenixFeatureFlag;
            _bulkCompletedRetryPolicy = new Lazy<AsyncRetryPolicy>(GetBulkCompletedRetryPolicy, LazyThreadSafetyMode.PublicationOnly);
        }

        public async Task<bool> ShouldProcessLinearChannelOfBulkUpload(int partnerId, long bulkUploadId, long linearChannelId)
        {
            var isInProgress = await _bulkUploadRepository.IsLinearChannelOfBulkUploadInProgress(partnerId, bulkUploadId, linearChannelId);
            if (_phoenixFeatureFlag.IsKafkaIdempotencyShieldShouldNotBeUsed())
            {
                return !isInProgress;
            }

            if (!isInProgress)
            {
                return true;
            }

            var bulkCompletedPolicyResult = await _bulkCompletedRetryPolicy.Value.ExecuteAndCaptureAsync(() => IsBulkLinearUploadCompleted(partnerId, bulkUploadId, linearChannelId));
            if (bulkCompletedPolicyResult.Outcome == OutcomeType.Failure)
            {
                _logger.LogError($"BulkUpload ({bulkUploadId}) for partner ({partnerId}) hasn't been finished in {_bulkCompletedRetryPolicyConfiguration.Duration} seconds.");
                return true;
            }

            _logger.LogInformation($"BulkUpload ({bulkUploadId}) for partner ({partnerId}) has been finished in {_bulkCompletedRetryPolicyConfiguration.Duration} seconds.");
            return false;
        }

        private async Task IsBulkLinearUploadCompleted(int partnerId, long bulkUploadId, long linearChannelId)
        {
            var bulkUploadIdempotency = await _bulkUploadRepository.GetBulkUploadIdempotency(partnerId, bulkUploadId, linearChannelId);
            if (bulkUploadIdempotency == null)
            {
                _logger.LogWarning($"BulkUpload ({bulkUploadId}) for linear channel ({linearChannelId}) couldn't be found for partner ({partnerId}).");
                throw new BulkUploadNotFinishedException();
            }

            if (bulkUploadIdempotency.Status == BulkUploadIdempotencyStatus.InProgress)
            {
                _logger.LogWarning($"BulkUpload ({bulkUploadId}) is still in progress for linear channel ({linearChannelId}) on another ingest-handler for partner ({partnerId}).");
                throw new BulkUploadNotFinishedException();
            }
        }

        private AsyncRetryPolicy GetBulkCompletedRetryPolicy()
        {
            var numberOfRetries = (int)Math.Ceiling((double)_bulkCompletedRetryPolicyConfiguration.Duration / _bulkCompletedRetryPolicyConfiguration.Timeout);
            return Policy.Handle<BulkUploadNotFinishedException>().WaitAndRetryAsync(numberOfRetries, _ => TimeSpan.FromSeconds(_bulkCompletedRetryPolicyConfiguration.Timeout));
        }
    }
}
