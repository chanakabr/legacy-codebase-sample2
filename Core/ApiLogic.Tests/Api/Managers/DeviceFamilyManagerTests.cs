using System.Collections.Generic;
using ApiLogic.Api.Managers;
using ApiLogic.Repositories;
using ApiObjects;
using ApiObjects.Response;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.Api.Managers
{
    [TestFixture]
    public class DeviceFamilyManagerTests
    {
        private MockRepository _mockRepository;
        private Mock<IDeviceFamilyRepository> _deviceFamilyRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _deviceFamilyRepositoryMock = _mockRepository.Create<IDeviceFamilyRepository>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void Add_ValidParameters_ReturnsExpectedResult()
        {
            var deviceFamily = CreateDeviceFamily(1, "DeviceFamily A");
            var addResult = new GenericResponse<DeviceFamily>();
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, new List<DeviceFamily> { CreateDeviceFamily(2, "DeviceFamily B") }));
            _deviceFamilyRepositoryMock
                .Setup(x => x.Add(10, deviceFamily, 2))
                .Returns(addResult);
            var deviceFamilyManager = new DeviceFamilyManager(_deviceFamilyRepositoryMock.Object);

            var result = deviceFamilyManager.Add(10, deviceFamily, 2);

            result.Should().Be(addResult);
        }

        [Test]
        public void Add_DeviceFamilyIdAlreadyInUse_ReturnsExpectedResult()
        {
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, new List<DeviceFamily> { CreateDeviceFamily(1, "DeviceFamily B") }));
            var deviceFamilyManager = new DeviceFamilyManager(_deviceFamilyRepositoryMock.Object);

            var result = deviceFamilyManager.Add(10, CreateDeviceFamily(1, "DeviceFamily A"), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(eResponseStatus.DeviceFamilyIdAlreadyInUse));
            result.Object.Should().BeNull();
        }

        [Test]
        public void Add_ListResponseWithError_ReturnsExpectedResult()
        {
            var listResponseStatus = new Status(eResponseStatus.Error, "Custom Error");
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(listResponseStatus, null));
            var deviceFamilyManager = new DeviceFamilyManager(_deviceFamilyRepositoryMock.Object);

            var result = deviceFamilyManager.Add(10, CreateDeviceFamily(1, "DeviceFamily A"), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(listResponseStatus);
            result.Object.Should().BeNull();
        }

        [Test]
        public void Update_ValidParameters_ReturnsExpectedResult()
        {
            var deviceFamily = CreateDeviceFamily(1, "DeviceFamily A");
            var updateResult = new GenericResponse<DeviceFamily>();
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, new List<DeviceFamily> { CreateDeviceFamily(1, "DeviceFamily B") }));
            _deviceFamilyRepositoryMock
                .Setup(x => x.Update(10, deviceFamily, 2))
                .Returns(updateResult);
            var deviceFamilyManager = new DeviceFamilyManager(_deviceFamilyRepositoryMock.Object);

            var result = deviceFamilyManager.Update(10, deviceFamily, 2);

            result.Should().Be(updateResult);
        }

        [Test]
        public void Update_DeviceFamilyDoesNotExist_ReturnsExpectedResult()
        {
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, new List<DeviceFamily> { CreateDeviceFamily(2, "DeviceFamily B") }));
            var deviceFamilyManager = new DeviceFamilyManager(_deviceFamilyRepositoryMock.Object);

            var result = deviceFamilyManager.Update(10, CreateDeviceFamily(1, "DeviceFamily A"), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(eResponseStatus.DeviceFamilyDoesNotExist));
            result.Object.Should().BeNull();
        }

        [Test]
        public void Update_ListResponseWithError_ReturnsExpectedResult()
        {
            var listResponseStatus = new Status(eResponseStatus.Error, "Custom Error");
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(listResponseStatus, null));
            var deviceFamilyManager = new DeviceFamilyManager(_deviceFamilyRepositoryMock.Object);

            var result = deviceFamilyManager.Update(10, CreateDeviceFamily(1, "DeviceFamily A"), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(listResponseStatus);
            result.Object.Should().BeNull();
        }

        [Test]
        public void List_RepositoryReturnsError_ReturnsExpectedResult()
        {
            var listResult = new GenericListResponse<DeviceFamily>();
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(listResult);
            var deviceFamilyManager = new DeviceFamilyManager(_deviceFamilyRepositoryMock.Object);

            var result = deviceFamilyManager.List(10, null, null, null, true, 1, 2);

            result.Should().Be(listResult);
        }

        [Test]
        public void List_AscendingOrder_ReturnsExpectedResult()
        {
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, CreateDeviceFamilyList()));
            var deviceFamilyManager = new DeviceFamilyManager(_deviceFamilyRepositoryMock.Object);

            var result = deviceFamilyManager.List(10, null, null, null, true, 1, 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeNull();
            result.Objects.Count.Should().Be(2);
            result.Objects[0].Id.Should().Be(3);
            result.Objects[1].Id.Should().Be(100001);
            result.TotalItems.Should().Be(7);
        }

        [Test]
        public void List_DescendingOrder_ReturnsExpectedResult()
        {
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, CreateDeviceFamilyList()));
            var deviceFamilyManager = new DeviceFamilyManager(_deviceFamilyRepositoryMock.Object);

            var result = deviceFamilyManager.List(10, null, null, null, false, 1, 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeNull();
            result.Objects.Count.Should().Be(2);
            result.Objects[0].Id.Should().Be(100002);
            result.Objects[1].Id.Should().Be(100001);
            result.TotalItems.Should().Be(7);
        }

        [Test]
        public void List_FilterById_ReturnsExpectedResult()
        {
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, CreateDeviceFamilyList()));
            var deviceFamilyManager = new DeviceFamilyManager(_deviceFamilyRepositoryMock.Object);

            var result = deviceFamilyManager.List(10, 3, null, null, false, 0, 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeNull();
            result.Objects.Count.Should().Be(1);
            result.Objects[0].Id.Should().Be(3);
            result.TotalItems.Should().Be(1);
        }

        [Test]
        public void List_FilterByName_ReturnsExpectedResult()
        {
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, CreateDeviceFamilyList()));
            var deviceFamilyManager = new DeviceFamilyManager(_deviceFamilyRepositoryMock.Object);

            var result = deviceFamilyManager.List(10, null, "DeviceFamily A", null, false, 1, 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeNull();
            result.Objects.Count.Should().Be(1);
            result.Objects[0].Id.Should().Be(1);
            result.TotalItems.Should().Be(3);
        }

        [Test]
        public void List_FilterBySystem_ReturnsExpectedResult()
        {
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, CreateDeviceFamilyList()));
            var deviceFamilyManager = new DeviceFamilyManager(_deviceFamilyRepositoryMock.Object);

            var result = deviceFamilyManager.List(10, null, null, true, false, 0, 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeNull();
            result.Objects.Count.Should().Be(2);
            result.Objects[0].Id.Should().Be(3);
            result.Objects[1].Id.Should().Be(2);
            result.TotalItems.Should().Be(3);
        }

        [Test]
        public void List_FilterByCustom_ReturnsExpectedResult()
        {
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, CreateDeviceFamilyList()));
            var deviceFamilyManager = new DeviceFamilyManager(_deviceFamilyRepositoryMock.Object);

            var result = deviceFamilyManager.List(10, null, null, false, false, 1, 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeNull();
            result.Objects.Count.Should().Be(2);
            result.Objects[0].Id.Should().Be(100002);
            result.Objects[1].Id.Should().Be(100001);
            result.TotalItems.Should().Be(4);
        }

        private DeviceFamily CreateDeviceFamily(long id, string name)
        {
            return new DeviceFamily
            {
                Id = (int)id,
                Name = name
            };
        }

        private List<DeviceFamily> CreateDeviceFamilyList()
        {
            return new List<DeviceFamily>
            {
                CreateDeviceFamily(1, "DeviceFamily A"),
                CreateDeviceFamily(2, "DeviceFamily B"),
                CreateDeviceFamily(3, "DeviceFamily C"),
                CreateDeviceFamily(100001, "DeviceFamily A"),
                CreateDeviceFamily(100002, "DeviceFamily A"),
                CreateDeviceFamily(100003, "DeviceFamily AAA"),
                CreateDeviceFamily(100004, "DeviceFamily D")
            };
        }
    }
}