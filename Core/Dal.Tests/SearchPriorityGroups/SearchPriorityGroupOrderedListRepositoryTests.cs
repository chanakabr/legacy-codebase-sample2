using System;
using System.Linq;
using ApiObjects.Response;
using ApiObjects.SearchPriorityGroups;
using Core.Tests;
using CouchbaseManager;
using DAL.SearchPriorityGroups;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Dal.Tests.SearchPriorityGroups
{
    [TestFixture]
    public class SearchPriorityGroupOrderedListRepositoryTests
    {
        private MockRepository _mockRepository;
        private Mock<ICouchbaseManager> _couchbaseManagerMock;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _couchbaseManagerMock = _mockRepository.Create<ICouchbaseManager>();
            _loggerMock = _mockRepository.Create<ILogger>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void Get_StatusSuccess_ReturnsOk()
        {
            var getStatus = eResultStatus.SUCCESS;
            _couchbaseManagerMock
                .Setup(x => x.Get<SearchPriorityGroupOrderedListCb>("1_KalturaSearchPriorityGroupOrderedIdsSet", out getStatus, It.IsNotNull<JsonSerializerSettings>()))
                .Returns(FakeSearchPriorityGroupOrderedListCb());
            var repository = new SearchPriorityGroupOrderedListRepository(_couchbaseManagerMock.Object, _loggerMock.Object);

            var result = repository.Get(1);

            result.Status.Should().Be(Status.Ok);
            result.Object.Should().NotBeNull();
            result.Object.PriorityGroupIds.Should().BeEquivalentTo(new long[] { 10, 11 });
        }

        [Test]
        public void Get_StatusKeyNotExist_ReturnsOk()
        {
            var getStatus = eResultStatus.KEY_NOT_EXIST;
            _couchbaseManagerMock
                .Setup(x => x.Get<SearchPriorityGroupOrderedListCb>("1_KalturaSearchPriorityGroupOrderedIdsSet", out getStatus, It.IsNotNull<JsonSerializerSettings>()))
                .Returns((SearchPriorityGroupOrderedListCb)null);
            var repository = new SearchPriorityGroupOrderedListRepository(_couchbaseManagerMock.Object, _loggerMock.Object);

            var result = repository.Get(1);

            result.Status.Should().Be(Status.Ok);
            result.Object.Should().NotBeNull();
            result.Object.PriorityGroupIds.Should().BeEquivalentTo(Enumerable.Empty<long>());
        }

        [Test]
        public void Get_StatusError_ReturnsError()
        {
            var getStatus = eResultStatus.ERROR;
            _couchbaseManagerMock
                .Setup(x => x.Get<SearchPriorityGroupOrderedListCb>("1_KalturaSearchPriorityGroupOrderedIdsSet", out getStatus, It.IsNotNull<JsonSerializerSettings>()))
                .Returns((SearchPriorityGroupOrderedListCb)null);
            _loggerMock
                .Setup(LogLevel.Error, "Get failed: documentKey=1_KalturaSearchPriorityGroupOrderedIdsSet.");
            var repository = new SearchPriorityGroupOrderedListRepository(_couchbaseManagerMock.Object, _loggerMock.Object);

            var result = repository.Get(1);

            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [Test]
        public void Get_ExceptionIsThrown_ReturnsError()
        {
            var getStatus = eResultStatus.SUCCESS;
            _couchbaseManagerMock
                .Setup(x => x.Get<SearchPriorityGroupOrderedListCb>("1_KalturaSearchPriorityGroupOrderedIdsSet", out getStatus, It.IsNotNull<JsonSerializerSettings>()))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Get failed with groupId=1. Error: message.");
            var repository = new SearchPriorityGroupOrderedListRepository(_couchbaseManagerMock.Object, _loggerMock.Object);

            var result = repository.Get(1);

            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [Test]
        public void Update_Success_ReturnsOk()
        {
            _couchbaseManagerMock
                .Setup(x => x.Set("1_KalturaSearchPriorityGroupOrderedIdsSet", It.Is<SearchPriorityGroupOrderedListCb>(_ => _.PriorityGroupIds.SequenceEqual(new long[] { 10, 11 })), 0))
                .Returns(true);
            var repository = new SearchPriorityGroupOrderedListRepository(_couchbaseManagerMock.Object, _loggerMock.Object);

            var result = repository.Update(1, FakeSearchPriorityGroupOrderedIdsSet());

            result.Status.Should().Be(Status.Ok);
            result.Object.Should().NotBeNull();
            result.Object.PriorityGroupIds.Should().BeEquivalentTo(new long[] { 10, 11 });
        }

        [Test]
        public void Update_Failed_ReturnsError()
        {
            _couchbaseManagerMock
                .Setup(x => x.Set("1_KalturaSearchPriorityGroupOrderedIdsSet", It.Is<SearchPriorityGroupOrderedListCb>(_ => _.PriorityGroupIds.SequenceEqual(new long[] { 10, 11 })), 0))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "Set failed: documentKey=1_KalturaSearchPriorityGroupOrderedIdsSet, orderedList:[10,11].");
            var repository = new SearchPriorityGroupOrderedListRepository(_couchbaseManagerMock.Object, _loggerMock.Object);

            var result = repository.Update(1, FakeSearchPriorityGroupOrderedIdsSet());

            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [Test]
        public void Update_ExceptionIsThrown_ReturnsError()
        {
            _couchbaseManagerMock
                .Setup(x => x.Set("1_KalturaSearchPriorityGroupOrderedIdsSet", It.Is<SearchPriorityGroupOrderedListCb>(_ => _.PriorityGroupIds.SequenceEqual(new long[] { 10, 11 })), 0))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Update failed. Error: message.");
            var repository = new SearchPriorityGroupOrderedListRepository(_couchbaseManagerMock.Object, _loggerMock.Object);

            var result = repository.Update(1, FakeSearchPriorityGroupOrderedIdsSet());

            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        private SearchPriorityGroupOrderedIdsSet FakeSearchPriorityGroupOrderedIdsSet()
        {
            return new SearchPriorityGroupOrderedIdsSet(new long[] { 10, 11 });
        }

        private SearchPriorityGroupOrderedListCb FakeSearchPriorityGroupOrderedListCb()
        {
            return new SearchPriorityGroupOrderedListCb(new long[] { 10, 11 });
        }
    }
}