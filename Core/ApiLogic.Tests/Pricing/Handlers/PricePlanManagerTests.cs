using ApiLogic.Pricing.Handlers;
using ApiObjects.Base;
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

namespace ApiLogic.Tests.Pricing
{
    [TestFixture]
    public class PricePlanManagerTests
    {
        [TestCaseSource(nameof(DeleteCases))]
        public void CheckDelete(eResponseStatus expectedCode, bool deleteUsageModule, bool isUsageModuleExist, List<UsageModule> usageModuleList, long expectedId)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IPricePlanRepository>();
            var priceDetailsRepositoryMock = new Mock<IPriceDetailsRepository>();
            var pricingCacheMock = new Mock<IPricingCache>();
            var priceDetailsManagerMock = new Mock<IPriceDetailsManager>();
            var pricingUtilsMock = new Mock<IPricingUtils>();
            var moduleManagerRepositoryMock = new Mock<IModuleManagerRepository>();

            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(usageModuleList, true, false);

            moduleManagerRepositoryMock.Setup(x => x.IsUsageModuleExistsById(It.IsAny<int>(), It.IsAny<long>()))
                                     .Returns(isUsageModuleExist);

            repositoryMock.Setup(x => x.DeletePricePlan(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
                                   .Returns(deleteUsageModule);

            PricePlanManager manager = new PricePlanManager(repositoryMock.Object, priceDetailsRepositoryMock.Object, pricingCacheMock.Object, layeredCacheMock.Object,
                           priceDetailsManagerMock.Object, pricingUtilsMock.Object, moduleManagerRepositoryMock.Object);

            var response = manager.Delete(fixture.Create<ContextData>(), expectedId);

            Assert.That(response.Code, Is.EqualTo((int)expectedCode));
        }

        [TestCaseSource(nameof(InsertCases))]
        public void CheckInsert(eResponseStatus expectedCode, int insertId)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IPricePlanRepository>();
            var priceDetailsRepositoryMock = new Mock<IPriceDetailsRepository>();
            var pricingCacheMock = new Mock<IPricingCache>();
            var layeredCacheMock = new Mock<ILayeredCache>();
            var priceDetailsManagerMock = new Mock<IPriceDetailsManager>();
            var pricingUtilsMock = new Mock<IPricingUtils>();
            var moduleManagerRepositoryMock = new Mock<IModuleManagerRepository>();

            priceDetailsManagerMock.Setup(x => x.IsPriceCodeExist(It.IsAny<int>(), It.IsAny<long>())).Returns(true);
            pricingUtilsMock.Setup(x => x.GetMinPeriodDescription(It.IsAny<int>())).Returns("test");

            repositoryMock.Setup(x => x.InsertPricePlan(It.IsAny<int>(), It.IsAny<ApiObjects.IngestPricePlan>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<int>())).Returns(insertId);

            PricePlanManager manager = new PricePlanManager(repositoryMock.Object, priceDetailsRepositoryMock.Object, pricingCacheMock.Object, layeredCacheMock.Object,
                priceDetailsManagerMock.Object, pricingUtilsMock.Object, moduleManagerRepositoryMock.Object);

            var response = manager.Add(fixture.Create<ContextData>(), fixture.Create<UsageModule>());

            Assert.That(response.Status.Code, Is.EqualTo((int)expectedCode));
        }


        private static IEnumerable DeleteCases()
        {
            Fixture fixture = new Fixture();
            UsageModule usageModule = fixture.Create<UsageModule>();
            List<UsageModule> usageModuleList = new List<UsageModule>();
            usageModuleList.Add(usageModule);

            yield return new TestCaseData(eResponseStatus.OK, true, true, usageModuleList, usageModuleList[0].m_pricing_id).SetName("CheckDeleteSuccess");
            yield return new TestCaseData(eResponseStatus.PricePlanDoesNotExist, false, false, usageModuleList, usageModuleList[0].m_pricing_id + 1).SetName("CheckDeleteCodeNotExist");
            yield return new TestCaseData(eResponseStatus.Error, false, true, usageModuleList, usageModuleList[0].m_pricing_id).SetName("CheckDeleteFailed");
        }

        private static IEnumerable InsertCases()
        {
            yield return new TestCaseData(eResponseStatus.Error, 0).SetName("CheckInsertFailed");
            yield return new TestCaseData(eResponseStatus.OK, 12).SetName("CheckInsertSuccess");
        }
    }
}
