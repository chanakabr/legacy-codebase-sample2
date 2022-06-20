using System.Collections.Generic;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiObjects;
using ApiObjects.Catalog;
using Core.Catalog;
using FluentAssertions;
using NUnit.Framework;

namespace ApiLogic.Tests.Catalog.CatalogManagement.Services
{
    [TestFixture]
    public class MediaIngestProtectProcessorTests
    {
        [Test]
        public void ProcessIngestProtect_OldAssetIsNull_DoesNothing()
        {
            var newAsset = BuildMediaAsset("new");
            var catalogGroupCache = BuildCatalogGroupCache();
            var processor = new MediaIngestProtectProcessor();

            processor.ProcessIngestProtect(null, newAsset, catalogGroupCache);

            newAsset.Name.Should().Be("newName");
            newAsset.Description.Should().Be("newDescription");
            newAsset.Metas.Count.Should().Be(2);
            newAsset.Metas[0].m_sValue.Should().Be("newMeta0");
            newAsset.Metas[1].m_sValue.Should().Be("newMeta1");
            newAsset.Tags.Count.Should().Be(2);
            newAsset.Tags[0].m_lValues.Count.Should().Be(1);
            newAsset.Tags[0].m_lValues[0].Should().Be("newTag0");
            newAsset.Tags[1].m_lValues.Count.Should().Be(1);
            newAsset.Tags[1].m_lValues[0].Should().Be("newTag1");
        }

        [Test]
        public void ProcessIngestProtect_AssetStructNotFound_DoesNothing()
        {
            var oldAsset = BuildMediaAsset("old");
            var newAsset = BuildMediaAsset("new");
            var catalogGroupCache = BuildCatalogGroupCache(hasAssetStruct: false);
            var processor = new MediaIngestProtectProcessor();

            processor.ProcessIngestProtect(oldAsset, newAsset, catalogGroupCache);

            newAsset.Name.Should().Be("newName");
            newAsset.Description.Should().Be("newDescription");
            newAsset.Metas.Count.Should().Be(2);
            newAsset.Metas[0].m_sValue.Should().Be("newMeta0");
            newAsset.Metas[1].m_sValue.Should().Be("newMeta1");
            newAsset.Tags.Count.Should().Be(2);
            newAsset.Tags[0].m_lValues.Count.Should().Be(1);
            newAsset.Tags[0].m_lValues[0].Should().Be("newTag0");
            newAsset.Tags[1].m_lValues.Count.Should().Be(1);
            newAsset.Tags[1].m_lValues[0].Should().Be("newTag1");
        }

        [TestCase(false, true)]
        [TestCase(true, false)]
        public void ProcessIngestProtect_ProtectedTopicsNotFound_DoesNothing(bool hasProtectedMetasAndTags, bool hasTopics)
        {
            var oldAsset = BuildMediaAsset("old");
            var newAsset = BuildMediaAsset("new");
            var catalogGroupCache = BuildCatalogGroupCache(hasAssetStruct: true, hasProtectedMetasAndTags, hasTopics);
            var processor = new MediaIngestProtectProcessor();

            processor.ProcessIngestProtect(oldAsset, newAsset, catalogGroupCache);

            newAsset.Name.Should().Be("newName");
            newAsset.Description.Should().Be("newDescription");
            newAsset.Metas.Count.Should().Be(2);
            newAsset.Metas[0].m_sValue.Should().Be("newMeta0");
            newAsset.Metas[1].m_sValue.Should().Be("newMeta1");
            newAsset.Tags.Count.Should().Be(2);
            newAsset.Tags[0].m_lValues.Count.Should().Be(1);
            newAsset.Tags[0].m_lValues[0].Should().Be("newTag0");
            newAsset.Tags[1].m_lValues.Count.Should().Be(1);
            newAsset.Tags[1].m_lValues[0].Should().Be("newTag1");
        }

        [Test]
        public void ProcessIngestProtect_ProtectedTopicsFound_ProtectsNewAsset()
        {
            var oldAsset = BuildMediaAsset("old");
            var newAsset = BuildMediaAsset("new");
            var catalogGroupCache = BuildCatalogGroupCache();
            var processor = new MediaIngestProtectProcessor();

            processor.ProcessIngestProtect(oldAsset, newAsset, catalogGroupCache);

            newAsset.Name.Should().Be("oldName");
            newAsset.Description.Should().Be("newDescription");
            newAsset.Metas.Count.Should().Be(1);
            newAsset.Metas[0].m_sValue.Should().Be("newMeta1");
            newAsset.Tags.Count.Should().Be(1);
            newAsset.Tags[0].m_lValues.Count.Should().Be(1);
            newAsset.Tags[0].m_lValues[0].Should().Be("newTag1");
        }

