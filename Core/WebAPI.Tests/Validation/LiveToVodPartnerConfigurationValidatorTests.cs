using FluentAssertions;
using NUnit.Framework;
using WebAPI.Exceptions;
using WebAPI.Models.LiveToVod;
using WebAPI.Validation;

namespace WebAPI.Tests.Validation
{
    [TestFixture]
    public class LiveToVodPartnerConfigurationValidatorTests
    {
        [Test]
        public void Instance_ValidParameters_ReturnsInitializedObject()
        {
            LiveToVodPartnerConfigurationValidator.Instance.Should().NotBeNull();
        }

        [Test]
        public void IsLiveToVodEnabled_IsNull_ExceptionIsThrown()
        {
            var configuration = new KalturaLiveToVodPartnerConfiguration
            {
                RetentionPeriodDays = 1,
                MetadataClassifier = "metadataClassifier"
            };
            var validator = new LiveToVodPartnerConfigurationValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.Validate(configuration, "configuration"));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY.statusCode);
            exception.Message.Should().Be("Argument [configuration.isL2vEnabled] cannot be empty");
        }

        [Test]
        public void RetentionPeriodDays_IsNull_ExceptionIsThrown()
        {
            var configuration = new KalturaLiveToVodPartnerConfiguration
            {
                IsLiveToVodEnabled = false,
                MetadataClassifier = "metadataClassifier"
            };
            var validator = new LiveToVodPartnerConfigurationValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.Validate(configuration, "configuration"));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY.statusCode);
            exception.Message.Should().Be("Argument [configuration.retentionPeriodDays] cannot be empty");
        }

        [Test]
        public void RetentionPeriodDays_IsLessThanMinValue_ExceptionIsThrown()
        {
            var configuration = new KalturaLiveToVodPartnerConfiguration
            {
                IsLiveToVodEnabled = false,
                RetentionPeriodDays = 0,
                MetadataClassifier = "metadataClassifier"
            };
            var validator = new LiveToVodPartnerConfigurationValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.Validate(configuration, "configuration"));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED.statusCode);
            exception.Message.Should().Be("Argument [configuration.retentionPeriodDays] minimum value is [1]");
        }

        [Test]
        public void RetentionPeriodDays_IsGreaterThanMaxValue_ExceptionIsThrown()
        {
            var configuration = new KalturaLiveToVodPartnerConfiguration
            {
                IsLiveToVodEnabled = false,
                RetentionPeriodDays = 1000000000,
                MetadataClassifier = "metadataClassifier"
            };
            var validator = new LiveToVodPartnerConfigurationValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.Validate(configuration, "configuration"));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_MAX_VALUE_CROSSED.statusCode);
            exception.Message.Should().Be("Argument [configuration.retentionPeriodDays] maximum value is [999999999]");
        }

        [TestCase(1)]
        [TestCase(999999999)]
        public void RetentionPeriodDays_IsInValidRange_NoExceptionIsThrown(int retentionPeriodDays)
        {
            var configuration = new KalturaLiveToVodPartnerConfiguration
            {
                IsLiveToVodEnabled = false,
                RetentionPeriodDays = retentionPeriodDays,
                MetadataClassifier = "metadataClassifier"
            };
            var validator = new LiveToVodPartnerConfigurationValidator();

            validator.Validate(configuration, "configuration");
        }

        [TestCase(null)]
        [TestCase("")]
        public void MetadataClassifier_IsEmpty_ExceptionIsThrown(string metadataClassifier)
        {
            var configuration = new KalturaLiveToVodPartnerConfiguration
            {
                IsLiveToVodEnabled = false,
                RetentionPeriodDays = 1,
                MetadataClassifier = metadataClassifier
            };
            var validator = new LiveToVodPartnerConfigurationValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.Validate(configuration, "configuration"));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY.statusCode);
            exception.Message.Should().Be("Argument [configuration.metadataClassifier] cannot be empty");
        }

        [Test]
        public void MetadataClassifier_TooLong_ExceptionIsThrown()
        {
            var configuration = new KalturaLiveToVodPartnerConfiguration
            {
                IsLiveToVodEnabled = false,
                RetentionPeriodDays = 1,
                MetadataClassifier = new string('A', 51)
            };
            var validator = new LiveToVodPartnerConfigurationValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.Validate(configuration, "configuration"));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED.statusCode);
            exception.Message.Should().Be("Argument [configuration.metadataClassifier] maximum length is [50]");
        }

        [TestCase(1)]
        [TestCase(50)]
        public void MetadataClassifier_IsValid_NoExceptionIsThrown(int metadataClassifierLength)
        {
            var configuration = new KalturaLiveToVodPartnerConfiguration
            {
                IsLiveToVodEnabled = false,
                RetentionPeriodDays = 1,
                MetadataClassifier = new string('A', metadataClassifierLength)
            };
            var validator = new LiveToVodPartnerConfigurationValidator();

            validator.Validate(configuration, "configuration");
        }
    }
}