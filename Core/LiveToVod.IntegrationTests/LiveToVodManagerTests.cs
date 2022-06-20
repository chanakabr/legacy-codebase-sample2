using System;
using ApiLogic.Catalog.CatalogManagement.Services;
using CachingProvider.LayeredCache;
using Core.Tests;
using FluentAssertions;
using LiveToVod.BOL;
using LiveToVod.IntegrationTests.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LiveToVod.IntegrationTests
{
    [TestFixture]
    public class LiveToVodManagerTests
    {
        private MockRepository _mockRepository;
        private Mock<ILiveToVodService> _liveToVodServiceMock;
        private Mock<ILayeredCache> _layeredCacheMock;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _liveToVodServiceMock = _mockRepository.Create<ILiveToVodService>();
            _layeredCacheMock = _mockRepository.Create<ILayeredCache>();
            _loggerMock = _mockRepository.Create<ILogger>(MockBehavior.Loose);
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void GetFullConfiguration_PartnerConfigExistsAndLinearAssetConfigsIsEmpty_ReturnsExpectedResult()
        {
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object, hasLinearAssetsData: false);
            var manager = new LiveToVodManager(repository, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _loggerMock.Object);

            var result = manager.GetFullConfiguration(partnerId);

            result.Should().NotBeNull();
            result.IsLiveToVodEnabled.Should().BeTrue();
            result.RetentionPeriodDays.Should().Be(10);
            result.MetadataClassifier.Should().Be("metadataClassifier");
            result.LinearAssets.Should().BeEmpty();
        }

        [Test]
        public void GetFullConfiguration_PartnerConfigAndLinearAssetConfigsExist_ReturnsExpectedResult()
        {
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object);
            var manager = new LiveToVodManager(repository, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _loggerMock.Object);

            var result = manager.GetFullConfiguration(partnerId);

            result.Should().NotBeNull();
            result.IsLiveToVodEnabled.Should().BeTrue();
            result.RetentionPeriodDays.Should().Be(10);
            result.MetadataClassifier.Should().Be("metadataClassifier");
            result.LinearAssets.Should().NotBeEmpty();
            result.LinearAssets.Count.Should().Be(2);
            result.LinearAssets[0].LinearAssetId.Should().Be(11);
            result.LinearAssets[0].IsLiveToVodEnabled.Should().BeTrue();
            result.LinearAssets[0].RetentionPeriodDays.Should().Be(20);
            result.LinearAssets[1].LinearAssetId.Should().Be(12);
            result.LinearAssets[1].IsLiveToVodEnabled.Should().BeFalse();
            result.LinearAssets[1].RetentionPeriodDays.Should().Be(10);
        }

        [Test]
        public void GetFullConfiguration_PartnerConfigNotExistsAndLinearAssetConfigsExist_ReturnsExpectedResult()
        {
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object, false);
            _loggerMock
                .Setup(LogLevel.Warning, $"LiveToVodPartnerConfiguration has not been found. The default configuration will be returned. partnerId={partnerId}.");
            var manager = new LiveToVodManager(repository, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _loggerMock.Object);

            var result = manager.GetFullConfiguration(partnerId);

            result.Should().NotBeNull();
            result.IsLiveToVodEnabled.Should().BeFalse();
            result.RetentionPeriodDays.Should().Be(0);
            result.MetadataClassifier.Should().BeNull();
            result.LinearAssets.Should().NotBeEmpty();
            result.LinearAssets.Count.Should().Be(2);
            result.LinearAssets[0].LinearAssetId.Should().Be(11);
            result.LinearAssets[0].IsLiveToVodEnabled.Should().BeFalse();
            result.LinearAssets[0].RetentionPeriodDays.Should().Be(20);
            result.LinearAssets[1].LinearAssetId.Should().Be(12);
            result.LinearAssets[1].IsLiveToVodEnabled.Should().BeFalse();
            result.LinearAssets[1].RetentionPeriodDays.Should().Be(0);
        }

        [Test]
        public void GetPartnerConfiguration_PartnerConfigExists_ReturnsExpectedResult()
        {
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object);
            var manager = new LiveToVodManager(repository, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _loggerMock.Object);

            var result = manager.GetPartnerConfiguration(partnerId);

            result.Should().NotBeNull();
            result.IsLiveToVodEnabled.Should().BeTrue();
            result.RetentionPeriodDays.Should().Be(10);
            result.MetadataClassifier.Should().Be("metadataClassifier");
        }

        [Test]
        public void GetPartnerConfiguration_PartnerConfigNotExists_ReturnsDefaultConfiguration()
        {
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object, false);
            _loggerMock
                .Setup(LogLevel.Warning, $"LiveToVodPartnerConfiguration has not been found. The default configuration will be returned. partnerId={partnerId}.");
            var manager = new LiveToVodManager(repository, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _loggerMock.Object);

            var result = manager.GetPartnerConfiguration(partnerId);

            result.Should().NotBeNull();
            result.IsLiveToVodEnabled.Should().BeFalse();
            result.RetentionPeriodDays.Should().Be(0);
            result.MetadataClassifier.Should().BeNull();
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void GetLinearAssetConfiguration_LinearAssetConfigsExists_ReturnsExpectedResult(bool isLiveToVodEnabledPartnerConfig, bool isLiveToVodEnabledLinearAssetConfig)
        {
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object);
            repository.UpsertPartnerConfiguration(partnerId, new LiveToVodPartnerConfiguration(isLiveToVodEnabledPartnerConfig, 0, "metadataClassifier"), 2);
            repository.UpsertLinearAssetConfiguration(partnerId, new LiveToVodLinearAssetConfiguration(1, isLiveToVodEnabledLinearAssetConfig, 10), 2);
            var manager = new LiveToVodManager(repository, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _loggerMock.Object);

            var result = manager.GetLinearAssetConfiguration(partnerId, 1);

            result.Should().NotBeNull();
            result.LinearAssetId.Should().Be(1);
            result.IsLiveToVodEnabled.Should().Be(isLiveToVodEnabledPartnerConfig && isLiveToVodEnabledLinearAssetConfig);
            result.RetentionPeriodDays.Should().Be(10);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetLinearAssetConfiguration_LinearAssetConfigsNotExists_ReturnsDefaultConfiguration(bool isLiveToVodEnabledPartnerConfig)
        {
            var (partnerId, repository) = RepositoryFactory.Get(_loggerMock.Object, hasLinearAssetsData: false);
            repository.UpsertPartnerConfiguration(partnerId, new LiveToVodPartnerConfiguration(isLiveToVodEnabledPartnerConfig, 10, "metadataClassifier"), 1);
            _loggerMock
                .Setup(LogLevel.Warning, $"LiveToVodLinearAssetConfiguration has not been found. partnerId={partnerId}, linearAssetId=1.");
            var manager = new LiveToVodManager(repository, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _loggerMock.Object);

            var result = manager.GetLinearAssetConfiguration(partnerId, 1);

            result.Should().NotBeNull();
            result.LinearAssetId.Should().Be(1);
            result.IsLiveToVodEnabled.Should().Be(isLiveToVodEnabledPartnerConfig);
            result.RetentionPeriodDays.Should().Be(10);
        }
    }
}