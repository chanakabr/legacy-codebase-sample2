using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Epg;
using Core.Catalog;
using DAL;
using Phoenix.Generated.Api.Events.Crud.PartnerMigrationHouseholdRecording;
using Phx.Lib.Appconfig;
using System;
using System.Collections.Generic;
using System.Linq;
using TVinciShared;

namespace ApiLogic.ConditionalAccess.Recordings
{
    public class PartnerMigrationHouseholdRecordingCrudEventMapper
    {
        private static readonly double EXPIRY_DATE_DELTA = ApplicationConfiguration.Current.EPGDocumentExpiry.Value > 0 ? ApplicationConfiguration.Current.EPGDocumentExpiry.Value : 7;

        public static EpgAsset MapToEpgAsset(PartnerMigrationHouseholdRecording householdRecording, long linearAssetId, CatalogGroupCache cache, string name, List<LanguageContainer> nameTranslations)
        {
            var epgAsset = new EpgAsset()
            {
                LinearAssetId = linearAssetId,
                Crid = householdRecording.ProgramAssetCrid,
                CoGuid = householdRecording.ProgramAssetExternalId,
                EpgIdentifier = householdRecording.ProgramAssetExternalId,
                StartDate = DateUtils.UtcUnixTimestampAbsSecondsToDateTime(householdRecording.ProgramAssetStartDateTime),
                EndDate = DateUtils.UtcUnixTimestampAbsSecondsToDateTime(householdRecording.ProgramAssetEndDateTime),
                SearchEndDate = DateTime.UtcNow.AddDays(EXPIRY_DATE_DELTA),
                Metas = MapToMetas(householdRecording.ProgramAssetMetas),
                Tags = MapToTags(householdRecording.ProgramAssetTags),
                Name = name,
                NamesWithLanguages = nameTranslations
            };

            var (description, descriptionTranslations) = GetMultilingual(householdRecording.ProgramAssetmultilingualDescription, cache);
            if (!description.IsNullOrEmpty())
            {
                epgAsset.Description = description;
                epgAsset.DescriptionsWithLanguages = descriptionTranslations;
            }
            return epgAsset;
        }

        public static (string, List<LanguageContainer>) GetMultilingual(TranslationValue[] values, CatalogGroupCache cache)
        {
            if (values == null || values.Length == 0) return (null, null);
            var defaultValue = values.FirstOrDefault(x => x.Language == cache.DefaultLanguage.Code)?.Value;
            var translations = values.Where(x => cache.LanguageMapByCode.ContainsKey(x.Language) && x.Language != cache.DefaultLanguage.Code).ToArray();
            var otherValues = MapToLanguageContainers(translations)?.ToList();
            return (defaultValue, otherValues);
        }

        private static List<Metas> MapToMetas(AssetMeta[] programAssetMetas)
        {
            if (programAssetMetas == null) return null;
            return programAssetMetas.Where(x => IsValidEventObject(x)).Select(x => MapToMeta(x)).ToList();
        }

        private static bool IsValidEventObject(AssetMeta meta)
        {
            return !meta.Name.IsNullOrEmpty() &&
                !meta.Value.IsNullOrEmpty() &&
                !meta.Type.IsNullOrEmpty() &&
                (meta.Translations == null || meta.Translations.All(x => IsValidEventObject(x)));
        }

        private static Metas MapToMeta(AssetMeta assetMeta)
        {
            var meta = new Metas()
            {
                m_sValue = assetMeta.Value,
                m_oTagMeta = new TagMeta()
                {
                    m_sName = assetMeta.Name,
                    m_sType = assetMeta.Type
                },
                Value = MapToLanguageContainers(assetMeta.Translations)
            };

            return meta;
        }

        private static LanguageContainer[] MapToLanguageContainers(TranslationValue[] translations)
        {
            if (translations == null || translations.Length == 0) return null;
            return translations.Where(x => IsValidEventObject(x)).Select(x => MapToLanguageContainer(x)).ToArray();
        }

