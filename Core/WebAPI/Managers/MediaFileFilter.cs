using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.Api.Managers.Rule;
using ApiObjects;
using ApiObjects.Rules;
using AutoMapper;
using Core.Catalog.CatalogManagement;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.Managers
{
    public class MediaFileFilter : IMediaFileFilter
    {
        private static readonly Lazy<MediaFileFilter> Lazy = new Lazy<MediaFileFilter>(
            () => new MediaFileFilter(FilterFileRule.Instance, FilterRuleStorage.Instance, FileManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IFilterFileRule _filterFileRule;
        private readonly IFilterRuleStorage _filterRuleStorage;
        private readonly IMediaFileTypeManager _mediaFileTypeManager;

        public static IMediaFileFilter Instance => Lazy.Value;

        public MediaFileFilter(IFilterFileRule filterFileRule, IFilterRuleStorage filterRuleStorage, IMediaFileTypeManager mediaFileTypeManager)
        {
            _filterFileRule = filterFileRule;
            _filterRuleStorage = filterRuleStorage;
            _mediaFileTypeManager = mediaFileTypeManager;
        }

        public void FilterAssetFiles(KalturaAsset asset, int groupId, string sessionCharacteristicKey)
        {
            var fileTypes = new FileManager.FileTypes(groupId, _mediaFileTypeManager);

            var filterRuleCondition = new FilterRuleCondition(groupId, sessionCharacteristicKey);
            var discoveryRules = _filterRuleStorage.GetFilterFileRulesForDiscovery(filterRuleCondition);
            var playbackRules = _filterRuleStorage.GetFilterFileRulesForPlayback(filterRuleCondition);

            FilterMediaFile(asset, fileTypes, discoveryRules, playbackRules);
        }

        public void FilterAssetFiles(IEnumerable<KalturaAsset> assets, int groupId, string sessionCharacteristicKey)
        {
            var fileTypes = new FileManager.FileTypes(groupId, _mediaFileTypeManager);

            var filterRuleCondition = new FilterRuleCondition(groupId, sessionCharacteristicKey);
            var discoveryRules = _filterRuleStorage.GetFilterFileRulesForDiscovery(filterRuleCondition);
            var playbackRules = _filterRuleStorage.GetFilterFileRulesForPlayback(filterRuleCondition);

            foreach (var asset in assets)
            {
                FilterMediaFile(asset, fileTypes, discoveryRules, playbackRules);
            }
        }

        public IEnumerable<KalturaPlaybackSource> GetFilteredAssetFiles(KalturaAssetType assetType, IEnumerable<KalturaPlaybackSource> mediaFiles, int groupId, string sessionCharacteristicKey)
        {
            var fileTypes = new FileManager.FileTypes(groupId, _mediaFileTypeManager);

            var filterRuleCondition = new FilterRuleCondition(groupId, sessionCharacteristicKey);
            var playbackRules = _filterRuleStorage.GetFilterFileRulesForPlayback(filterRuleCondition);

            return FilterAssetFilesForUser(mediaFiles, playbackRules, AssetTypeMapper.ToEAssetType(assetType), fileTypes).ToList();
        }

        private IEnumerable<T> FilterAssetFilesForUser<T>(IEnumerable<T> mediaFiles, IReadOnlyCollection<AssetRuleAction> rules, eAssetTypes assetType, FileManager.FileTypes fileTypes) where T : KalturaMediaFile
        {
            return rules.Count == 0
                ? mediaFiles
                : mediaFiles?.Where(mediaFile => FileMatchUser(rules, assetType, mediaFile, fileTypes));
        }

        private void FilterMediaFile(KalturaAsset asset, FileManager.FileTypes fileTypes, IReadOnlyCollection<AssetRuleAction> discoveryRules, IReadOnlyCollection<AssetRuleAction> playbackRules)
        {
            var assetType = AssetTypeMapper.ToEAssetType(asset.Type);
            var mediaFiles = FilterAssetFilesForUser(asset.MediaFiles, discoveryRules, assetType, fileTypes);
            asset.MediaFiles = CreateDiscoveryMediaFiles(mediaFiles, playbackRules, assetType, fileTypes)?.ToList();
        }

        private bool FileMatchUser(IEnumerable<AssetRuleAction> rules, eAssetTypes assetType, KalturaMediaFile mediaFile, FileManager.FileTypes fileTypes)
        {
            var fileType = fileTypes.GetFileType(mediaFile.TypeId);
            var fileDynamicData = mediaFile.DynamicData?
                .ToDictionary(x => x.Key, x => x.Value.Objects.Select(_ => _.value));
            var target = new FilterFileRule.Target(fileType, assetType, mediaFile.Labels, fileDynamicData);
            var match = _filterFileRule.MatchRules(target, rules);

            return match;
        }

        private IEnumerable<KalturaMediaFile> CreateDiscoveryMediaFiles(IEnumerable<KalturaMediaFile> mediaFiles, IReadOnlyCollection<AssetRuleAction> rules, eAssetTypes assetType, FileManager.FileTypes fileTypes)
        {
            return rules.Count == 0
                ? mediaFiles
                : mediaFiles?.Select(x => CreateDiscoveryMediaFile(rules, assetType, fileTypes, x));
        }

        private KalturaDiscoveryMediaFile CreateDiscoveryMediaFile(IEnumerable<AssetRuleAction> rules, eAssetTypes assetType, FileManager.FileTypes fileTypes, KalturaMediaFile mediaFile)
        {
            var discoveryMediaFile = Mapper.Map<KalturaDiscoveryMediaFile>(mediaFile);
            discoveryMediaFile.IsPlaybackable = FileMatchUser(rules, assetType, mediaFile, fileTypes);

            return discoveryMediaFile;
        }
    }
}