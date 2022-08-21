using System;
using ElasticSearch.Common;
using ElasticSearch.Utils;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace ElasticSearch.Test.Utils
{
    [TestFixture]
    public class SpecialSortingServiceV2Tests
    {
        private const string ANALYZERS_DEFINITION = @"""hun_sorting_analyzer"":{}";

        private Mock<IElasticSearchIndexDefinitionsBase> _indexDefinitionsMock;

        [SetUp]
        public void SetUp()
        {
            _indexDefinitionsMock = new Mock<IElasticSearchIndexDefinitionsBase>();
        }

        [TestCase(null)]
        [TestCase("")]
        public void IsSpecialSortingField_EmptyLanguageCode_ReturnsFalse(string languageCode)
        {
            var service = new SpecialSortingServiceV2(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingField(languageCode);

            result.Should().BeFalse();
        }

        [TestCase("lg1", null)]
        [TestCase("lg2", "")]
        public void IsSpecialSortingField_AnalyzersNotDefined_ReturnsFalse(string languageCode, string analyzersDefinition)
        {
            _indexDefinitionsMock
                .Setup(x => x.GetAnalyzerDefinition($"{languageCode}_analyzer_v2"))
                .Returns(analyzersDefinition);
            var service = new SpecialSortingServiceV2(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingField(languageCode);

            result.Should().BeFalse();
        }

        [TestCase("eng", false)]
        [TestCase("hun", true)]
        public void IsSpecialSortingField_ValidParameters_ReturnsExpectedResult(string languageCode, bool expectedResult)
        {
            _indexDefinitionsMock
                .Setup(x => x.GetAnalyzerDefinition($"{languageCode}_analyzer_v2"))
                .Returns(ANALYZERS_DEFINITION);
            var service = new SpecialSortingServiceV2(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingField(languageCode);

            result.Should().Be(expectedResult);
        }

        [TestCase(typeof(int))]
        [TestCase(typeof(double))]
        [TestCase(typeof(DateTime))]
        public void IsSpecialSortingMeta_MetaTypeNotString_ReturnsFalse(Type metaType)
        {
            var service = new SpecialSortingServiceV2(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingMeta("eng", metaType);

            result.Should().BeFalse();
        }

        [TestCase(null)]
        [TestCase("")]
        public void IsSpecialSortingMeta_EmptyLanguageCode_ReturnsFalse(string languageCode)
        {
            var service = new SpecialSortingServiceV2(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingMeta(languageCode, typeof(string));

            result.Should().BeFalse();
        }

        [TestCase("lg3", null)]
        [TestCase("lg4", "")]
        public void IsSpecialSortingMeta_AnalyzersNotDefined_ReturnsFalse(string languageCode, string analyzersDefinition)
        {
            _indexDefinitionsMock
                .Setup(x => x.GetAnalyzerDefinition($"{languageCode}_analyzer_v2"))
                .Returns(analyzersDefinition);
            var service = new SpecialSortingServiceV2(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingMeta(languageCode, typeof(string));

            result.Should().BeFalse();
        }

        [TestCase("eng", false)]
        [TestCase("hun", true)]
        public void IsSpecialSortingMeta_ValidParameters_ReturnsExpectedResult(string languageCode, bool expectedResult)
        {
            _indexDefinitionsMock
                .Setup(x => x.GetAnalyzerDefinition($"{languageCode}_analyzer_v2"))
                .Returns(ANALYZERS_DEFINITION);
            var service = new SpecialSortingServiceV2(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingMeta(languageCode, typeof(string));

            result.Should().Be(expectedResult);
        }
    }
}