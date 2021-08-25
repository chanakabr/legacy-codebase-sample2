using ApiLogic.Api.Managers;
using ApiLogic.Pricing.Handlers;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using AutoFixture;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Pricing;
using DAL;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.GroupManagers;

namespace ApiLogic.Tests.Pricing
{
    [TestFixture]
    public class PriceDetailsManagerTests
    {
        [TestCaseSource(nameof(DeleteCases))]
        public void CheckDelete(eResponseStatus expectedCode, bool deletePriceDetails, List<PriceDetails> priceDetailsList, long expectedId, bool isOPC)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IPriceDetailsRepository>();
            repositoryMock.Setup(x => x.DeletePriceDetails(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
                                    .Returns(deletePriceDetails);
            var groupSettingsManagerMock = new Mock<IGroupSettingsManager>();
            groupSettingsManagerMock.Setup(x => x.IsOpc(It.IsAny<int>())).Returns(isOPC);

            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(priceDetailsList, true, false);

            PriceDetailsManager manager = new PriceDetailsManager(repositoryMock.Object, 
                                                                  Mock.Of<IGeneralPartnerConfigManager>(), 
                                                                  layeredCacheMock.Object, 
                                                                  Mock.Of<ICountryManager>(),
                                                                  groupSettingsManagerMock.Object);
            var response = manager.Delete(fixture.Create<ContextData>(), expectedId);
            Assert.That(response.Code, Is.EqualTo((int)expectedCode));
        }

        private static IEnumerable DeleteCases()
        {
            Fixture fixture = new Fixture();
            var priceDetails = fixture.Create<PriceDetails>();
            var priceDetailsList = fixture.Create<List<PriceDetails>>();
            priceDetailsList.Add(priceDetails);
            var idNotExist = fixture.Create<long>();
            priceDetailsList.RemoveAll(x => x.Id == idNotExist);

            yield return new TestCaseData(eResponseStatus.OK, true, priceDetailsList, priceDetailsList[0].Id, true).SetName("Delete_OK");
            yield return new TestCaseData(eResponseStatus.Error, false, priceDetailsList, priceDetailsList[0].Id,true).SetName("Delete_Error");
            yield return new TestCaseData(eResponseStatus.AccountIsNotOpcSupported, true, priceDetailsList, priceDetailsList[0].Id, false).SetName("Delete_NotOpcSupported");
            yield return new TestCaseData(eResponseStatus.PriceDetailsDoesNotExist, false, priceDetailsList, idNotExist, true).SetName("Delete_PriceDetailsDoesNotExist");
        }

        [TestCaseSource(nameof(AddCases))]
        public void CheckAdd(AddTestCase addTestCase)
        {
            Fixture fixture = new Fixture();

            var generalPartnerConfigManagerMock = new Mock<IGeneralPartnerConfigManager>();
            generalPartnerConfigManagerMock.Setup(x => x.GetCurrencyMapByCode3(It.IsAny<int>())).Returns(addTestCase.CurrencyMap);
            var groupSettingsManagerMock = new Mock<IGroupSettingsManager>();
            groupSettingsManagerMock.Setup(x => x.IsOpc(It.IsAny<int>())).Returns(addTestCase.IsOPC);
            
            var repositoryMock = new Mock<IPriceDetailsRepository>();
            repositoryMock.Setup(x => x.InsertPriceDetails(It.IsAny<int>(), It.IsAny<PriceDetailsDTO>(), It.IsAny<long>()))
                          .Returns(addTestCase.PriceDetails.Id);

            var layeredCacheMock = new Mock<ILayeredCache>();
            layeredCacheMock.Setup(x => x.SetInvalidationKey(It.IsAny<string>(), null)).Returns(true);

            var countryManagerMock = new Mock<ICountryManager>();
            countryManagerMock.Setup(x => x.GetCountryMapById(It.IsAny<int>())).Returns(addTestCase.CountryMap);

            PriceDetailsManager manager = new PriceDetailsManager(repositoryMock.Object, 
                                                                  generalPartnerConfigManagerMock.Object, 
                                                                  layeredCacheMock.Object,
                                                                  countryManagerMock.Object, 
                                                                  groupSettingsManagerMock.Object);          

            var response = manager.Add(fixture.Create<ContextData>(), addTestCase.PriceDetails);
            Assert.That(response.Status.Code, Is.EqualTo(addTestCase.ExpectedCode));
        }

        private static IEnumerable AddCases()
        {
            yield return new TestCaseData(new AddTestCase(eResponseStatus.OK)).SetName("Add_OK");
            yield return new TestCaseData(new AddTestCase(eResponseStatus.AccountIsNotOpcSupported, isOPC: false)).SetName("Add_CheckNotOpcSupported");
            
            var addInvalidCurrency = new AddTestCase(eResponseStatus.InvalidCurrency, setPriceDetailsCurrencies: false);
            addInvalidCurrency.CurrencyMap.Remove(addInvalidCurrency.PriceDetails.Prices[0].m_oCurrency.m_sCurrencyCD3.ToLower());
            yield return new TestCaseData(addInvalidCurrency).SetName("Add_InvalidCurrency");

            var addCountryNotFound = new AddTestCase(eResponseStatus.CountryNotFound, setPriceDetailsCountries: false);
            addCountryNotFound.CountryMap.Remove(addCountryNotFound.PriceDetails.Prices[0].countryId);
            yield return new TestCaseData(addCountryNotFound).SetName("Add_CountryNotFound");

            var addError = new AddTestCase(eResponseStatus.Error);
            addError.PriceDetails.Id = 0;
            yield return new TestCaseData(addError).SetName("Add_Error");
        }

        [TestCaseSource(nameof(UpdateCases))]
        public void CheckUpdate(UpdateTestCase updateTestCase)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IPriceDetailsRepository>();
            repositoryMock.Setup(x => x.UpdatePriceDetails(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<PriceDetailsDTO>(), It.IsAny<bool>(), It.IsAny<long>()))
                          .Returns(updateTestCase.SuccessfulUpdate);
            
            var groupSettingsManagerMock = new Mock<IGroupSettingsManager>();
            groupSettingsManagerMock.Setup(x => x.IsOpc(It.IsAny<int>())).Returns(updateTestCase.IsOPC);
            
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(updateTestCase.PriceDetailsList, true, false);
            layeredCacheMock.Setup(x => x.SetInvalidationKey(It.IsAny<string>(), null)).Returns(true);

            var generalPartnerConfigManagerMock = new Mock<IGeneralPartnerConfigManager>();
            generalPartnerConfigManagerMock.Setup(x => x.GetCurrencyMapByCode3(It.IsAny<int>())).Returns(updateTestCase.CurrencyMap);

            var countryManagerMock = new Mock<ICountryManager>();
            countryManagerMock.Setup(x => x.GetCountryMapById(It.IsAny<int>())).Returns(updateTestCase.CountryMap);

            PriceDetailsManager manager = new PriceDetailsManager(repositoryMock.Object, 
                                                                  generalPartnerConfigManagerMock.Object, 
                                                                  layeredCacheMock.Object,
                                                                  countryManagerMock.Object,
                                                                  groupSettingsManagerMock.Object);

            var response = manager.Update(fixture.Create<ContextData>(), updateTestCase.PriceDetails);
            Assert.That(response.Status.Code, Is.EqualTo(updateTestCase.ExpectedCode));
        }

