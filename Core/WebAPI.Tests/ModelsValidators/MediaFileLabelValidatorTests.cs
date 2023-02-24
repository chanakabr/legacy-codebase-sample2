using FluentAssertions;
using NUnit.Framework;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.ModelsValidators;

namespace WebAPI.Tests.ModelsValidators
{
    [TestFixture]
    public class MediaFileLabelValidatorTests
    {
        [TestCase(1)]
        [TestCase(128)]
        public void ValidateToAdd_ValidCommaSeparatedString_NoException(int valueLength)
        {
            var commaSeparatedLabelValues = $" {new string('x', valueLength)} ,l1,l2,l3,l4,l5,l6,l7,l8,l9,l10,l11,l12,l13,l14,l15,l16,l17,l18,l19,l20,l21,l22,l23,l24";
            var validator = new MediaFileLabelValidator();

            validator.ValidateToAdd(commaSeparatedLabelValues, KalturaEntityAttribute.MEDIA_FILE_LABELS, "label");
        }

        [TestCase(null)]
        [TestCase("")]
        public void ValidateToAdd_EmptyCommaSeparatedString_NoException(string commaSeparatedLabelValues)
        {
            var validator = new MediaFileLabelValidator();

            validator.ValidateToAdd(commaSeparatedLabelValues, KalturaEntityAttribute.MEDIA_FILE_LABELS, "label");
        }

        [TestCase(" ")]
        [TestCase(" ,l1")]
        [TestCase("l1,")]
        [TestCase("l1,,l3")]
        public void ValidateToAdd_CommaSeparatedStringWithEmptyValue_ThrowsException(string commaSeparatedLabelValues)
        {
            var validator = new MediaFileLabelValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(commaSeparatedLabelValues, KalturaEntityAttribute.MEDIA_FILE_LABELS, "label"));

            exception.Message.Should().Be("Argument [label.value] cannot be empty");
        }

        [Test]
        public void ValidateToAdd_CommaSeparatedStringWithTooLongValue_ThrowsException()
        {
            var commaSeparatedLabelValues = $"{new string('x', 129)},l1,l2";
            var validator = new MediaFileLabelValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(commaSeparatedLabelValues, KalturaEntityAttribute.MEDIA_FILE_LABELS, "label"));

