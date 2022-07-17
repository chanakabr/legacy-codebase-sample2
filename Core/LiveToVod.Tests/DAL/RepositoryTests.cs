using System;
using Core.Tests;
using FluentAssertions;
using LiveToVod.BOL;
using LiveToVod.DAL;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using OTT.Lib.MongoDB;

namespace LiveToVod.Tests.DAL
{
    [TestFixture]
    public class RepositoryTests
    {
        private MockRepository _mockRepository;
        private Mock<IMongoDbClientFactory> _clientFactoryMock;
        private Mock<IMongoDbAdminClientFactory> _adminClientFactoryMock;
        private Mock<IMongoDbClient> _clientMock;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _clientFactoryMock = _mockRepository.Create<IMongoDbClientFactory>();
            _loggerMock = _mockRepository.Create<ILogger>();

            _clientMock = _mockRepository.Create<IMongoDbClient>();
            _clientFactoryMock
                .Setup(x => x.NewMongoDbClient(1, _loggerMock.Object))
                .Returns(_clientMock.Object);
            _adminClientFactoryMock = _mockRepository.Create<IMongoDbAdminClientFactory>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [TestCase("id", 0)]
        [TestCase("", 1)]
        public void UpsertPartnerConfiguration_DataIsUpdated_ReturnsTrue(string id, long modifiedCount)
        {
            _clientMock
                .Setup(x => x.UpdateOne(
                    "live_to_vod_partner_configurations",
                    It.IsAny<Func<FilterDefinitionBuilder<LiveToVodPartnerConfigurationData>, FilterDefinition<LiveToVodPartnerConfigurationData>>>(),
                    It.IsAny<Func<UpdateDefinitionBuilder<LiveToVodPartnerConfigurationData>, UpdateDefinition<LiveToVodPartnerConfigurationData>>>(),
                    It.Is<MongoDbUpdateOptions>(_ => _.IsUpsert)))
                .Returns(new MongoDbUpdateResult(new UpdateResult.Acknowledged(1, modifiedCount, id)));
            var repository = new Repository(_clientFactoryMock.Object, _adminClientFactoryMock.Object, _loggerMock.Object);

            var result = repository.UpsertPartnerConfiguration(1, new LiveToVodPartnerConfiguration(), 2);

            result.Should().BeTrue();
        }

        [Test]
        public void UpsertPartnerConfiguration_DataIsNotUpdated_ReturnsFalse()
        {
            _clientMock
                .Setup(x => x.UpdateOne(
                    "live_to_vod_partner_configurations",
                    It.IsAny<Func<FilterDefinitionBuilder<LiveToVodPartnerConfigurationData>, FilterDefinition<LiveToVodPartnerConfigurationData>>>(),
                    It.IsAny<Func<UpdateDefinitionBuilder<LiveToVodPartnerConfigurationData>, UpdateDefinition<LiveToVodPartnerConfigurationData>>>(),
                    It.Is<MongoDbUpdateOptions>(_ => _.IsUpsert)))
                .Returns(new MongoDbUpdateResult(new UpdateResult.Acknowledged(0, 0, "")));
            var repository = new Repository(_clientFactoryMock.Object, _adminClientFactoryMock.Object, _loggerMock.Object);

            var result = repository.UpsertPartnerConfiguration(1, new LiveToVodPartnerConfiguration(), 2);

            result.Should().BeFalse();
        }

        [Test]
        public void UpsertPartnerConfiguration_WrongMatchedCount_LogsError()
        {
            _clientMock
                .Setup(x => x.UpdateOne(
                    "live_to_vod_partner_configurations",
                    It.IsAny<Func<FilterDefinitionBuilder<LiveToVodPartnerConfigurationData>, FilterDefinition<LiveToVodPartnerConfigurationData>>>(),
                    It.IsAny<Func<UpdateDefinitionBuilder<LiveToVodPartnerConfigurationData>, UpdateDefinition<LiveToVodPartnerConfigurationData>>>(),
                    It.Is<MongoDbUpdateOptions>(_ => _.IsUpsert)))
                .Returns(new MongoDbUpdateResult(new UpdateResult.Acknowledged(2, 0, "")));
            _loggerMock
                .Setup(LogLevel.Error, "There have been found 2 LiveToVodPartnerConfiguration's documents in the database: partnerId=1.");
            var repository = new Repository(_clientFactoryMock.Object, _adminClientFactoryMock.Object, _loggerMock.Object);

            var result = repository.UpsertPartnerConfiguration(1, new LiveToVodPartnerConfiguration(), 2);

            result.Should().BeFalse();
        }

