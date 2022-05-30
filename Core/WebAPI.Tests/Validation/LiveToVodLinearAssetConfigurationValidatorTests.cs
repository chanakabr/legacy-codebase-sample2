using System.Collections.Generic;
using System.Linq;
using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using Core.Catalog;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WebAPI.Exceptions;
using WebAPI.Models.LiveToVod;
using WebAPI.Validation;

namespace WebAPI.Tests.Validation
{
    [TestFixture]
    public class LiveToVodLinearAssetConfigurationValidatorTests
    {
        private MockRepository _mockRepository;
        private Mock<IAssetManager> _assetManagerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _assetManagerMock = _mockRepository.Create<IAssetManager>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void Instance_ValidParameters_ReturnsInitializedObject()
        {
            LiveToVodLinearAssetConfigurationValidator.Instance.Should().NotBeNull();
        }

        [Test]
        public void Validate_LinearAssetIdIsNull_ExceptionIsThrown()
        {
            var configuration = new KalturaLiveToVodLinearAssetConfiguration
            {
                IsLiveToVodEnabled = false
            };
            var validator = new LiveToVodLinearAssetConfigurationValidator(_assetManagerMock.Object);

            var exception = Assert.Throws<BadRequestException>(() => validator.Validate(1, configuration, "configuration"));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY.statusCode);
            exception.Message.Should().Be("Argument [configuration.linearAssetId] cannot be empty");
        }

        [Test]
        public void Validate_LinearAssetIdNotExists_ExceptionIsThrown()
        {
            var configuration = new KalturaLiveToVodLinearAssetConfiguration
            {
                LinearAssetId = 2,
                IsLiveToVodEnabled = false
            };
            _assetManagerMock
                .Setup(x => x.GetAssets(1, It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[] { new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 2) })), false))
                .Returns(new List<Asset>());
            var validator = new LiveToVodLinearAssetConfigurationValidator(_assetManagerMock.Object);

            var exception = Assert.Throws<ClientException>(() => validator.Validate(1, configuration, "configuration"));

            exception.Code.Should().Be((int)eResponseStatus.AssetDoesNotExist);
            exception.ExceptionMessage.Should().Be(eResponseStatus.AssetDoesNotExist.ToString());
        }

        [Test]
        public void Validate_IsLiveToVodEnabledIsNull_ExceptionIsThrown()
        {
            var configuration = new KalturaLiveToVodLinearAssetConfiguration
            {
                LinearAssetId = 2
            };
            _assetManagerMock
                .Setup(x => x.GetAssets(1, It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[] { new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 2) })), false))
                .Returns(new List<Asset> { new LiveAsset() });
            var validator = new LiveToVodLinearAssetConfigurationValidator(_assetManagerMock.Object);

            var exception = Assert.Throws<BadRequestException>(() => validator.Validate(1, configuration, "configuration"));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY.statusCode);
            exception.Message.Should().Be("Argument [configuration.isL2vEnabled] cannot be empty");
        }

        [TestCase(null)]
        [TestCase(1)]
        [TestCase(999999999)]
        public void Validate_RetentionPeriodDaysIsValid_NoExceptionIsThrown(int? retentionPeriodDays)
        {
            var configuration = new KalturaLiveToVodLinearAssetConfiguration
            {
                LinearAssetId = 2,
                IsLiveToVodEnabled = true,
                RetentionPeriodDays = retentionPeriodDays
            };
            _assetManagerMock
                .Setup(x => x.GetAssets(1, It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[] { new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 2) })), false))
                .Returns(new List<Asset> { new LiveAsset() });
            var validator = new LiveToVodLinearAssetConfigurationValidator(_assetManagerMock.Object);

            validator.Validate(1, configuration, "configuration");
        }

        [Test]
        public void Validate_RetentionPeriodDaysIsLessThanMinValue_ExceptionIsThrown()
        {
            var configuration = new KalturaLiveToVodLinearAssetConfiguration
            {
                LinearAssetId = 2,
                IsLiveToVodEnabled = true,
                RetentionPeriodDays = 0
            };
            _assetManagerMock
                .Setup(x => x.GetAssets(1, It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[] { new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 2) })), false))
                .Returns(new List<Asset> { new LiveAsset() });
            var validator = new LiveToVodLinearAssetConfigurationValidator(_assetManagerMock.Object);

            var exception = Assert.Throws<BadRequestException>(() => validator.Validate(1, configuration, "configuration"));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED.statusCode);
            exception.Message.Should().Be("Argument [configuration.retentionPeriodDays] minimum value is [1]");
        }

        [Test]
        public void Validate_RetentionPeriodDaysIsGreaterThanMaxValue_ExceptionIsThrown()
        {
            var configuration = new KalturaLiveToVodLinearAssetConfiguration
            {
                LinearAssetId = 2,
                IsLiveToVodEnabled = true,
                RetentionPeriodDays = 1000000000
            };
            _assetManagerMock
                .Setup(x => x.GetAssets(1, It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[] { new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 2) })), false))
                .Returns(new List<Asset> { new LiveAsset() });
            var validator = new LiveToVodLinearAssetConfigurationValidator(_assetManagerMock.Object);

            var exception = Assert.Throws<BadRequestException>(() => validator.Validate(1, configuration, "configuration"));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_MAX_VALUE_CROSSED.statusCode);
            exception.Message.Should().Be("Argument [configuration.retentionPeriodDays] maximum value is [999999999]");
        }

        [Test]
        public void ValidateLinearAssetId_LinearAssetIdNotExists_ExceptionIsThrown()
        {
            _assetManagerMock
                .Setup(x => x.GetAssets(1, It.Is<IEnumerable<KeyValuePair<eAssetTypes, long>>>(_ => _.SequenceEqual(new[] { new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, 2) })), false))
                .Returns(new List<Asset>());
            var validator = new LiveToVodLinearAssetConfigurationValidator(_assetManagerMock.Object);

            var exception = Assert.Throws<ClientException>(() => validator.ValidateLinearAssetId(1, 2, "configuration"));

            exception.Code.Should().Be((int)eResponseStatus.AssetDoesNotExist);
            exception.ExceptionMessage.Should().Be(eResponseStatus.AssetDoesNotExist.ToString());
        }
    }
}