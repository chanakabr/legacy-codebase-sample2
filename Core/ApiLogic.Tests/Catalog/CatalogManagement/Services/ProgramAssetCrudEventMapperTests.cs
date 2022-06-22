using System;
using System.Collections.Generic;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiObjects;
using ApiObjects.Catalog;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Phoenix.Generated.Api.Events.Crud.ProgramAsset;
using TVinciShared;
using eAssetTypes = AdapterControllers.RecommendationEngineAdapter.eAssetTypes;

namespace ApiLogic.Tests.Catalog.CatalogManagement.Services
{
    public class ProgramAssetCrudEventMapperTests
    {
        private Mock<ICatalogManager> _catalogManagerMock;
        private MockRepository _mockRepository;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _catalogManagerMock = _mockRepository.Create<ICatalogManager>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void MapToAssetForAdd_BasicFieldsMapping()
        {
            var startDate = new DateTime(2022, 1, 1);
            var programAsset = new ProgramAsset
            {
                Name = "movie name",
                Description = "movie description",
                MultilingualName = new[] { new TranslationValue { Language = "fr", Value = "name in fr" } },
                MultilingualDescription = new[] { new TranslationValue { Language = "fr", Value = "desc in fr" } },
                Id = 1,
                LinearAssetId = 2,
                Crid = "crid",
                EpgId = "epg_id",
                EpgChannelId = 3,
                StartDate = startDate.ToUtcUnixTimestampSeconds(),
                EndDate = startDate.AddMinutes(30).ToUtcUnixTimestampSeconds()
            };

            var liveAsset = new LiveAsset
            {
                SummedCatchUpBuffer = 60,
                PaddingBeforeProgramStarts = 20,
                PaddingAfterProgramEnds = 30
            };

            const int retentionPeriodInDays = 3;
            var mapper = new ProgramAssetCrudEventMapper(_catalogManagerMock.Object);
            var actualResult = mapper.MapToAssetForAdd(programAsset, liveAsset, retentionPeriodInDays);

            actualResult.Should().NotBeNull();
            actualResult.AssetType.Should().Be(eAssetTypes.MEDIA);
            actualResult.MediaAssetType.Should().Be(MediaAssetType.Media);
            actualResult.Name.Should().Be(programAsset.Name);
            actualResult.Description.Should().Be(programAsset.Description);
            actualResult.NamesWithLanguages.Should().BeEquivalentTo(new List<LanguageContainer>
            {
                new LanguageContainer { IsDefault = false, m_sValue = "name in fr", m_sLanguageCode3 = "fr" }
            });
            actualResult.DescriptionsWithLanguages.Should().BeEquivalentTo(new List<LanguageContainer>
            {
                new LanguageContainer { IsDefault = false, m_sValue = "desc in fr", m_sLanguageCode3 = "fr" }
            });
            actualResult.IsActive.Should().Be(false);
            actualResult.MediaType.Should().BeEquivalentTo(new MediaType
                { m_sTypeName = LiveToVodService.LIVE_TO_VOD_ASSET_STRUCT_SYSTEM_NAME });
            actualResult.Metas.Should().BeEmpty();
            actualResult.Tags.Should().BeEmpty();
            actualResult.EntryId.Should().BeEmpty();
            actualResult.EpgId.Should().Be(programAsset.Id);
            actualResult.EpgChannelId.Should().Be(programAsset.EpgChannelId);
            actualResult.EpgIdentifier.Should().Be(programAsset.EpgId);
            actualResult.Crid.Should().Be(programAsset.Crid);
            actualResult.PaddingAfterProgramEnds.Should().Be(liveAsset.PaddingAfterProgramEnds);
            actualResult.PaddingBeforeProgramStarts.Should().Be(liveAsset.PaddingBeforeProgramStarts);
            actualResult.CatalogStartDate.Should().Be(new DateTime(2022, 1, 1, 1, 0, 0));
            actualResult.FinalEndDate.Should().Be(new DateTime(2022, 1, 4, 1, 0, 0));
            actualResult.StartDate.Should().Be(new DateTime(2022, 1, 1, 1, 0, 0));
            actualResult.EndDate.Should().Be(new DateTime(2022, 1, 4, 1, 0, 0));
            actualResult.OriginalStartDate.Should().Be(new DateTime(2021, 12, 31, 23, 59, 40));
            actualResult.OriginalEndDate.Should().Be(new DateTime(2022, 1, 1, 0, 30, 30));
        }

