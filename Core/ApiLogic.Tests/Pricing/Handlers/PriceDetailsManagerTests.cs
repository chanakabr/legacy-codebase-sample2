using ApiLogic.Api.Managers;
using ApiLogic.Catalog;
using ApiLogic.Pricing.Handlers;
using ApiObjects.Base;
using ApiObjects.Catalog;
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
    public class PriceDetailsManagerTests
    {
        [TestCaseSource(nameof(DeleteCases))]
        public void CheckDelete(eResponseStatus expectedCode, bool deletePriceDetails, List<PriceDetails> priceDetailsList, long expectedId)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IPriceDetailsRepository>();
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(priceDetailsList, true, false);

            repositoryMock.Setup(x => x.IsPriceCodeExistsById(It.IsAny<int>(), It.IsAny<long>())).Returns(true);

            repositoryMock.Setup(x => x.DeletePriceDetails(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
                                    .Returns(deletePriceDetails);

            var priceMock = new Mock<IPrice>();
            var generalPartnerConfigManagerMock = new Mock<IGeneralPartnerConfigManager>();


            PriceDetailsManager manager = new PriceDetailsManager(repositoryMock.Object, generalPartnerConfigManagerMock.Object, priceMock.Object, layeredCacheMock.Object);

            var response = manager.Delete(fixture.Create<ContextData>(), expectedId);

            Assert.That(response.Code, Is.EqualTo((int)expectedCode));
        }

        [TestCaseSource(nameof(InsertCases))]
        public void CheckInsert(eResponseStatus expectedCode, int InsertId)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IPriceDetailsRepository>();
            var layeredCacheMock = new Mock<ILayeredCache>();
            var priceMock = new Mock<IPrice>();
            var generalPartnerConfigManagerMock = new Mock<IGeneralPartnerConfigManager>();
            generalPartnerConfigManagerMock.Setup( x => x.IsValidCurrencyCode(It.IsAny<int>(), It.IsAny<string>())).Returns(true);

            var priceDto = new PriceDTO() { CountryId= 1, Price = 3, Currency = new CurrencyDTO() { CurrencyId = 2 } };            

            repositoryMock.Setup(x => x.InsertPriceDetails(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<long>(), 
                new List<PriceDTO>() { priceDto }, It.IsAny<long>())).Returns(InsertId);

            priceMock.Setup(x => x.InitializeByCD3(It.IsAny<string>(), It.IsAny<double>())).Returns(
               new Price { countryId = 1, m_dPrice = 3, m_oCurrency = new Currency() });

            PriceDetailsManager manager = new PriceDetailsManager(repositoryMock.Object, generalPartnerConfigManagerMock.Object, priceMock.Object, layeredCacheMock.Object);          

            var response = manager.Add(fixture.Create<ContextData>(), fixture.Create<PriceDetails>());

            Assert.That(response.Status.Code, Is.EqualTo((int)expectedCode));
        }


        private static IEnumerable DeleteCases()
        {
            Fixture fixture = new Fixture();
            PriceDetails priceDetails = fixture.Create<PriceDetails>();
            List<PriceDetails> priceDetailsList = new List<PriceDetails>();
            priceDetailsList.Add(priceDetails);

            yield return new TestCaseData(eResponseStatus.OK, true, priceDetailsList, priceDetailsList[0].Id).SetName("CheckDeleteSuccess");
            yield return new TestCaseData(eResponseStatus.Error, false, priceDetailsList, priceDetailsList[0].Id).SetName("CheckDeleteFailed");
        }

        private static IEnumerable InsertCases()
        {
            yield return new TestCaseData(eResponseStatus.Error, 0).SetName("CheckInsertFailed");
            //yield return new TestCaseData(eResponseStatus.OK, 12).SetName("CheckInsertSuccess");
        }
    }
}