        private static bool IsValidEventObject(TranslationValue translation)
        {
            return !translation.Language.IsNullOrEmpty() &&
                !translation.Value.IsNullOrEmpty();
        }

        private static LanguageContainer MapToLanguageContainer(TranslationValue translation)
        {
            var languageContainer = new LanguageContainer()
            {
                m_sLanguageCode3 = translation.Language,
                m_sValue = translation.Value
            };

            return languageContainer;
        }

        private static List<Tags> MapToTags(AssetTag[] programAssetTags)
        {
            if (programAssetTags == null) return null;
            return programAssetTags.Where(x => IsValidEventObject(x)).Select(x => MapToTag(x)).ToList();
        }

        private static bool IsValidEventObject(AssetTag assetTag)
        {
            return !assetTag.Name.IsNullOrEmpty() &&
                assetTag.Values != null &&
                assetTag.Values.Length > 0 &&
                assetTag.Values.All(x => IsValidEventObject(x));
        }

        private static bool IsValidEventObject(AssetTagValue assetTagValue)
        {
            return !assetTagValue.Value.IsNullOrEmpty() &&
               assetTagValue.Translations != null &&
               assetTagValue.Translations.Length > 0 &&
               assetTagValue.Translations.All(x => IsValidEventObject(x));
        }

        private static Tags MapToTag(AssetTag assetTag)
        {
            var tag = new Tags()
            {
                m_oTagMeta = new TagMeta()
                {
                    m_sName = assetTag.Name,
                    m_sType = "Tag"
                },
                m_lValues = assetTag.Values.Select(x => x.Value).ToList(),
                Values = assetTag.Values.Select(x => MapToLanguageContainers(x.Translations)).ToList()
            };

            return tag;
        }

        public static IList<EpgPicture> MapToEpgPictures(int groupId, ProgramAssetImage[] programAssetImages, EpgAsset epgAsset)
        {
            if (programAssetImages == null) return null;

            var groupRatioNamesToImageTypes = Core.Catalog.CatalogManagement.ImageManager.GetImageTypesMapBySystemName(groupId);
            return programAssetImages.Where(x => IsValidEventObject(x)).Select(x => MapToEpgPicture(x, epgAsset, groupRatioNamesToImageTypes)).ToList();
        }

        private static EpgPicture MapToEpgPicture(ProgramAssetImage image, EpgAsset epgAsset, Dictionary<string, ImageType> groupRatioNamesToImageTypes)
        {
            var epgPicture = new EpgPicture()
            {
                ImageTypeId = image.ImageTypeId,
                SourceUrl = image.SourceUrl,
                Url = image.SourceUrl,
                Version = (int)image.Version,
                PicHeight = GetValueIfExist(image.Height),
                PicWidth = GetValueIfExist(image.Width),
                PicName = image.ImageTypeName,
                Ratio = image.Ratio,
                IsProgramImage = true,
                EpgProgramId = (int)epgAsset.Id,
                PicID = -1,
                ProgramName = epgAsset.Name,
                ChannelId = GetValueIfExist(epgAsset.EpgChannelId),
                BaseUrl = image.SourceUrl
            };

            if (groupRatioNamesToImageTypes.ContainsKey(epgPicture.Ratio))
            {
                epgPicture.ImageTypeId = groupRatioNamesToImageTypes[epgPicture.Ratio].Id;
                epgPicture.RatioId = GetValueIfExist(groupRatioNamesToImageTypes[epgPicture.Ratio].RatioId);
            }

            return epgPicture;
        }

        private static bool IsValidEventObject(ProgramAssetImage image)
        {
            return image.ImageTypeId > 0 &&
                !image.SourceUrl.IsNullOrEmpty() &&
                image.Version > 0;
        }

        private static int GetValueIfExist(long? value)
        {
            return value.HasValue ? (int)value.Value : 0;
        }

        public static int? GetNullableValueIfExist(long? value)
        {
            return value.HasValue ? (int)value.Value : (int?)null;
        }
    }
}
