using System;
using System.Collections.Generic;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiLogic.Catalog.CatalogManagement.Services.GroupRepresentatives;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.SearchObjects;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Request;
using ElasticSearch.Searcher;
using FluentAssertions;
using GroupsCacheManager;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.Catalog.CatalogManagement.Services
{
    public class AssetOrderingServiceTests
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
        public void MapToEsOrderByFields_MediaRelatedRequest_ReturnsRelated()
        {
            var request = new MediaRelatedRequest
            {
                OrderingParameters = null,
                OrderObj = null,
                m_nMediaID = 1
            };
            var expectedResult = new AssetListEsOrderingResult
            {
                Order = new OrderObj { m_eOrderBy = OrderBy.RELATED, m_eOrderDir = OrderDir.DESC },
                EsOrderByFields = new[] { new EsOrderByField(OrderBy.RELATED, OrderDir.DESC) }
            };

            var service = new AssetOrderingService(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IGroupRepresentativesExtendedRequestMapper>().Object);
            var input = new AssetListEsOrderingCommonInput();

            var result = service.MapToEsOrderByFields(request, input);

            result.Should().NotBeNull();
            result.EsOrderByFields.Should().BeEquivalentTo(expectedResult.EsOrderByFields);
            result.Order.Should().BeEquivalentTo(
                expectedResult.Order,
                opts => opts.Excluding(x => x.m_dSlidingWindowStartTimeField));
        }

        [TestCaseSource(nameof(OrderObjData))]
        public void MapToEsOrderByFields_MediaRelatedRequest_ReturnsExpectedResult(
            OrderObj order,
            AssetListEsOrderingResult expectedResult)
        {
            var request = new MediaRelatedRequest { OrderObj = order };
            var service = new AssetOrderingService(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IGroupRepresentativesExtendedRequestMapper>().Object);
            var input = new AssetListEsOrderingCommonInput();

            var result = service.MapToEsOrderByFields(request, input);

            result.Should().NotBeNull();
            result.EsOrderByFields.Should().BeEquivalentTo(expectedResult.EsOrderByFields);
            result.Order.Should().BeEquivalentTo(
                expectedResult.Order,
                opts => opts.Excluding(x => x.m_dSlidingWindowStartTimeField));
        }

        [TestCaseSource(nameof(AssetOrdersData))]
        public void MapToEsOrderByFields_MediaRelatedRequest_ReturnsExpectedResult(
            IReadOnlyCollection<AssetOrder> orderings,
            AssetListEsOrderingResult expectedResult)
        {
            var request = new MediaRelatedRequest
            {
                OrderingParameters = orderings,
                OrderObj = new OrderObj { m_eOrderBy = OrderBy.START_DATE, m_eOrderDir = OrderDir.DESC }
            };
            var service = new AssetOrderingService(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IGroupRepresentativesExtendedRequestMapper>().Object);
            var input = new AssetListEsOrderingCommonInput();

            var result = service.MapToEsOrderByFields(request, input);

            result.Should().NotBeNull();
            result.EsOrderByFields.Should().BeEquivalentTo(expectedResult.EsOrderByFields);
            result.Order.Should().BeEquivalentTo(
                expectedResult.Order,
                opts => opts.Excluding(x => x.m_dSlidingWindowStartTimeField));
        }

        [TestCaseSource(nameof(OrderByMetaData))]
        public void MapToEsOrderByFields_MediaRelatedRequest_ReturnsMeta(
            bool shouldSearchEpg,
            Type metaType,
            bool isMetaPadded,
            LanguageObj language)
        {
            const string metaName = "meta";
            var expectedResult = new AssetListEsOrderingResult
            {
                Order = new OrderObj { m_eOrderBy = OrderBy.META, m_eOrderDir = OrderDir.DESC, m_sOrderValue = metaName, shouldPadString = isMetaPadded },
                EsOrderByFields = new[] { new EsOrderByMetaField(metaName, OrderDir.DESC, isMetaPadded, metaType, language) }
            };

            var request = new MediaRelatedRequest
            {
                OrderObj = new OrderObj
                {
                    m_eOrderBy = OrderBy.META,
                    m_eOrderDir = OrderDir.DESC,
                    m_sOrderValue = metaName
                }
            };

            var input = new AssetListEsOrderingCommonInput
            {
                ShouldSearchMedia = true,
                ShouldSearchEpg = shouldSearchEpg,
                ShouldSearchRecordings = true,
                GroupId = 10,
                Language = language
            };

            var catalogManagerMock = _mockRepository.Create<ICatalogManager>();
            catalogManagerMock
                .Setup(x => x.GetMetaByName(
                    It.Is<MetaByNameInput>(y => y.MetaName == metaName
                        && y.ShouldSearchEpg == input.ShouldSearchEpg
                        && y.ShouldSearchMedia == input.ShouldSearchMedia
                        && y.ShouldSearchRecordings == input.ShouldSearchRecordings
                        && y.GroupId == input.GroupId)))
                .Returns(new BooleanLeafFieldDefinitions { ValueType = metaType });

            var service = new AssetOrderingService(
                catalogManagerMock.Object,
                _mockRepository.Create<IGroupRepresentativesExtendedRequestMapper>().Object);
            var result = service.MapToEsOrderByFields(request, input);

            result.Should().NotBeNull();
            result.EsOrderByFields.Should().BeEquivalentTo(expectedResult.EsOrderByFields);
            result.Order.Should().BeEquivalentTo(
                expectedResult.Order,
                opts => opts.Excluding(x => x.m_dSlidingWindowStartTimeField));
        }

        [TestCaseSource(nameof(OrderByStartDate))]
        public void MapToEsOrderByFields_MediaRelatedRequest_ReturnsStartDate(
            OrderObj order,
            IReadOnlyCollection<AssetOrder> orderings,
            AssetListEsOrderingResult expectedResult)
        {
            var request = new MediaRelatedRequest
            {
                OrderObj = order,
                OrderingParameters = orderings
            };

            var input = new AssetListEsOrderingCommonInput
            {
                ShouldSearchMedia = true,
                ParentMediaTypes = new Dictionary<int, int> { { 1, 1 } },
                AssociationTags = new Dictionary<int, string> { { 1, "1" } }
            };

            var service = new AssetOrderingService(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IGroupRepresentativesExtendedRequestMapper>().Object);
            var result = service.MapToEsOrderByFields(request, input);

            result.Should().NotBeNull();
            result.EsOrderByFields.Should().BeEquivalentTo(expectedResult.EsOrderByFields);
            result.Order.Should().BeEquivalentTo(
                expectedResult.Order,
                opts => opts.Excluding(x => x.m_dSlidingWindowStartTimeField));
        }

        [TestCaseSource(nameof(OrderObjData))]
        public void MapToEsOrderByFields_Order_ReturnsExpectedResult(
            OrderObj order,
            AssetListEsOrderingResult expectedResult)
        {
            var input = new AssetListEsOrderingCommonInput();
            var service = new AssetOrderingService(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IGroupRepresentativesExtendedRequestMapper>().Object);

            var result = service.MapToEsOrderByFields(order, null, input);

            result.Should().NotBeNull();
            result.EsOrderByFields.Should().BeEquivalentTo(expectedResult.EsOrderByFields);
            result.Order.Should().BeEquivalentTo(
                expectedResult.Order,
                opts => opts.Excluding(x => x.m_dSlidingWindowStartTimeField));
        }


        [TestCaseSource(nameof(AssetOrdersData))]
        public void MapToEsOrderByFields_Orderings_ReturnsExpectedResult(
            IReadOnlyCollection<AssetOrder> orderings,
            AssetListEsOrderingResult expectedResult)
        {
            var order = new OrderObj { m_eOrderBy = OrderBy.START_DATE, m_eOrderDir = OrderDir.DESC };
            var service = new AssetOrderingService(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IGroupRepresentativesExtendedRequestMapper>().Object);
            var input = new AssetListEsOrderingCommonInput();

            var result = service.MapToEsOrderByFields(order, orderings, input);

            result.Should().NotBeNull();
            result.EsOrderByFields.Should().BeEquivalentTo(expectedResult.EsOrderByFields);
            result.Order.Should().BeEquivalentTo(
                expectedResult.Order,
                opts => opts.Excluding(x => x.m_dSlidingWindowStartTimeField));
        }
        public void MapToChannelEsOrderByFields_OrderBySlidingWindow(
            Channel channel,
            ChannelEsOrderingResult expectedResult)
        {
            var request = new InternalChannelRequest
            {
                order = new OrderObj { m_eOrderBy = OrderBy.NAME, m_eOrderDir = OrderDir.ASC }
            };

            var input = new AssetListEsOrderingCommonInput();
            var service = new AssetOrderingService(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IGroupRepresentativesExtendedRequestMapper>().Object);
            var result = service.MapToChannelEsOrderByFields(request, channel, input);
            result.Should().NotBeNull();
            result.EsOrderByFields.Should().BeEquivalentTo(expectedResult.EsOrderByFields);
            result.Order.Should().BeEquivalentTo(
                expectedResult.Order,
                opts => opts.Excluding(x => x.m_dSlidingWindowStartTimeField));
        }

        [Test]
        public void MapToChannelEsOrderByFields_OrderBySlidingWindowWithGroupBy()
        {
            var expectedResult = new ChannelEsOrderingResult
            {
                EsOrderByFields = new List<IEsOrderByField> { new EsOrderByField(OrderBy.NAME, OrderDir.DESC) },
                Order = new OrderObj { m_eOrderBy = OrderBy.NAME, m_eOrderDir = OrderDir.DESC }
            };

            var request = new InternalChannelRequest
            {
                order = new OrderObj { m_eOrderBy = OrderBy.NAME, m_eOrderDir = OrderDir.DESC },
                searchGroupBy = new SearchAggregationGroupBy
                {
                    groupBy = new List<string> { "meta" }
                }
            };

            var channel = new Channel
            {
                OrderingParameters = new List<AssetOrder>
                {
                    new AssetSlidingWindowOrder { Field = OrderBy.VIEWS, Direction = OrderDir.DESC, SlidingWindowPeriod = 10 }
                }
            };

            var input = new AssetListEsOrderingCommonInput();
            var service = new AssetOrderingService(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IGroupRepresentativesExtendedRequestMapper>().Object);
            var result = service.MapToChannelEsOrderByFields(request, channel, input);
            result.Should().NotBeNull();
            result.EsOrderByFields.Should().BeEquivalentTo(expectedResult.EsOrderByFields);
            result.Order.Should().BeEquivalentTo(
                expectedResult.Order,
                opts => opts.Excluding(x => x.m_dSlidingWindowStartTimeField));
        }

        [Test]
        public void MapToChannelEsOrderByFields_OrderFromRequestIsPrioritized()
        {
            var expectedResult = new ChannelEsOrderingResult
            {
                EsOrderByFields = new List<IEsOrderByField> { new EsOrderByField(OrderBy.NAME, OrderDir.DESC) },
                Order = new OrderObj { m_eOrderBy = OrderBy.NAME, m_eOrderDir = OrderDir.DESC }
            };

            var request = new InternalChannelRequest
            {
                order = new OrderObj { m_eOrderBy = OrderBy.NAME, m_eOrderDir = OrderDir.DESC },
            };

            var channel = new Channel
            {
                OrderingParameters = new List<AssetOrder>
                {
                    new AssetOrder { Field = OrderBy.START_DATE, Direction = OrderDir.ASC }
                }
            };

            var input = new AssetListEsOrderingCommonInput();
            var service = new AssetOrderingService(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IGroupRepresentativesExtendedRequestMapper>().Object);
            var result = service.MapToChannelEsOrderByFields(request, channel, input);
            result.Should().NotBeNull();
            result.EsOrderByFields.Should().BeEquivalentTo(expectedResult.EsOrderByFields);
            result.Order.Should().BeEquivalentTo(
                expectedResult.Order,
                opts => opts.Excluding(x => x.m_dSlidingWindowStartTimeField));
        }

        [Test]
        public void MapToChannelEsOrderByFields_OrderFromChannelIsDefault()
        {
            var expectedResult = new ChannelEsOrderingResult
            {
                EsOrderByFields = new List<IEsOrderByField> { new EsOrderByField(OrderBy.START_DATE, OrderDir.ASC) },
                Order = new OrderObj { m_eOrderBy = OrderBy.START_DATE, m_eOrderDir = OrderDir.ASC }
            };

            var request = new InternalChannelRequest();
            var channel = new Channel
            {
                OrderingParameters = new List<AssetOrder>
                {
                    new AssetOrder { Field = OrderBy.START_DATE, Direction = OrderDir.ASC }
                }
            };

            var input = new AssetListEsOrderingCommonInput();
            var service = new AssetOrderingService(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IGroupRepresentativesExtendedRequestMapper>().Object);
            var result = service.MapToChannelEsOrderByFields(request, channel, input);
            result.Should().NotBeNull();
            result.EsOrderByFields.Should().BeEquivalentTo(expectedResult.EsOrderByFields);
            result.Order.Should().BeEquivalentTo(
                expectedResult.Order,
                opts => opts.Excluding(x => x.m_dSlidingWindowStartTimeField));
        }

        [TestCaseSource(nameof(ManualChannelData))]
        public void MapToChannelEsOrderByFields_OrderForManualChannel(Channel channel)
        {
            var expectedResult = new ChannelEsOrderingResult
            {
                Order = new OrderObj { m_eOrderBy = OrderBy.RECOMMENDATION, m_eOrderDir = OrderDir.DESC },
                EsOrderByFields = new IEsOrderByField[] { new EsOrderByField(OrderBy.RECOMMENDATION, OrderDir.DESC) },
                SpecificOrder = new long[] { 3, 2, 1 }
            };

            var request = new InternalChannelRequest();

            var input = new AssetListEsOrderingCommonInput();
            var service = new AssetOrderingService(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IGroupRepresentativesExtendedRequestMapper>().Object);
            var result = service.MapToChannelEsOrderByFields(request, channel, input);
            result.Should().NotBeNull();
            result.EsOrderByFields.Should().BeEquivalentTo(expectedResult.EsOrderByFields);
            result.Order.Should().BeEquivalentTo(
                expectedResult.Order,
                opts => opts.Excluding(x => x.m_dSlidingWindowStartTimeField));
        }

        [Test]
        public void MapToChannelEsOrderByFields_OrderForManualChannelWithSegmentOrdering()
        {
            var expectedResult = new ChannelEsOrderingResult
            {
                Order = new OrderObj { m_eOrderBy = OrderBy.RELATED, m_eOrderDir = OrderDir.DESC },
                EsOrderByFields = new IEsOrderByField[]
                {
                    new EsOrderByField(OrderBy.RELATED, OrderDir.DESC),
                    new EsOrderByField(OrderBy.RECOMMENDATION, OrderDir.DESC)
                },
                SpecificOrder = new long[] { 3, 2, 1 }
            };

            var request = new InternalChannelRequest();
            var channel = new Channel
            {
                m_nChannelTypeID = (int)ChannelType.Manual,
                SupportSegmentBasedOrdering = true,
                ManualAssets = new List<ManualAsset>
                {
                    new ManualAsset { AssetId = 1, OrderNum = 3 },
                    new ManualAsset { AssetId = 2, OrderNum = 2 },
                    new ManualAsset { AssetId = 3, OrderNum = 1 }
                },
                OrderingParameters = new List<AssetOrder>
                {
                    new AssetOrder { Field = OrderBy.ID, Direction = OrderDir.ASC }
                }
            };

            var input = new AssetListEsOrderingCommonInput();
            var service = new AssetOrderingService(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IGroupRepresentativesExtendedRequestMapper>().Object);
            var result = service.MapToChannelEsOrderByFields(request, channel, input);
            result.Should().NotBeNull();
            result.EsOrderByFields.Should().BeEquivalentTo(expectedResult.EsOrderByFields);
            result.Order.Should().BeEquivalentTo(
                expectedResult.Order,
                opts => opts.Excluding(x => x.m_dSlidingWindowStartTimeField));
        }

        [Test]
        public void MapToEsOrderByFields_MediaRelatedRequest_ReturnsStartDate()
        {
            var expectedResult = new AssetListEsOrderingResult
            {
                Order = new OrderObj { m_eOrderBy = OrderBy.START_DATE, m_eOrderDir = OrderDir.DESC },
                EsOrderByFields = new[] { new EsOrderByStartDateAndAssociationTags(OrderDir.DESC) }
            };

            var request = new MediaRelatedRequest
            {
                OrderObj = new OrderObj
                {
                    m_eOrderBy = OrderBy.START_DATE,
                    m_eOrderDir = OrderDir.DESC
                }
            };

            var input = new AssetListEsOrderingCommonInput
            {
                ShouldSearchMedia = true,
                ParentMediaTypes = new Dictionary<int, int> { { 1, 1 } },
                AssociationTags = new Dictionary<int, string> { { 1, "1" } }
            };

            var service = new AssetOrderingService(
                _mockRepository.Create<ICatalogManager>().Object,
                _mockRepository.Create<IGroupRepresentativesExtendedRequestMapper>().Object);
            var result = service.MapToEsOrderByFields(request, input);

            result.Should().NotBeNull();
            result.EsOrderByFields.Should().BeEquivalentTo(expectedResult.EsOrderByFields);
            result.Order.Should().BeEquivalentTo(
                expectedResult.Order,
                opts => opts.Excluding(x => x.m_dSlidingWindowStartTimeField));
        }

        private static IEnumerable<TestCaseData> OrderObjData()
        {
            var trendingAssetWindow = DateTime.UtcNow.AddDays(-1);
            const int slidingWindow = 1;
            yield return new TestCaseData(
                new OrderObj(),
                new AssetListEsOrderingResult
                {
                    Order = new OrderObj { m_eOrderBy = OrderBy.ID, m_eOrderDir = OrderDir.DESC },
                    EsOrderByFields = new[] { new EsOrderByField(OrderBy.ID, OrderDir.DESC) }
                });
            yield return new TestCaseData(
                new OrderObj
                {
                    m_eOrderBy = OrderBy.VIEWS, m_eOrderDir = OrderDir.DESC, trendingAssetWindow = trendingAssetWindow
                },
                new AssetListEsOrderingResult
                {
                    Order = new OrderObj { m_eOrderBy = OrderBy.VIEWS, m_eOrderDir = OrderDir.DESC, trendingAssetWindow = trendingAssetWindow },
                    EsOrderByFields = new[] { new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, trendingAssetWindow) }
                });
            yield return new TestCaseData(
                new OrderObj
                {
                    m_eOrderBy = OrderBy.LIKE_COUNTER, m_eOrderDir = OrderDir.ASC
                },
                new AssetListEsOrderingResult
                {
                    Order = new OrderObj { m_eOrderBy = OrderBy.LIKE_COUNTER, m_eOrderDir = OrderDir.ASC },
                    EsOrderByFields = new[] { new EsOrderByStatisticsField(OrderBy.LIKE_COUNTER, OrderDir.ASC, null) }
                });
            yield return new TestCaseData(
                new OrderObj
                {
                    m_eOrderBy = OrderBy.RATING, m_eOrderDir = OrderDir.DESC, m_bIsSlidingWindowField = true, lu_min_period_id = slidingWindow
                },
                new AssetListEsOrderingResult
                {
                    Order = new OrderObj
                    {
                        m_eOrderBy = OrderBy.RATING, m_eOrderDir = OrderDir.DESC, m_bIsSlidingWindowField = true, lu_min_period_id = slidingWindow
                    },
                    EsOrderByFields = new[] { new EsOrderBySlidingWindow(OrderBy.RATING, OrderDir.DESC, slidingWindow) }
                });
            yield return new TestCaseData(
                new OrderObj
                {
                    m_eOrderBy = OrderBy.VOTES_COUNT, m_eOrderDir = OrderDir.ASC, m_bIsSlidingWindowField = true, lu_min_period_id = slidingWindow
                },
                new AssetListEsOrderingResult
                {
                    Order = new OrderObj
                    {
                        m_eOrderBy = OrderBy.VOTES_COUNT, m_eOrderDir = OrderDir.ASC, m_bIsSlidingWindowField = true, lu_min_period_id = slidingWindow
                    },
                    EsOrderByFields = new[] { new EsOrderBySlidingWindow(OrderBy.VOTES_COUNT, OrderDir.ASC, slidingWindow) }
                });

            var equivalentOrderBy = new[]
            {
                OrderBy.NAME,
                OrderBy.EPG_ID,
                OrderBy.START_DATE,
                OrderBy.CREATE_DATE,
                OrderBy.UPDATE_DATE,
                OrderBy.NONE,
                OrderBy.RELATED
            };

            var increment = 1;
            foreach (var orderBy in equivalentOrderBy)
            {
                var direction = ++increment % 2 == 0 ? OrderDir.ASC : OrderDir.DESC;
                var resultOrderBy = orderBy == OrderBy.NONE ? OrderBy.START_DATE : orderBy;
                yield return new TestCaseData(
                    new OrderObj
                    {
                        m_eOrderBy = orderBy, m_eOrderDir = direction
                    },
                    new AssetListEsOrderingResult
                    {
                        Order = new OrderObj
                        {
                            m_eOrderBy = resultOrderBy,
                            m_eOrderDir = direction
                        },
                        EsOrderByFields = new[] { new EsOrderByField(resultOrderBy, direction) }
                    });
            }
        }

        private static IEnumerable<TestCaseData> OrderByMetaData()
        {
            var typeToShouldPad = new Dictionary<Type, bool>
            {
                { typeof(int), true },
                { typeof(double), true },
                { typeof(long), true },
                { typeof(float), true },
                { typeof(string), false },
                { typeof(DateTime), false },
            };

            foreach (var (type, _) in typeToShouldPad)
            {
                yield return new TestCaseData(false, type, false, null);
            }

            foreach (var (type, shouldPad) in typeToShouldPad)
            {
                yield return new TestCaseData(true, type, shouldPad, new LanguageObj { Code = "eng" });
            }
        }

        private static IEnumerable<TestCaseData> OrderByStartDate()
        {
            var expectedResult = new AssetListEsOrderingResult
            {
                Order = new OrderObj { m_eOrderBy = OrderBy.START_DATE, m_eOrderDir = OrderDir.DESC },
                EsOrderByFields = new[] { new EsOrderByStartDateAndAssociationTags(OrderDir.DESC) }
            };

            yield return new TestCaseData(
                new OrderObj { m_eOrderBy = OrderBy.START_DATE, m_eOrderDir = OrderDir.DESC },
                null,
                expectedResult);
            yield return new TestCaseData(
                null,
                new List<AssetOrder> { new AssetOrder { Field = OrderBy.START_DATE, Direction = OrderDir.DESC } },
                expectedResult);
            yield return new TestCaseData(
                new OrderObj { m_eOrderBy = OrderBy.START_DATE, m_eOrderDir = OrderDir.DESC },
                new List<AssetOrder> { new AssetOrder { Field = OrderBy.START_DATE, Direction = OrderDir.DESC } },
                expectedResult);
        }

        private static IEnumerable<TestCaseData> AssetOrdersData()
        {
            var trendingAssetWindow = DateTime.UtcNow.AddDays(-1);
            const int slidingWindow = 1;
            yield return new TestCaseData(
                new List<AssetOrder>
                {
                    new AssetOrderByStatistics { Field = OrderBy.VIEWS, Direction = OrderDir.DESC, TrendingAssetWindow = trendingAssetWindow }
                },
                new AssetListEsOrderingResult
                {
                    Order = new OrderObj { m_eOrderBy = OrderBy.VIEWS, m_eOrderDir = OrderDir.DESC, trendingAssetWindow = trendingAssetWindow },
                    EsOrderByFields = new[] { new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, trendingAssetWindow) }
                });

            var statisticsOrderBy = new[] { OrderBy.VIEWS, OrderBy.RATING, OrderBy.LIKE_COUNTER, OrderBy.VOTES_COUNT };
            foreach (var orderBy in statisticsOrderBy)
            {
                yield return new TestCaseData(
                    new List<AssetOrder>
                    {
                        new AssetSlidingWindowOrder { Field = orderBy, Direction = OrderDir.DESC, SlidingWindowPeriod = slidingWindow }
                    },
                    new AssetListEsOrderingResult
                    {
                        Order = new OrderObj { m_eOrderBy = orderBy, m_eOrderDir = OrderDir.DESC, m_bIsSlidingWindowField = true, lu_min_period_id = slidingWindow },
                        EsOrderByFields = new[] { new EsOrderBySlidingWindow(orderBy, OrderDir.DESC, slidingWindow) }
                    });
                yield return new TestCaseData(
                    new List<AssetOrder>
                    {
                        new AssetOrderByStatistics { Field = orderBy, Direction = OrderDir.DESC, TrendingAssetWindow = trendingAssetWindow }
                    },
                    new AssetListEsOrderingResult
                    {
                        Order = new OrderObj { m_eOrderBy = orderBy, m_eOrderDir = OrderDir.DESC, trendingAssetWindow = trendingAssetWindow },
                        EsOrderByFields = new[] { new EsOrderByStatisticsField(orderBy, OrderDir.DESC, trendingAssetWindow) }
                    });
            }

            var basicOrderBy = new[]
            {
                OrderBy.NAME,
                OrderBy.EPG_ID,
                OrderBy.START_DATE,
                OrderBy.CREATE_DATE,
                OrderBy.UPDATE_DATE,
                OrderBy.NONE,
                OrderBy.RELATED,
                OrderBy.RECOMMENDATION
            };

            foreach (var orderBy in basicOrderBy)
            {
                yield return new TestCaseData(
                    new List<AssetOrder>
                    {
                        new AssetOrder { Field = orderBy, Direction = OrderDir.ASC }
                    },
                    new AssetListEsOrderingResult
                    {
                        Order = new OrderObj { m_eOrderBy = orderBy, m_eOrderDir = OrderDir.ASC },
                        EsOrderByFields = new[] { new EsOrderByField(orderBy, OrderDir.ASC) }
                    });
            }

            yield return new TestCaseData(
                new List<AssetOrder>
                {
                    new AssetOrderByStatistics { Field = OrderBy.VIEWS, Direction = OrderDir.DESC },
                    new AssetOrder { Field = OrderBy.NAME, Direction = OrderDir.ASC }
                },
                new AssetListEsOrderingResult
                {
                    Order = new OrderObj { m_eOrderBy = OrderBy.VIEWS, m_eOrderDir = OrderDir.DESC },
                    EsOrderByFields = new IEsOrderByField[]
                    {
                        new EsOrderByStatisticsField(OrderBy.VIEWS, OrderDir.DESC, null),
                        new EsOrderByField(OrderBy.NAME, OrderDir.ASC)
                    }
                });
        }
        private static IEnumerable<TestCaseData> ManualChannelData()
        {
            yield return new TestCaseData(
                new Channel
                {
                    m_nChannelTypeID = (int)ChannelType.Manual,
                    ManualAssets = new List<ManualAsset>
                    {
                        new ManualAsset { AssetId = 1, OrderNum = 3 },
                        new ManualAsset { AssetId = 2, OrderNum = 2 },
                        new ManualAsset { AssetId = 3, OrderNum = 1 }
                    },
                    OrderingParameters = new List<AssetOrder>
                    {
                        new AssetOrder { Field = OrderBy.ID, Direction = OrderDir.ASC }
                    }
                });

            yield return new TestCaseData(
                new Channel
                {
                    m_nChannelTypeID = (int)ChannelType.Manual,
                    m_lManualMedias = new List<ManualMedia>
                    {
                        new ManualMedia("1", 3),
                        new ManualMedia("2", 2),
                        new ManualMedia("3", 1),
                    },
                    OrderingParameters = new List<AssetOrder>
                    {
                        new AssetOrder { Field = OrderBy.ID, Direction = OrderDir.ASC }
                    }
                });
        }
    }
}