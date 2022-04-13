using FluentAssertions;
using NUnit.Framework;
using WebAPI.Exceptions;
using WebAPI.Models.Domains;
using WebAPI.Validation;

namespace WebAPI.Tests.Validation
{
    [TestFixture]
    public class DeviceBrandValidatorTests
    {
        [Test]
        public void Instance_ValidParameters_ReturnsInitializedObject()
        {
            DeviceBrandValidator.Instance.Should().NotBeNull();
        }

        [TestCase(10000, 1, 1)]
        [TestCase(10000, 10000, 1)]
        [TestCase(10999, 9999, 50)]
        [TestCase(10999, 10049, 50)]
        public void ValidateToAdd_ValidParameters_NoExceptionIsThrown(long brandId, long familyId, int nameLength)
        {
            var name = new string('A', nameLength);
            var deviceBrand = new KalturaDeviceBrand
            {
                Id = brandId,
                DeviceFamilyId = familyId,
                Name = name
            };
            var validator = new DeviceBrandValidator();

            validator.ValidateToAdd(1, deviceBrand);
        }

        [Test]
        public void ValidateToAdd_IdIsNull_ExceptionIsThrown()
        {
            var deviceBrand = new KalturaDeviceBrand
            {
                DeviceFamilyId = 1,
                Name = "DeviceBrandName"
            };
            var validator = new DeviceBrandValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(1, deviceBrand));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY.statusCode);
            exception.Message.Should().Be("Argument [Id] cannot be empty");
        }

        [TestCase(20000)]
        [TestCase(11000)]
        public void ValidateToAdd_IdIsOutOfRange_ExceptionIsThrown(long id)
        {
            var deviceBrand = new KalturaDeviceBrand
            {
                Id = id,
                DeviceFamilyId = 1,
                Name = "DeviceBrandName"
            };
            var validator = new DeviceBrandValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(1, deviceBrand));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_NOT_IN_PREDEFINED_RANGE.statusCode);
            exception.Message.Should().Be("Argument [Id] not in predefined range [10000,10999]");
        }

        [Test]
        public void ValidateToAdd_DeviceFamilyIdIsNull_ExceptionIsThrown()
        {
            var deviceBrand = new KalturaDeviceBrand
            {
                Id = 10000,
                Name = "DeviceBrandName"
            };
            var validator = new DeviceBrandValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(1, deviceBrand));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY.statusCode);
            exception.Message.Should().Be("Argument [DeviceFamilyId] cannot be empty");
        }

        [TestCase(20000)]
        [TestCase(10050)]
        public void ValidateToAdd_DeviceFamilyIdIsOutOfRange_ExceptionIsThrown(long deviceFamilyId)
        {
            var deviceBrand = new KalturaDeviceBrand
            {
                Id = 10000,
                DeviceFamilyId = deviceFamilyId,
                Name = "DeviceBrandName"
            };
            var validator = new DeviceBrandValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(1, deviceBrand));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_NOT_IN_PREDEFINED_RANGE.statusCode);
            exception.Message.Should().Be("Argument [DeviceFamilyId] not in predefined range [1,9999] or [10000,10049]");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ValidateToAdd_NameIsEmpty_ExceptionIsThrown(string name)
        {
            var deviceBrand = new KalturaDeviceBrand
            {
                Id = 10000,
                DeviceFamilyId = 1,
                Name = name
            };
            var validator = new DeviceBrandValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(1, deviceBrand));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY.statusCode);
            exception.Message.Should().Be("Argument [Name] cannot be empty");
        }

        [Test]
        public void ValidateToAdd_NameIsTooLong_ExceptionIsThrown()
        {
            var deviceBrand = new KalturaDeviceBrand
            {
                Id = 10000,
                DeviceFamilyId = 1,
                Name = new string('A', 51)
            };
            var validator = new DeviceBrandValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(1, deviceBrand));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED.statusCode);
            exception.Message.Should().Be("Argument [Name] maximum length is [50]");
        }

        [TestCase(10000, 1, 1)]
        [TestCase(10000, 10000, 1)]
        [TestCase(10999, 9999, 50)]
        [TestCase(10999, 10049, 50)]
        [TestCase(10000, null, 1)]
        [TestCase(10000, 1, null)]
        public void ValidateToUpdate_ValidParameters_NoExceptionIsThrown(long brandId, long? familyId, int? nameLength)
        {
            var name = nameLength.HasValue
                ? new string('A', nameLength.Value)
                : null;
            var deviceBrand = new KalturaDeviceBrand
            {
                Id = brandId,
                DeviceFamilyId = familyId,
                Name = name
            };
            var validator = new DeviceBrandValidator();

            validator.ValidateToUpdate(1, deviceBrand);
        }

        [Test]
        public void ValidateToUpdate_IdIsNull_ExceptionIsThrown()
        {
            var deviceBrand = new KalturaDeviceBrand
            {
                DeviceFamilyId = 1,
                Name = "DeviceBrandName"
            };
            var validator = new DeviceBrandValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToUpdate(1, deviceBrand));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY.statusCode);
            exception.Message.Should().Be("Argument [Id] cannot be empty");
        }

        [TestCase(20000)]
        [TestCase(11000)]
        public void ValidateToUpdate_IdIsOutOfRange_ExceptionIsThrown(long id)
        {
            var deviceBrand = new KalturaDeviceBrand
            {
                Id = id,
                DeviceFamilyId = 1,
                Name = "DeviceBrandName"
            };
            var validator = new DeviceBrandValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToUpdate(1, deviceBrand));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_NOT_IN_PREDEFINED_RANGE.statusCode);
            exception.Message.Should().Be("Argument [Id] not in predefined range [10000,10999]");
        }

        [TestCase(20000)]
        [TestCase(10050)]
        public void ValidateToUpdate_DeviceFamilyIdIsOutOfRange_ExceptionIsThrown(long deviceFamilyId)
        {
            var deviceBrand = new KalturaDeviceBrand
            {
                Id = 10000,
                DeviceFamilyId = deviceFamilyId,
                Name = "DeviceBrandName"
            };
            var validator = new DeviceBrandValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToUpdate(1, deviceBrand));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_NOT_IN_PREDEFINED_RANGE.statusCode);
            exception.Message.Should().Be("Argument [DeviceFamilyId] not in predefined range [1,9999] or [10000,10049]");
        }

        [TestCase("")]
        [TestCase(" ")]
        public void ValidateToUpdate_NameIsEmpty_ExceptionIsThrown(string name)
        {
            var deviceBrand = new KalturaDeviceBrand
            {
                Id = 10000,
                DeviceFamilyId = 1,
                Name = name
            };
            var validator = new DeviceBrandValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToUpdate(1, deviceBrand));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY.statusCode);
            exception.Message.Should().Be("Argument [Name] cannot be empty");
        }

        [Test]
        public void ValidateToUpdate_NameIsTooLong_ExceptionIsThrown()
        {
            var deviceBrand = new KalturaDeviceBrand
            {
                Id = 10000,
                DeviceFamilyId = 1,
                Name = new string('A', 51)
            };
            var validator = new DeviceBrandValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToUpdate(1, deviceBrand));

            exception.Code.Should().Be(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED.statusCode);
            exception.Message.Should().Be("Argument [Name] maximum length is [50]");
        }
    }
}