//using ApiLogic.Api.Managers;
//using ApiLogic.Pricing.Handlers;
//using ApiObjects;
//using ApiObjects.Base;
//using ApiObjects.Pricing;
//using ApiObjects.Response;
//using AutoFixture;
//using Core.Api;
//using Core.Catalog;
//using Core.Catalog.CatalogManagement;
//using Core.Domains;
//using Core.Pricing;
//using DAL;
//using Moq;
//using NUnit.Framework;
//using System.Collections;
//using System.Collections.Generic;
//using Tvinci.Core.DAL;

//namespace ApiLogic.Tests.Pricing
//{
//    [TestFixture]
//    public class SubscriptionManagerTests
//    {
//        [TestCaseSource(nameof(DeleteCases))]
//        public void CheckDelete(eResponseStatus expectedCode, int deleteSubscription, List<SubscriptionInternal> subscriptionInternalList, long expectedId)
//        {
//            Fixture fixture = new Fixture();

//            var repositoryMock = new Mock<ISubscriptionManagerRepository>();
//            var channelRepository = new Mock<IChannelRepository>();
//            var moduleManagerRepository = new Mock<IModuleManagerRepository>();
//            var pricingModule = new Mock<IPricingModule>();
//            var fileManager = new Mock<IFileManager>();
//            var domainModule = new Mock<IDomainModule>();
//            var pricePlanManager = new Mock<IPricePlanManager>();
//            var virtualAssetManager = new Mock<IVirtualAssetManager>();
            
//            virtualAssetManager.Setup(x => x.DeleteVirtualAsset(It.IsAny<int>(), It.IsAny<VirtualAssetInfo>()))
//                                 .Returns(new VirtualAssetInfoResponse() { Status = VirtualAssetInfoStatus.OK });

//            repositoryMock.Setup(x => x.IsSubscriptionExists(It.IsAny<int>(), It.IsAny<long>())).Returns(true);

//            repositoryMock.Setup(x => x.DeleteSubscription(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
//                                    .Returns(deleteSubscription);

//            var priceMock = new Mock<IPrice>();
//            var generalPartnerConfigManagerMock = new Mock<IGeneralPartnerConfigManager>();

//            SubscriptionManager manager = new SubscriptionManager(repositoryMock.Object,
//                                                moduleManagerRepository.Object,
//                                                channelRepository.Object,
//                                                pricingModule.Object,
//                                                fileManager.Object,
//                                                domainModule.Object,
//                                                pricePlanManager.Object,
//                                                api.Instance);

//            var response = manager.Delete(fixture.Create<ContextData>(), expectedId);

//            Assert.That(response.Code, Is.EqualTo((int)expectedCode));
//        }

//        [TestCaseSource(nameof(InsertCases))]
//        public void CheckInsert(eResponseStatus expectedCode, int InsertId)
//        {
//            Fixture fixture = new Fixture();

//            var repositoryMock = new Mock<ISubscriptionManagerRepository>();
//            var moduleManagerRepository = new Mock<IModuleManagerRepository>();
//            var channelRepository = new Mock<IChannelRepository>();
//            var pricingModule = new Mock<IPricingModule>();
//            var fileManager = new Mock<IFileManager>();
//            var domainModule = new Mock<IDomainModule>();
//            var pricePlanManager = new Mock<IPricePlanManager>();
//            var virtualAssetManager = new Mock<IVirtualAssetManager>();

//            virtualAssetManager.Setup(x => x.AddVirtualAsset(It.IsAny<int>(), It.IsAny<VirtualAssetInfo>(), null))
//                                  .Returns(new VirtualAssetInfoResponse() { Status = VirtualAssetInfoStatus.OK });

//            fileManager.Setup(x => x.GetMediaFileTypes(It.IsAny<int>())).Returns(new GenericListResponse<MediaFileType>(Status.Ok, new List<MediaFileType>()));

//            channelRepository.Setup(x => x.IsChannelExists(It.IsAny<int>(), It.IsAny<long>())).Returns(true);
//            var couponsGroupResponse = new CouponsGroupResponse() { Status = Status.Ok, CouponsGroup = new CouponsGroup() { m_sGroupCode = "123" } };
//            pricingModule.Setup(x => x.GetCouponsGroup(It.IsAny<int>(), 123)).Returns(couponsGroupResponse);


//            repositoryMock.Setup(x => x.AddSubscription(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<SubscriptionInternal>(), It.IsAny<int?>(), It.IsAny<int?>()
//                , It.IsAny<bool>())).Returns(InsertId);

//            SubscriptionManager manager = new SubscriptionManager(repositoryMock.Object,
//                                               moduleManagerRepository.Object,
//                                               channelRepository.Object,
//                                               pricingModule.Object,
//                                               fileManager.Object,
//                                               domainModule.Object,
//                                               pricePlanManager.Object,
//                                               api.Instance);

//            var subscriptionInternal = fixture.Create<SubscriptionInternal>();
//            subscriptionInternal.ExternalId = null;
//            subscriptionInternal.CouponGroups = null;
//            subscriptionInternal.FileTypesIds = null;
//            subscriptionInternal.HouseholdLimitationsId = null;
//            subscriptionInternal.InternalDiscountModuleId = null;
//            subscriptionInternal.PricePlanIds = null;
//            var response = manager.Add(fixture.Create<ContextData>(), subscriptionInternal);

//            Assert.That(response.Status.Code, Is.EqualTo((int)expectedCode));
//        }

//        private static IEnumerable DeleteCases()
//        {
//            Fixture fixture = new Fixture();
//            SubscriptionInternal subscriptionInternal = fixture.Create<SubscriptionInternal>();
//            List<SubscriptionInternal> subscriptionInternalList = new List<SubscriptionInternal>();
//            subscriptionInternalList.Add(subscriptionInternal);

//            yield return new TestCaseData(eResponseStatus.OK, 1, subscriptionInternalList, subscriptionInternalList[0].Id).SetName("CheckDeleteSuccess");
//            yield return new TestCaseData(eResponseStatus.Error, 0, subscriptionInternalList, subscriptionInternalList[0].Id).SetName("CheckDeleteFailed");
//        }

//        private static IEnumerable InsertCases()
//        {
//            yield return new TestCaseData(eResponseStatus.Error, 0).SetName("CheckInsertFailed");
//            yield return new TestCaseData(eResponseStatus.OK, 12).SetName("CheckInsertSuccess");
//        }
//    }
//}