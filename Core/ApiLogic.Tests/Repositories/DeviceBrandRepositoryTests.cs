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
    public class DeviceBrandRepositoryTests
    {
        private MockRepository _mockRepository;
        private Mock<IDeviceBrandDal> _deviceBrandDalMock;
        private Mock<ILayeredCache> _cacheMock;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _deviceBrandDalMock = _mockRepository.Create<IDeviceBrandDal>();
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
            var deviceBrand = CreateDeviceBrand();
            _deviceBrandDalMock
                .Setup(x => x.Add(1, deviceBrand, 2))
                .Returns(CreateDataSet());
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_DeviceBrands_1", null))
                .Returns(true);
            var deviceBrandRepository = new DeviceBrandRepository(_deviceBrandDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceBrandRepository.Add(1, deviceBrand, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Ok);
            response.Object.Should().NotBeNull();
            response.Object.Id.Should().Be(1);
            response.Object.Name.Should().Be("DeviceBrand 1");
            response.Object.DeviceFamilyId.Should().Be(11);
        }

        [Test]
        public void Add_SuccessfullyAddedButCacheInvalidationFailed_ReturnsOkResponse()
        {
            var deviceBrand = CreateDeviceBrand();
            _deviceBrandDalMock
                .Setup(x => x.Add(1, deviceBrand, 2))
                .Returns(CreateDataSet());
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_DeviceBrands_1", null))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "Failed to set invalidation key for device brands. key = invalidationKey_DeviceBrands_1.");
            var deviceBrandRepository = new DeviceBrandRepository(_deviceBrandDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceBrandRepository.Add(1, deviceBrand, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Ok);
            response.Object.Should().NotBeNull();
            response.Object.Id.Should().Be(1);
            response.Object.Name.Should().Be("DeviceBrand 1");
            response.Object.DeviceFamilyId.Should().Be(11);
        }

        [TestCase(true, 1, 1)]
        [TestCase(false, 2, 1)]
        [TestCase(false, 1, 2)]
        public void Add_DataSetInvalid_ReturnsErrorResponse(bool isNull, int tablesCount, int rowsCount)
        {
            var deviceBrand = CreateDeviceBrand();
            _deviceBrandDalMock
                .Setup(x => x.Add(1, deviceBrand, 2))
                .Returns(CreateDataSet(isNull, tablesCount, rowsCount));
            var deviceBrandRepository = new DeviceBrandRepository(_deviceBrandDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceBrandRepository.Add(1, deviceBrand, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Error);
            response.Object.Should().BeNull();
        }

        [Test]
        public void Add_ExceptionIsThrown_ReturnsErrorResponse()
        {
            var deviceBrand = CreateDeviceBrand();
            _deviceBrandDalMock
                .Setup(x => x.Add(1, deviceBrand, 2))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing Add: message.");
            var deviceBrandRepository = new DeviceBrandRepository(_deviceBrandDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceBrandRepository.Add(1, deviceBrand, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Error);
            response.Object.Should().BeNull();
        }

        [Test]
        public void Update_SuccessfullyUpdatedAndCacheInvalidated_ReturnsOkResponse()
        {
            var deviceBrand = CreateDeviceBrand();
            _deviceBrandDalMock
                .Setup(x => x.Update(1, deviceBrand, 2))
                .Returns(CreateDataSet());
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_DeviceBrands_1", null))
                .Returns(true);
            var deviceBrandRepository = new DeviceBrandRepository(_deviceBrandDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceBrandRepository.Update(1, deviceBrand, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Ok);
            response.Object.Should().NotBeNull();
            response.Object.Id.Should().Be(1);
            response.Object.Name.Should().Be("DeviceBrand 1");
            response.Object.DeviceFamilyId.Should().Be(11);
        }

        [Test]
        public void Update_SuccessfullyUpdatedButCacheInvalidationFailed_ReturnsOkResponse()
        {
            var deviceBrand = CreateDeviceBrand();
            _deviceBrandDalMock
                .Setup(x => x.Update(1, deviceBrand, 2))
                .Returns(CreateDataSet());
            _cacheMock
                .Setup(x => x.SetInvalidationKey("invalidationKey_DeviceBrands_1", null))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "Failed to set invalidation key for device brands. key = invalidationKey_DeviceBrands_1.");
            var deviceBrandRepository = new DeviceBrandRepository(_deviceBrandDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceBrandRepository.Update(1, deviceBrand, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Ok);
            response.Object.Should().NotBeNull();
            response.Object.Id.Should().Be(1);
            response.Object.Name.Should().Be("DeviceBrand 1");
            response.Object.DeviceFamilyId.Should().Be(11);
        }

        [TestCase(true, 1, 1)]
        [TestCase(false, 2, 1)]
        [TestCase(false, 1, 2)]
        public void Update_DataSetInvalid_ReturnsErrorResponse(bool isNull, int tablesCount, int rowsCount)
        {
            var deviceBrand = CreateDeviceBrand();
            _deviceBrandDalMock
                .Setup(x => x.Update(1, deviceBrand, 2))
                .Returns(CreateDataSet(isNull, tablesCount, rowsCount));
            var deviceBrandRepository = new DeviceBrandRepository(_deviceBrandDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceBrandRepository.Update(1, deviceBrand, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Error);
            response.Object.Should().BeNull();
        }

        [Test]
        public void Update_ExceptionIsThrown_ReturnsErrorResponse()
        {
            var deviceBrand = CreateDeviceBrand();
            _deviceBrandDalMock
                .Setup(x => x.Update(1, deviceBrand, 2))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing Update: message.");
            var deviceBrandRepository = new DeviceBrandRepository(_deviceBrandDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var response = deviceBrandRepository.Update(1, deviceBrand, 2);

            response.Should().NotBeNull();
            response.Status.Should().Be(Status.Error);
            response.Object.Should().BeNull();
        }

        [Test]
        public void List_CacheSuccess_ReturnsOkResponse()
        {
            var refDeviceBrands = It.IsAny<List<DeviceBrand>>();
            _cacheMock
                .Setup(x => x.Get(
                    "device_brands_1",
                    ref refDeviceBrands,
                    It.Is<Func<Dictionary<string, object>, Tuple<List<DeviceBrand>, bool>>>(_ => _.Method.Name == "GetDeviceBrands" && _.Method.IsPrivate),
                    It.Is<Dictionary<string, object>>(_ => _.Count == 1 && (long)_["groupId"] == 1),
                    1,
                    "GetDeviceBrands",
                    It.Is<List<string>>(_ => _.Count == 1 && _[0] == "invalidationKey_DeviceBrands_1"),
                    false))
                .Returns(true);
            var deviceBrandRepository = new DeviceBrandRepository(_deviceBrandDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = deviceBrandRepository.List(1);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
        }

        [Test]
        public void List_CacheFail_ReturnsErrorResponse()
        {
            var refDeviceBrands = It.IsAny<List<DeviceBrand>>();
            _cacheMock
                .Setup(x => x.Get(
                    "device_brands_1",
                    ref refDeviceBrands,
                    It.Is<Func<Dictionary<string, object>, Tuple<List<DeviceBrand>, bool>>>(_ => _.Method.Name == "GetDeviceBrands" && _.Method.IsPrivate),
                    It.Is<Dictionary<string, object>>(_ => _.Count == 1 && (long)_["groupId"] == 1),
                    1,
                    "GetDeviceBrands",
                    It.Is<List<string>>(_ => _.Count == 1 && _[0] == "invalidationKey_DeviceBrands_1"),
                    false))
                .Returns(false);
            _loggerMock
                .Setup(LogLevel.Error, "List - Failed to get device brands: groupId=1.");
            var deviceBrandRepository = new DeviceBrandRepository(_deviceBrandDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = deviceBrandRepository.List(1);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Objects.Should().BeEmpty();
        }

        [Test]
        public void List_ExceptionIsThrown_ReturnsErrorResponse()
        {
            var refDeviceBrands = It.IsAny<List<DeviceBrand>>();
            _cacheMock
                .Setup(x => x.Get(
                    "device_brands_1",
                    ref refDeviceBrands,
                    It.Is<Func<Dictionary<string, object>, Tuple<List<DeviceBrand>, bool>>>(_ => _.Method.Name == "GetDeviceBrands" && _.Method.IsPrivate),
                    It.Is<Dictionary<string, object>>(_ => _.Count == 1 && (long)_["groupId"] == 1),
                    1,
                    "GetDeviceBrands",
                    It.Is<List<string>>(_ => _.Count == 1 && _[0] == "invalidationKey_DeviceBrands_1"),
                    false))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing List: message.");
            var deviceBrandRepository = new DeviceBrandRepository(_deviceBrandDalMock.Object, _cacheMock.Object, _loggerMock.Object);

            var result = deviceBrandRepository.List(1);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Error);
            result.Objects.Should().BeEmpty();
        }

        [Test]
        public void GetDeviceBrands_Success_ReturnsExpectedResult()
        {
            _deviceBrandDalMock
                .Setup(x => x.List(1))
                .Returns(CreateDataSet(rowsCount: 2));
            var deviceBrandRepository = new DeviceBrandRepository(_deviceBrandDalMock.Object, _cacheMock.Object, _loggerMock.Object);
            var listDeviceBrandsMethod = deviceBrandRepository
                .GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetDeviceBrands" && x.IsPrivate);

            var result = (Tuple<List<DeviceBrand>, bool>)listDeviceBrandsMethod.Invoke(deviceBrandRepository, new object[] { new Dictionary<string, object> { { "groupId", (long)1 } } });

            result.Should().NotBeNull();
            result.Item1.Should().HaveCount(2);
            result.Item1[0].Id.Should().Be(1);
            result.Item1[0].Name.Should().Be("DeviceBrand 1");
            result.Item1[0].DeviceFamilyId.Should().Be(11);
            result.Item1[1].Id.Should().Be(2);
            result.Item1[1].Name.Should().Be("DeviceBrand 2");
            result.Item1[1].DeviceFamilyId.Should().Be(12);
            result.Item2.Should().BeTrue();
        }

        [Test]
        [TestCase(true, 0)]
        [TestCase(false, 2)]
        public void GetDeviceBrands_SuccessButDataSetInvalid_ReturnsExpectedResult(bool isNullDataSet, int tablesCount)
        {
            _deviceBrandDalMock
                .Setup(x => x.List(1))
                .Returns(CreateDataSet(isNullDataSet, tablesCount));
            var deviceBrandRepository = new DeviceBrandRepository(_deviceBrandDalMock.Object, _cacheMock.Object, _loggerMock.Object);
            var listDeviceBrandsMethod = deviceBrandRepository
                .GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetDeviceBrands" && x.IsPrivate);

            var result = (Tuple<List<DeviceBrand>, bool>)listDeviceBrandsMethod.Invoke(deviceBrandRepository, new object[] { new Dictionary<string, object> { { "groupId", (long)1 } } });

            result.Should().NotBeNull();
            result.Item1.Should().BeNull();
            result.Item2.Should().BeFalse();
        }

        [Test]
        public void GetDeviceBrands_ExceptionIsThrown_ReturnsExpectedResult()
        {
            _deviceBrandDalMock
                .Setup(x => x.List(1))
                .Throws(new Exception("message"));
            _loggerMock
                .Setup(LogLevel.Error, "Error while executing GetDeviceBrands({key: groupId, value:1}): message.");
            var deviceBrandRepository = new DeviceBrandRepository(_deviceBrandDalMock.Object, _cacheMock.Object, _loggerMock.Object);
            var listDeviceBrandsMethod = deviceBrandRepository
                .GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(x => x.Name == "GetDeviceBrands" && x.IsPrivate);

            var result = (Tuple<List<DeviceBrand>, bool>)listDeviceBrandsMethod.Invoke(deviceBrandRepository, new object[] { new Dictionary<string, object> { { "groupId", (long)1 } } });

            result.Should().NotBeNull();
            result.Item1.Should().BeNull();
            result.Item2.Should().BeFalse();
        }

        private DeviceBrand CreateDeviceBrand()
        {
            return new DeviceBrand();
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
                    table.Columns.Add(new DataColumn("Name", typeof(string)));
                    table.Columns.Add(new DataColumn("Device_Family_ID", typeof(int)));
                    for (var j = 1; j <= rowsCount; j++)
                    {
                        table.Rows.Add(j, $"DeviceBrand {j}", 10 + j);
                    }
                }
            }

            return dataSet;
        }
    }
}