using System;
using System.Collections.Generic;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiLogic.IndexManager.Sorting;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.IndexManager
{
    public class SortingAdapterTests
    {
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

        [Test]
        public void ResolveOrdering_ThrowsArgumentNullException()
        {
            var sortingAdapter = new SortingAdapter(_mockRepository.Create<IAssetOrderingService>().Object);

            Action act = () => sortingAdapter.ResolveOrdering(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestCaseSource(nameof(NotImplementedExceptionData))]
        public void ResolveOrdering_ThrowsNotImplementedException(UnifiedSearchDefinitions definitions)
        {
            var sortingAdapter = new SortingAdapter(_mockRepository.Create<IAssetOrderingService>().Object);

            Action act = () => sortingAdapter.ResolveOrdering(definitions);

            act.Should().Throw<NotImplementedException>();
        }

        [Test]
        public void ResolveOrdering_OrderByFields_ReturnsExpectedResult()
        {
            var definitions = new UnifiedSearchDefinitions
            {
                orderByFields = new IEsOrderByField[]
                {
                    new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.DESC, null),
                    new EsOrderByField(OrderBy.NAME, OrderDir.DESC)
                }
            };

            var sortingAdapter = new SortingAdapter(_mockRepository.Create<IAssetOrderingService>().Object);
            var result = sortingAdapter.ResolveOrdering(definitions);

            result.Should().BeEquivalentTo(definitions.orderByFields);
        }

        [Test]
        public void ResolveOrdering_Order_ReturnsExpectedResult()
        {
            var expectedResult = new AssetListEsOrderingResult
            {
                EsOrderByFields = new List<IEsOrderByField> { new EsOrderByField(OrderBy.NAME, OrderDir.DESC) },
                Order = new OrderObj()
            };

            var definitions = new UnifiedSearchDefinitions
            {
                order = new OrderObj(),
                groupId = 10,
                shouldSearchEpg = true,
                shouldSearchMedia = true,
                shouldSearchRecordings = true,
                associationTags = new Dictionary<int, string>(),
                parentMediaTypes = new Dictionary<int, int>()
            };

            var assetOrderingServiceMock = _mockRepository.Create<IAssetOrderingService>();
            assetOrderingServiceMock.Setup(x => x.MapToEsOrderByFields(
                definitions.order,
                It.Is<AssetListEsOrderingCommonInput>(y
                    => y.ShouldSearchEpg == definitions.shouldSearchEpg
                    && y.ShouldSearchMedia == definitions.shouldSearchEpg
                    && y.ShouldSearchRecordings == definitions.shouldSearchRecordings
                    && y.GroupId == definitions.groupId
                    && y.AssociationTags == definitions.associationTags
                    && y.ParentMediaTypes == definitions.parentMediaTypes)))
                .Returns(expectedResult);

            var sortingAdapter = new SortingAdapter(assetOrderingServiceMock.Object);

            var result = sortingAdapter.ResolveOrdering(definitions);

            result.Should().BeEquivalentTo(expectedResult.EsOrderByFields, options => options.WithStrictOrdering());
        }

        private static IEnumerable<TestCaseData> NotImplementedExceptionData()
        {
            yield return new TestCaseData(new UnifiedSearchDefinitions { order = null });
            yield return new TestCaseData(new UnifiedSearchDefinitions { orderByFields = new List<IEsOrderByField>(), order = null});
        }
    }
}