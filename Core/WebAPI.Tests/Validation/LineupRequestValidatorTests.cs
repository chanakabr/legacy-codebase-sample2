using FluentAssertions;
using NUnit.Framework;
using WebAPI.Validation;

namespace WebAPI.Tests.Validation
{
    [TestFixture]
    public class LineupRequestValidatorTests
    {
        [Test]
        public void LineupRequestValidator_MinPageIndex_ReturnsExpectedResult()
        {
            var validator = new LineupRequestValidator();

            var result = validator.MinPageIndex;

            result.Should().Be(1);
        }

        [Test]
        public void LineupRequestValidator_DefaultPageSize_ReturnsExpectedResult()
        {
            var validator = new LineupRequestValidator();

            var result = validator.DefaultPageSize;

            result.Should().Be(500);
        }

        [Test]
        public void LineupRequestValidator_AllowedPageSizes_ContainsExpectedItems()
        {
            var validator = new LineupRequestValidator();

            var result = validator.AllowedPageSizes;

            result.Should().BeEquivalentTo(new[] { 100, 200, 800, 1200, 1600 });
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void LineupRequestValidator_InvalidPageIndex_ReturnsFalse(int pageIndex)
        {
            var validator = new LineupRequestValidator();

            var result = validator.ValidatePageIndex(pageIndex);

            result.Should().BeFalse();
        }

        [TestCase(1)]
        [TestCase(2)]
        public void LineupRequestValidator_ValidPageIndex_ReturnsTrue(int pageIndex)
        {
            var validator = new LineupRequestValidator();

            var result = validator.ValidatePageIndex(pageIndex);

            result.Should().BeTrue();
        }

        [Test]
        public void LineupRequestValidator_InvalidPageSize_ReturnsFalse()
        {
            var validator = new LineupRequestValidator();

            var result = validator.ValidatePageSize(10);

            result.Should().BeFalse();
        }

        [Test]
        public void LineupRequestValidator_ExplicitDefaultPageSize_ReturnsFalse()
        {
            var validator = new LineupRequestValidator();

            var result = validator.ValidatePageSize(500);

            result.Should().BeFalse();
        }

        [Test]
        public void LineupRequestValidator_ValidPageSize_ReturnsTrue()
        {
            var validator = new LineupRequestValidator();

            var result = validator.ValidatePageSize(100);

            result.Should().BeTrue();
        }
    }
}