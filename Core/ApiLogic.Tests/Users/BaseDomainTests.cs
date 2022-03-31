using System;
using System.Collections.Generic;
using ApiLogic.Repositories;
using ApiLogic.Users;
using ApiObjects;
using ApiObjects.Response;
using Core.Users;
using FluentAssertions;
using log4net;
using Moq;
using NUnit.Framework;
using Status = ApiObjects.Response.Status;

namespace ApiLogic.Tests.Users
{
    [TestFixture]
    public class BaseDomainTests
    {
        private MockRepository _mockRepository;
        private Mock<IDomainLimitationModuleRepository> _dlmRepositoryMock;
        private Mock<IDeviceFamilyRepository> _deviceFamilyRepositoryMock;
        private Mock<ILog> _logMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _dlmRepositoryMock = _mockRepository.Create<IDomainLimitationModuleRepository>();
            _deviceFamilyRepositoryMock = new Mock<IDeviceFamilyRepository>();
            _logMock = _mockRepository.Create<ILog>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void AddDLM_DlmCreated_ReturnsOkGenericResponse()
        {
            var inLimitationManager = FakeLimitationsManager();
            var outLimitationManager = new LimitationsManager();
            _dlmRepositoryMock
                .Setup(x => x.Add(1, 7, It.IsAny<LimitationsManager>()))
                .Returns(outLimitationManager);
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(1))
                .Returns(FakeDeviceFamilyGenericResponse());
            var baseDomain = new FakeBaseDomain(1, _dlmRepositoryMock.Object, _logMock.Object, _deviceFamilyRepositoryMock.Object);

            var result = baseDomain.AddDLM(1, inLimitationManager, 7);

            result.Should().NotBeNull();
            result.Status.Should().Match<Status>(x => x.Code == (int)eResponseStatus.OK && x.Message == "OK");
            result.Object.Should().Be(outLimitationManager);
        }

        [Test]
        public void AddDLM_DlmNotCreated_ReturnsErrorGenericResponse()
        {
            _dlmRepositoryMock
                .Setup(x => x.Add(1, 7, It.IsAny<LimitationsManager>()))
                .Returns((LimitationsManager)null);
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(1))
                .Returns(FakeDeviceFamilyGenericResponse());
            var baseDomain = new FakeBaseDomain(1, _dlmRepositoryMock.Object, _logMock.Object, _deviceFamilyRepositoryMock.Object);

            var result = baseDomain.AddDLM(1, FakeLimitationsManager(), 7);

