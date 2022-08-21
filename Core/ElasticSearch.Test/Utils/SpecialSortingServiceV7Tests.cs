using System;
using System.Collections.Generic;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ElasticSearch.Common;
using ElasticSearch.Searcher.Settings;
using ElasticSearch.Utils;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace ElasticSearch.Test.Utils
{
    [TestFixture]
    public class SpecialSortingServiceV7Tests
    {
        private Mock<IElasticSearchIndexDefinitionsNest> _indexDefinitionsMock;

        [SetUp]
        public void SetUp()
        {
            _indexDefinitionsMock = new Mock<IElasticSearchIndexDefinitionsNest>();
        }

        [TestCase(null)]
        [TestCase("")]
        public void IsSpecialSortingField_EmptyLanguageCode_ReturnsFalse(string languageCode)
        {
            var service = new SpecialSortingServiceV7(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingField(languageCode);

            result.Should().BeFalse();
        }

        [TestCase("lg1")]
        public void IsSpecialSortingField_CustomPropertyNotDefined_ReturnsFalse(string languageCode)
        {
            _indexDefinitionsMock
                .Setup(x => x.GetCustomProperties(ElasticsearchVersion.ES_7, languageCode))
                .Returns(new Dictionary<string, CustomProperty> { { "hun_sort", new CustomProperty() } });
            var service = new SpecialSortingServiceV7(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingField(languageCode);

            result.Should().BeFalse();
        }

        [TestCase("eng", false)]
        [TestCase("hun", true)]
        public void IsSpecialSortingField_ValidParameters_ReturnsExpectedResult(string languageCode, bool expectedResult)
        {
            _indexDefinitionsMock
                .Setup(x => x.GetCustomProperties(ElasticsearchVersion.ES_7, languageCode))
                .Returns(new Dictionary<string, CustomProperty> { { "hun_sort", new CustomProperty() } });
            var service = new SpecialSortingServiceV7(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingField(languageCode);

            result.Should().Be(expectedResult);
        }

        [TestCase(typeof(int))]
        [TestCase(typeof(double))]
        [TestCase(typeof(DateTime))]
        public void IsSpecialSortingMeta_MetaTypeNotString_ReturnsFalse(Type metaType)
        {
            var service = new SpecialSortingServiceV7(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingMeta("eng", metaType);

            result.Should().BeFalse();
        }

        [TestCase(null)]
        [TestCase("")]
        public void IsSpecialSortingMeta_EmptyLanguageCode_ReturnsFalse(string languageCode)
        {
            var service = new SpecialSortingServiceV7(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingMeta(languageCode, typeof(string));

            result.Should().BeFalse();
        }

        [TestCase("lg2")]
        public void IsSpecialSortingMeta_AnalyzersNotDefined_ReturnsFalse(string languageCode)
        {
            _indexDefinitionsMock
                .Setup(x => x.GetCustomProperties(ElasticsearchVersion.ES_7, languageCode))
                .Returns(new Dictionary<string, CustomProperty> { { "hun_sort", new CustomProperty() } });
            var service = new SpecialSortingServiceV7(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingMeta(languageCode, typeof(string));

            result.Should().BeFalse();
        }

        [TestCase("eng", false)]
        [TestCase("hun", true)]
        public void IsSpecialSortingMeta_ValidParameters_ReturnsExpectedResult(string languageCode, bool expectedResult)
        {
            _indexDefinitionsMock
                .Setup(x => x.GetCustomProperties(ElasticsearchVersion.ES_7, languageCode))
                .Returns(new Dictionary<string, CustomProperty> { { "hun_sort", new CustomProperty() } });
            var service = new SpecialSortingServiceV7(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingMeta(languageCode, typeof(string));

            result.Should().Be(expectedResult);
        }
    }
}