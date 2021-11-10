using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using ApiLogic.Catalog.CatalogManagement.Repositories;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Core.Tests;
using DAL;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.Catalog.CatalogManagement.Repositories
{
    [TestFixture]
    public class LabelRepositoryTests
    {
        private MockRepository _mockRepository;
        private Mock<ILabelDal> _labelDalMock;
        private Mock<ILayeredCache> _cacheMock;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _labelDalMock = _mockRepository.Create<ILabelDal>();
            _cacheMock = _mockRepository.Create<ILayeredCache>();
            _loggerMock = _mockRepository.Create<ILogger>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void Add_Success_ReturnsExpectedResult()
        {
            _labelDalMock
                .Setup(x => x.Add(1, 2, "label_3", 4))
                .Returns(FakeLabelDataSet());
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_Labels_1", null))
                .Returns(true);
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.Add(1, new LabelValue(0, (EntityAttribute)2, "  label_3  "), 4);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Id.Should().Be(30);
            result.Object.EntityAttribute.Should().Be(EntityAttribute.MediaFileLabels);
            result.Object.Value.Should().Be("label_30");
        }

        [Test]
        [TestCase(true, true, 1)]
        [TestCase(false, false, 1)]
        [TestCase(false, true, 0)]
        [TestCase(false, true, 2)]
        public void Add_SuccessButDataSetInvalid_ReturnsExpectedResult(bool isNullDataSet, bool hasLabelTable, int labelRowCount)
        {
            _labelDalMock
                .Setup(x => x.Add(1, 2, "label_3", 4))
                .Returns(FakeLabelDataSet(isNullDataSet, hasLabelTable, labelRowCount: labelRowCount));
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.Add(1, new LabelValue(0, (EntityAttribute)2, "label_3"), 4);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [Test]
        [TestCase(-222, eResponseStatus.LabelAlreadyInUse, "collision_value is already used in the context of MediaFileLabels.")]
        [TestCase(-42, eResponseStatus.Error, "Error")]
        public void Add_SuccessButDataSetWithErrorStatusCode_ReturnsExpectedResult(int dbStatusCode, int statusCode, string statusMessage)
        {
            _labelDalMock
                .Setup(x => x.Add(1, 2, "label_3", 4))
                .Returns(FakeErrorDataSet(dbStatusCode));
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.Add(1, new LabelValue(0, (EntityAttribute)2, "label_3"), 4);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(statusCode, statusMessage));
            result.Object.Should().BeNull();
        }

        [Test]
        public void Add_SuccessButCacheInvalidationFailed_ReturnsExpectedResult()
        {
            _labelDalMock
                .Setup(x => x.Add(1, 2, "label_3", 4))
                .Returns(FakeLabelDataSet());
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_Labels_1", null))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "Failed to set invalidation key for labels. key = invalidationKey_Labels_1.");
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.Add(1, new LabelValue(0, (EntityAttribute)2, "label_3"), 4);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Id.Should().Be(30);
            result.Object.EntityAttribute.Should().Be(EntityAttribute.MediaFileLabels);
            result.Object.Value.Should().Be("label_30");
        }

        [Test]
        public void Add_ExceptionIsThrown_ReturnsExpectedResult()
        {
            _labelDalMock
                .Setup(x => x.Add(1, 2, "label_3", 4))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing Add: message.");
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.Add(1, new LabelValue(0, (EntityAttribute)2, "label_3"), 4);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [Test]
        public void Update_Success_ReturnsExpectedResult()
        {
            _labelDalMock
                .Setup(x => x.Update(1, 3, "label_3", 4))
                .Returns(FakeLabelDataSet());
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_Labels_1", null))
                .Returns(true);
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.Update(1, new LabelValue(3, (EntityAttribute)2, "  label_3  "), 4);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Id.Should().Be(30);
            result.Object.EntityAttribute.Should().Be(EntityAttribute.MediaFileLabels);
            result.Object.Value.Should().Be("label_30");
        }

        [Test]
        [TestCase(true, true, 1)]
        [TestCase(false, false, 1)]
        [TestCase(false, true, 0)]
        [TestCase(false, true, 2)]
        public void Update_SuccessButDataSetInvalid_ReturnsExpectedResult(bool isNullDataSet, bool hasLabelTable, int labelRowCount)
        {
            _labelDalMock
                .Setup(x => x.Update(1, 3, "label_3", 4))
                .Returns(FakeLabelDataSet(isNullDataSet, hasLabelTable, labelRowCount: labelRowCount));
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.Update(1, new LabelValue(3, (EntityAttribute)2, "label_3"), 4);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [Test]
        [TestCase(-222, eResponseStatus.LabelAlreadyInUse, "collision_value is already used in the context of MediaFileLabels.")]
        [TestCase(-333, eResponseStatus.LabelDoesNotExist, "LabelDoesNotExist")]
        [TestCase(-42, eResponseStatus.Error, "Error")]
        public void Update_SuccessButDataSetWithErrorStatusCode_ReturnsExpectedResult(int dbStatusCode, int statusCode, string statusMessage)
        {
            _labelDalMock
                .Setup(x => x.Update(1, 3, "label_3", 4))
                .Returns(FakeErrorDataSet(dbStatusCode));
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.Update(1, new LabelValue(3, (EntityAttribute)2, "label_3"), 4);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(statusCode, statusMessage));
            result.Object.Should().BeNull();
        }

        [Test]
        public void Update_SuccessButCacheInvalidationFailed_ReturnsExpectedResult()
        {
            _labelDalMock
                .Setup(x => x.Update(1, 3, "label_3", 4))
                .Returns(FakeLabelDataSet());
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_Labels_1", null))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "Failed to set invalidation key for labels. key = invalidationKey_Labels_1.");
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.Update(1, new LabelValue(3, (EntityAttribute)2, "label_3"), 4);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Object.Id.Should().Be(30);
            result.Object.EntityAttribute.Should().Be(EntityAttribute.MediaFileLabels);
            result.Object.Value.Should().Be("label_30");
        }

        [Test]
        public void Update_ExceptionIsThrown_ReturnsExpectedResult()
        {
            _labelDalMock
                .Setup(x => x.Update(1, 3, "label_3", 4))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing Update: message.");
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.Update(1, new LabelValue(3, (EntityAttribute)2, "label_3"), 4);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [Test]
        public void Delete_Success_ReturnsExpectedResult()
        {
            _labelDalMock
                .Setup(x => x.Delete(1, 2, 3))
                .Returns(true);
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_Labels_1", null))
                .Returns(true);
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.Delete(1, 2, 3);

            result.Should().Be(Status.Ok);
        }

        [Test]
        public void Delete_SuccessButCacheInvalidationFailed_ReturnsExpectedResult()
        {
            _labelDalMock
                .Setup(x => x.Delete(1, 2, 3))
                .Returns(true);
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_Labels_1", null))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "Failed to set invalidation key for labels. key = invalidationKey_Labels_1.");
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.Delete(1, 2, 3);

            result.Should().Be(Status.Ok);
        }

        [Test]
        public void Delete_Fail_ReturnsExpectedResult()
        {
            _labelDalMock
                .Setup(x => x.Delete(1, 2, 3))
                .Returns(false);
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.Delete(1, 2, 3);

            result.Should().Be(new Status(eResponseStatus.LabelDoesNotExist));
        }

        [Test]
        public void Delete_ExceptionIsThrown_ReturnsExpectedResult()
        {
            _labelDalMock
                .Setup(x => x.Delete(1, 2, 3))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing Delete: message.");
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.Delete(1, 2, 3);

            result.Should().Be(Status.Error);
        }

        [Test]
        public void List_CacheSuccess_ReturnsExpectedResults()
        {
            var refLabelValues = It.IsAny<List<LabelValue>>();
            _cacheMock
                .Setup(x => x.Get(
                    "Labels_1",
                    ref refLabelValues,
                    It.Is<Func<Dictionary<string, object>, Tuple<List<LabelValue>, bool>>>(_ => _.Method.Name == "GetLabels" && _.Method.IsPrivate),
                    It.Is<Dictionary<string, object>>(_ => _.Count == 1 && (long)_["groupId"] == 1),
                    1,
                    "GetLabels",
                    It.Is<List<string>>(_ => _.Count == 1 && _[0] == "invalidationKey_Labels_1"),
                    false))
                .Returns(true);
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.List(1);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
        }

        [Test]
        public void List_CacheFail_ReturnsExpectedResults()
        {
            var refLabelValues = It.IsAny<List<LabelValue>>();
            _cacheMock
                .Setup(x => x.Get(
                    "Labels_1",
                    ref refLabelValues,
                    It.Is<Func<Dictionary<string, object>, Tuple<List<LabelValue>, bool>>>(_ => _.Method.Name == "GetLabels" && _.Method.IsPrivate),
                    It.Is<Dictionary<string, object>>(_ => _.Count == 1 && (long)_["groupId"] == 1),
                    1,
                    "GetLabels",
                    It.Is<List<string>>(_ => _.Count == 1 && _[0] == "invalidationKey_Labels_1"),
                    false))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "List - Failed get data from cache groupId = 1.");
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.List(1);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Objects.Should().BeEmpty();
        }

        [Test]
        public void List_ExceptionIsThrown_ReturnsExpectedResult()
        {
            var refLabelValues = It.IsAny<List<LabelValue>>();
            _cacheMock
                .Setup(x => x.Get(
                    "Labels_1",
                    ref refLabelValues,
                    It.Is<Func<Dictionary<string, object>, Tuple<List<LabelValue>, bool>>>(_ => _.Method.Name == "GetLabels" && _.Method.IsPrivate),
                    It.Is<Dictionary<string, object>>(_ => _.Count == 1 && (long)_["groupId"] == 1),
                    1,
                    "GetLabels",
                    It.Is<List<string>>(_ => _.Count == 1 && _[0] == "invalidationKey_Labels_1"),
                    false))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing List: message.");
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = labelRepository.List(1);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Objects.Should().BeEmpty();
        }

        [Test]
        public void InvalidateCache_Success_ReturnsExpectedResults()
        {
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_Labels_1", null))
                .Returns(true);
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            labelRepository.InvalidateCache(1);
        }

        [Test]
        public void InvalidateCache_Fail_ReturnsExpectedResults()
        {
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_Labels_1", null))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "Failed to set invalidation key for labels. key = invalidationKey_Labels_1.");
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            labelRepository.InvalidateCache(1);
        }

        [Test]
        public void GetLabels_Success_ReturnsExpectedResult()
        {
            _labelDalMock
                .Setup(x => x.Get(1))
                .Returns(FakeLabelDataSet(labelRowCount: 2));
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);
            var getLabelsMethod = labelRepository
                .GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetLabels" && x.IsPrivate);

            var result = (Tuple<List<LabelValue>, bool>)getLabelsMethod.Invoke(labelRepository, new object[] { new Dictionary<string, object> { { "groupId", (long)1 } } });

            result.Should().NotBeNull();
            result.Item1.Should().NotBeNull();
            result.Item1.Count.Should().Be(2);
            result.Item1[0].Id.Should().Be(30);
            result.Item1[0].EntityAttribute.Should().Be(EntityAttribute.MediaFileLabels);
            result.Item1[0].Value.Should().Be("label_30");
            result.Item1[1].Id.Should().Be(31);
            result.Item1[1].EntityAttribute.Should().Be(EntityAttribute.MediaFileLabels);
            result.Item1[1].Value.Should().Be("label_31");
            result.Item2.Should().BeTrue();
        }
        
        [Test]
        [TestCase(true, true)]
        [TestCase(false, false)]
        public void GetLabels_SuccessButDataSetInvalid_ReturnsExpectedResult(bool isNullDataSet, bool hasLabelTable)
        {
            _labelDalMock
                .Setup(x => x.Get(1))
                .Returns(FakeLabelDataSet(isNullDataSet, hasLabelTable));
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);
            var getLabelsMethod = labelRepository
                .GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetLabels" && x.IsPrivate);

            var result = (Tuple<List<LabelValue>, bool>)getLabelsMethod.Invoke(labelRepository, new object[] { new Dictionary<string, object> { { "groupId", (long)1 } } });

            result.Should().NotBeNull();
            result.Item1.Should().BeNull();
            result.Item2.Should().BeFalse();
        }

        [Test]
        public void GetLabels_ExceptionIsThrown_ReturnsExpectedResult()
        {
            _labelDalMock
                .Setup(x => x.Get(1))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing GetLabels({key: groupId, value:1}): message.");
            var labelRepository = new LabelRepository(_labelDalMock.Object, _cacheMock.Object, _loggerMock.Object);
            var getLabelsMethod = labelRepository
                .GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetLabels" && x.IsPrivate);

            var result = (Tuple<List<LabelValue>, bool>)getLabelsMethod.Invoke(labelRepository, new object[] { new Dictionary<string, object> { { "groupId", (long)1 } } });

            result.Should().NotBeNull();
            result.Item1.Should().BeNull();
            result.Item2.Should().BeFalse();
        }

        private DataSet FakeLabelDataSet(bool isNull = false, bool hasLabelTable = true, bool hasLabelRow = true, int labelRowCount = 1)
        {
            if (isNull)
            {
                return null;
            }
            
            var dataSet = new DataSet();

            if (hasLabelTable)
            {
                var table0 = dataSet.Tables.Add();
                table0.Columns.Add(new DataColumn("ID", typeof(int)));
                table0.Columns.Add(new DataColumn("ENTITY_ATTRIBUTE", typeof(string)));
                table0.Columns.Add(new DataColumn("VALUE", typeof(string)));
                if (hasLabelRow)
                {
                    for (var i = 30; i < 30 + labelRowCount; i++)
                    {
                        table0.Rows.Add(i, 1, $"label_{i}");
                    }
                }
            }

            return dataSet;
        }

        private DataSet FakeErrorDataSet(int dbStatusCode)
        {
            var dataSet = new DataSet();

            var table0 = dataSet.Tables.Add();
            table0.Columns.Add(new DataColumn("StatusCode", typeof(int)));
            table0.Columns.Add(new DataColumn("CollisionEntityAttribute", typeof(int)));
            table0.Columns.Add(new DataColumn("CollisionValue", typeof(string)));

            if (dbStatusCode == -222)
            {
                table0.Rows.Add(dbStatusCode, 1, "collision_value");
            }
            else if (dbStatusCode == -333)
            {
                table0.Rows.Add(dbStatusCode, null);
            }
            else
            {
                table0.Rows.Add(dbStatusCode, null);
            }

            return dataSet;
        }
    }
}