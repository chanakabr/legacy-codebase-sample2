using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.IndexManager.Sorting;
using ApiObjects;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.IndexManager
{
    public class SortingByBasicFieldsServiceTests
    {
        private static readonly Random Random = new Random();

        private MockRepository _mockRepository;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [TestCaseSource(nameof(NotImplementedExceptionData))]
        public void ListOrderedIdsWithSortValues_ThrowsNotImplementedException(IEsOrderByField esOrderByField)
        {
            var sortingByBasicFieldsService = new SortingByBasicFieldsService();

            Action act = () => sortingByBasicFieldsService.ListOrderedIdsWithSortValues(
                Enumerable.Empty<ElasticSearchApi.ESAssetDocument>(),
                esOrderByField);

            act.Should().Throw<NotImplementedException>();
        }

        [TestCaseSource(nameof(SortingData))]
        public void ListOrderedIdsWithSortValues_ReturnsExpectedResult(
            IEnumerable<ElasticSearchApi.ESAssetDocument> esDocuments,
            IEsOrderByField orderByField,
            IEnumerable<(long id, string sortValue)> expectedResult)
        {
            var sortingByBasicFieldsService = new SortingByBasicFieldsService();

            var actualResult = sortingByBasicFieldsService.ListOrderedIdsWithSortValues(esDocuments, orderByField);

            actualResult.Should().BeEquivalentTo(expectedResult, options => options.WithStrictOrdering());
        }

        private static IEnumerable<ElasticSearchApi.ESAssetDocument> GenerateData(int count)
        {
            return Enumerable.Range(1, count)
                .Select(x => GenerateDocument(x, count))
                .Reverse()
                .ToArray();
        }

        private static ElasticSearchApi.ESAssetDocument GenerateDocument(int assetId, int totalCount)
        {
            var assetDateTime = DateTime.UtcNow.AddDays(assetId - totalCount);

            return new ElasticSearchApi.ESAssetDocument
            {
                name = $"document from {assetDateTime:s}",
                id = assetId.ToString(),
                asset_id = assetId,
                start_date = assetDateTime,
                update_date = assetDateTime,
                score = assetId,
                extraReturnFields = new Dictionary<string, string>
                {
                    {
                        "create_date",
                        $"{assetDateTime}"
                    },
                    {
                        "metas.meta",
                        $"meta from {assetDateTime:s}"
                    },
                    {
                        "metas.padded_meta_date",
                        $"{assetDateTime}"
                    },
                    {
                        "metas.meta_int",
                        $"{assetId}"
                    },
                    {
                        "metas.padded_meta_double",
                        $"{assetId}.5"
                    },
                    {
                        "metas.meta_eng",
                        $"eng meta from {assetDateTime:s}"
                    },
                    {
                        "metas.padded_meta_eng",
                        $"padded eng meta from {assetDateTime:s}"
                    }
                }
            };
        }

        private static IEnumerable<TestCaseData> NotImplementedExceptionData()
        {
            yield return new TestCaseData(new EsOrderBySlidingWindow(OrderBy.VIEWS, OrderDir.DESC, default));
            yield return new TestCaseData(new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, default));
            yield return new TestCaseData(new EsOrderByStartDateAndAssociationTags(OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.VIEWS, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.RATING, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.LIKE_COUNTER, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.VOTES_COUNT, OrderDir.DESC));
        }

        private static IEnumerable<TestCaseData> SortingData()
        {
            var esDocuments = GenerateData(10);
            var shuffledDocuments = esDocuments.OrderBy(x => Random.Next()).ToList();

            IEnumerable<(long id, string sortValue)> GetExpectedResult(Func<ElasticSearchApi.ESAssetDocument, string> getSortValue)
                => esDocuments
                    .Select(x => (x.asset_id, getSortValue(x)))
                    .Select(x => ((long id, string sortValue))x)
                    .ToArray();

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByField(OrderBy.ID, OrderDir.DESC),
                GetExpectedResult(x => x.id));

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByField(OrderBy.NAME, OrderDir.DESC),
                GetExpectedResult(x => x.name));

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByField(OrderBy.START_DATE, OrderDir.ASC),
                GetExpectedResult(x => x.start_date.ToString()).Reverse());

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByField(OrderBy.EPG_ID, OrderDir.DESC),
                GetExpectedResult(x => x.asset_id.ToString()));

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByField(OrderBy.MEDIA_ID, OrderDir.ASC),
                GetExpectedResult(x => x.asset_id.ToString()).Reverse());

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.DESC),
                GetExpectedResult(x => DateTime.Parse(x.extraReturnFields["create_date"]).ToString()));

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByField(OrderBy.UPDATE_DATE, OrderDir.DESC),
                GetExpectedResult(x => x.update_date.ToString()));

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByField(OrderBy.UPDATE_DATE, OrderDir.DESC),
                GetExpectedResult(x => x.update_date.ToString()));

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByField(OrderBy.RELATED, OrderDir.ASC),
                GetExpectedResult(x => x.score.ToString()).Reverse());

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByField(OrderBy.NONE, OrderDir.DESC),
                GetExpectedResult(x => x.score.ToString()));

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByMetaField("meta", OrderDir.DESC, false, typeof(string), null),
                GetExpectedResult(x => x.extraReturnFields["metas.meta"]));

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByMetaField("meta_date", OrderDir.ASC, true, typeof(DateTime), new LanguageObj { IsDefault = true }),
                GetExpectedResult(x => x.extraReturnFields["metas.padded_meta_date"]).Reverse());

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByMetaField("meta_int", OrderDir.DESC, false, typeof(int), new LanguageObj { IsDefault = true }),
                GetExpectedResult(x => x.extraReturnFields["metas.meta_int"]));

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByMetaField("meta_double", OrderDir.ASC, true, typeof(double), null),
                GetExpectedResult(x => x.extraReturnFields["metas.padded_meta_double"]).Reverse());

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByMetaField("meta", OrderDir.DESC, false, typeof(string), new LanguageObj { Code = "eng" }),
                GetExpectedResult(x => x.extraReturnFields["metas.meta_eng"]));

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByMetaField("meta", OrderDir.ASC, false, typeof(string), new LanguageObj { Code = "eng" }),
                GetExpectedResult(x => x.extraReturnFields["metas.meta_eng"]).Reverse());

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByMetaField("meta", OrderDir.DESC, true, typeof(string), new LanguageObj { Code = "eng" }),
                GetExpectedResult(x => x.extraReturnFields["metas.padded_meta_eng"]));

            yield return new TestCaseData(
                shuffledDocuments,
                new EsOrderByMetaField("meta", OrderDir.ASC, true, typeof(string), new LanguageObj { Code = "eng" }),
                GetExpectedResult(x => x.extraReturnFields["metas.padded_meta_eng"]).Reverse());
        }
    }
}