        [Test]
        public void MapToAssetForUpdate_BasicFieldsMapping()
        {
            var existingItem = new LiveToVodAsset
            {
                CoGuid = Guid.NewGuid().ToString("N")
            };

            var startDate = new DateTime(2022, 1, 1);
            var programAsset = new ProgramAsset
            {
                Name = "movie name",
                Description = "movie description",
                MultilingualName = new[] { new TranslationValue { Language = "fr", Value = "name in fr" } },
                MultilingualDescription = new[] { new TranslationValue { Language = "fr", Value = "desc in fr" } },
                Id = 1,
                LinearAssetId = 2,
                Crid = "crid",
                EpgId = "epg_id",
                EpgChannelId = 3,
                StartDate = startDate.ToUtcUnixTimestampSeconds(),
                EndDate = startDate.AddMinutes(30).ToUtcUnixTimestampSeconds()
            };

            var liveAsset = new LiveAsset
            {
                SummedCatchUpBuffer = 60,
                PaddingBeforeProgramStarts = 20,
                PaddingAfterProgramEnds = 30
            };

            const int retentionPeriodInDays = 3;
            var mapper = new ProgramAssetCrudEventMapper(_catalogManagerMock.Object);
            var actualResult = mapper.MapToAssetForUpdate(programAsset, liveAsset, existingItem, retentionPeriodInDays);

            actualResult.Should().NotBeNull();
            actualResult.AssetType.Should().Be(eAssetTypes.MEDIA);
            actualResult.MediaAssetType.Should().Be(MediaAssetType.Media);
            actualResult.Name.Should().Be(programAsset.Name);
            actualResult.Description.Should().Be(programAsset.Description);
            actualResult.NamesWithLanguages.Should().BeEquivalentTo(new List<LanguageContainer>
            {
                new LanguageContainer { IsDefault = false, m_sValue = "name in fr", m_sLanguageCode3 = "fr" }
            });
            actualResult.DescriptionsWithLanguages.Should().BeEquivalentTo(new List<LanguageContainer>
            {
                new LanguageContainer { IsDefault = false, m_sValue = "desc in fr", m_sLanguageCode3 = "fr" }
            });
            actualResult.CoGuid.Should().Be(existingItem.CoGuid);
            actualResult.Metas.Should().BeEmpty();
            actualResult.Tags.Should().BeEmpty();
            actualResult.Crid.Should().Be(programAsset.Crid);
            actualResult.PaddingAfterProgramEnds.Should().Be(liveAsset.PaddingAfterProgramEnds);
            actualResult.PaddingBeforeProgramStarts.Should().Be(liveAsset.PaddingBeforeProgramStarts);
            actualResult.CatalogStartDate.Should().Be(new DateTime(2022, 1, 1, 1, 0, 0));
            actualResult.FinalEndDate.Should().Be(new DateTime(2022, 1, 4, 1, 0, 0));
            actualResult.StartDate.Should().Be(new DateTime(2022, 1, 1, 1, 0, 0));
            actualResult.EndDate.Should().Be(new DateTime(2022, 1, 4, 1, 0, 0));
            actualResult.OriginalStartDate.Should().Be(new DateTime(2021, 12, 31, 23, 59, 40));
            actualResult.OriginalEndDate.Should().Be(new DateTime(2022, 1, 1, 0, 30, 30));
        }

