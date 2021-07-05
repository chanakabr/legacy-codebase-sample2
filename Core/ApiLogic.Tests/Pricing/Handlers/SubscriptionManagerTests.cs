using ApiLogic.Api.Managers;
using ApiLogic.Pricing.Handlers;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using AutoFixture;
using Core.Pricing;
using DAL;
using Moq;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

namespace ApiLogic.Tests.Pricing
{
    [TestFixture]
    public class SubscriptionManagerTests
    {
        [TestCaseSource(nameof(DeleteCases))]
        public void CheckDelete(eResponseStatus expectedCode, int deleteSubscription, List<SubscriptionInternal> subscriptionInternalList, long expectedId)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<ISubscriptionManagerRepository>();
            var pricingCache = new Mock<IPricingCache>();
            var moduleManagerRepository = new Mock<IModuleManagerRepository>();
            repositoryMock.Setup(x => x.IsSubscriptionExists(It.IsAny<int>(), It.IsAny<long>())).Returns(true);

            repositoryMock.Setup(x => x.DeleteSubscription(It.IsAny<int>(), It.IsAny<long>()))
                                    .Returns(deleteSubscription);

            var priceMock = new Mock<IPrice>();
            var generalPartnerConfigManagerMock = new Mock<IGeneralPartnerConfigManager>();


            SubscriptionManager manager = new SubscriptionManager(pricingCache.Object, repositoryMock.Object, moduleManagerRepository.Object);

            var response = manager.Delete(fixture.Create<ContextData>(), expectedId);

            Assert.That(response.Code, Is.EqualTo((int)expectedCode));
        }


        [TestCaseSource(nameof(InsertCases))]
        public void CheckInsert(eResponseStatus expectedCode, int InsertId)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<ISubscriptionManagerRepository>();
            var pricingCache = new Mock<IPricingCache>();
            var moduleManagerRepository = new Mock<IModuleManagerRepository>();


            repositoryMock.Setup(x => x.AddSubscription(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<SubscriptionInternal>(), It.IsAny<int?>(), It.IsAny<int?>()
                , It.IsAny<bool>())).Returns(InsertId);

            SubscriptionManager manager = new SubscriptionManager(pricingCache.Object, repositoryMock.Object, moduleManagerRepository.Object);

            var response = manager.Add(fixture.Create<ContextData>(), fixture.Create<SubscriptionInternal>());

            Assert.That(response.Status.Code, Is.EqualTo((int)expectedCode));
        }


        private static IEnumerable DeleteCases()
        {
            Fixture fixture = new Fixture();
            SubscriptionInternal subscriptionInternal = fixture.Create<SubscriptionInternal>();
            List<SubscriptionInternal> subscriptionInternalList = new List<SubscriptionInternal>();
            subscriptionInternalList.Add(subscriptionInternal);

            yield return new TestCaseData(eResponseStatus.OK, 1, subscriptionInternalList, subscriptionInternalList[0].Id).SetName("CheckDeleteSuccess");
            yield return new TestCaseData(eResponseStatus.Error, 0, subscriptionInternalList, subscriptionInternalList[0].Id).SetName("CheckDeleteFailed");
        }

        private static IEnumerable InsertCases()
        {
            yield return new TestCaseData(eResponseStatus.Error, 0).SetName("CheckInsertFailed");
            yield return new TestCaseData(eResponseStatus.OK, 12).SetName("CheckInsertSuccess");
        }
    }
}
