using ApiLogic.Catalog;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using ConfigurationManager;
using Core.Catalog;
using Core.Catalog.Cache;
using Core.Catalog.CatalogManagement;
using ElasticSearch.Common;
using GroupsCacheManager;
using JsonDiffPatchDotNet;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TVinciShared;
using OrderDir = ApiObjects.SearchObjects.OrderDir;

namespace ApiLogic.Tests.IndexManager
{
    [TestFixture]
    public class ElasticSearchWrapperTests
    {
        private readonly JsonDiffPatch jdp = new JsonDiffPatch();

        delegate void MockSendPostHttpReq(string url, ref int status, string userName, string password, string parameters, bool isFirstTry, bool isPut = false);
        
        private MockRepository _mockRepository;
        private Mock<IGroupManager> _mockGroupManager;
        private Mock<ICatalogManager> _mockCatalogManager;
        private Mock<IChannelManager> _mockChannelManager;
        private ESSerializerV2 _mockEsSerializerV2;
        private ElasticSearchIndexDefinitions _elasticSearchIndexDefinitions;
        private Mock<ILayeredCache> _mockLayeredCache;
        private Mock<ICatalogCache> _mockCatalogCache;
        private Mock<IWatchRuleManager> _mockWatchRuleManager;
        private Mock<IApplicationConfiguration> _mockApplicationConfiguration;

        [SetUp]
        public void SetUp()
        {
            ApplicationConfiguration.Current._elasticSearchConfiguration = new MockElasticSearchConfiguration();
            _mockEsSerializerV2 = new ESSerializerV2();
            _mockRepository = new MockRepository(MockBehavior.Loose);
            _mockGroupManager = _mockRepository.Create<IGroupManager>();
            _mockCatalogManager = _mockRepository.Create<ICatalogManager>();
            MockChannelManager = _mockRepository.Create<IChannelManager>();
            _mockCatalogCache = _mockRepository.Create<ICatalogCache>();
            _mockLayeredCache = _mockRepository.Create<ILayeredCache>();
            _mockWatchRuleManager = _mockRepository.Create<IWatchRuleManager>();
            _mockApplicationConfiguration = _mockRepository.Create<IApplicationConfiguration>();
            _elasticSearchIndexDefinitions = new ElasticSearchIndexDefinitions(ElasticSearch.Common.Utils.Instance, _mockApplicationConfiguration.Object);
        }


        [TestCaseSource(nameof(TestCases))]
        public void ShouldGenerateCorrectRequestAndFilterResponse(
            OrderBy orderBy,
            OrderDir orderDirection,
            int pageIndex,
            int pageSize,
            string expectedRequest,
            string response,
            int expectedTotalCount,
            string[] expectedAssets)
        {
            var parentGroupId = 1;

            // TODO think ohw to change this test since now ES API should be hidden form logic 
            var clientMock = new Mock<IElasticSearchApi>();
            clientMock.Setup(x => x.baseUrl).Returns("http://elasticsearch.service.consul:9200");

            var elasticSearchWrapper = new IndexManagerV2(parentGroupId,
                clientMock.Object,
                _mockGroupManager.Object,
                _mockEsSerializerV2,
                _mockCatalogManager.Object,
                _elasticSearchIndexDefinitions,
                _mockLayeredCache.Object,
                _mockChannelManager.Object,
                _mockCatalogCache.Object,
                _mockWatchRuleManager.Object
                );

            var groupBy = KeyValuePair.Create("content_reference_id", "content_reference_id.name.in.elastic");
            var unifiedSearchDefinitions = new UnifiedSearchDefinitions
            {
                groupId = 2,
                groupBy = new List<KeyValuePair<string, string>>() { groupBy },
                distinctGroup = groupBy,
                topHitsCount = 3, // ignored, 1 will be used
                order = new OrderObj {
                    m_eOrderBy = orderBy,
                    m_eOrderDir = orderDirection,
                    m_sOrderValue = orderBy == OrderBy.META ? "Series_Name" : string.Empty
                },
                pageIndex = pageIndex, // starts from 0
                pageSize = pageSize,
                //from = 100, // TODO test
                groupByOrder = AggregationOrder.Value_Asc, // not important
                shouldSearchMedia = true
            };

            clientMock.Setup(x => x.SendPostHttpReq(It.IsAny<string>(), ref It.Ref<int>.IsAny, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true, false))
              .Callback(new MockSendPostHttpReq((string url, ref int status, string userName, string password, string actualRequest, bool isFirstTry, bool isPut) =>
              {
                  status = ElasticSearchApi.STATUS_OK;

                  // ASSERT that correct request was generated
                  Assert.That(url, Is.EqualTo($"http://elasticsearch.service.consul:9200/{parentGroupId}/media/_search"));
                  var diff = jdp.Diff(actualRequest, expectedRequest);
                  Assert.That(diff, Is.Null);
              }))
              .Returns(response);

            using var overwriteTime = SystemDateTime.UtcNowIs(new DateTime(2020, 11, 11));

            var aggregationsResult = elasticSearchWrapper.UnifiedSearchForGroupBy(unifiedSearchDefinitions);

            Assert.That(aggregationsResult, Is.Not.Null);
            Assert.That(aggregationsResult.totalItems, Is.EqualTo(expectedTotalCount));
            Assert.That(aggregationsResult.field, Is.EqualTo(groupBy.Key));
            Assert.That(aggregationsResult.results.Select(_ => _.topHits.Single().AssetId).ToArray(), Is.EqualTo(expectedAssets));
        }

