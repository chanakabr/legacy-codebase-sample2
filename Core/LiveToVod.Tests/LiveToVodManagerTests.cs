using System.Collections.Generic;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Tests;
using FluentAssertions;
using LiveToVod.BOL;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LiveToVod.Tests
{
    [TestFixture]
    public class LiveToVodManagerTests
    {
        private MockRepository _mockRepository;
        private Mock<IRepository> _repositoryMock;
        private Mock<ILiveToVodService> _liveToVodServiceMock;
        private Mock<ILayeredCache> _layeredCacheMock;
        private Mock<ICatalogManager> _catalogManagerMock;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _repositoryMock = _mockRepository.Create<IRepository>();
            _liveToVodServiceMock = _mockRepository.Create<ILiveToVodService>();
            _layeredCacheMock = _mockRepository.Create<ILayeredCache>();
            _catalogManagerMock = _mockRepository.Create<ICatalogManager>();
            _loggerMock = _mockRepository.Create<ILogger>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }
        
        [Test]
        public void UpdatePartnerConfiguration_ExistenceOfAssetStructShouldNotBeChecked_ReturnsExpectedResult()
        {
            var config = new LiveToVodPartnerConfiguration(true, 0, "popular");
            var resultConfig = new LiveToVodPartnerConfiguration(true, 0, "popular");
            _repositoryMock
                .Setup(x => x.GetPartnerConfiguration(1))
                .Returns(resultConfig);
            _repositoryMock
                .Setup(x => x.UpsertPartnerConfiguration(1, config, 2))
                .Returns(true);
            _layeredCacheMock
                .Setup(x => x.SetInvalidationKey(LayeredCacheKeys.GetLiveToVodFullConfigurationInvalidationKey(1), null))
                .Returns(true);
            var catalogGroupCache = new CatalogGroupCache();
            catalogGroupCache.AssetStructsMapById.Add(
                0,
                new AssetStruct
                {
                    Id = 0,
                    AssetStructMetas = new Dictionary<long, AssetStructMeta> { { 1, new AssetStructMeta() } },
                    IsProgramAssetStruct = true
                });
            catalogGroupCache.TopicsMapBySystemNameAndByType.Add(
                "popular",
                new Dictionary<string, Topic> { { MetaType.Bool.ToString(), new Topic { Id = 1 } } });
            _catalogManagerMock.Setup(x => x.TryGetCatalogGroupCacheFromCache(1, out catalogGroupCache))
                .Returns(true);
            var manager = new LiveToVodManager(_repositoryMock.Object, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _catalogManagerMock.Object, _loggerMock.Object);

            var result = manager.UpdatePartnerConfiguration(1, config, 2);

            result.Status.Should().Be(Status.Ok);
            result.Object.Should().Be(resultConfig);
        }

        [Test]
        public void UpdatePartnerConfiguration_ExistenceOfAssetStructShouldBeCheckedAndAssetStructExists_ReturnsExpectedResult()
        {
            var config = new LiveToVodPartnerConfiguration(true, 0, "popular");
            var resultConfig = new LiveToVodPartnerConfiguration();
            _repositoryMock
                .SetupSequence(x => x.GetPartnerConfiguration(1))
                .Returns(new LiveToVodPartnerConfiguration())
                .Returns(resultConfig);
            _repositoryMock
                .Setup(x => x.UpsertPartnerConfiguration(1, config, 2))
                .Returns(true);
            _layeredCacheMock
                .Setup(x => x.SetInvalidationKey(LayeredCacheKeys.GetLiveToVodFullConfigurationInvalidationKey(1), null))
                .Returns(true);
            _liveToVodServiceMock
                .Setup(x => x.GetLiveToVodAssetStruct(1))
                .Returns(new GenericResponse<AssetStruct>(Status.Ok));
            var catalogGroupCache = new CatalogGroupCache();
            catalogGroupCache.AssetStructsMapById.Add(
                0,
                new AssetStruct
                {
                    Id = 0,
                    AssetStructMetas = new Dictionary<long, AssetStructMeta> { { 1, new AssetStructMeta() } },
                    IsProgramAssetStruct = true
                });
            catalogGroupCache.TopicsMapBySystemNameAndByType.Add(
                "popular",
                new Dictionary<string, Topic> { { MetaType.Bool.ToString(), new Topic { Id = 1 } } });
            _catalogManagerMock.Setup(x => x.TryGetCatalogGroupCacheFromCache(1, out catalogGroupCache))
                .Returns(true);
            var manager = new LiveToVodManager(_repositoryMock.Object, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _catalogManagerMock.Object, _loggerMock.Object);

            var result = manager.UpdatePartnerConfiguration(1, config, 2);

            result.Status.Should().Be(Status.Ok);
            result.Object.Should().Be(resultConfig);
        }
        
        [Test]
        public void UpdatePartnerConfiguration_MetadataClassifierIsInvalid_ReturnsExpectedResult()
        {
            var config = new LiveToVodPartnerConfiguration(true, 0, "popular");
            _repositoryMock
                .SetupSequence(x => x.GetPartnerConfiguration(1))
                .Returns(new LiveToVodPartnerConfiguration());
            _liveToVodServiceMock
                .Setup(x => x.GetLiveToVodAssetStruct(1))
                .Returns(new GenericResponse<AssetStruct>(Status.Ok));
            var catalogGroupCache = new CatalogGroupCache();
            catalogGroupCache.AssetStructsMapById.Add(
                0,
                new AssetStruct
                {
                    Id = 0,
                    AssetStructMetas = new Dictionary<long, AssetStructMeta> { { 1, new AssetStructMeta() } },
                    IsProgramAssetStruct = true
                });
            _catalogManagerMock.Setup(x => x.TryGetCatalogGroupCacheFromCache(1, out catalogGroupCache))
                .Returns(true);
            var manager = new LiveToVodManager(_repositoryMock.Object, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _catalogManagerMock.Object, _loggerMock.Object);

            var result = manager.UpdatePartnerConfiguration(1, config, 2);

            result.Status.Should().Be(new Status((eResponseStatus.L2VMetadataClassifierIsNotValid)));
            result.Object.Should().BeNull();
        }

        [Test]
        public void UpdatePartnerConfiguration_ExistenceOfAssetStructShouldBeCheckedAndGetAssetStructFailed_ReturnsExpectedResult()
        {
            var resultConfig = new LiveToVodPartnerConfiguration();
            _repositoryMock
                .SetupSequence(x => x.GetPartnerConfiguration(1))
                .Returns(new LiveToVodPartnerConfiguration())
                .Returns(resultConfig);
            _liveToVodServiceMock
                .Setup(x => x.GetLiveToVodAssetStruct(1))
                .Returns(new GenericResponse<AssetStruct>(Status.Error));
            var manager = new LiveToVodManager(_repositoryMock.Object, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _catalogManagerMock.Object, _loggerMock.Object);

            var result = manager.UpdatePartnerConfiguration(1, new LiveToVodPartnerConfiguration(true, 0, null), 2);

            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [Test]
        public void UpdatePartnerConfiguration_ExistenceOfAssetStructShouldBeCheckedAndAssetStructCreatedSuccessfullyAndUpdateSuccessfully_ReturnsExpectedResult()
        {
            var config = new LiveToVodPartnerConfiguration(true, 0, "popular");
            var resultConfig = new LiveToVodPartnerConfiguration(true, 0, "popular");
            _repositoryMock
                .SetupSequence(x => x.GetPartnerConfiguration(1))
                .Returns(new LiveToVodPartnerConfiguration())
                .Returns(resultConfig);
            _repositoryMock
                .Setup(x => x.UpsertPartnerConfiguration(1, config, 2))
                .Returns(true);
            _layeredCacheMock
                .Setup(x => x.SetInvalidationKey(LayeredCacheKeys.GetLiveToVodFullConfigurationInvalidationKey(1), null))
                .Returns(true);
            var catalogGroupCache = new CatalogGroupCache();
            catalogGroupCache.AssetStructsMapById.Add(
                0,
                new AssetStruct
                {
                    Id = 0,
                    AssetStructMetas = new Dictionary<long, AssetStructMeta> { { 1, new AssetStructMeta() } },
                    IsProgramAssetStruct = true
                });
            catalogGroupCache.TopicsMapBySystemNameAndByType.Add(
                "popular",
                new Dictionary<string, Topic> { { MetaType.Bool.ToString(), new Topic { Id = 1 } } });
            _catalogManagerMock.Setup(x => x.TryGetCatalogGroupCacheFromCache(1, out catalogGroupCache))
                .Returns(true);
            _liveToVodServiceMock
                .Setup(x => x.GetLiveToVodAssetStruct(1))
                .Returns(new GenericResponse<AssetStruct>(new Status(eResponseStatus.AssetStructDoesNotExist)));
            _liveToVodServiceMock
                .Setup(x => x.AddLiveToVodAssetStruct(1, 2))
                .Returns(new GenericResponse<AssetStruct>(Status.Ok));
            var manager = new LiveToVodManager(_repositoryMock.Object, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _catalogManagerMock.Object, _loggerMock.Object);

            var result = manager.UpdatePartnerConfiguration(1, config, 2);

            result.Status.Should().Be(Status.Ok);
            result.Object.Should().Be(resultConfig);
        }

        [Test]
        public void UpdatePartnerConfiguration_ExistenceOfAssetStructShouldBeCheckedAndAssetStructCreationFailed_ReturnsExpectedResult()
        {
            var resultConfig = new LiveToVodPartnerConfiguration();
            _repositoryMock
                .SetupSequence(x => x.GetPartnerConfiguration(1))
                .Returns(resultConfig)
                .Returns(resultConfig);
            _liveToVodServiceMock
                .Setup(x => x.GetLiveToVodAssetStruct(1))
                .Returns(new GenericResponse<AssetStruct>(new Status(eResponseStatus.AssetStructDoesNotExist)));
            _liveToVodServiceMock
                .Setup(x => x.AddLiveToVodAssetStruct(1, 2))
                .Returns(new GenericResponse<AssetStruct>(eResponseStatus.Error, "Custom Message"));
            _loggerMock
                .Setup(LogLevel.Error, "Failed to create L2V asset struct. 1 - Custom Message.");
            var manager = new LiveToVodManager(_repositoryMock.Object, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _catalogManagerMock.Object, _loggerMock.Object);
            var result = manager.UpdatePartnerConfiguration(1, new LiveToVodPartnerConfiguration(true, 0, null), 2);
            
            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [Test]
        public void UpdatePartnerConfiguration_UpdateFailed_ReturnsExpectedResult()
        {
            var config = new LiveToVodPartnerConfiguration(false, 0, "popular");
            var resultConfig = new LiveToVodPartnerConfiguration();
            _repositoryMock
                .Setup(x => x.UpsertPartnerConfiguration(1, config, 2))
                .Returns(false);
            _repositoryMock
                .Setup(x => x.GetPartnerConfiguration(1))
                .Returns(resultConfig);
            _loggerMock
                .Setup(LogLevel.Error, "UpdatePartnerConfiguration failed. partnerId=1, updaterId=2.");
            var catalogGroupCache = new CatalogGroupCache();
            catalogGroupCache.AssetStructsMapById.Add(
                0,
                new AssetStruct
                {
                    Id = 0,
                    AssetStructMetas = new Dictionary<long, AssetStructMeta> { { 1, new AssetStructMeta() } },
                    IsProgramAssetStruct = true
                });
            catalogGroupCache.TopicsMapBySystemNameAndByType.Add(
                "popular",
                new Dictionary<string, Topic> { { MetaType.Bool.ToString(), new Topic { Id = 1 } } });
            _catalogManagerMock.Setup(x => x.TryGetCatalogGroupCacheFromCache(1, out catalogGroupCache))
                .Returns(true);
            var manager = new LiveToVodManager(_repositoryMock.Object, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _catalogManagerMock.Object, _loggerMock.Object);

            var result = manager.UpdatePartnerConfiguration(1, config, 2);

            result.Object.Should().Be(resultConfig);
            result.Status.Should().Be(Status.Ok);
        }

        [Test]
        public void UpdateLinearAssetConfiguration_UpdatedSuccessfully_ReturnsExpectedResult()
        {
            var config = new LiveToVodLinearAssetConfiguration(11, false);
            _repositoryMock
                .Setup(x => x.GetPartnerConfiguration(1))
                .Returns(new LiveToVodPartnerConfiguration(true, 10, null));
            _repositoryMock
                .Setup(x => x.UpsertLinearAssetConfiguration(1, config, 2))
                .Returns(true);
            _layeredCacheMock
                .Setup(x => x.SetInvalidationKey(LayeredCacheKeys.GetLiveToVodFullConfigurationInvalidationKey(1), null))
                .Returns(true);
            _repositoryMock
                .Setup(x => x.GetLinearAssetConfiguration(1, 11))
                .Returns(new LiveToVodLinearAssetConfiguration(11, false, 20));
            var manager = new LiveToVodManager(_repositoryMock.Object, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _catalogManagerMock.Object, _loggerMock.Object);

            var result = manager.UpdateLinearAssetConfiguration(1, config, 2);

            result.Should().NotBeNull();
            result.LinearAssetId.Should().Be(11);
            result.IsLiveToVodEnabled.Should().BeFalse();
            result.RetentionPeriodDays.Should().Be(20);
        }

        [Test]
        public void UpdateLinearAssetConfiguration_UpdateFailed_ReturnsExpectedResult()
        {
            var config = new LiveToVodLinearAssetConfiguration(11, false);
            _repositoryMock
                .Setup(x => x.GetPartnerConfiguration(1))
                .Returns(new LiveToVodPartnerConfiguration(true, 10, null));
            _repositoryMock
                .Setup(x => x.UpsertLinearAssetConfiguration(1, config, 2))
                .Returns(false);
            _repositoryMock
                .Setup(x => x.GetLinearAssetConfiguration(1, 11))
                .Returns(new LiveToVodLinearAssetConfiguration(11, false, 20));
            _loggerMock
                .Setup(LogLevel.Error, "UpdateLinearAssetConfiguration failed. partnerId=1, LinearAssetId=11, updaterId=2.");
            var manager = new LiveToVodManager(_repositoryMock.Object, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _catalogManagerMock.Object, _loggerMock.Object);

            var result = manager.UpdateLinearAssetConfiguration(1, config, 2);

            result.Should().NotBeNull();
            result.LinearAssetId.Should().Be(11);
            result.IsLiveToVodEnabled.Should().BeFalse();
            result.RetentionPeriodDays.Should().Be(20);
        }

        [Test]
        public void UpdateLinearAssetConfiguration_PartnerLiveToVodIsDisabled_ReturnsExpectedResult()
        {
            var config = new LiveToVodLinearAssetConfiguration(11, false);
            _repositoryMock
                .Setup(x => x.GetPartnerConfiguration(1))
                .Returns(new LiveToVodPartnerConfiguration(false, 10, null));
            _repositoryMock
                .Setup(x => x.GetLinearAssetConfiguration(1, 11))
                .Returns(new LiveToVodLinearAssetConfiguration(11, true, 20));
            _loggerMock
                .Setup(LogLevel.Warning, "Update of LiveToVodLinearAssetConfiguration was skipped because LiveToVod is disabled on partner's level. partnerId=1.");
            var manager = new LiveToVodManager(_repositoryMock.Object, _liveToVodServiceMock.Object, _layeredCacheMock.Object, _catalogManagerMock.Object, _loggerMock.Object);

            var result = manager.UpdateLinearAssetConfiguration(1, config, 2);

            result.Should().NotBeNull();
            result.LinearAssetId.Should().Be(11);
            result.IsLiveToVodEnabled.Should().BeFalse();
            result.RetentionPeriodDays.Should().Be(20);
        }
    }
}