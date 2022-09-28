using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using ApiLogic.IndexManager.Mappings;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using Core.Catalog;
using IngestHandler.Common.Infrastructure;
using IngestHandler.Common.Managers;
using IngestHandler.Common.Repositories;
using Phx.Lib.Log;
using Moq;
using NeoSmart.StreamCompare;
using NUnit.Framework;
using TVinciShared;

namespace IngestV2.Tests
{
    public class Tests
    {
        private MockRepository _mockRepository;
        private Mock<ICatalogManagerAdapter> _catalogManagerAdapterMock;
        private Mock<IMappingTypeResolver> _mappingTypeResolverMock;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WindowsService, "/var/log");
            Console.SetOut(TestContext.Progress);
        }

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _catalogManagerAdapterMock = _mockRepository.Create<ICatalogManagerAdapter>();
            _mappingTypeResolverMock = _mockRepository.Create<IMappingTypeResolver>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }


        [Test]
        public void TestBulkUploadObjectHashComparison_ShouldBe_True_When_Equal()
        {
            var sCompare = new StreamCompare();
            var bulk1 = new BulkUpload()
            {
                Id = 1,
                Errors = new[]
                {
                    new Status(1, "message1"),
                    new Status(1, "message2"),
                },
                AddedObjects = new List<IAffectedObject>()
                {
                    new EpgProgramBulkUploadObject()
                    {
                        EpgId = 100,
                        ChannelId = 200,
                    }
                }
            };

            var bulk2 = ObjectCopier.Clone(bulk1);
            var areEqual = false;
            var bf = new BinaryFormatter();
            using (var ms1 = new MemoryStream())
            using (var ms2 = new MemoryStream())
            {
                bf.Serialize(ms1, bulk1);
                bf.Serialize(ms2, bulk2);
                ms1.Seek(0, SeekOrigin.Begin);
                ms2.Seek(0, SeekOrigin.Begin);
                areEqual = sCompare.AreEqualAsync(ms1, ms2, true).GetAwaiter().GetResult();
                // var r1 = ms1.ToArray();
                // var r2 = ms2.ToArray();
                //
                // areEqual = r1.SequenceEqual(r2);
            }

            Assert.True(areEqual);
        }

        public static byte[] ObjectToByteArray(object obj)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        [Test]
        public void TestShouldAddProgramsToEmptyEpg()
        {
            var existingEpg = new List<EpgProgramBulkUploadObject>();
            var epgRepositoryMock = GetMockEpgRepo(existingEpg);
            _catalogManagerAdapterMock.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>())).Returns(true);
            _catalogManagerAdapterMock.Setup(x => x.GetCatalogGroupCache(It.IsAny<int>())).Returns(new CatalogGroupCache());

            var crudManager = new EpgCRUDOperationsManager(epgRepositoryMock.Object, _mappingTypeResolverMock.Object, _catalogManagerAdapterMock.Object);
            var bulkUpload = ProgramGenerator.Generate(3).BuildBulkUploadObj();
            var programsToIngest = bulkUpload.Results.Cast<BulkUploadProgramAssetResult>().ToList();
            var crudOperations = crudManager.CalculateCRUDOperations(bulkUpload, eIngestProfileOverlapPolicy.Reject, eIngestProfileAutofillPolicy.KeepHoles, null);


            var externalIdsToAddResult = crudOperations.ItemsToAdd.Select(p => p.EpgExternalId);
            var externalIdsToIngest = programsToIngest.Select(p => p.ProgramExternalId);

            CollectionAssert.AreEquivalent(externalIdsToIngest, externalIdsToAddResult);
            CollectionAssert.IsEmpty(crudOperations.ItemsToDelete);
            CollectionAssert.IsEmpty(crudOperations.ItemsToUpdate);
            CollectionAssert.IsEmpty(crudOperations.RemainingItems);
            CollectionAssert.IsEmpty(crudOperations.AffectedItems);
            CollectionAssert.IsEmpty(programsToIngest.Where(r => r.Errors != null).SelectMany(r => r.Errors), "no errors expected");
        }

        [Test]
        public void TestShouldOverwriteEntireEpg()
        {
            var existingEpgStart = new DateTime(2000, 1, 1, 0, 0, 0);
            var existingEpg = ProgramGenerator.Generate(3).FromDate(existingEpgStart).WithEpgIds().StartFromId(100).BuildExistingPrograms();
            var epgRepositoryMock = GetMockEpgRepo(existingEpg);
            _catalogManagerAdapterMock.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>())).Returns(true);
            _catalogManagerAdapterMock.Setup(x => x.GetCatalogGroupCache(It.IsAny<int>())).Returns(new CatalogGroupCache());

            var crudManager = new EpgCRUDOperationsManager(epgRepositoryMock.Object, _mappingTypeResolverMock.Object, _catalogManagerAdapterMock.Object);
            var bulkUpload = ProgramGenerator.Generate(3).FromDate(existingEpgStart).BuildBulkUploadObj();

            var programsToIngest = bulkUpload.Results.Cast<BulkUploadProgramAssetResult>().ToList();
            var crudOperations = crudManager.CalculateCRUDOperations(bulkUpload, eIngestProfileOverlapPolicy.Reject, eIngestProfileAutofillPolicy.KeepHoles, null);


            var externalIdsToIngest = programsToIngest.Select(p => p.ProgramExternalId);
            var existingEpgExternalIds = existingEpg.Select(p => p.EpgExternalId);
            var externalIdsToAdd = crudOperations.ItemsToAdd.Select(p => p.EpgExternalId);
            var externalIdsToDelete = crudOperations.ItemsToDelete.Select(p => p.EpgExternalId);

            CollectionAssert.AreEquivalent(externalIdsToAdd, externalIdsToIngest, "itemsToAdd");
            CollectionAssert.AreEquivalent(existingEpgExternalIds, externalIdsToDelete, "itemsToDelete");
            CollectionAssert.IsEmpty(crudOperations.ItemsToUpdate, "ItemsToUpdate");
            CollectionAssert.IsEmpty(crudOperations.RemainingItems, "RemainingItems");
            CollectionAssert.IsEmpty(crudOperations.AffectedItems, "AffectedItems");
            CollectionAssert.IsEmpty(programsToIngest.Where(r => r.Errors != null).SelectMany(r => r.Errors), "no errors expected");
        }

        [Test]
        public void TestShouldOverwritePartOfEpg()
        {
            var existingEpgStart = new DateTime(2000, 1, 1, 0, 0, 0);
            var existingEpg = ProgramGenerator.Generate(4).FromDate(existingEpgStart).WithEpgIds().StartFromId(100).BuildExistingPrograms();
            var epgRepositoryMock = GetMockEpgRepo(existingEpg);
            _catalogManagerAdapterMock.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>())).Returns(true);
            _catalogManagerAdapterMock.Setup(x => x.GetCatalogGroupCache(It.IsAny<int>())).Returns(new CatalogGroupCache());

            var crudManager = new EpgCRUDOperationsManager(epgRepositoryMock.Object, _mappingTypeResolverMock.Object, _catalogManagerAdapterMock.Object);

            // programs to ingest are smaller by 1 and shifted one prog to the future
            // so we expect the existing first program to be in remaining items
            var bulkUpload = ProgramGenerator.Generate(3).FromDate(existingEpgStart.AddHours(1)).BuildBulkUploadObj();
            var programsToIngest = bulkUpload.Results.Cast<BulkUploadProgramAssetResult>().ToList();
            var crudOperations = crudManager.CalculateCRUDOperations(bulkUpload, eIngestProfileOverlapPolicy.Reject, eIngestProfileAutofillPolicy.KeepHoles, null);


            var itemsToIngest = programsToIngest.Select(p => p.ProgramExternalId);
            var itemsToAdd = crudOperations.ItemsToAdd.Select(p => p.EpgExternalId);

            var firstExistingProgram = existingEpg.First().EpgExternalId;
            var itemsToDelete = crudOperations.ItemsToDelete.Select(e => e.EpgExternalId);
            var expectedItemsToDelete = existingEpg.Select(p => p.EpgExternalId).ToList();
            expectedItemsToDelete.Remove(firstExistingProgram);


            CollectionAssert.AreEquivalent(itemsToIngest, itemsToAdd, "itemsToAdd");
            CollectionAssert.AreEquivalent(expectedItemsToDelete, itemsToDelete, "itemsToDelete");
            CollectionAssert.IsEmpty(crudOperations.ItemsToUpdate, "ItemsToUpdate");
            CollectionAssert.IsEmpty(crudOperations.AffectedItems, "AffectedItems");
            Assert.That(crudOperations.RemainingItems, Has.Count.EqualTo(1));
            Assert.That(crudOperations.RemainingItems.First().EpgExternalId, Is.EqualTo(firstExistingProgram));
            CollectionAssert.IsEmpty(programsToIngest.Where(r => r.Errors != null).SelectMany(r => r.Errors), "no errors expected");
        }

        [Test]
        public void TestOverlapInSourceReject()
        {
            var existingEpgStart = new DateTime(2000, 1, 1, 0, 0, 0);
            var existingEpg = ProgramGenerator.Generate(3).BuildExistingPrograms();
            var epgRepositoryMock = GetMockEpgRepo(existingEpg);

            var crudManager = new EpgCRUDOperationsManager(epgRepositoryMock.Object, _mappingTypeResolverMock.Object, _catalogManagerAdapterMock.Object);

            var bulkUpload = ProgramGenerator.Generate(3).StartFromId(10).FromDate(existingEpgStart).BuildBulkUploadObj();
            var programsToIngestOverlap = ProgramGenerator.Generate(3).StartFromId(20).FromDate(existingEpgStart.AddMinutes(-30)).BuildProgramsToIngest();
            bulkUpload.Results.AddRange(programsToIngestOverlap);
            var programsToIngest = bulkUpload.Results.Cast<BulkUploadProgramAssetResult>().ToList();
            var crudOperations = crudManager.CalculateCRUDOperations(bulkUpload, eIngestProfileOverlapPolicy.Reject, eIngestProfileAutofillPolicy.KeepHoles, null);

            Assert.IsNull(crudOperations);
            CollectionAssert.IsNotEmpty(programsToIngest.Where(r => r.Errors != null).SelectMany(r => r.Errors), "expected overlap errors");
        }

        [Test]
        public void TestOverlapInAfterIngestReject()
        {
            var existingEpgStart = new DateTime(2000, 1, 1, 0, 0, 0);
            var existingEpg = ProgramGenerator.Generate(3).FromDate(existingEpgStart).BuildExistingPrograms();
            var epgRepositoryMock = GetMockEpgRepo(existingEpg);
            _catalogManagerAdapterMock.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>())).Returns(true);
            _catalogManagerAdapterMock.Setup(x => x.GetCatalogGroupCache(It.IsAny<int>())).Returns(new CatalogGroupCache());

            var crudManager = new EpgCRUDOperationsManager(epgRepositoryMock.Object, _mappingTypeResolverMock.Object, _catalogManagerAdapterMock.Object);

            var bulkUpload = ProgramGenerator.Generate(3).StartFromId(10).FromDate(existingEpgStart.AddMinutes(30)).BuildBulkUploadObj();
            var programsToIngest = bulkUpload.Results.Cast<BulkUploadProgramAssetResult>().ToList();
            var crudOperations = crudManager.CalculateCRUDOperations(bulkUpload, eIngestProfileOverlapPolicy.Reject, eIngestProfileAutofillPolicy.KeepHoles, null);

            Assert.IsNull(crudOperations);
            Assert.That(programsToIngest.First().Errors.First().Code, Is.EqualTo((int)eResponseStatus.Error));
            Assert.That(programsToIngest.First().Errors.First().Message, Contains.Substring("overlap").IgnoreCase);
            Assert.That(programsToIngest.First().Errors.First().Message, Contains.Substring("Reject").IgnoreCase);
        }

        [TestCase(30, 30, Description = "new epg starts and ends in the middle of existing epg")]
        [TestCase(0, 30, Description = "new epg starts in the same time as existing epg and ends in the middle of existing epg")]
        [TestCase(30, 90, Description = "new epg starts in the middle of existing epg and ends in the same time as existing epg")]
        public void TestOverlapInAfterIngestCutTargetMiddle(int shiftFromExistingEpgInMinutes, int durationInMinutes)
        {
            var existingEpgStart = new DateTime(2000, 1, 1, 0, 0, 0);
            var existingEpg = ProgramGenerator
                .Generate(3)
                .WithEpgIds()
                .WithDuration(TimeSpan.FromHours(2))
                .FromDate(existingEpgStart)
                .BuildExistingPrograms();
            var epgRepositoryMock = GetMockEpgRepo(existingEpg);
            _catalogManagerAdapterMock.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>())).Returns(true);
            _catalogManagerAdapterMock.Setup(x => x.GetCatalogGroupCache(It.IsAny<int>())).Returns(new CatalogGroupCache());

            var crudManager = new EpgCRUDOperationsManager(epgRepositoryMock.Object, _mappingTypeResolverMock.Object, _catalogManagerAdapterMock.Object);

            var bulkUpload = ProgramGenerator
                .Generate(1)
                .WithEpgIds()
                .StartFromId(10)
                .FromDate(existingEpgStart.AddMinutes(shiftFromExistingEpgInMinutes))
                .WithDuration(TimeSpan.FromMinutes(durationInMinutes))
                .BuildBulkUploadObj();

            var crudOperations = crudManager.CalculateCRUDOperations(bulkUpload, eIngestProfileOverlapPolicy.CutTarget, eIngestProfileAutofillPolicy.KeepHoles, null);

            Assert.That(crudOperations, Is.Not.Null);
            Assert.That(crudOperations.ItemsToAdd.Select(_ => _.EpgId), Is.EquivalentTo(new[] { 10 }));
            Assert.That(crudOperations.ItemsToDelete.Select(_ => _.EpgId), Is.EquivalentTo(new[] { 1 }));
        }

        [Test]
        public void TestGapsReject()
        {
            var existingEpgStart = new DateTime(2000, 1, 1, 0, 0, 0);
            var existingEpg = new List<EpgProgramBulkUploadObject>();
            var epgRepositoryMock = GetMockEpgRepo(existingEpg);
            _catalogManagerAdapterMock.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>())).Returns(true);
            _catalogManagerAdapterMock.Setup(x => x.GetCatalogGroupCache(It.IsAny<int>())).Returns(new CatalogGroupCache());

            var crudManager = new EpgCRUDOperationsManager(epgRepositoryMock.Object, _mappingTypeResolverMock.Object, _catalogManagerAdapterMock.Object);

            var bulkUpload = ProgramGenerator
                .Generate(1)
                .StartFromId(10)
                .FromDate(existingEpgStart.AddMinutes(30))
                .WithDuration(TimeSpan.FromMinutes(30))
                .BuildBulkUploadObj();

            var additionalPrograms = ProgramGenerator
                .Generate(2)
                .FromDate(existingEpgStart.AddHours(4))
                .BuildProgramsToIngest();

            bulkUpload.Results.AddRange(additionalPrograms);
            var programsToIngest = bulkUpload.Results.Cast<BulkUploadProgramAssetResult>().ToList();
            var crudOperations = crudManager.CalculateCRUDOperations(bulkUpload, eIngestProfileOverlapPolicy.Reject, eIngestProfileAutofillPolicy.Reject, null);

            Assert.That(crudOperations, Is.Null);
            Assert.That(programsToIngest.First().Errors.First().Code, Is.EqualTo((int)eResponseStatus.EPGSProgramDatesError));
        }


        [Test]
        public void TestGapsAutofill()
        {
            var existingEpgStart = new DateTime(2000, 1, 1, 0, 0, 0);
            var existingEpg = new List<EpgProgramBulkUploadObject>();
            var epgRepositoryMock = GetMockEpgRepo(existingEpg);
            _catalogManagerAdapterMock.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>())).Returns(true);
            _catalogManagerAdapterMock.Setup(x => x.GetCatalogGroupCache(It.IsAny<int>())).Returns(new CatalogGroupCache());

            var crudManager = new EpgCRUDOperationsManager(epgRepositoryMock.Object, _mappingTypeResolverMock.Object, _catalogManagerAdapterMock.Object);

            var bulkUpload = ProgramGenerator
                .Generate(1)
                .StartFromId(10)
                .FromDate(existingEpgStart.AddMinutes(30))
                .WithDuration(TimeSpan.FromMinutes(30))
                .BuildBulkUploadObj();

            var additionalPrograms = ProgramGenerator
                .Generate(2)
                .FromDate(existingEpgStart.AddHours(4))
                .BuildProgramsToIngest();

            bulkUpload.Results.AddRange(additionalPrograms);
            var programsToIngest = bulkUpload.Results.Cast<BulkUploadProgramAssetResult>().ToList();
            var crudOperations = crudManager.CalculateCRUDOperations(bulkUpload, eIngestProfileOverlapPolicy.Reject, eIngestProfileAutofillPolicy.Autofill, null);

            Assert.That(crudOperations.ItemsToAdd.Where(i => i.IsAutoFill).ToList(), Has.Count.EqualTo(1));
            Assert.That(programsToIngest.First().Warnings.First().Code, Is.EqualTo((int)eResponseStatus.EPGSProgramDatesError));
        }

        [Test]
        public void TestAutofillDueToMiddleCutTargetRemoval()
        {
            var existingEpgStart = new DateTime(2000, 1, 1, 0, 0, 0);
            var existingEpg = ProgramGenerator
                .Generate(3)
                .WithDuration(TimeSpan.FromHours(2))
                .FromDate(existingEpgStart)
                .BuildExistingPrograms();
            var epgRepositoryMock = GetMockEpgRepo(existingEpg);
            _catalogManagerAdapterMock.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>())).Returns(true);
            _catalogManagerAdapterMock.Setup(x => x.GetCatalogGroupCache(It.IsAny<int>())).Returns(new CatalogGroupCache());

            var crudManager = new EpgCRUDOperationsManager(epgRepositoryMock.Object, _mappingTypeResolverMock.Object, _catalogManagerAdapterMock.Object);

            var bulkUpload = ProgramGenerator
                .Generate(1)
                .StartFromId(10)
                .FromDate(existingEpgStart.AddHours(2).AddMinutes(30))
                .BuildBulkUploadObj();
            var programsToIngest = bulkUpload.Results.Cast<BulkUploadProgramAssetResult>().ToList();
            var crudOperations = crudManager.CalculateCRUDOperations(bulkUpload, eIngestProfileOverlapPolicy.CutTarget, eIngestProfileAutofillPolicy.Autofill, null);

            Assert.That(crudOperations.ItemsToDelete, Has.Count.EqualTo(1));
            Assert.That(crudOperations.ItemsToAdd.Where(i => i.IsAutoFill).ToList(), Has.Count.EqualTo(2));
            Assert.That(crudOperations.ItemsToAdd, Has.Count.EqualTo(3));
        }

        [Test]
        public void TestAutofillWithExistingProgram()
        {
            var existingEpgStart = new DateTime(2000, 1, 1, 0, 0, 0);
            var existingEpg = ProgramGenerator
                .Generate(1)
                .FromDate(existingEpgStart)
                .BuildExistingPrograms();
            var epgRepositoryMock = GetMockEpgRepo(existingEpg);
            _catalogManagerAdapterMock.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>())).Returns(true);
            _catalogManagerAdapterMock.Setup(x => x.GetCatalogGroupCache(It.IsAny<int>())).Returns(new CatalogGroupCache());

            var crudManager = new EpgCRUDOperationsManager(epgRepositoryMock.Object, _mappingTypeResolverMock.Object, _catalogManagerAdapterMock.Object);

            var bulkUpload = ProgramGenerator
                .Generate(1)
                .StartFromId(10)
                .FromDate(existingEpgStart.AddHours(4))
                .BuildBulkUploadObj();
            var programsToIngest = bulkUpload.Results.Cast<BulkUploadProgramAssetResult>().ToList();

            var crudOps = crudManager
                .CalculateCRUDOperations(bulkUpload, eIngestProfileOverlapPolicy.Reject, eIngestProfileAutofillPolicy.Autofill, null);

            CollectionAssert.IsEmpty(crudOps.ItemsToDelete);
            CollectionAssert.IsEmpty(crudOps.AffectedItems);
            Assert.That(crudOps.ItemsToAdd, Has.Count.EqualTo(2));
            Assert.That(crudOps.ItemsToAdd.Where(i => i.IsAutoFill).ToList(), Has.Count.EqualTo(1));
        }


        [Test]
        public void Test5ProgramsUpdate1stToOverlap2Last()
        {
            var existingEpgStart = new DateTime(2000, 1, 1, 0, 0, 0);
            var existingEpg = ProgramGenerator
                .Generate(5)
                .FromDate(existingEpgStart)
                .WithEpgIds()
                .BuildExistingPrograms();
            var epgRepositoryMock = GetMockEpgRepo(existingEpg);
            _catalogManagerAdapterMock.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>())).Returns(true);
            _catalogManagerAdapterMock.Setup(x => x.GetCatalogGroupCache(It.IsAny<int>())).Returns(new CatalogGroupCache());

            var crudManager = new EpgCRUDOperationsManager(epgRepositoryMock.Object, _mappingTypeResolverMock.Object, _catalogManagerAdapterMock.Object);

            var bulkUpload = ProgramGenerator
                .Generate(1)
                .FromDate(existingEpgStart.AddHours(3).AddMinutes(30))
                .WithDuration(TimeSpan.FromMinutes(90))
                .BuildBulkUploadObj();
            var programsToIngest = bulkUpload.Results.Cast<BulkUploadProgramAssetResult>().ToList();

            var crudOps = crudManager
                .CalculateCRUDOperations(bulkUpload, eIngestProfileOverlapPolicy.CutTarget, eIngestProfileAutofillPolicy.Autofill, null);

            // last program should be completle removed
            // the one before last is the affected item that should be deleted as well and the added back as an affected item with same epgId
            Assert.That(crudOps.ItemsToDelete, Has.Count.EqualTo(3));
            Assert.That(crudOps.AffectedItems, Has.Count.EqualTo(1));
            Assert.That(crudOps.AffectedItems.First().EndDate, Is.EqualTo(programsToIngest.First().StartDate));
            Assert.That(crudOps.AffectedItems.First().EpgId, Is.EqualTo(4));
        }


        [Test]
        public void TestMoveProgramBetweenDays()
        {
            var existingEpgStart = new DateTime(2000, 1, 1, 0, 0, 0);
            var existingEpg = ProgramGenerator
                .Generate(1)
                .FromDate(existingEpgStart)
                .WithEpgIds()
                .BuildExistingPrograms();
            var epgRepositoryMock = GetMockEpgRepo(existingEpg);
            _catalogManagerAdapterMock.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>())).Returns(true);
            _catalogManagerAdapterMock.Setup(x => x.GetCatalogGroupCache(It.IsAny<int>())).Returns(new CatalogGroupCache());

            var crudManager = new EpgCRUDOperationsManager(epgRepositoryMock.Object, _mappingTypeResolverMock.Object, _catalogManagerAdapterMock.Object);

            var bulkUpload = ProgramGenerator
                .Generate(1)
                .FromDate(existingEpgStart.AddDays(1).AddHours(2))
                .WithDuration(TimeSpan.FromMinutes(90))
                .BuildBulkUploadObj();
            var programsToIngest = bulkUpload.Results.Cast<BulkUploadProgramAssetResult>().ToList();

            var crudOps = crudManager
                .CalculateCRUDOperations(bulkUpload, eIngestProfileOverlapPolicy.CutTarget, eIngestProfileAutofillPolicy.Autofill, null);


            Assert.That(crudOps.ItemsToDelete, Has.Count.EqualTo(1));
            Assert.That(crudOps.ItemsToUpdate, Has.Count.EqualTo(1));
        }

        [Test]
        public void TestAddShallowCopyOfProgramToDeleteIfCrossMidnight()
        {
            var existingEpgStart = new DateTime(2021, 1, 20, 22, 30, 0);
            var existingEpg = ProgramGenerator
                .Generate(1)
                .FromDate(existingEpgStart)
                .WithEpgIds()
                .StartFromId(1000)
                .WithDuration(TimeSpan.FromHours(3))
                .BuildExistingPrograms();

            var epgRepositoryMock = GetMockEpgRepo(existingEpg);
            _catalogManagerAdapterMock.Setup(x => x.DoesGroupUsesTemplates(It.IsAny<int>())).Returns(true);
            _catalogManagerAdapterMock.Setup(x => x.GetCatalogGroupCache(It.IsAny<int>())).Returns(new CatalogGroupCache());

            var crudManager = new EpgCRUDOperationsManager(epgRepositoryMock.Object, _mappingTypeResolverMock.Object, _catalogManagerAdapterMock.Object);

            var bulkUpload = ProgramGenerator
                .Generate(1)
                .FromDate(existingEpgStart.AddMinutes(-30))
                .WithEpgIds()
                .WithDuration(TimeSpan.FromHours(3))
                .BuildBulkUploadObj();

            var crudOps = crudManager
                .CalculateCRUDOperations(bulkUpload, eIngestProfileOverlapPolicy.CutTarget, eIngestProfileAutofillPolicy.Autofill, null);


            var shallowCopyEpg = crudOps.ItemsToDelete[0];
            var epgToCut = crudOps.ItemsToDelete[1];
            var expectedStartDateOfCutTargetProgram = new DateTime(2021, 1, 21, 1, 0, 0);

            Assert.That(shallowCopyEpg.StartDate, Is.EqualTo(existingEpgStart));
            Assert.That(epgToCut.StartDate, Is.EqualTo(expectedStartDateOfCutTargetProgram));
            Assert.That(shallowCopyEpg, Is.Not.SameAs(epgToCut));
            Assert.That(crudOps.ItemsToDelete, Has.Count.EqualTo(2));
            Assert.That(crudOps.ItemsToAdd, Has.Count.EqualTo(1));
            Assert.That(crudOps.AffectedItems, Has.Count.EqualTo(1));
        }

        private static Mock<IEpgRepository> GetMockEpgRepo(List<EpgProgramBulkUploadObject> existingEpg)
        {
            var epgRepositoryMock = new Mock<IEpgRepository>();
            epgRepositoryMock.Setup(m => m
                    .GetCurrentProgramsByDate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(existingEpg);
            return epgRepositoryMock;
        }
    }
};