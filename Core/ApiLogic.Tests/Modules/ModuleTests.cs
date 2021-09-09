using ApiLogic.Users;
using ApiObjects.Response;
using Core.Users;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Module = Core.Domains.Module;
using Status = ApiObjects.Response.Status;

namespace ApiLogic.Tests.Modules
{
    [TestFixture]
    public class ModuleTests
    {
        private MockRepository _mockRepository;
        private Mock<IBaseDomain> _mockBaseDomain;
        private Mock<IBaseDomainFactory> _mockBaseDomainFactory;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _mockBaseDomain = new Mock<IBaseDomain>();
            _mockBaseDomainFactory = new Mock<IBaseDomainFactory>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void AddDLM_ValidParameters_ReturnsGenericResponse()
        {
            var limitationManager = new LimitationsManager();
            _mockBaseDomain
                .Setup(x => x.AddDLM(1, limitationManager, 2))
                .Returns(new GenericResponse<LimitationsManager>(new Status(100, "Custom Status"), new LimitationsManager()));
            _mockBaseDomainFactory
                .Setup(x => x.GetBaseImpl(1))
                .Returns(_mockBaseDomain.Object);
            var module = new Module(_mockBaseDomainFactory.Object);

            var response = module.AddDLM(1, 2, limitationManager);

            response.Should().Match<GenericResponse<LimitationsManager>>(x => x.Status.Code == 100 && x.Status.Message == "Custom Status" && x.Object != null);
        }

        [Test]
        public void AddDLM_BaseImplementationNotFound_ReturnsErrorGenericResponse()
        {
            _mockBaseDomainFactory
                .Setup(x => x.GetBaseImpl(1))
                .Returns((IBaseDomain)null);
            var module = new Module(_mockBaseDomainFactory.Object);

            var response = module.AddDLM(1, 2, new LimitationsManager());

            response.Should().Match<GenericResponse<LimitationsManager>>(x => x.Status.Code == (int)eResponseStatus.Error && x.Status.Message == "Error" && x.Object == null);
        }

        [Test]
        public void DeleteDLM_ValidParameters_ReturnsStatus()
        {
            _mockBaseDomain
                .Setup(x => x.DeleteDLM(1, 2, 3))
                .Returns(new Status(100, "Custom Status"));
            _mockBaseDomainFactory
                .Setup(x => x.GetBaseImpl(1))
                .Returns(_mockBaseDomain.Object);
            var module = new Module(_mockBaseDomainFactory.Object);

            var response = module.DeleteDLM(1, 2, 3);

            response.Should().Match<Status>(x => x.Code == 100 && x.Message == "Custom Status");
        }

        [Test]
        public void DeleteDLM_BaseImplementationNotFound_ReturnsErrorStatus()
        {
            _mockBaseDomainFactory
                .Setup(x => x.GetBaseImpl(1))
                .Returns((IBaseDomain)null);
            var module = new Module(_mockBaseDomainFactory.Object);

            var response = module.DeleteDLM(1, 2, 3);

            response.Should().Match<Status>(x => x.Code == (int)eResponseStatus.Error && x.Message == "Error");
        }
    }
}
