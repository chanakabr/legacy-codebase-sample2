using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.Api.Managers.Rule;
using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Rules;
using ApiObjects.Rules.FilterActions;
using Core.Api.Managers;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using OTT.Service.Authentication;
using UserSessionProfileCondition = ApiObjects.Rules.UserSessionProfileCondition;

namespace ApiLogic.Tests.Api.Managers.Rule
{
    [TestFixture]
    public class FilterRuleStorageTests
    {
        private MockRepository _mockRepository;
        private Mock<IAssetRuleManager> _assetRuleManagerMock;
        private Mock<IPreActionConditionMatcher> _preActionConditionMatcher;
        private Mock<ISessionCharacteristicManager> _sessionCharacteristicsManagerMock;
        private Mock<IShopMarkerService> _shopMarkerServiceMock;
        private Mock<IAssetUserRuleManager> _assetUserRuleManagerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _assetRuleManagerMock = _mockRepository.Create<IAssetRuleManager>();
            _preActionConditionMatcher = _mockRepository.Create<IPreActionConditionMatcher>();
            _sessionCharacteristicsManagerMock = _mockRepository.Create<ISessionCharacteristicManager>();
            _shopMarkerServiceMock = _mockRepository.Create<IShopMarkerService>();
            _assetUserRuleManagerMock = _mockRepository.Create<IAssetUserRuleManager>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void GetAssetFilterRulesForDiscovery_NoActions()
        {
            var groupId = 1;
            var sck = "my_sck";
            var filterAsset = new Lazy<FilterMediaFileAsset>(() => new FilterMediaFileAsset { AssetId = 1 });
            _assetRuleManagerMock.Setup(x => x.GetAssetRules(RuleConditionType.UserSessionProfile, groupId, null, null,
                    null, AssetRuleOrderBy.None))
                .Returns(new GenericListResponse<AssetRule>(Status.Ok, new List<AssetRule>
                {
                    new AssetRule
                    {
                        Conditions = new List<RuleCondition> { new UserSessionProfileCondition { Id = 5 } },
                        Actions = new List<AssetRuleAction>
                        {
                            new FilterFileByFileTypeInPlayback(),
                            new FilterFileByLabelInPlayback()
                        }
                    }
                }));

            var context = new PreActionConditionContext(groupId, _assetUserRuleManagerMock.Object, _shopMarkerServiceMock.Object);
            var condition = new FilterFileRuleCondition(groupId, sck, filterAsset, context);

            _sessionCharacteristicsManagerMock.Setup(x => x.GetFromCache(groupId, sck))
                .Returns(new GetSessionCharacteristicsResponse { UserSessionProfileIds = { 5 } });

            var filterRuleStorage = new FilterRuleStorage(
                _assetRuleManagerMock.Object,
                _preActionConditionMatcher.Object,
                new Lazy<ISessionCharacteristicManager>(() => _sessionCharacteristicsManagerMock.Object));
            var result = filterRuleStorage.GetAssetFilterRulesForDiscovery(condition);

            result.Should().BeEmpty();
        }

        [Test]
        public void GetFilterFileRulesForDiscovery_ReturnsExpectedResult()
        {
            var groupId = 1;
            var sck = "my_sck";
            var filterAsset = new Lazy<FilterMediaFileAsset>(() => new FilterMediaFileAsset { AssetId = 1 });
            var action = new FilterFileByStreamerTypeInDiscovery();
            _assetRuleManagerMock.Setup(x => x.GetAssetRules(RuleConditionType.UserSessionProfile, groupId, null, null,
                    null, AssetRuleOrderBy.None))
                .Returns(new GenericListResponse<AssetRule>(Status.Ok, new List<AssetRule>
                {
                    new AssetRule
                    {
                        Conditions = new List<RuleCondition> { new UserSessionProfileCondition { Id = 5 } },
                        Actions = new List<AssetRuleAction> { action }
                    }
                }));

            var context = new PreActionConditionContext(groupId, _assetUserRuleManagerMock.Object, _shopMarkerServiceMock.Object);
            var condition = new FilterFileRuleCondition(groupId, sck, filterAsset, context);

            _sessionCharacteristicsManagerMock.Setup(x => x.GetFromCache(groupId, sck))
                .Returns(new GetSessionCharacteristicsResponse { UserSessionProfileIds = { 5 } });

            _preActionConditionMatcher.Setup(x => x.IsMatched(condition.Context, action, condition.Asset))
                .Returns(true);

            var filterRuleStorage = new FilterRuleStorage(
                _assetRuleManagerMock.Object,
                _preActionConditionMatcher.Object,
                new Lazy<ISessionCharacteristicManager>(() => _sessionCharacteristicsManagerMock.Object));

            var result = filterRuleStorage.GetAssetFilterRulesForDiscovery(condition);

            result.Count.Should().Be(1);
            result.Single().Should().Be(action);
        }

