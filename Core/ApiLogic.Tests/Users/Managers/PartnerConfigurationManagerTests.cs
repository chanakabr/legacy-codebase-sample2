using ApiLogic.Api.Managers;
using APILogic.Api.Managers;
using ApiObjects;
using CachingProvider.LayeredCache;
using Core.Users;
using DAL;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TVinciShared;
namespace ApiLogic.Tests.Users.Managers
{
    [TestFixture]
    public class PartnerConfigurationManagerTests
    {
        delegate void MockGetPartnerConfigurationDBFromCache(string key, ref GeneralPartnerConfig genericParameter, Func<Dictionary<string, object>, Tuple<GeneralPartnerConfig, bool>> fillObjectMethod,
                                      Dictionary<string, object> funcParameters, int groupId, string layeredCacheConfigName, List<string> inValidationKeys = null,
                                      bool shouldUseAutoNameTypeHandling = false);

        [TestCase]
        public void CheckSuspend()
        {
            var generalPartnerConfig = new Mock<GeneralPartnerConfig>();
            generalPartnerConfig.Object.SuspensionProfileInheritanceType = SuspensionProfileInheritanceType.Never;

            var layeredCacheMock = getMockGeneralPartnerConfigFromCache(generalPartnerConfig.Object);

            var repositoryMock = Mock.Of<IVirtualAssetPartnerConfigRepository>();
            var requestContextUtilsMock = new Mock<IRequestContextUtils>();
            requestContextUtilsMock.Setup(x => x.IsPartnerRequest())
                                         .Returns(true);

            var managerMock = new PartnerConfigurationManager(layeredCacheMock.Object, repositoryMock, requestContextUtilsMock.Object);
            var response = managerMock.AllowSuspendedAction(It.IsAny<int>());

            Assert.That(response, Is.EqualTo(true));

            generalPartnerConfig.Object.SuspensionProfileInheritanceType = SuspensionProfileInheritanceType.Always;
            layeredCacheMock = getMockGeneralPartnerConfigFromCache(generalPartnerConfig.Object);
            managerMock = new PartnerConfigurationManager(layeredCacheMock.Object, repositoryMock, requestContextUtilsMock.Object);
            response = managerMock.AllowSuspendedAction(It.IsAny<int>());
            Assert.That(response, Is.EqualTo(false));

            generalPartnerConfig.Object.SuspensionProfileInheritanceType = SuspensionProfileInheritanceType.Default;
            layeredCacheMock = getMockGeneralPartnerConfigFromCache(generalPartnerConfig.Object);
            managerMock = new PartnerConfigurationManager(layeredCacheMock.Object, repositoryMock, requestContextUtilsMock.Object);
            response = managerMock.AllowSuspendedAction(It.IsAny<int>(), true);
            Assert.That(response, Is.EqualTo(true));

            response = managerMock.AllowSuspendedAction(It.IsAny<int>(), false);
            Assert.That(response, Is.EqualTo(false));
        }

        [TestCase]
        public void CheckSuspendIfNotOperator()
        {
            var layeredCacheMock = getMockGeneralPartnerConfigFromCache(new Mock<GeneralPartnerConfig>().Object);

            var repositoryMock = Mock.Of<IVirtualAssetPartnerConfigRepository>();
            var requestContextUtilsMock = new Mock<IRequestContextUtils>();
            requestContextUtilsMock.Setup(x => x.IsPartnerRequest())
                                         .Returns(false);

            var managerMock = new PartnerConfigurationManager(layeredCacheMock.Object, repositoryMock, requestContextUtilsMock.Object);
            var response = managerMock.AllowSuspendedAction(It.IsAny<int>());

            Assert.That(response, Is.EqualTo(false));
        }

        private Mock<ILayeredCache> getMockGeneralPartnerConfigFromCache(GeneralPartnerConfig generalPartnerConfig)
        {
            var layeredCacheMock = new Mock<ILayeredCache>();
            layeredCacheMock.Setup(x => x.Get(It.IsAny<string>(),
                                             ref It.Ref<GeneralPartnerConfig>.IsAny,
                                             It.IsAny<Func<Dictionary<string, object>, Tuple<GeneralPartnerConfig, bool>>>(),
                                             It.IsAny<Dictionary<string, object>>(),
                                             It.IsAny<int>(),
                                             It.IsAny<string>(),
                                             It.IsAny<List<string>>(),
                                             It.IsAny<bool>()))
                           .Callback(new MockGetPartnerConfigurationDBFromCache((string key,
                                                          ref GeneralPartnerConfig genericParameter,
                                                          Func<Dictionary<string, object>, Tuple<GeneralPartnerConfig, bool>> fillObjectMethod,
                                                          Dictionary<string, object> funcParameters,
                                                          int groupId,
                                                          string layeredCacheConfigName,
                                                          List<string> inValidationKeys,
                                                          bool shouldUseAutoNameTypeHandling) =>
                           {
                               genericParameter = generalPartnerConfig;
                           }))
                          .Returns(true);
            return layeredCacheMock;
        }

       
    }
}
