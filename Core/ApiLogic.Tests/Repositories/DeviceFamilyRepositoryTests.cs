using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using ApiLogic.Repositories;
using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Tests;
using DAL;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.Repositories
{
    [TestFixture]
    public class DeviceFamilyRepositoryTests
    {
        private MockRepository _mockRepository;
        private Mock<IDeviceFamilyDal> _deviceFamilyDalMock;
        private Mock<ILayeredCache> _cacheMock;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _deviceFamilyDalMock = _mockRepository.Create<IDeviceFamilyDal>();
            _cacheMock = _mockRepository.Create<ILayeredCache>();
            _loggerMock = _mockRepository.Create<ILogger>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void Add_SuccessfullyAddedAndCacheInvalidated_ReturnsOkResponse()
        {
            var deviceFamily = CreateDeviceFamily();
            _deviceFamilyDalMock
                .Setup(x => x.Add(1, deviceFamily, 2))
                .Returns(CreateDataSet());
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_DeviceFamilies_1", null))
                .Returns(true);
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceFamilyRepository.Add(1, deviceFamily, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Ok);
            response.Object.Should().NotBeNull();
            response.Object.Id.Should().Be(1);
            response.Object.Name.Should().Be("DeviceFamily 1");
        }

        [Test]
        public void Add_SuccessfullyAddedButCacheInvalidationFailed_ReturnsOkResponse()
        {
            var deviceFamily = CreateDeviceFamily();
            _deviceFamilyDalMock
                .Setup(x => x.Add(1, deviceFamily, 2))
                .Returns(CreateDataSet());
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_DeviceFamilies_1", null))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "Failed to set invalidation key for device families. key = invalidationKey_DeviceFamilies_1.");
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceFamilyRepository.Add(1, deviceFamily, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Ok);
            response.Object.Should().NotBeNull();
            response.Object.Id.Should().Be(1);
            response.Object.Name.Should().Be("DeviceFamily 1");
        }

        [TestCase(true, 1, 1)]
        [TestCase(false, 2, 1)]
        [TestCase(false, 1, 2)]
        public void Add_DataSetInvalid_ReturnsErrorResponse(bool isNull, int tablesCount, int rowsCount)
        {
            var deviceFamily = CreateDeviceFamily();
            _deviceFamilyDalMock
                .Setup(x => x.Add(1, deviceFamily, 2))
                .Returns(CreateDataSet(isNull, tablesCount, rowsCount));
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceFamilyRepository.Add(1, deviceFamily, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Error);
            response.Object.Should().BeNull();
        }

        [Test]
        public void Add_ExceptionIsThrown_ReturnsErrorResponse()
        {
            var deviceFamily = CreateDeviceFamily();
            _deviceFamilyDalMock
                .Setup(x => x.Add(1, deviceFamily, 2))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing Add: message.");
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceFamilyRepository.Add(1, deviceFamily, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Error);
            response.Object.Should().BeNull();
        }

        [Test]
        public void Update_SuccessfullyUpdatedAndCacheInvalidated_ReturnsOkResponse()
        {
            var deviceFamily = CreateDeviceFamily();
            _deviceFamilyDalMock
                .Setup(x => x.Update(1, deviceFamily, 2))
                .Returns(CreateDataSet());
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_DeviceFamilies_1", null))
                .Returns(true);
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceFamilyRepository.Update(1, deviceFamily, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Ok);
            response.Object.Should().NotBeNull();
            response.Object.Id.Should().Be(1);
            response.Object.Name.Should().Be("DeviceFamily 1");
        }

        [Test]
        public void Update_SuccessfullyUpdatedButCacheInvalidationFailed_ReturnsOkResponse()
        {
            var deviceFamily = CreateDeviceFamily();
            _deviceFamilyDalMock
                .Setup(x => x.Update(1, deviceFamily, 2))
                .Returns(CreateDataSet());
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_DeviceFamilies_1", null))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "Failed to set invalidation key for device families. key = invalidationKey_DeviceFamilies_1.");
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceFamilyRepository.Update(1, deviceFamily, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Ok);
            response.Object.Should().NotBeNull();
            response.Object.Id.Should().Be(1);
            response.Object.Name.Should().Be("DeviceFamily 1");
        }

        [TestCase(true, 1, 1)]
        [TestCase(false, 2, 1)]
        [TestCase(false, 1, 2)]
        public void Update_DataSetInvalid_ReturnsErrorResponse(bool isNull, int tablesCount, int rowsCount)
        {
            var deviceFamily = CreateDeviceFamily();
            _deviceFamilyDalMock
                .Setup(x => x.Update(1, deviceFamily, 2))
                .Returns(CreateDataSet(isNull, tablesCount, rowsCount));
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceFamilyRepository.Update(1, deviceFamily, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Error);
            response.Object.Should().BeNull();
        }

        [Test]
        public void Update_ExceptionIsThrown_ReturnsErrorResponse()
        {
            var deviceFamily = CreateDeviceFamily();
            _deviceFamilyDalMock
                .Setup(x => x.Update(1, deviceFamily, 2))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing Update: message.");
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceFamilyRepository.Update(1, deviceFamily, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Error);
            response.Object.Should().BeNull();
        }

        [Test]
        public void GetByDeviceBrandId_DataSetValid_ReturnsOkResponse()
        {
            _deviceFamilyDalMock
                .Setup(x => x.GetByDeviceBrandId(1, 2))
                .Returns(CreateDataSet());
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceFamilyRepository.GetByDeviceBrandId(1, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Ok);
            response.Object.Should().NotBeNull();
            response.Object.Id.Should().Be(1);
            response.Object.Name.Should().Be("DeviceFamily 1");
        }

        [TestCase(true, 1, 1)]
        [TestCase(false, 2, 1)]
        [TestCase(false, 1, 2)]
        public void GetByDeviceBrandId_DataSetInvalid_ReturnsErrorResponse(bool isNull, int tablesCount, int rowsCount)
        {
            _deviceFamilyDalMock
                .Setup(x => x.GetByDeviceBrandId(1, 2))
                .Returns(CreateDataSet(isNull, tablesCount, rowsCount));
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceFamilyRepository.GetByDeviceBrandId(1, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Error);
            response.Object.Should().BeNull();
        }

        [Test]
        public void GetByDeviceBrandId_ExceptionIsThrown_ReturnsErrorResponse()
        {
            _deviceFamilyDalMock
                .Setup(x => x.GetByDeviceBrandId(1, 2))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing GetByDeviceBrandId: message.");
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceFamilyRepository.GetByDeviceBrandId(1, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Error);
            response.Object.Should().BeNull();
        }

        [Test]
        public void List_CacheSuccess_ReturnsOkResponse()
        {
            var refDeviceFamilies = It.IsAny<List<DeviceFamily>>();
            _cacheMock
                .Setup(x => x.Get(
                    "device_families_1",
                    ref refDeviceFamilies,
                    It.Is<Func<Dictionary<string, object>, Tuple<List<DeviceFamily>, bool>>>(_ => _.Method.Name == "GetDeviceFamilies" && _.Method.IsPrivate),
                    It.Is<Dictionary<string, object>>(_ => _.Count == 1 && (long)_["groupId"] == 1),
                    1,
                    "GetDeviceFamilies",
                    It.Is<List<string>>(_ => _.Count == 1 && _[0] == "invalidationKey_DeviceFamilies_1"),
                    false))
                .Returns(true);
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = deviceFamilyRepository.List(1);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
        }

        [Test]
        public void List_CacheFail_ReturnsErrorResponse()
        {
            var refDeviceFamilies = It.IsAny<List<DeviceFamily>>();
            _cacheMock
                .Setup(x => x.Get(
                    "device_families_1",
                    ref refDeviceFamilies,
                    It.Is<Func<Dictionary<string, object>, Tuple<List<DeviceFamily>, bool>>>(_ => _.Method.Name == "GetDeviceFamilies" && _.Method.IsPrivate),
                    It.Is<Dictionary<string, object>>(_ => _.Count == 1 && (long)_["groupId"] == 1),
                    1,
                    "GetDeviceFamilies",
                    It.Is<List<string>>(_ => _.Count == 1 && _[0] == "invalidationKey_DeviceFamilies_1"),
                    false))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "List - Failed to get device families: groupId=1.");
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = deviceFamilyRepository.List(1);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Objects.Should().BeEmpty();
        }

        [Test]
        public void List_ExceptionIsThrown_ReturnsErrorResponse()
        {
            var refDeviceFamilies = It.IsAny<List<DeviceFamily>>();
            _cacheMock
                .Setup(x => x.Get(
                    "device_families_1",
                    ref refDeviceFamilies,
                    It.Is<Func<Dictionary<string, object>, Tuple<List<DeviceFamily>, bool>>>(_ => _.Method.Name == "GetDeviceFamilies" && _.Method.IsPrivate),
                    It.Is<Dictionary<string, object>>(_ => _.Count == 1 && (long)_["groupId"] == 1),
                    1,
                    "GetDeviceFamilies",
                    It.Is<List<string>>(_ => _.Count == 1 && _[0] == "invalidationKey_DeviceFamilies_1"),
                    false))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing List: message.");
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = deviceFamilyRepository.List(1);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Objects.Should().BeEmpty();
        }

        [Test]
        public void GetDeviceFamilies_Success_ReturnsExpectedResult()
        {
            _deviceFamilyDalMock
                .Setup(x => x.List(1))
                .Returns(CreateDataSet(rowsCount: 2));
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);
            var listDeviceFamiliesMethod = deviceFamilyRepository
                .GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetDeviceFamilies" && x.IsPrivate);

            var result = (Tuple<List<DeviceFamily>, bool>)listDeviceFamiliesMethod.Invoke(deviceFamilyRepository, new object[] { new Dictionary<string, object> { { "groupId", (long)1 } } });

            result.Should().NotBeNull();
            result.Item1.Should().HaveCount(2);
            result.Item1[0].Id.Should().Be(1);
            result.Item1[0].Name.Should().Be("DeviceFamily 1");
            result.Item1[1].Id.Should().Be(2);
            result.Item1[1].Name.Should().Be("DeviceFamily 2");
            result.Item2.Should().BeTrue();
        }

        [Test]
        [TestCase(true, 0)]
        [TestCase(false, 2)]
        public void GetDeviceFamilies_SuccessButDataSetInvalid_ReturnsExpectedResult(bool isNullDataSet, int tablesCount)
        {
            _deviceFamilyDalMock
                .Setup(x => x.List(1))
                .Returns(CreateDataSet(isNullDataSet, tablesCount));
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);
            var listDeviceFamiliesMethod = deviceFamilyRepository
                .GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetDeviceFamilies" && x.IsPrivate);

            var result = (Tuple<List<DeviceFamily>, bool>)listDeviceFamiliesMethod.Invoke(deviceFamilyRepository, new object[] { new Dictionary<string, object> { { "groupId", (long)1 } } });

            result.Should().NotBeNull();
            result.Item1.Should().BeNull();
            result.Item2.Should().BeFalse();
        }

        [Test]
        public void GetDeviceFamilies_ExceptionIsThrown_ReturnsExpectedResult()
        {
            _deviceFamilyDalMock
                .Setup(x => x.List(1))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing GetDeviceFamilies({key: groupId, value:1}): message.");
            var deviceFamilyRepository = new DeviceFamilyRepository(_deviceFamilyDalMock.Object, _cacheMock.Object, _loggerMock.Object);
            var listDeviceFamiliesMethod = deviceFamilyRepository
                .GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetDeviceFamilies" && x.IsPrivate);

            var result = (Tuple<List<DeviceFamily>, bool>)listDeviceFamiliesMethod.Invoke(deviceFamilyRepository, new object[] { new Dictionary<string, object> { { "groupId", (long)1 } } });

            result.Should().NotBeNull();
            result.Item1.Should().BeNull();
            result.Item2.Should().BeFalse();
        }

        private DeviceFamily CreateDeviceFamily()
        {
            return new DeviceFamily();
        }

        private DataSet CreateDataSet(bool isNull = false, int tablesCount = 1, int rowsCount = 1)
        {
            if (isNull)
            {
                return null;
            }

            var dataSet = new DataSet();
            for (var i = 0; i < tablesCount; i++)
            {
                var table = dataSet.Tables.Add();
                if (i == 0)
                {
                    table.Columns.Add(new DataColumn("ID", typeof(int)));
                    table.Columns.Add(new DataColumn("NAME", typeof(string)));
                    for (var j = 1; j <= rowsCount; j++)
                    {
                        table.Rows.Add(j, $"DeviceFamily {j}");
                    }
                }
            }

            return dataSet;
        }
    }
}