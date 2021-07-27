using FluentAssertions;
using NUnit.Framework;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.Tests.Models.Catalog
{
    [TestFixture]
    public class KalturaLabelFilterValidatorTests
    {
        [Test]
        public void Validate_Null_ThrowsException()
        {
            var validator = new KalturaLabelFilterValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.Validate(null, "filter"));

            exception.Message.Should().Be("Argument [filter] is invalid");
        }

        [TestCase("1,2,3", "", "")]
        [TestCase("", "label", "")]
        [TestCase("", "", "labelStart")]
        public void Validate_ValidFilter_ReturnsExpectedResult(string idIn, string labelEqual, string labelStartsWith)
        {
            var filter = new KalturaLabelFilter
            {
                IdIn = idIn,
                LabelEqual = labelEqual,
                LabelStartsWith = labelStartsWith,
                EntityAttributeEqual = KalturaEntityAttribute.MEDIA_FILE_LABELS
            };
            var validator = new KalturaLabelFilterValidator();

            validator.Validate(filter, "filter");
        }

        [TestCase("1,2,3", "label", "", "Only one of filter.idIn or filter.labelEqual can be used, not both of them")]
        [TestCase("1,2,3", "", "labelStart", "Only one of filter.idIn or filter.labelStartsWith can be used, not both of them")]
        [TestCase("", "label", "labelStart", "Only one of filter.labelEqual or filter.labelStartsWith can be used, not both of them")]
        [TestCase("1,2,3", "label", "labelStart", "Only one of filter.idIn or filter.labelEqual can be used, not both of them")]
        public void Validate_InvalidFilter_ThrowsException(string idIn, string labelEqual, string labelStartsWith, string exceptionMessage)
        {
            var filter = new KalturaLabelFilter
            {
                IdIn = idIn,
                LabelEqual = labelEqual,
                LabelStartsWith = labelStartsWith,
                EntityAttributeEqual = KalturaEntityAttribute.MEDIA_FILE_LABELS
            };
            var validator = new KalturaLabelFilterValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.Validate(filter, "filter"));

            exception.Message.Should().Be(exceptionMessage);
        }

        [Test]
        public void Validate_InvalidEntityAttributeEqual()
        {
            var filter = new KalturaLabelFilter
            {
                IdIn = "1,2,3",
                EntityAttributeEqual = (KalturaEntityAttribute)2
            };
            var validator = new KalturaLabelFilterValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.Validate(filter, "filter"));

            exception.Message.Should().Be("Enumerator value [2] is not supported for argument [filter.entityAttributeEqual]");
        }
    }
}
