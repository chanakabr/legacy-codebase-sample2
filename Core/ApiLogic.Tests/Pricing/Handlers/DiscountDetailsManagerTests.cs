using ApiLogic.Pricing.Handlers;
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

namespace ApiLogic.Tests.Pricing
{
    [TestFixture]
    public class DiscountDetailsManagerTests
    {
        [TestCaseSource(nameof(DeleteCases))]
        public void CheckDelete(eResponseStatus expectedCode, bool deleteDiscountDetails, List<DiscountDetails> discountDetailsList, long expectedId)
        {
            var fixture = new Fixture();

            var repositoryMock = new Mock<IDiscountDetailsRepository>();
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(discountDetailsList, true, false);

            repositoryMock.Setup(x => x.DeleteDiscountDetails(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
                                    .Returns(deleteDiscountDetails);

            var priceMock = new Mock<IPrice>();

            var manager = new DiscountDetailsManager(repositoryMock.Object, priceMock.Object, layeredCacheMock.Object);

            var response = manager.Delete(fixture.Create<ContextData>(), expectedId);

            Assert.That(response.Code, Is.EqualTo((int)expectedCode));
        }

        [TestCaseSource(nameof(InsertCases))]
        public void CheckInsert(eResponseStatus expectedCode, int insertId)
        {
            var fixture = new Fixture();

            var repositoryMock = new Mock<IDiscountDetailsRepository>();
            var priceMock = new Mock<IPrice>();
            var layeredCacheMock = new Mock<ILayeredCache>();

            repositoryMock.Setup(x => x.InsertDiscountDetails(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(),
                It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<long>(), It.IsAny<List<DiscountDTO>>(), 
                It.IsAny<WhenAlgoType>(), It.IsAny<int>())).Returns(insertId);

            priceMock.Setup(x => x.InitializeByCD3(It.IsAny<string>(), It.IsAny<double>())).Returns(
                new Price { countryId = 1, m_dPrice = 3, m_oCurrency = new Currency() });

            var manager = new DiscountDetailsManager(repositoryMock.Object, priceMock.Object, layeredCacheMock.Object);

            var response = manager.Add(fixture.Create<ContextData>(), fixture.Create<DiscountDetails>());

            Assert.That(response.Status.Code, Is.EqualTo((int)expectedCode));
        }

        private static IEnumerable InsertCases()
        {
            yield return new TestCaseData(eResponseStatus.Error, 0).SetName("CheckInsertFailed");
            yield return new TestCaseData(eResponseStatus.OK, 12).SetName("CheckInsertSuccess");
        }

        private static IEnumerable DeleteCases()
        {
            Fixture fixture = new Fixture();
            DiscountDetails discountDetails = fixture.Create<DiscountDetails>();
            List<DiscountDetails> discountDetailsList = new List<DiscountDetails>();
            discountDetailsList.Add(discountDetails);

            yield return new TestCaseData(eResponseStatus.OK, true, discountDetailsList, discountDetailsList[0].Id).SetName("CheckDeleteSuccess");
            yield return new TestCaseData(eResponseStatus.DiscountCodeNotExist, false, discountDetailsList, discountDetailsList[0].Id + 1).SetName("CheckDeleteCodeNotExist");
            yield return new TestCaseData(eResponseStatus.Error, false, discountDetailsList, discountDetailsList[0].Id).SetName("CheckDeleteFailed");
        }
    }
}
