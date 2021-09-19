using System.Collections.Generic;
using System.Linq;
using ApiObjects;
using ApiObjects.BulkUpload;
using Core.Catalog;
using IngestHandler.Common.Infrastructure;
using IngestHandler.Domain.IngestProtection;
using Moq;
using NUnit.Framework;
using Tvinci.Core.DAL;

namespace IngestV2.Tests.Domain.IngestProtection
{
    public class IngestProtectProcessorTests
    {
        private MockRepository _mockRepository;
        private Mock<IEpgDal> _epgDalMock;
        private Mock<ICatalogManagerAdapter> _catalogManagerAdapterMock;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _epgDalMock = _mockRepository.Create<IEpgDal>();
            _catalogManagerAdapterMock = _mockRepository.Create<ICatalogManagerAdapter>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void ProcessIngestProtect_GroupNotUsesTemplates_ReturnsUnchangedItemsToUpdate()
        {
            _catalogManagerAdapterMock
                .Setup(x => x.DoesGroupUsesTemplates(1))
                .Returns(false);
            var crudOperations = BuildCrudOperations();
            var processor = new IngestProtectProcessor(_epgDalMock.Object, _catalogManagerAdapterMock.Object);

            processor.ProcessIngestProtect(1, crudOperations);

            AssertData(crudOperations.ItemsToUpdate[0].EpgCbObjects[0], "1 ENG New Name", "1 ENG New Description", "1 ENG New Meta1", "1 ENG New Meta2","1 ENG New Tag1", "1 ENG New Tag2");
            AssertData(crudOperations.ItemsToUpdate[0].EpgCbObjects[1], "1 ARB New Name", "1 ARB New Description", "1 ARB New Meta1", "1 ARB New Meta2","1 ARB New Tag1", "1 ARB New Tag2");
            AssertData(crudOperations.ItemsToUpdate[0].EpgCbObjects[2], "1 CSP New Name", "1 CSP New Description", "1 CSP New Meta1", "1 CSP New Meta2","1 CSP New Tag1", "1 CSP New Tag2");
        }

        [Test]
        public void ProcessIngestProtect_ProgramStructNotFound_ReturnsUnchangedItemsToUpdate()
        {
            _catalogManagerAdapterMock
                .Setup(x => x.DoesGroupUsesTemplates(1))
                .Returns(true);
            _catalogManagerAdapterMock
                .Setup(x => x.GetCatalogGroupCache(1))
                .Returns(BuildCatalogGroupCache(hasProgramStruct:false));
            var crudOperations = BuildCrudOperations();
            var processor = new IngestProtectProcessor(_epgDalMock.Object, _catalogManagerAdapterMock.Object);

            processor.ProcessIngestProtect(1, crudOperations);

            AssertData(crudOperations.ItemsToUpdate[0].EpgCbObjects[0], "1 ENG New Name", "1 ENG New Description", "1 ENG New Meta1", "1 ENG New Meta2","1 ENG New Tag1", "1 ENG New Tag2");
            AssertData(crudOperations.ItemsToUpdate[0].EpgCbObjects[1], "1 ARB New Name", "1 ARB New Description", "1 ARB New Meta1", "1 ARB New Meta2","1 ARB New Tag1", "1 ARB New Tag2");
            AssertData(crudOperations.ItemsToUpdate[0].EpgCbObjects[2], "1 CSP New Name", "1 CSP New Description", "1 CSP New Meta1", "1 CSP New Meta2","1 CSP New Tag1", "1 CSP New Tag2");
        }

