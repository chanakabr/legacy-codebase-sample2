using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects;
using ApiObjects.Catalog;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Phoenix.Generated.Api.Events.Crud.ProgramAsset;
using TVinciShared;
using Image = Core.Catalog.Image;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class ProgramAssetCrudEventMapper : IProgramAssetCrudEventMapper
    {
        private readonly ICatalogManager _catalogManager;
        private static readonly Lazy<IProgramAssetCrudEventMapper> Lazy = new Lazy<IProgramAssetCrudEventMapper>(
            () => new ProgramAssetCrudEventMapper(CatalogManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IProgramAssetCrudEventMapper Instance => Lazy.Value;

        public ProgramAssetCrudEventMapper(ICatalogManager catalogManager)
        {
            _catalogManager = catalogManager;
        }

        public LiveToVodAsset MapToAssetForAdd(ProgramAsset programAsset, LiveAsset liveAsset, int retentionPeriodInDays)
        {
            var assetToAdd = new LiveToVodAsset
            {
                AssetType = eAssetTypes.MEDIA,
                MediaAssetType = MediaAssetType.Media,
                Name = programAsset.Name,
                Description = programAsset.Description,
                CoGuid = $"kaltura_{Guid.NewGuid()}",
                NamesWithLanguages = programAsset.MultilingualName?.Select(Map).ToList(),
                DescriptionsWithLanguages = programAsset.MultilingualDescription?.Select(Map).ToList(),
                IsActive = false,
                MediaType = new MediaType { m_sTypeName = LiveToVodService.LIVE_TO_VOD_ASSET_STRUCT_SYSTEM_NAME },
                Metas = BuildLiveToVodMetas(programAsset.PartnerId, programAsset.Metas),
                Tags = programAsset.Tags?.Select(Map).ToList() ?? Enumerable.Empty<Tags>().ToList(),
                EntryId = string.Empty,
                // l2v properties
                EpgId = programAsset.Id,
                EpgChannelId = programAsset.EpgChannelId,
                EpgIdentifier = programAsset.EpgId,
                Crid = programAsset.Crid,
                LinearAssetId = programAsset.LinearAssetId,
                PaddingBeforeProgramStarts = liveAsset.PaddingBeforeProgramStarts.Value,
                PaddingAfterProgramEnds = liveAsset.PaddingAfterProgramEnds.Value
            };

            SetCalculatedDates(assetToAdd, programAsset, liveAsset, retentionPeriodInDays);

            return assetToAdd;
        }

        public LiveToVodAsset MapToAssetForUpdate(
            ProgramAsset programAsset,
            LiveAsset liveAsset,
            LiveToVodAsset currentAsset,
            int retentionPeriodInDays)
        {
            var assetToUpdate = new LiveToVodAsset
            {
                AssetType = eAssetTypes.MEDIA,
                MediaAssetType = MediaAssetType.Media,
                Name = programAsset.Name,
                Description = programAsset.Description,
                NamesWithLanguages = programAsset.MultilingualName?.Select(Map).ToList(),
                DescriptionsWithLanguages = programAsset.MultilingualDescription.Select(Map).ToList(),
                Metas = BuildLiveToVodMetas(programAsset.PartnerId, programAsset.Metas),
                Tags = programAsset.Tags?.Select(Map).ToList() ?? Enumerable.Empty<Tags>().ToList(),
                CoGuid = currentAsset.CoGuid,
                // l2v properties to update
                Crid = programAsset.Crid,
                PaddingBeforeProgramStarts = liveAsset.PaddingBeforeProgramStarts.Value,
                PaddingAfterProgramEnds = liveAsset.PaddingAfterProgramEnds.Value
            };

            SetCalculatedDates(assetToUpdate, programAsset, liveAsset, retentionPeriodInDays);

            return assetToUpdate;
        }

        public Image Map(Phoenix.Generated.Api.Events.Crud.ProgramAsset.Image image)
            => new Image
            {
                ImageTypeId = image.ImageTypeId,
                ContentId = image.ContentId,
                Status = Map(image.Status),
                Version = (int)image.Version,
                SourceUrl = image.SourceUrl
            };
        
        public ProgramAsset Map(EpgAsset epgAsset, LiveAsset liveAsset, long groupId, long updaterId, long operation)
        {
            var expirationDate = epgAsset.CatchUpEnabled == true
                ? epgAsset.EndDate?.AddMinutes(liveAsset.SummedCatchUpBuffer)
                : epgAsset.EndDate;
            
            return new ProgramAsset
            {
                Id = epgAsset.Id,
                Name = epgAsset.Name,
                MultilingualName = epgAsset.NamesWithLanguages?.Select(Map).ToArray()
                    ?? Enumerable.Empty<TranslationValue>().ToArray(),
                Description = epgAsset.Description,
                MultilingualDescription = epgAsset.DescriptionsWithLanguages?.Select(Map).ToArray()
                    ?? Enumerable.Empty<TranslationValue>().ToArray(),
                Metas = epgAsset.Metas?.Select(Map).ToArray() ?? Enumerable.Empty<AssetMeta>().ToArray(),
                Tags = epgAsset.Tags?.Select(Map).ToArray() ?? Enumerable.Empty<AssetTag>().ToArray(),
                Crid = epgAsset.Crid,
                LinearAssetId = epgAsset.LinearAssetId.Value,
                EpgChannelId = epgAsset.EpgChannelId.Value,
                EpgId = epgAsset.EpgIdentifier,
                Images = epgAsset.Images?.Select(Map).ToArray()
                    ?? Enumerable.Empty<Phoenix.Generated.Api.Events.Crud.ProgramAsset.Image>().ToArray(),
                EndDate = epgAsset.EndDate.Value.ToUtcUnixTimestampSeconds(),
                ExpirationDate = expirationDate?.ToUtcUnixTimestampSeconds(),
                ExternalOfferIds = (epgAsset.ExternalOfferIds ?? new List<string>(0)).ToArray(),
                PartnerId = groupId,
                UpdaterId = updaterId,
                StartDate = epgAsset.StartDate.Value.ToUtcUnixTimestampSeconds(),
                Operation = operation
            };
        }
        
        private static Phoenix.Generated.Api.Events.Crud.ProgramAsset.Image Map(Image source)
            => new Phoenix.Generated.Api.Events.Crud.ProgramAsset.Image
            {
                ImageObjectId = source.ImageObjectId,
                Status = Map(source.Status),
                ContentId = source.ContentId,
                Version = source.Version,
                ImageTypeId = source.ImageTypeId,
                SourceUrl = source.SourceUrl
            };

        private static Status Map(eTableStatus source)
        {
            switch (source)
            {
                case eTableStatus.Pending:
                    return Status.Pending;
                case eTableStatus.OK:
                    return Status.Ready;
                case eTableStatus.Failed:
                    return Status.Failed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }

        private static AssetTag Map(Tags source)
            => new AssetTag
            {
                Name = source.m_oTagMeta.m_sName,
                Values = source.Values.Select(Map).ToArray()
            };

        private static AssetTagValue Map(IEnumerable<LanguageContainer> translations)
        {
            var result = new AssetTagValue();
            var resultTranslations = new List<TranslationValue>();
            foreach (var translation in translations)
            {
                if (translation.IsDefault)
                {
                    result.Value = translation.m_sValue;
                }
                else
                {
                    resultTranslations.Add(Map(translation));
                }
            }
            
            result.Translations = resultTranslations.ToArray();
            
            return result;
        }

        private static AssetMeta Map(Metas source)
            => new AssetMeta
            {
                Type = source.m_oTagMeta.m_sType,
                Name = source.m_oTagMeta.m_sName,
                Value = source.m_sValue,
                Translations = source.Value?.Where(x => !x.IsDefault).Select(Map).ToArray()
            };

        private static TranslationValue Map(LanguageContainer source)
            => new TranslationValue
            {
                Language = source.m_sLanguageCode3,
                Value = source.m_sValue
            };

        private static eTableStatus Map(Status source)
        {
            switch (source)
            {
                case Status.Ready:
                    return eTableStatus.OK;
                case Status.Failed:
                    return eTableStatus.Failed;
                default:
                    return eTableStatus.Pending;
            }
        }

        private List<Metas> BuildLiveToVodMetas(long partnerId, IEnumerable<AssetMeta> sourceMetas)
        {
            if (sourceMetas == null
                || !_catalogManager.TryGetCatalogGroupCacheFromCache((int)partnerId, out var cache)
                || !cache.AssetStructsMapBySystemName.TryGetValue(
                    LiveToVodService.LIVE_TO_VOD_ASSET_STRUCT_SYSTEM_NAME, out var liveToVodAssetStruct))
            {
                return new List<Metas>();
            }

            var liveToVodTopics = liveToVodAssetStruct.MetaIds
                .Where(x => cache.TopicsMapById.ContainsKey(x))
                .Select(x => cache.TopicsMapById[x]);
            // Remove metas duplicated by system name inside l2v asset struct.
            var uniqueTopicsBySystemName = liveToVodTopics
                .GroupBy(x => x.SystemName, StringComparer.InvariantCultureIgnoreCase)
                .Where(x => x.Count() == 1)
                .ToDictionary(x => x.Key, y => y.Single(), StringComparer.OrdinalIgnoreCase);
            // Remove metas duplicated by system name inside program asset event.
            // It will guarantee 1 to 1 mapping by system name between program asset event metas and l2v asset struct metas.
            var uniqueSourceMetasByName = sourceMetas
                .GroupBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase)
                .Where(x => x.Count() == 1)
                .Select(x => x.Single());

            return uniqueSourceMetasByName
                .Where(x => uniqueTopicsBySystemName.ContainsKey(x.Name))
                .Select(x => Map(x, uniqueTopicsBySystemName[x.Name].Type))
                .ToList();
        }

        private static void SetCalculatedDates(
            LiveToVodAsset liveToVodAsset,
            ProgramAsset epgAsset,
            LiveAsset liveAsset,
            int retentionPeriodInDays)
        {
            var catchupBufferInSeconds = liveAsset.SummedCatchUpBuffer * 60;
            var catalogStartDate = epgAsset.StartDate + catchupBufferInSeconds;
            var catalogEndDate = catalogStartDate + (long)TimeSpan.FromDays(retentionPeriodInDays).TotalSeconds;
            var originalStartDate = epgAsset.StartDate - liveAsset.PaddingBeforeProgramStarts.Value;
            var originalEndDate = epgAsset.EndDate + liveAsset.PaddingAfterProgramEnds.Value;
            liveToVodAsset.CatalogStartDate = DateUtils.UtcUnixTimestampSecondsToDateTime(catalogStartDate);
            liveToVodAsset.FinalEndDate = DateUtils.UtcUnixTimestampSecondsToDateTime(catalogEndDate);
            liveToVodAsset.StartDate = DateUtils.UtcUnixTimestampSecondsToDateTime(catalogStartDate);
            liveToVodAsset.EndDate = DateUtils.UtcUnixTimestampSecondsToDateTime(catalogEndDate);
            // l2v properties
            liveToVodAsset.OriginalStartDate = DateUtils.UtcUnixTimestampSecondsToDateTime(originalStartDate);
            liveToVodAsset.OriginalEndDate = DateUtils.UtcUnixTimestampSecondsToDateTime(originalEndDate);
        }

        private static Tags Map(AssetTag source)
            => new Tags
            {
                m_oTagMeta = new TagMeta(source.Name, MetaType.Tag.ToString()),
                m_lValues = source.Values.Select(x => x.Value).ToList()
            };

        private static Metas Map(AssetMeta source, MetaType correctMetaType)
        {
            var value = source.Value;
            // The logic below is motivated by difference of boolean meta indexing between programs and medias.
            // ES indexes programs with true/false boolean meta values without issues, but throws an error on indexing
            // medias with such values. Therefore we convert true/false to 1/0 (just like Phoenix does) to avoid that issue.
            if (correctMetaType == MetaType.Bool && bool.TryParse(value, out var metaBoolValue))
            {
                value = Convert.ToInt32(metaBoolValue).ToString();
            }

            return new Metas
            {
                m_sValue = value,
                m_oTagMeta = new TagMeta
                {
                    m_sName = source.Name,
                    m_sType = correctMetaType.ToString()
                },
                Value = source.Translations?.Select(Map).ToArray()
            };
        }

        private static LanguageContainer Map(TranslationValue source)
            => new LanguageContainer
            {
                m_sValue = source.Value,
                m_sLanguageCode3 = source.Language
            };
    }
}