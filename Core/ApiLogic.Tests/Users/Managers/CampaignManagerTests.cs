using ApiLogic.Api.Managers;
using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects.Roles;
using ApiObjects.Rules;
using AutoFixture;
using AutoFixture.Kernel;
using CachingProvider.LayeredCache;
using Core.Api.Managers;
using Core.Catalog.CatalogManagement;
using Core.Notification;
using DAL;
using EventBus.Abstraction;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TVinciShared;

namespace ApiLogic.Tests.Users.Managers
{
    [TestFixture]
    public class CampaignManagerTests
    {
        delegate void MockGetCampaignDBFromCache(string key, ref IEnumerable<CampaignDB> genericParameter, Func<Dictionary<string, object>, Tuple<IEnumerable<CampaignDB>, bool>> fillObjectMethod,
                                      Dictionary<string, object> funcParameters, int groupId, string layeredCacheConfigName, List<string> inValidationKeys = null,
                                      bool shouldUseAutoNameTypeHandling = false);

        delegate void MockGetCampaignFromCache(string key, ref ApiObjects.Campaign genericParameter, Func<Dictionary<string, object>, Tuple<ApiObjects.Campaign, bool>> fillObjectMethod,
                                      Dictionary<string, object> funcParameters, int groupId, string layeredCacheConfigName, List<string> inValidationKeys = null,
                                      bool shouldUseAutoNameTypeHandling = false);

        private static readonly long date = DateUtils.GetUtcUnixTimestampNow() + 900000;

        private IChannelManager ChannelManagerMock;
        private IEventBusPublisher eventBusPublisherMock;
        private ICatalogManager catalogManager;
        private GroupsCacheManager.IGroupsCache groupsCache;
        private IConditionValidator conditionValidator;
        private IPromotionValidator promotionValidator;
        private IMessageInboxManger messageInboxManger;
        private IAssetUserRuleManager assetUserRuleManager;
        Mock<ICampaignRepository> campaignRepositoryMock;

        private ContextData contextData;

        [SetUp]
        public void SetUp()
        {
            ChannelManagerMock = Mock.Of<IChannelManager>();
            eventBusPublisherMock = Mock.Of<IEventBusPublisher>();
            catalogManager = Mock.Of<ICatalogManager>();
            groupsCache = Mock.Of<GroupsCacheManager.IGroupsCache>();
            conditionValidator = Mock.Of<IConditionValidator>();
            promotionValidator = Mock.Of<IPromotionValidator>();
            messageInboxManger = Mock.Of<IMessageInboxManger>();
            assetUserRuleManager = Mock.Of<IAssetUserRuleManager>();

            campaignRepositoryMock = new Mock<ICampaignRepository>();
            campaignRepositoryMock.Setup(x => x.UpdateCampaign(It.IsAny<ApiObjects.Campaign>(), It.IsAny<ContextData>()))
                                         .Returns(true);

            contextData = new ContextData(123)
            {
                UserId = 456,
                DomainId = 789,
                UserRoleIds = new List<long>() { PredefinedRoleId.ADMINISTRATOR}
            };
        }

