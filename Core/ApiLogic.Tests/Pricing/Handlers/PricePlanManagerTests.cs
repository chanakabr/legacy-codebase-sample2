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
using Core.GroupManagers;

namespace ApiLogic.Tests.Pricing
{
    [TestFixture]
    public class PricePlanManagerTests
    {
        [TestCaseSource(nameof(DeleteCases))]
        public void CheckDelete(DeleteTestCase deleteTestCase)
        {
            Fixture fixture = new Fixture();
            var repositoryMock = new Mock<IPricePlanRepository>();
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(deleteTestCase.PricePlanList, true, false);
            var priceDetailsManagerMock = new Mock<IPriceDetailsManager>();
            var discountDetailsManagerMock = new Mock<IDiscountDetailsManager>();
            var groupSettingsManagerMock = new Mock<IGroupSettingsManager>();
            groupSettingsManagerMock.Setup(x => x.IsOpc(It.IsAny<int>())).Returns(deleteTestCase.IsOPC);
            repositoryMock.Setup(x => x.DeletePricePlan(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
                                   .Returns(deleteTestCase.DeletePricePlan);
            PricePlanManager manager = new PricePlanManager(repositoryMock.Object, layeredCacheMock.Object, priceDetailsManagerMock.Object, discountDetailsManagerMock.Object, groupSettingsManagerMock.Object);
            var response = manager.Delete(fixture.Create<ContextData>(), deleteTestCase.ExpectedId);

            Assert.That(response.Code, Is.EqualTo((int)deleteTestCase.ResponseStatus));
        }

        private static IEnumerable DeleteCases()
        {
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.OK, true)).SetName("CheckDeleteSuccess");
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.PricePlanDoesNotExist, false, false)).SetName("CheckDeleteCodeNotExist");
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.Error, false)).SetName("CheckDeleteFailed");
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.AccountIsNotOpcSupported, true, isOpc: false)).SetName("CheckNotOpcSupported");

        }

        public class DeleteTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal bool DeletePricePlan { get; private set; }
            internal eResponseStatus ResponseStatus { get; private set; }
            internal List<PricePlan> PricePlanList { get; private set; }
            internal long ExpectedId { get; private set; }
            internal bool IsOPC { get; private set; }
            

            public DeleteTestCase(eResponseStatus responseStatus, bool deletePricePlan, bool idExist = true, bool isOpc = true)
            {
                ResponseStatus = responseStatus;
                DeletePricePlan = deletePricePlan;
                PricePlanList = fixture.Create<List<PricePlan>>();
                ExpectedId = PricePlanList[0].Id.Value;
                IsOPC = isOpc;
                if (!idExist)
                {
                    ExpectedId++;
                }
            }
        }

        [TestCaseSource(nameof(InsertCases))]
        public void CheckInsert(InsertTestCase insertTestCase)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IPricePlanRepository>();
            var discountDetailsManagerMock = new Mock<IDiscountDetailsManager>();
            var priceDetailsManagerMock = new Mock<IPriceDetailsManager>();
            var groupSettingsManagerMock = new Mock<IGroupSettingsManager>();
            groupSettingsManagerMock.Setup(x => x.IsOpc(It.IsAny<int>())).Returns(insertTestCase.IsOPC);
            discountDetailsManagerMock.Setup(x => x.GetDiscountDetailsById(It.IsAny<int>(), It.IsAny<long>())).Returns(insertTestCase.DiscountDetailsResponse);
            
            priceDetailsManagerMock.Setup(x => x.GetPriceDetailsById(It.IsAny<int>(), It.IsAny<long>())).Returns(insertTestCase.PriceCode);
            repositoryMock.Setup(x => x.InsertPricePlan(It.IsAny<int>(), It.IsAny<PricePlan>(), It.IsAny<long>())).Returns(insertTestCase.InsertId);
            PricePlanManager manager = new PricePlanManager(repositoryMock.Object,  Mock.Of<ILayeredCache>(), priceDetailsManagerMock.Object, discountDetailsManagerMock.Object, groupSettingsManagerMock.Object);
            var response = manager.Add(fixture.Create<ContextData>(), fixture.Create<PricePlan>());

            Assert.That(response.Status.Code, Is.EqualTo((int)insertTestCase.ResponseStatus));
        }

        private static IEnumerable InsertCases()
        {
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.PriceDetailsDoesNotExist, isPriceCodeExist: false)).SetName("CheckInsertPriceCodeDoesNotExist");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.DiscountCodeNotExist, eResponseStatus.DiscountCodeNotExist)).SetName("CheckInsertDiscountCodeNotExist");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.Error)).SetName("CheckInsertFailed");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.OK, isInsertSuccess: true)).SetName("CheckInsertSuccess");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.AccountIsNotOpcSupported, isPriceCodeExist: false, isOPC: false)).SetName("CheckNotOpcSupported");

        }

        public class InsertTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal GenericResponse<PriceDetails> PriceCode { get; private set; }
            internal eResponseStatus ResponseStatus { get; private set; }
            internal GenericResponse<DiscountDetails> DiscountDetailsResponse { get; private set; }
            internal long InsertId { get; private set; }
            internal bool IsOPC { get; private set; }

            public InsertTestCase(eResponseStatus responseStatus, eResponseStatus discountResponseStatus = eResponseStatus.OK, bool isPriceCodeExist = true, bool isInsertSuccess = false, bool isOPC = true)
            {
                Fixture fixture = new Fixture();
                ResponseStatus = responseStatus;
                DiscountDetailsResponse = fixture.Create<GenericResponse<DiscountDetails>>();
                DiscountDetailsResponse.SetStatus(discountResponseStatus);
                InsertId = 0;
                IsOPC = isOPC;
                if(isInsertSuccess)
                {
                    // Need to be bigger than 0
                    InsertId = fixture.Create<int>() + 1;
                }

                if (isPriceCodeExist)
                {
                    var priceDetails = fixture.Create<PriceDetails>();
                    PriceCode = new GenericResponse<PriceDetails>(Status.Ok, priceDetails);
                }
                else
                {
                    PriceCode = new GenericResponse<PriceDetails>(eResponseStatus.PriceDetailsDoesNotExist);
                }
            }
        }

        [TestCaseSource(nameof(UpdateCases))]
        public void CheckUpdate(UpdateTestCase updateTestCase)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IPricePlanRepository>();
            var discountDetailsManagerMock = new Mock<IDiscountDetailsManager>();
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(updateTestCase.PricePlanList, true, false);
            var priceDetailsManagerMock = new Mock<IPriceDetailsManager>();
            var groupSettingsManagerMock = new Mock<IGroupSettingsManager>();
            groupSettingsManagerMock.Setup(x => x.IsOpc(It.IsAny<int>())).Returns(updateTestCase.IsOPC);
            discountDetailsManagerMock.Setup(x => x.GetDiscountDetailsById(It.IsAny<int>(), It.IsAny<long>())).Returns(updateTestCase.DiscountDetailsResponse);
            priceDetailsManagerMock.Setup(x => x.GetPriceDetailsById(It.IsAny<int>(), It.IsAny<long>())).Returns(updateTestCase.PriceCode);
            repositoryMock.Setup(x => x.UpdatePricePlan(It.IsAny<int>(), It.IsAny<PricePlan>(), It.IsAny<long>(), It.IsAny<long>())).Returns((int)updateTestCase.UpdatedRow);
            PricePlanManager manager = new PricePlanManager(repositoryMock.Object, layeredCacheMock.Object, priceDetailsManagerMock.Object, discountDetailsManagerMock.Object, groupSettingsManagerMock.Object);
            var response = manager.Update(fixture.Create<ContextData>(), updateTestCase.Id, updateTestCase.PricePlanToInsert);

            Assert.That(response.Status.Code, Is.EqualTo((int)updateTestCase.ResponseStatus));
            repositoryMock.Verify(x => x.UpdatePricePlan(It.IsAny<int>(), It.IsAny<PricePlan>(), It.IsAny<long>(), It.IsAny<long>()), Times.Exactly(updateTestCase.AmountCallToRepository));

        }

        private static IEnumerable UpdateCases()
        {
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.PricePlanDoesNotExist, isPricePlanExist: false)).SetName("CheckUpdatePricePlanDoesNotExist");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.PriceDetailsDoesNotExist, isPriceCodeExist: false)).SetName("CheckUpdatePriceDetailsDoesNotExist");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.DiscountCodeNotExist, eResponseStatus.DiscountCodeNotExist)).SetName("CheckUpdateDiscountCodeNotExist");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.Error, amountCallToRepository: 1)).SetName("CheckUpdateFailed");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK, isUpdateSuccess: true, amountCallToRepository: 1)).SetName("CheckUpdateSuccess");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK, isUpdateSuccess: true)).SetName("CheckUpdateNotNedded");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.AccountIsNotOpcSupported, isUpdateSuccess: true, isOPC: false)).SetName("CheckNotOpcSupported");

        }

        public class UpdateTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal GenericResponse<PriceDetails> PriceCode { get; private set; }
            internal eResponseStatus ResponseStatus { get; private set; }
            internal GenericResponse<DiscountDetails> DiscountDetailsResponse { get; private set; }
            internal List<PricePlan> PricePlanList { get; private set; }
            internal long UpdatedRow { get; private set; }
            internal int Id { get; private set; }
            internal int AmountCallToRepository { get; private set; }
            internal PricePlan PricePlanToInsert { get; private set; }
            internal bool IsOPC { get; private set; }
            public UpdateTestCase(eResponseStatus responseStatus, eResponseStatus discountResponseStatus = eResponseStatus.OK, 
                                 bool isPriceCodeExist = true, bool isUpdateSuccess = false, bool isPricePlanExist = true, int amountCallToRepository = 0, bool isOPC = true)
            {
                Fixture fixture = new Fixture();
                IsOPC = isOPC;
                ResponseStatus = responseStatus;
                DiscountDetailsResponse = fixture.Create<GenericResponse<DiscountDetails>>();
                DiscountDetailsResponse.SetStatus(discountResponseStatus);
                UpdatedRow = 0;
                PricePlanList = fixture.Create<List<PricePlan>>();
                PricePlanToInsert = PricePlanList[0];
                Id = (int)PricePlanToInsert.Id.Value;
                AmountCallToRepository = amountCallToRepository;
                if (isPriceCodeExist)
                {
                    var priceDetails = fixture.Create<PriceDetails>();
                    PriceCode = new GenericResponse<PriceDetails>(Status.Ok, priceDetails);
                }
                else
                {
                    PriceCode = new GenericResponse<PriceDetails>(eResponseStatus.PriceDetailsDoesNotExist);
                }
                if (isUpdateSuccess)
                {
                    // Need to be bigger than 0
                    UpdatedRow = fixture.Create<int>() + 1;
                }
                if (!isPricePlanExist)
                {
                    Id++;
                }
                if (amountCallToRepository > 0)
                {
                    PricePlanList.Remove(PricePlanToInsert);
                    var pricePlan = fixture.Create<PricePlan>();
                    pricePlan.Id = Id;
                    PricePlanList.Add(pricePlan);
                }
            }
        }

        [TestCaseSource(nameof(ListCases))]
        public void Checklist(eResponseStatus expectedCode, List<PricePlan> pricePlanList, List<long> pricePlanIds, int listCount)
        {
            Fixture fixture = new Fixture();
            var repositoryMock = new Mock<IPricePlanRepository>();
            var discountDetailsManagerMock = new Mock<IDiscountDetailsManager>();
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(pricePlanList, true, false);
            var priceDetailsManagerMock = new Mock<IPriceDetailsManager>();
            var groupSettingsManagerMock = new Mock<IGroupSettingsManager>();
            PricePlanManager manager = new PricePlanManager(repositoryMock.Object, layeredCacheMock.Object, priceDetailsManagerMock.Object, discountDetailsManagerMock.Object, groupSettingsManagerMock.Object);
            var response = manager.GetPricePlans(fixture.Create<int>(),pricePlanIds);

            Assert.That(response.Objects.Count, Is.EqualTo(listCount));
            Assert.That(response.Status.Code, Is.EqualTo((int)expectedCode));
        }

        private static IEnumerable ListCases()
        {
            Fixture fixture = new Fixture();
            List<PricePlan> pricePlanList = fixture.Create<List<PricePlan>>();
            yield return new TestCaseData(eResponseStatus.OK, pricePlanList, new List<long>(), pricePlanList.Count).SetName("ChecListeSuccess");
            List<PricePlan> pricePlanFilterList = fixture.Create<List<PricePlan>>();
            yield return new TestCaseData(eResponseStatus.OK, pricePlanFilterList, new List<long>() { pricePlanFilterList[0].Id.Value }, 1).SetName("ChecListeFilterSuccess");
        }
    }
}