using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using ApiLogic.Api.Managers;
using ApiLogic.Api.Managers.Rule;
using ApiObjects;
using ApiObjects.Rules;
using AutoMapper;
using Core.Api.Managers;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using KalturaRequestContext;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.ObjectsConvertor.Mapping;

namespace WebAPI.Managers
{
    public class MediaFileFilter : IMediaFileFilter
    {
        private static readonly Lazy<MediaFileFilter> Lazy = new Lazy<MediaFileFilter>(
            () => new MediaFileFilter(
                FilterFileRule.Instance,
                FilterRuleStorage.Instance,
                FileManager.Instance,
                AssetManager.Instance,
                ShopMarkerService.Instance,
                AssetUserRuleManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IFilterFileRule _filterFileRule;
        private readonly IFilterRuleStorage _filterRuleStorage;
        private readonly IMediaFileTypeManager _mediaFileTypeManager;
        private readonly IAssetManager _assetManager;
        private readonly IShopMarkerService _shopMarkerService;
        private readonly IAssetUserRuleManager _assetUserRuleManager;

        public static IMediaFileFilter Instance => Lazy.Value;

        public MediaFileFilter(
            IFilterFileRule filterFileRule,
            IFilterRuleStorage filterRuleStorage,
            IMediaFileTypeManager mediaFileTypeManager,
            IAssetManager assetManager,
            IShopMarkerService shopMarkerService,
            IAssetUserRuleManager assetUserRuleManager)
        {
            _filterFileRule = filterFileRule;
            _filterRuleStorage = filterRuleStorage;
            _mediaFileTypeManager = mediaFileTypeManager;
            _assetManager = assetManager;
            _shopMarkerService = shopMarkerService;
            _assetUserRuleManager = assetUserRuleManager;
        }

        public void FilterAssetFiles(KalturaAsset asset, int groupId, string sessionCharacteristicKey)
        {
            FilterAssetFiles(new[] { asset }, groupId, sessionCharacteristicKey);
        }

        public void FilterAssetFiles(IEnumerable<KalturaAsset> assets, int groupId, string sessionCharacteristicKey)
        {
            var fileTypes = new FileManager.FileTypes(groupId, _mediaFileTypeManager);
            var context = new PreActionConditionContext(groupId, _assetUserRuleManager, _shopMarkerService);
            foreach (var asset in assets)
            {
                var assetType = AssetTypeMapper.ToEAssetType(asset.Type);
                var lazyAsset = new Lazy<FilterMediaFileAsset>(() => Map(asset));
                var condition = new FilterFileRuleCondition(groupId, sessionCharacteristicKey, lazyAsset, context);
                var assetDiscoveryRules = _filterRuleStorage.GetAssetFilterRulesForDiscovery(condition);
                var assetPlaybackRules = _filterRuleStorage.GetAssetFilterRulesForPlayback(condition);

                var mediaFiles = FilterAssetFilesForUser(asset.MediaFiles, assetDiscoveryRules, assetType, fileTypes);
                asset.MediaFiles = CreateDiscoveryMediaFiles(mediaFiles, assetPlaybackRules, assetType, fileTypes)?.ToList();
            }
        }

        public IEnumerable<KalturaPlaybackSource> GetFilteredAssetFiles(
            IEnumerable<KalturaPlaybackSource> mediaFiles,
            int groupId,
            long assetId,
            KalturaAssetType assetType,
            string sessionCharacteristicKey)
        {
            var fileTypes = new FileManager.FileTypes(groupId, _mediaFileTypeManager);
            var type = AssetTypeMapper.ToEAssetType(assetType);
            var lazyAsset = new Lazy<FilterMediaFileAsset>(() =>
            {
                var assetToGet =
                    type == eAssetTypes.NPVR &&
                    RequestContextUtilsInstance.Get().TryGetRecordingConvertId(out var programId)
                        ? new KeyValuePair<eAssetTypes, long>(eAssetTypes.EPG, programId)
                        : new KeyValuePair<eAssetTypes, long>(type, assetId);

                var asset = _assetManager.GetAssets(
                    groupId,
                    new[] { assetToGet },
                    true).Single();
                return Map(asset);
            });

            var context = new PreActionConditionContext(groupId, _assetUserRuleManager, _shopMarkerService);
            var condition = new FilterFileRuleCondition(groupId, sessionCharacteristicKey, lazyAsset, context);
            var assetPlaybackRules = _filterRuleStorage.GetAssetFilterRulesForPlayback(condition);

            return FilterAssetFilesForUser(mediaFiles, assetPlaybackRules, type, fileTypes).ToList();
        }

        private IEnumerable<T> FilterAssetFilesForUser<T>(
            IEnumerable<T> mediaFiles,
            IReadOnlyCollection<AssetRuleFilterAction> rules,
            eAssetTypes assetType,
            FileManager.FileTypes fileTypes) where T : KalturaMediaFile
        {
            return rules.Count == 0
                ? mediaFiles
                : mediaFiles?.Where(mediaFile => FileMatchUser(rules, assetType, mediaFile, fileTypes));
        }

        private bool FileMatchUser(
            IEnumerable<AssetRuleFilterAction> actions,
            eAssetTypes assetType,
            KalturaMediaFile mediaFile,
            FileManager.FileTypes fileTypes)
        {
            var fileType = fileTypes.GetFileType(mediaFile.TypeId);
            var fileDynamicData = mediaFile.DynamicData?
                .ToDictionary(x => x.Key, x => x.Value.Objects.Select(_ => _.value));
            var target = new FilterFileRule.Target(fileType, assetType, mediaFile.Labels, fileDynamicData);
            var match = _filterFileRule.MatchRules(target, actions);

            return match;
        }

        private IEnumerable<KalturaMediaFile> CreateDiscoveryMediaFiles(
            IEnumerable<KalturaMediaFile> mediaFiles,
            IReadOnlyCollection<AssetRuleFilterAction> actions,
            eAssetTypes assetType,
            FileManager.FileTypes fileTypes)
        {
            return actions.Count == 0
                ? mediaFiles
                : mediaFiles?.Select(x => CreateDiscoveryMediaFile(actions, assetType, fileTypes, x));
        }

        private KalturaDiscoveryMediaFile CreateDiscoveryMediaFile(
            IEnumerable<AssetRuleFilterAction> actions,
            eAssetTypes assetType,
            FileManager.FileTypes fileTypes,
            KalturaMediaFile mediaFile)
        {
            var discoveryMediaFile = Mapper.Map<KalturaDiscoveryMediaFile>(mediaFile);
            discoveryMediaFile.IsPlaybackable = FileMatchUser(actions, assetType, mediaFile, fileTypes);

            return discoveryMediaFile;
        }

        private static FilterMediaFileAsset Map(KalturaAsset asset)
            => new FilterMediaFileAsset
            {
                AssetId = asset.Id.Value,
                Metas = CatalogMappings.GetMetaList(asset.Metas),
                Tags = CatalogMappings.GetTagsList(asset.Tags)
            };

        private static FilterMediaFileAsset Map(Asset asset)
            => new FilterMediaFileAsset
            {
                AssetId = asset.Id,
                Metas = asset.Metas,
                Tags = asset.Tags
            };
    }
}