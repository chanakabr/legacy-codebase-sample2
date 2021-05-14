using System;
using System.Collections.Generic;
using ApiLogic.Users;
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
        private Mock<ILog> _logMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _dlmRepositoryMock = _mockRepository.Create<IDomainLimitationModuleRepository>();
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
                .Setup(x => x.Add(1, 2, 3, 4, "Domain Limit Name", 5, 6, It.Is<DeviceFamilyLimitations[]>(items => items.Length == 2), 7))
                .Returns(outLimitationManager);
            var baseDomain = new FakeBaseDomain(1, _dlmRepositoryMock.Object, _logMock.Object);

            var result = baseDomain.AddDLM(1, inLimitationManager, 7);

            result.Should().NotBeNull();
            result.Status.Should().Match<Status>(x => x.Code == (int)eResponseStatus.OK && x.Message == "OK");
            result.Object.Should().Be(outLimitationManager);
        }

        [Test]
        public void AddDLM_DlmNotCreated_ReturnsErrorGenericResponse()
        {
            _dlmRepositoryMock
                .Setup(x => x.Add(1, 2, 3, 4, "Domain Limit Name", 5, 6, It.Is<DeviceFamilyLimitations[]>(items => items.Length == 2), 7))
                .Returns((LimitationsManager)null);
            var baseDomain = new FakeBaseDomain(1, _dlmRepositoryMock.Object, _logMock.Object);

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
                .Setup(x => x.Add(1, 2, 3, 4, "Domain Limit Name", 5, 6, It.Is<DeviceFamilyLimitations[]>(items => items.Length == 2), 7))
                .Throws(exception);
            _logMock
                .Setup(x => x.Error("AddDLM - failed groupId=1, userId=7, exception: Message", exception));
            var baseDomain = new FakeBaseDomain(1, _dlmRepositoryMock.Object, _logMock.Object);

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
            var baseDomain = new FakeBaseDomain(1, _dlmRepositoryMock.Object, _logMock.Object);

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
            var baseDomain = new FakeBaseDomain(1, _dlmRepositoryMock.Object, _logMock.Object);

            var result = baseDomain.DeleteDLM(1, 2, 3);

            result.Should().Match<Status>(x => x.Code == (int)eResponseStatus.Error && x.Message == "Error");
        }

        private class FakeBaseDomain : BaseDomain
        {
            public FakeBaseDomain(int groupId, IDomainLimitationModuleRepository dlmRepository, ILog log)
                : base(groupId, dlmRepository, log)
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
                    new DeviceFamilyLimitations(),
                    new DeviceFamilyLimitations()
                }
            };
        }
    }
}