        private static IEnumerable TestCases()
        {
            // numeric
            yield return new TestCaseData(OrderBy.CREATE_DATE, OrderDir.DESC, 1, 3, CreateDateRequest(6), createDateDescResponse, 9505, new[] { "654869", "654867", "654865" }).SetName("CREATE_DATE_DESC");
            yield return new TestCaseData(OrderBy.CREATE_DATE, OrderDir.DESC, 1, 5, CreateDateRequest(10), createDateDescResponse, 9505, new[] { "654865" }).SetName("CREATE_DATE_DESC last page");
            yield return new TestCaseData(OrderBy.CREATE_DATE, OrderDir.DESC, 2, 5, CreateDateRequest(15), createDateDescResponse, 9505, new string[0]).SetName("CREATE_DATE_DESC next page after the last one");
            // non-numeric
            yield return new TestCaseData(OrderBy.NAME, OrderDir.ASC, 1, 3, NonNumericRequest("name", "asc"), nonNumericResponse, 9432, new[] { "626357", "624099", "626363" }).SetName("NAME_ASC");
            yield return new TestCaseData(OrderBy.NAME, OrderDir.DESC, 1, 3, NonNumericRequest("name", "desc"), nonNumericResponse, 9432, new[] { "620834", "626363", "624099" }).SetName("NAME_DESC");
            yield return new TestCaseData(OrderBy.NAME, OrderDir.DESC, 1, 8, NonNumericRequest("name", "desc"), nonNumericResponse, 9432, new[] { "564941", "565058" }).SetName("NAME_DESC last page");
            yield return new TestCaseData(OrderBy.NAME, OrderDir.DESC, 2, 8, NonNumericRequest("name", "desc"), nonNumericResponse, 9432, new string[0]).SetName("NAME_DESC next page after the last one");
            yield return new TestCaseData(OrderBy.META, OrderDir.ASC, 1, 5, NonNumericRequest("metas.series_name", "asc", true), nonNumericResponse, 9432, new[] { "565058", "564941", "639231", "624099", "624327" }).SetName("META_ASC");
            yield return new TestCaseData(OrderBy.META, OrderDir.DESC, 1, 2, NonNumericRequest("metas.series_name", "desc", true), nonNumericResponse, 9432, new[] { "639231", "564941" }).SetName("META_DESC");
            // empty response
            yield return new TestCaseData(OrderBy.NAME, OrderDir.DESC, 2, 8, NonNumericRequest("name", "desc"), emptyResponse, 0, new string[0]).SetName("empty response");
        }

        #region Raw requests & responses