            exception.Message.Should().Be("Argument [label.value] maximum length is [128]");
        }

        [Test]
        public void ValidateToAdd_CommaSeparatedStringWithInvalidEntityAttribute_ThrowsException()
        {
            var validator = new MediaFileLabelValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd("l1,l2,l3", (KalturaEntityAttribute)2, "label"));

            exception.Message.Should().Be("Enumerator value [2] is not supported for argument [label.entityAttribute]");
        }

        [Test]
        public void ValidateToAdd_CommaSeparatedStringWithTooManyLabelValues_ThrowsException()
        {
            var commaSeparatedLabelValues = "l0,l1,l2,l3,l4,l5,l6,l7,l8,l9,l10,l11,l12,l13,l14,l15,l16,l17,l18,l19,l20,l21,l22,l23,l24,l25";
            var validator = new MediaFileLabelValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(commaSeparatedLabelValues, KalturaEntityAttribute.MEDIA_FILE_LABELS, "label"));

            exception.Message.Should().Be("Argument [label] maximum items is [25]");
        }

        [Test]
        public void ValidateToAdd_CommaSeparatedStringWithDuplicatedLabelValues_ThrowsException()
        {
            var validator = new MediaFileLabelValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd("l1,l2,l1", KalturaEntityAttribute.MEDIA_FILE_LABELS, "label"));

            exception.Message.Should().Be("Argument [label] can not appear twice");
        }

        [TestCase(1)]
        [TestCase(128)]
        public void ValidateToAdd_ValidLabel_NoException(int valueLength)
        {
            var value = $" {new string('x', valueLength)} ";
            var label = new KalturaLabel { EntityAttribute = KalturaEntityAttribute.MEDIA_FILE_LABELS, Value = value };
            var validator = new MediaFileLabelValidator();

            validator.ValidateToAdd(label, "label");
        }

        [Test]
        public void ValidateToAdd_Null_ThrowsException()
        {
            var validator = new MediaFileLabelValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(null, "label"));

            exception.Message.Should().Be("Argument [label] is invalid");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ValidateToAdd_EmptyValue_ThrowsException(string value)
        {
            var label = new KalturaLabel { EntityAttribute = KalturaEntityAttribute.MEDIA_FILE_LABELS, Value = value };
            var validator = new MediaFileLabelValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(label, "label"));

            exception.Message.Should().Be("Argument [label.value] cannot be empty");
        }

        [Test]
        public void ValidateToAdd_ValueTooLong_ThrowsException()
        {
            var value = new string('x', 129);
            var label = new KalturaLabel { EntityAttribute = KalturaEntityAttribute.MEDIA_FILE_LABELS, Value = value };
            var validator = new MediaFileLabelValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(label, "label"));

            exception.Message.Should().Be("Argument [label.value] maximum length is [128]");
        }

        [Test]
        public void ValidateToAdd_ValueWithComma_ThrowsException()
        {
            var label = new KalturaLabel { EntityAttribute = KalturaEntityAttribute.MEDIA_FILE_LABELS, Value = "with,comma" };
            var validator = new MediaFileLabelValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(label, "label"));

            exception.Message.Should().Be("Argument [label.value] is invalid");
        }

        [Test]
        public void ValidateToAdd_EntityAttributeInvalid_ThrowsException()
        {
            var label = new KalturaLabel { EntityAttribute = (KalturaEntityAttribute)2, Value = "value" };
            var validator = new MediaFileLabelValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToAdd(label, "label"));

            exception.Message.Should().Be("Enumerator value [2] is not supported for argument [label.entityAttribute]");
        }

        [TestCase(1)]
        [TestCase(128)]
        public void ValidateToUpdate_ValidLabel_NoException(int valueLength)
        {
            var value = $" {new string('x', valueLength)} ";
            var label = new KalturaLabel { EntityAttribute = (KalturaEntityAttribute)2, Value = value };
            var validator = new MediaFileLabelValidator();

            validator.ValidateToUpdate(label, "label");
        }

        [Test]
        public void ValidateToUpdate_Null_ThrowsException()
        {
            var validator = new MediaFileLabelValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToUpdate(null, "label"));

            exception.Message.Should().Be("Argument [label] is invalid");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ValidateToUpdate_EmptyValue_ThrowsException(string value)
        {
            var label = new KalturaLabel { EntityAttribute = KalturaEntityAttribute.MEDIA_FILE_LABELS, Value = value };
            var validator = new MediaFileLabelValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToUpdate(label, "label"));

            exception.Message.Should().Be("Argument [label.value] cannot be empty");
        }

        [Test]
        public void ValidateToUpdate_ValueTooLong_ThrowsException()
        {
            var value = new string('x', 129);
            var label = new KalturaLabel { EntityAttribute = KalturaEntityAttribute.MEDIA_FILE_LABELS, Value = value };
            var validator = new MediaFileLabelValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToUpdate(label, "label"));

            exception.Message.Should().Be("Argument [label.value] maximum length is [128]");
        }

        [Test]
        public void ValidateToUpdate_ValueWithComma_ThrowsException()
        {
            var label = new KalturaLabel { EntityAttribute = KalturaEntityAttribute.MEDIA_FILE_LABELS, Value = "with,comma" };
            var validator = new MediaFileLabelValidator();

            var exception = Assert.Throws<BadRequestException>(() => validator.ValidateToUpdate(label, "label"));

            exception.Message.Should().Be("Argument [label.value] is invalid");
        }
    }
}