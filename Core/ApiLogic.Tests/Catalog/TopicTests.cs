using ApiLogic.Catalog;
using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Response;
using AutoFixture;
using Core.Catalog.CatalogManagement;
using Moq;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TVinciShared;
using ApiObjects;
using Tvinci.Core.DAL;
using Core.Catalog;
using ApiLogic.Api.Managers;

namespace ApiLogic.Tests.Catalog
{
    [TestFixture]
    public class TopicTests
    {
        delegate void MockInsertTopic(int groupId, string name, List<KeyValuePair<string, string>> namesInOtherLanguages, string systemName, ApiObjects.MetaType topicType, string commaSeparatedFeatures,
                                            bool? isPredefined, long? parent_topic_id, string helpText, long userId, bool shouldCheckRegularFlowValidations, Dictionary<string, string> dynamicData, out long id);
        delegate void MockUpdateTopic(int groupId, long id, string name, bool shouldUpdateOtherNames, List<KeyValuePair<string, string>> namesInOtherLanguages, string commaSeparatedFeatures,
                                            long? parent_topic_id, string helpText, long userId, Dictionary<string, string> dynamicData);
        delegate void MockDeleteTopic(int groupId, long id, long userId);

        [Test]
        public void CheckTopicAdd()
        {
            var fixture = new Fixture();
            var topicToAdd = fixture.Create<Topic>();
            topicToAdd.ParentId = null;

            var catalogManager = new Mock<ICatalogManager>();
            catalogManager.Setup(x => x.InvalidateCatalogGroupCache(It.IsAny<int>(), It.IsAny<Status>(), It.IsAny<bool>(), It.IsAny<object>()));

            catalogManager.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>()))
            .Returns(true);

            var cgc = fixture.Create<Core.Catalog.CatalogGroupCache>();
            catalogManager.Setup(x => x.TryGetCatalogGroupCacheFromCache(It.IsAny<int>(), out cgc))
            .Returns(true);

            var dalMock = new Mock<ITopicRepository>();
            dalMock.Setup(x => x.InsertTopic(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>(),
                                             It.IsAny<string>(), It.IsAny<ApiObjects.MetaType>(), It.IsAny<string>(),
                                             It.IsAny<bool?>(), It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<long>(),
                                             It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>(), out It.Ref<long>.IsAny))
                           .Callback(new MockInsertTopic((int groupId, string name, List<KeyValuePair<string, string>> namesInOtherLanguages,
                                                          string systemName, ApiObjects.MetaType topicType, string commaSeparatedFeatures,
                                                          bool? isPredefined, long? parent_topic_id, string helpText, long userId,
                                                          bool shouldCheckRegularFlowValidations, Dictionary<string, string> dynamicData, out long id) =>
                           {
                               id = fixture.Create<long>();
                           }))
                          .Returns(fixture.Create<Topic>());

            var virtualAssetPartnerConfigManagerMock = Mock.Of<IVirtualAssetPartnerConfigManager>();
            var elasticsearchWrapperMock = Mock.Of<IIndexManagerFactory>();
            var groupsCacheMock = Mock.Of<GroupsCacheManager.IGroupsCache>();
            var conditionalAccessMock = Mock.Of<Core.ConditionalAccess.IConditionalAccessUtils>();
            var notificationCacheMock = Mock.Of<Core.Notification.INotificationCache>();

            var manager = new TopicManager(catalogManager.Object, dalMock.Object, virtualAssetPartnerConfigManagerMock,
                elasticsearchWrapperMock, groupsCacheMock, conditionalAccessMock, notificationCacheMock);

            var testAddResult = manager.AddTopic(fixture.Create<int>(), topicToAdd, fixture.Create<long>(), fixture.Create<bool>());

