using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Catalog;
using Core.Catalog;
using Core.GroupManagers;
using TvinciCache;
using TvinciCache.Adapters;

namespace ApiLogic.Catalog.CatalogManagement.Helpers
{
    public class EpgAssetMultilingualMutator : IEpgAssetMultilingualMutator
    {
        private readonly ICatalogPartnerConfigManager _catalogPartnerConfigManager;
        private readonly IGroupSettingsManager _groupSettingsManager;
        private readonly IGroupsFeatures _groupsFeatures;

        private static readonly Lazy<IEpgAssetMultilingualMutator> InternalEpgAssetMultilingualMutator = new Lazy<IEpgAssetMultilingualMutator>(
            () => new EpgAssetMultilingualMutator(CatalogPartnerConfigManager.Instance, GroupSettingsManager.Instance, GroupsFeatureAdapter.Instance));

        public EpgAssetMultilingualMutator(ICatalogPartnerConfigManager catalogPartnerConfigManager, IGroupSettingsManager groupSettingsManager, IGroupsFeatures groupsFeatures)
        {
            _catalogPartnerConfigManager = catalogPartnerConfigManager;
            _groupSettingsManager = groupSettingsManager;
            _groupsFeatures = groupsFeatures;
        }

        public static IEpgAssetMultilingualMutator Instance => InternalEpgAssetMultilingualMutator.Value;

        public bool IsAllowedToFallback(int groupId, IDictionary<string, LanguageObj> languages)
        {
            if (languages.Count <= 1)
            {
                return false;
            }

            if (!_groupSettingsManager.DoesGroupUsesTemplates(groupId))
            {
                return false;
            }

            var epgFeatureVersion = _groupSettingsManager.GetEpgFeatureVersion(groupId);
            if (epgFeatureVersion == EpgFeatureVersion.V1)
            {
                return false;
            }

            var catalogConfigResponse = _catalogPartnerConfigManager.GetCatalogConfig(groupId);
            if (!catalogConfigResponse.HasObject())
            {
                return false;
            }

            if (!catalogConfigResponse.Object.EpgMultilingualFallbackSupport.GetValueOrDefault())
            {
                return false;
            }

            return true;
        }

        public void PrepareEpgAsset(int groupId, EpgAsset epgAsset, LanguageObj defaultLanguage, IDictionary<string, LanguageObj> languageMapByCode)
        {
            if (!IsAllowedToFallback(groupId, languageMapByCode))
            {
                return;
            }

            var languageCodesWithoutDefault = languageMapByCode.Where(l => l.Key != defaultLanguage.Code).Select(v => v.Value.Code).ToHashSet();

            epgAsset.NamesWithLanguages =
                FallbackMultilingualLanguageContainers(epgAsset.NamesWithLanguages, epgAsset.Name, languageCodesWithoutDefault).ToList();
            epgAsset.DescriptionsWithLanguages =
                FallbackMultilingualLanguageContainers(epgAsset.DescriptionsWithLanguages, epgAsset.Description, languageCodesWithoutDefault, true)
                    .ToList();

            FallbackMultilingualMetas(epgAsset.Metas, languageCodesWithoutDefault);
            FallbackMultilingualTags(epgAsset.Tags, languageCodesWithoutDefault);
        }

        private static void FallbackMultilingualMetas(IEnumerable<Metas> metas, IReadOnlyCollection<string> languageCodesWithoutDefault)
        {
            foreach (var meta in metas)
            {
                FallbackMeta(meta, languageCodesWithoutDefault);
            }
        }

        private static void FallbackMeta(Metas meta, IReadOnlyCollection<string> languageCodesWithoutDefault)
        {
            meta.Value = FallbackMultilingualLanguageContainers(meta.Value, meta.m_sValue, languageCodesWithoutDefault);
        }
        
        private static void FallbackMultilingualTags(IEnumerable<Tags> tags, IReadOnlyCollection<string> languageCodesWithoutDefault)
        {
            foreach (var tag in tags)
            {
                FallbackTag(tag, languageCodesWithoutDefault);
            }
        }

        private static void FallbackTag(Tags tag, IReadOnlyCollection<string> languageCodesWithoutDefault)
        {
            if (tag.m_lValues == null || tag.m_lValues.Count == 0)
            {
                return;
            }

            if (tag.Values == null)
            {
                tag.Values = new List<LanguageContainer[]>();
            }

            var tagValuesToAdd = tag.m_lValues
                .Select((t, index) => FallbackMultilingualLanguageContainers(tag.Values.ElementAtOrDefault(index), t, languageCodesWithoutDefault))
                .ToList();

            tag.Values = tagValuesToAdd;
        }

        private static LanguageContainer[] FallbackMultilingualLanguageContainers(
            IEnumerable<LanguageContainer> languageContainers,
            string defaultLanguageValue,
            IReadOnlyCollection<string> languageCodesWithoutDefault,
            bool replaceNullValueWithDefault = false)
        {
            var containers = languageContainers ?? Enumerable.Empty<LanguageContainer>();
            var existingLanguages = !replaceNullValueWithDefault
                ? containers.Select(lc => lc.m_sLanguageCode3)
                : containers.Where(x => x.m_sValue != null).Select(lc => lc.m_sLanguageCode3);
            var languagesToAdd = !string.IsNullOrEmpty(defaultLanguageValue) ? languageCodesWithoutDefault.Except(existingLanguages) : Enumerable.Empty<string>();
            var languageContainersToReAdd = containers.Where(x => !languagesToAdd.Contains(x.m_sLanguageCode3));
            return languagesToAdd.Select(l => new LanguageContainer(l, defaultLanguageValue)).Concat(languageContainersToReAdd).ToArray();
        }
    }
}