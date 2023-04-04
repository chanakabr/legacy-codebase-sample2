using System;
using System.Collections.Generic;
using ApiLogic.Api.Managers.Rule;
using ApiObjects;
using ApiObjects.Catalog;
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
    public class PreActionConditionMatcherTests
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

        [Test]
        public void IsMatched_PreActionConditionEmpty()
        {
            const int groupId = 1;
            var action = new FilterFileByFileTypeInPlayback();
            var filterAsset = new Lazy<FilterMediaFileAsset>(() => new FilterMediaFileAsset { AssetId = 1 });
            var assetUserRuleManager = _assetUserRuleManagerMock.Object;
            var context = new PreActionConditionContext(groupId, assetUserRuleManager, _shopMarkerServiceMock.Object);

            var matcher = new PreActionConditionMatcher(assetUserRuleManager);
            var isMatched = matcher.IsMatched(context, action, filterAsset);

            isMatched.Should().BeTrue();
        }

        [Test]
        public void IsMatched_ShopPreActionCondition_NoShopCondition()
        {
            const int groupId = 1;
            var action = new FilterFileByFileTypeInPlayback { PreActionCondition = new ShopPreActionCondition { ShopAssetUserRuleId = 1 }};
            var filterAsset = new Lazy<FilterMediaFileAsset>(() => new FilterMediaFileAsset { AssetId = 1 });
            _assetUserRuleManagerMock.Setup(x => x.GetCachedAssetUserRuleByRuleId(groupId, 1))
                .Returns(new GenericResponse<AssetUserRule>(
                    Status.Ok,
                    new AssetUserRule
                    {
                        Conditions = new List<AssetConditionBase> { new AssetCondition() },
                        Actions = new List<AssetUserRuleAction> { new AssetUserRuleFilterAction() }
                    }));
            var assetUserRuleManager = _assetUserRuleManagerMock.Object;
            var context = new PreActionConditionContext(groupId, assetUserRuleManager, _shopMarkerServiceMock.Object);

            var matcher = new PreActionConditionMatcher(assetUserRuleManager);
            var isMatched = matcher.IsMatched(context, action, filterAsset);

            isMatched.Should().BeFalse();
        }

        [Test]
        public void IsMatched_ShopPreActionCondition_NoShopMeta()
        {
            const int groupId = 1;
            var action = new FilterFileByFileTypeInPlayback { PreActionCondition = new ShopPreActionCondition { ShopAssetUserRuleId = 1 }};
            var filterAsset = new Lazy<FilterMediaFileAsset>(() => new FilterMediaFileAsset { AssetId = 1 });

            _shopMarkerServiceMock.Setup(x => x.GetShopMarkerTopic(groupId))
                .Returns(new GenericResponse<Topic>(eResponseStatus.TopicNotFound));

            var shopCondition = new AssetShopCondition { Values = new List<string> { "val1" } };
            _assetUserRuleManagerMock.Setup(x => x.GetCachedAssetUserRuleByRuleId(groupId, 1))
                .Returns(new GenericResponse<AssetUserRule>(
                    Status.Ok,
                    new AssetUserRule
                    {
                        Conditions = new List<AssetConditionBase> { shopCondition },
                        Actions = new List<AssetUserRuleAction> { new AssetUserRuleFilterAction() }
                    }));
            var assetUserRuleManager = _assetUserRuleManagerMock.Object;
            var context = new PreActionConditionContext(groupId, assetUserRuleManager, _shopMarkerServiceMock.Object);

            var matcher = new PreActionConditionMatcher(assetUserRuleManager);
            var isMatched = matcher.IsMatched(context, action, filterAsset);

            isMatched.Should().BeFalse();
        }

        [Test]
        public void IsMatched_ShopPreActionCondition_ReturnsExpectedResult()
        {
            const int groupId = 1;
            var action = new FilterFileByFileTypeInPlayback { PreActionCondition = new ShopPreActionCondition { ShopAssetUserRuleId = 1 }};
            var shopMeta = new Topic { SystemName = "someMeta" };
            var filterAsset = new Lazy<FilterMediaFileAsset>(() => new FilterMediaFileAsset {
                AssetId = 1,
                Metas = new [] { new Metas { m_oTagMeta = new TagMeta { m_sName = "someMeta" }, m_sValue = "val1"} }
            });

            _shopMarkerServiceMock.Setup(x => x.GetShopMarkerTopic(groupId))
                .Returns(new GenericResponse<Topic>(Status.Ok, shopMeta));

            var shopCondition = new AssetShopCondition { Values = new List<string> { "val1" } };
            _assetUserRuleManagerMock.Setup(x => x.GetCachedAssetUserRuleByRuleId(groupId, 1))
                .Returns(new GenericResponse<AssetUserRule>(
                    Status.Ok,
                    new AssetUserRule
                    {
                        Conditions = new List<AssetConditionBase> { shopCondition },
                        Actions = new List<AssetUserRuleAction> { new AssetUserRuleFilterAction() }
                    }));
            _assetUserRuleManagerMock.Setup(x => x.IsAssetPartOfShopRule(shopMeta, shopCondition, filterAsset.Value.Metas, filterAsset.Value.Tags))
                .Returns(true);
            var assetUserRuleManager = _assetUserRuleManagerMock.Object;
            var context = new PreActionConditionContext(groupId, assetUserRuleManager, _shopMarkerServiceMock.Object);

            var matcher = new PreActionConditionMatcher(assetUserRuleManager);
            var isMatched = matcher.IsMatched(context, action, filterAsset);

            isMatched.Should().BeTrue();
        }

        [Test]
        public void IsMatched_NoShopPreActionCondition_NoShopCondition()
        {
            const int groupId = 1;
            var action = new FilterFileByFileTypeInPlayback { PreActionCondition = new NoShopPreActionCondition()};
            var filterAsset = new Lazy<FilterMediaFileAsset>(() => new FilterMediaFileAsset { AssetId = 1 });
            _assetUserRuleManagerMock
                .Setup(x => x.GetAssetUserRuleList(
                    groupId,
                    null,
                    true,
                    RuleActionType.UserFilter,
                    RuleConditionType.AssetShop,
                    false))
                .Returns(new GenericListResponse<AssetUserRule>(Status.Ok, new List<AssetUserRule>()));
            var assetUserRuleManager = _assetUserRuleManagerMock.Object;
            var context = new PreActionConditionContext(groupId, assetUserRuleManager, _shopMarkerServiceMock.Object);

            var matcher = new PreActionConditionMatcher(assetUserRuleManager);
            var isMatched = matcher.IsMatched(context, action, filterAsset);

            isMatched.Should().BeFalse();
        }

        [Test]
        public void IsMatched_NoShopPreActionCondition_NoShopMeta()
        {
            const int groupId = 1;
            var action = new FilterFileByFileTypeInPlayback { PreActionCondition = new NoShopPreActionCondition()};
            var filterAsset = new Lazy<FilterMediaFileAsset>(() => new FilterMediaFileAsset { AssetId = 1 });

            _shopMarkerServiceMock.Setup(x => x.GetShopMarkerTopic(groupId))
                .Returns(new GenericResponse<Topic>(eResponseStatus.TopicNotFound));

            var shopCondition1 = new AssetShopCondition { Values = new List<string> { "val1" } };
            var shopCondition2 = new AssetShopCondition { Values = new List<string> { "val2" } };
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
            var assetUserRuleManager = _assetUserRuleManagerMock.Object;
            var context = new PreActionConditionContext(groupId, assetUserRuleManager, _shopMarkerServiceMock.Object);

            var matcher = new PreActionConditionMatcher(assetUserRuleManager);
            var isMatched = matcher.IsMatched(context, action, filterAsset);

            isMatched.Should().BeFalse();
        }

        [Test]
        public void IsMatched_NoShopPreActionCondition_ReturnsExpectedResult()
        {
            const int groupId = 1;
            var action = new FilterFileByFileTypeInPlayback { PreActionCondition = new NoShopPreActionCondition()};
            var shopMeta = new Topic { SystemName = "someMeta" };
            var filterAsset = new Lazy<FilterMediaFileAsset>(() => new FilterMediaFileAsset {
                AssetId = 1,
                Metas = new [] { new Metas { m_oTagMeta = new TagMeta { m_sName = "someMeta" }, m_sValue = "val1"} }
            });

            _shopMarkerServiceMock.Setup(x => x.GetShopMarkerTopic(groupId))
                .Returns(new GenericResponse<Topic>(Status.Ok, shopMeta));

            var shopCondition1 = new AssetShopCondition { Values = new List<string> { "val1" } };
            var shopCondition2 = new AssetShopCondition { Values = new List<string> { "val2" } };
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
            _assetUserRuleManagerMock.Setup(x => x.IsAssetPartOfShopRule(shopMeta, shopCondition1, filterAsset.Value.Metas, filterAsset.Value.Tags))
                .Returns(false);
            _assetUserRuleManagerMock.Setup(x => x.IsAssetPartOfShopRule(shopMeta, shopCondition2, filterAsset.Value.Metas, filterAsset.Value.Tags))
                .Returns(false);
            var assetUserRuleManager = _assetUserRuleManagerMock.Object;
            var context = new PreActionConditionContext(groupId, assetUserRuleManager, _shopMarkerServiceMock.Object);

            var matcher = new PreActionConditionMatcher(assetUserRuleManager);
            var isMatched = matcher.IsMatched(context, action, filterAsset);

            isMatched.Should().BeTrue();
        }

        [Test]
        public void IsMatched_PreActionConditions_VerifyRequestsCount()
        {
            const int groupId = 1;
            var actions = new AssetRuleFilterAction[]
            {
                new FilterFileByAudioCodecInDiscovery { PreActionCondition = new ShopPreActionCondition { ShopAssetUserRuleId = 1 } },
                new FilterFileByQualityInDiscovery { PreActionCondition = new NoShopPreActionCondition() },
                new FilterFileByLabelInDiscovery { PreActionCondition = new NoShopPreActionCondition() }
            };
            var shopMeta = new Topic { SystemName = "someMeta" };
            var filterAsset = new Lazy<FilterMediaFileAsset>(() => new FilterMediaFileAsset {
                AssetId = 1,
                Metas = new [] { new Metas { m_oTagMeta = new TagMeta { m_sName = "someMeta" }, m_sValue = "val1"} }
            });

            _shopMarkerServiceMock.Setup(x => x.GetShopMarkerTopic(groupId))
                .Returns(new GenericResponse<Topic>(Status.Ok, shopMeta));

            var shopCondition1 = new AssetShopCondition { Values = new List<string> { "val1" } };
            var shopCondition2 = new AssetShopCondition { Values = new List<string> { "val2" } };
            var shopCondition3 = new AssetShopCondition { Values = new List<string> { "val3" } };

            _assetUserRuleManagerMock.Setup(x => x.GetCachedAssetUserRuleByRuleId(groupId, 1))
                .Returns(new GenericResponse<AssetUserRule>(
                    Status.Ok,
                    new AssetUserRule
                    {
                        Conditions = new List<AssetConditionBase> { shopCondition3 },
                        Actions = new List<AssetUserRuleAction> { new AssetUserRuleFilterAction() }
                    }));

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
            _assetUserRuleManagerMock.Setup(x => x.IsAssetPartOfShopRule(shopMeta, shopCondition1, filterAsset.Value.Metas, filterAsset.Value.Tags))
                .Returns(false);
            _assetUserRuleManagerMock.Setup(x => x.IsAssetPartOfShopRule(shopMeta, shopCondition2, filterAsset.Value.Metas, filterAsset.Value.Tags))
                .Returns(false);
            _assetUserRuleManagerMock.Setup(x => x.IsAssetPartOfShopRule(shopMeta, shopCondition3, filterAsset.Value.Metas, filterAsset.Value.Tags))
                .Returns(true);
            var assetUserRuleManager = _assetUserRuleManagerMock.Object;
            var context = new PreActionConditionContext(groupId, assetUserRuleManager, _shopMarkerServiceMock.Object);

            var matcher = new PreActionConditionMatcher(assetUserRuleManager);
            Array.ForEach(actions, a => matcher.IsMatched(context, a, filterAsset));
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }
    }
}