            Assert.That(testAddResult.Status.Code, Is.EqualTo((int)eResponseStatus.OK));
        }

        [Test]
        public void CheckTopicUpdate()
        {
            var fixture = new Fixture();
            var topicToUpdate = fixture.Create<Topic>();
            topicToUpdate.ParentId = null;

            var catalogManager = new Mock<ICatalogManager>();
            catalogManager.Setup(x => x.InvalidateCatalogGroupCache(It.IsAny<int>(), It.IsAny<Status>(), It.IsAny<bool>(), It.IsAny<object>()));

            catalogManager.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>()))
            .Returns(true);

            var cgc = fixture.Create<Core.Catalog.CatalogGroupCache>();
            catalogManager.Setup(x => x.TryGetCatalogGroupCacheFromCache(It.IsAny<int>(), out cgc))
            .Returns(true);

            cgc.TopicsMapById = new Dictionary<long, Topic>();
            cgc.TopicsMapById.Add(topicToUpdate.Id, topicToUpdate);

            var dalMock = new Mock<ITopicRepository>();
            dalMock.Setup(x => x.UpdateTopic(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<List<KeyValuePair<string, string>>>(), It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<string>(),
                It.IsAny<long>(), It.IsAny<Dictionary<string, string>>()))
                           .Callback(new MockUpdateTopic((int groupId, long id, string name, bool shouldUpdateOtherNames, List<KeyValuePair<string, string>> namesInOtherLanguages, string commaSeparatedFeatures,
                                            long? parent_topic_id, string helpText, long userId, Dictionary<string, string> dynamicData) =>
                           {
                               id = fixture.Create<long>();
                           }))
                          .Returns(fixture.Create<Topic>());

            var virtualAssetPartnerConfigManagerMock = Mock.Of<IVirtualAssetPartnerConfigManager>();
            var elasticsearchWrapperMock = Mock.Of<IIndexManagerFactory>();
            var groupsCacheMock = Mock.Of<GroupsCacheManager.IGroupsCache>();
            var conditionalAccessMock = Mock.Of<Core.ConditionalAccess.IConditionalAccessUtils>();
            var notificationCacheMock = Mock.Of<Core.Notification.INotificationCache>();

            var manager = new TopicManager(catalogManager.Object, dalMock.Object, virtualAssetPartnerConfigManagerMock,
                elasticsearchWrapperMock, groupsCacheMock, conditionalAccessMock, notificationCacheMock);

            var testUpdateResult = manager.UpdateTopic(fixture.Create<int>(), topicToUpdate.Id, topicToUpdate, fixture.Create<long>());

            Assert.That(testUpdateResult.Status.Code, Is.EqualTo((int)eResponseStatus.OK));
        }

        [Test]
        public void CheckTopicDelete()
        {
            var fixture = new Fixture();
            var topicToDelete = fixture.Create<Topic>();
            topicToDelete.ParentId = null;

            var catalogManager = new Mock<ICatalogManager>();
            catalogManager.Setup(x => x.InvalidateCatalogGroupCache(It.IsAny<int>(), It.IsAny<Status>(), It.IsAny<bool>(), It.IsAny<object>()));
            catalogManager.Setup(x => x.InvalidateCacheAndUpdateIndexForTopicAssets(It.IsAny<int>(), It.IsAny<List<long>>(), It.IsAny<bool>(),
                It.IsAny<bool>(), It.IsAny<List<long>>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<long>>(), It.IsAny<bool>()))
                .Returns(true);
            catalogManager.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>()))
            .Returns(true);

            var cgc = fixture.Create<Core.Catalog.CatalogGroupCache>();
            catalogManager.Setup(x => x.TryGetCatalogGroupCacheFromCache(It.IsAny<int>(), out cgc))
            .Returns(true);

            cgc.TopicsMapById = new Dictionary<long, Topic>();
            cgc.TopicsMapById.Add(topicToDelete.Id, topicToDelete);

            var dalMock = new Mock<ITopicRepository>();
            dalMock.Setup(x => x.DeleteTopic(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
                          .Callback(new MockDeleteTopic((int groupId, long id, long userId) => { }))
                          .Returns(true);

            var virtualAssetPartnerConfigManagerMock = new Mock<IVirtualAssetPartnerConfigManager>();
            virtualAssetPartnerConfigManagerMock.Setup(x => x.GetObjectVirtualAssetPartnerConfiguration(It.IsAny<int>()))
                .Returns(new GenericListResponse<ObjectVirtualAssetPartnerConfig>() { Objects = fixture.Create<List<ObjectVirtualAssetPartnerConfig>>() });

            var elasticsearchWrapperMock = Mock.Of<IIndexManagerFactory>();
            var groupsCacheMock = Mock.Of<GroupsCacheManager.IGroupsCache>();
            var conditionalAccessMock = Mock.Of<Core.ConditionalAccess.IConditionalAccessUtils>();
            var notificationCacheMock = Mock.Of<Core.Notification.INotificationCache>();

            var manager = new TopicManager(catalogManager.Object, dalMock.Object, virtualAssetPartnerConfigManagerMock.Object,
                elasticsearchWrapperMock, groupsCacheMock, conditionalAccessMock, notificationCacheMock);

            var result = manager.DeleteTopic(fixture.Create<int>(), topicToDelete.Id, fixture.Create<long>());

            Assert.That(result.Code, Is.EqualTo((int)eResponseStatus.OK));
        }

        //[TestCaseSource(nameof(TestTopicAddErrorsTestCases))]
        //public void CheckTopicAddWithError(Topic topicToAdd, eResponseStatus expectedError,
        //    bool doesGroupUsesTemplates, bool initCache, long? dbValueId, CatalogGroupCache catalogGroupCache)
        //{
        //    var fixture = new Fixture();

        //    var catalogManager = new Mock<ICatalogManager>();
        //    catalogManager.Setup(x => x.InvalidateCatalogGroupCache(It.IsAny<int>(), It.IsAny<Status>(), It.IsAny<bool>(), It.IsAny<object>()));
        //    catalogManager.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>()))
        //    .Returns(doesGroupUsesTemplates);

        //    catalogManager.Setup(x => x.TryGetCatalogGroupCacheFromCache(It.IsAny<int>(), out catalogGroupCache))
        //    .Returns(initCache);

        //    var dalMock = new Mock<ITopicRepository>();
        //    dalMock.Setup(x => x.InsertTopic(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>(),
        //                                     It.IsAny<string>(), It.IsAny<MetaType>(), It.IsAny<string>(),
        //                                     It.IsAny<bool?>(), It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<long>(),
        //                                     It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>(), out It.Ref<long>.IsAny))
        //                   .Callback(new MockInsertTopic((int groupId, string name, List<KeyValuePair<string, string>> namesInOtherLanguages,
        //                                                  string systemName, MetaType topicType, string commaSeparatedFeatures,
        //                                                  bool? isPredefined, long? parent_topic_id, string helpText, long userId,
        //                                                  bool shouldCheckRegularFlowValidations, Dictionary<string, string> dynamicData, out long id) =>
        //                   {
        //                       id = dbValueId ?? fixture.Create<long>();
        //                   }))
        //                  .Returns(fixture.Create<Topic>());

        //    var virtualAssetPartnerConfigManagerMock = Mock.Of<Api.Managers.IVirtualAssetPartnerConfigManager>();
        //    var elasticsearchWrapperMock = Mock.Of<IIndexManager>();
        //    var groupsCacheMock = Mock.Of<GroupsCacheManager.IGroupsCache>();
        //    var conditionalAccessMock = Mock.Of<Core.ConditionalAccess.IConditionalAccessUtils>();
        //    var notificationCacheMock = Mock.Of<Core.Notification.INotificationCache>();

        //    var manager = new TopicManager(catalogManager.Object, dalMock.Object, virtualAssetPartnerConfigManagerMock,
        //        elasticsearchWrapperMock, groupsCacheMock, conditionalAccessMock, notificationCacheMock);

        //    var testAddResult = manager.AddTopic(fixture.Create<int>(), topicToAdd, fixture.Create<long>(), fixture.Create<bool>());

        //    Assert.That(testAddResult.Status.Code, Is.EqualTo((int)expectedError));
        //}

        private static IEnumerable TestTopicAddErrorsTestCases()
        {
            var fixture = new Fixture();

            // Account Is Not Opc Supported
            var object1 = fixture.Create<Topic>();
            var catalogGroupCache1 = fixture.Create<CatalogGroupCache>();
            yield return new TestCaseData(object1, eResponseStatus.AccountIsNotOpcSupported, false, true, null, catalogGroupCache1)
                .SetName("TestTopicAddErrors_AccountIsNotOpcSupported");

            //No catalogGroupCache
            var object2 = fixture.Create<Topic>();
            var catalogGroupCache2 = fixture.Create<CatalogGroupCache>();
            yield return new TestCaseData(object2, eResponseStatus.Error, true, false, null, catalogGroupCache2)
                .SetName("TestTopicAddErrors_NoCatalogGroupCache");

            //Meta System Name Already In Use
            var object3 = fixture.Create<Topic>();
            var catalogGroupCache3 = fixture.Create<CatalogGroupCache>();
            catalogGroupCache3.TopicsMapBySystemNameAndByType.Add(object3.SystemName, null);

            yield return new TestCaseData(object3, eResponseStatus.MetaSystemNameAlreadyInUse, true, true, null, catalogGroupCache3)
                .SetName("TestTopicAddErrors_MetaSystemNameAlreadyInUse1");

            //Parent Id Should Not Point To Itself
            var object4 = fixture.Create<Topic>();
            var catalogGroupCache4 = fixture.Create<CatalogGroupCache>();
            var _id = fixture.Create<long>();
            object4.Id = _id;
            object4.ParentId = _id;
            yield return new TestCaseData(object4, eResponseStatus.ParentIdShouldNotPointToItself, true, true, _id, catalogGroupCache4)
                .SetName("TestTopicAddErrors_ParentIdShouldNotPointToItself");

            //ParentIdNotExist
            var object5 = fixture.Create<Topic>();
            var catalogGroupCache5 = fixture.Create<CatalogGroupCache>();
            catalogGroupCache5.TopicsMapById = new Dictionary<long, Topic>();
            catalogGroupCache5.TopicsMapById.Add(object5.Id + 1, null);
            yield return new TestCaseData(object5, eResponseStatus.ParentIdNotExist, true, true, null, catalogGroupCache5)
                .SetName("TestTopicAddErrors_ParentIdNotExist");

            //MetaSystemNameAlreadyInUse
            var object6 = fixture.Create<Topic>();
            object6.ParentId = null;
            var catalogGroupCache6 = fixture.Create<CatalogGroupCache>();
            long val1 = -222;
            yield return new TestCaseData(object6, eResponseStatus.MetaSystemNameAlreadyInUse, true, true, val1, catalogGroupCache6)
                .SetName("TestTopicAddErrors_MetaSystemNameAlreadyInUse");

            //MetaDoesNotExist
            var object7 = fixture.Create<Topic>();
            object7.ParentId = null;
            var catalogGroupCache7 = fixture.Create<CatalogGroupCache>();
            long val2 = -333;
            yield return new TestCaseData(object7, eResponseStatus.MetaDoesNotExist, true, true, val2, catalogGroupCache7)
                .SetName("TestTopicAddErrors_MetaSystemNameAlreadyInUse");

            //General db error
            var object8 = fixture.Create<Topic>();
            object8.ParentId = null;
            var catalogGroupCache8 = fixture.Create<CatalogGroupCache>();
            long val3 = -500;
            yield return new TestCaseData(object8, eResponseStatus.Error, true, true, val3, catalogGroupCache8)
                .SetName("TestTopicAddErrors_GeneralDbError");
        }

        [Test]
        public void GetTopics()
        {
            var fixture = new Fixture();

            var catalogManager = new Mock<ICatalogManager>();
            catalogManager.Setup(x => x.InvalidateCatalogGroupCache(It.IsAny<int>(), It.IsAny<Status>(), It.IsAny<bool>(), It.IsAny<object>()));

            catalogManager.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>()))
            .Returns(true);

            var cgc = fixture.Create<Core.Catalog.CatalogGroupCache>();
            catalogManager.Setup(x => x.TryGetCatalogGroupCacheFromCache(It.IsAny<int>(), out cgc))
            .Returns(true);

            var _epgGroupSettings = fixture.Create<GroupsCacheManager.EpgGroupSettings>();
            var _groupMock = new GroupsCacheManager.Group(fixture.Create<int>(), fixture.Create<List<int>>(), _epgGroupSettings);

            var groupsCacheMock = new Mock<GroupsCacheManager.IGroupsCache>();
            groupsCacheMock.Setup(x => x.GetGroup(It.IsAny<int>()))
                .Returns(_groupMock);

            var notificationCacheMock = new Mock<Core.Notification.INotificationCache>();
            notificationCacheMock.Setup(x => x.GetPartnerTopicInterests(It.IsAny<int>()))
            .Returns(new List<Meta>());

            var dalMock = Mock.Of<ITopicRepository>();
            var virtualAssetPartnerConfigManagerMock = Mock.Of<IVirtualAssetPartnerConfigManager>();
            var elasticsearchWrapperMock = Mock.Of<IIndexManagerFactory>();
            var conditionalAccessMock = Mock.Of<Core.ConditionalAccess.IConditionalAccessUtils>();

            //Init test data
            var list = fixture.Create<List<long>>();
            cgc.TopicsMapById.Add(list.First(), fixture.Create<Topic>());

            var manager = new TopicManager(catalogManager.Object, dalMock, virtualAssetPartnerConfigManagerMock,
                elasticsearchWrapperMock, groupsCacheMock.Object, conditionalAccessMock, notificationCacheMock.Object);

            var testGetByStructResult = manager.GetTopicsByAssetStructId(fixture.Create<int>(), fixture.Create<long>(), fixture.Create<MetaType>());
            Assert.That(testGetByStructResult.Status.Code, Is.EqualTo((int)eResponseStatus.OK));

            var testGetByIds = manager.GetTopicsByIds(fixture.Create<int>(), list, MetaType.All);
            Assert.That(testGetByIds.Status.Code, Is.EqualTo((int)eResponseStatus.OK));
            Assert.That(testGetByIds.Objects.Count, Is.EqualTo(1));

            var getGroupMetaListResponse = manager.GetGroupMetaList(fixture.Create<int>(), fixture.Create<eAssetTypes>(),
                MetaType.All, MetaFieldName.All, MetaFieldName.All,
                fixture.Create<List<MetaFeatureType>>());

            Assert.That(getGroupMetaListResponse.MetaList.Count, Is.EqualTo(6));
            Assert.That(getGroupMetaListResponse.Status.Code, Is.EqualTo((int)eResponseStatus.OK));
        }
    }
}