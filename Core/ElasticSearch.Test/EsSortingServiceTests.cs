using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;
using ElasticSearch.Utils;
using FluentAssertions;
using NUnit.Framework;

namespace ElasticSearch.Test
{
    public class EsSortingServiceTests
    {
        [TestCaseSource(nameof(ShouldSortByStatisticsNegativeData))]
        public void ShouldSortByStatistics_ReturnsFalse(IEnumerable<IEsOrderByField> esOrderByFields)
        {
            var service = new EsSortingService(ElasticsearchVersion.ES_2_3);

            var result = service.ShouldSortByStatistics(esOrderByFields);

            result.Should().BeFalse();
        }

        [TestCaseSource(nameof(ShouldSortByStatisticsPositiveData))]
        public void ShouldSortByStatistics_ReturnsTrue(IEnumerable<IEsOrderByField> esOrderByFields)
        {
            var service = new EsSortingService(ElasticsearchVersion.ES_2_3);

            var result = service.ShouldSortByStatistics(esOrderByFields);

            result.Should().BeTrue();
        }

        [TestCaseSource(nameof(ShouldSortByStatisticsSingleNegativeData))]
        public void ShouldSortByStatistics_ReturnsFalse(IEsOrderByField esOrderByField)
        {
            var service = new EsSortingService(ElasticsearchVersion.ES_2_3);

            var result = service.ShouldSortByStatistics(esOrderByField);

            result.Should().BeFalse();
        }

        [TestCaseSource(nameof(ShouldSortByStatisticsSinglePositiveData))]
        public void ShouldSortByStatistics_ReturnsTrue(IEsOrderByField esOrderByField)
        {
            var service = new EsSortingService(ElasticsearchVersion.ES_2_3);

            var result = service.ShouldSortByStatistics(esOrderByField);

            result.Should().BeTrue();
        }

        [TestCaseSource(nameof(ShouldSortByStartDateOfAssociationTagsNegativeData))]
        public void ShouldSortByStartDateOfAssociationTags_ReturnsFalse(IEnumerable<IEsOrderByField> esOrderByFields)
        {
            var service = new EsSortingService(ElasticsearchVersion.ES_2_3);

            var result = service.ShouldSortByStartDateOfAssociationTags(esOrderByFields);

            result.Should().BeFalse();
        }

        [TestCaseSource(nameof(ShouldSortByStartDateOfAssociationTagsPositiveData))]
        public void ShouldSortByStartDateOfAssociationTags_ReturnsTrue(IEnumerable<IEsOrderByField> esOrderByFields)
        {
            var service = new EsSortingService(ElasticsearchVersion.ES_2_3);

            var result = service.ShouldSortByStatistics(esOrderByFields);

            result.Should().BeTrue();
        }

        [TestCaseSource(nameof(IsBucketsReorderingRequiredNegativeData))]
        public void IsBucketsReorderingRequired_ReturnsFalse(
            IReadOnlyCollection<IEsOrderByField> esOrderByFields,
            GroupByDefinition groupBy)
        {
            var service = new EsSortingService(ElasticsearchVersion.ES_2_3);

            var result = service.IsBucketsReorderingRequired(esOrderByFields, groupBy);

            result.Should().BeFalse();
        }

        [TestCaseSource(nameof(IsBucketsReorderingRequiredPositiveData))]
        public void IsBucketsReorderingRequired_ReturnsTrue(IReadOnlyCollection<IEsOrderByField> esOrderByFields)
        {
            var service = new EsSortingService(ElasticsearchVersion.ES_2_3);

            var result = service.IsBucketsReorderingRequired(esOrderByFields, new GroupByDefinition { Key = "key" });

            result.Should().BeTrue();
        }

        [TestCaseSource(nameof(GetSortingData))]
        public void GetSorting_ReturnsExpectedResult(
            IEnumerable<IEsOrderByField> orderByFields,
            bool functionScoreSort,
            string expectedResult)
        {
            var service = new EsSortingService(ElasticsearchVersion.ES_2_3);

            var result = service.GetSorting(orderByFields, functionScoreSort);

            result.Should().Be(expectedResult);
        }

        [TestCaseSource(nameof(BuildExtraReturnFields))]
        public void BuildExtraReturnFields_ReturnsExpectedResult(
            IEnumerable<IEsOrderByField> orderByFields,
            IEnumerable<string> expectedResult)
        {
            var service = new EsSortingService(ElasticsearchVersion.ES_2_3);

            var result = service.BuildExtraReturnFields(orderByFields);

            result.Should().BeEquivalentTo(expectedResult);
        }

