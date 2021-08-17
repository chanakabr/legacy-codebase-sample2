using System.Collections.Generic;
using ApiLogic.Api.Managers;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Roles;
using CachingProvider.LayeredCache;
using Core.Users;
using FluentAssertions;
using KalturaRequestContext;
using Moq;
using NUnit.Framework;
using TVinciShared;
using Status = ApiObjects.Response.Status;

namespace ApiLogic.Tests.Users.Managers
{
    [TestFixture]
    public class RolesPermissionsManagerTests
    {
        private MockRepository _mockRepository;
        private Mock<ILayeredCache> _layeredCacheMock;
        private Mock<IRequestContextUtils> _requestContextUtilsMock;
        private Mock<IGeneralPartnerConfigManager> _generalPartnerConfigManagerMock;
        private Mock<IUserModule> _userModuleMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _layeredCacheMock = new Mock<ILayeredCache>();
            _requestContextUtilsMock = _mockRepository.Create<IRequestContextUtils>();
            _generalPartnerConfigManagerMock = _mockRepository.Create<IGeneralPartnerConfigManager>();
            _userModuleMock = _mockRepository.Create<IUserModule>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [TestCase(SuspensionProfileInheritanceType.Default, true)]
        [TestCase(SuspensionProfileInheritanceType.Never, true)]
        [TestCase(SuspensionProfileInheritanceType.Never, false)]
        public void AllowActionInSuspendedDomain_AllowedPartnerRequestAndUserWithoutSuspendedPermission_ReturnsTrue(SuspensionProfileInheritanceType inheritanceType, bool isDefault)
        {
            _requestContextUtilsMock
                .Setup(x => x.IsPartnerRequest())
                .Returns(true);
            _generalPartnerConfigManagerMock
                .Setup(x => x.GetGeneralPartnerConfig(1))
                .Returns(new GeneralPartnerConfig { SuspensionProfileInheritanceType = inheritanceType });
            var rolesPermissionsManager = new RolesPermissionsManager(_layeredCacheMock.Object, _requestContextUtilsMock.Object, _generalPartnerConfigManagerMock.Object, _userModuleMock.Object);

            var result = rolesPermissionsManager.AllowActionInSuspendedDomain(1, 2, isDefault);

            result.Should().BeTrue();
        }

        [TestCase(SuspensionProfileInheritanceType.Always, true)]
        [TestCase(SuspensionProfileInheritanceType.Always, false)]
        [TestCase(SuspensionProfileInheritanceType.Default, false)]
        public void AllowActionInSuspendedDomain_ForbiddenPartnerRequestAndUserWithoutSuspendedPermission_ReturnsFalse(SuspensionProfileInheritanceType inheritanceType, bool isDefault)
        {
            _layeredCacheMock
                .Setup(FakeRoles(false), true, false);
            _requestContextUtilsMock
                .Setup(x => x.IsPartnerRequest())
                .Returns(true);
            _generalPartnerConfigManagerMock
                .Setup(x => x.GetGeneralPartnerConfig(1))
                .Returns(new GeneralPartnerConfig { SuspensionProfileInheritanceType = inheritanceType });
            _userModuleMock
                .Setup(x => x.GetUserData(1, 2, string.Empty))
                .Returns(FakeUserResponse(true));
            var rolesPermissionsManager = new RolesPermissionsManager(_layeredCacheMock.Object, _requestContextUtilsMock.Object, _generalPartnerConfigManagerMock.Object, _userModuleMock.Object);

            var result = rolesPermissionsManager.AllowActionInSuspendedDomain(1, 2, isDefault);

            result.Should().BeFalse();
        }

        [Test]
        public void AllowActionInSuspendedDomain_PartnerRequestAndNullGeneralPartnerConfigAndUserWithoutSuspendedPermission_ReturnsFalse()
        {
            _layeredCacheMock
                .Setup(FakeRoles(false), true, false);
            _requestContextUtilsMock
                .Setup(x => x.IsPartnerRequest())
                .Returns(true);
            _generalPartnerConfigManagerMock
                .Setup(x => x.GetGeneralPartnerConfig(1))
                .Returns((GeneralPartnerConfig)null);
            _userModuleMock
                .Setup(x => x.GetUserData(1, 2, string.Empty))
                .Returns(FakeUserResponse(true));
            var rolesPermissionsManager = new RolesPermissionsManager(_layeredCacheMock.Object, _requestContextUtilsMock.Object, _generalPartnerConfigManagerMock.Object, _userModuleMock.Object);

            var result = rolesPermissionsManager.AllowActionInSuspendedDomain(1, 2, true);

            result.Should().BeFalse();
        }

