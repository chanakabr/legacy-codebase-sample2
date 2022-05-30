using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LiveToVod.BOL;
using LiveToVod.IntegrationTests.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LiveToVod.IntegrationTests
{
    [TestFixture]
    public class RepositoryTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public RepositoryTests()
        {
            _loggerMock = new Mock<ILogger>(MockBehavior.Loose);
        }

        [Test]
        public void GetPartnerConfiguration_EmptyDatabase_ReturnsNull()
        {
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object, false);

            var result = repository.GetPartnerConfiguration(partnerId);

            result.Should().BeNull();
        }

        [Test]
        public void GetPartnerConfiguration_NonEmptyDatabase_ReturnsExpectedConfiguration()
        {
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object);

            var result = repository.GetPartnerConfiguration(partnerId);

            result.Should().NotBeNull();
            result.IsLiveToVodEnabled.Should().BeTrue();
            result.RetentionPeriodDays.Should().Be(10);
            result.MetadataClassifier.Should().Be("metadataClassifier");
        }

        [Test]
        public void GetLinearAssetConfigurations_EmptyDatabase_ReturnsEmptyCollection()
        {
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object, hasLinearAssetsData: false);

            var result = repository.GetLinearAssetConfigurations(partnerId).ToArray();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public void GetLinearAssetConfigurations_NonEmptyDatabase_ReturnsExpectedResult()
        {
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object);

            var result = repository.GetLinearAssetConfigurations(partnerId).ToArray();

            result.Should().NotBeNull();
            result.Length.Should().Be(2);
            result[0].LinearAssetId.Should().Be(RepositoryFactory.LINEAR_ASSET_ID_1);
            result[0].IsLiveToVodEnabled.Should().BeTrue();
            result[0].RetentionPeriodDays.Should().Be(20);
            result[1].LinearAssetId.Should().Be(RepositoryFactory.LINEAR_ASSET_ID_2);
            result[1].IsLiveToVodEnabled.Should().BeFalse();
            result[1].RetentionPeriodDays.Should().BeNull();
        }

        [Test]
        public void GetLinearAssetConfiguration_ConfigurationNotExists_ReturnsNull()
        {
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object);

            var result = repository.GetLinearAssetConfiguration(partnerId, 100);

            result.Should().BeNull();
        }

        [Test]
        public void GetLinearAssetConfiguration_ConfigurationExists_ReturnsExpectedResult()
        {
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object);

            var result = repository.GetLinearAssetConfiguration(partnerId, RepositoryFactory.LINEAR_ASSET_ID_1);

            result.Should().NotBeNull();
            result.LinearAssetId.Should().Be(RepositoryFactory.LINEAR_ASSET_ID_1);
            result.IsLiveToVodEnabled.Should().BeTrue();
            result.RetentionPeriodDays.Should().Be(20);
        }

        [Test]
        public void UpsertPartnerConfiguration_ConfigurationNotExists_InsertsConfiguration()
        {
            var config = new LiveToVodPartnerConfiguration(true, 10, "metaDataClassifier");
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object, false);

            var result = repository.UpsertPartnerConfiguration(partnerId, config, 2);
            var savedConfig = repository.GetPartnerConfiguration(partnerId);

            result.Should().BeTrue();
            savedConfig.IsLiveToVodEnabled.Should().Be(config.IsLiveToVodEnabled);
            savedConfig.RetentionPeriodDays.Should().Be(config.RetentionPeriodDays);
            savedConfig.MetadataClassifier.Should().Be(config.MetadataClassifier);
        }

        [Test]
        public void UpsertPartnerConfiguration_ConfigurationExists_UpdatesConfiguration()
        {
            var config = new LiveToVodPartnerConfiguration(false, 100, "metaDataClassifier_updated");
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object);

            var result = repository.UpsertPartnerConfiguration(partnerId, config, 2);
            var savedConfig = repository.GetPartnerConfiguration(partnerId);

            result.Should().BeTrue();
            savedConfig.IsLiveToVodEnabled.Should().Be(config.IsLiveToVodEnabled);
            savedConfig.RetentionPeriodDays.Should().Be(config.RetentionPeriodDays);
            savedConfig.MetadataClassifier.Should().Be(config.MetadataClassifier);
        }

        [Test]
        public void UpsertPartnerConfiguration_WithConcurrentRequests_DoesNotInsertDuplicateRecords([Range(2, 8)] int numberOfThreads)
        {
            var config = new LiveToVodPartnerConfiguration(false, 10, "metaDataClassifier");
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object);

            Parallel.For(1, numberOfThreads, _ => repository.UpsertPartnerConfiguration(partnerId, config, 2));

            Assert.DoesNotThrow(() => repository.GetPartnerConfiguration(partnerId));
        }

        [Test]
        public void UpsertLinearAssetConfiguration_ConfigurationNotExists_InsertsConfiguration()
        {
            var config = new LiveToVodLinearAssetConfiguration(RepositoryFactory.LINEAR_ASSET_ID_1, true, 20);
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object, hasLinearAssetsData: false);

            var result = repository.UpsertLinearAssetConfiguration(partnerId, config, 2);
            var savedConfig = repository.GetLinearAssetConfiguration(partnerId, config.LinearAssetId);

            result.Should().BeTrue();
            savedConfig.LinearAssetId.Should().Be(config.LinearAssetId);
            savedConfig.IsLiveToVodEnabled.Should().Be(config.IsLiveToVodEnabled);
            savedConfig.RetentionPeriodDays.Should().Be(config.RetentionPeriodDays);
        }

        [TestCase(200)]
        [TestCase(null)]
        public void UpsertLinearAssetConfiguration_ConfigurationExists_UpdatesConfiguration(int? retentionPeriodDays)
        {
            var config = new LiveToVodLinearAssetConfiguration(RepositoryFactory.LINEAR_ASSET_ID_1, false, retentionPeriodDays);
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object);

            var result = repository.UpsertLinearAssetConfiguration(partnerId, config, 2);
            var savedConfig = repository.GetLinearAssetConfiguration(partnerId, config.LinearAssetId);

            result.Should().BeTrue();
            savedConfig.LinearAssetId.Should().Be(config.LinearAssetId);
            savedConfig.IsLiveToVodEnabled.Should().Be(config.IsLiveToVodEnabled);
            savedConfig.RetentionPeriodDays.Should().Be(config.RetentionPeriodDays);
        }

        [Test]
        public void UpsertLinearAssetConfiguration_WithConcurrentRequests_DoesNotInsertDuplicateRecords([Range(2, 8)] int numberOfThreads)
        {
            var config = new LiveToVodLinearAssetConfiguration(RepositoryFactory.LINEAR_ASSET_ID_1, false, 20);
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object);

            Parallel.For(1, numberOfThreads, _ => repository.UpsertLinearAssetConfiguration(partnerId, config, 2));

            Assert.DoesNotThrow(() => repository.GetPartnerConfiguration(partnerId));
        }
    }
}