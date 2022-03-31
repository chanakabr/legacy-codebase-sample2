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
    public class DeviceBrandManagerTests
    {
        private MockRepository _mockRepository;
        private Mock<IDeviceFamilyRepository> _deviceFamilyRepositoryMock;
        private Mock<IDeviceBrandRepository> _deviceBrandRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _deviceFamilyRepositoryMock = _mockRepository.Create<IDeviceFamilyRepository>();
            _deviceBrandRepositoryMock = _mockRepository.Create<IDeviceBrandRepository>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void Add_ValidParameters_ReturnsExpectedResult()
        {
            var deviceBrand = CreateDeviceBrand(1, 100, "DeviceBrand A");
            var addResult = new GenericResponse<DeviceBrand>();
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, CreateDeviceFamilyList()));
            _deviceBrandRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceBrand>(Status.Ok, new List<DeviceBrand> { CreateDeviceBrand(2, 200, "DeviceBrand B") }));
            _deviceBrandRepositoryMock
                .Setup(x => x.Add(10, deviceBrand, 2))
                .Returns(addResult);
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.Add(10, deviceBrand, 2);

            result.Should().Be(addResult);
        }

        [Test]
        public void Add_DeviceFamilyDoesNotExist_ReturnsExpectedResult()
        {
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, CreateDeviceFamilyList()));
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.Add(10, CreateDeviceBrand(2, 200, "DeviceBrand B"), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(eResponseStatus.DeviceFamilyDoesNotExist));
            result.Object.Should().BeNull();
        }

        [Test]
        public void Add_DeviceFamilyListResponseWithError_ReturnsExpectedResult()
        {
            var listResponseStatus = new Status(eResponseStatus.Error, "Device Family List Custom Error");
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(listResponseStatus, null));
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.Add(10, CreateDeviceBrand(1, 100, "DeviceBrand A"), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(listResponseStatus);
            result.Object.Should().BeNull();
        }

        [Test]
        public void Add_DeviceBrandIdAlreadyInUse_ReturnsExpectedResult()
        {
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, CreateDeviceFamilyList()));
            _deviceBrandRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceBrand>(Status.Ok, CreateDeviceBrandList()));
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.Add(10, CreateDeviceBrand(2, 100, "DeviceBrand A"), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(eResponseStatus.DeviceBrandIdAlreadyInUse));
            result.Object.Should().BeNull();
        }

        [Test]
        public void Add_DeviceBrandListResponseWithError_ReturnsExpectedResult()
        {
            var listResponseStatus = new Status(eResponseStatus.Error, "Device Brand List Custom Error");
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, CreateDeviceFamilyList()));
            _deviceBrandRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceBrand>(listResponseStatus, null));
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.Add(10, CreateDeviceBrand(1, 100, "DeviceBrand A"), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(listResponseStatus);
            result.Object.Should().BeNull();
        }

        [TestCase(0)]
        [TestCase(100)]
        public void Update_ValidParameters_ReturnsExpectedResult(long deviceFamilyId)
        {
            var deviceBrand = CreateDeviceBrand(1, deviceFamilyId, "DeviceBrand A");
            var addResult = new GenericResponse<DeviceBrand>();
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, CreateDeviceFamilyList()));
            _deviceBrandRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceBrand>(Status.Ok, CreateDeviceBrandList()));
            _deviceBrandRepositoryMock
                .Setup(x => x.Update(10, deviceBrand, 2))
                .Returns(addResult);
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.Update(10, deviceBrand, 2);

            result.Should().Be(addResult);
        }

        [Test]
        public void Update_DeviceFamilyDoesNotExist_ReturnsExpectedResult()
        {
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, CreateDeviceFamilyList()));
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.Update(10, CreateDeviceBrand(2, 200, "DeviceBrand B"), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(eResponseStatus.DeviceFamilyDoesNotExist));
            result.Object.Should().BeNull();
        }

        [Test]
        public void Update_DeviceFamilyListResponseWithError_ReturnsExpectedResult()
        {
            var listResponseStatus = new Status(eResponseStatus.Error, "Device Family List Custom Error");
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(listResponseStatus, null));
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.Update(10, CreateDeviceBrand(1, 100, "DeviceBrand A"), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(listResponseStatus);
            result.Object.Should().BeNull();
        }

        [Test]
        public void Update_DeviceBrandDoesNotExist_ReturnsExpectedResult()
        {
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, CreateDeviceFamilyList()));
            _deviceBrandRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceBrand>(Status.Ok, CreateDeviceBrandList()));
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.Update(10, CreateDeviceBrand(4, 100, "DeviceBrand D"), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(new Status(eResponseStatus.DeviceBrandDoesNotExist));
            result.Object.Should().BeNull();
        }

        [Test]
        public void Update_DeviceBrandListResponseWithError_ReturnsExpectedResult()
        {
            var listResponseStatus = new Status(eResponseStatus.Error, "Device Brand List Custom Error");
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceFamily>(Status.Ok, CreateDeviceFamilyList()));
            _deviceBrandRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceBrand>(listResponseStatus, null));
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.Update(10, CreateDeviceBrand(1, 100, "DeviceBrand A"), 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(listResponseStatus);
            result.Object.Should().BeNull();
        }

        [Test]
        public void List_RepositoryReturnsError_ReturnsExpectedResult()
        {
            var listResult = new GenericListResponse<DeviceBrand>();
            _deviceBrandRepositoryMock
                .Setup(x => x.List(10))
                .Returns(listResult);
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.List(10, null, null, null, null, true, 1, 2);

            result.Should().Be(listResult);
        }

        [Test]
        public void List_AscendingOrder_ReturnsExpectedResult()
        {
            _deviceBrandRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceBrand>(Status.Ok, CreateDeviceBrandList()));
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.List(10, null, null, null, null, true, 1, 2);

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
            _deviceBrandRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceBrand>(Status.Ok, CreateDeviceBrandList()));
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.List(10, null, null, null, null, false, 1, 2);

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
            _deviceBrandRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceBrand>(Status.Ok, CreateDeviceBrandList()));
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.List(10, 3, null, null, null, false, 0, 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeNull();
            result.Objects.Count.Should().Be(1);
            result.Objects[0].Id.Should().Be(3);
            result.TotalItems.Should().Be(1);
        }

        [Test]
        public void List_FilterByFamilyId_ReturnsExpectedResult()
        {
            _deviceBrandRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceBrand>(Status.Ok, CreateDeviceBrandList()));
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.List(10, null, 200, null, null, false, 1, 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeNull();
            result.Objects.Count.Should().Be(2);
            result.Objects[0].Id.Should().Be(100001);
            result.Objects[1].Id.Should().Be(3);
            result.TotalItems.Should().Be(5);
        }

        [Test]
        public void List_FilterByName_ReturnsExpectedResult()
        {
            _deviceBrandRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceBrand>(Status.Ok, CreateDeviceBrandList()));
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.List(10, null, null, "DeviceBrand A", null, false, 1, 2);

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
            _deviceBrandRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceBrand>(Status.Ok, CreateDeviceBrandList()));
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.List(10, null, null, null, true, false, 0, 2);

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
            _deviceBrandRepositoryMock
                .Setup(x => x.List(10))
                .Returns(new GenericListResponse<DeviceBrand>(Status.Ok, CreateDeviceBrandList()));
            var deviceBrandManager = new DeviceBrandManager(_deviceFamilyRepositoryMock.Object, _deviceBrandRepositoryMock.Object);

            var result = deviceBrandManager.List(10, null, null, null, false, false, 1, 2);

            result.Should().NotBeNull();
            result.Status.Should().Be(Status.Ok);
            result.Objects.Should().NotBeNull();
            result.Objects.Count.Should().Be(2);
            result.Objects[0].Id.Should().Be(100002);
            result.Objects[1].Id.Should().Be(100001);
            result.TotalItems.Should().Be(4);
        }

        private DeviceBrand CreateDeviceBrand(long id, long deviceFamilyId, string name)
        {
            return new DeviceBrand
            {
                Id = (int)id,
                DeviceFamilyId = (int)deviceFamilyId,
                Name = name
            };
        }

        private List<DeviceFamily> CreateDeviceFamilyList()
        {
            return new List<DeviceFamily>
            {
                new DeviceFamily(100, "DeviceFamily 100")
            };
        }

        private List<DeviceBrand> CreateDeviceBrandList()
        {
            return new List<DeviceBrand>
            {
                CreateDeviceBrand(1, 200, "DeviceBrand A"),
                CreateDeviceBrand(2, 100, "DeviceBrand B"),
                CreateDeviceBrand(3, 200, "DeviceBrand C"),
                CreateDeviceBrand(100001, 200, "DeviceBrand A"),
                CreateDeviceBrand(100002, 100, "DeviceBrand A"),
                CreateDeviceBrand(100003, 200, "DeviceBrand AAA"),
                CreateDeviceBrand(100004, 200, "DeviceBrand D")
            };
        }
    }
}