using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.Catalog.CatalogManagement.Repositories;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Catalog.CatalogManagement;
using FluentAssertions;
using KLogMonitor;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.Catalog.CatalogManagement.Managers
{
    [TestFixture]
    public class CatalogManagerTests
    {
        private MockRepository _mockRepository;
        private Mock<ILabelRepository> _labelRepositoryMock;
        private Mock<IKLogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _labelRepositoryMock = _mockRepository.Create<ILabelRepository>();
            _loggerMock = _mockRepository.Create<IKLogger>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void AddLabel_ValidParameters_ReturnsExpectedResponse()
        {
            var fakeLabel = FakeLabelValue();
            var fakeResponse = new GenericResponse<LabelValue>();
            _labelRepositoryMock
                .Setup(x => x.Add(1, fakeLabel, 2))
                .Returns(fakeResponse);
            var catalogManager = new CatalogManager(_labelRepositoryMock.Object, _loggerMock.Object);

            var result = catalogManager.AddLabel(1, fakeLabel, 2);

            result.Should().Be(fakeResponse);
        }

        [Test]
        public void AddLabel_ExceptionIsThrown_ReturnsExpectedResponse()
        {
            var fakeLabel = FakeLabelValue();
            var exception = new Exception("message");
            _labelRepositoryMock
                .Setup(x => x.Add(1, fakeLabel, 2))
                .Throws(exception);
            _loggerMock
                .Setup(x => x.Error("Failed AddLabel, groupId:1, requestLabel:{Id=1, EntityAttribute=MediaFileLabels, Value=\"label_1\"}, userId:2.", exception, null));
            var catalogManager = new CatalogManager(_labelRepositoryMock.Object, _loggerMock.Object);

            var result = catalogManager.AddLabel(1, fakeLabel, 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [Test]
        public void UpdateLabel_ValidParameters_ReturnsExpectedResponse()
        {
            var fakeLabel = FakeLabelValue();
            var fakeResponse = new GenericResponse<LabelValue>();
            _labelRepositoryMock
                .Setup(x => x.Update(1, fakeLabel, 2))
                .Returns(fakeResponse);
            var catalogManager = new CatalogManager(_labelRepositoryMock.Object, _loggerMock.Object);

            var result = catalogManager.UpdateLabel(1, fakeLabel, 2);

            result.Should().Be(fakeResponse);
        }

        [Test]
        public void UpdateLabel_ExceptionIsThrown_ReturnsExpectedResponse()
        {
            var fakeLabel = FakeLabelValue();
            var exception = new Exception("message");
            _labelRepositoryMock
                .Setup(x => x.Update(1, fakeLabel, 2))
                .Throws(exception);
            _loggerMock
                .Setup(x => x.Error("Failed UpdateLabel, groupId:1, requestLabel:{Id=1, EntityAttribute=MediaFileLabels, Value=\"label_1\"}, userId:2.", exception, null));
            var catalogManager = new CatalogManager(_labelRepositoryMock.Object, _loggerMock.Object);

            var result = catalogManager.UpdateLabel(1, fakeLabel, 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Object.Should().BeNull();
        }

        [Test]
        public void DeleteLabel_ValidParameters_ReturnsExpectedStatus()
        {
            var fakeStatus = new Status(42, "message 42");
            _labelRepositoryMock
                .Setup(x => x.Delete(1, 2, 3))
                .Returns(fakeStatus);
            var catalogManager = new CatalogManager(_labelRepositoryMock.Object, _loggerMock.Object);

            var result = catalogManager.DeleteLabel(1, 2, 3);

            result.Should().Be(fakeStatus);
        }

        [Test]
        public void DeleteLabel_ExceptionIsThrown_ReturnsExpectedStatus()
        {
            var exception = new Exception("message");
            _labelRepositoryMock
                .Setup(x => x.Delete(1, 2, 3))
                .Throws(exception);
            _loggerMock
                .Setup(x => x.Error("Failed DeleteLabel, groupId:1, labelId:2, userId:3.", exception, null));
            var catalogManager = new CatalogManager(_labelRepositoryMock.Object, _loggerMock.Object);

            var result = catalogManager.DeleteLabel(1, 2, 3);

            result.Should().Be(Status.Error);
        }

        [Test]
        public void SearchLabels_IdInSpecified_ReturnsExpectedResult()
        {
            var fakeLabelValues = FakeLabelValues();
            _labelRepositoryMock
                .Setup(x => x.List(1))
                .Returns(new GenericListResponse<LabelValue>(Status.Ok, fakeLabelValues));
            var catalogManager = new CatalogManager(_labelRepositoryMock.Object, _loggerMock.Object);

            var result = catalogManager.SearchLabels(1, Enumerable.Range(10, 25).Select(x => (long)x).ToList(), null, null, EntityAttribute.MediaFileLabels, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.TotalItems.Should().Be(8);
            result.Objects.Should().Equal(fakeLabelValues[13], fakeLabelValues[15], fakeLabelValues[16]);
        }

        [Test]
        public void SearchLabels_LabelEqualSpecifiedAndFound_ReturnsExpectedResult()
        {
            var fakeLabelValues = FakeLabelValues();
            _labelRepositoryMock
                .Setup(x => x.List(1))
                .Returns(new GenericListResponse<LabelValue>(Status.Ok, fakeLabelValues));
            var catalogManager = new CatalogManager(_labelRepositoryMock.Object, _loggerMock.Object);

            var result = catalogManager.SearchLabels(1, null, "LaBeL_7", null, EntityAttribute.MediaFileLabels, 0, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.TotalItems.Should().Be(1);
            result.Objects.Should().Equal(fakeLabelValues[6]);
        }

        [Test]
        public void SearchLabels_LabelEqualSpecifiedButNotFound_ReturnsExpectedResult()
        {
            var fakeLabelValues = FakeLabelValues();
            _labelRepositoryMock
                .Setup(x => x.List(1))
                .Returns(new GenericListResponse<LabelValue>(Status.Ok, fakeLabelValues));
            var catalogManager = new CatalogManager(_labelRepositoryMock.Object, _loggerMock.Object);

            var result = catalogManager.SearchLabels(1, null, "label_3", null, EntityAttribute.MediaFileLabels, 0, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.TotalItems.Should().Be(0);
            result.Objects.Should().BeEmpty();
        }

        [Test]
        public void SearchLabels_LabelStartWithSpecified_ReturnsExpectedResult()
        {
            var fakeLabelValues = FakeLabelValues();
            _labelRepositoryMock
                .Setup(x => x.List(1))
                .Returns(new GenericListResponse<LabelValue>(Status.Ok, fakeLabelValues));
            var catalogManager = new CatalogManager(_labelRepositoryMock.Object, _loggerMock.Object);

            var result = catalogManager.SearchLabels(1, null, null, "LaBeL_1", EntityAttribute.MediaFileLabels, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.TotalItems.Should().Be(8);
            result.Objects.Should().Equal(fakeLabelValues[12], fakeLabelValues[13], fakeLabelValues[15]);
        }

        [Test]
        public void SearchLabels_All_ReturnsExpectedResult()
        {
            var fakeLabelValues = FakeLabelValues();
            _labelRepositoryMock
                .Setup(x => x.List(1))
                .Returns(new GenericListResponse<LabelValue>(Status.Ok, fakeLabelValues));
            var catalogManager = new CatalogManager(_labelRepositoryMock.Object, _loggerMock.Object);

            var result = catalogManager.SearchLabels(1, null, null, null, EntityAttribute.MediaFileLabels, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.TotalItems.Should().Be(14);
            result.Objects.Should().Equal(fakeLabelValues[4], fakeLabelValues[6], fakeLabelValues[7]);
        }

        [Test]
        public void SearchLabels_ListFailed_ReturnsExpectedResult()
        {
            var fakeStatus = new Status(42, "message 42");
            _labelRepositoryMock
                .Setup(x => x.List(1))
                .Returns(new GenericListResponse<LabelValue>(fakeStatus, null));
            var catalogManager = new CatalogManager(_labelRepositoryMock.Object, _loggerMock.Object);

            var result = catalogManager.SearchLabels(1, new List<long> { 1, 2 }, "labelEqualValue", "labelStartWithValue", EntityAttribute.MediaFileLabels, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(fakeStatus);
            result.TotalItems.Should().Be(0);
            result.Objects.Should().BeEmpty();
        }

        [Test]
        public void SearchLabels_ExceptionIsThrown_ReturnsExpectedResult()
        {
            var exception = new Exception("message");
            _labelRepositoryMock
                .Setup(x => x.List(1))
                .Throws(exception);
            _loggerMock
                .Setup(x => x.Error("Failed SearchLabels, groupId:1, idIn:[1,2], labelEqual:labelEqualValue, labelStartWith:labelStartWithValue, entityAttribute:MediaFileLabels, pageIndex:1, pageSize:3.", exception, null));
            var catalogManager = new CatalogManager(_labelRepositoryMock.Object, _loggerMock.Object);

            var result = catalogManager.SearchLabels(1, new List<long> { 1, 2 }, "labelEqualValue", "labelStartWithValue", EntityAttribute.MediaFileLabels, 1, 3);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.TotalItems.Should().Be(0);
            result.Objects.Should().BeEmpty();
        }

        private LabelValue FakeLabelValue()
        {
            return new LabelValue(1, EntityAttribute.MediaFileLabels, "label_1");
        }

        private List<LabelValue> FakeLabelValues()
        {
            var labelValues = new List<LabelValue>();
            for (var i = 1; i <= 20; i++)
            {
                var entityAttribute = i % 3 == 0
                    ? (EntityAttribute)2
                    : EntityAttribute.MediaFileLabels;
                var value = i % 2 == 0
                    ? $"label_{i}"
                    : $"label_{i}".ToUpper();
                var labelValue = new LabelValue(i, entityAttribute, value);
                labelValues.Add(labelValue);
            }

            return labelValues;
        }
    }
}