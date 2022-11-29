using System;
using System.Collections.Generic;
using ApiObjects;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
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

        [Test]
        public void IsSpecialSortingField_NoNameField_ReturnsFalse()
        {
            var field = new EsOrderByField(OrderBy.ID, OrderDir.ASC, new LanguageObj { Code = "lg1" });
            var service = new SpecialSortingServiceV7(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingField(field);

            result.Should().BeFalse();
        }

        [TestCase(null)]
        [TestCase("")]
        public void IsSpecialSortingField_EmptyLanguageCode_ReturnsFalse(string languageCode)
        {
            var field = new EsOrderByField(OrderBy.NAME, OrderDir.ASC, new LanguageObj { Code = languageCode });
            var service = new SpecialSortingServiceV7(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingField(field);

            result.Should().BeFalse();
        }

        [Test]
        public void IsSpecialSortingField_CustomPropertyNotDefined_ReturnsFalse()
        {
            var languageCode = "lg1";
            var field = new EsOrderByField(OrderBy.NAME, OrderDir.ASC, new LanguageObj { Code = languageCode });
            _indexDefinitionsMock
                .Setup(x => x.GetCustomProperties(ElasticsearchVersion.ES_7, languageCode))
                .Returns(new Dictionary<string, CustomProperty> { { "hun_sort", new CustomProperty() } });
            var service = new SpecialSortingServiceV7(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingField(field);

            result.Should().BeFalse();
        }

        [TestCase("eng", false)]
        [TestCase("hun", true)]
        public void IsSpecialSortingField_ValidParameters_ReturnsExpectedResult(string languageCode, bool expectedResult)
        {
            var field = new EsOrderByField(OrderBy.NAME, OrderDir.ASC, new LanguageObj { Code = languageCode });
            _indexDefinitionsMock
                .Setup(x => x.GetCustomProperties(ElasticsearchVersion.ES_7, languageCode))
                .Returns(new Dictionary<string, CustomProperty> { { "hun_sort", new CustomProperty() } });
            var service = new SpecialSortingServiceV7(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingField(field);

            result.Should().Be(expectedResult);
        }

        [TestCase(typeof(int))]
        [TestCase(typeof(double))]
        [TestCase(typeof(DateTime))]
        public void IsSpecialSortingMeta_MetaTypeNotString_ReturnsFalse(Type metaType)
        {
            var meta = new EsOrderByMetaField("metaName", OrderDir.ASC, false, metaType, new LanguageObj { Code = "eng" });
            var service = new SpecialSortingServiceV7(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingMeta(meta);

            result.Should().BeFalse();
        }

        [TestCase(null)]
        [TestCase("")]
        public void IsSpecialSortingMeta_EmptyLanguageCode_ReturnsFalse(string languageCode)
        {
            var meta = new EsOrderByMetaField("metaName", OrderDir.ASC, false, typeof(string), new LanguageObj { Code = languageCode });
            var service = new SpecialSortingServiceV7(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingMeta(meta);

            result.Should().BeFalse();
        }

        [TestCase("lg2")]
        public void IsSpecialSortingMeta_AnalyzersNotDefined_ReturnsFalse(string languageCode)
        {
            var meta = new EsOrderByMetaField("metaName", OrderDir.ASC, false, typeof(string), new LanguageObj { Code = "lg1" });
            _indexDefinitionsMock
                .Setup(x => x.GetCustomProperties(ElasticsearchVersion.ES_7, languageCode))
                .Returns(new Dictionary<string, CustomProperty> { { "hun_sort", new CustomProperty() } });
            var service = new SpecialSortingServiceV7(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingMeta(meta);

            result.Should().BeFalse();
        }

        [TestCase("eng", false)]
        [TestCase("hun", true)]
        public void IsSpecialSortingMeta_ValidParameters_ReturnsExpectedResult(string languageCode, bool expectedResult)
        {
            var meta = new EsOrderByMetaField("metaName", OrderDir.ASC, false, typeof(string), new LanguageObj { Code = languageCode });
            _indexDefinitionsMock
                .Setup(x => x.GetCustomProperties(ElasticsearchVersion.ES_7, languageCode))
                .Returns(new Dictionary<string, CustomProperty> { { "hun_sort", new CustomProperty() } });
            var service = new SpecialSortingServiceV7(_indexDefinitionsMock.Object);

            var result = service.IsSpecialSortingMeta(meta);

            result.Should().Be(expectedResult);
        }
    }
}