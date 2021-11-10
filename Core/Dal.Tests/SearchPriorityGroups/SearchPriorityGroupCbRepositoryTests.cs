using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.SearchPriorityGroups;
using Core.Tests;
using CouchbaseManager;
using DAL.SearchPriorityGroups;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Dal.Tests.SearchPriorityGroups
{
    [TestFixture]
    public class SearchPriorityGroupCbRepositoryTests
    {
        private MockRepository _mockRepository;
        private Mock<ICouchbaseManager> _couchbaseManagerMock;
        private Mock<IKeyGenerator> _keyGeneratorMock;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _couchbaseManagerMock = _mockRepository.Create<ICouchbaseManager>();
            _keyGeneratorMock = _mockRepository.Create<IKeyGenerator>();
            _loggerMock = _mockRepository.Create<ILogger>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void Save_NewDocumentWithSuccess_ReturnsDocumentKey()
        {
            var searchPriorityGroupCb = FakeSearchPriorityGroupCb();
            _keyGeneratorMock
                .Setup(x => x.GetGuidKey())
                .Returns("generatedValue");
            _couchbaseManagerMock
                .Setup(x => x.Set("1_KalturaSearchPriorityGroup_generatedValue", searchPriorityGroupCb, 0))
                .Returns(true);
            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.Save(1, searchPriorityGroupCb);

            result.Should().Be("1_KalturaSearchPriorityGroup_generatedValue");
        }

        [Test]
        public void Save_NewDocumentWithFail_ReturnsNull()
        {
            var searchPriorityGroupCb = FakeSearchPriorityGroupCb();
            _keyGeneratorMock
                .Setup(x => x.GetGuidKey())
                .Returns("generatedValue");
            _couchbaseManagerMock
                .Setup(x => x.Set("1_KalturaSearchPriorityGroup_generatedValue", searchPriorityGroupCb, 0))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "Set failed: documentKey=1_KalturaSearchPriorityGroup_generatedValue.");
            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.Save(1, searchPriorityGroupCb);

            result.Should().BeNull();
        }

        [Test]
        public void Save_NewDocumentWithExceptionThrown_ReturnsNull()
        {
            var searchPriorityGroupCb = FakeSearchPriorityGroupCb();
            _keyGeneratorMock
                .Setup(x => x.GetGuidKey())
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Save failed. Error: message.");

            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.Save(1, searchPriorityGroupCb);

            result.Should().BeNull();
        }

        [Test]
        public void Save_ExistingDocumentWithSuccess_ReturnsDocumentKey()
        {
            var searchPriorityGroupCb = FakeSearchPriorityGroupCb();
            _couchbaseManagerMock
                .Setup(x => x.Set("1_KalturaSearchPriorityGroup_10", searchPriorityGroupCb, 0))
                .Returns(true);
            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.Save(1, "1_KalturaSearchPriorityGroup_10", searchPriorityGroupCb);

            result.Should().Be("1_KalturaSearchPriorityGroup_10");
        }

        [Test]
        public void Save_ExistingDocumentWithInvalidDocumentKey_ReturnsNull()
        {
            _loggerMock
                .Setup(LogLevel.Error, "Save failed: documentKey=2_KalturaSearchPriorityGroup_10 is invalid.");
            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.Save(1, "2_KalturaSearchPriorityGroup_10", FakeSearchPriorityGroupCb());

            result.Should().BeNull();
        }

        [Test]
        public void Save_ExistingDocumentWithFail_ReturnsNull()
        {
            var searchPriorityGroupCb = FakeSearchPriorityGroupCb();
            _couchbaseManagerMock
                .Setup(x => x.Set("1_KalturaSearchPriorityGroup_10", searchPriorityGroupCb, 0))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "Set failed: documentKey=1_KalturaSearchPriorityGroup_10.");
            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.Save(1, "1_KalturaSearchPriorityGroup_10", searchPriorityGroupCb);

            result.Should().BeNull();
        }

        [Test]
        public void Save_ExistingDocumentWithExceptionThrown_ReturnsNull()
        {
            var searchPriorityGroupCb = FakeSearchPriorityGroupCb();
            _couchbaseManagerMock
                .Setup(x => x.Set("1_KalturaSearchPriorityGroup_10", searchPriorityGroupCb, 0))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Save failed. Error: message.");
            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.Save(1, "1_KalturaSearchPriorityGroup_10", searchPriorityGroupCb);

            result.Should().BeNull();
        }

        [Test]
        public void Get_Success_ReturnsDocument()
        {
            var searchPriorityGroupCb = FakeSearchPriorityGroupCb();
            _couchbaseManagerMock
                .Setup(x => x.Get<SearchPriorityGroupCb>("1_KalturaSearchPriorityGroup_10", false))
                .Returns(searchPriorityGroupCb);
            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.Get(1, "1_KalturaSearchPriorityGroup_10");

            result.Should().Be(searchPriorityGroupCb);
        }

        [Test]
        public void Get_InvalidDocumentKey_ReturnsNull()
        {
            _loggerMock
                .Setup(LogLevel.Error, "Get failed: documentKey=2_KalturaSearchPriorityGroup_10 is invalid.");
            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.Get(1, "2_KalturaSearchPriorityGroup_10");

            result.Should().BeNull();
        }

        [Test]
        public void Get_ExceptionIsThrown_ReturnsNull()
        {
            _couchbaseManagerMock
                .Setup(x => x.Get<SearchPriorityGroupCb>("1_KalturaSearchPriorityGroup_10", false))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Get failed with documentKey=1_KalturaSearchPriorityGroup_10. Error: message.");
            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.Get(1, "1_KalturaSearchPriorityGroup_10");

            result.Should().BeNull();
        }

        [Test]
        public void List_Success_ReturnsDocuments()
        {
            var searchPriorityGroupCb0 = FakeSearchPriorityGroupCb();
            var searchPriorityGroupCb1 = FakeSearchPriorityGroupCb();
            _couchbaseManagerMock
                .Setup(x => x.GetValues<SearchPriorityGroupCb>(It.Is<List<string>>(_ => _.SequenceEqual(new List<string> { "1_KalturaSearchPriorityGroup_10", "1_KalturaSearchPriorityGroup_12" })), false, false))
                .Returns(new Dictionary<string, SearchPriorityGroupCb> { { "key0", searchPriorityGroupCb0 }, { "key1", searchPriorityGroupCb1 } });
            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.List(1, new[] { "1_KalturaSearchPriorityGroup_10", "2_KalturaSearchPriorityGroup_11", "1_KalturaSearchPriorityGroup_12" });

            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result["key0"].Should().Be(searchPriorityGroupCb0);
            result["key1"].Should().Be(searchPriorityGroupCb1);
        }

        [Test]
        public void List_ExceptionIsThrown_ReturnsNull()
        {
            _couchbaseManagerMock
                .Setup(x => x.GetValues<SearchPriorityGroupCb>(It.Is<List<string>>(_ => _.SequenceEqual(new List<string> { "1_KalturaSearchPriorityGroup_10", "1_KalturaSearchPriorityGroup_12" })), false, false))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "List failed with documentKeys=[1_KalturaSearchPriorityGroup_10,1_KalturaSearchPriorityGroup_12]. Error: message.");
            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.List(1, new[] { "1_KalturaSearchPriorityGroup_10", "2_KalturaSearchPriorityGroup_11", "1_KalturaSearchPriorityGroup_12" });

            result.Should().BeNull();
        }

        [Test]
        public void Delete_Success_ReturnsTrue()
        {
            _couchbaseManagerMock
                .Setup(x => x.Remove("1_KalturaSearchPriorityGroup_10", 0))
                .Returns(true);
            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.Delete(1, "1_KalturaSearchPriorityGroup_10");

            result.Should().BeTrue();
        }

        [Test]
        public void Delete_InvalidDocumentKey_ReturnsFalse()
        {
            _loggerMock
                .Setup(LogLevel.Error, "Delete failed: documentKey=2_KalturaSearchPriorityGroup_10 is invalid.");
            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.Delete(1, "2_KalturaSearchPriorityGroup_10");

            result.Should().BeFalse();
        }

        [Test]
        public void Delete_Fail_ReturnsFalse()
        {
            _couchbaseManagerMock
                .Setup(x => x.Remove("1_KalturaSearchPriorityGroup_10", 0))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "Remove failed: documentKey=1_KalturaSearchPriorityGroup_10.");
            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.Delete(1, "1_KalturaSearchPriorityGroup_10");

            result.Should().BeFalse();
        }

        [Test]
        public void Delete_ExceptionIsThrown_ReturnsFalse()
        {
            _couchbaseManagerMock
                .Setup(x => x.Remove("1_KalturaSearchPriorityGroup_10", 0))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Delete failed. Error: message.");
            var repository = new SearchPriorityGroupCbRepository(_couchbaseManagerMock.Object, _keyGeneratorMock.Object, _loggerMock.Object);

            var result = repository.Delete(1, "1_KalturaSearchPriorityGroup_10");

            result.Should().BeFalse();
        }

        private SearchPriorityGroupCb FakeSearchPriorityGroupCb(int seed = 10)
        {
            return new SearchPriorityGroupCb(new Dictionary<string, string> { { "abc", $"abc name {seed}" } }, SearchPriorityCriteriaType.KSql, "criteriaValue");
        }
    }
}