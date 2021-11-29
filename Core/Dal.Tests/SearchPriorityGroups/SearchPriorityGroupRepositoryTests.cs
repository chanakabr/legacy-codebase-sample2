using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchPriorityGroups;
using Core.Tests;
using DAL.SearchPriorityGroups;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Dal.Tests.SearchPriorityGroups
{
    [TestFixture]
    public class SearchPriorityGroupRepositoryTests
    {
        private MockRepository _mockRepository;
        private Mock<ISearchPriorityGroupDal> _searchPriorityGroupDalMock;
        private Mock<ISearchPriorityGroupCbRepository> _searchPriorityGroupCbRepositoryMock;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _searchPriorityGroupDalMock = _mockRepository.Create<ISearchPriorityGroupDal>();
            _searchPriorityGroupCbRepositoryMock = _mockRepository.Create<ISearchPriorityGroupCbRepository>();
            _loggerMock = _mockRepository.Create<ILogger>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void Add_Success_ReturnsOk()
        {
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Save(1, It.Is<SearchPriorityGroupCb>(_ => Compare(_, FakeSearchPriorityGroupCb("abc", "abc name 10", "criteriaValue")))))
                .Returns("document_key");
            _searchPriorityGroupDalMock
                .Setup(x => x.Add(1, "document_key", 2))
                .Returns(FakeDataSet());
            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Add(1, FakeSearchPriorityGroup(0, new[] { "abc" }), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Id.Should().Be(10);
            result.Object.Name.Should().NotBeNull();
            result.Object.Name.Length.Should().Be(1);
            result.Object.Name[0].Equals(new LanguageContainer("abc", "abc name 10")).Should().BeTrue();
            result.Object.Criteria.Should().NotBeNull();
            result.Object.Criteria.Type.Should().Be(SearchPriorityCriteriaType.KSql);
            result.Object.Criteria.Value.Should().Be("criteriaValue");
        }

        [TestCase(null)]
        [TestCase("")]
        public void Add_SaveToCouchbaseFailed_ReturnsError(string documentKey)
        {
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Save(1, It.Is<SearchPriorityGroupCb>(_ => Compare(_, FakeSearchPriorityGroupCb("abc", "abc name 10", "criteriaValue")))))
                .Returns(documentKey);
            _loggerMock
                .Setup(LogLevel.Error, "Could not save SearchPriorityGroupCb.");
            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Add(1, FakeSearchPriorityGroup(0, new[] { "abc" }), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [TestCase(true, 1, 1)]
        [TestCase(false, 0, 1)]
        [TestCase(false, 1, 0)]
        [TestCase(false, 2, 1)]
        public void Add_SaveToDatabaseFailed_ReturnsError(bool isNullDataSet, int tableCount, int rowCount)
        {
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Save(1, It.Is<SearchPriorityGroupCb>(_ => Compare(_, FakeSearchPriorityGroupCb("abc", "abc name 10", "criteriaValue")))))
                .Returns("document_key");
            _searchPriorityGroupDalMock
                .Setup(x => x.Add(1, "document_key", 2))
                .Returns(FakeDataSet(isNullDataSet, tableCount, rowCount));
            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Add(1, FakeSearchPriorityGroup(0, new[] { "abc" }), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [Test]
        public void Add_ExceptionIsThrown_ReturnsError()
        {
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Save(1, It.IsAny<SearchPriorityGroupCb>()))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing Add: message.");
            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Add(1, FakeSearchPriorityGroup(0, new[] { "abc" }), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [Test]
        public void Update_NewNameOfExistingLanguage_ReturnsOk()
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.Get(1, 10))
                .Returns(FakeDataSet());
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Get(1, "document_key_10"))
                .Returns(FakeSearchPriorityGroupCb("abc", "abc name 10 old", "oldCriteriaValue"));
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Save(1, "document_key_10", It.Is<SearchPriorityGroupCb>(_ => Compare(_, FakeSearchPriorityGroupCb("abc", "abc name 10", "oldCriteriaValue")))))
                .Returns("document_key");
            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Update(1, FakeSearchPriorityGroup(10, new[] { "abc" }, null));

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Id.Should().Be(10);
            result.Object.Name.Should().NotBeNull();
            result.Object.Name.Length.Should().Be(1);
            result.Object.Name[0].Equals(new LanguageContainer("abc", "abc name 10")).Should().BeTrue();
            result.Object.Criteria.Should().NotBeNull();
            result.Object.Criteria.Type.Should().Be(SearchPriorityCriteriaType.KSql);
            result.Object.Criteria.Value.Should().Be("oldCriteriaValue");
        }

        [Test]
        public void Update_NewNameOfNewLanguage_ReturnsOk()
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.Get(1, 10))
                .Returns(FakeDataSet());
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Get(1, "document_key_10"))
                .Returns(FakeSearchPriorityGroupCb("abc", "abc name 10 old", "oldCriteriaValue"));
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Save(1, "document_key_10", It.Is<SearchPriorityGroupCb>(_ => Compare(_, FakeSearchPriorityGroupCb("abc", "abc name 10 old", "xyz", "xyz name 10", "oldCriteriaValue")))))
                .Returns("document_key");
            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Update(1, FakeSearchPriorityGroup(10, new[] { "xyz" }, null));

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Id.Should().Be(10);
            result.Object.Name.Should().NotBeNull();
            result.Object.Name.Length.Should().Be(2);
            result.Object.Name[0].Equals(new LanguageContainer("abc", "abc name 10 old")).Should().BeTrue();
            result.Object.Name[1].Equals(new LanguageContainer("xyz", "xyz name 10")).Should().BeTrue();
            result.Object.Criteria.Should().NotBeNull();
            result.Object.Criteria.Type.Should().Be(SearchPriorityCriteriaType.KSql);
            result.Object.Criteria.Value.Should().Be("oldCriteriaValue");
        }

        [Test]
        public void Update_NewCriteriaValue_ReturnsOk()
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.Get(1, 10))
                .Returns(FakeDataSet());
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Get(1, "document_key_10"))
                .Returns(FakeSearchPriorityGroupCb("abc", "abc name 10 old", "oldCriteriaValue"));
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Save(1, "document_key_10", It.Is<SearchPriorityGroupCb>(_ => Compare(_, FakeSearchPriorityGroupCb("abc", "abc name 10 old", "criteriaValue")))))
                .Returns("document_key");
            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Update(1, FakeSearchPriorityGroup(10, new string[0]));

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Id.Should().Be(10);
            result.Object.Name.Should().NotBeNull();
            result.Object.Name.Length.Should().Be(1);
            result.Object.Name[0].Equals(new LanguageContainer("abc", "abc name 10 old")).Should().BeTrue();
            result.Object.Criteria.Should().NotBeNull();
            result.Object.Criteria.Type.Should().Be(SearchPriorityCriteriaType.KSql);
            result.Object.Criteria.Value.Should().Be("criteriaValue");
        }

        [Test]
        public void Update_NewAllProperties_ReturnsOk()
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.Get(1, 10))
                .Returns(FakeDataSet());
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Get(1, "document_key_10"))
                .Returns(FakeSearchPriorityGroupCb("abc", "abc name 10 old", "oldCriteriaValue"));
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Save(1, "document_key_10", It.Is<SearchPriorityGroupCb>(_ => Compare(_, FakeSearchPriorityGroupCb("abc", "abc name 10", "xyz", "xyz name 10", "criteriaValue")))))
                .Returns("document_key");
            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Update(1, FakeSearchPriorityGroup(10, new[] { "abc", "xyz" }));

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Id.Should().Be(10);
            result.Object.Name.Should().NotBeNull();
            result.Object.Name.Length.Should().Be(2);
            result.Object.Name[0].Equals(new LanguageContainer("abc", "abc name 10")).Should().BeTrue();
            result.Object.Name[1].Equals(new LanguageContainer("xyz", "xyz name 10")).Should().BeTrue();
            result.Object.Criteria.Should().NotBeNull();
            result.Object.Criteria.Type.Should().Be(SearchPriorityCriteriaType.KSql);
            result.Object.Criteria.Value.Should().Be("criteriaValue");
        }

        [TestCase(true, 1, 1, eResponseStatus.Error)]
        [TestCase(false, 0, 1, eResponseStatus.Error)]
        [TestCase(false, 1, 0, eResponseStatus.SearchPriorityGroupDoesNotExist)]
        [TestCase(false, 2, 1, eResponseStatus.Error)]
        public void Update_GetFromDatabaseFailed_ReturnsExpectedResult(bool isNullDataSet, int tableCount, int rowCount, eResponseStatus expectedStatus)
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.Get(1, 10))
                .Returns(FakeDataSet(isNullDataSet, tableCount, rowCount));
            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Update(1, FakeSearchPriorityGroup(10, new[] { "abc" }));

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(expectedStatus));
            result.Object.Should().BeNull();
        }

        [TestCase(null)]
        [TestCase("")]
        public void Update_SaveToCouchbaseFailed_ReturnsError(string documentKey)
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.Get(1, 10))
                .Returns(FakeDataSet());
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Get(1, "document_key_10"))
                .Returns(FakeSearchPriorityGroupCb("abc", "abc name 10 old", "criteriaValue"));
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Save(1, "document_key_10", It.Is<SearchPriorityGroupCb>(_ => Compare(_, FakeSearchPriorityGroupCb("abc", "abc name 10", "criteriaValue")))))
                .Returns(documentKey);
            _loggerMock
                .Setup(LogLevel.Error, "Could not save SearchPriorityGroupCb.");

            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Update(1, FakeSearchPriorityGroup(10, new[] { "abc" }));

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [Test]
        public void Update_ExceptionIsThrown_ReturnsError()
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.Get(1, 10))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing Update: message.");

            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Update(1, FakeSearchPriorityGroup(10, new[] { "abc" }));

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [Test]
        public void Delete_Success_ReturnsOk()
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.Get(1, 10))
                .Returns(FakeDataSet());
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Delete(1, "document_key_10"))
                .Returns(true);
            _searchPriorityGroupDalMock
                .Setup(x => x.Delete(1, 10, 2))
                .Returns(true);

            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Delete(1, 10, 2);

            result.Should().Be(Status.Ok);
        }

        [Test]
        public void Delete_GetFromDatabaseFailed_ReturnsSearchPriorityGroupDoesNotExist()
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.Get(1, 10))
                .Returns(FakeDataSet(false, 1, 0));

            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Delete(1, 10, 2);

            result.Should().Be(new Status(eResponseStatus.SearchPriorityGroupDoesNotExist));
        }

        [Test]
        public void Delete_DeleteFromDatabaseFailed_ReturnsError()
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.Get(1, 10))
                .Returns(FakeDataSet());
            _searchPriorityGroupDalMock
                .Setup(x => x.Delete(1, 10, 2))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "Could not delete search priority group with id=10.");

            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Delete(1, 10, 2);

            result.Should().Be(Status.Error);
        }

        [Test]
        public void Delete_DeleteFromCouchbaseFailed_ReturnsError()
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.Get(1, 10))
                .Returns(FakeDataSet());
            _searchPriorityGroupDalMock
                .Setup(x => x.Delete(1, 10, 2))
                .Returns(true);
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.Delete(1, "document_key_10"))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "Could not delete SearchPriorityGroupCb.");

            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Delete(1, 10, 2);

            result.Should().Be(Status.Error);
        }

        [Test]
        public void Delete_ExceptionIsThrown_ReturnsError()
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.Get(1, 10))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing Delete: message.");

            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.Delete(1, 10, 2);

            result.Should().Be(Status.Error);
        }

        [Test]
        public void List_WithIdsAndSuccess_ReturnsOk()
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.List(1, It.Is<IEnumerable<long>>(_ => _.SequenceEqual(new long[] { 10, 11 }))))
                .Returns(FakeDataSet(rowCount: 2));
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.List(1, It.Is<IEnumerable<string>>(_ => _.SequenceEqual(new[] { "document_key_10", "document_key_11" }))))
                .Returns(FakeFakeSearchPriorityGroupCbList(10, 11, 12));
            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.List(1, new long[] { 10, 11 });

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.HasObjects().Should().BeTrue();
            result.TotalItems.Should().Be(2);
            result.Objects.Count.Should().Be(2);
            result.Objects[0].Id.Should().Be(10);
            result.Objects[0].Name.Should().NotBeNull();
            result.Objects[0].Name.Length.Should().Be(1);
            result.Objects[0].Name[0].Equals(new LanguageContainer("abc", "abc name 10")).Should().BeTrue();
            result.Objects[0].Criteria.Should().NotBeNull();
            result.Objects[0].Criteria.Type.Should().Be(SearchPriorityCriteriaType.KSql);
            result.Objects[0].Criteria.Value.Should().Be("criteriaValue10");
            result.Objects[1].Id.Should().Be(11);
            result.Objects[1].Name.Should().NotBeNull();
            result.Objects[1].Name.Length.Should().Be(2);
            result.Objects[1].Name[0].Equals(new LanguageContainer("abc", "abc name 11")).Should().BeTrue();
            result.Objects[1].Name[1].Equals(new LanguageContainer("xyz", "xyz name 11")).Should().BeTrue();
            result.Objects[1].Criteria.Should().NotBeNull();
            result.Objects[1].Criteria.Type.Should().Be(SearchPriorityCriteriaType.KSql);
            result.Objects[1].Criteria.Value.Should().Be("criteriaValue11");
        }

        [Test]
        public void List_WithoutIdsAndSuccess_ReturnsOk()
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.List(1))
                .Returns(FakeDataSet(rowCount: 2));
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.List(1, It.Is<IEnumerable<string>>(_ => _.SequenceEqual(new[] { "document_key_10", "document_key_11" }))))
                .Returns(FakeFakeSearchPriorityGroupCbList(10, 11, 12));
            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.List(1, Enumerable.Empty<long>());

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.HasObjects().Should().BeTrue();
            result.TotalItems.Should().Be(2);
            result.Objects.Count.Should().Be(2);
            result.Objects[0].Id.Should().Be(10);
            result.Objects[0].Name.Should().NotBeNull();
            result.Objects[0].Name.Length.Should().Be(1);
            result.Objects[0].Name[0].Equals(new LanguageContainer("abc", "abc name 10")).Should().BeTrue();
            result.Objects[0].Criteria.Should().NotBeNull();
            result.Objects[0].Criteria.Type.Should().Be(SearchPriorityCriteriaType.KSql);
            result.Objects[0].Criteria.Value.Should().Be("criteriaValue10");
            result.Objects[1].Id.Should().Be(11);
            result.Objects[1].Name.Should().NotBeNull();
            result.Objects[1].Name.Length.Should().Be(2);
            result.Objects[1].Name[0].Equals(new LanguageContainer("abc", "abc name 11")).Should().BeTrue();
            result.Objects[1].Name[1].Equals(new LanguageContainer("xyz", "xyz name 11")).Should().BeTrue();
            result.Objects[1].Criteria.Should().NotBeNull();
            result.Objects[1].Criteria.Type.Should().Be(SearchPriorityCriteriaType.KSql);
            result.Objects[1].Criteria.Value.Should().Be("criteriaValue11");
        }

        [TestCase(true, 1)]
        [TestCase(false, 0)]
        [TestCase(false, 2)]
        public void List_ListFromDatabaseFailed_ReturnsError(bool isNullDataSet, int tableCount)
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.List(1, It.Is<IEnumerable<long>>(_ => _.SequenceEqual(new long[] { 10, 11 }))))
                .Returns(FakeDataSet(isNullDataSet, tableCount));
            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.List(1, new long[] { 10, 11 });

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.HasObjects().Should().BeFalse();
            result.TotalItems.Should().Be(0);
            result.Objects.Count.Should().Be(0);
        }

        [Test]
        public void List_ListFromCouchbaseFailed_ReturnsError()
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.List(1, It.Is<IEnumerable<long>>(_ => _.SequenceEqual(new long[] { 10, 11 }))))
                .Returns(FakeDataSet(rowCount: 2));
            _searchPriorityGroupCbRepositoryMock
                .Setup(x => x.List(1, It.Is<IEnumerable<string>>(_ => _.SequenceEqual(new[] { "document_key_10", "document_key_11" }))))
                .Returns((Dictionary<string, SearchPriorityGroupCb>)null);
            _loggerMock
                .Setup(LogLevel.Error, "Could not get SearchPriorityGroupCb's with documentKeys [document_key_10,document_key_11].");
            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.List(1, new long[] { 10, 11 });

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.HasObjects().Should().BeFalse();
            result.TotalItems.Should().Be(0);
            result.Objects.Count.Should().Be(0);
        }

        [Test]
        public void List_ExceptionIsThrown_ReturnsError()
        {
            _searchPriorityGroupDalMock
                .Setup(x => x.List(1, It.Is<IEnumerable<long>>(_ => _.SequenceEqual(new long[] { 10, 11 }))))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing List: message.");
            var searchPriorityGroupRepository = new SearchPriorityGroupRepository(_searchPriorityGroupDalMock.Object, _searchPriorityGroupCbRepositoryMock.Object, _loggerMock.Object);

            var result = searchPriorityGroupRepository.List(1, new long[] { 10, 11 });

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.HasObjects().Should().BeFalse();
            result.TotalItems.Should().Be(0);
            result.Objects.Count.Should().Be(0);
        }

        private DataSet FakeDataSet(bool isNull = false, int tableCount = 1, int rowCount = 1)
        {
            if (isNull)
            {
                return null;
            }

            var dataSet = new DataSet();
            for (var tableIndex = 0; tableIndex < tableCount; tableIndex++)
            {
                var table = dataSet.Tables.Add();
                if (tableIndex == 0)
                {
                    table.Columns.Add(new DataColumn("ID", typeof(long)));
                    table.Columns.Add(new DataColumn("DOCUMENT_KEY", typeof(string)));
                    for (var i = 10; i < 10 + rowCount; i++)
                    {
                        table.Rows.Add(i, $"document_key_{i}");
                    }
                }
            }

            return dataSet;
        }

        private SearchPriorityGroup FakeSearchPriorityGroup(long id, IEnumerable<string> languages, string criteriaValue = "criteriaValue")
        {
            var nameSeed = id == 0 ? 10 : id;
            var name = languages.Select(x => new LanguageContainer(x, $"{x} name {nameSeed}")).ToArray();

            return new SearchPriorityGroup(id, name, SearchPriorityCriteriaType.KSql, criteriaValue);
        }

        private SearchPriorityGroupCb FakeSearchPriorityGroupCb(params string[] values)
        {
            var name = new Dictionary<string, string>();
            for (var i = 0; i < values.Length - 1; i += 2)
            {
                name.Add(values[i], values[i + 1]);
            }

            return new SearchPriorityGroupCb(name, SearchPriorityCriteriaType.KSql, values.Last());
        }

        private Dictionary<string, SearchPriorityGroupCb> FakeFakeSearchPriorityGroupCbList(params long[] ids)
        {
            var list = new Dictionary<string, SearchPriorityGroupCb>();
            foreach (var id in ids)
            {
                var documentKey = $"document_key_{id}";
                var document = id % 2 == 0
                    ? FakeSearchPriorityGroupCb("abc", $"abc name {id}", $"criteriaValue{id}")
                    : FakeSearchPriorityGroupCb("abc", $"abc name {id}", "xyz", $"xyz name {id}", $"criteriaValue{id}");
                list.Add(documentKey, document);
            }

            return list;
        }

        private bool Compare(SearchPriorityGroupCb spg1, SearchPriorityGroupCb spg2)
        {
            return spg1.Name.SequenceEqual(spg2.Name) && spg1.Criteria.Type == spg2.Criteria.Type && spg1.Criteria.Value == spg2.Criteria.Value;
        }
    }
}