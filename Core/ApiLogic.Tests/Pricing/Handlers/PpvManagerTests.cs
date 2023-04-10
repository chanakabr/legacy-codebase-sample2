using System;
using System.Collections;
using System.Collections.Generic;
using ApiLogic.Pricing.Handlers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Pricing.Dto;
using ApiObjects.Response;
using AutoFixture;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Api.Managers;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Pricing;
using DAL;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.Pricing.Handlers
{
    [TestFixture]
    [Ignore("Temporarily ignored because of random fails. Described in https://github.com/kaltura/ott-backend/pull/1113")]
    public class PpvManagerTests
    {
        [TestCaseSource(nameof(DeleteCases))]
        public void CheckDelete(DeleteTestCase deleteTestCase)
        {
            Fixture fixture = new Fixture();
            var repositoryMock = new Mock<IPpvManagerRepository>();
            var virtualAssetManagerMock = new Mock<IVirtualAssetManager>();
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(deleteTestCase.ppvList, true, false);
            virtualAssetManagerMock.Setup(x => x.DeleteVirtualAsset(It.IsAny<int>(), It.IsAny<VirtualAssetInfo>()))
                .Returns(deleteTestCase.VirtualAssetInfoResponse);
            layeredCacheMock.Setup(deleteTestCase.ppvToDelete, true, true);
            repositoryMock.Setup(x => x.DeletePPV(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
                                   .Returns(deleteTestCase.DeletePpv);
            PpvManager manager = new PpvManager(
                repositoryMock.Object,
                layeredCacheMock.Object,
                Mock.Of<IPriceDetailsManager>(),
                Mock.Of<IDiscountDetailsManager>(),
                Mock.Of<IUsageModuleManager>(),
                Mock.Of<IPricingModule>(),
                virtualAssetManagerMock.Object,
                Mock.Of<IMediaFileTypeManager>(),
                Mock.Of<IAssetUserRuleManager>());
            var response = manager.Delete(fixture.Create<ContextData>(), deleteTestCase.ExpectedId);

            Assert.That(response.Code, Is.EqualTo((int)deleteTestCase.ResponseStatus));
        }

        private static IEnumerable DeleteCases()
        {
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.OK, true)).SetName("CheckDeleteSuccess");
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.PpvModuleNotExist, false, false)).SetName("CheckDeleteCodeNotExist");
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.Error, false)).SetName("CheckDeleteFailed");
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.Error, false, isVirtualAssetDeleted: false)).SetName("CheckDeleteVirtualAssetFailed");
        }

        public class DeleteTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal bool DeletePpv { get; private set; }
            internal eResponseStatus ResponseStatus { get; private set; }
            internal List<PpvDTO> ppvList { get; private set; }
            internal PPVModule ppvToDelete { get; private set; }
            internal long ExpectedId { get; private set; }
            internal VirtualAssetInfoResponse VirtualAssetInfoResponse { get; private set; }
            

            public DeleteTestCase(eResponseStatus responseStatus, bool deletePpv, bool idExist = true, bool isVirtualAssetDeleted = true)
            {
                VirtualAssetInfoResponse = fixture.Create<VirtualAssetInfoResponse>();
                VirtualAssetInfoResponse.Status = VirtualAssetInfoStatus.OK;
                ResponseStatus = responseStatus;
                DeletePpv = deletePpv;
                ppvList = fixture.Create<List<PpvDTO>>();
                ppvToDelete = fixture.Create<PPVModule>();
                ppvToDelete.m_oCouponsGroup.m_sGroupCode = fixture.Create<int>().ToString();
                ppvList[0] = convertToDto(ppvToDelete);
                foreach (var ppvModule in ppvList)
                {
                    ppvModule.IsActive = true;
                    ppvModule.DiscountCode = fixture.Create<int>();
                }
                ExpectedId = ppvList[0].Id;
                if (!idExist)
                {
                    ExpectedId++;
                }

                if (!isVirtualAssetDeleted)
                {
                    ppvList[0].VirtualAssetId = fixture.Create<int>();
                    VirtualAssetInfoResponse.Status = VirtualAssetInfoStatus.Error;
                }
            }
        }
        
        [TestCaseSource(nameof(InsertCases))]
        public void CheckInsert(InsertTestCase insertTestCase)
        {
            Fixture fixture = new Fixture();
            var repositoryMock = new Mock<IPpvManagerRepository>();
            var discountDetailsManagerMock = new Mock<IDiscountDetailsManager>();
            var priceDetailsManagerMock = new Mock<IPriceDetailsManager>();
            var usageModuleManagerMock  = new Mock<IUsageModuleManager>();
            var pricingModuleMock  = new Mock<IPricingModule>();
            var virtualAssetManagerMock = new Mock<IVirtualAssetManager>();
            var mediaFileTypeManagerMock = new Mock<IMediaFileTypeManager>();
            var assetUserRuleManagerMock = new Mock<IAssetUserRuleManager>();
            
            mediaFileTypeManagerMock.Setup(x => x.GetMediaFileTypes(It.IsAny<int>()))
                .Returns(insertTestCase.MediaFileTypeResponse);
            virtualAssetManagerMock.Setup(x => x.AddVirtualAsset(It.IsAny<int>(), It.IsAny<VirtualAssetInfo>(), null))
                .Returns(insertTestCase.VirtualAssetInfoResponse);
            discountDetailsManagerMock.Setup(x => x.GetDiscountDetailsById(It.IsAny<int>(), It.IsAny<long>())).Returns(insertTestCase.DiscountDetailsResponse);
            priceDetailsManagerMock.Setup(x => x.GetPriceDetailsById(It.IsAny<int>(), It.IsAny<long>())).Returns(insertTestCase.PriceCode);
            usageModuleManagerMock.Setup(x => x.GetUsageModuleById(It.IsAny<int>(), It.IsAny<long>())).Returns(insertTestCase.UsageModuleResponse);
            pricingModuleMock.Setup(x => x.GetCouponsGroup(It.IsAny<int>(), It.IsAny<long>())).Returns(insertTestCase.CouponsGroupResponse);

            repositoryMock.Setup(x => x.InsertPPV(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<PpvDTO>())).Returns(insertTestCase.InsertId);
            PpvManager manager = new PpvManager(
                repositoryMock.Object,
                Mock.Of<ILayeredCache>(),
                priceDetailsManagerMock.Object,
                discountDetailsManagerMock.Object,
                usageModuleManagerMock.Object,
                pricingModuleMock.Object,
                virtualAssetManagerMock.Object,
                mediaFileTypeManagerMock.Object,
                assetUserRuleManagerMock.Object);
            var response = manager.Add(fixture.Create<ContextData>(), insertTestCase.PpvToAdd);

            Assert.That(response.Status.Code, Is.EqualTo((int)insertTestCase.ResponseStatus));
        }

        private static IEnumerable InsertCases()
        {
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.PriceDetailsDoesNotExist, false)).SetName("CheckInsertPriceCodeDoesNotExist");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.DiscountCodeNotExist,isDiscountDetailsExist: false )).SetName("CheckInsertDiscountCodeNotExist");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.Error,isCouponsGroupExist: false )).SetName("CheckInsertCouponsGroupNotExist");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.UsageModuleDoesNotExist,isUsageModuleExist: false )).SetName("CheckInsertUsageModuleNotExist");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.InvalidFileType, isFileTypeExist: false )).SetName("CheckInsertFileTypeNotExist");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.Error)).SetName("CheckInsertFailed");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.OK, isInsertSuccess: true)).SetName("CheckInsertSuccess");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.Error, isInsertSuccess: true, isVirtualAssetInserted: false)).SetName("CheckInsertVirtualAssetFailed");

        }

        public class InsertTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal GenericResponse<PriceDetails> PriceCode { get; private set; }
            internal eResponseStatus ResponseStatus { get; private set; }
            internal GenericResponse<DiscountDetails> DiscountDetailsResponse { get; private set; }
            internal GenericResponse<UsageModule> UsageModuleResponse { get; private set; }
            internal CouponsGroupResponse CouponsGroupResponse { get; private set; }
            internal GenericListResponse<MediaFileType> MediaFileTypeResponse { get; private set; }
            internal PpvModuleInternal PpvToAdd { get; private set; }
            internal int InsertId { get; private set; }
            internal VirtualAssetInfoResponse VirtualAssetInfoResponse { get; private set; }

            public InsertTestCase(eResponseStatus responseStatus, bool isPriceCodeExist = true, bool isUsageModuleExist = true, 
                bool isDiscountDetailsExist = true,bool isCouponsGroupExist = true,bool isInsertSuccess = false, bool isVirtualAssetInserted = true, bool isFileTypeExist = true)
            {
                ResponseStatus = responseStatus;
                InsertId = 0;
                PpvToAdd = fixture.Create<PpvModuleInternal>();
                PpvToAdd.CouponsGroupId = fixture.Create<int>();
                var mediaFileType = fixture.Create<List<MediaFileType>>();
                MediaFileTypeResponse = new GenericListResponse<MediaFileType>(Status.Ok, mediaFileType);
                if (isFileTypeExist)
                {
                    PpvToAdd.RelatedFileTypes = new List<int>((int) mediaFileType[0].Id);
                }

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
                
                if (isUsageModuleExist)
                {
                    var usageModule = fixture.Create<UsageModule>();
                    UsageModuleResponse = new GenericResponse<UsageModule>(Status.Ok, usageModule);
                }
                else
                {
                    UsageModuleResponse = new GenericResponse<UsageModule>(eResponseStatus.UsageModuleDoesNotExist);
                }
                
                if (isDiscountDetailsExist)
                {
                    var discountDetails = fixture.Create<DiscountDetails>();
                    DiscountDetailsResponse = new GenericResponse<DiscountDetails>(Status.Ok, discountDetails);
                }
                else
                {
                    DiscountDetailsResponse = new GenericResponse<DiscountDetails>(eResponseStatus.DiscountCodeNotExist);
                }
                
                if (isCouponsGroupExist)
                {
                    CouponsGroupResponse = fixture.Create<CouponsGroupResponse>();
                    CouponsGroupResponse.Status = Status.Ok;
                }
                else
                {
                    CouponsGroupResponse = fixture.Create<CouponsGroupResponse>();
                    CouponsGroupResponse.Status = Status.Error;
                }
                
                VirtualAssetInfoResponse = fixture.Create<VirtualAssetInfoResponse>();
                VirtualAssetInfoResponse.Status = VirtualAssetInfoStatus.OK;
                if (!isVirtualAssetInserted)
                {
                    VirtualAssetInfoResponse.Status = VirtualAssetInfoStatus.Error;
                }
            }
        }
        
        [TestCaseSource(nameof(UpdateCases))]
        public void CheckUpdate(UpdateTestCase updateTestCase)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IPpvManagerRepository>();
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(updateTestCase.ppvList, true, false);
            layeredCacheMock.Setup(updateTestCase.PpvModuleToInsert, true, true);

            var discountDetailsManagerMock = new Mock<IDiscountDetailsManager>();
            var priceDetailsManagerMock = new Mock<IPriceDetailsManager>();
            var usageModuleManagerMock  = new Mock<IUsageModuleManager>();
            var pricingModuleMock  = new Mock<IPricingModule>();
            var virtualAssetManagerMock = new Mock<IVirtualAssetManager>();
            var mediaFileTypeManagerMock = new Mock<IMediaFileTypeManager>();
            var assetUserRuleManagerMock = new Mock<IAssetUserRuleManager>();
            
            mediaFileTypeManagerMock.Setup(x => x.GetMediaFileTypes(It.IsAny<int>()))
                .Returns(updateTestCase.MediaFileTypeResponse);
            virtualAssetManagerMock.Setup(x => x.UpdateVirtualAsset(It.IsAny<int>(), It.IsAny<VirtualAssetInfo>()))
                .Returns(updateTestCase.VirtualAssetInfoResponse);
            discountDetailsManagerMock.Setup(x => x.GetDiscountDetailsById(It.IsAny<int>(), It.IsAny<long>())).Returns(updateTestCase.DiscountDetailsResponse);
            priceDetailsManagerMock.Setup(x => x.GetPriceDetailsById(It.IsAny<int>(), It.IsAny<long>())).Returns(updateTestCase.PriceCode);
            usageModuleManagerMock.Setup(x => x.GetUsageModuleById(It.IsAny<int>(), It.IsAny<long>())).Returns(updateTestCase.UsageModuleResponse);
            pricingModuleMock.Setup(x => x.GetCouponsGroup(It.IsAny<int>(), It.IsAny<long>())).Returns(updateTestCase.CouponsGroupResponse);
            
            repositoryMock.Setup(x => x.UpdatePPV(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(),It.IsAny<PpvDTO>())).Returns((int)updateTestCase.UpdatedRow);
            repositoryMock.Setup(x => x.UpdatePPVDescriptions(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(),It.IsAny<LanguageContainer[]>())).Returns((int)updateTestCase.UpdatedRow);
            repositoryMock.Setup(x => x.UpdatePPVFileTypes(It.IsAny<int>(), It.IsAny<int>(),It.IsAny< List<int>>())).Returns((int)updateTestCase.UpdatedRow);
            PpvManager manager = new PpvManager(
                repositoryMock.Object,
                layeredCacheMock.Object,
                priceDetailsManagerMock.Object,
                discountDetailsManagerMock.Object,
                usageModuleManagerMock.Object,
                pricingModuleMock.Object,
                virtualAssetManagerMock.Object,
                mediaFileTypeManagerMock.Object,
                assetUserRuleManagerMock.Object);
            var response = manager.Update(updateTestCase.Id, fixture.Create<ContextData>(), updateTestCase.PPvToInsert);

            Assert.That(response.Status.Code, Is.EqualTo((int)updateTestCase.ResponseStatus));
            repositoryMock.Verify(x => x.UpdatePPV(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(),It.IsAny<PpvDTO>()), Times.Exactly(updateTestCase.AmountCallToRepository));

        }

        private static IEnumerable UpdateCases()
        {
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.PpvModuleNotExist, isppvExist: false)).SetName("CheckUpdatePpvDoesNotExist");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.PriceDetailsDoesNotExist, isPriceCodeExist: false)).SetName("CheckUpdatePriceDetailsDoesNotExist");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.DiscountCodeNotExist, isDiscountDetailsExist: false)).SetName("CheckUpdateDiscountCodeNotExist");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.Error,isCouponsGroupExist: false )).SetName("CheckUpdateCouponsGroupNotExist");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.UsageModuleDoesNotExist,isUsageModuleExist: false )).SetName("CheckUpdateUsageModuleNotExist");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.InvalidFileType, isFileTypeExist: false )).SetName("CheckUpdateFileTypeNotExist");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.Error, amountCallToRepository: 1)).SetName("CheckUpdateFailed");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK, isUpdateSuccess: true, amountCallToRepository: 1)).SetName("CheckUpdateSuccess");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK, isUpdateSuccess: true)).SetName("CheckUpdateNotNedded");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.Error, amountCallToRepository: 1, isVirtualAssetUpdated: false)).SetName("CheckUpdateVirtualAssetFailed");

        }

        public class UpdateTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal GenericResponse<PriceDetails> PriceCode { get; private set; }
            internal eResponseStatus ResponseStatus { get; private set; }
            internal GenericResponse<DiscountDetails> DiscountDetailsResponse { get; private set; }
            internal GenericResponse<UsageModule> UsageModuleResponse { get; private set; }
            internal CouponsGroupResponse CouponsGroupResponse { get; private set; }
            internal List<PpvDTO> ppvList { get; private set; }
            internal long UpdatedRow { get; private set; }
            internal int Id { get; private set; }
            internal int AmountCallToRepository { get; private set; }
            internal PpvModuleInternal PPvToInsert { get; private set; }
            internal PPVModule PpvModuleToInsert { get; private set; }
            internal GenericListResponse<MediaFileType> MediaFileTypeResponse { get; private set; }
            internal VirtualAssetInfoResponse VirtualAssetInfoResponse { get; private set; }
            public UpdateTestCase(eResponseStatus responseStatus, bool isUsageModuleExist = true, 
                bool isDiscountDetailsExist = true, bool isCouponsGroupExist = true, bool isPriceCodeExist = true, 
                bool isUpdateSuccess = false, bool isppvExist = true, int amountCallToRepository = 0, bool isVirtualAssetUpdated = true, bool isFileTypeExist = true)
            {
                ResponseStatus = responseStatus;
                UpdatedRow = 0;
                ppvList = fixture.Create<List<PpvDTO>>();
                
                foreach (var ppvModule in ppvList)
                {
                    ppvModule.Id = fixture.Create<int>();
                    ppvModule.IsActive = true;
                }
                
                PPvToInsert = fixture.Create<PpvModuleInternal>();
                Id = PPvToInsert.Id = fixture.Create<int>();
                PPvToInsert.IsActive = true;
                PPvToInsert.CouponsGroupId = fixture.Create<int>();
                var mediaFileType = fixture.Create<List<MediaFileType>>();
                MediaFileTypeResponse = new GenericListResponse<MediaFileType>(Status.Ok, mediaFileType);
                if (isFileTypeExist)
                {
                    PPvToInsert.RelatedFileTypes = new List<int>((int) mediaFileType[0].Id);
                }
                PpvModuleToInsert =  convertToPPVModule(PPvToInsert);
                var PPvDTOToInsert = convertToPPVDTO(PPvToInsert);
                ppvList.Add(PPvDTOToInsert);
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
                
                if (isUsageModuleExist)
                {
                    var usageModule = fixture.Create<UsageModule>();
                    UsageModuleResponse = new GenericResponse<UsageModule>(Status.Ok, usageModule);
                }
                else
                {
                    UsageModuleResponse = new GenericResponse<UsageModule>(eResponseStatus.UsageModuleDoesNotExist);
                }
                
                if (isDiscountDetailsExist)
                {
                    var discountDetails = fixture.Create<DiscountDetails>();
                    DiscountDetailsResponse = new GenericResponse<DiscountDetails>(Status.Ok, discountDetails);
                }
                else
                {
                    DiscountDetailsResponse = new GenericResponse<DiscountDetails>(eResponseStatus.DiscountCodeNotExist);
                }
                
                CouponsGroupResponse = fixture.Create<CouponsGroupResponse>();
                if (isCouponsGroupExist)
                {
                    CouponsGroupResponse.Status = Status.Ok;
                }
                else
                {
                    CouponsGroupResponse.Status = Status.Error;
                }
                
                if (isUpdateSuccess)
                {
                    // Need to be bigger than 0
                    UpdatedRow = fixture.Create<int>() + 1;
                }
                if (!isppvExist)
                {
                    Id++;
                }
                if (amountCallToRepository > 0)
                {
                    PpvModuleToInsert = fixture.Create<PPVModule>();
                    PpvModuleToInsert.m_sObjectCode = Id.ToString();
                    ppvList.Remove(PPvDTOToInsert);
                    var newppv = fixture.Create<PpvDTO>();
                    newppv.IsActive = true;
                    newppv.Id = Id;
                    ppvList.Add(newppv);
                }
                
                VirtualAssetInfoResponse = fixture.Create<VirtualAssetInfoResponse>();
                VirtualAssetInfoResponse.Status = VirtualAssetInfoStatus.OK;
                if (!isVirtualAssetUpdated)
                {
                    AmountCallToRepository = 0;
                    VirtualAssetInfoResponse.ResponseStatus = new Status(eResponseStatus.Error);
                    VirtualAssetInfoResponse.Status = VirtualAssetInfoStatus.Error;
                }
            }
        }
        
        private static PPVModule convertToPPVModule(PpvModuleInternal ppv)
        {
            var usageModule = new UsageModule();
            usageModule.m_nObjectID = Convert.ToInt32(ppv.UsageModuleId.Value);
            var couponsGroup = new CouponsGroup();
            couponsGroup.m_sGroupCode = ppv.CouponsGroupId.ToString();
            var discountModule = new DiscountModule();
            discountModule.m_nObjectID =  Convert.ToInt32(ppv.DiscountId.Value);
            return new PPVModule()
            {
                m_sObjectCode = ppv.Id.ToString(),
                m_oPriceCode = new PriceCode(ppv.PriceId.Value),
                m_oUsageModule = usageModule,
                m_oCouponsGroup = couponsGroup,
                m_oDiscountModule = discountModule,
                m_sDescription = ppv.Description,
                m_sObjectVirtualName= ppv.Name,
                AdsParam = ppv.AdsParam,
                m_Product_Code = ppv.ProductCode,
                m_bFirstDeviceLimitation = ppv.FirstDeviceLimitation.Value,
                m_bSubscriptionOnly = ppv.SubscriptionOnly.Value,
                IsActive = ppv.IsActive.Value,
                alias = ppv.alias,
                m_relatedFileTypes = ppv.RelatedFileTypes,
                AdsPolicy = ppv.AdsPolicy,
            };
        }
        private static PpvDTO convertToDto(PPVModule ppv)
        {
            return new PpvDTO()
            {
                Name = ppv.m_sObjectVirtualName,
                PriceCode = ppv.m_oPriceCode.m_nObjectID,
                UsageModuleCode = ppv.m_oUsageModule.m_nObjectID,
                DiscountCode = ppv.m_oDiscountModule.m_nObjectID,
                CouponsGroupCode = int.Parse(ppv.m_oCouponsGroup.m_sGroupCode),
                Descriptions = ppv.m_sDescription,
                AdsParam = ppv.AdsParam,
                SubscriptionOnly = ppv.m_bSubscriptionOnly,
                FileTypesIds = ppv.m_relatedFileTypes,
                ProductCode = ppv.m_Product_Code,
                FirstDeviceLimitation = ppv.m_bFirstDeviceLimitation,
                alias = ppv.alias,
                AdsPolicy = ppv.AdsPolicy
            };
        }
        private static PpvDTO convertToPPVDTO(PpvModuleInternal ppv)
        {
            var ppvDto =  new PpvDTO()
            {
                Id = ppv.Id,
                Descriptions = ppv.Description,
                Name = ppv.Name,
                AdsParam = ppv.AdsParam,
                ProductCode = ppv.ProductCode,
                FirstDeviceLimitation = ppv.FirstDeviceLimitation.HasValue ? ppv.FirstDeviceLimitation.Value: false,
                SubscriptionOnly = ppv.SubscriptionOnly.HasValue ? ppv.SubscriptionOnly.Value: false,
                IsActive = ppv.IsActive.HasValue ? ppv.IsActive.Value: true,
                alias = ppv.alias,
                FileTypesIds = ppv.RelatedFileTypes,
                AdsPolicy = ppv.AdsPolicy,
                CouponsGroupCode = ppv.CouponsGroupId.HasValue ? ppv.CouponsGroupId.Value : 0,
                UsageModuleCode = ppv.UsageModuleId.HasValue ? ppv.UsageModuleId.Value : 0,
                DiscountCode = ppv.DiscountId.HasValue ? ppv.DiscountId.Value : 0,
                PriceCode = ppv.PriceId.HasValue ? ppv.PriceId.Value : 0,
            };
            if (ppv.VirtualAssetId.HasValue)
                ppvDto.VirtualAssetId = ppv.VirtualAssetId;
            if (ppv.CreateDate.HasValue)
                ppvDto.CreateDate = ppv.CreateDate.Value;
            if (ppv.UpdateDate.HasValue)
                ppvDto.UpdateDate = ppv.UpdateDate;
            return ppvDto;
        }
    }
}