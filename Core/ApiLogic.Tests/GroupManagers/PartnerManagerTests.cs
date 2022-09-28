using ApiLogic.Users.Managers;
using ApiObjects.Response;
using AutoFixture;
using Phx.Lib.Appconfig;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.GroupManagers;
using DAL;
using ElasticSearch.Common;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using QueueWrapper;
using QueueWrapper.Enums;
using QueueWrapper.Queues;
using RabbitMQ.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Phx.Lib.Appconfig.Types;
using Phx.Lib.Appconfig.Settings.Base;

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


        [TestCaseSource(nameof(ListCases))]
        public void CheckList(List<long> partnerIds, List<ApiObjects.Partner> expectedPartners)
        {
            var partnerDal = new Mock<IPartnerDal>();
            var allPartners = new List<int>() { 1, 2, 3, 4, 5 };

            partnerDal.Setup(x => x.GetPartners()).Returns(new List<ApiObjects.Partner>(allPartners.Select( x=> new ApiObjects.Partner() { Id = x })));

            var manager = new PartnerManager(partnerDal.Object, 
                                             Mock.Of<IRabbitConnection>(),
                                             Mock.Of<IApplicationConfiguration>(), 
                                             Mock.Of<IUserManager>(), 
                                             Mock.Of<IRabbitConfigDal>(),
                                             Mock.Of<IPricingPartnerRepository>(), 
                                             Mock.Of<ICatalogManager>(),
                                             Mock.Of<IUserPartnerRepository>(), 
                                             Mock.Of<IBillingPartnerRepository>(), 
                                             Mock.Of<ICAPartnerRepository>(),
                                             Mock.Of<IGroupSettingsManager>(),
                                             Mock.Of<IIndexManagerFactory>());


            var response = manager.GetPartners(partnerIds);

            response.Status.Should().BeEquivalentTo(Status.Ok);
            for (int i = 0; i < expectedPartners.Count; i++)
            {
                Assert.AreEqual(expectedPartners[i].Id, response.Objects[i].Id);

            }
        }

        private static IEnumerable ListCases()
        {
            var allPartners = new List<int>() { 1, 2, 3, 4, 5 };
            var partners = new List<ApiObjects.Partner>(allPartners.Select(x => new ApiObjects.Partner() { Id = x }));

            var partialPartnersIds = new List<int>() { 1, 2, 3 };
            var partialPartners = new List<ApiObjects.Partner>(partialPartnersIds.Select(x => new ApiObjects.Partner() { Id = x }));

            yield return new TestCaseData(null, partners).SetName("CheckNullList");
            yield return new TestCaseData(new List<long>(), partners).SetName("CheckEmptyList");
            yield return new TestCaseData(new List<long>() { 1, 2, 3 }, partialPartners).SetName("CheckPartialList");
            yield return new TestCaseData(new List<long>() { 1, 2, 3, 4, 5 },  partners).SetName("CheckFullList");
            yield return new TestCaseData(new List<long>() { 1, 2, 3, 9, 10 }, partialPartners).SetName("CheckPartialExist");
            yield return new TestCaseData(new List<long>() { 6, 7, 8 }, new List<ApiObjects.Partner>()).SetName("ChecknoneExist");

        }

        [Test]
        public void CheckAddPartnerCantAddRoutingKeyToQueue()
        {
            var fixture = new Fixture();
            var partnerDal = new Mock<IPartnerDal>();
            var pricingPartnerRepository = new Mock<IPricingPartnerRepository>();
            var userPartnerRepository = new Mock<IUserPartnerRepository>();
            var billingPartnerRepository = new Mock<IBillingPartnerRepository>();
            var caPartnerRepository = new Mock<ICAPartnerRepository>();

            partnerDal.Setup(x => x.GetPartners()).Returns(new List<ApiObjects.Partner>(0));
            partnerDal.Setup(x => x.AddPartner(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<long>())).Returns(1);
            partnerDal.Setup(x => x.SetupPartnerInDb(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>())).Returns(true);
            pricingPartnerRepository.Setup(x => x.SetupPartnerInDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            userPartnerRepository.Setup(x => x.SetupPartnerInDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            billingPartnerRepository.Setup(x => x.SetupPartnerInDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            caPartnerRepository.Setup(x => x.SetupPartnerInDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);

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

            var manager = new PartnerManager(partnerDal.Object,
                                            rabbitConnection.Object,
                                            applicationConfiguration.Object,
                                            userManager.Object,
                                            rabbitConfigDal.Object,
                                            pricingPartnerRepository.Object,
                                            Mock.Of<ICatalogManager>(),
                                            userPartnerRepository.Object,
                                            billingPartnerRepository.Object, 
                                            caPartnerRepository.Object,
                                            Mock.Of<IGroupSettingsManager>(),
                                            Mock.Of<IIndexManagerFactory>());


            Assert.Throws<AggregateException>(() => manager.AddPartner(fixture.Create<ApiObjects.Partner>(),
                fixture.Create<ApiObjects.PartnerSetup>(), fixture.Create<long>()));
        }

        [Test]
        public void CheckAddGroupRabbitWithNoBindings()
        {
            var fixture = new Fixture();
            var pricingPartnerRepository = new Mock<IPricingPartnerRepository>();
            var partnerDal = new Mock<IPartnerDal>();
            var userPartnerRepository = new Mock<IUserPartnerRepository>();
            var billingPartnerRepository = new Mock<IBillingPartnerRepository>();
            var caPartnerRepository = new Mock<ICAPartnerRepository>();

            partnerDal.Setup(x => x.GetPartners()).Returns(new List<ApiObjects.Partner>(0));
            partnerDal.Setup(x => x.AddPartner(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<long>())).Returns(1);
            partnerDal.Setup(x => x.SetupPartnerInDb(It.IsAny<long>(), It.IsAny<string>(),It.IsAny<long>(), It.IsAny<bool>())).Returns(true);
            pricingPartnerRepository.Setup(x => x.SetupPartnerInDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            userPartnerRepository.Setup(x => x.SetupPartnerInDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            billingPartnerRepository.Setup(x => x.SetupPartnerInDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            caPartnerRepository.Setup(x => x.SetupPartnerInDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);

            var userManager = new Mock<IUserManager>();
            userManager.Setup(x => x.AddAdminUser(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(fixture.Create<int>());

            var rabbitConfigDal = new Mock<IRabbitConfigDal>();
            var noBindings = new Dictionary<string, string>(0);
            rabbitConfigDal.Setup(x => x.GetRabbitRoutingBindings()).Returns(noBindings);

            var manager = new PartnerManager(partnerDal.Object,
                                            Mock.Of<IRabbitConnection>(),
                                            Mock.Of<IApplicationConfiguration>(),
                                            userManager.Object,
                                            rabbitConfigDal.Object,
                                            pricingPartnerRepository.Object,
                                            Mock.Of<ICatalogManager>(),
                                            userPartnerRepository.Object,
                                            billingPartnerRepository.Object,
                                            caPartnerRepository.Object,
                                            Mock.Of<IGroupSettingsManager>(),
                                            Mock.Of<IIndexManagerFactory>());

            Assert.Throws<Exception>(() => manager.AddPartner(fixture.Create<ApiObjects.Partner>(),
                fixture.Create<ApiObjects.PartnerSetup>(), fixture.Create<long>()));
        }

        [TestCase(1, "NotAbc")]
        [TestCase(2, "Abc")]
        public void CheckAddNotUniquePartner(int existingId, string existingName)
        {
            var fixture = new Fixture();
            var partnerDal = new Mock<IPartnerDal>();
            var pricingPartnerRepository = new Mock<IPricingPartnerRepository>();
            var userPartnerRepository = new Mock<IUserPartnerRepository>();
            var billingPartnerRepository = new Mock<IBillingPartnerRepository>();
            var caPartnerRepository = new Mock<ICAPartnerRepository>();


            var existingPartner = new ApiObjects.Partner { Id = existingId, Name = existingName };
            partnerDal.Setup(x => x.GetPartners()).Returns(new List<ApiObjects.Partner> { existingPartner });

            var manager = new PartnerManager(partnerDal.Object,
                                             Mock.Of<IRabbitConnection>(),
                                             Mock.Of<IApplicationConfiguration>(),
                                             Mock.Of<IUserManager>(),
                                             Mock.Of<IRabbitConfigDal>(),
                                             Mock.Of<IPricingPartnerRepository>(),
                                             Mock.Of<ICatalogManager>(),
                                             Mock.Of<IUserPartnerRepository>(),
                                             Mock.Of<IBillingPartnerRepository>(),
                                             Mock.Of<ICAPartnerRepository>(),
                                             Mock.Of<IGroupSettingsManager>(),
                                             Mock.Of<IIndexManagerFactory>());

            var partner = new ApiObjects.Partner { Id = 1, Name = "Abc" };
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
            var pricingPartnerRepository = new Mock<IPricingPartnerRepository>();
            var userPartnerRepository = new Mock<IUserPartnerRepository>();
            var billingPartnerRepository = new Mock<IBillingPartnerRepository>();
            var caPartnerRepository = new Mock<ICAPartnerRepository>();

            var partnerDal = new Mock<IPartnerDal>();
            partnerDal.Setup(x => x.GetPartners()).Returns(new List<ApiObjects.Partner>(0));
            partnerDal.Setup(x => x.AddPartner(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<long>())).Returns(partnerId);
            partnerDal.Setup(x => x.SetupPartnerInDb(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<bool>())).Returns(true);
            pricingPartnerRepository.Setup(x => x.SetupPartnerInDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            userPartnerRepository.Setup(x => x.SetupPartnerInDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            billingPartnerRepository.Setup(x => x.SetupPartnerInDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            caPartnerRepository.Setup(x => x.SetupPartnerInDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);

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

            var manager = new PartnerManager(partnerDal.Object,
                                            rabbitConnection.Object,
                                            applicationConfiguration.Object,
                                            userManager.Object,
                                            rabbitConfigDal.Object,
                                            pricingPartnerRepository.Object,
                                            Mock.Of<ICatalogManager>(),
                                            userPartnerRepository.Object,
                                            billingPartnerRepository.Object, 
                                            caPartnerRepository.Object,
                                            Mock.Of<IGroupSettingsManager>(),
                                            Mock.Of<IIndexManagerFactory>());


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
            var userDal = new Mock<IUserPartnerRepository>();
            userDal.Setup(x => x.DeletePartnerDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            partnerDal.Setup(x => x.IsPartnerExists(It.IsAny<int>())).Returns(isPartnerExsits);
            partnerDal.Setup(x => x.DeletePartner(It.IsAny<int>(), It.IsAny<long>())).Returns(deletePartner);
            var userManager = new Mock<IUserManager>();
            var pricingPartnerRepository = new Mock<IPricingPartnerRepository>();
            var userPartnerRepository = new Mock<IUserPartnerRepository>();
            var billingPartnerRepository = new Mock<IBillingPartnerRepository>();
            var caPartnerRepository = new Mock<ICAPartnerRepository>();
            var groupSettingsManager = new Mock<IGroupSettingsManager>();

            var applicationConfiguration = new Mock<IApplicationConfiguration>();
            applicationConfiguration.Setup(x => x.RabbitConfiguration).Returns(_rabbitConfiguration);

            var rabbitConfigDal = new Mock<IRabbitConfigDal>();
            rabbitConfigDal.Setup(x => x.GetRabbitRoutingBindings()).Returns(_rabbitBindings);

            var rabbitConnection = new Mock<IRabbitConnection>();
            rabbitConnection.Setup(x => x.IterateRoutingKeyQueue(It.IsAny<RabbitConfigurationData>(), It.IsAny<RoutingKeyQueueAction>())).Returns(true);
            int _ = 0;
            IConnection __;
            rabbitConnection.Setup(x => x.InitializeRabbitInstance(It.IsAny<RabbitConfigurationData>(), It.IsAny<QueueAction>(), ref _, out __)).Returns(true);

            caPartnerRepository.Setup(x => x.DeletePartnerBasicDataDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            billingPartnerRepository.Setup(x => x.DeletePartnerBasicDataDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            userPartnerRepository.Setup(x => x.DeletePartnerDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            pricingPartnerRepository.Setup(x => x.DeletePartnerBasicDataDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            groupSettingsManager.Setup(x => x.GetEpgFeatureVersion(It.IsAny<int>())).Returns(ApiObjects.EpgFeatureVersion.V1);

            var manager = new PartnerManager(partnerDal.Object,
                                            rabbitConnection.Object,
                                            applicationConfiguration.Object,
                                            userManager.Object,
                                            rabbitConfigDal.Object,
                                            pricingPartnerRepository.Object,


                                            Mock.Of<ICatalogManager>(),
                                            userPartnerRepository.Object,
                                            billingPartnerRepository.Object,
                                            caPartnerRepository.Object,
                                            groupSettingsManager.Object,
                                            Mock.Of<IIndexManagerFactory>());

            var response = manager.Delete(fixture.Create<long>(), fixture.Create<int>());

            Assert.That(response.Code, Is.EqualTo((int)expectedCode));
        }

        [Test]
        public void DeleteSuccess()
        {
            var fixture = new Fixture();

            var partnerDal = new Mock<IPartnerDal>();
            var userDal = new Mock<IUserPartnerRepository>();

            userDal.Setup(x => x.DeletePartnerDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            partnerDal.Setup(x => x.IsPartnerExists(It.IsAny<int>())).Returns(true);
            partnerDal.Setup(x => x.DeletePartner(It.IsAny<int>(), It.IsAny<long>())).Returns(true);
            var userManager = new Mock<IUserManager>();
            var pricingPartnerRepository = new Mock<IPricingPartnerRepository>();
            var userPartnerRepository = new Mock<IUserPartnerRepository>();
            var billingPartnerRepository = new Mock<IBillingPartnerRepository>();
            var caPartnerRepository = new Mock<ICAPartnerRepository>();
            var groupSettingsManager = new Mock<IGroupSettingsManager>();

            var applicationConfiguration = new Mock<IApplicationConfiguration>();
            applicationConfiguration.Setup(x => x.RabbitConfiguration).Returns(_rabbitConfiguration);

            var rabbitConfigDal = new Mock<IRabbitConfigDal>();

            rabbitConfigDal.Setup(x => x.GetRabbitRoutingBindings()).Returns(_rabbitBindings);

            var rabbitConnection = new Mock<IRabbitConnection>();
            rabbitConnection.Setup(x => x.IterateRoutingKeyQueue(It.IsAny<RabbitConfigurationData>(), It.IsAny<RoutingKeyQueueAction>())).Returns(true);
            int _ = 0;
            IConnection __;
            rabbitConnection.Setup(x => x.InitializeRabbitInstance(It.IsAny<RabbitConfigurationData>(), It.IsAny<QueueAction>(), ref _, out __)).Returns(true);

            caPartnerRepository.Setup(x => x.DeletePartnerBasicDataDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            billingPartnerRepository.Setup(x => x.DeletePartnerBasicDataDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            userPartnerRepository.Setup(x => x.DeletePartnerDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            pricingPartnerRepository.Setup(x => x.DeletePartnerBasicDataDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            partnerDal.Setup(x => x.DeletePartnerBasicDataDb(It.IsAny<long>(), It.IsAny<long>())).Returns(true);
            groupSettingsManager.Setup(x => x.GetEpgFeatureVersion(It.IsAny<int>())).Returns(ApiObjects.EpgFeatureVersion.V1);
            var manager = new PartnerManager(partnerDal.Object,
                                            rabbitConnection.Object,
                                            applicationConfiguration.Object,
                                            userManager.Object,
                                            rabbitConfigDal.Object,
                                            pricingPartnerRepository.Object,
                                            Mock.Of<ICatalogManager>(),
                                            userPartnerRepository.Object,
                                            billingPartnerRepository.Object,
                                            caPartnerRepository.Object,
                                            groupSettingsManager.Object,
                                            Mock.Of<IIndexManagerFactory>());

            var response = manager.Delete(fixture.Create<long>(), fixture.Create<int>());

            Assert.That(response.Code, Is.EqualTo((int)eResponseStatus.OK));
            Verify(partnerDal);
            Verify(userManager);
            Verify(rabbitConfigDal);
            Verify(applicationConfiguration);
            Verify(rabbitConnection);
            Verify(pricingPartnerRepository);
            Verify(userPartnerRepository);
            Verify(billingPartnerRepository);
            Verify(caPartnerRepository);
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