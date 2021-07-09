using ApiLogic.Api.Managers;
using ApiObjects;
using DAL;
using KalturaRequestContext;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.Users.Managers
{
    [TestFixture]
    public class PartnerConfigurationManagerTests
    {
        [TestCase]
        public void CheckSuspend()
        {
            var generalPartnerConfig = new Mock<IGeneralPartnerConfigManager>();
            
            var generalPartnerConfigObject = new Mock<GeneralPartnerConfig>();
            generalPartnerConfigObject.Object.SuspensionProfileInheritanceType = SuspensionProfileInheritanceType.Never;
            generalPartnerConfig.Setup(x => x.GetGeneralPartnerConfig(It.IsAny<int>()))
                                         .Returns(generalPartnerConfigObject.Object);

            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(generalPartnerConfigObject.Object, true, false);

            var repositoryMock = Mock.Of<IVirtualAssetPartnerConfigRepository>();
            var requestContextUtilsMock = new Mock<IRequestContextUtils>();
            requestContextUtilsMock.Setup(x => x.IsPartnerRequest())
                                         .Returns(true);

            var managerMock = new PartnerConfigurationManager(layeredCacheMock.Object, repositoryMock, requestContextUtilsMock.Object, generalPartnerConfig.Object);
            var response = managerMock.AllowSuspendedAction(It.IsAny<int>());

            Assert.That(response, Is.EqualTo(true));

            generalPartnerConfigObject.Object.SuspensionProfileInheritanceType = SuspensionProfileInheritanceType.Always;
            layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(generalPartnerConfigObject.Object, true, false);
            managerMock = new PartnerConfigurationManager(layeredCacheMock.Object, repositoryMock, requestContextUtilsMock.Object, generalPartnerConfig.Object);
            response = managerMock.AllowSuspendedAction(It.IsAny<int>());
            Assert.That(response, Is.EqualTo(false));

            generalPartnerConfigObject.Object.SuspensionProfileInheritanceType = SuspensionProfileInheritanceType.Default;
            layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(generalPartnerConfigObject.Object, true, false);
            managerMock = new PartnerConfigurationManager(layeredCacheMock.Object, repositoryMock, requestContextUtilsMock.Object, generalPartnerConfig.Object);
            response = managerMock.AllowSuspendedAction(It.IsAny<int>(), true);
            Assert.That(response, Is.EqualTo(true));

            response = managerMock.AllowSuspendedAction(It.IsAny<int>(), false);
            Assert.That(response, Is.EqualTo(false));
        }

        [TestCase]
        public void CheckSuspendIfNotOperator()
        {
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(new Mock<GeneralPartnerConfig>().Object, true, false);

            var repositoryMock = Mock.Of<IVirtualAssetPartnerConfigRepository>();
            var requestContextUtilsMock = new Mock<IRequestContextUtils>();
            requestContextUtilsMock.Setup(x => x.IsPartnerRequest())
                                         .Returns(false);
            var generalPartnerConfigManagerMock = Mock.Of<IGeneralPartnerConfigManager>();

            var managerMock = new PartnerConfigurationManager(layeredCacheMock.Object, repositoryMock, requestContextUtilsMock.Object, generalPartnerConfigManagerMock);
            var response = managerMock.AllowSuspendedAction(It.IsAny<int>());

            Assert.That(response, Is.EqualTo(false));
        }
    }
}
