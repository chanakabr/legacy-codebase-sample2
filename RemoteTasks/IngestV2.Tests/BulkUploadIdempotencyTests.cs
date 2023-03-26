using System.Threading.Tasks;
using FeatureFlag;
using FluentAssertions;
using IngestHandler.Common.Infrastructure;
using IngestHandler.Common.Managers;
using IngestHandler.Common.Repositories;
using IngestHandler.Common.Repositories.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace IngestV2.Tests
{
    [TestFixture]
    public class BulkUploadIdempotencyTests
    {
        private MockRepository _mockRepository;
        private Mock<IBulkUploadRepository> _bulkUploadRepositoryMock;
        private Mock<IBulkCompletedRetryPolicyConfiguration> _bulkCompletedRetryPolicyConfiguration;
        private Mock<ILogger<BulkUploadService>> _loggerMock;
        private Mock<IPhoenixFeatureFlag> _phoenixFeatureFlagMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _bulkUploadRepositoryMock = _mockRepository.Create<IBulkUploadRepository>();
            _bulkCompletedRetryPolicyConfiguration = _mockRepository.Create<IBulkCompletedRetryPolicyConfiguration>();
            _loggerMock = _mockRepository.Create<ILogger<BulkUploadService>>(MockBehavior.Loose);
            _phoenixFeatureFlagMock = _mockRepository.Create<IPhoenixFeatureFlag>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public async Task IsLinearChannelOfBulkUploadFinished_NotInProgress()
        {
            var partnerId = 39842;
            var bulkUploadId = 935824;
            var linearChannelId = 6049435;
            _bulkUploadRepositoryMock.Setup(x => x.IsLinearChannelOfBulkUploadInProgress(partnerId, bulkUploadId, linearChannelId)).ReturnsAsync(false);
            _phoenixFeatureFlagMock.Setup(x => x.IsKafkaIdempotencyShieldShouldNotBeUsed()).Returns(false);

            var bulkUploadService = new BulkUploadService(_bulkUploadRepositoryMock.Object, _bulkCompletedRetryPolicyConfiguration.Object, _loggerMock.Object, _phoenixFeatureFlagMock.Object);
            var shouldProcessBulkUpload = await bulkUploadService.ShouldProcessLinearChannelOfBulkUpload(partnerId, bulkUploadId, linearChannelId);

            shouldProcessBulkUpload.Should().BeTrue();
        }

        [Test]
        public async Task IsLinearChannelOfBulkUploadFinished_KafkaFeatureIdempotencyDisabled_NotInProgress()
        {
            var partnerId = 39842;
            var bulkUploadId = 935824;
            var linearChannelId = 6049435;
            _bulkUploadRepositoryMock.Setup(x => x.IsLinearChannelOfBulkUploadInProgress(partnerId, bulkUploadId, linearChannelId)).ReturnsAsync(false);
            _phoenixFeatureFlagMock.Setup(x => x.IsKafkaIdempotencyShieldShouldNotBeUsed()).Returns(true);

            var bulkUploadService = new BulkUploadService(_bulkUploadRepositoryMock.Object, _bulkCompletedRetryPolicyConfiguration.Object, _loggerMock.Object, _phoenixFeatureFlagMock.Object);
            var shouldProcessBulkUpload = await bulkUploadService.ShouldProcessLinearChannelOfBulkUpload(partnerId, bulkUploadId, linearChannelId);

            shouldProcessBulkUpload.Should().BeTrue();
        }

        [Test]
        public async Task IsLinearChannelOfBulkUploadFinished_KafkaFeatureIdempotencyDisabled_InProgress()
        {
            var partnerId = 39842;
            var bulkUploadId = 935824;
            var linearChannelId = 6049435;
            _bulkUploadRepositoryMock.Setup(x => x.IsLinearChannelOfBulkUploadInProgress(partnerId, bulkUploadId, linearChannelId)).ReturnsAsync(true);
            _phoenixFeatureFlagMock.Setup(x => x.IsKafkaIdempotencyShieldShouldNotBeUsed()).Returns(true);

            var bulkUploadService = new BulkUploadService(_bulkUploadRepositoryMock.Object, _bulkCompletedRetryPolicyConfiguration.Object, _loggerMock.Object, _phoenixFeatureFlagMock.Object);
            var shouldProcessBulkUpload = await bulkUploadService.ShouldProcessLinearChannelOfBulkUpload(partnerId, bulkUploadId, linearChannelId);

            shouldProcessBulkUpload.Should().BeFalse();
        }

        [Test]
        public async Task IsLinearChannelOfBulkUploadFinished_InProgressButFinished()
        {
            var partnerId = 39842;
            var bulkUploadId = 935824;
            var linearChannelId = 6049435;
            _bulkUploadRepositoryMock.Setup(x => x.IsLinearChannelOfBulkUploadInProgress(partnerId, bulkUploadId, linearChannelId)).ReturnsAsync(true);
            _phoenixFeatureFlagMock.Setup(x => x.IsKafkaIdempotencyShieldShouldNotBeUsed()).Returns(false);
            _bulkCompletedRetryPolicyConfiguration.SetupGet(x => x.Timeout).Returns(1);
            _bulkCompletedRetryPolicyConfiguration.SetupGet(x => x.Duration).Returns(5);
            _bulkUploadRepositoryMock.Setup(x => x.GetBulkUploadIdempotency(partnerId, bulkUploadId, linearChannelId))
                .ReturnsAsync(() => new BulkUploadIdempotencyDocument(bulkUploadId, linearChannelId, BulkUploadIdempotencyStatus.Completed));

            var bulkUploadService = new BulkUploadService(_bulkUploadRepositoryMock.Object, _bulkCompletedRetryPolicyConfiguration.Object, _loggerMock.Object, _phoenixFeatureFlagMock.Object);
            var shouldProcessBulkUpload = await bulkUploadService.ShouldProcessLinearChannelOfBulkUpload(partnerId, bulkUploadId, linearChannelId);

            shouldProcessBulkUpload.Should().BeFalse();
        }

        [Test]
        public async Task IsLinearChannelOfBulkUploadFinished_InProgressButNotFinishedDuringRetry()
        {
            var partnerId = 39842;
            var bulkUploadId = 935824;
            var linearChannelId = 6049435;
            _bulkUploadRepositoryMock.Setup(x => x.IsLinearChannelOfBulkUploadInProgress(partnerId, bulkUploadId, linearChannelId)).ReturnsAsync(true);
            _phoenixFeatureFlagMock.Setup(x => x.IsKafkaIdempotencyShieldShouldNotBeUsed()).Returns(false);
            _bulkCompletedRetryPolicyConfiguration.SetupGet(x => x.Timeout).Returns(1);
            _bulkCompletedRetryPolicyConfiguration.SetupGet(x => x.Duration).Returns(5);
            _bulkUploadRepositoryMock.Setup(x => x.GetBulkUploadIdempotency(partnerId, bulkUploadId, linearChannelId))
                .ReturnsAsync(() => new BulkUploadIdempotencyDocument(bulkUploadId, linearChannelId));

            var bulkUploadService = new BulkUploadService(_bulkUploadRepositoryMock.Object, _bulkCompletedRetryPolicyConfiguration.Object, _loggerMock.Object, _phoenixFeatureFlagMock.Object);
            var shouldProcessBulkUpload = await bulkUploadService.ShouldProcessLinearChannelOfBulkUpload(partnerId, bulkUploadId, linearChannelId);

            shouldProcessBulkUpload.Should().BeTrue();
        }
    }
}
