using System.Collections.Generic;
using ApiLogic.Api.Managers.Rule;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Rules;
using ApiObjects.Rules.FilterActions;
using ApiObjects.Rules.PreActionCondition;
using Core.Api.Managers;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.Api.Managers.Rule
{
    public class FilterAssetRuleTests
    {
        private MockRepository _mockRepository;
        private Mock<IShopMarkerService> _shopMarkerServiceMock;
        private Mock<IAssetUserRuleManager> _assetUserRuleManagerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _shopMarkerServiceMock = _mockRepository.Create<IShopMarkerService>();
            _assetUserRuleManagerMock = _mockRepository.Create<IAssetUserRuleManager>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void GetFilteringKsql_ShopPreActionCondition_ReturnsExpectedResult()
        {
            var expectedResult =
                "(and (and (and asset_type='1') (or someMeta='val1' someMeta='val2')) (and asset_type='2') (and (and asset_type='3') (or someMeta='val3')))";
            var groupId = 1;
            var rules = new []
            {
                new FilterAssetByKsql
                {
                    PreActionCondition = new ShopPreActionCondition { ShopAssetUserRuleId = 1 },
                    Ksql = "(and asset_type='1')"
                },
                new FilterAssetByKsql { Ksql = "(and asset_type='2')" },
                new FilterAssetByKsql
                {
                    PreActionCondition = new ShopPreActionCondition { ShopAssetUserRuleId = 2 },
                    Ksql = "(and asset_type='3')"
                },
            };

            var shopCondition1 = new AssetShopCondition { Values = new List<string> { "val1", "val2" } };
            var shopCondition2 = new AssetShopCondition { Values = new List<string> { "val3" } };
            var assetUserRule1 = new AssetUserRule
            {
                Conditions = new List<AssetConditionBase> { shopCondition1 },
                Actions = new List<AssetUserRuleAction> { new AssetUserRuleFilterAction() }
            };

            var assetUserRule2 = new AssetUserRule
            {
                Conditions = new List<AssetConditionBase> { shopCondition2 },
                Actions = new List<AssetUserRuleAction> { new AssetUserRuleFilterAction() }
            };

            _assetUserRuleManagerMock.Setup(x => x.GetCachedAssetUserRuleByRuleId(groupId, 1))
                .Returns(new GenericResponse<AssetUserRule>(Status.Ok, assetUserRule1));
            _assetUserRuleManagerMock.Setup(x => x.GetCachedAssetUserRuleByRuleId(groupId, 2))
                .Returns(new GenericResponse<AssetUserRule>(Status.Ok, assetUserRule2));

            var shopMeta = new Topic { SystemName = "someMeta" };
            _shopMarkerServiceMock.Setup(x => x.GetShopMarkerTopic(groupId))
                .Returns(new GenericResponse<Topic>(Status.Ok, shopMeta));

            var filterAssetRule = new FilterAssetRule(_assetUserRuleManagerMock.Object, _shopMarkerServiceMock.Object);
            var result = filterAssetRule.GetFilteringKsql(groupId, rules);

            result.Should().Be(expectedResult);
        }

        [Test]
        public void GetFilteringKsql_NoShopPreActionCondition_ReturnsExpectedResult()
        {
            var expectedResult =
                "(and (and (and asset_type='1') someMeta!='val1' someMeta!='val2' someMeta!='val3') (and asset_type='2'))";
            var groupId = 1;
            var rules = new []
            {
                new FilterAssetByKsql
                {
                    PreActionCondition = new NoShopPreActionCondition(),
                    Ksql = "(and asset_type='1')"
                },
                new FilterAssetByKsql { Ksql = "(and asset_type='2')" }
            };

            var shopMeta = new Topic { SystemName = "someMeta" };
            _shopMarkerServiceMock.Setup(x => x.GetShopMarkerTopic(groupId))
                .Returns(new GenericResponse<Topic>(Status.Ok, shopMeta));

            var shopCondition1 = new AssetShopCondition { Values = new List<string> { "val1", "val2" } };
            var shopCondition2 = new AssetShopCondition { Values = new List<string> { "val3" } };
            var assetUserRule1 = new AssetUserRule
            {
                Conditions = new List<AssetConditionBase> { shopCondition1 },
                Actions = new List<AssetUserRuleAction> { new AssetUserRuleFilterAction() }
            };

            var assetUserRule2 = new AssetUserRule
            {
                Conditions = new List<AssetConditionBase> { shopCondition2 },
                Actions = new List<AssetUserRuleAction> { new AssetUserRuleFilterAction() }
            };

            _assetUserRuleManagerMock
                .Setup(x => x.GetAssetUserRuleList(
                    groupId,
                    null,
                    true,
                    RuleActionType.UserFilter,
                    RuleConditionType.AssetShop,
                    false))
                .Returns(new GenericListResponse<AssetUserRule>(Status.Ok, new List<AssetUserRule> { assetUserRule1, assetUserRule2 }));

            var filterAssetRule = new FilterAssetRule(_assetUserRuleManagerMock.Object, _shopMarkerServiceMock.Object);
            var result = filterAssetRule.GetFilteringKsql(groupId, rules);

            result.Should().Be(expectedResult);
        }
    }
}