        private static string CreateDateRequest(int requestSize) { return createDateDescRequest.Replace("$REQUEST_SIZE", requestSize.ToString()); }
        private const string createDateDescRequest = @"{
          ""size"": 0,
          ""from"": 0,
          ""fields"": [
            ""_id"",
            ""_index"",
            ""_type"",
            ""_score"",
            ""group_id"",
            ""name"",
            ""cache_date"",
            ""update_date"",
            ""media_id""
          ],
          ""sort"": [
            {
              ""create_date"": {
                ""order"": ""desc""
              }
            },
            {
              ""_score"": ""desc""
            },
            {
              ""_uid"": {
                ""order"": ""desc""
              }
            }
          ],
          ""aggs"": {
            ""content_reference_id_count"": {
              ""cardinality"": {
                ""field"": ""content_reference_id.name.in.elastic""
              }
            },
            ""content_reference_id"": {
              ""aggregations"": {
                ""top_hits_assets"": {
                  ""top_hits"": {
                    ""sort"": [
                      {
                        ""create_date"": {
                          ""order"": ""desc""
                        }
                      },
                      {
                        ""_score"": ""desc""
                      },
                      {
                        ""_uid"": {
                          ""order"": ""desc""
                        }
                      }
                    ],
                    ""size"": 1,
                    ""_source"": {
                      ""includes"": [
                        ""_id"",
                        ""_index"",
                        ""_type"",
                        ""_score"",
                        ""group_id"",
                        ""name"",
                        ""cache_date"",
                        ""update_date"",
                        ""media_id""
                      ]
                    }
                  }
                },
                ""order_aggregation"": {
                  ""max"": {
                    ""field"": ""create_date""
                  }
                }
              },
              ""terms"": {
                ""field"": ""content_reference_id.name.in.elastic"",
                ""order"": {
                  ""order_aggregation"": ""desc""
                },
                ""size"": $REQUEST_SIZE
              }
            }
          }, " + queryPart + "}";

