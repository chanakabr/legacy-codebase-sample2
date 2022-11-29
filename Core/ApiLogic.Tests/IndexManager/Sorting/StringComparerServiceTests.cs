using System.Linq;
using ApiLogic.IndexManager.Sorting;
using FluentAssertions;
using NUnit.Framework;

namespace ApiLogic.Tests.IndexManager.Sorting
{
    [TestFixture]
    public class StringComparerServiceTests
    {
        [Test]
        public void Instance_IsNotNull()
        {
            StringComparerService.Instance.Should().NotBeNull();
        }

        [TestCase(null)]
        [TestCase("")]
        public void GetComparer_EmptyLanguageCode_ReturnsNull(string languageCode)
        {
            var service = new StringComparerService();

            var result = service.GetComparer(languageCode);

            result.Should().BeNull();
        }

        [TestCase("frn", "oe", "oê", "oë")]
        [TestCase("mnd", "木", "林", "森")]
        [TestCase("hnd", "क", "ख", "ग")]
        [TestCase("csp", "m", "n", "ñ")]
        [TestCase("eng", "bee", "cat", "dog")]
        [TestCase("hun", "ddd", "dzz", "dzs")]
        [TestCase("cze", "cza", "čar", "chy")]
        [TestCase("abc", "a", "b", "c")]
        public void GetComparer_ValidLanguageCode_ReturnsExpectedResult(string languageCode, string str0, string str1, string str2)
        {
            var service = new StringComparerService();

            var result = service.GetComparer(languageCode);

            result.Should().NotBeNull();

            var sortedArray = new[] { str2, str1, str0 }.OrderBy(x => x, result).ToArray();
            sortedArray[0].Should().Be(str0);
            sortedArray[1].Should().Be(str1);
            sortedArray[2].Should().Be(str2);
        }

        [Test]
        public void GetComparer_TheSameLanguage_ReturnsTheSameInstance()
        {
            var service = new StringComparerService();

            var result1 = service.GetComparer("eng");
            var result2 = service.GetComparer("eng");

            result1.Should().Be(result2);
        }
    }
}