using APILogic.Api.Managers;
using Core.Users;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace ApiLogic.Tests.Modules
{
    [TestFixture]
    public class DomainTests
    {
        [TestCase]
        public void CheckHouseholdDeviceAdd()
        {
            var rolesPermissionsManagerMock = new Mock<IRolesPermissionsManager>();
            rolesPermissionsManagerMock.Setup(x => x.AllowActionInSuspendedDomain(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(false);
            rolesPermissionsManagerMock.Setup(x => x.IsPermittedPermissionItem(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            var refDevice = Mock.Of<Device>();
            var domain = new Domain();
            var domainManager = new ApiLogic.Users.Managers.DomainManager(rolesPermissionsManagerMock.Object);
            domain.m_DomainStatus = DomainStatus.DomainSuspended;
            domain.m_masterGUIDs = new List<int> { It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>() };
            var response = domainManager.AddDeviceToDomain(It.IsAny<int>(),
                                            It.IsAny<int>(),
                                            It.IsAny<string>(),
                                            It.IsAny<string>(),
                                            It.IsAny<int>(),
                                            domain,
                                            ref refDevice,
                                            out bool a
                                            );
            Assert.That(a, Is.EqualTo(false));
            Assert.That(response, Is.EqualTo(DomainResponseStatus.DomainSuspended));

            //*******************************************
            rolesPermissionsManagerMock
                .Setup(x => x.AllowActionInSuspendedDomain(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(true);

            domain = new Domain();
            domain.m_DomainStatus = DomainStatus.DomainSuspended;
            domain.m_masterGUIDs = new List<int> { It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>() };
            response = domainManager.AddDeviceToDomain(It.IsAny<int>(),
                                            It.IsAny<int>(),
                                            It.IsAny<string>(),
                                            It.IsAny<string>(),
                                            It.IsAny<int>(),
                                            domain,
                                            ref refDevice,
                                            out a
                                            );

            Assert.That(response.Equals(DomainResponseStatus.DomainSuspended), Is.False);
        }
    }
}
