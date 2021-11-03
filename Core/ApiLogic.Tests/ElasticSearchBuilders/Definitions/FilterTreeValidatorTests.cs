using System.Collections.Generic;
using System.Linq;
using ApiLogic.Catalog.Tree;
using ApiObjects;
using ApiObjects.SearchObjects;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.ElasticSearchBuilders.Definitions
{
    [TestFixture]
    public class FilterTreeValidatorTests
    {
        private readonly MockRepository _mockRepository = new MockRepository(MockBehavior.Strict);
        private Mock<IFilterTreeResultProcessor> _resultProcessorMock;
        private long? _programAssetStructId;

        [SetUp]
        public void SetUp()
        {
            _resultProcessorMock = _mockRepository.Create<IFilterTreeResultProcessor>();
            _programAssetStructId = 93527484;
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        [TestCaseSource(nameof(GetSourceForTraversalCheck))]
        public void ValidateTree_TraversalCheck(BooleanPhrase tree, IEnumerable<IEnumerable<IndexesModel>> arguments, IEnumerable<eCutType> operands)
        {
            _resultProcessorMock.Setup(x => x.ProcessResults(It.IsAny<eCutType>(), It.IsAny<IEnumerable<IndexesModel>>())).Returns(() => new IndexesModel());

            var filterTreeValidator = new FilterTreeValidator(_resultProcessorMock.Object, _programAssetStructId);
            filterTreeValidator.ValidateTree(tree);

            var models = arguments as IEnumerable<IndexesModel>[] ?? arguments.ToArray();
            var eCutTypes = operands as eCutType[] ?? operands.ToArray();
            if (models.Length != _resultProcessorMock.Invocations.Count || eCutTypes.Length != _resultProcessorMock.Invocations.Count)
            {
                Assert.Fail();
            }

            for (var index = 0; index < _resultProcessorMock.Invocations.Count; index++)
            {
                _resultProcessorMock.Invocations[index].Arguments[0].Should().BeEquivalentTo(eCutTypes[index]);
                _resultProcessorMock.Invocations[index].Arguments[1].Should().BeEquivalentTo(models[index]);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetSourceForValidation))]
        public void ValidateTree_Success(string kSql, IndexesModel expectedResult)
        {
            BooleanPhraseNode tree = null;
            BooleanPhraseNode.ParseSearchExpression(kSql, ref tree);

            var resultProcessor = new FilterTreeResultProcessor();
            var filterTreeValidator = new FilterTreeValidator(resultProcessor, _programAssetStructId);
            var indexesModel = filterTreeValidator.ValidateTree(tree);

            indexesModel.Should().BeEquivalentTo(expectedResult);
        }
        
        [Test]
        [TestCaseSource(nameof(GetSourceForValidationWithTypes))]
        public void ValidateTreeWithMediaTypes_Success(string kSql, int[] mediaTypes, IndexesModel expectedResult)
        {
            BooleanPhraseNode tree = null;
            BooleanPhraseNode.ParseSearchExpression(kSql, ref tree);

            var resultProcessor = new FilterTreeResultProcessor();
            var filterTreeValidator = new FilterTreeValidator(resultProcessor, _programAssetStructId);
            var indexesModel = filterTreeValidator.ValidateTree(tree, mediaTypes);

            indexesModel.Should().BeEquivalentTo(expectedResult);
        }

        #region TestCaseSources

        private static IEnumerable<object> GetSourceForTraversalCheck()
        {
            // (and media_id = '1234' name = '1234')
            var tree1 = new BooleanPhrase
            {
                operand = eCutType.And,
                nodes = new List<BooleanPhraseNode>
                {
                    new BooleanLeaf("media_id", "1234"),
                    new BooleanLeaf("name", "1234")
                }
            };
            var indexes1 = new[]
            {
                new[]
                {
                    new IndexesModel
                    {
                        Indexes = ElasticSearchIndexes.Media
                    },
                    new IndexesModel
                    {
                        Indexes = ElasticSearchIndexes.Common
                    }
                }
            };
            var operands1 = new[] { eCutType.And };
            yield return new TestCaseData(new object[] {tree1, indexes1, operands1});
            
            // (and epg_id = '1234' name = '1234')
            var tree2 = new BooleanPhrase
            {
                operand = eCutType.And,
                nodes = new List<BooleanPhraseNode>
                {
                    new BooleanLeaf("epg_id", "1234"),
                    new BooleanLeaf("name", "1234")
                }
            };
            var indexes2 = new[]
            {
                new[]
                {
                    new IndexesModel
                    {
                        Indexes = ElasticSearchIndexes.Epg
                    },
                    new IndexesModel
                    {
                        Indexes = ElasticSearchIndexes.Common
                    }
                }
            };
            var operands2 = new[] { eCutType.And };
            yield return new TestCaseData(new object[] {tree2, indexes2, operands2});
            
            // (or epg_id = '1234' name = '1234')
            var tree3 = new BooleanPhrase
            {
                operand = eCutType.Or,
                nodes = new List<BooleanPhraseNode>
                {
                    new BooleanLeaf("epg_id", "1234"),
                    new BooleanLeaf("name", "1234")
                }
            };
            var indexes3 = new[]
            {
                new[]
                {
                    new IndexesModel
                    {
                        Indexes = ElasticSearchIndexes.Epg
                    },
                    new IndexesModel
                    {
                        Indexes = ElasticSearchIndexes.Common
                    }
                }
            };
            var operands3 = new[] { eCutType.Or };
            yield return new TestCaseData(new object[] {tree3, indexes3, operands3});
            
            // (or media_id = '1234' name = '1234')
            var tree4 = new BooleanPhrase
            {
                operand = eCutType.Or,
                nodes = new List<BooleanPhraseNode>
                {
                    new BooleanLeaf("media_id", "1234"),
                    new BooleanLeaf("name", "1234")
                }
            };
            var indexes4 = new[]
            {
                new[]
                {
                    new IndexesModel
                    {
                        Indexes = ElasticSearchIndexes.Media
                    },
                    new IndexesModel
                    {
                        Indexes = ElasticSearchIndexes.Common
                    }
                }
            };
            var operands4 = new[] { eCutType.Or };
            yield return new TestCaseData(new object[] {tree4, indexes4, operands4});
            
            // (or media_id = '1234')
            var tree5 = new BooleanPhrase
            {
                operand = eCutType.Or,
                nodes = new List<BooleanPhraseNode>
                {
                    new BooleanLeaf("media_id", "1234")
                }
            };
            var indexes5 = new[]
            {
                new[]
                {
                    new IndexesModel
                    {
                        Indexes = ElasticSearchIndexes.Media
                    }
                }
            };
            var operands5 = new[] { eCutType.Or };
            yield return new TestCaseData(new object[] {tree5, indexes5, operands5});
            
            // (or epg_id = '1234')
            var tree6 = new BooleanPhrase
            {
                operand = eCutType.Or,
                nodes = new List<BooleanPhraseNode>
                {
                    new BooleanLeaf("epg_id", "1234")
                }
            };
            var indexes6 = new[]
            {
                new[]
                {
                    new IndexesModel
                    {
                        Indexes = ElasticSearchIndexes.Epg
                    }
                }
            };
            var operands6 = new[] { eCutType.Or };
            yield return new TestCaseData(new object[] {tree6, indexes6, operands6});
            
            // (or description = '1234')
            var tree7 = new BooleanPhrase
            {
                operand = eCutType.Or,
                nodes = new List<BooleanPhraseNode>
                {
                    new BooleanLeaf("description", "1234")
                }
            };
            var indexes7 = new[]
            {
                new[]
                {
                    new IndexesModel
                    {
                        Indexes = ElasticSearchIndexes.Common
                    }
                }
            };
            var operands7 = new[] { eCutType.Or };
            yield return new TestCaseData(new object[] {tree7, indexes7, operands7});
            
            // (or media_id = '1234' asset_type = '0')
            var tree8 = new BooleanPhrase
            {
                operand = eCutType.Or,
                nodes = new List<BooleanPhraseNode>
                {
                    new BooleanLeaf("media_id", "1234"),
                    new BooleanLeaf("asset_type", "0"),
                }
            };
            var indexes8 = new[]
            {
                new[]
                {
                    new IndexesModel
                    {
                        Indexes = ElasticSearchIndexes.Media
                    },
                    new IndexesModel
                    {
                        Indexes = ElasticSearchIndexes.Epg
                    }
                }
            };
            var operands8 = new[] { eCutType.Or };
            yield return new TestCaseData(new object[] {tree8, indexes8, operands8});
            
            // (and asset_type = '234' asset_type = 'epg' name = '1352sdf')
            var tree9 = new BooleanPhrase
            {
                operand = eCutType.And,
                nodes = new List<BooleanPhraseNode>
                {
                    new BooleanLeaf("asset_type", "234"),
                    new BooleanLeaf("asset_type", "epg"),
                    new BooleanLeaf("name", "1352sdf"),
                }
            };
            var indexes9 = new[]
            {
                new[]
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
                    },
                }
            };
            var operands9 = new[] { eCutType.And };
            yield return new TestCaseData(new object[] {tree9, indexes9, operands9});
            
            // (and asset_type = '93527484') it's a custom value for program asset struct
            var tree10 = new BooleanPhrase
            {
                operand = eCutType.Or,
                nodes = new List<BooleanPhraseNode>
                {
                    new BooleanLeaf("asset_type", "93527484")
                }
            };
            var indexes10 = new[]
            {
                new[]
                {
                    new IndexesModel
                    {
                        Indexes = ElasticSearchIndexes.Epg
                    }
                }
            };
            var operands10 = new[] { eCutType.Or };
            yield return new TestCaseData(new object[] {tree10, indexes10, operands10});
        }

        private static IEnumerable<object> GetSourceForValidation()
        {
            // -------------------------
            var kSql1 = "(or linear_media_id = '1234' name = '35234')";
            var result1 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Epg | ElasticSearchIndexes.Common
            };
            yield return new TestCaseData(kSql1, result1);
            
            // -------------------------
            var kSql2 = "(and linear_media_id = '1234' name = '35234')";
            var result2 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Epg
            };
            yield return new TestCaseData(kSql2, result2);
            
            // -------------------------
            var kSql3 = "(or media_id = '1234' name = '35234')";
            var result3 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media | ElasticSearchIndexes.Common
            };
            yield return new TestCaseData(kSql3, result3);
            
            // -------------------------
            var kSql4 = "(and media_id = '1234' name = '35234')";
            var result4 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(kSql4, result4);
            
            // -------------------------
            var kSql5 = "(and media_id:'748678,748679,929911,925474,782526,925426,748681,721909,748680,748682,919159,748683,748684,748685,748686,748687,748688,748689,748690,748691,747039,747040,747041,747042,747043,747044,783560,822725,747045,747046,747047,747048,747049,747051,747050,748670,713441,748674,748675,911409,726791,726790,726787,726789,726786,726779,726776,726785,726781,747035,747032,747036,730756,726780,726778,726777,726775,726771,726770,726769,921823,915324,747034,914493,879232,763767,747037,747033,747030,747031,747028,747029,747023,747026,747025,747024,747022,747027,747020,747021,747019,747018,723199,723200,912624,904985,904992,904991,904988,904989,904987,904986,904963,904984,904983,904982,904981,904979,904980,904977,904978,904975,904976,904973,904974,904971,904972,904970,904969,904968,904967,904965,904966,904964,904962,904961,904960,904959,904958,904957,904956,904955,904954,904953,904804,905527,899531,897825,737923,737922,734464,801374,896829,895413,889467,884359,834196,741213,763788,763790,763791,763781,763780,763775,763779,763773,763778,763774,763772,763777,763769,763771,763776,763766,763768,763770,754738,810408,727612,782411' (and (and deep_link_type!='netflix' deep_link_type!='amazon' deep_link_type!='youtube' Is_adult!='1') (or asset_type='583' asset_type='584' asset_type='585' asset_type='586' asset_type='593' (and asset_type='582' PPV_module! '')) customer_type_blacklist!='1'))";
            var result5 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(kSql5, result5);
            
            // -------------------------
            var kSql6 = "(and (or media_id:'803437,800584,720207,730702,925692,928232,728860,744107,744150,726633,726287,723402,904843,763699,895760') customer_type_blacklist!='1')";
            var result6 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(kSql6, result6);
            
            // -------------------------
            var kSql7 = "media_id='803437'";
            var result7 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(kSql7, result7);
            
            // -------------------------
            var kSql8 = "asset_type='epg'";
            var result8 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Epg
            };
            yield return new TestCaseData(kSql8, result8);
            
            // -------------------------
            var kSql9 = "(and (or name ~ 'asdf' description ^ 'asdf' EntryID = 'asdf' ExternalID = 'asdf' media_id = '0' epg_id = '0' ) (or asset_type = '1884' asset_type = '1883' asset_type = '1885' asset_type = '21636' asset_type = '21638' asset_type = '16916' asset_type = '16807' asset_type = '16896' asset_type = '13871' asset_type = '20289' asset_type = '19386' asset_type = '19387' asset_type = '19416' asset_type = '21937' asset_type = '14271' asset_type = '21126' asset_type = '20288' asset_type = '21123' asset_type = '21125' asset_type = '21500' asset_type = '18505' asset_type = '13612' asset_type = '13611' asset_type = '2285' asset_type = '21497' asset_type = '8281' asset_type = '16854' asset_type = '21637' asset_type = '12262' asset_type = '21294' asset_type = '21501' asset_type = '13506' asset_type = '13505' asset_type = '21889' asset_type = '16542' asset_type = '21888' asset_type = '2272' asset_type = '2284' asset_type = '8324' asset_type = '21293' asset_type = '12263' asset_type = '12268' asset_type = '19184' asset_type = '21533' asset_type = '21845' asset_type = '21498' asset_type = '21844' asset_type = '21499' asset_type = '2289' asset_type = '8323' asset_type = '2257' asset_type = '14861' asset_type = '16219' asset_type = '12269' asset_type = '14047' asset_type = '21843' asset_type = '2243' asset_type = 'epg' ) )";
            var result9 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Epg | ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(kSql9, result9);
            
            // -------------------------
            var kSql10 = "(or media_id='123' media_id='456')";
            var result10 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(kSql10, result10);
            
            // -------------------------
            var kSql11 = "(or asset_type='93527484')";
            var result11 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Epg
            };
            yield return new TestCaseData(kSql11, result11);
            
            // -------------------------
            var kSql12 = "(or blabla='skdlfjawef')";
            var result12 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Common
            };
            yield return new TestCaseData(kSql12, result12);
        }
        
        private static IEnumerable<object> GetSourceForValidationWithTypes()
        {
            // -------------------------
            var kSql1 = "(or linear_media_id = '1234' name = '35234')";
            var result1 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Epg | ElasticSearchIndexes.Common
            };
            yield return new TestCaseData(kSql1, new int[]{}, result1);
            
            // -------------------------
            var kSql2 = "(and linear_media_id = '1234' name = '35234')";
            var result2 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Epg
            };
            yield return new TestCaseData(kSql2, new int[]{}, result2);
            
            // -------------------------
            var kSql3 = "(or media_id = '1234' name = '35234')";
            var result3 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media | ElasticSearchIndexes.Common
            };
            yield return new TestCaseData(kSql3, new int[]{}, result3);
            
            // -------------------------
            var kSql4 = "(and media_id = '1234' name = '35234')";
            var result4 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(kSql4, new int[]{}, result4);
            
            // -------------------------
            var kSql5 = "(and media_id:'748678,748679,929911,925474,782526,925426,748681,721909,748680,748682,919159,748683,748684,748685,748686,748687,748688,748689,748690,748691,747039,747040,747041,747042,747043,747044,783560,822725,747045,747046,747047,747048,747049,747051,747050,748670,713441,748674,748675,911409,726791,726790,726787,726789,726786,726779,726776,726785,726781,747035,747032,747036,730756,726780,726778,726777,726775,726771,726770,726769,921823,915324,747034,914493,879232,763767,747037,747033,747030,747031,747028,747029,747023,747026,747025,747024,747022,747027,747020,747021,747019,747018,723199,723200,912624,904985,904992,904991,904988,904989,904987,904986,904963,904984,904983,904982,904981,904979,904980,904977,904978,904975,904976,904973,904974,904971,904972,904970,904969,904968,904967,904965,904966,904964,904962,904961,904960,904959,904958,904957,904956,904955,904954,904953,904804,905527,899531,897825,737923,737922,734464,801374,896829,895413,889467,884359,834196,741213,763788,763790,763791,763781,763780,763775,763779,763773,763778,763774,763772,763777,763769,763771,763776,763766,763768,763770,754738,810408,727612,782411' (and (and deep_link_type!='netflix' deep_link_type!='amazon' deep_link_type!='youtube' Is_adult!='1') (or asset_type='583' asset_type='584' asset_type='585' asset_type='586' asset_type='593' (and asset_type='582' PPV_module! '')) customer_type_blacklist!='1'))";
            var result5 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(kSql5, new int[]{}, result5);
            
            // -------------------------
            var kSql6 = "(and (or media_id:'803437,800584,720207,730702,925692,928232,728860,744107,744150,726633,726287,723402,904843,763699,895760') customer_type_blacklist!='1')";
            var result6 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(kSql6, new int[]{}, result6);
            
            // -------------------------
            var kSql7 = "media_id='803437'";
            var result7 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(kSql7, new int[]{}, result7);
            
            // -------------------------
            var kSql8 = "asset_type='epg'";
            var result8 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Epg
            };
            yield return new TestCaseData(kSql8, new int[]{}, result8);
            
            // -------------------------
            var kSql9 = "(and (or name ~ 'asdf' description ^ 'asdf' EntryID = 'asdf' ExternalID = 'asdf' media_id = '0' epg_id = '0' ) (or asset_type = '1884' asset_type = '1883' asset_type = '1885' asset_type = '21636' asset_type = '21638' asset_type = '16916' asset_type = '16807' asset_type = '16896' asset_type = '13871' asset_type = '20289' asset_type = '19386' asset_type = '19387' asset_type = '19416' asset_type = '21937' asset_type = '14271' asset_type = '21126' asset_type = '20288' asset_type = '21123' asset_type = '21125' asset_type = '21500' asset_type = '18505' asset_type = '13612' asset_type = '13611' asset_type = '2285' asset_type = '21497' asset_type = '8281' asset_type = '16854' asset_type = '21637' asset_type = '12262' asset_type = '21294' asset_type = '21501' asset_type = '13506' asset_type = '13505' asset_type = '21889' asset_type = '16542' asset_type = '21888' asset_type = '2272' asset_type = '2284' asset_type = '8324' asset_type = '21293' asset_type = '12263' asset_type = '12268' asset_type = '19184' asset_type = '21533' asset_type = '21845' asset_type = '21498' asset_type = '21844' asset_type = '21499' asset_type = '2289' asset_type = '8323' asset_type = '2257' asset_type = '14861' asset_type = '16219' asset_type = '12269' asset_type = '14047' asset_type = '21843' asset_type = '2243' asset_type = 'epg' ) )";
            var result9 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Epg | ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(kSql9, new int[]{}, result9);
            
            // -------------------------
            var kSql10 = "(or media_id='123' media_id='456')";
            var result10 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(kSql10, new int[]{}, result10);
            
            // -------------------------
            var kSql11 = "(or asset_type='93527484')";
            var result11 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Epg
            };
            yield return new TestCaseData(kSql11, new int[]{}, result11);
            
            // -------------------------
            var kSql12 = "(or blabla='23lksdjfe')";
            var mediaTypes12 = new int[] { 234 };
            var result12 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(kSql12, mediaTypes12, result12);
            
            var kSql13 = "(or blabla='23lksdjfe')";
            var mediaTypes13 = new int[] { 0 };
            var result13 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Epg
            };
            yield return new TestCaseData(kSql13, mediaTypes13, result13);
            
            var kSql14 = "(or blabla='23lksdjfe')";
            var result14 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Common
            };
            yield return new TestCaseData(kSql14, new int[]{}, result14);
            
            var kSql15 = "(and blabla='23lksdjfe')";
            var mediaTypes15 = new[] { 34234, 2342 };
            var result15 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media
            };
            yield return new TestCaseData(kSql15, mediaTypes15, result15);
            
            var kSql16 = "(and blabla='23lksdjfe')";
            var mediaTypes16 = new[] { 34234, 2342, 0 };
            var result16 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Media | ElasticSearchIndexes.Epg
            };
            yield return new TestCaseData(kSql16, mediaTypes16, result16);

            string kSql17 = null;
            var mediaTypes17 = new[] { 1 };
            var result17 = new IndexesModel
            {
                Indexes = new ElasticSearchIndexes()
            };
            yield return new TestCaseData(kSql17, mediaTypes17, result17);
            
            string kSql18 = "(and series name = 'episode1')";
            var mediaTypes18 = new[] { 1 };
            var result18 = new IndexesModel
            {
                Indexes = ElasticSearchIndexes.Common
            };
            yield return new TestCaseData(kSql18, mediaTypes18, result18);
        }

        #endregion
    }
}