        [TestCaseSource(nameof(SetTriggerStateFromInactiveToActiveTestCase))]
        [TestCaseSource(nameof(SetTriggerStateFromActiveToArchiveTestCase))]
        [TestCaseSource(nameof(SetStateCampaignDoesNotExistTestCase))]
        [TestCaseSource(nameof(SetTriggerStateInvalidCampaignStateTestCase))]
        [TestCaseSource(nameof(SetTriggerStateCampaignStateUpdateNotAllowedArchiveToInactiveTestCase))]
        [TestCaseSource(nameof(SetTriggerStateCampaignStateUpdateNotAllowedArchiveToActiveTestCase))]
        [TestCaseSource(nameof(SetTriggerStateCampaignStateUpdateNotAllowedActiveToInactiveTestCase))]
        [TestCaseSource(nameof(SetTriggerStateCampaignStateUpdateNotAllowedInactiveToArchiveTestCase))]
        [TestCaseSource(nameof(SetTriggerStateExceededMaxCapacityTestCase))]
        [TestCaseSource(nameof(SetTriggerStateInvalidCampaignEndDateTestCase))]
        [TestCaseSource(nameof(SetBatchStateFromInactiveToActiveTestCase))]
        [TestCaseSource(nameof(SetBatchStateFromActiveToArchiveTestCase))]
        [TestCaseSource(nameof(SetBatchStateInvalidCampaignStateTestCase))]
        [TestCaseSource(nameof(SetBatchStateCampaignStateUpdateNotAllowedArchiveToInactiveTestCase))]
        [TestCaseSource(nameof(SetBatchStateCampaignStateUpdateNotAllowedArchiveToActiveTestCase))]
        [TestCaseSource(nameof(SetBatchStateCampaignStateUpdateNotAllowedActiveToInactiveTestCase))]
        [TestCaseSource(nameof(SetBatchStateCampaignStateUpdateNotAllowedInactiveToArchiveTestCase))]
        [TestCaseSource(nameof(SetBatchStateExceededMaxCapacityTestCase))]
        [TestCaseSource(nameof(SetBatchStateInvalidCampaignEndDateTestCase))]
        public void CheckSetState<T>(long id, CampaignState newState, IEnumerable<CampaignDB> campaignDbs, T campaign, eResponseStatus status) where T: ApiObjects.Campaign, new()
        {
            var layeredCacheMock = GetLayeredCacheMock(campaignDbs, campaign, contextData);

            CampaignManager manager = new CampaignManager(layeredCacheMock.Object,
                                                          campaignRepositoryMock.Object,
                                                          ChannelManagerMock,
                                                          eventBusPublisherMock,
                                                          catalogManager,
                                                          groupsCache,
                                                          conditionValidator,
                                                          promotionValidator,
                                                          messageInboxManger,
                                                          assetUserRuleManager);

            var response = manager.SetState(contextData, id, newState);

            Assert.That(response.Code, Is.EqualTo((int)status));
        }