        [TestCase(false, true)]
        [TestCase(true, false)]
        public void ProcessIngestProtect_ProtectedTopicsNotFound_ReturnsUnchangedItemsToUpdate(bool hasProtectedMetasAndTags, bool hasTopics)
        {
            _catalogManagerAdapterMock
                .Setup(x => x.DoesGroupUsesTemplates(1))
                .Returns(true);
            _catalogManagerAdapterMock
                .Setup(x => x.GetCatalogGroupCache(1))
                .Returns(BuildCatalogGroupCache(true, hasProtectedMetasAndTags, hasTopics));
            var crudOperations = BuildCrudOperations();
            var processor = new IngestProtectProcessor(_epgDalMock.Object, _catalogManagerAdapterMock.Object);

            processor.ProcessIngestProtect(1, crudOperations);

            AssertData(crudOperations.ItemsToUpdate[0].EpgCbObjects[0], "1 ENG New Name", "1 ENG New Description", "1 ENG New Meta1", "1 ENG New Meta2","1 ENG New Tag1", "1 ENG New Tag2");
            AssertData(crudOperations.ItemsToUpdate[0].EpgCbObjects[1], "1 ARB New Name", "1 ARB New Description", "1 ARB New Meta1", "1 ARB New Meta2","1 ARB New Tag1", "1 ARB New Tag2");
            AssertData(crudOperations.ItemsToUpdate[0].EpgCbObjects[2], "1 CSP New Name", "1 CSP New Description", "1 CSP New Meta1", "1 CSP New Meta2","1 CSP New Tag1", "1 CSP New Tag2");
        }

        [Test]
        public void ProcessIngestProtect_ProtectedTopicsFound_ReturnsChangedItemsToUpdate()
        {
            _catalogManagerAdapterMock
                .Setup(x => x.DoesGroupUsesTemplates(1))
                .Returns(true);
            _catalogManagerAdapterMock
                .Setup(x => x.GetCatalogGroupCache(1))
                .Returns(BuildCatalogGroupCache());
            _epgDalMock
                .Setup(x => x.GetEpgDocs(It.Is<IEnumerable<string>>(_ => _.SequenceEqual(new[] { "bulkId_eng_doc1", "bulkId_arb_doc1", "bulkId_csp_doc1" })), true))
                .Returns(BuildOldEpgs());
            var crudOperations = BuildCrudOperations();
            var processor = new IngestProtectProcessor(_epgDalMock.Object, _catalogManagerAdapterMock.Object);

            processor.ProcessIngestProtect(1, crudOperations);

            AssertData(crudOperations.ItemsToUpdate[0].EpgCbObjects[0], "1 ENG Old Name", "1 ENG New Description", "1 ENG Old Meta1", "1 ENG New Meta2", "1 ENG Old Tag1", "1 ENG New Tag2");
            AssertData(crudOperations.ItemsToUpdate[0].EpgCbObjects[1], "1 ARB New Name", "1 ARB New Description", "1 ARB New Meta1", "1 ARB New Meta2", "1 ARB New Tag1", "1 ARB New Tag2");
            AssertData(crudOperations.ItemsToUpdate[0].EpgCbObjects[2], "1 CSP Old Name", "1 CSP New Description", "1 CSP Old Meta1", "1 CSP New Meta2", "1 CSP Old Tag1", "1 CSP New Tag2");
        }

        private void AssertData(EpgCB epgDoc, string expectedName, string expectedDescription, string expectedMeta1, string expectedMeta2, string expectedTag1, string expectedTag2)
        {
            Assert.AreEqual(expectedName, epgDoc.Name);
            Assert.AreEqual(expectedDescription, epgDoc.Description);
            Assert.AreEqual(expectedMeta1, epgDoc.Metas["Meta1"][0]);
            Assert.AreEqual(expectedMeta2, epgDoc.Metas["Meta2"][0]);
            Assert.AreEqual(expectedTag1, epgDoc.Tags["Tag1"][0]);
            Assert.AreEqual(expectedTag2, epgDoc.Tags["Tag2"][0]);
        }

        private CRUDOperations<EpgProgramBulkUploadObject> BuildCrudOperations()
        {
            var cbDocumentIds = new Dictionary<string, string>
            {
                { "eng", "bulkId_eng_doc1" },
                { "arb", "bulkId_arb_doc1" },
                { "csp", "bulkId_csp_doc1" }
            };

            var updateBulkUploadObject1 = new EpgProgramBulkUploadObject
            {
                EpgCbObjects = new List<EpgCB> { BuildEpg(1, "eng", "New"), BuildEpg(1, "arb", "New"), BuildEpg(1, "csp", "New") },
                CbDocumentIdsMap = cbDocumentIds
            };

            var operations = new CRUDOperations<EpgProgramBulkUploadObject>
            {
                ItemsToUpdate = new List<EpgProgramBulkUploadObject> { updateBulkUploadObject1 }
            };

            return operations;
        }

