using FluentAssertions;
using NUnit.Framework;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.Domains;
using WebAPI.Validation;

namespace WebAPI.Tests.Validation
{
    [TestFixture]
    public class DeviceFamilyFilterValidatorTests
    {
        [TestCase(null, null, null)]
        [TestCase(1, null, null)]
        [TestCase(null, "deviceFamilyName", null)]
        [TestCase(null, null, KalturaDeviceFamilyType.System)]
        public void Validate_ValidParameters_NoExceptionIsThrown(long? idEqual, string nameEqual, KalturaDeviceFamilyType? typeEqual)
        {
            var filter = new KalturaDeviceFamilyFilter
            {
                IdEqual = idEqual,
                NameEqual = nameEqual,
                TypeEqual = typeEqual
            };
            var validator = new DeviceFamilyFilterValidator();

            validator.Validate(filter, nameof(filter));
        }

        [TestCase(1, "deviceFamilyName", null)]
        [TestCase(1, null, KalturaDeviceFamilyType.System)]
        [TestCase(null, "deviceFamilyName", KalturaDeviceFamilyType.System)]
        [TestCase(1, "deviceFamilyName", KalturaDeviceFamilyType.System)]
        public void Validate_InvalidParameters_BadRequestExceptionIsThrown(long? idEqual, string nameEqual, KalturaDeviceFamilyType? typeEqual)
        {
            var filter = new KalturaDeviceFamilyFilter
            {
                IdEqual = idEqual,
                NameEqual = nameEqual,
                TypeEqual = typeEqual
            };
            var validator = new DeviceFamilyFilterValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.Validate(filter, nameof(filter)));

            exception.Code.Should().Be((int)StatusCode.InvalidArgument);
            exception.Message.Should().Be("Argument [filter] is invalid");
        }
    }
}