        private MediaAsset BuildMediaAsset(string prefix)
        {
            var metas = new List<Metas>
            {
                new Metas { m_sValue = $"{prefix}Meta0", m_oTagMeta = new TagMeta("Meta0", "String") },
                new Metas { m_sValue = $"{prefix}Meta1", m_oTagMeta = new TagMeta("Meta1", "String") }
            };

            var tags = new List<Tags>
            {
                new Tags { m_lValues = new List<string> { $"{prefix}Tag0" }, m_oTagMeta = new TagMeta("Tag0", "String") },
                new Tags { m_lValues = new List<string> { $"{prefix}Tag1" }, m_oTagMeta = new TagMeta("Tag1", "String") }
            };

            return new MediaAsset
            {
                Name = $"{prefix}Name",
                Description = $"{prefix}Description",
                Metas = metas,
                Tags = tags
            };
        }

        private CatalogGroupCache BuildCatalogGroupCache(bool hasAssetStruct = true, bool hasProtectedMetasAndTags = true, bool hasTopics = true)
        {
            var catalogGroupCache = new CatalogGroupCache();

            if (hasAssetStruct)
            {
                var assetStruct = new AssetStruct();
                assetStruct.AssetStructMetas.Add(1001, new AssetStructMeta { MetaId = 1001, ProtectFromIngest = hasProtectedMetasAndTags ? true : (bool?)null }); // Name
                assetStruct.AssetStructMetas.Add(1002, new AssetStructMeta { MetaId = 1002, ProtectFromIngest = hasProtectedMetasAndTags ? true : (bool?)null }); // Meta0
                assetStruct.AssetStructMetas.Add(1003, new AssetStructMeta { MetaId = 1003, ProtectFromIngest = hasProtectedMetasAndTags }); // Tag0
                assetStruct.AssetStructMetas.Add(1004, new AssetStructMeta { MetaId = 1004, ProtectFromIngest = null }); // Description
                assetStruct.AssetStructMetas.Add(1005, new AssetStructMeta { MetaId = 1005, ProtectFromIngest = null }); // Meta1
                assetStruct.AssetStructMetas.Add(1006, new AssetStructMeta { MetaId = 1006, ProtectFromIngest = false }); // Tag1
                assetStruct.AssetStructMetas.Add(1007, new AssetStructMeta { MetaId = 1007, ProtectFromIngest = hasProtectedMetasAndTags }); // SomeField
                assetStruct.AssetStructMetas.Add(1008, new AssetStructMeta { MetaId = 1008, ProtectFromIngest = hasProtectedMetasAndTags }); // SomeMeta
                assetStruct.AssetStructMetas.Add(1009, new AssetStructMeta { MetaId = 1009, ProtectFromIngest = hasProtectedMetasAndTags }); // SomeTag

                catalogGroupCache.AssetStructsMapById.Add(0, assetStruct);
            }

            if (hasTopics)
            {
                catalogGroupCache.TopicsMapById.Add(1001, new Topic { SystemName = "Name" });
                catalogGroupCache.TopicsMapById.Add(1002, new Topic { SystemName = "Meta0" });
                catalogGroupCache.TopicsMapById.Add(1003, new Topic { SystemName = "Tag0" });
                catalogGroupCache.TopicsMapById.Add(1004, new Topic { SystemName = "Description" });
                catalogGroupCache.TopicsMapById.Add(1005, new Topic { SystemName = "Meta1" });
                catalogGroupCache.TopicsMapById.Add(1006, new Topic { SystemName = "Tag1" });
                catalogGroupCache.TopicsMapById.Add(1007, new Topic { SystemName = "SomeField" });
                catalogGroupCache.TopicsMapById.Add(1008, new Topic { SystemName = "SomeMeta" });
                catalogGroupCache.TopicsMapById.Add(1009, new Topic { SystemName = "SomeTag" });
            }

            return catalogGroupCache;
        }
    }
}