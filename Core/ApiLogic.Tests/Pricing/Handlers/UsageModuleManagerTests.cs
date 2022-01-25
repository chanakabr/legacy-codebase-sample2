using ApiLogic.Pricing.Handlers;
using ApiObjects.Base;
using ApiObjects.Response;
using AutoFixture;
using Core.Pricing;
using DAL;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ApiObjects.Pricing;
using CachingProvider.LayeredCache;

namespace ApiLogic.Tests.Pricing.Handlers
{
    [TestFixture]
    [Ignore("Temporarily ignored because of random fails. Described in https://github.com/kaltura/ott-backend/pull/1113")]
    class UsageModuleManagerTests
    {
        [TestCaseSource(nameof(DeleteCases))]
        [Ignore("ignored due to https://kaltura.atlassian.net/browse/BEO-11237")]
        // This test cannot run and will required changes for decoupling UsageModuleManager form PPVManager
        public void CheckDelete(DeleteTestCase deleteTestCase)
        {
            Fixture fixture = new Fixture();
            var repositoryMock = new Mock<IModuleManagerRepository>();
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(deleteTestCase.UsageModuleList, true, false);
            repositoryMock.Setup(x => x.DeletePricePlan(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
                                    .Returns(deleteTestCase.IsUsageModuleDeleted);
            UsageModuleManager manager = new UsageModuleManager(repositoryMock.Object, layeredCacheMock.Object);
            var response = manager.Delete(fixture.Create<ContextData>(), deleteTestCase.ExpectedId);

            Assert.That(response.Code, Is.EqualTo((int)deleteTestCase.ResponseStatus));
        }
        
        private static IEnumerable DeleteCases()
        {
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.OK, true)).SetName("CheckDeleteSuccess");
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.UsageModuleDoesNotExist, true, false)).SetName("CheckDeleteCodeNotExist");
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.Error, false)).SetName("CheckDeleteFailed");
        }
        
        public class DeleteTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal bool IsUsageModuleDeleted { get; private set; }
            internal eResponseStatus ResponseStatus { get; private set; }
            internal List<UsageModuleDTO> UsageModuleList { get; private set; }
            internal long ExpectedId { get; private set; }

            public DeleteTestCase(eResponseStatus responseStatus, bool isUsageModuleDeleted, bool idExist = true)
            {
                ResponseStatus = responseStatus;
                IsUsageModuleDeleted = isUsageModuleDeleted;
                UsageModuleList = fixture.Create<List<UsageModuleDTO>>();
                ExpectedId = UsageModuleList[0].Id;
                if (!idExist)
                {
                    ExpectedId++;
                }
            }
        }
        
        [TestCaseSource(nameof(InsertCases))]
        public void CheckAdd(InsertTestCase insertTestCase)
        {
            Fixture fixture = new Fixture();
            var repositoryMock = new Mock<IModuleManagerRepository>();

            repositoryMock.Setup(x => x.InsertUsageModule(It.IsAny<long>(), It.IsAny<int>(), 
                It.IsAny<UsageModuleDTO>())).Returns(insertTestCase.InsertId);

            UsageModuleManager manager = new UsageModuleManager(repositoryMock.Object, Mock.Of<ILayeredCache>());

            var response = manager.Add(fixture.Create<ContextData>(), fixture.Create<UsageModule>());

            Assert.That(response.Status.Code, Is.EqualTo((int)insertTestCase.ResponseStatus));
        }

        private static IEnumerable InsertCases()
        {
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.OK)).SetName("CheckInsertSuccess");
            yield return new TestCaseData(new InsertTestCase(eResponseStatus.Error, false)).SetName("CheckInsertFailed");
        }

        public class InsertTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal eResponseStatus ResponseStatus { get; private set; }
            internal int InsertId { get; private set; }

            public InsertTestCase(eResponseStatus responseStatus, bool isInsertSuccess = true)
            {
                ResponseStatus = responseStatus;
                InsertId = 0;
                if(isInsertSuccess)
                {
                    // Need to be bigger than 0
                    InsertId = fixture.Create<int>() + 1;
                }
            }
        }
        
        [TestCaseSource(nameof(UpdateCases))]
        public void CheckUpdate(UpdateTestCase updateTestCase)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IModuleManagerRepository>();
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(updateTestCase.UsageModuleList, true, false);
            
            repositoryMock.Setup(x => x.UpdateUsageModule(It.IsAny<int>(), It.IsAny<long>(),
                It.IsAny<long>(), It.IsAny<UsageModuleDTO>())).Returns((int)updateTestCase.UpdatedRow);
            UsageModuleManager manager = new UsageModuleManager(repositoryMock.Object, layeredCacheMock.Object);
            var response = manager.Update(updateTestCase.Id, fixture.Create<ContextData>(), updateTestCase.UsageModuleToInsert);

            Assert.That(response.Status.Code, Is.EqualTo((int)updateTestCase.ResponseStatus));
            repositoryMock.Verify(x => x.UpdateUsageModule(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(),It.IsAny<UsageModuleDTO>()), Times.Exactly(updateTestCase.AmountCallToRepository));
        }
        
        private static IEnumerable UpdateCases()
        {
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.UsageModuleDoesNotExist, isUsageModuleExist: false)).SetName("CheckUpdateUsageModuleDoesNotExist");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.Error, amountCallToRepository: 1)).SetName("CheckUpdateFailed");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK, isUpdateSuccess: true, amountCallToRepository: 1)).SetName("CheckUpdateSuccess");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK, isUpdateSuccess: true)).SetName("CheckUpdateNotNedded");

        }

        public class UpdateTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal eResponseStatus ResponseStatus { get; private set; }
            internal List<UsageModuleDTO> UsageModuleList { get; private set; }
            internal long UpdatedRow { get; private set; }
            internal int Id { get; private set; }
            internal int AmountCallToRepository { get; private set; }
            internal UsageModuleForUpdate UsageModuleToInsert { get; private set; }
            public UpdateTestCase(eResponseStatus responseStatus, bool isUpdateSuccess = false, bool isUsageModuleExist = true, int amountCallToRepository = 0)
            {
                ResponseStatus = responseStatus;
               
                UpdatedRow = 0;
                UsageModuleList = fixture.Create<List<UsageModuleDTO>>();
                UsageModuleDTO usageModuleDTO = UsageModuleList[0];
                UsageModuleToInsert = new UsageModuleForUpdate()
                {
                    Id = usageModuleDTO.Id,
                    Name  =  usageModuleDTO.VirtualName,
                    MaxNumberOfViews =  usageModuleDTO.MaxNumberOfViews,
                    TsViewLifeCycle  =  usageModuleDTO.TsViewLifeCycle,
                    TsMaxUsageModuleLifeCycle =  usageModuleDTO.TsMaxUsageModuleLifeCycle,
                    Waiver =  usageModuleDTO.Waiver,
                    WaiverPeriod = usageModuleDTO.WaiverPeriod,
                    IsOfflinePlayBack = usageModuleDTO.IsOfflinePlayBack
                };
                Id = UsageModuleToInsert.Id;
                AmountCallToRepository = amountCallToRepository;
                
                if (isUpdateSuccess)
                {
                    // Need to be bigger than 0
                    UpdatedRow = fixture.Create<int>() + 1;
                }
                if (!isUsageModuleExist)
                {
                    Id++;
                }
                if (amountCallToRepository > 0)
                {
                    UsageModuleList.Remove(usageModuleDTO);
                    var usageModule = fixture.Create<UsageModuleDTO>();
                    usageModule.Id = Id;
                    UsageModuleList.Add(usageModule);
                }
            }
        }

        
        
        
        [TestCaseSource(nameof(ListCases))]
        public void CheckList(ListTestCase listTestCase)
        {
            Fixture fixture = new Fixture();
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(listTestCase.UsageModuleList, true, false);

            UsageModuleManager manager = new UsageModuleManager(Mock.Of<IModuleManagerRepository>(), layeredCacheMock.Object);
            var response = manager.GetUsageModules(fixture.Create<int>(), listTestCase.UsageModulId);
            Assert.That(response.Status.Code, Is.EqualTo((int)listTestCase.ResponseStatus));
            Assert.That(response.Objects.Count, Is.EqualTo(listTestCase.ListLength));
        }

        private static IEnumerable ListCases()
        {
            yield return new TestCaseData(new ListTestCase(eResponseStatus.OK, true)).SetName("CheckListEmpty");
            yield return new TestCaseData(new ListTestCase(eResponseStatus.OK)).SetName("CheckList");
            yield return new TestCaseData(new ListTestCase(eResponseStatus.OK, isIdInEmpty: false)).SetName("CheckListWithIdIn");
            yield return new TestCaseData(new ListTestCase(eResponseStatus.OK, isIdInEmpty: false, isIdInExists:false)).SetName("CheckListWithIdInNotExists");
        }
        
        public class ListTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal eResponseStatus ResponseStatus { get; private set; }
            internal List<UsageModuleDTO> UsageModuleList { get; private set; }
            internal int? UsageModulId { get; private set; }
            internal int ListLength { get; private set; }
            internal UsageModuleDTO usageModuleDTO  { get; private set; }
            public ListTestCase(eResponseStatus responseStatus, bool isListEmpty = false, bool isIdInEmpty = true, bool isIdInExists = true)
            {
                UsageModulId = null;
                ResponseStatus = responseStatus;
                if (isListEmpty)
                {
                    UsageModuleList = new List<UsageModuleDTO>();

                }
                else
                {                   
                    UsageModuleList = fixture.Create<List<UsageModuleDTO>>();
                    UsageModuleList.ForEach(x => x.Type = 1);
                    usageModuleDTO = UsageModuleList[0];
                    
                }
                ListLength = UsageModuleList.Count;
                if (!isIdInEmpty)
                {
                    UsageModulId = usageModuleDTO.Id;
                    ListLength = 1;
                    if (!isIdInExists)
                    {
                        UsageModuleList.Remove(usageModuleDTO);
                        ListLength = 0;
                    }
                }
            }
        }
    }
}