            result.Should().NotBeNull();
            result.Status.Should().Match<Status>(x => x.Code == (int)eResponseStatus.Error && x.Message == "DLM not created");
            result.Object.Should().BeNull();
        }

        [Test]
        public void AddDLM_ExceptionIsThrown_ReturnsErrorGenericResponse()
        {
            var exception = new Exception("Message");
            _dlmRepositoryMock
                .Setup(x => x.Add(1, 7, It.IsAny<LimitationsManager>()))
                .Throws(exception);
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(1))
                .Returns(FakeDeviceFamilyGenericResponse());
            _logMock
                .Setup(x => x.Error("AddDLM - failed groupId=1, userId=7, exception: Message", exception));
            var baseDomain = new FakeBaseDomain(1, _dlmRepositoryMock.Object, _logMock.Object, _deviceFamilyRepositoryMock.Object);

            var result = baseDomain.AddDLM(1, FakeLimitationsManager(), 7);

            result.Should().NotBeNull();
            result.Status.Should().Match<Status>(x => x.Code == (int)eResponseStatus.Error && x.Message == "Error");
            result.Object.Should().BeNull();
        }

        [Test]
        public void DeleteDLM_DlmNotDeleted_ReturnsDlmNotExistStatus()
        {
            _dlmRepositoryMock
                .Setup(x => x.Delete(2, 3))
                .Returns(false);
            var baseDomain = new FakeBaseDomain(1, _dlmRepositoryMock.Object, _logMock.Object, _deviceFamilyRepositoryMock.Object);

            var result = baseDomain.DeleteDLM(1, 2, 3);

            result.Should().Match<Status>(x => x.Code == (int)eResponseStatus.DlmNotExist && x.Message == "DlmNotExist");
        }

        [Test]
        public void DeleteDLM_ExceptionIsThrown_ReturnsErrorStatus()
        {
            var exception = new Exception("Message");
            _dlmRepositoryMock
                .Setup(x => x.Delete(2, 3))
                .Throws(exception);
            _logMock
                .Setup(x => x.Error("Failed to delete DLM. userId:3, groupId:1, dlmId:2. Message: Message", exception));
            var baseDomain = new FakeBaseDomain(1, _dlmRepositoryMock.Object, _logMock.Object, _deviceFamilyRepositoryMock.Object);

            var result = baseDomain.DeleteDLM(1, 2, 3);

            result.Should().Match<Status>(x => x.Code == (int)eResponseStatus.Error && x.Message == "Error");
        }

        [Test]
        public void UpdateDLM_DlmUpdated_ReturnsOkGenericResponse()
        {
            var inLimitationManager = FakeLimitationsManager();
            var outLimitationManager = new LimitationsManager();
            _dlmRepositoryMock
                .Setup(x => x.Update(1, 7, It.IsAny<LimitationsManager>()))
                .Returns(outLimitationManager);
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(1))
                .Returns(FakeDeviceFamilyGenericResponse());
            var baseDomain = new FakeBaseDomain(1, _dlmRepositoryMock.Object, _logMock.Object, _deviceFamilyRepositoryMock.Object);

            var result = baseDomain.UpdateDLM(1, 1, 7, inLimitationManager);

            result.Should().NotBeNull();
            result.resp.Should().Match<Status>(x => x.Code == (int)eResponseStatus.OK && x.Message == "OK");
            result.dlm.Should().Be(outLimitationManager);
        }

        [Test]
        public void UpdateDLM_DlmNotUpdated_ReturnsErrorGenericResponse()
        {
            _dlmRepositoryMock
                .Setup(x => x.Update(1, 7, It.IsAny<LimitationsManager>()))
                .Returns((LimitationsManager)null);
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(1))
                .Returns(FakeDeviceFamilyGenericResponse());
            var baseDomain = new FakeBaseDomain(1, _dlmRepositoryMock.Object, _logMock.Object, _deviceFamilyRepositoryMock.Object);

            var result = baseDomain.UpdateDLM(1, 1, 7, FakeLimitationsManager());

            result.Should().NotBeNull();
            result.resp.Should().Match<Status>(x => x.Code == (int)eResponseStatus.Error && x.Message == "DLM not updated");
            result.dlm.Should().BeNull();
        }

        [Test]
        public void UpdateDLM_DlmNotUpdated_ReturnsErrorGenericResponseDeviceFamilies()
        {
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(1))
                .Returns(new GenericListResponse<DeviceFamily>());
            var baseDomain = new FakeBaseDomain(1, _dlmRepositoryMock.Object, _logMock.Object, _deviceFamilyRepositoryMock.Object);

            var result = baseDomain.UpdateDLM(123, 1, 7, FakeLimitationsManager());

            result.Should().NotBeNull();
            result.resp.Should().Match<Status>(x => x.Code == (int)eResponseStatus.Error && x.Message == "DLM is not valid, can't retrieve device family.");
            result.dlm.Should().BeNull();
        }

        [Test]
        public void UpdateDLM_DlmNotUpdated_ReturnsErrorDeviceFamiliesNotValidGenericResponse()
        {
            _deviceFamilyRepositoryMock
                .Setup(x => x.List(1))
                .Returns(FakeDeviceFamilyDiffIdsGenericResponse);
            var baseDomain = new FakeBaseDomain(1, _dlmRepositoryMock.Object, _logMock.Object, _deviceFamilyRepositoryMock.Object);

            var result = baseDomain.UpdateDLM(123, 1, 7, FakeLimitationsManager());

            result.Should().NotBeNull();
            result.resp.Should().Match<Status>(x => x.Code == (int)eResponseStatus.Error && x.Message == "DLM is not valid, these device family ids doesn't exist 1,2");
            result.dlm.Should().BeNull();
        }

        private class FakeBaseDomain : BaseDomain
        {
            public FakeBaseDomain(int groupId, IDomainLimitationModuleRepository dlmRepository, ILog log, IDeviceFamilyRepository deviceFamilyRepository)
                : base(groupId, dlmRepository, deviceFamilyRepository, log)
            {
            }

            public override DomainResponseObject AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int groupId, string sCoGuid, int? regionId = null)
            {
                return null;
            }

            public override DomainResponseObject AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int groupId, int? regionId = null)
            {
                return null;
            }

            public override DomainResponseObject SubmitAddUserToDomainRequest(int groupId, int nUserGuid, string sMasterUsername)
            {
                return null;
            }

            protected override Status RemoveDomainHomeNetworkInner(long domainId, int numOfAllowedNetworks, int numOfActiveNetworks, int frequency, HomeNetwork candidate, HomeNetwork existingNetwork, DateTime dtLastDeactivationDate, ref Status res)
            {
                return null;
            }

            protected override HomeNetwork UpdateDomainHomeNetworkInner(long domainId, int numOfAllowedNetworks, int numOfActiveNetworks, int frequency, HomeNetwork candidate, HomeNetwork existingNetwork, DateTime dtLastDeactivationDate, ref Status res)
            {
                return null;
            }

            protected override Domain DomainInitializer(int groupId, int domainId, bool bCache = true)
            {
                return null;
            }
        }

        private LimitationsManager FakeLimitationsManager()
        {
            return new LimitationsManager
            {
                Concurrency = 2,
                Frequency = 3,
                Quantity = 4,
                DomainLimitName = "Domain Limit Name",
                UserFrequency = 5,
                nUserLimit = 6,
                lDeviceFamilyLimitations = new List<DeviceFamilyLimitations>
                {
                    new DeviceFamilyLimitations { deviceFamily = 1 },
                    new DeviceFamilyLimitations { deviceFamily = 2 }
                },
                Description = "description"
            };
        }

        private GenericListResponse<DeviceFamily> FakeDeviceFamilyGenericResponse()
        {
            var deviceFamilies = new List<DeviceFamily>
            {
                new DeviceFamily { Id = 1 },
                new DeviceFamily { Id = 2 }
            };

            return new GenericListResponse<DeviceFamily>(Status.Ok, deviceFamilies);
        }

        private GenericListResponse<DeviceFamily> FakeDeviceFamilyDiffIdsGenericResponse()
        {
            var deviceFamilies = new List<DeviceFamily>
            {
                new DeviceFamily { Id = 3 },
                new DeviceFamily { Id = 4 }
            };

            return new GenericListResponse<DeviceFamily>(Status.Ok, deviceFamilies);
        }
    }
}