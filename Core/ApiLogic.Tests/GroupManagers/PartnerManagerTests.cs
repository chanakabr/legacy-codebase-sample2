using AutoFixture;
using NUnit.Framework;
using System.Collections.Generic;
using ApiObjects.Response;
using Moq;
using DAL;
using Core.GroupManagers;
using System;
using System.Linq;
using QueueWrapper;
using ConfigurationManager;
using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using ApiLogic.Users.Managers;
using FluentAssertions;
using QueueWrapper.Queues;
using RabbitMQ.Client;
using ApiObjects.Base;
using System.Collections;
using QueueWrapper.Enums;

namespace ApiLogic.Tests.GroupManagers
{
    [TestFixture]
    public class PartnerManagerTests
    {
        private readonly RabbitConfiguration _rabbitConfiguration = new RabbitConfiguration
        {
            Default =
            {
                Password = new BaseValue<string>("password", "ggg", false,
                    "RabbitMQ login password. Only for 'default' it is mandatory.")
            }
        };

        private readonly Dictionary<string, string> _rabbitBindings = new Dictionary<string, string>
        {
            {"tasks.cdr_notification", "CDR_NOTIFICATION\\partner_id"}
        };

        private readonly Dictionary<string, string> _rabbitWithoutBindings = new Dictionary<string, string>(0);

        [Test]
        public void CheckGetPartnersWithoutFilter()
        {
            var fixture = new Fixture();
            var partnerDal = new Mock<IPartnerDal>();
            var allPartners = fixture.CreateMany<ApiObjects.Partner>(5).ToList();
            partnerDal.Setup(x => x.GetPartners()).Returns(allPartners);

            var manager = new PartnerManager(partnerDal.Object, Mock.Of<IRabbitConnection>(),
                Mock.Of<IApplicationConfiguration>(), Mock.Of<IUserManager>(), Mock.Of<IRabbitConfigDal>(),
                Mock.Of<IPartnerRepository>());

            var response = manager.GetPartners(null);

            response.Objects.Should().BeEquivalentTo(allPartners);
            response.Status.Should().BeEquivalentTo(Status.Ok);
            response.TotalItems.Should().Be(allPartners.Count);
        }

        [Test]
        public void CheckGetPartnersWithFilter()
        {
            var fixture = new Fixture();
            var partnerDal = new Mock<IPartnerDal>();
            var allPartners = fixture.CreateMany<ApiObjects.Partner>(5).ToList();
            var partner1 = fixture.Create<ApiObjects.Partner>();
            var partner2 = fixture.Create<ApiObjects.Partner>();
            allPartners.Add(partner1);
            allPartners.Add(partner2);
            partnerDal.Setup(x => x.GetPartners()).Returns(allPartners);

            var manager = new PartnerManager(partnerDal.Object, Mock.Of<IRabbitConnection>(),
                Mock.Of<IApplicationConfiguration>(), Mock.Of<IUserManager>(), Mock.Of<IRabbitConfigDal>(),
                Mock.Of<IPartnerRepository>());

            var response = manager.GetPartners(new List<long> {partner1.Id.Value, partner2.Id.Value});

            response.Objects.Should().BeEquivalentTo(new List<ApiObjects.Partner> {partner1, partner2});
            response.Status.Should().BeEquivalentTo(Status.Ok);
            response.TotalItems.Should().Be(2);
        }

