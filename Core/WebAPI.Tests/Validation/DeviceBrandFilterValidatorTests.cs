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
    public class DeviceBrandFilterValidatorTests
    {
        [TestCase(null, null, null, null)]
        [TestCase(1, null, null, null)]
        [TestCase(null, "deviceBrandName", null, null)]
        [TestCase(null, null, 2, null)]
        [TestCase(null, null, null, KalturaDeviceBrandType.System)]
        public void Validate_ValidParameters_NoExceptionIsThrown(long? idEqual, string nameEqual, long? deviceFamilyIdEqual, KalturaDeviceBrandType? typeEqual)
        {
            var filter = new KalturaDeviceBrandFilter
            {
                IdEqual = idEqual,
                NameEqual = nameEqual,
                DeviceFamilyIdEqual = deviceFamilyIdEqual,
                TypeEqual = typeEqual
            };
            var validator = new DeviceBrandFilterValidator();

            validator.Validate(filter, nameof(filter));
        }

        [TestCase(1, "deviceBrandName", null, null)]
        [TestCase(1, null, 2, null)]
        [TestCase(1, null, null, KalturaDeviceBrandType.System)]
        [TestCase(null, "deviceBrandName", 2, null)]
        [TestCase(null, "deviceBrandName", null, KalturaDeviceBrandType.System)]
        [TestCase(null, null, 2, KalturaDeviceBrandType.System)]
        [TestCase(1, "deviceBrandName", 2, KalturaDeviceBrandType.System)]
        public void Validate_InvalidParameters_BadRequestExceptionIsThrown(long? idEqual, string nameEqual, long? deviceFamilyIdEqual, KalturaDeviceBrandType? typeEqual)
        {
            var filter = new KalturaDeviceBrandFilter
            {
                IdEqual = idEqual,
                NameEqual = nameEqual,
                DeviceFamilyIdEqual = deviceFamilyIdEqual,
                TypeEqual = typeEqual
            };
            var validator = new DeviceBrandFilterValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.Validate(filter, nameof(filter)));

            exception.Code.Should().Be((int)StatusCode.InvalidArgument);
            exception.Message.Should().Be("Argument [filter] is invalid");
        }
    }
}