        private static IEnumerable<TestCaseData> ShouldSortByStatisticsNegativeData()
        {
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.NAME, OrderDir.DESC)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByMetaField("meta", OrderDir.DESC, true, typeof(string), new LanguageObj { Code = "eng" })
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByMetaField("meta", OrderDir.ASC, false, typeof(int), new LanguageObj { IsDefault = true }),
                new EsOrderByField(OrderBy.START_DATE, OrderDir.ASC)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.DESC),
                new EsOrderByField(OrderBy.NAME, OrderDir.ASC)
            });
        }

        private static IEnumerable<TestCaseData> ShouldSortByStatisticsPositiveData()
        {
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, DateTime.UtcNow)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByStatisticsField(OrderBy.RATING, OrderDir.DESC, null)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByStatisticsField(OrderBy.VOTES_COUNT, OrderDir.DESC, null)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.DESC, null)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderBySlidingWindow(OrderBy.VIEWS, OrderDir.DESC, default)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderBySlidingWindow(OrderBy.RATING, OrderDir.DESC, 10)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderBySlidingWindow(OrderBy.VOTES_COUNT, OrderDir.DESC, 100)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderBySlidingWindow(OrderBy.LIKE_COUNTER, OrderDir.DESC, default)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByStartDateAndAssociationTags(OrderDir.DESC)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.START_DATE, OrderDir.DESC),
                new EsOrderByStatisticsField(OrderBy.VOTES_COUNT, OrderDir.DESC, null)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.DESC, null),
                new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.DESC),
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.RECOMMENDATION, OrderDir.DESC),
                new EsOrderByField(OrderBy.NAME, OrderDir.DESC),
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderBySlidingWindow(OrderBy.RATING, OrderDir.DESC, 10),
                new EsOrderByField(OrderBy.NAME, OrderDir.DESC),
            });
        }

        private static IEnumerable<TestCaseData> ShouldSortByStatisticsSingleNegativeData()
        {
            yield return new TestCaseData(new EsOrderByField(OrderBy.ID, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.NAME, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.START_DATE, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.UPDATE_DATE, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.EPG_ID, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.MEDIA_ID, OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByMetaField("meta", OrderDir.DESC, true, typeof(int), new LanguageObj { IsDefault = true }));
            yield return new TestCaseData(new EsOrderByMetaField("meta", OrderDir.ASC, false, typeof(DateTime), new LanguageObj { Code = "eng" }));
        }

        private static IEnumerable<TestCaseData> ShouldSortByStatisticsSinglePositiveData()
        {
            yield return new TestCaseData(new EsOrderByStatisticsField(OrderBy.VOTES_COUNT, OrderDir.DESC, DateTime.UtcNow));
            yield return new TestCaseData(new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.ASC, DateTime.UtcNow));
            yield return new TestCaseData(new EsOrderByStatisticsField(OrderBy.RATING, OrderDir.DESC, null));
            yield return new TestCaseData(new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.ASC, DateTime.UtcNow));
            yield return new TestCaseData(new EsOrderBySlidingWindow(OrderBy.VOTES_COUNT, OrderDir.DESC, default));
            yield return new TestCaseData(new EsOrderBySlidingWindow(OrderBy.LIKE_COUNTER, OrderDir.ASC, 10));
            yield return new TestCaseData(new EsOrderBySlidingWindow(OrderBy.RATING, OrderDir.DESC, 5));
            yield return new TestCaseData(new EsOrderBySlidingWindow(OrderBy.VIEWS, OrderDir.ASC, default));
            yield return new TestCaseData(new EsOrderByStartDateAndAssociationTags(OrderDir.DESC));
            yield return new TestCaseData(new EsOrderByField(OrderBy.RECOMMENDATION, OrderDir.DESC));
        }

        private static IEnumerable<TestCaseData> ShouldSortByStartDateOfAssociationTagsNegativeData()
        {
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.NAME, OrderDir.DESC)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByMetaField("meta", OrderDir.DESC, true, typeof(string), new LanguageObj { IsDefault = true })
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByMetaField("meta", OrderDir.ASC, false, typeof(int), new LanguageObj { Code = "eng" }),
                new EsOrderByField(OrderBy.START_DATE, OrderDir.ASC)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.DESC),
                new EsOrderByField(OrderBy.NAME, OrderDir.ASC)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderBySlidingWindow(OrderBy.VOTES_COUNT, OrderDir.DESC, 10),
                new EsOrderByField(OrderBy.NAME, OrderDir.ASC)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, DateTime.UtcNow),
                new EsOrderByField(OrderBy.NAME, OrderDir.ASC)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.NAME, OrderDir.ASC),
                new EsOrderByField(OrderBy.RECOMMENDATION, OrderDir.DESC)
            });
        }

        private static IEnumerable<TestCaseData> ShouldSortByStartDateOfAssociationTagsPositiveData()
        {
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByStartDateAndAssociationTags(OrderDir.DESC)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByStartDateAndAssociationTags(OrderDir.DESC),
                new EsOrderByMetaField("meta", OrderDir.DESC, true, typeof(string), null)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.ASC),
                new EsOrderByStartDateAndAssociationTags(OrderDir.DESC)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByStartDateAndAssociationTags(OrderDir.DESC),
                new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.ASC, DateTime.Now)
            });
        }

        private static IEnumerable<TestCaseData> IsBucketsReorderingRequiredNegativeData()
        {
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.DESC)
            }, null);
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, DateTime.UtcNow)
            }, null);
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByStartDateAndAssociationTags(OrderDir.DESC)
            }, null);
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderBySlidingWindow(OrderBy.VOTES_COUNT, OrderDir.DESC, default)
            }, null);
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.DESC)
            }, new GroupByDefinition { Key = "key" });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.START_DATE, OrderDir.DESC)
            }, new GroupByDefinition { Key = "key" });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.NONE, OrderDir.DESC)
            }, new GroupByDefinition { Key = "key" });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.RELATED, OrderDir.DESC)
            }, new GroupByDefinition { Key = "key" });
        }

        private static IEnumerable<TestCaseData> IsBucketsReorderingRequiredPositiveData()
        {
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, DateTime.UtcNow)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByStartDateAndAssociationTags(OrderDir.DESC)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderBySlidingWindow(OrderBy.VOTES_COUNT, OrderDir.DESC, default)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.DESC),
                new EsOrderByField(OrderBy.NAME, OrderDir.DESC)
            });
            yield return new TestCaseData(new List<IEsOrderByField>
            {
                new EsOrderByField(OrderBy.NAME, OrderDir.DESC),
                new EsOrderByStatisticsField(OrderBy.VOTES_COUNT, OrderDir.DESC, DateTime.UtcNow)
            });
        }

        private static IEnumerable<TestCaseData> GetSortingData()
        {
            yield return new TestCaseData(
                new List<IEsOrderByField> { new EsOrderByField(OrderBy.NAME, OrderDir.DESC) },
                false,
                "\"sort\" : [{\"name\":{\"order\":\"desc\"}},{\"_uid\":{\"order\":\"desc\"}}]");
            yield return new TestCaseData(
                new List<IEsOrderByField> { new EsOrderByField(OrderBy.ID, OrderDir.ASC) },
                false,
                "\"sort\" : [{\"_uid\":{\"order\":\"asc\"}}]");
            yield return new TestCaseData(
                new List<IEsOrderByField> { new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.ASC, DateTime.UtcNow) },
                false,
                "\"sort\" : [{\"_uid\":{\"order\":\"desc\"}}]");
            yield return new TestCaseData(
                new List<IEsOrderByField> { new EsOrderByMetaField("meta", OrderDir.ASC, false, typeof(DateTime), null) },
                false,
                "\"sort\" : [{\"metas.meta\":{\"order\":\"asc\"}},{\"_uid\":{\"order\":\"desc\"}}]");
            yield return new TestCaseData(
                new List<IEsOrderByField>
                {
                    new EsOrderByMetaField("meta", OrderDir.ASC, true, typeof(int), new LanguageObj { IsDefault = true }),
                    new EsOrderByField(OrderBy.NAME, OrderDir.DESC)
                },
                false,
                "\"sort\" : [{\"metas.padded_meta\":{\"order\":\"asc\"}},{\"name\":{\"order\":\"desc\"}},{\"_uid\":{\"order\":\"desc\"}}]");
            yield return new TestCaseData(
                new List<IEsOrderByField>
                {
                    new EsOrderByMetaField("meta1", OrderDir.ASC, false, typeof(int), new LanguageObj { Code = "eng" }),
                    new EsOrderByMetaField("meta2", OrderDir.DESC, true, typeof(int), new LanguageObj { Code = "eng" }),
                },
                false,
                "\"sort\" : [{\"metas.meta1_eng\":{\"order\":\"asc\"}},{\"metas.padded_meta2_eng\":{\"order\":\"desc\"}},{\"_uid\":{\"order\":\"desc\"}}]");
            yield return new TestCaseData(
                new List<IEsOrderByField>
                {
                    new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.DESC),
                    new EsOrderByStartDateAndAssociationTags(OrderDir.DESC)
                },
                false,
                "\"sort\" : [{\"create_date\":{\"order\":\"desc\"}},{\"_uid\":{\"order\":\"desc\"}}]");
            yield return new TestCaseData(
                new List<IEsOrderByField>
                {
                    new EsOrderBySlidingWindow(OrderBy.LIKE_COUNTER, OrderDir.DESC, 10),
                    new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.DESC)
                },
                false,
                "\"sort\" : [{\"_uid\":{\"order\":\"desc\"}}]");
            yield return new TestCaseData(
                new List<IEsOrderByField> { new EsOrderByField(OrderBy.NAME, OrderDir.DESC) },
                true,
                "\"sort\" : [{\"_score\":{\"order\":\"desc\"}},{\"name\":{\"order\":\"desc\"}},{\"_uid\":{\"order\":\"desc\"}}]");
            yield return new TestCaseData(
                new List<IEsOrderByField>
                {
                    new EsOrderBySlidingWindow(OrderBy.LIKE_COUNTER, OrderDir.DESC, 10),
                    new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.DESC)
                },
                true,
                "\"sort\" : [{\"_score\":{\"order\":\"desc\"}},{\"_uid\":{\"order\":\"desc\"}}]");
            yield return new TestCaseData(
                new List<IEsOrderByField>
                {
                    new EsOrderByField(OrderBy.NAME, OrderDir.DESC),
                    new EsOrderByField(OrderBy.RELATED, OrderDir.DESC)
                },
                true,
                "\"sort\" : [{\"_score\":{\"order\":\"desc\"}},{\"name\":{\"order\":\"desc\"}},{\"_uid\":{\"order\":\"desc\"}}]");
            yield return new TestCaseData(
                new List<IEsOrderByField>
                {
                    new EsOrderByField(OrderBy.NAME, OrderDir.DESC, new LanguageObj { Code = "eng", IsDefault = true })
                },
                false,
                "\"sort\" : [{\"name\":{\"order\":\"desc\"}},{\"_uid\":{\"order\":\"desc\"}}]");
            yield return new TestCaseData(
                new List<IEsOrderByField>
                {
                    new EsOrderByField(OrderBy.CREATE_DATE, OrderDir.DESC),
                    new EsOrderByField(OrderBy.NAME, OrderDir.DESC, new LanguageObj { Code = "arb"})
                },
                false,
                "\"sort\" : [{\"create_date\":{\"order\":\"desc\"}},{\"name_arb\":{\"order\":\"desc\"}},{\"_uid\":{\"order\":\"desc\"}}]");
        }

        private static IEnumerable<TestCaseData> BuildExtraReturnFields()
        {
            yield return new TestCaseData(
                new IEsOrderByField[]
                {
                    new EsOrderByField(OrderBy.NAME, OrderDir.DESC),
                    new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, null)
                },
                new[] { "name" });
            yield return new TestCaseData(
                new IEsOrderByField[]
                {
                    new EsOrderByMetaField("meta1", OrderDir.DESC, false, typeof(int), null),
                    new EsOrderByMetaField("meta2", OrderDir.DESC, true, typeof(string), new LanguageObj {Code = "eng"})
                },
                Enumerable.Empty<string>());
            yield return new TestCaseData(
                new IEsOrderByField[]
                {
                    new EsOrderByStartDateAndAssociationTags(OrderDir.DESC),
                    new EsOrderByMetaField("meta2", OrderDir.DESC, true, typeof(string), new LanguageObj { IsDefault = true })
                },
                new[] { "metas.padded_meta2" });
            yield return new TestCaseData(
                new IEsOrderByField[]
                {

                    new EsOrderByMetaField("meta2", OrderDir.ASC, true, typeof(string), new LanguageObj { Code = "eng" }),
                    new EsOrderBySlidingWindow(OrderBy.RATING, OrderDir.DESC, 1)
                },
                new[] { "metas.padded_meta2_eng" });
            yield return new TestCaseData(
                new IEsOrderByField[]
                {
                    new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, null),
                    new EsOrderByField(OrderBy.NAME, OrderDir.DESC, new LanguageObj { Code = "csp", IsDefault = true }),
                },
                new[] { "name" });
            yield return new TestCaseData(
                new IEsOrderByField[]
                {

                    new EsOrderByField(OrderBy.NAME, OrderDir.ASC, new LanguageObj { Code = "csp" }),
                    new EsOrderByStatisticsField(OrderBy.VOTES_COUNT, OrderDir.DESC, null)
                },
                new[] { "name_csp" });
        }
    }
}