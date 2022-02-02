using ApiLogic.Api.Managers;
using ApiLogic.Pricing.Handlers;
using ApiObjects;
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.GroupManagers;

namespace ApiLogic.Tests.Pricing
{
    [TestFixture]
    public class DiscountDetailsManagerTests
    {
        [TestCaseSource(nameof(DeleteCases))]
        public void CheckDelete(DeleteTestCase deleteTestCase)
        {
            var fixture = new Fixture();

            var repositoryMock = new Mock<IDiscountDetailsRepository>();
            var generalPartnerConfigManagerMock = new Mock<IGeneralPartnerConfigManager>();
            var countryManageMock = new Mock<ICountryManager>();
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(deleteTestCase.DiscountDetailsList, true, false);
            var groupSettingsManagerMock = new Mock<IGroupSettingsManager>();
            groupSettingsManagerMock.Setup(x => x.IsOpc(It.IsAny<int>())).Returns(deleteTestCase.IsOPC);
            repositoryMock.Setup(x => x.DeleteDiscountDetails(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
                                    .Returns(deleteTestCase.IsDeleted);
            var manager = new DiscountDetailsManager(repositoryMock.Object, layeredCacheMock.Object, generalPartnerConfigManagerMock.Object, countryManageMock.Object, groupSettingsManagerMock.Object);
            var response = manager.Delete(fixture.Create<ContextData>(), deleteTestCase.IdToDelete);

            Assert.That(response.Code, Is.EqualTo((int)deleteTestCase.ResponseStatus));
        }

        private static IEnumerable DeleteCases()
        {
            Fixture fixture = new Fixture();
            DiscountDetails discountDetails = fixture.Create<DiscountDetails>();
            List<DiscountDetails> discountDetailsList = new List<DiscountDetails>();
            discountDetailsList.Add(discountDetails);

            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.OK)).SetName("Delete_CheckSuccess");
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.AccountIsNotOpcSupported, isOPC: false)).SetName("Delete_CheckNotOpcSupported");
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.DiscountCodeNotExist, false, false)).SetName("Delete_CheckCodeNotExist");
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.Error, isDeleted: false)).SetName("Delete_CheckFailed");
        }

        public class DeleteTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal bool IsDeleted { get; private set; }
            internal eResponseStatus ResponseStatus { get; private set; }
            internal List<DiscountDetails> DiscountDetailsList { get; private set; }
            internal long IdToDelete { get; private set; }
            internal bool IsOPC { get; private set; }

            public DeleteTestCase(eResponseStatus responseStatus, bool isDeleted = true, bool idExist = true, bool isOPC = true)
            {
                ResponseStatus = responseStatus;
                IsDeleted = isDeleted;
                IsOPC = isOPC;
                DiscountDetailsList = fixture.Create<List<DiscountDetails>>();

                if (idExist)
                {
                    IdToDelete = DiscountDetailsList[0].Id;
                }
                else
                {
                    IdToDelete = DiscountDetailsList[0].Id + 1;
                }
            }
        }

        [TestCaseSource(nameof(InsertCases))]
        public void CheckInsert(InsertTestCase insertTestCase)
        {
            var fixture = new Fixture();

            var repositoryMock = new Mock<IDiscountDetailsRepository>();
            var generalPartnerConfigManagerMock = new Mock<IGeneralPartnerConfigManager>();
            var countryManageMock = new Mock<ICountryManager>();
            var layeredCacheMock = new Mock<ILayeredCache>();
            var groupSettingsManagerMock = new Mock<IGroupSettingsManager>();
            groupSettingsManagerMock.Setup(x => x.IsOpc(It.IsAny<int>())).Returns(insertTestCase.IsOPC);
            countryManageMock.Setup(x => x.GetCountryMapById(It.IsAny<int>())).Returns(insertTestCase.Countries);
            generalPartnerConfigManagerMock.Setup(x => x.GetCurrencyMapByCode3(It.IsAny<int>())).Returns(insertTestCase.Currencies);
            repositoryMock.Setup(x => x.InsertDiscountDetails(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<DiscountDetailsDTO>())).Returns(insertTestCase.Id);
            var manager = new DiscountDetailsManager(repositoryMock.Object, layeredCacheMock.Object, generalPartnerConfigManagerMock.Object, countryManageMock.Object, groupSettingsManagerMock.Object);
            var response = manager.Add(fixture.Create<ContextData>(), insertTestCase.DiscountDetails);

            Assert.That(response.Status.Code, Is.EqualTo((int)insertTestCase.ResponseStatus));
        }

        private static IEnumerable InsertCases()
        {
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.Error, isSuccess: false)).SetName("Insert_CheckFailed");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.InvalidCurrency, isCurrencyValid: false)).SetName("Insert_CheckInvalidCurrency");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.CountryNotFound, isCountryExist: false)).SetName("Insert_CheckCountryNotFound");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.OK)).SetName("Insert_CheckSuccess");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.AccountIsNotOpcSupported, isOPC: false)).SetName("Insert_CheckNotOpcSupported");
        }

        public class InsertTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal eResponseStatus ResponseStatus { get; private set; }
            internal DiscountDetails DiscountDetails { get; private set; }
            internal long Id { get; private set; }
            internal Dictionary<int, Country> Countries { get; private set; }
            internal Dictionary<string, Currency> Currencies { get; private set; }
            internal bool IsOPC { get; private set; }

            public InsertTestCase(eResponseStatus responseStatus, bool isSuccess = true, bool isCurrencyValid = true, bool isCountryExist = true, bool isOPC = true)
            {
                ResponseStatus = responseStatus;
                IsOPC = isOPC;
                DiscountDetails = fixture.Create<DiscountDetails>();
                Countries = fixture.Create<Dictionary<int, Country>>();
                Currencies = fixture.Create<Dictionary<string, Currency>>();
                Id = DiscountDetails.Id;

                DiscountDetails.MultiCurrencyDiscounts.ForEach(cd =>
                {
                    cd.m_oCurrency.m_sCurrencyCD3 = Currencies.First().Key;
                    cd.countryId = Countries.First().Key;
                });

                if (!isSuccess)
                {
                    Id = 0;
                }
                if (!isCurrencyValid)
                {
                    Currencies.Remove(DiscountDetails.MultiCurrencyDiscounts[0].m_oCurrency.m_sCurrencyCD3);
                }
                if (!isCountryExist)
                {
                    Countries.Remove(DiscountDetails.MultiCurrencyDiscounts[0].countryId);
                }
            }
        }

        [TestCaseSource(nameof(UpdateCases))]
        public void CheckUpdate(UpdateTestCase updateTestCase)
        {
            var fixture = new Fixture();
            var repositoryMock = new Mock<IDiscountDetailsRepository>();
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(updateTestCase.DiscountDetailsList, true, false);
            var generalPartnerConfigManagerMock = new Mock<IGeneralPartnerConfigManager>();
            var countryManageMock = new Mock<ICountryManager>();
            var groupSettingsManagerMock = new Mock<IGroupSettingsManager>();
            groupSettingsManagerMock.Setup(x => x.IsOpc(It.IsAny<int>())).Returns(updateTestCase.IsOPC);
            generalPartnerConfigManagerMock.Setup(x => x.GetCurrencyMapByCode3(It.IsAny<int>())).Returns(updateTestCase.Currencies);
            countryManageMock.Setup(x => x.GetCountryMapById(It.IsAny<int>())).Returns(updateTestCase.Countries);
            repositoryMock.Setup(x => x.UpdateDiscountDetails(It.IsAny<long>(), It.IsAny<int>(), 
                It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<bool>(),It.IsAny<DiscountDetailsDTO>())).Returns(updateTestCase.UpdatedRow);
            var manager = new DiscountDetailsManager(repositoryMock.Object, layeredCacheMock.Object, generalPartnerConfigManagerMock.Object, countryManageMock.Object, groupSettingsManagerMock.Object);
            var response = manager.Update(fixture.Create<ContextData>(), updateTestCase.Id, updateTestCase.DiscountDetails);

            Assert.That(response.Status.Code, Is.EqualTo((int)updateTestCase.ResponseStatus));
        }

        private static IEnumerable UpdateCases()
        {
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.DiscountCodeNotExist, isDiscountExist: false)).SetName("Update_CheckDiscountCodeNotExist");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.Error, isSuccess: false)).SetName("Update_CheckUpdateFailed");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.InvalidCurrency, isCurrencyValid: false)).SetName("Update_CheckInvalidCurrency");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.CountryNotFound, isCountryExist: false)).SetName("Update_CheckCountryNotFound");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK)).SetName("Update_CheckSuccess");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.AccountIsNotOpcSupported, isOPC: false)).SetName("Update_CheckNotOpcSupported");

        }

        public class UpdateTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal eResponseStatus ResponseStatus { get; private set; }
            internal DiscountDetails DiscountDetails { get; private set; }
            internal List<DiscountDetails> DiscountDetailsList { get; private set; }
            internal long Id { get; private set; }
            internal long UpdatedRow { get; private set; }
            internal Dictionary<int, Country> Countries { get; private set; }
            internal Dictionary<string, Currency> Currencies { get; private set; }
            internal bool IsOPC { get; private set; }

            public UpdateTestCase(eResponseStatus responseStatus, bool isSuccess = true, bool isCurrencyValid = true, bool isCountryExist = true, bool isDiscountExist = true, bool isOPC = true)
            {
                ResponseStatus = responseStatus;
                IsOPC = isOPC;
                DiscountDetailsList = fixture.Create<List<DiscountDetails>>();
                DiscountDetails = fixture.Create<DiscountDetails>();
                Countries = fixture.Create<Dictionary<int, Country>>();
                Currencies = fixture.Create<Dictionary<string, Currency>>();
                Id = DiscountDetailsList[0].Id;
                UpdatedRow = 1;
                DiscountDetails.MultiCurrencyDiscounts.ForEach(cd =>
                {
                    cd.m_oCurrency.m_sCurrencyCD3 = Currencies.First().Key;
                    cd.countryId = Countries.First().Key;
                });

                if (!isSuccess)
                {
                    UpdatedRow = 0;
                }
                if (!isCurrencyValid)
                {
                    Currencies.Remove(DiscountDetails.MultiCurrencyDiscounts[0].m_oCurrency.m_sCurrencyCD3);
                }
                if (!isCountryExist)
                {
                    Countries.Remove(DiscountDetails.MultiCurrencyDiscounts[0].countryId);
                }
                if (!isDiscountExist)
                {
                    DiscountDetailsList = DiscountDetailsList.Skip(1).ToList();
                }
            }
        }

        [TestCaseSource(nameof(ListCases))]
        public void CheckList(ListTestCase listTestCase)
        {
            var fixture = new Fixture();

            var repositoryMock = new Mock<IDiscountDetailsRepository>();
            var generalPartnerConfigManagerMock = new Mock<IGeneralPartnerConfigManager>();
            var countryManageMock = new Mock<ICountryManager>();
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(listTestCase.DiscountDetailsList, true, false);

            var manager = new DiscountDetailsManager(repositoryMock.Object, layeredCacheMock.Object, generalPartnerConfigManagerMock.Object, countryManageMock.Object, Mock.Of<IGroupSettingsManager>());
            var response = manager.GetDiscounts(fixture.Create<int>(), listTestCase.Filter, listTestCase.Currency);

            Assert.That(response.Objects.Count == listTestCase.ReturnListSize);
            Assert.That(response.Status.Code, Is.EqualTo((int)listTestCase.ResponseStatus));
        }

        private static IEnumerable ListCases()
        {
            yield return new TestCaseData(new ListTestCase(eResponseStatus.OK)).SetName("List_CheckSuccess");
            yield return new TestCaseData(new ListTestCase(eResponseStatus.OK, isDiscountDetailsExists: false)).SetName("List_CheckEmpty");
            yield return new TestCaseData(new ListTestCase(eResponseStatus.OK, isFilterExists: true, IsFilterEmpty: true)).SetName("List_CheckFilterEmpty");
            yield return new TestCaseData(new ListTestCase(eResponseStatus.OK, isFilterExists: true)).SetName("List_CheckFilterWithExistsIds");
            yield return new TestCaseData(new ListTestCase(eResponseStatus.OK, isFilterExists: true, isFilterContainsExistsId:false, isFilterContainsNonExistentId:true)).SetName("List_CheckFilterWithNonExistentIds");
            yield return new TestCaseData(new ListTestCase(eResponseStatus.OK, isFilterExists: true, isFilterContainsNonExistentId:true)).SetName("List_CheckFilterWithExistentIdsAndNonExistentIds");
            yield return new TestCaseData(new ListTestCase(eResponseStatus.InvalidCurrency, isDiscountDetailsExists:false, isCountryExists: false)).SetName("List_CheckInvalidCurrency");

        }

        public class ListTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal eResponseStatus ResponseStatus { get; private set; }
            internal List<DiscountDetails> DiscountDetailsList { get; private set; }
            internal List<long> Filter { get; private set; }
            internal string Currency { get; private set; }
            internal int ReturnListSize { get; private set; }

            public ListTestCase(eResponseStatus responseStatus, bool isDiscountDetailsExists = true, bool isFilterExists = false, bool IsFilterEmpty = false,
                bool isFilterContainsExistsId = true, bool isFilterContainsNonExistentId = false, bool isCountryExists = true)
            {
                ResponseStatus = responseStatus;
                DiscountDetailsList = fixture.Create<List<DiscountDetails>>();
                Currency = "*";
                if (!isDiscountDetailsExists)
                {
                    DiscountDetailsList = new List<DiscountDetails>();
                }
                ReturnListSize = DiscountDetailsList.Count;
                if (!isCountryExists)
                {
                    Currency = fixture.Create<string>();
                }
                if (isFilterExists)
                {
                    Filter = new List<long>();
                    if (!IsFilterEmpty)
                    {
                        ReturnListSize = 0;
                        if (isFilterContainsExistsId)
                        {
                            Filter.Add(DiscountDetailsList[0].Id);
                            ReturnListSize = Filter.Count();
                        }
                        if (isFilterContainsNonExistentId)
                        {
                            var discountDetailsNotExistsInFilter = DiscountDetailsList.Where(x => !Filter.Contains(x.Id)).First();
                            Filter.Add(discountDetailsNotExistsInFilter.Id);
                            DiscountDetailsList.Remove(discountDetailsNotExistsInFilter);
                        }
                    }
                }
            }
        }
    }
}