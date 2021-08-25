/*using ApiLogic.Pricing.Handlers;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using AutoFixture;
using CachingProvider.LayeredCache;
using Core.Pricing;
using DAL;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Tvinci.Core.DAL;
using System.Linq;

namespace ApiLogic.Tests.Pricing.Handlers
{
    [TestFixture]
    public class PartnerPremiumServicesManagerTest
    {
        private static readonly Fixture fixture = new Fixture();

        [TestCaseSource(nameof(UpdateCases))]
        public void CheckUpdate(UpdateTestCase updateTestCase)
        {
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(updateTestCase.OldPartnerPremiumServices, true, updateTestCase.NeedToUpdate);
            layeredCacheMock.SetupMock(updateTestCase.AllServices);

            var serviceRepository = new Mock<IServiceRepository>();
            serviceRepository.Setup(x => x.UpdateGroupServices(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<PartnerPremiumServices>())).Returns(updateTestCase.SuccessToUpdate);

            var manager = new PartnerPremiumServicesManager(Mock.Of<IServiceRepository>(), layeredCacheMock.Object);
            
            var response = manager.Update(fixture.Create<ContextData>(), updateTestCase.PartnerPremiumServices);

            Assert.That(response.Status.Code, Is.EqualTo((int)updateTestCase.ResponseStatus));
            layeredCacheMock.Verify(foo => foo.SetInvalidationKey(It.IsAny<string>(), It.IsAny<DateTime?>()), updateTestCase.NeedToUpdate ? Times.Once() : Times.Never());
        }

        private static IEnumerable UpdateCases()
        {
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK)).SetName("update_OK_full");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.Error, successToUpdate: false)).SetName("update_Error");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.ServiceDoesNotExist, servicesExist: false)).SetName("update_ServiceDoesNotExist");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK, needToUpdate: false)).SetName("update_OK_Not_Need_To_Update");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK, servicesForUpdate: false)).SetName("update_OK_no_services_for_update");
        }

        public class UpdateTestCase
        {
            public PartnerPremiumServices PartnerPremiumServices { get; private set; }
            public eResponseStatus ResponseStatus { get; private set; }
            public PartnerPremiumServices OldPartnerPremiumServices { get; private set; }
            public bool NeedToUpdate { get; private set; }
            public Dictionary<int, string> AllServices { get; private set; }
            public bool SuccessToUpdate { get; private set; }

            public UpdateTestCase(eResponseStatus responseStatus, bool successToUpdate = true, bool servicesExist = true, bool needToUpdate = true, bool servicesForUpdate = true)
            {
                ResponseStatus = responseStatus;
                PartnerPremiumServices = fixture.Create<PartnerPremiumServices>();
                if (!servicesForUpdate)
                {
                    PartnerPremiumServices.Services = null;
                }

                NeedToUpdate = needToUpdate;
                if (NeedToUpdate)
                {
                    OldPartnerPremiumServices = fixture.Create<PartnerPremiumServices>();
                }
                else
                {
                    OldPartnerPremiumServices = PartnerPremiumServices;
                }
                
                AllServices = fixture.Create<Dictionary<int, string>>();

                if (servicesExist && servicesForUpdate)
                {
                    var rand = new Random();
                    var allServicesList = AllServices.Keys.ToList();
                    foreach (var service in PartnerPremiumServices.Services)
                    {
                        if (!AllServices.ContainsKey(service.Id))
                        {
                            var index = rand.Next(allServicesList.Count);
                            service.Id = allServicesList[index];
                        }
                    }
                }
                
                SuccessToUpdate = successToUpdate;
            }
        }
    }
}
*/