        private IEnumerable<EpgCB> BuildOldEpgs()
        {
            return new[]
            {
                BuildEpg(1, "eng", "Old"), BuildEpg(1, "csp", "Old")
            };
        }

        private EpgCB BuildEpg(long id, string language, string suffix)
        {
            var prefix = $"{id} {language.ToUpper()} {suffix}";
            var epgDoc = new EpgCB
            {
                Language = language,
                Name = $"{prefix} Name",
                Description = $"{prefix} Description",
                Metas = new Dictionary<string, List<string>> { { "Meta1", new List<string> { $"{prefix} Meta1" } }, { "Meta2", new List<string> { $"{prefix} Meta2" } } },
                Tags = new Dictionary<string, List<string>> { { "Tag1", new List<string> { $"{prefix} Tag1" } }, { "Tag2", new List<string> { $"{prefix} Tag2" } } }
            };

            return epgDoc;
        }

        private CatalogGroupCache BuildCatalogGroupCache(bool hasProgramStruct = true, bool hasProtectedMetasAndTags = true, bool hasTopics = true)
        {
            var catalogGroupCache = new CatalogGroupCache();

            if (hasProgramStruct)
            {
                var programStruct = new AssetStruct();
                programStruct.AssetStructMetas.Add(1001, new AssetStructMeta { MetaId = 1001, ProtectFromIngest = hasProtectedMetasAndTags ? true : (bool?)null }); // Name
                programStruct.AssetStructMetas.Add(1002, new AssetStructMeta { MetaId = 1002, ProtectFromIngest = hasProtectedMetasAndTags ? true : (bool?)null }); // Meta1
                programStruct.AssetStructMetas.Add(1003, new AssetStructMeta { MetaId = 1003, ProtectFromIngest = hasProtectedMetasAndTags }); // Tag1
                programStruct.AssetStructMetas.Add(1004, new AssetStructMeta { MetaId = 1004, ProtectFromIngest = null }); // Description
                programStruct.AssetStructMetas.Add(1005, new AssetStructMeta { MetaId = 1005, ProtectFromIngest = null }); // Meta2
                programStruct.AssetStructMetas.Add(1006, new AssetStructMeta { MetaId = 1006, ProtectFromIngest = false }); // Tag2
                programStruct.AssetStructMetas.Add(1007, new AssetStructMeta { MetaId = 1007, ProtectFromIngest = hasProtectedMetasAndTags }); // SomeField
                programStruct.AssetStructMetas.Add(1008, new AssetStructMeta { MetaId = 1008, ProtectFromIngest = hasProtectedMetasAndTags }); // SomeMeta
                programStruct.AssetStructMetas.Add(1009, new AssetStructMeta { MetaId = 1009, ProtectFromIngest = hasProtectedMetasAndTags }); // SomeTag

                catalogGroupCache.AssetStructsMapById.Add(0, programStruct);
            }

            if (hasTopics)
            {
                catalogGroupCache.TopicsMapById.Add(1001, new Topic { SystemName = "Name" });
                catalogGroupCache.TopicsMapById.Add(1002, new Topic { SystemName = "Meta1" });
                catalogGroupCache.TopicsMapById.Add(1003, new Topic { SystemName = "Tag1" });
                catalogGroupCache.TopicsMapById.Add(1004, new Topic { SystemName = "Description" });
                catalogGroupCache.TopicsMapById.Add(1005, new Topic { SystemName = "Meta2" });
                catalogGroupCache.TopicsMapById.Add(1006, new Topic { SystemName = "Tag2" });
                catalogGroupCache.TopicsMapById.Add(1007, new Topic { SystemName = "SomeField" });
                catalogGroupCache.TopicsMapById.Add(1008, new Topic { SystemName = "SomeMeta" });
                catalogGroupCache.TopicsMapById.Add(1009, new Topic { SystemName = "SomeTag" });
            }

            return catalogGroupCache;
        }
    }
}