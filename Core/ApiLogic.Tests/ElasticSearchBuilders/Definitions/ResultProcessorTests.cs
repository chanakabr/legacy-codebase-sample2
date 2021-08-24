using System.Collections.Generic;
using ApiLogic.Catalog.Tree;
using ApiObjects;
using FluentAssertions;
using NUnit.Framework;

namespace ApiLogic.Tests.ElasticSearchBuilders.Definitions
{
    [TestFixture]
    public class ResultProcessorTests
    {
        [Test]
        [TestCaseSource(nameof(ProcessResultsTestCases))]
        public void ProcessResults_Success(eCutType operand, IndexesModel[] results, IndexesModel expectedResult)
        {
            var resultProcessor = new FilterTreeResultProcessor();
            var result = resultProcessor.ProcessResults(operand, results);
            
            result.Should().BeEquivalentTo(expectedResult);
        }

        private static IEnumerable<object> ProcessResultsTestCases()
        {
            // Common or Epg => Common + Epg
            var operand1 = eCutType.Or;
            var results1 = new[]
            {
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Common
                },
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Epg
                }
            };
            var expectedResult1 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Common | ElasticSearchIndexes.Epg
            };
            yield return new TestCaseData(operand1, results1, expectedResult1);
            
            // Common or Media => Common + Media
            var operand2 = eCutType.Or;
            var results2 = new[]
            {
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Common
                },
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Media
                }
            };
            var expectedResult2 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Common | ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(operand2, results2, expectedResult2);
            
            // Common or Media or Epg => Common + Epg + Media
            var operand3 = eCutType.Or;
            var results3 = new[]
            {
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Common
                },
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Media
                },
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Epg
                }
            };
            var expectedResult3 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Common | ElasticSearchIndexes.Media | ElasticSearchIndexes.Epg
            };
            yield return new TestCaseData(operand3, results3, expectedResult3);
            
            // Common and Media => Media
            var operand4 = eCutType.And;
            var results4 = new[]
            {
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Common
                },
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Media
                }
            };
            var expectedResult4 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(operand4, results4, expectedResult4);
            
            // Common and Epg => Epg
            var operand5 = eCutType.And;
            var results5 = new[]
            {
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Common
                },
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Epg
                }
            };
            var expectedResult5 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Epg
            };
            yield return new TestCaseData(operand5, results5, expectedResult5);
            
            // Media and Epg => Media + Epg
            var operand6 = eCutType.And;
            var results6 = new[]
            {
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Media
                },
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Epg
                }
            };
            var expectedResult6 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Epg | ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(operand6, results6, expectedResult6);
            
            // Common and Media and Epg => Common + Media + Epg
            var operand7 = eCutType.And;
            var results7 = new[]
            {
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Media
                },
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Epg
                },
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Common
                }
            };
            var expectedResult7 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Epg | ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(operand7, results7, expectedResult7);
            
            // Media or Media => Media
            var operand8 = eCutType.Or;
            var results8 = new[]
            {
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Media
                },
                new IndexesModel
                {
                    Indexes = ElasticSearchIndexes.Media
                }
            };
            var expectedResult8 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(operand8, results8, expectedResult8);
        }
    }
}