        private const string createDateDescResponse = @"{
          ""took"": 7913,
          ""timed_out"": false,
          ""_shards"": {
            ""total"": 4,
            ""successful"": 4,
            ""failed"": 0
          },
          ""hits"": {
            ""total"": 10079,
            ""max_score"": 0.0,
            ""hits"": []
          },
          ""aggregations"": {
            ""content_reference_id_count"": {
              ""value"": 9505
            },
            ""content_reference_id"": {
              ""doc_count_error_upper_bound"": -1,
              ""sum_other_doc_count"": 10073,
              ""buckets"": [
                {
                  ""key"": ""start_62086dca-dac0-495c-87d4-7575860836a5"",
                  ""doc_count"": 1,
                  ""order_aggregation"": {
                    ""value"": 1.568698593E+12,
                    ""value_as_string"": ""20190917053633""
                  },
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 1,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""654874"",
                          ""_score"": null,
                          ""_source"": {
                            ""group_id"": 479,
                            ""name"": ""Юху и его друзья_Сезон-2_Серия-84"",
                            ""media_id"": 654874,
                            ""update_date"": ""20190917053633"",
                            ""cache_date"": ""20200705114259""
                          },
                          ""sort"": [
                            1568698593000,
                            1.0,
                            ""media#654874""
                          ]
                        }
                      ]
                    }
                  }
                },
                {
                  ""key"": ""start_638e1f3a-5d42-4534-bde8-f8dab219a2fb"",
                  ""doc_count"": 1,
                  ""order_aggregation"": {
                    ""value"": 1.568698581E+12,
                    ""value_as_string"": ""20190917053621""
                  },
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 1,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""654872"",
                          ""_score"": null,
                          ""_source"": {
                            ""group_id"": 479,
                            ""name"": ""Юху и его друзья_Сезон-2_Серия-92"",
                            ""media_id"": 654872,
                            ""update_date"": ""20190917053621"",
                            ""cache_date"": ""20200705114259""
                          },
                          ""sort"": [
                            1568698581000,
                            1.0,
                            ""media#654872""
                          ]
                        }
                      ]
                    }
                  }
                },
                {
                  ""key"": ""start_66ea7b77-6db9-4954-b6b7-d478a5fd715d"",
                  ""doc_count"": 1,
                  ""order_aggregation"": {
                    ""value"": 1.568698575E+12,
                    ""value_as_string"": ""20190917053615""
                  },
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 1,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""654871"",
                          ""_score"": null,
                          ""_source"": {
                            ""group_id"": 479,
                            ""name"": ""Юху и его друзья_Сезон-2_Серия-60"",
                            ""media_id"": 654871,
                            ""update_date"": ""20190917053615"",
                            ""cache_date"": ""20200705114259""
                          },
                          ""sort"": [
                            1568698575000,
                            1.0,
                            ""media#654871""
                          ]
                        }
                      ]
                    }
                  }
                },
                {
                  ""key"": ""start_8ed4b316-9848-40ec-98c7-364c2e5d6cf5"",
                  ""doc_count"": 1,
                  ""order_aggregation"": {
                    ""value"": 1.568698563E+12,
                    ""value_as_string"": ""20190917053603""
                  },
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 1,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""654869"",
                          ""_score"": null,
                          ""_source"": {
                            ""group_id"": 479,
                            ""name"": ""Юху и его друзья_Сезон-2_Серия-83"",
                            ""media_id"": 654869,
                            ""update_date"": ""20190917053603"",
                            ""cache_date"": ""20200705114259""
                          },
                          ""sort"": [
                            1568698563000,
                            1.0,
                            ""media#654869""
                          ]
                        }
                      ]
                    }
                  }
                },
                {
                  ""key"": ""start_c2d02f3d-2cf5-4f9a-9027-6196d82fc7d1"",
                  ""doc_count"": 1,
                  ""order_aggregation"": {
                    ""value"": 1.568698551E+12,
                    ""value_as_string"": ""20190917053551""
                  },
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 1,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""654867"",
                          ""_score"": null,
                          ""_source"": {
                            ""group_id"": 479,
                            ""name"": ""Юху и его друзья_Сезон-2_Серия-53"",
                            ""media_id"": 654867,
                            ""update_date"": ""20190917053551"",
                            ""cache_date"": ""20200705114259""
                          },
                          ""sort"": [
                            1568698551000,
                            1.0,
                            ""media#654867""
                          ]
                        }
                      ]
                    }
                  }
                },
                {
                  ""key"": ""start_510315c2-654b-42b0-b743-cd146252e720"",
                  ""doc_count"": 1,
                  ""order_aggregation"": {
                    ""value"": 1.568698539E+12,
                    ""value_as_string"": ""20190917053539""
                  },
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 1,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""654865"",
                          ""_score"": null,
                          ""_source"": {
                            ""group_id"": 479,
                            ""name"": ""Юху и его друзья_Сезон-2_Серия-55"",
                            ""media_id"": 654865,
                            ""update_date"": ""20190917053538"",
                            ""cache_date"": ""20200705114259""
                          },
                          ""sort"": [
                            1568698539000,
                            1.0,
                            ""media#654865""
                          ]
                        }
                      ]
                    }
                  }
                }
              ]
            }
          }
        }";

        private static string NonNumericRequest(string orderField, string orderDirection, bool includeToReturn = false)
        {
            return nonNumericRequest
                .Replace("$ORDER_DIRECTION", orderDirection)
                .Replace("$ORDER_FIELD", orderField)
                .Replace("$EXTRA_FIELD", includeToReturn ? $",\"{orderField}\"" : string.Empty);
        }
        private const string nonNumericRequest = @"{
          ""size"": 0,
          ""from"": 0,
          ""fields"": [
            ""_id"",
            ""_index"",
            ""_type"",
            ""_score"",
            ""group_id"",
            ""name"",
            ""cache_date"",
            ""update_date"",
            ""media_id""
          ],
          ""sort"": [
            {
              ""$ORDER_FIELD"": {
                ""order"": ""$ORDER_DIRECTION""
              }
            },
            {
              ""_score"": ""desc""
            },
            {
              ""_uid"": {
                ""order"": ""desc""
              }
            }
          ],
          ""aggs"": {
            ""content_reference_id_count"": {
              ""cardinality"": {
                ""field"": ""content_reference_id.name.in.elastic""
              }
            },
            ""content_reference_id"": {
              ""aggregations"": {
                ""top_hits_assets"": {
                  ""top_hits"": {
                    ""sort"": [
                      {
                        ""$ORDER_FIELD"": {
                          ""order"": ""$ORDER_DIRECTION""
                        }
                      },
                      {
                        ""_score"": ""desc""
                      },
                      {
                        ""_uid"": {
                          ""order"": ""desc""
                        }
                      }
                    ],
                    ""size"": 1,
                    ""_source"": {
                      ""includes"": [
                        ""_id"",
                        ""_index"",
                        ""_type"",
                        ""_score"",
                        ""group_id"",
                        ""name"",
                        ""cache_date"",
                        ""update_date"",
                        ""media_id"" $EXTRA_FIELD
                      ]
                    }
                  }
                }
              },
              ""terms"": {
                ""field"": ""content_reference_id.name.in.elastic"",
                ""order"": {
                  ""_term"": ""asc""
                },
                ""size"": 100000
              }
            }
          }, " + queryPart + "}";

        private const string nonNumericResponse = @"{
          ""took"": 189,
          ""timed_out"": false,
          ""_shards"": {
            ""total"": 4,
            ""successful"": 4,
            ""failed"": 0
          },
          ""hits"": {
            ""total"": 10079,
            ""max_score"": 0.0,
            ""hits"": []
          },
          ""aggregations"": {
            ""content_reference_id_count"": {
              ""value"": 9432
            },
            ""content_reference_id"": {
              ""doc_count_error_upper_bound"": 0,
              ""sum_other_doc_count"": 10066,
              ""buckets"": [
                {
                  ""key"": ""1000359"",
                  ""doc_count"": 1,
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 1,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""565058"",
                          ""_score"": null,
                          ""_source"": {
                            ""metas"": {
                              ""series_name"": ""Again Batman""
                            },
                            ""group_id"": 479,
                            ""name"": ""Batman"",
                            ""media_id"": 565058,
                            ""update_date"": ""20190307223824"",
                            ""cache_date"": ""20200705114252""
                          },
                          ""sort"": [
                            ""batman"",
                            1.0,
                            ""media#565058""
                          ]
                        }
                      ]
                    }
                  }
                },        
                {
                  ""key"": ""1002146_1235993"",
                  ""doc_count"": 4,
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 4,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""592366"",
                          ""_score"": null,
                          ""_source"": {
                            ""group_id"": 479,
                            ""name"": ""הערת שוליים"",
                            ""media_id"": 592366,
                            ""update_date"": ""20190530151652"",
                            ""cache_date"": ""20200705114253""
                          },
                          ""sort"": [
                            ""הערת שוליים"",
                            1.0,
                            ""media#592366""
                          ]
                        }
                      ]
                    }
                  }
                },
                {
                  ""key"": ""1000482"",
                  ""doc_count"": 1,
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 1,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""564941"",
                          ""_score"": null,
                          ""_source"": {
                            ""metas"": {
                              ""series_name"": ""Forever spiderman""
                            },
                            ""group_id"": 479,
                            ""name"": ""Spiderman. New one"",
                            ""media_id"": 564941,
                            ""update_date"": ""20190314232236"",
                            ""cache_date"": ""20200705114252""
                          },
                          ""sort"": [
                            ""spiderman. new one"",
                            1.0,
                            ""media#564941""
                          ]
                        }
                      ]
                    }
                  }
                },
                {
                  ""key"": ""1002168"",
                  ""doc_count"": 1,
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 1,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""639231"",
                          ""_score"": null,
                          ""_source"": {
                            ""metas"": {
                              ""series_name"": ""Вчера. Сегодня. Навсегда.""
                            },
                            ""group_id"": 479,
                            ""name"": ""Удиви меня"",
                            ""media_id"": 639231,
                            ""update_date"": ""20190831200939"",
                            ""cache_date"": ""20200705114257""
                          },
                          ""sort"": [
                            ""удиви меня"",
                            1.0,
                            ""media#639231""
                          ]
                        }
                      ]
                    }
                  }
                },
                {
                  ""key"": ""1002279"",
                  ""doc_count"": 1,
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 1,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""624327"",
                          ""_score"": null,
                          ""_source"": {
                            ""metas"": {
                              ""series_name"": ""Сегодня. Навсегда.""
                            },
                            ""group_id"": 479,
                            ""name"": ""Время грехов"",
                            ""media_id"": 624327,
                            ""update_date"": ""20190426092640"",
                            ""cache_date"": ""20200705114256""
                          },
                          ""sort"": [
                            ""время грехов"",
                            1.0,
                            ""media#624327""
                          ]
                        }
                      ]
                    }
                  }
                },
                {
                  ""key"": ""1004175"",
                  ""doc_count"": 1,
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 1,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""624099"",
                          ""_score"": null,
                          ""_source"": {
                            ""metas"": {
                              ""series_name"": ""Навсегда.""
                            },
                            ""group_id"": 479,
                            ""name"": ""Неидеальная женщина"",
                            ""media_id"": 624099,
                            ""update_date"": ""20190426052151"",
                            ""cache_date"": ""20200705114256""
                          },
                          ""sort"": [
                            ""неидеальная женщина"",
                            1.0,
                            ""media#624099""
                          ]
                        }
                      ]
                    }
                  }
                },
                {
                  ""key"": ""1007722"",
                  ""doc_count"": 1,
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 1,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""633511"",
                          ""_score"": null,
                          ""_source"": {
                            ""group_id"": 479,
                            ""name"": ""את לי לילה"",
                            ""media_id"": 633511,
                            ""update_date"": ""20190724101330"",
                            ""cache_date"": ""20200705114257""
                          },
                          ""sort"": [
                            ""את לי לילה"",
                            1.0,
                            ""media#633511""
                          ]
                        }
                      ]
                    }
                  }
                },
                {
                  ""key"": ""1010926"",
                  ""doc_count"": 1,
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 1,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""620834"",
                          ""_score"": null,
                          ""_source"": {
                            ""group_id"": 479,
                            ""name"": ""Стиляги"",
                            ""media_id"": 620834,
                            ""update_date"": ""20190425201456"",
                            ""cache_date"": ""20200705114256""
                          },
                          ""sort"": [
                            ""стиляги"",
                            1.0,
                            ""media#620834""
                          ]
                        }
                      ]
                    }
                  }
                },
                {
                  ""key"": ""1012663"",
                  ""doc_count"": 1,
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 1,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""626363"",
                          ""_score"": null,
                          ""_source"": {
                            ""group_id"": 479,
                            ""name"": ""Перед рассветом"",
                            ""media_id"": 626363,
                            ""update_date"": ""20190429195647"",
                            ""cache_date"": ""20200705114256""
                          },
                          ""sort"": [
                            ""перед рассветом"",
                            1.0,
                            ""media#626363""
                          ]
                        }
                      ]
                    }
                  }
                },
                {
                  ""key"": ""1013119"",
                  ""doc_count"": 1,
                  ""top_hits_assets"": {
                    ""hits"": {
                      ""total"": 1,
                      ""max_score"": null,
                      ""hits"": [
                        {
                          ""_index"": ""478_20200705114236"",
                          ""_type"": ""media"",
                          ""_id"": ""626357"",
                          ""_score"": null,
                          ""_source"": {
                            ""group_id"": 479,
                            ""name"": ""Казачья быль"",
                            ""media_id"": 626357,
                            ""update_date"": ""20190429195607"",
                            ""cache_date"": ""20200705114256""
                          },
                          ""sort"": [
                            ""казачья быль"",
                            1.0,
                            ""media#626357""
                          ]
                        }
                      ]
                    }
                  }
                }
              ]
            }
          }
        }";

        private const string emptyResponse = @"{""took"":3,""timed_out"":false,""_shards"":{""total"":4,""successful"":4,""failed"":0},""hits"":{""total"":0,""max_score"":0.0,""hits"":[]},""aggregations"":{""content_reference_id_count"":{""value"":0},""content_reference_id"":{""doc_count_error_upper_bound"":0,""sum_other_doc_count"":0,""buckets"":[]}}}";

        private const string queryPart = @"""query"": {
            ""filtered"": {
              ""filter"": {
                ""and"": [
                  {
                    ""or"": [
                      {
                        ""and"": [
                          {
                            ""or"": [
                              {
                                ""term"": {
                                  ""group_id"": {
                                    ""value"": 2
                                  }
                                }
                              }
                            ]
                          },
                          {
                            ""and"": [
                              {
                                ""range"": {
                                  ""start_date"": {
                                    ""gte"": ""00010101000000"",
                                    ""lte"": ""20201111000000""
                                  }
                                }
                              },
                              {
                                ""range"": {
                                  ""end_date"": {
                                    ""gte"": ""20201111000000"",
                                    ""lte"": ""99991231235959""
                                  }
                                }
                              }
                            ]
                          },
                          {
                            ""prefix"": {
                              ""_type"": ""media""
                            }
                          },
                          {
                            ""terms"": {
                              ""user_types"": [
                                0
                              ]
                            }
                          },
                          {
                            ""terms"": {
                              ""device_rule_id"": [
                                0
                              ]
                            }
                          }
                        ]
                      }
                    ]
                  },
                  {
                    ""and"": [
                      {
                        ""term"": {
                          ""is_active"": {
                            ""value"": 1
                          }
                        }
                      }
                    ]
                  }
                ]
              }
            }
          }";

        public Mock<IChannelManager> MockChannelManager { get => _mockChannelManager; set => _mockChannelManager = value; }

        #endregion

        internal class MockElasticSearchConfiguration : ElasticSearchConfiguration
        {
            public MockElasticSearchConfiguration()
            {
                SetActualValue(MaxResults, 100000);
                SetActualValue(URL_V2, "http://elasticsearch.service.consul:9200");
            }
        }
    }
}