using System;
using ApiLogic.Api.Managers.Rule;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Rules;
using Core.Tests;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.Api.Managers.Rule
{
    [TestFixture]
    public class AssetConditionKsqlFactoryTests
    {
        private MockRepository _mockRepository;
        private Mock<IShopMarkerService> _shopMarkerServiceMock;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _shopMarkerServiceMock = _mockRepository.Create<IShopMarkerService>();
            _loggerMock = _mockRepository.Create<ILogger>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void GetKsql_ValidAssetCondition_ReturnsExpectedKsql()
        {
            var factory = new AssetConditionKsqlFactory(_shopMarkerServiceMock.Object, _loggerMock.Object);

            var result = factory.GetKsql(1, new AssetCondition { Ksql = "ksql" });

            result.Should().Be("ksql");
        }

        [Test]
        public void GetKsql_ValidAssetShopConditionAndGetShopMarkerTopicFailed_ThrowsException()
        {
            _shopMarkerServiceMock
                .Setup(x => x.GetShopMarkerTopic(1))
                .Returns(new GenericResponse<Topic>(new Status(eResponseStatus.TopicNotFound, "Topic Response Message")));
            _loggerMock
                .Setup(LogLevel.Error, "ShopMarkerMeta has not been determined and Ksql can not be built: shopMetaResponse.Status={2038 - Topic Response Message}.");
            var factory = new AssetConditionKsqlFactory(_shopMarkerServiceMock.Object, _loggerMock.Object);

            var exception = Assert.Throws<Exception>(() => factory.GetKsql(1, new AssetShopCondition { Value = "ShopMetaValue" }));

            exception.Message.Should().Be("Topic Response Message");
        }

        [Test]
        public void GetKsql_ValidAssetShopCondition_ReturnsExpectedKsql()
        {
            _shopMarkerServiceMock
                .Setup(x => x.GetShopMarkerTopic(1))
                .Returns(new GenericResponse<Topic>(Status.Ok, new Topic { SystemName = "ShopMetaName" }));
            var factory = new AssetConditionKsqlFactory(_shopMarkerServiceMock.Object, _loggerMock.Object);

            var result = factory.GetKsql(1, new AssetShopCondition { Value = "ShopMetaValue" });

            result.Should().Be("ShopMetaName='ShopMetaValue'");
        }
    }
}