        [TestCase("id", 0)]
        [TestCase("", 1)]
        public void UpsertLinearAssetConfiguration_DataIsUpdated_ReturnsTrue(string id, long modifiedCount)
        {
            _clientMock
                .Setup(x => x.UpdateOne(
                    "live_to_vod_linear_asset_configurations",
                    It.IsAny<Func<FilterDefinitionBuilder<LiveToVodLinearAssetConfigurationData>, FilterDefinition<LiveToVodLinearAssetConfigurationData>>>(),
                    It.IsAny<Func<UpdateDefinitionBuilder<LiveToVodLinearAssetConfigurationData>, UpdateDefinition<LiveToVodLinearAssetConfigurationData>>>(),
                    It.Is<MongoDbUpdateOptions>(_ => _.IsUpsert)))
                .Returns(new MongoDbUpdateResult(new UpdateResult.Acknowledged(1, modifiedCount, id)));
            var repository = new Repository(_clientFactoryMock.Object, _adminClientFactoryMock.Object, _loggerMock.Object);

            var result = repository.UpsertLinearAssetConfiguration(1, new LiveToVodLinearAssetConfiguration(0, false), 2);

            result.Should().BeTrue();
        }

        [Test]
        public void UpsertLinearAssetConfiguration_DataIsNotUpdated_ReturnsFalse()
        {
            _clientMock
                .Setup(x => x.UpdateOne(
                    "live_to_vod_linear_asset_configurations",
                    It.IsAny<Func<FilterDefinitionBuilder<LiveToVodLinearAssetConfigurationData>, FilterDefinition<LiveToVodLinearAssetConfigurationData>>>(),
                    It.IsAny<Func<UpdateDefinitionBuilder<LiveToVodLinearAssetConfigurationData>, UpdateDefinition<LiveToVodLinearAssetConfigurationData>>>(),
                    It.Is<MongoDbUpdateOptions>(_ => _.IsUpsert)))
                .Returns(new MongoDbUpdateResult(new UpdateResult.Acknowledged(0, 0, "")));
            var repository = new Repository(_clientFactoryMock.Object, _adminClientFactoryMock.Object, _loggerMock.Object);

            var result = repository.UpsertLinearAssetConfiguration(1, new LiveToVodLinearAssetConfiguration(0, false), 2);

            result.Should().BeFalse();
        }

        [Test]
        public void UpsertLinearAssetConfiguration_WrongMatchedCount_LogsError()
        {
            _clientMock
                .Setup(x => x.UpdateOne(
                    "live_to_vod_linear_asset_configurations",
                    It.IsAny<Func<FilterDefinitionBuilder<LiveToVodLinearAssetConfigurationData>, FilterDefinition<LiveToVodLinearAssetConfigurationData>>>(),
                    It.IsAny<Func<UpdateDefinitionBuilder<LiveToVodLinearAssetConfigurationData>, UpdateDefinition<LiveToVodLinearAssetConfigurationData>>>(),
                    It.Is<MongoDbUpdateOptions>(_ => _.IsUpsert)))
                .Returns(new MongoDbUpdateResult(new UpdateResult.Acknowledged(2, 0, "")));
            _loggerMock
                .Setup(LogLevel.Error, "There have been found 2 LiveToVodLinearAssetConfiguration's documents in the database: partnerId=1, LinearAssetId=3.");
            var repository = new Repository(_clientFactoryMock.Object, _adminClientFactoryMock.Object, _loggerMock.Object);

            var result = repository.UpsertLinearAssetConfiguration(1, new LiveToVodLinearAssetConfiguration(3, false), 2);

            result.Should().BeFalse();
        }
    }
}