        [Test]
        public void GetFilterFileRulesForPlayback_NoActions()
        {
            var groupId = 1;
            var sck = "my_sck";
            var filterAsset = new Lazy<FilterMediaFileAsset>(() => new FilterMediaFileAsset { AssetId = 1 });
            _assetRuleManagerMock.Setup(x => x.GetAssetRules(RuleConditionType.UserSessionProfile, groupId, null, null,
                    null, AssetRuleOrderBy.None))
                .Returns(new GenericListResponse<AssetRule>(Status.Ok, new List<AssetRule>
                {
                    new AssetRule
                    {
                        Conditions = new List<RuleCondition> { new UserSessionProfileCondition { Id = 5 } },
                        Actions = new List<AssetRuleAction>
                        {
                            new FilterFileByLabelInDiscovery(),
                            new FilterFileByAudioCodecInDiscovery()
                        }
                    }
                }));

            var context = new PreActionConditionContext(groupId, _assetUserRuleManagerMock.Object, _shopMarkerServiceMock.Object);
            var condition = new FilterFileRuleCondition(groupId, sck, filterAsset, context);

            _sessionCharacteristicsManagerMock.Setup(x => x.GetFromCache(groupId, sck))
                .Returns(new GetSessionCharacteristicsResponse { UserSessionProfileIds = { 5 } });

            var filterRuleStorage = new FilterRuleStorage(
                _assetRuleManagerMock.Object,
                _preActionConditionMatcher.Object,
                new Lazy<ISessionCharacteristicManager>(() => _sessionCharacteristicsManagerMock.Object));

            var result = filterRuleStorage.GetAssetFilterRulesForPlayback(condition);

            result.Should().BeEmpty();
        }

        [Test]
        public void GetFilterFileRulesForPlayback_ReturnsExpectedResult()
        {
            var groupId = 1;
            var sck = "my_sck";
            var filterAsset = new Lazy<FilterMediaFileAsset>(() => new FilterMediaFileAsset { AssetId = 1 });
            var action = new FilterFileByStreamerTypeInPlayback();
            _assetRuleManagerMock
                .Setup(x => x.GetAssetRules(
                    RuleConditionType.UserSessionProfile,
                    groupId,
                    null,
                    null,
                    null,
                    AssetRuleOrderBy.None))
                .Returns(new GenericListResponse<AssetRule>(Status.Ok, new List<AssetRule>
                {
                    new AssetRule
                    {
                        Conditions = new List<RuleCondition> { new UserSessionProfileCondition { Id = 5 } },
                        Actions = new List<AssetRuleAction> { action }
                    }
                }));

            var context = new PreActionConditionContext(groupId, _assetUserRuleManagerMock.Object, _shopMarkerServiceMock.Object);
            var condition = new FilterFileRuleCondition(groupId, sck, filterAsset, context);

            _sessionCharacteristicsManagerMock.Setup(x => x.GetFromCache(groupId, sck))
                .Returns(new GetSessionCharacteristicsResponse { UserSessionProfileIds = { 5 } });

            _preActionConditionMatcher.Setup(x => x.IsMatched(condition.Context, action, condition.Asset))
                .Returns(true);

            var filterRuleStorage = new FilterRuleStorage(
                _assetRuleManagerMock.Object,
                _preActionConditionMatcher.Object,
                new Lazy<ISessionCharacteristicManager>(() => _sessionCharacteristicsManagerMock.Object));

            var result = filterRuleStorage.GetAssetFilterRulesForPlayback(condition);

            result.Count.Should().Be(1);
            result.Single().Should().Be(action);
        }
    }
}