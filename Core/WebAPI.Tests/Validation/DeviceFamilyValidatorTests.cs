using FluentAssertions;
using NUnit.Framework;
using WebAPI.Exceptions;
using WebAPI.Models.Domains;
using WebAPI.Validation;

namespace WebAPI.Tests.Validation
{
    [TestFixture]
    public class DeviceFamilyValidatorTests
    {
        [Test]
        public void Instance_ValidParameters_ReturnsInitializedObject()
        {
            DeviceFamilyValidator.Instance.Should().NotBeNull();
        }

        [TestCase(10000, 1)]
        [TestCase(10049, 50)]
        public void ValidateToAdd_ValidParameters_NoExceptionIsThrown(long id, int nameLength)
        {
            var name = new string('A', nameLength);
            var deviceFamily = new KalturaDeviceFamily
            {
                Id = id,
                Name = name
            };
            var validator = new DeviceFamilyValidator();

            validator.ValidateToAdd(1, deviceFamily);
        }

        [Test]
        public void ValidateToAdd_IdIsNull_ExceptionIsThrown()
        {
            var deviceFamily = new KalturaDeviceFamily
            {
                Name = "DeviceFamilyName"
            };
            var validator = new DeviceFamilyValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(1, deviceFamily));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY.statusCode);
            exception.Message.Should().Be("Argument [Id] cannot be empty");
        }

        [TestCase(20000)]
        [TestCase(10050)]
        public void ValidateToAdd_IdIsOutOfRange_ExceptionIsThrown(long id)
        {
            var deviceFamily = new KalturaDeviceFamily
            {
                Id = id,
                Name = "DeviceFamilyName"
            };
            var validator = new DeviceFamilyValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(1, deviceFamily));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_NOT_IN_PREDEFINED_RANGE.statusCode);
            exception.Message.Should().Be("Argument [Id] not in predefined range [10000,10049]");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ValidateToAdd_NameIsEmpty_ExceptionIsThrown(string name)
        {
            var deviceFamily = new KalturaDeviceFamily
            {
                Id = 10000,
                Name = name
            };
            var validator = new DeviceFamilyValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(1, deviceFamily));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY.statusCode);
            exception.Message.Should().Be("Argument [Name] cannot be empty");
        }

        [Test]
        public void ValidateToAdd_NameIsTooLong_ExceptionIsThrown()
        {
            var deviceFamily = new KalturaDeviceFamily
            {
                Id = 10000,
                Name = new string('A', 51)
            };
            var validator = new DeviceFamilyValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(1, deviceFamily));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED.statusCode);
            exception.Message.Should().Be("Argument [Name] maximum length is [50]");
        }

        [TestCase(10000, 1)]
        [TestCase(10049, 50)]
        [TestCase(10049, null)]
        public void ValidateToUpdate_ValidParameters_NoExceptionIsThrown(long id, int? nameLength)
        {
            var name = nameLength.HasValue
                ? new string('A', nameLength.Value)
                : null;
            var deviceFamily = new KalturaDeviceFamily
            {
                Id = id,
                Name = name
            };
            var validator = new DeviceFamilyValidator();

            validator.ValidateToUpdate(1, deviceFamily);
        }

        [Test]
        public void ValidateToUpdate_IdIsNull_ExceptionIsThrown()
        {
            var deviceFamily = new KalturaDeviceFamily
            {
                Name = "DeviceFamilyName"
            };
            var validator = new DeviceFamilyValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToUpdate(1, deviceFamily));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY.statusCode);
            exception.Message.Should().Be("Argument [Id] cannot be empty");
        }

        [TestCase(20000)]
        [TestCase(10050)]
        public void ValidateToUpdate_IdIsOutOfRange_ExceptionIsThrown(long id)
        {
            var deviceFamily = new KalturaDeviceFamily
            {
                Id = id,
                Name = "DeviceFamilyName"
            };
            var validator = new DeviceFamilyValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToUpdate(1, deviceFamily));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_NOT_IN_PREDEFINED_RANGE.statusCode);
            exception.Message.Should().Be("Argument [Id] not in predefined range [10000,10049]");
        }

        [TestCase("")]
        [TestCase(" ")]
        public void ValidateToUpdate_NameIsEmpty_ExceptionIsThrown(string name)
        {
            var deviceFamily = new KalturaDeviceFamily
            {
                Id = 10000,
                Name = name
            };
            var validator = new DeviceFamilyValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToUpdate(1, deviceFamily));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY.statusCode);
            exception.Message.Should().Be("Argument [Name] cannot be empty");
        }

        [Test]
        public void ValidateToUpdate_NameIsTooLong_ExceptionIsThrown()
        {
            var deviceFamily = new KalturaDeviceFamily
            {
                Id = 10000,
                Name = new string('A', 51)
            };
            var validator = new DeviceFamilyValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToUpdate(1, deviceFamily));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED.statusCode);
            exception.Message.Should().Be("Argument [Name] maximum length is [50]");
        }
    }
}