        [Test]
        public void CheckAddPartnerCantAddRoutingKeyToQueue()
        {
            var fixture = new Fixture();
            var pricingDal = new Mock<IPartnerRepository>();
            var partnerDal = new Mock<IPartnerDal>();
            partnerDal.Setup(x => x.GetPartners()).Returns(new List<ApiObjects.Partner>(0));
            partnerDal.Setup(x => x.AddPartner(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<long>())).Returns(1);
            partnerDal.Setup(x =>
                    x.SetupPartnerInUsersDb(It.IsAny<long>(), It.IsAny<List<KeyValuePair<long, long>>>(),
                        It.IsAny<long>()))
                .Returns(true);
            pricingDal.Setup(x =>
                    x.SetupPartnerInPricingDb(It.IsAny<long>(), It.IsAny<List<KeyValuePair<long, long>>>(),
                        It.IsAny<long>()))
                .Returns(true);

            var userManager = new Mock<IUserManager>();
            userManager.Setup(x => x.AddAdminUser(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fixture.Create<int>());

            var rabbitConfigDal = new Mock<IRabbitConfigDal>();
            rabbitConfigDal.Setup(x => x.GetRabbitRoutingBindings()).Returns(_rabbitBindings);

            var applicationConfiguration = new Mock<IApplicationConfiguration>();
            applicationConfiguration.Setup(x => x.RabbitConfiguration).Returns(_rabbitConfiguration);

            var rabbitConnection = new Mock<IRabbitConnection>();
            const bool canAddRoutingKey = false;
            rabbitConnection.Setup(x => x.IterateRoutingKeyQueue(It.IsAny<RabbitConfigurationData>(), It.IsAny<RoutingKeyQueueAction>()))
                .Returns(canAddRoutingKey);

            var manager = new PartnerManager(partnerDal.Object, rabbitConnection.Object,
                applicationConfiguration.Object, userManager.Object, rabbitConfigDal.Object, pricingDal.Object);

            Assert.Throws<AggregateException>(() => manager.AddPartner(fixture.Create<ApiObjects.Partner>(),
                fixture.Create<ApiObjects.PartnerSetup>(), fixture.Create<long>()));
        }

        [Test]
        public void CheckAddGroupRabbitWithNoBindings()
        {
            var fixture = new Fixture();
            var pricingDal = new Mock<IPartnerRepository>();
            var partnerDal = new Mock<IPartnerDal>();
            partnerDal.Setup(x => x.GetPartners()).Returns(new List<ApiObjects.Partner>(0));
            partnerDal.Setup(x => x.AddPartner(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<long>())).Returns(1);
            partnerDal.Setup(x =>
                    x.SetupPartnerInUsersDb(It.IsAny<long>(), It.IsAny<List<KeyValuePair<long, long>>>(),
                        It.IsAny<long>()))
                .Returns(true);
            pricingDal.Setup(x =>
                   x.SetupPartnerInPricingDb(It.IsAny<long>(), It.IsAny<List<KeyValuePair<long, long>>>(),
                       It.IsAny<long>()))
               .Returns(true);

            var userManager = new Mock<IUserManager>();
            userManager.Setup(x => x.AddAdminUser(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fixture.Create<int>());

            var rabbitConfigDal = new Mock<IRabbitConfigDal>();
            var noBindings = new Dictionary<string, string>(0);
            rabbitConfigDal.Setup(x => x.GetRabbitRoutingBindings()).Returns(noBindings);

            var manager = new PartnerManager(partnerDal.Object, Mock.Of<IRabbitConnection>(),
                Mock.Of<IApplicationConfiguration>(), userManager.Object, rabbitConfigDal.Object, pricingDal.Object);

            Assert.Throws<Exception>(() => manager.AddPartner(fixture.Create<ApiObjects.Partner>(),
                fixture.Create<ApiObjects.PartnerSetup>(), fixture.Create<long>()));
        }

        [TestCase(1, "NotAbc")]
        [TestCase(2, "Abc")]
        public void CheckAddNotUniquePartner(int existingId, string existingName)
        {
            var fixture = new Fixture();
            var partnerDal = new Mock<IPartnerDal>();
            var existingPartner = new ApiObjects.Partner {Id = existingId, Name = existingName}; 
            partnerDal.Setup(x => x.GetPartners()).Returns(new List<ApiObjects.Partner>{existingPartner});
            var manager = new PartnerManager(partnerDal.Object, Mock.Of<IRabbitConnection>(),
                Mock.Of<IApplicationConfiguration>(), Mock.Of<IUserManager>(), Mock.Of<IRabbitConfigDal>(),
                Mock.Of<IPartnerRepository>());

            var partner = new ApiObjects.Partner {Id = 1, Name = "Abc"};
            var partnerResponse = manager.AddPartner(partner,
                fixture.Create<ApiObjects.PartnerSetup>(), fixture.Create<long>());
            partnerDal.VerifyAll();
            partnerDal.VerifyNoOtherCalls();
            partnerResponse.HasObject().Should().BeFalse();
        }

        [Test]
        public void CheckAdd()
        {
            var fixture = new Fixture();
            const int partnerId = 3;
            var pricingDal = new Mock<IPartnerRepository>();
            var partnerDal = new Mock<IPartnerDal>();
            partnerDal.Setup(x => x.GetPartners()).Returns(new List<ApiObjects.Partner>(0));
            partnerDal.Setup(x => x.AddPartner(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<long>())).Returns(partnerId);
            partnerDal.Setup(x =>
                    x.SetupPartnerInUsersDb(It.IsAny<long>(), It.IsAny<List<KeyValuePair<long, long>>>(),
                        It.IsAny<long>()))
                .Returns(true);
            pricingDal.Setup(x =>
                   x.SetupPartnerInPricingDb(It.IsAny<long>(), It.IsAny<List<KeyValuePair<long, long>>>(),
                       It.IsAny<long>()))
               .Returns(true);

            var userManager = new Mock<IUserManager>();
            userManager.Setup(x => x.AddAdminUser(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fixture.Create<int>());

            var rabbitConfigDal = new Mock<IRabbitConfigDal>();
            rabbitConfigDal.Setup(x => x.GetRabbitRoutingBindings()).Returns(_rabbitBindings);

            var applicationConfiguration = new Mock<IApplicationConfiguration>();
            applicationConfiguration.Setup(x => x.RabbitConfiguration).Returns(_rabbitConfiguration);

            var rabbitConnection = new Mock<IRabbitConnection>();
            rabbitConnection.Setup(x => x.IterateRoutingKeyQueue(It.IsAny<RabbitConfigurationData>(), It.IsAny<RoutingKeyQueueAction>())).Returns(true);
            int _ = 0;
            IConnection __;
            rabbitConnection.Setup(x => x.InitializeRabbitInstance(It.IsAny<RabbitConfigurationData>(), It.IsAny<QueueAction>(), ref _, out __)).Returns(true);

            var manager = new PartnerManager(partnerDal.Object, rabbitConnection.Object,
                applicationConfiguration.Object, userManager.Object, rabbitConfigDal.Object, pricingDal.Object);

            var partnerResponse = manager.AddPartner(fixture.Create<ApiObjects.Partner>(),
                fixture.Create<ApiObjects.PartnerSetup>(), fixture.Create<long>());
            partnerResponse.HasObject().Should().BeTrue();
            rabbitConnection.Verify(x => x.IterateRoutingKeyQueue(It.Is<RabbitConfigurationData>(
                c => c.Password == "ggg" && c.Username == "admin" && c.Port == "0"
                     && c.VirtualHost == "/" && c.ExchangeType == "topic"
                     && c.Exchange == "scheduled_tasks" && c.RoutingKey == "CDR_NOTIFICATION\\3"), It.IsAny<RoutingKeyQueueAction>()), Times.Once());
            Verify(partnerDal);
            Verify(userManager);
            Verify(rabbitConfigDal);
            Verify(applicationConfiguration);
            Verify(rabbitConnection);
        }

        [TestCaseSource(nameof(DeleteCases))]
        public void CheckDelete(eResponseStatus expectedCode, bool deletePartner, bool isPartnerExsits)
        {
            var fixture = new Fixture();
            
            var partnerDal = new Mock<IPartnerDal>();
            partnerDal.Setup(x => x.DeletePartnerInUsersDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            partnerDal.Setup(x => x.IsPartnerExists(It.IsAny<int>())).Returns(isPartnerExsits);
            partnerDal.Setup(x => x.DeletePartner(It.IsAny<int>(), It.IsAny<long>())).Returns(deletePartner);
            var userManager = new Mock<IUserManager>();

            var applicationConfiguration = new Mock<IApplicationConfiguration>();
            applicationConfiguration.Setup(x => x.RabbitConfiguration).Returns(_rabbitConfiguration);

            var rabbitConfigDal = new Mock<IRabbitConfigDal>();
            rabbitConfigDal.Setup(x => x.GetRabbitRoutingBindings()).Returns(_rabbitBindings);

            var rabbitConnection = new Mock<IRabbitConnection>();
            rabbitConnection.Setup(x => x.IterateRoutingKeyQueue(It.IsAny<RabbitConfigurationData>(), It.IsAny<RoutingKeyQueueAction>())).Returns(true);
            int _ = 0;
            IConnection __;
            rabbitConnection.Setup(x => x.InitializeRabbitInstance(It.IsAny<RabbitConfigurationData>(), It.IsAny<QueueAction>(), ref _, out __)).Returns(true);

            var manager = new PartnerManager(partnerDal.Object, rabbitConnection.Object,
                applicationConfiguration.Object, userManager.Object, rabbitConfigDal.Object);

            var response = manager.Delete(fixture.Create<long>(), fixture.Create<int>());

            Assert.That(response.Code, Is.EqualTo((int)expectedCode));
        }

        [Test]
        public void DeleteSuccess()
        {
            var fixture = new Fixture();

            var partnerDal = new Mock<IPartnerDal>();
            partnerDal.Setup(x => x.DeletePartnerInUsersDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            partnerDal.Setup(x => x.IsPartnerExists(It.IsAny<int>())).Returns(true);
            partnerDal.Setup(x => x.DeletePartner(It.IsAny<int>(), It.IsAny<long>())).Returns(true);
            var userManager = new Mock<IUserManager>();

            var applicationConfiguration = new Mock<IApplicationConfiguration>();
            applicationConfiguration.Setup(x => x.RabbitConfiguration).Returns(_rabbitConfiguration);

            var rabbitConfigDal = new Mock<IRabbitConfigDal>();

            rabbitConfigDal.Setup(x => x.GetRabbitRoutingBindings()).Returns(_rabbitBindings);

            var rabbitConnection = new Mock<IRabbitConnection>();
            rabbitConnection.Setup(x => x.IterateRoutingKeyQueue(It.IsAny<RabbitConfigurationData>(), It.IsAny<RoutingKeyQueueAction>())).Returns(true);
            int _ = 0;
            IConnection __;
            rabbitConnection.Setup(x => x.InitializeRabbitInstance(It.IsAny<RabbitConfigurationData>(), It.IsAny<QueueAction>(), ref _, out __)).Returns(true);

            var manager = new PartnerManager(partnerDal.Object, rabbitConnection.Object,
                applicationConfiguration.Object, userManager.Object, rabbitConfigDal.Object);

            var response = manager.Delete(fixture.Create<long>(), fixture.Create<int>());

            Assert.That(response.Code, Is.EqualTo((int)eResponseStatus.OK));
            Verify(partnerDal);
            Verify(userManager);
            Verify(rabbitConfigDal);
            Verify(applicationConfiguration);
            Verify(rabbitConnection);
        }

        private static void Verify<T>(Mock<T> m) where T : class
        {
            m.VerifyAll();
            m.VerifyNoOtherCalls();
        }

        private static IEnumerable DeleteCases()
        {
            yield return new TestCaseData(eResponseStatus.PartnerDoesNotExist, true, false).SetName("DeletePartnerNotExist");
            yield return new TestCaseData(eResponseStatus.Error, false, true).SetName("DeletePartnerFailed");
        }
    }
}