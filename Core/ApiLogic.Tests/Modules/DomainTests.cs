using ApiLogic.Api.Managers;
using APILogic.Api.Managers;
using Core.Users;
using DAL;
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
            var partnerConfigurationManagerMock = new Mock<IPartnerConfigurationManager>();
            partnerConfigurationManagerMock.Setup(x => x.AllowSuspendedAction(It.IsAny<int>(), false))
                .Returns(false);

            var rolesPermissionsManagerMock = new Mock<IRolesPermissionsManager>();
            rolesPermissionsManagerMock.Setup(x => x.IsPermittedPermissionItem(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            var reflong = It.IsAny<long>();
            var isActive = It.IsAny<int>();
            var nStatus = It.IsAny<int>();
            var domainDeviceId = It.IsAny<long>();
            var domainDalMock = new Mock<IDomainDal>();
            domainDalMock.Setup(x => x.GetDeviceDomainData(It.IsAny<int>(), It.IsAny<string>(), ref reflong, ref isActive, ref nStatus, ref domainDeviceId))
                .Returns(It.IsAny<int>());

            var refDevice = Mock.Of<Device>();
            var domain = new Domain();
            var domainManager = new ApiLogic.Users.Managers.DomainManager(domainDalMock.Object, partnerConfigurationManagerMock.Object, rolesPermissionsManagerMock.Object);
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
            partnerConfigurationManagerMock.Setup(x => x.AllowSuspendedAction(It.IsAny<int>(), false))
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