        private static IEnumerable UpdateCases()
        {
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK)).SetName("Update_OK");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.AccountIsNotOpcSupported, isOPC: false)).SetName("Update_CheckNotOpcSupported");

            var updateInvalidCurrency = new UpdateTestCase(eResponseStatus.InvalidCurrency, setPriceDetailsCurrencies: false);
            updateInvalidCurrency.CurrencyMap.Remove(updateInvalidCurrency.PriceDetails.Prices[0].m_oCurrency.m_sCurrencyCD3.ToLower());
            yield return new TestCaseData(updateInvalidCurrency).SetName("Update_InvalidCurrency");

            var updateCountryNotFound = new UpdateTestCase(eResponseStatus.CountryNotFound, setPriceDetailsCountries: false);
            updateCountryNotFound.CountryMap.Remove(updateInvalidCurrency.PriceDetails.Prices[0].countryId);
            yield return new TestCaseData(updateCountryNotFound).SetName("Update_CountryNotFound");

            var updateError = new UpdateTestCase(eResponseStatus.Error);
            updateError.SuccessfulUpdate = false;
            yield return new TestCaseData(updateError).SetName("Update_Error");

            var updatePriceDetailsDoesNotExist = new UpdateTestCase(eResponseStatus.PriceDetailsDoesNotExist);
            Fixture fixture = new Fixture();
            var priceDetailsId = fixture.Create<long>();
            updatePriceDetailsDoesNotExist.PriceDetails.Id = priceDetailsId;
            updatePriceDetailsDoesNotExist.PriceDetailsList.RemoveAll(x => x.Id == priceDetailsId);
            yield return new TestCaseData(updatePriceDetailsDoesNotExist).SetName("Update_PriceDetailsDoesNotExist");
        }

        public abstract class TestCase
        {
            protected static readonly Fixture fixture = new Fixture();
            public int ExpectedCode { get; private set; }
            public PriceDetails PriceDetails { get; set; }
            public Dictionary<string, Currency> CurrencyMap { get; set; }
            public Dictionary<int, ApiObjects.Country> CountryMap { get; set; }
            internal bool IsOPC { get; private set; }

            public TestCase(eResponseStatus expectedCode, bool setPriceDetailsCurrencies, bool setPriceDetailsCountries, bool isOPC = true)
            {
                PriceDetails = fixture.Create<PriceDetails>();
                ExpectedCode = (int)expectedCode;
                IsOPC = isOPC;
                var currencyMapUpper = fixture.Create<Dictionary<string, Currency>>();
                foreach (var item in currencyMapUpper)
                {
                    item.Value.m_sCurrencyCD3 = item.Key;
                }
                CurrencyMap = currencyMapUpper.ToDictionary(x => x.Key.ToLower(), y => y.Value);
                
                if (setPriceDetailsCurrencies)
                {
                    var rand = new Random();
                    var currencyList = CurrencyMap.Values.ToList();
                    foreach (var price in this.PriceDetails.Prices)
                    {
                        if (!CurrencyMap.ContainsKey(price.m_oCurrency.m_sCurrencyCD3.ToLower()))
                        {
                            var index = rand.Next(CurrencyMap.Count);
                            price.m_oCurrency.m_sCurrencyCD3 = currencyList[index].m_sCurrencyCD3;
                        }
                    }
                }

                CountryMap = fixture.Create<Dictionary<int, ApiObjects.Country>>();
                foreach (var item in CountryMap)
                {
                    item.Value.Id = item.Key;
                }
                if (setPriceDetailsCountries)
                {
                    var rand = new Random();
                    var countryList = CountryMap.Values.ToList();
                    foreach (var price in this.PriceDetails.Prices)
                    {
                        if (!CountryMap.ContainsKey(price.countryId))
                        {
                            var index = rand.Next(CountryMap.Count);
                            price.countryId = countryList[index].Id;
                        }
                    }
                }
            }
        }

        public class AddTestCase : TestCase
        {
            public AddTestCase(eResponseStatus expectedCode, bool setPriceDetailsCurrencies = true, bool setPriceDetailsCountries = true, bool isOPC = true)
                : base(expectedCode, setPriceDetailsCurrencies, setPriceDetailsCountries, isOPC)
            {
            }
        }

        public class UpdateTestCase : TestCase
        {
            public List<PriceDetails> PriceDetailsList { get; set; }
            public bool SuccessfulUpdate { get; set; }

            public UpdateTestCase(eResponseStatus expectedCode, bool setPriceDetailsCurrencies = true, bool setPriceDetailsCountries = true, bool isOPC = true)
                : base(expectedCode, setPriceDetailsCurrencies, setPriceDetailsCountries, isOPC)
            {
                PriceDetailsList = fixture.Create<List<PriceDetails>>();
                PriceDetailsList.Add(PriceDetails);
                SuccessfulUpdate = true;
            }
        }
    }
}