        [Test]
        public void MapToAssetForAdd_MetasMapping()
        {
            var expectedMetaValues = new List<Metas>
            {
                new Metas
                {
                    m_sValue = "1",
                    m_oTagMeta = new TagMeta { m_sName = "topic_1", m_sType = MetaType.Number.ToString() }
                },
                new Metas
                {
                    m_sValue = "topic 2 value",
                    m_oTagMeta = new TagMeta { m_sName = "topic_2", m_sType = MetaType.MultilingualString.ToString() },
                    Value = new[]
                        { new LanguageContainer { m_sLanguageCode3 = "fr", m_sValue = "topic 2 value in fr" } }
                },
                new Metas
                {
                    m_sValue = "5",
                    m_oTagMeta = new TagMeta { m_sName = "topic_5", m_sType = MetaType.Number.ToString() }
                }
            };

            var liveAsset = new LiveAsset
            {
                SummedCatchUpBuffer = 3600,
                PaddingBeforeProgramStarts = 20,
                PaddingAfterProgramEnds = 30
            };

            var programAsset = new ProgramAsset
            {
                PartnerId = 1,
                Metas = new[]
                {
                    new AssetMeta { Name = "topic_1", Type = "number", Value = "1" },
                    new AssetMeta
                    {
                        Name = "topic_2",
                        Type = "multilingual",
                        Value = "topic 2 value",
                        Translations = new[] { new TranslationValue { Language = "fr", Value = "topic 2 value in fr" } }
                    },
                    new AssetMeta { Name = "topic_3", Type = "number", Value = "3" },
                    new AssetMeta { Name = "topic_3", Type = "string", Value = "topic 4 value" },
                    new AssetMeta { Name = "topic_5", Type = "string", Value = "5" }
                }
            };

            var cache = new CatalogGroupCache
            {
                TopicsMapById = new Dictionary<long, Topic>
                {
                    { 1, new Topic { SystemName = "topic_1", Type = MetaType.Number, Id = 1 } },
                    { 2, new Topic { SystemName = "topic_2", Type = MetaType.MultilingualString, Id = 2 } },
                    { 3, new Topic { SystemName = "topic_3", Type = MetaType.Number, Id = 3 } },
                    { 4, new Topic { SystemName = "topiC_3", Type = MetaType.String, Id = 4 } },
                    { 5, new Topic { SystemName = "topic_5", Type = MetaType.Number, Id = 5 } }
                },
                AssetStructsMapBySystemName = new Dictionary<string, AssetStruct>
                {
                    {
                        LiveToVodService.LIVE_TO_VOD_ASSET_STRUCT_SYSTEM_NAME,
                        new AssetStruct { MetaIds = new List<long> { 1, 2, 3, 4, 5 } }
                    }
                }
            };

            const int retentionPeriodInDays = 3;

            _catalogManagerMock
                .Setup(x => x.TryGetCatalogGroupCacheFromCache(1, out cache))
                .Returns(true);

            var mapper = new ProgramAssetCrudEventMapper(_catalogManagerMock.Object);
            var actualResult = mapper.MapToAssetForAdd(programAsset, liveAsset, retentionPeriodInDays);

            actualResult.Should().NotBeNull();
            actualResult.Metas.Should().BeEquivalentTo(expectedMetaValues);
        }

        [Test]
        public void MapToAssetForAdd_TagsMapping()
        {
            var expectedTagValues = new List<Tags>
            {
                new Tags
                {
                    m_lValues = new List<string> { "Drama", "Thriller" },
                    m_oTagMeta = new TagMeta { m_sName = "genre", m_sType = MetaType.Tag.ToString() }
                },
                new Tags
                {
                    m_lValues = new List<string> { "Windows" },
                    m_oTagMeta = new TagMeta { m_sName = "os", m_sType = MetaType.Tag.ToString() }
                }
            };

            var liveAsset = new LiveAsset
            {
                SummedCatchUpBuffer = 3600,
                PaddingBeforeProgramStarts = 20,
                PaddingAfterProgramEnds = 30
            };

            var programAsset = new ProgramAsset
            {
                PartnerId = 1,
                Tags = new[]
                {
                    new AssetTag
                    {
                        Name = "genre",
                        Values = new[]
                        {
                            new AssetTagValue
                            {
                                Value = "Drama",
                                Translations = new[] { new TranslationValue { Language = "fr", Value = "Drama in fr" } }
                            },
                            new AssetTagValue
                            {
                                Value = "Thriller",
                                Translations = new[]
                                    { new TranslationValue { Language = "fr", Value = "Thriller in fr" } }
                            }
                        }
                    },
                    new AssetTag
                    {
                        Name = "os",
                        Values = new[]
                        {
                            new AssetTagValue
                            {
                                Value = "Windows",
                                Translations = new[] { new TranslationValue { Language = "fr", Value = "Windows" } }
                            }
                        }
                    }
                }
            };

            const int retentionPeriodInDays = 3;

            var mapper = new ProgramAssetCrudEventMapper(_catalogManagerMock.Object);
            var actualResult = mapper.MapToAssetForAdd(programAsset, liveAsset, retentionPeriodInDays);

            actualResult.Should().NotBeNull();
            actualResult.Tags.Should().BeEquivalentTo(expectedTagValues);
        }
    }
}