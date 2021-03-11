using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects.Rules;
using AutoFixture;
using CachingProvider.LayeredCache;
using Core.Catalog.CatalogManagement;
using Core.Pricing;
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

        [TestCaseSource(nameof(SetStateBatchErrorsTestCases))]
        [TestCaseSource(nameof(SetStateTriggerErrorsTestCases))]
        [TestCaseSource(nameof(SetStateBatchTestCases))]
        [TestCaseSource(nameof(SetStateTriggerTestCases))]
        public void CheckSetState(long id, CampaignState newState, IEnumerable<CampaignDB> campaignDbs, ApiObjects.Campaign campaign, eResponseStatus status, ContextData contextData)
        {
            var layeredCacheMock = GetLayeredCacheMock(campaignDbs, campaign);

            var campaignRepositoryMock = new Mock<ICampaignRepository>();

            campaignRepositoryMock.Setup(x => x.Update_Campaign(It.IsAny<ApiObjects.Campaign>(),
                                                                It.IsAny<ContextData>()))
                                         .Returns(true);

            var pricingModuleMock = Mock.Of<IPricingModule>();
            var ChannelManagerMock = Mock.Of<IChannelManager>();
            var eventBusPublisherMock = Mock.Of<IEventBusPublisher>();
            var catalogManager = Mock.Of<ICatalogManager>();
            var groupsCache = Mock.Of<GroupsCacheManager.IGroupsCache>();

            CampaignManager manager = new CampaignManager(layeredCacheMock.Object,
                                                          campaignRepositoryMock.Object,
                                                          pricingModuleMock,
                                                          ChannelManagerMock,
                                                          eventBusPublisherMock,
                                                          catalogManager,
                                                          groupsCache);

            var response = manager.SetState(contextData, id, newState);

            Assert.That(response.Code, Is.EqualTo((int)status));
        }

        private static IEnumerable SetStateTriggerTestCases()
        {
            var fixture = new Fixture();
            var date = DateUtils.GetUtcUnixTimestampNow() + 900000;
            fixture.Register<RuleCondition>(() => null);
            var contextData = fixture.Create<ContextData>();

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
                                          eResponseStatus.OK, 
                                          contextData)
                .SetName("SetState_Trigger_INACTIVE_to_ACTIVE");


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
                                          eResponseStatus.OK,
                                          contextData)
                .SetName("SetState_Trigger_ACTIVE_to_ARCHIVE");
        }

        private static IEnumerable SetStateBatchTestCases()
        {
            var fixture = new Fixture();
            var date = DateUtils.GetUtcUnixTimestampNow() + 900000;
            fixture.Register<RuleCondition>(() => null);
            var contextData = fixture.Create<ContextData>();

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
                                          eResponseStatus.OK,
                                          contextData)
                .SetName("SetState_Batch_INACTIVE_to_ACTIVE");


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
                                          eResponseStatus.OK,
                                          contextData)
                .SetName("SetState_Batch_ACTIVE_to_ARCHIVE");
        }

        private static IEnumerable SetStateTriggerErrorsTestCases()
        {
            var fixture = new Fixture();
            var date = DateUtils.GetUtcUnixTimestampNow() + 900000;
            fixture.Register<RuleCondition>(() => null);
            var contextData = fixture.Create<ContextData>();

            long id = 777;
            CampaignState newState = CampaignState.ACTIVE;
            IEnumerable<CampaignDB> campaignDbs = fixture.CreateMany<CampaignDB>(10);

            yield return new TestCaseData(id,
                                          newState,
                                          campaignDbs,
                                          null,
                                          eResponseStatus.CampaignDoesNotExist,
                                          contextData)
                .SetName("SetStateError_Trigger_CAMPAIGN_NOT_FOUND");

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
                                          eResponseStatus.Error,
                                          contextData)
                .SetName("SetStateError_Trigger_SAME_STATE");

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
                                          eResponseStatus.Error,
                                          contextData)
                .SetName("SetStateError_Trigger_ARCHIVE_to_INACTIVE");

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
                                          eResponseStatus.Error,
                                          contextData)
                .SetName("SetStateError_Trigger_ARCHIVE_to_ACTIVE");

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
                                          eResponseStatus.Error,
                                          contextData)
                .SetName("SetStateError_Trigger_ACTIVE_to_INACTIVE");

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
                                          eResponseStatus.Error,
                                          contextData)
                .SetName("SetStateError_Trigger_INACTIVE_to_ARCHIVE");

            TriggerCampaign campaign7 = fixture.Create<TriggerCampaign>();
            campaign7.EndDate = date;
            long id7 = campaign7.Id;
            campaign7.State = CampaignState.INACTIVE;
            CampaignState newState7 = CampaignState.ACTIVE;
            IEnumerable<CampaignDB> campaignDbs7 = fixture.CreateMany<CampaignDB>(100);
            campaignDbs7 = campaignDbs7.Select(cmp =>
            {
                cmp.EndDate = date;
                cmp.State = CampaignState.ACTIVE;
                cmp.type = (int)eCampaignType.Trigger;
                return cmp;
            });

            yield return new TestCaseData(id7,
                                          newState7,
                                          campaignDbs7,
                                          campaign7,
                                          eResponseStatus.ExceededMaxCapacity,
                                          contextData)
                .SetName("SetStateError_Trigger_Exceeded_Max_Capacity");

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
                                          eResponseStatus.Error,
                                          contextData)
                .SetName("SetStateError_Trigger_CAMPAIGN_ENDED");
        }

        private static IEnumerable SetStateBatchErrorsTestCases()
        {
            var fixture = new Fixture();
            var date = DateUtils.GetUtcUnixTimestampNow() + 900000;
            fixture.Register<RuleCondition>(() => null);
            var contextData = fixture.Create<ContextData>();

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
                                          eResponseStatus.Error,
                                          contextData)
                .SetName("SetStateError_Batch_SAME_STATE");

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
                                          eResponseStatus.Error,
                                          contextData)
                .SetName("SetStateError_Batch_ARCHIVE_to_INACTIVE");

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
                                          eResponseStatus.Error,
                                          contextData)
                .SetName("SetStateError_Batch_ARCHIVE_to_ACTIVE");

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
                                          eResponseStatus.Error,
                                          contextData)
                .SetName("SetStateError_Batch_ACTIVE_to_INACTIVE");

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
                                          eResponseStatus.Error,
                                          contextData)
                .SetName("SetStateError_Batch_INACTIVE_to_ARCHIVE");

            BatchCampaign campaign7 = fixture.Create<BatchCampaign>();
            campaign7.EndDate = date;
            long id7 = campaign7.Id;
            campaign7.State = CampaignState.INACTIVE;
            CampaignState newState7 = CampaignState.ACTIVE;
            IEnumerable<CampaignDB> campaignDbs7 = fixture.CreateMany<CampaignDB>(100);
            campaignDbs7 = campaignDbs7.Select(cmp =>
            {
                cmp.EndDate = date;
                cmp.State = CampaignState.ACTIVE;
                cmp.type = (int)eCampaignType.Trigger;
                return cmp;
            });

            yield return new TestCaseData(id7,
                                          newState7,
                                          campaignDbs7,
                                          campaign7,
                                          eResponseStatus.ExceededMaxCapacity,
                                          contextData)
                .SetName("SetStateError_Batch_Exceeded_Max_Capacity");

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
                                          eResponseStatus.Error,
                                          contextData)
                .SetName("SetStateError_Batch_CAMPAIGN_ENDED");
        }

        private Mock<ILayeredCache> GetLayeredCacheMock(IEnumerable<CampaignDB> camapaignDbs, ApiObjects.Campaign campaign)
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

            layeredCacheMock.Setup(x => x.Get(It.IsAny<string>(),
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

                layeredCacheMock.Setup(x => x.SetInvalidationKey(It.IsAny<string>(), null))
                            .Returns(true);

            return layeredCacheMock;
        }
    }
}