        private static IEnumerable SetTriggerStateFromInactiveToActiveTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));

            TriggerCampaign campaign = fixture.Create<TriggerCampaign>();
            campaign.EndDate = date;
            long id = campaign.Id;
            campaign.State = CampaignState.INACTIVE;
            CampaignState newState = CampaignState.ACTIVE;
            IEnumerable<CampaignDB> campaignDbs = fixture.CreateMany<CampaignDB>(1);

            yield return new TestCaseData(id,
                                          newState,
                                          campaignDbs,
                                          campaign,
                                          eResponseStatus.OK)
                .SetName("SetState_Trigger_INACTIVE_to_ACTIVE");
        }

        private static IEnumerable SetTriggerStateFromActiveToArchiveTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));
            
            TriggerCampaign campaign2 = fixture.Create<TriggerCampaign>();
            campaign2.EndDate = date;
            long id2 = campaign2.Id;
            campaign2.State = CampaignState.ACTIVE;
            CampaignState newState2 = CampaignState.ARCHIVE;
            IEnumerable<CampaignDB> campaignDbs2 = fixture.CreateMany<CampaignDB>(1);

            yield return new TestCaseData(id2,
                                          newState2,
                                          campaignDbs2,
                                          campaign2,
                                          eResponseStatus.OK)
                .SetName("SetState_Trigger_ACTIVE_to_ARCHIVE");
        }

        private static IEnumerable SetBatchStateFromInactiveToActiveTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));

            BatchCampaign campaign = fixture.Create<BatchCampaign>();
            campaign.EndDate = date;
            long id = campaign.Id;
            campaign.State = CampaignState.INACTIVE;
            CampaignState newState = CampaignState.ACTIVE;
            IEnumerable<CampaignDB> campaignDbs = fixture.CreateMany<CampaignDB>(1);

            yield return new TestCaseData(id,
                                          newState,
                                          campaignDbs,
                                          campaign,
                                          eResponseStatus.OK)
                .SetName("SetState_Batch_INACTIVE_to_ACTIVE");
        }

        private static IEnumerable SetBatchStateFromActiveToArchiveTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));

            BatchCampaign campaign2 = fixture.Create<BatchCampaign>();
            campaign2.EndDate = date;
            long id2 = campaign2.Id;
            campaign2.State = CampaignState.ACTIVE;
            CampaignState newState2 = CampaignState.ARCHIVE;
            IEnumerable<CampaignDB> campaignDbs2 = fixture.CreateMany<CampaignDB>(1);

            yield return new TestCaseData(id2,
                                          newState2,
                                          campaignDbs2,
                                          campaign2,
                                          eResponseStatus.OK)
                .SetName("SetState_Batch_ACTIVE_to_ARCHIVE");
        }

        private static IEnumerable SetStateCampaignDoesNotExistTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));
            long id = 777;
            CampaignState newState = CampaignState.ACTIVE;
            IEnumerable<CampaignDB> campaignDbs = fixture.CreateMany<CampaignDB>(10);

            yield return new TestCaseData(id,
                                          newState,
                                          campaignDbs,
                                          new TriggerCampaign() { },
                                          eResponseStatus.CampaignDoesNotExist)
                .SetName("SetStateError_Trigger_CAMPAIGN_NOT_FOUND");
        }

        private static IEnumerable SetTriggerStateInvalidCampaignStateTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));

            TriggerCampaign campaign2 = fixture.Create<TriggerCampaign>();
            campaign2.EndDate = date;
            long id2 = campaign2.Id;
            campaign2.State = CampaignState.ACTIVE;
            CampaignState newState2 = CampaignState.ACTIVE;
            IEnumerable<CampaignDB> campaignDbs2 = fixture.CreateMany<CampaignDB>(2);

            yield return new TestCaseData(id2,
                                          newState2,
                                          campaignDbs2,
                                          campaign2,
                                          eResponseStatus.InvalidCampaignState)
                .SetName("SetStateError_Trigger_SAME_STATE");
        }

        private static IEnumerable SetTriggerStateCampaignStateUpdateNotAllowedArchiveToInactiveTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));
            
            TriggerCampaign campaign3 = fixture.Create<TriggerCampaign>();
            campaign3.EndDate = date;
            long id3 = campaign3.Id;
            campaign3.State = CampaignState.ARCHIVE;
            CampaignState newState3 = CampaignState.INACTIVE;
            IEnumerable<CampaignDB> campaignDbs3 = fixture.CreateMany<CampaignDB>(1);

            yield return new TestCaseData(id3,
                                          newState3,
                                          campaignDbs3,
                                          campaign3,
                                          eResponseStatus.CampaignStateUpdateNotAllowed)
                .SetName("SetStateError_Trigger_ARCHIVE_to_INACTIVE");
        }

        private static IEnumerable SetTriggerStateCampaignStateUpdateNotAllowedArchiveToActiveTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));
            
            TriggerCampaign campaign4 = fixture.Create<TriggerCampaign>();
            campaign4.EndDate = date;
            long id4 = campaign4.Id;
            campaign4.State = CampaignState.ARCHIVE;
            CampaignState newState4 = CampaignState.ACTIVE;
            IEnumerable<CampaignDB> campaignDbs4 = fixture.CreateMany<CampaignDB>(1);

            yield return new TestCaseData(id4,
                                          newState4,
                                          campaignDbs4,
                                          campaign4,
                                          eResponseStatus.CampaignStateUpdateNotAllowed)
                .SetName("SetStateError_Trigger_ARCHIVE_to_ACTIVE");
        }

        private static IEnumerable SetTriggerStateCampaignStateUpdateNotAllowedActiveToInactiveTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));
            TriggerCampaign campaign5 = fixture.Create<TriggerCampaign>();
            campaign5.EndDate = date;
            long id5 = campaign5.Id;
            campaign5.State = CampaignState.ACTIVE;
            CampaignState newState5 = CampaignState.INACTIVE;
            IEnumerable<CampaignDB> campaignDbs5 = fixture.CreateMany<CampaignDB>(1);

            yield return new TestCaseData(id5,
                                          newState5,
                                          campaignDbs5,
                                          campaign5,
                                          eResponseStatus.CampaignStateUpdateNotAllowed)
                .SetName("SetStateError_Trigger_ACTIVE_to_INACTIVE");
        }

        private static IEnumerable SetTriggerStateCampaignStateUpdateNotAllowedInactiveToArchiveTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));
            TriggerCampaign campaign6 = fixture.Create<TriggerCampaign>();
            campaign6.EndDate = date;
            long id6 = campaign6.Id;
            campaign6.State = CampaignState.INACTIVE;
            CampaignState newState6 = CampaignState.ARCHIVE;
            IEnumerable<CampaignDB> campaignDbs6 = fixture.CreateMany<CampaignDB>(1);

            yield return new TestCaseData(id6,
                                          newState6,
                                          campaignDbs6,
                                          campaign6,
                                          eResponseStatus.CampaignStateUpdateNotAllowed)
                .SetName("SetStateError_Trigger_INACTIVE_to_ARCHIVE");
        }

        private static IEnumerable SetTriggerStateExceededMaxCapacityTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));
            TriggerCampaign campaign7 = fixture.Create<TriggerCampaign>();

            campaign7.EndDate = date;
            campaign7.State = CampaignState.INACTIVE;
            IEnumerable<CampaignDB> campaignDbs7 = fixture.CreateMany<CampaignDB>(100);
            campaignDbs7 = campaignDbs7.Select(cmp =>
            {
                cmp.EndDate = date;
                cmp.State = CampaignState.ACTIVE;
                cmp.type = (int)eCampaignType.Trigger;
                return cmp;
            });

            yield return new TestCaseData(campaign7.Id,
                                          CampaignState.ACTIVE,
                                          campaignDbs7,
                                          campaign7,
                                          eResponseStatus.ExceededMaxCapacity)
                .SetName("SetStateError_Trigger_Exceeded_Max_Capacity");
        }

        private static IEnumerable SetTriggerStateInvalidCampaignEndDateTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));
            TriggerCampaign campaign8 = fixture.Create<TriggerCampaign>();
            campaign8.EndDate = DateUtils.GetUtcUnixTimestampNow() - 900000;
            long id8 = campaign8.Id;
            campaign8.State = CampaignState.INACTIVE;
            CampaignState newState8 = CampaignState.ACTIVE;
            IEnumerable<CampaignDB> campaignDbs8 = fixture.CreateMany<CampaignDB>(1);

            yield return new TestCaseData(id8,
                                          newState8,
                                          campaignDbs8,
                                          campaign8,
                                          eResponseStatus.InvalidCampaignEndDate)
                .SetName("SetStateError_Trigger_CAMPAIGN_ENDED");
        }

        private static IEnumerable SetBatchStateInvalidCampaignStateTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));

            BatchCampaign campaign2 = fixture.Create<BatchCampaign>();
            campaign2.EndDate = date;
            long id2 = campaign2.Id;
            campaign2.State = CampaignState.ACTIVE;
            CampaignState newState2 = CampaignState.ACTIVE;
            IEnumerable<CampaignDB> campaignDbs2 = fixture.CreateMany<CampaignDB>(2);

            yield return new TestCaseData(id2,
                                          newState2,
                                          campaignDbs2,
                                          campaign2,
                                          eResponseStatus.InvalidCampaignState)
                .SetName("SetStateError_Batch_SAME_STATE");
        }

        private static IEnumerable SetBatchStateCampaignStateUpdateNotAllowedArchiveToInactiveTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));
            BatchCampaign campaign3 = fixture.Create<BatchCampaign>();
            campaign3.EndDate = date;
            long id3 = campaign3.Id;
            campaign3.State = CampaignState.ARCHIVE;
            CampaignState newState3 = CampaignState.INACTIVE;
            IEnumerable<CampaignDB> campaignDbs3 = fixture.CreateMany<CampaignDB>(1);

            yield return new TestCaseData(id3,
                                          newState3,
                                          campaignDbs3,
                                          campaign3,
                                          eResponseStatus.CampaignStateUpdateNotAllowed)
                .SetName("SetStateError_Batch_ARCHIVE_to_INACTIVE");
        }

        private static IEnumerable SetBatchStateCampaignStateUpdateNotAllowedArchiveToActiveTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));
            BatchCampaign campaign4 = fixture.Create<BatchCampaign>();
            campaign4.EndDate = date;
            long id4 = campaign4.Id;
            campaign4.State = CampaignState.ARCHIVE;
            CampaignState newState4 = CampaignState.ACTIVE;
            IEnumerable<CampaignDB> campaignDbs4 = fixture.CreateMany<CampaignDB>(1);

            yield return new TestCaseData(id4,
                                          newState4,
                                          campaignDbs4,
                                          campaign4,
                                          eResponseStatus.CampaignStateUpdateNotAllowed)
                .SetName("SetStateError_Batch_ARCHIVE_to_ACTIVE");
        }

        private static IEnumerable SetBatchStateCampaignStateUpdateNotAllowedActiveToInactiveTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));
            BatchCampaign campaign5 = fixture.Create<BatchCampaign>();
            campaign5.EndDate = date;
            long id5 = campaign5.Id;
            campaign5.State = CampaignState.ACTIVE;
            CampaignState newState5 = CampaignState.INACTIVE;
            IEnumerable<CampaignDB> campaignDbs5 = fixture.CreateMany<CampaignDB>(1);

            yield return new TestCaseData(id5,
                                          newState5,
                                          campaignDbs5,
                                          campaign5,
                                          eResponseStatus.CampaignStateUpdateNotAllowed)
                .SetName("SetStateError_Batch_ACTIVE_to_INACTIVE");
        }

        private static IEnumerable SetBatchStateCampaignStateUpdateNotAllowedInactiveToArchiveTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));
            BatchCampaign campaign6 = fixture.Create<BatchCampaign>();
            campaign6.EndDate = date;
            long id6 = campaign6.Id;
            campaign6.State = CampaignState.INACTIVE;
            CampaignState newState6 = CampaignState.ARCHIVE;
            IEnumerable<CampaignDB> campaignDbs6 = fixture.CreateMany<CampaignDB>(1);

            yield return new TestCaseData(id6,
                                          newState6,
                                          campaignDbs6,
                                          campaign6,
                                          eResponseStatus.CampaignStateUpdateNotAllowed)
                .SetName("SetStateError_Batch_INACTIVE_to_ARCHIVE");
        }

        private static IEnumerable SetBatchStateExceededMaxCapacityTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));
            BatchCampaign campaign7 = fixture.Create<BatchCampaign>();

            campaign7.EndDate = date;
            campaign7.State = CampaignState.INACTIVE;
            IEnumerable<CampaignDB> campaignDbs7 = fixture.CreateMany<CampaignDB>(100);
            campaignDbs7 = campaignDbs7.Select(cmp =>
            {
                cmp.EndDate = date;
                cmp.State = CampaignState.ACTIVE;
                cmp.type = (int)eCampaignType.Batch;
                return cmp;
            });

            yield return new TestCaseData(campaign7.Id,
                                          CampaignState.ACTIVE,
                                          campaignDbs7,
                                          campaign7,
                                          eResponseStatus.ExceededMaxCapacity)
                .SetName("SetStateError_Batch_Exceeded_Max_Capacity");
        }

        private static IEnumerable SetBatchStateInvalidCampaignEndDateTestCase()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(RuleCondition), typeof(SegmentsCondition)));
            BatchCampaign campaign8 = fixture.Create<BatchCampaign>();
            campaign8.EndDate = DateUtils.GetUtcUnixTimestampNow() - 900000;
            long id8 = campaign8.Id;
            campaign8.State = CampaignState.INACTIVE;
            CampaignState newState8 = CampaignState.ACTIVE;
            IEnumerable<CampaignDB> campaignDbs8 = fixture.CreateMany<CampaignDB>(1);

            yield return new TestCaseData(id8,
                                          newState8,
                                          campaignDbs8,
                                          campaign8,
                                          eResponseStatus.InvalidCampaignEndDate)
                .SetName("SetStateError_Batch_CAMPAIGN_ENDED");
        }

        private Mock<ILayeredCache> GetLayeredCacheMock<T>(IEnumerable<CampaignDB> camapaignDbs, T campaign, ContextData contextData) where T : ApiObjects.Campaign, new()
        {
            var layeredCacheMock = new Mock<ILayeredCache>();
            layeredCacheMock.Setup(x => x.Get(It.IsAny<string>(),
                                             ref It.Ref<IEnumerable<CampaignDB>>.IsAny,
                                             It.IsAny<Func<Dictionary<string, object>, Tuple<IEnumerable<CampaignDB>, bool>>>(),
                                             It.IsAny<Dictionary<string, object>>(),
                                             It.IsAny<int>(),
                                             It.IsAny<string>(),
                                             It.IsAny<List<string>>(),
                                             It.IsAny<bool>()))
                           .Callback(new MockGetCampaignDBFromCache((string key,
                                                          ref IEnumerable<CampaignDB> genericParameter,
                                                          Func<Dictionary<string, object>, Tuple<IEnumerable<CampaignDB>, bool>> fillObjectMethod,
                                                          Dictionary<string, object> funcParameters,
                                                          int groupId,
                                                          string layeredCacheConfigName,
                                                          List<string> inValidationKeys,
                                                          bool shouldUseAutoNameTypeHandling) =>
                           {
                               genericParameter = camapaignDbs;
                           }))
                          .Returns(true);

            foreach (var otherCampaign in camapaignDbs)
            {
                var cmp = new T()
                {
                    Id = otherCampaign.Id,
                    StartDate = otherCampaign.StartDate,
                    EndDate = otherCampaign.EndDate,
                    HasPromotion = otherCampaign.HasPromotion,
                    State = otherCampaign.State,
                    type = otherCampaign.type
                };

                SetMockGetCampaignFromCache(layeredCacheMock, cmp, contextData);
            }

            SetMockGetCampaignFromCache(layeredCacheMock, campaign, contextData);

            layeredCacheMock.Setup(x => x.SetInvalidationKey(It.IsAny<string>(), null))
                        .Returns(true);

            return layeredCacheMock;
        }

        private void SetMockGetCampaignFromCache(Mock<ILayeredCache> layeredCacheMock, ApiObjects.Campaign campaign, ContextData contextData)
        {
            if (campaign.Id <= 0) { return; }
            
            var key = LayeredCacheKeys.GetCampaignKey(contextData.GroupId, campaign.Id);
            layeredCacheMock.Setup(x => x.Get(key,
                                             ref It.Ref<ApiObjects.Campaign>.IsAny,
                                             It.IsAny<Func<Dictionary<string, object>, Tuple<ApiObjects.Campaign, bool>>>(),
                                             It.IsAny<Dictionary<string, object>>(),
                                             It.IsAny<int>(),
                                             It.IsAny<string>(),
                                             It.IsAny<List<string>>(),
                                             It.IsAny<bool>()))
                           .Callback(new MockGetCampaignFromCache((string key,
                                                          ref ApiObjects.Campaign genericParameter,
                                                          Func<Dictionary<string, object>, Tuple<ApiObjects.Campaign, bool>> fillObjectMethod,
                                                          Dictionary<string, object> funcParameters,
                                                          int groupId,
                                                          string layeredCacheConfigName,
                                                          List<string> inValidationKeys,
                                                          bool shouldUseAutoNameTypeHandling) =>
                           {
                               genericParameter = campaign;
                           }))
                          .Returns(true);
        }
    }
}
