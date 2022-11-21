using System.Collections.Generic;
using ApiLogic.Api.Managers;
using ApiLogic.Api.Managers.Rule;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using ApiObjects.Rules;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.Api.Managers.Rule
{
    [TestFixture]
    public class ShopMarkerServiceTests
    {
        private MockRepository _mockRepository;
        private Mock<ICatalogPartnerConfigManager> _catalogPartnerConfigManagerMock;
        private Mock<ITopicManager> _topicManagerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _catalogPartnerConfigManagerMock = _mockRepository.Create<ICatalogPartnerConfigManager>();
            _topicManagerMock = _mockRepository.Create<ITopicManager>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void GetShopMarkerTopic_GetCatalogConfigFailed_ReturnsExpectedResult()
        {
            var expectedStatus = new Status(eResponseStatus.Error);
            _catalogPartnerConfigManagerMock
                .Setup(x => x.GetCatalogConfig(1))
                .Returns(new GenericResponse<CatalogPartnerConfig>(expectedStatus, new CatalogPartnerConfig { ShopMarkerMetaId = 2 }));
            var service = new ShopMarkerService(_catalogPartnerConfigManagerMock.Object, _topicManagerMock.Object);

            var result = service.GetShopMarkerTopic(1);

            result.Should().NotBeNull();
            result.Status.Should().Be(expectedStatus);
            result.Object.Should().BeNull();
        }

        [Test]
        public void GetShopMarkerTopic_ShopMarkerMetaIdIsNull_ReturnsExpectedResult()
        {
            _catalogPartnerConfigManagerMock
                .Setup(x => x.GetCatalogConfig(1))
                .Returns(new GenericResponse<CatalogPartnerConfig>(Status.Ok, new CatalogPartnerConfig()));
            var service = new ShopMarkerService(_catalogPartnerConfigManagerMock.Object, _topicManagerMock.Object);

            var result = service.GetShopMarkerTopic(1);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(eResponseStatus.TopicNotFound));
            result.Object.Should().BeNull();
        }

        [Test]
        public void GetShopMarkerTopic_TopicNotFound_ReturnsExpectedResult()
        {
            _catalogPartnerConfigManagerMock
                .Setup(x => x.GetCatalogConfig(1))
                .Returns(new GenericResponse<CatalogPartnerConfig>(Status.Ok, new CatalogPartnerConfig { ShopMarkerMetaId = 2 }));
            _topicManagerMock
                .Setup(x => x.GetTopicsByIds(1, It.Is<List<long>>(_ => _.Count == 1 && _[0] == 2), MetaType.All))
                .Returns(new GenericListResponse<Topic>(Status.Error, null));
            var service = new ShopMarkerService(_catalogPartnerConfigManagerMock.Object, _topicManagerMock.Object);

            var result = service.GetShopMarkerTopic(1);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(eResponseStatus.TopicNotFound));
            result.Object.Should().BeNull();
        }

        [Test]
        public void GetShopMarkerTopic_TopicExists_ReturnsExpectedResult()
        {
            var expectedTopic = new Topic();
            _catalogPartnerConfigManagerMock
                .Setup(x => x.GetCatalogConfig(1))
                .Returns(new GenericResponse<CatalogPartnerConfig>(Status.Ok, new CatalogPartnerConfig { ShopMarkerMetaId = 2 }));
            _topicManagerMock
                .Setup(x => x.GetTopicsByIds(1, It.Is<List<long>>(_ => _.Count == 1 && _[0] == 2), MetaType.All))
                .Returns(new GenericListResponse<Topic>(Status.Ok, new List<Topic> { expectedTopic }));
            var service = new ShopMarkerService(_catalogPartnerConfigManagerMock.Object, _topicManagerMock.Object);

            var result = service.GetShopMarkerTopic(1);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Should().Be(expectedTopic);
        }

        [Test]
        public void SetShopMarkerMeta_AssetUserRuleHasNoShopCondition_ReturnsError()
        {
            var service = new ShopMarkerService(_catalogPartnerConfigManagerMock.Object, _topicManagerMock.Object);

            var result = service.SetShopMarkerMeta(1, new AssetStruct(), new Asset(), new AssetUserRule { Conditions = new List<AssetConditionBase> { new AssetCondition() } });

            result.Should().Be(Status.Error);
        }

        [Test]
        public void SetShopMarkerMeta_GetShopMarkerTopicFailed_ReturnsExpectedResult()
        {
            var getShopMarkerTopicStatus = new Status(eResponseStatus.Error, "GetShopMarkerTopic Error");
            _catalogPartnerConfigManagerMock
                .Setup(x => x.GetCatalogConfig(1))
                .Returns(new GenericResponse<CatalogPartnerConfig>(getShopMarkerTopicStatus));
            var service = new ShopMarkerService(_catalogPartnerConfigManagerMock.Object, _topicManagerMock.Object);

            var result = service.SetShopMarkerMeta(1, new AssetStruct(), new Asset(), new AssetUserRule { Conditions = new List<AssetConditionBase> { new AssetCondition(), new AssetShopCondition { Values = new List<string> { "shopValue" } } } });

            result.Should().Be(getShopMarkerTopicStatus);
        }

        [Test]
        public void SetShopMarkerMeta_AssetStructNotContainsShopMarkerMeta_MetaIsNotAddedAndReturnsOk()
        {
            var asset = new Asset { Metas = new List<Metas> { new Metas(new TagMeta(), "metaValue") } };
            _catalogPartnerConfigManagerMock
                .Setup(x => x.GetCatalogConfig(1))
                .Returns(new GenericResponse<CatalogPartnerConfig>(Status.Ok, new CatalogPartnerConfig { ShopMarkerMetaId = 2 }));
            _topicManagerMock
                .Setup(x => x.GetTopicsByIds(1, It.Is<List<long>>(_ => _.Count == 1 && _[0] == 2), MetaType.All))
                .Returns(new GenericListResponse<Topic>(Status.Ok, new List<Topic> { new Topic { Id = 2, SystemName = "shopMakerTopic", Type = MetaType.Number } }));
            var service = new ShopMarkerService(_catalogPartnerConfigManagerMock.Object, _topicManagerMock.Object);

            var result = service.SetShopMarkerMeta(1, new AssetStruct { MetaIds = new List<long> { 2 } }, asset, new AssetUserRule { Conditions = new List<AssetConditionBase> { new AssetCondition(), new AssetShopCondition { Values = new List<string> { "shopValue" } } } });

            result.Should().Be(Status.Ok);
            asset.Metas.Should().NotBeNull();
            asset.Metas.Should().Contain(x => x.m_oTagMeta.m_sName == "shopMakerTopic" && x.m_oTagMeta.m_sType == "Number" && x.m_sValue == "shopValue");
            asset.Metas.Count.Should().Be(2);
        }

        [Test]
        public void SetShopMarkerMeta_ShopMarkerMetaNotExists_MetaIsAddedAndReturnsOk()
        {
            var asset = new Asset { Metas = new List<Metas> { new Metas(new TagMeta(), "metaValue") } };
            _catalogPartnerConfigManagerMock
                .Setup(x => x.GetCatalogConfig(1))
                .Returns(new GenericResponse<CatalogPartnerConfig>(Status.Ok, new CatalogPartnerConfig { ShopMarkerMetaId = 2 }));
            _topicManagerMock
                .Setup(x => x.GetTopicsByIds(1, It.Is<List<long>>(_ => _.Count == 1 && _[0] == 2), MetaType.All))

                .Returns(new GenericListResponse<Topic>(Status.Ok, new List<Topic> { new Topic { Id = 2, SystemName = "shopMakerTopic", Type = MetaType.Number } }));
            var service = new ShopMarkerService(_catalogPartnerConfigManagerMock.Object, _topicManagerMock.Object);

            var result = service.SetShopMarkerMeta(1, new AssetStruct { MetaIds = new List<long> { 2 } }, asset, new AssetUserRule { Conditions = new List<AssetConditionBase> { new AssetCondition(), new AssetShopCondition { Values = new List<string> { "shopValue" } } } });

            result.Should().Be(Status.Ok);
            asset.Metas.Should().NotBeNull();
            asset.Metas.Should().Contain(x => x.m_oTagMeta.m_sName == "shopMakerTopic" && x.m_oTagMeta.m_sType == "Number" && x.m_sValue == "shopValue");
            asset.Metas.Count.Should().Be(2);
        }

        [Test]
        public void SetShopMarkerMeta_ShopMarkerMetaExists_MetaIsNotChangedAndReturnsOk()
        {
            var asset = new Asset { Metas = new List<Metas> { new Metas(new TagMeta(), "metaValue"), new Metas(new TagMeta("shopMakerTopic", "Number"), "userShopValue") } };
            _catalogPartnerConfigManagerMock
                .Setup(x => x.GetCatalogConfig(1))
                .Returns(new GenericResponse<CatalogPartnerConfig>(Status.Ok, new CatalogPartnerConfig { ShopMarkerMetaId = 2 }));
            _topicManagerMock
                .Setup(x => x.GetTopicsByIds(1, It.Is<List<long>>(_ => _.Count == 1 && _[0] == 2), MetaType.All))
                .Returns(new GenericListResponse<Topic>(Status.Ok, new List<Topic> { new Topic { SystemName = "shopMakerTopic", Type = MetaType.String } }));
            var service = new ShopMarkerService(_catalogPartnerConfigManagerMock.Object, _topicManagerMock.Object);
            var result = service.SetShopMarkerMeta(1, new AssetStruct(), asset, new AssetUserRule { Conditions = new List<AssetConditionBase> { new AssetCondition(), new AssetShopCondition { Values = new List<string> { "shopValue" } } } });

            result.Should().Be(Status.Ok);
            asset.Metas.Should().NotBeNull();
            asset.Metas.Should().Contain(x => x.m_oTagMeta.m_sName == "shopMakerTopic" && x.m_oTagMeta.m_sType == "Number" && x.m_sValue == "userShopValue");
            asset.Metas.Count.Should().Be(2);
        }
    }
}