        [Test]
        public void AllowActionInSuspendedDomain_UserResponseWithError_ReturnsFalse()
        {
            _requestContextUtilsMock
                .Setup(x => x.IsPartnerRequest())
                .Returns(false);
            _userModuleMock
                .Setup(x => x.GetUserData(1, 2, string.Empty))
                .Returns(FakeUserResponse(false));
            var rolesPermissionsManager = new RolesPermissionsManager(_layeredCacheMock.Object, _requestContextUtilsMock.Object, _generalPartnerConfigManagerMock.Object, _userModuleMock.Object);

            var result = rolesPermissionsManager.AllowActionInSuspendedDomain(1, 2, true);

            result.Should().BeFalse();
        }

        [Test]
        public void AllowActionInSuspendedDomain_UserWithoutDomain_ReturnsFalse()
        {
            _requestContextUtilsMock
                .Setup(x => x.IsPartnerRequest())
                .Returns(false);
            _userModuleMock
                .Setup(x => x.GetUserData(1, 2, string.Empty))
                .Returns(FakeUserResponse(true, 0));
            var rolesPermissionsManager = new RolesPermissionsManager(_layeredCacheMock.Object, _requestContextUtilsMock.Object, _generalPartnerConfigManagerMock.Object, _userModuleMock.Object);

            var result = rolesPermissionsManager.AllowActionInSuspendedDomain(1, 2, true);

            result.Should().BeFalse();
        }

        [Test]
        public void AllowActionInSuspendedDomain_UserWithoutAllowActionInSuspendedDomainPermission_ReturnsFalse()
        {
            _layeredCacheMock
                .Setup(FakeRoles(false), true, false);
            _requestContextUtilsMock
                .Setup(x => x.IsPartnerRequest())
                .Returns(false);
            _userModuleMock
                .Setup(x => x.GetUserData(1, 2, string.Empty))
                .Returns(FakeUserResponse(true));
            var rolesPermissionsManager = new RolesPermissionsManager(_layeredCacheMock.Object, _requestContextUtilsMock.Object, _generalPartnerConfigManagerMock.Object, _userModuleMock.Object);

            var result = rolesPermissionsManager.AllowActionInSuspendedDomain(1, 2, true);

            result.Should().BeFalse();
        }

        [Test]
        public void AllowActionInSuspendedDomain_UserWithAllowActionInSuspendedDomainPermission_ReturnsTrue()
        {
            _layeredCacheMock
                .Setup(FakeRoles(true), true, false);
            _requestContextUtilsMock
                .Setup(x => x.IsPartnerRequest())
                .Returns(false);
            _userModuleMock
                .Setup(x => x.GetUserData(1, 2, string.Empty))
                .Returns(FakeUserResponse(true));
            _userModuleMock
                .Setup(x => x.GetUserRoleIds(1, 2))
                .Returns(new LongIdsResponse(Status.Ok, new List<long> { 3 }));
            var rolesPermissionsManager = new RolesPermissionsManager(_layeredCacheMock.Object, _requestContextUtilsMock.Object, _generalPartnerConfigManagerMock.Object, _userModuleMock.Object);

            var result = rolesPermissionsManager.AllowActionInSuspendedDomain(1, 2, true);

            result.Should().BeTrue();
        }

        private List<Role> FakeRoles(bool hasPermission)
        {
            var roles = new List<Role> { new Role { Id = 3, Permissions = new List<Permission>() } };
            if (hasPermission)
            {
                roles[0].Permissions.Add(new Permission { Name = RolePermissions.ALLOW_ACTION_IN_SUSPENDED_DOMAIN.ToString() });
            }

            return roles;
        }

        private UserResponseObject FakeUserResponse(bool isOk, int domainId = 4)
        {
            return new UserResponseObject
            {
                m_RespStatus = isOk ? ResponseStatus.OK : ResponseStatus.InternalError,
                m_user = new User
                {
                    m_domianID = domainId
                }
            };
        }
    }
}