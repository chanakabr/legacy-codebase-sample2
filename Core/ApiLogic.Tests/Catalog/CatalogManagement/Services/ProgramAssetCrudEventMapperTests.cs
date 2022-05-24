using System;
using System.Collections.Generic;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiObjects;
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
                Operation = 1,
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
                SummedCatchUpBuffer = 10,
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
            actualResult.MediaType.Should().Be(new MediaType { m_sTypeName = LiveToVodService.LIVE_TO_VOD_ASSET_STRUCT_SYSTEM_NAME });
            actualResult.Metas.Should().BeNull();
            actualResult.Tags.Should().BeNull();
            actualResult.EntryId.Should().BeEmpty();
            // l2v properties
            actualResult.EpgId.Should().Be(programAsset.Id);
            actualResult.EpgChannelId.Should().Be(liveAsset.EpgChannelId);
            actualResult.EpgIdentifier.Should().Be(programAsset.EpgId);
            actualResult.Crid.Should().Be(programAsset.Crid);
            actualResult.PaddingAfterProgramEnds.Should().Be(liveAsset.PaddingAfterProgramEnds);
            actualResult.PaddingBeforeProgramStarts.Should().Be(liveAsset.SummedPaddingBeforeProgramStarts);
        }
    }
}