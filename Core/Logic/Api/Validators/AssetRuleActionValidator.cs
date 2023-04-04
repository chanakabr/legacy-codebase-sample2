using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.Catalog.CatalogManagement.Repositories;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Rules;
using ApiObjects.Rules.FilterActions;
using ApiObjects.Rules.PreActionCondition;
using ApiObjects.SearchObjects;
using Core.Api.Managers;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Pricing;
using Module = Core.Pricing.Module;

namespace ApiLogic.Api.Validators
{
    public interface IAssetRuleActionValidator
    {
        Status Validate(int groupId, IEnumerable<AssetRuleAction> actions);
    }

    public class AssetRuleActionValidator : IAssetRuleActionValidator
    {
        private static readonly Lazy<AssetRuleActionValidator> LazyInstance = new Lazy<AssetRuleActionValidator>(() =>
            new AssetRuleActionValidator(
                Core.Catalog.CatalogManagement.FileManager.Instance,
                CatalogManager.Instance,
                Module.Instance,
                LabelRepository.Instance,
                AssetUserRuleManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IAssetRuleActionValidator Instance => LazyInstance.Value;

        private readonly IMediaFileTypeManager _mediaFileTypeManager;
        private readonly ITagManager _tagManager;
        private readonly IPPVModuleManager _ppvModuleManager;
        private readonly ILabelRepository _labelRepository;
        private readonly IAssetUserRuleManager _assetUserRuleManager;

        public AssetRuleActionValidator(IMediaFileTypeManager mediaFileTypeManager, ITagManager tagManager, IPPVModuleManager ppvModuleManager, ILabelRepository labelRepository, IAssetUserRuleManager assetUserRuleManager)
        {
            _mediaFileTypeManager = mediaFileTypeManager;
            _tagManager = tagManager;
            _ppvModuleManager = ppvModuleManager;
            _labelRepository = labelRepository;
            _assetUserRuleManager = assetUserRuleManager;
        }

        public Status Validate(int groupId, IEnumerable<AssetRuleAction> actions)
        {
            var mediaFileTypesResponse = _mediaFileTypeManager.GetMediaFileTypes(groupId);
            var mediaFileTypes = mediaFileTypesResponse.GetOrThrow();

            var labelsResponse = _labelRepository.List(groupId);
            var labels = labelsResponse.GetOrThrow().Where(x => x.EntityAttribute == EntityAttribute.MediaFileLabels).Select(x => x.Value).ToHashSet();

            foreach (var action in actions)
            {
                var res = ValidateAction(groupId, action, mediaFileTypes, labels);
                if (!res.IsOkStatusCode())
                {
                    return res;
                }
            }
            return Status.Ok;
        }

        public Status ValidateAction(int groupId, AssetRuleAction action, List<MediaFileType> mediaFileTypes, HashSet<string> labels)
        {
            switch (action)
            {
                case AssetLifeCycleBuisnessModuleTransitionAction c: return ValidateAssetLifeCycleBusinessModuleTransitionAction(groupId, c, mediaFileTypes);
                case AssetLifeCycleTagTransitionAction c: return ValidateAssetLifeCycleTagTransitionAction(groupId, c);
                case AssetRuleFilterAction c: return ValidateAction(groupId, c, mediaFileTypes, labels);
                default: return Status.Ok;
            }
        }

        private Status ValidateAction(int groupId, AssetRuleFilterAction action, List<MediaFileType> mediaFileTypes, HashSet<string> labels)
        {
            var status = ValidatePreActionCondition(groupId, action.PreActionCondition);
            if (!status.IsOkStatusCode())
            {
                return status;
            }

            switch (action)
            {
                case FilterFileByVideoCodec c: return ValidateFilterFileByVideoCodec(c, mediaFileTypes);
                case FilterFileByAudioCodec c: return ValidateFilterFileByAudioCodec(c, mediaFileTypes);
                case FilterFileByFileType c: return ValidateFileTypeIds(c.FileTypeIds, mediaFileTypes);
                case FilterFileByLabel c: return ValidateFilterFileByLabel(c, labels);
                case FilterAssetByKsql c: return ValidateFilterAssetByKsql(c);
                default: return Status.Ok;
            }
        }

        private Status ValidatePreActionCondition(int groupId, BasePreActionCondition preActionCondition)
        {
            if (preActionCondition == null)
            {
                return Status.Ok;
            }

            switch (preActionCondition)
            {
                case ShopPreActionCondition shopPreActionCondition:
                    return ValidateShopPreActionCondition(groupId, shopPreActionCondition);
                default:
                    return Status.Ok;
            }
        }

        private Status ValidateShopPreActionCondition(int groupId, ShopPreActionCondition condition)
        {
            var assetUserRuleResponse = _assetUserRuleManager.GetAssetUserRuleByRuleId(
                groupId,
                condition.ShopAssetUserRuleId);

            return assetUserRuleResponse.Status;
        }

        private static Status AllExists<T>(IEnumerable<T> idsToCheck, HashSet<T> existingIds, eResponseStatus errorStatus)
        {
            var nonExistingIds = idsToCheck.Where(x => !existingIds.Contains(x)).ToList();
            return nonExistingIds.Count > 0 ?
                new Status(errorStatus, $"{string.Join(", ", nonExistingIds)} does not exist") : Status.Ok;
        }

        private static Status ValidateFilterFileByVideoCodec(FilterFileByVideoCodec action, List<MediaFileType> mediaFileTypes)
        {
            var videoCodecs = mediaFileTypes.Where(x => x.VideoCodecs != null).SelectMany(x => x.VideoCodecs).ToHashSet();
            return AllExists(action.VideoCodecs, videoCodecs, eResponseStatus.VideoCodecsDoesNotExist);
        }

        private static Status ValidateFilterFileByAudioCodec(FilterFileByAudioCodec action, List<MediaFileType> mediaFileTypes)
        {
            var audioCodecs = mediaFileTypes.Where(x => x.AudioCodecs != null).SelectMany(x => x.AudioCodecs).ToHashSet();
            return AllExists(action.AudioCodecs, audioCodecs, eResponseStatus.AudioCodecsDoesNotExist);
        }

        private Status ValidateAssetLifeCycleTagTransitionAction(int groupId, AssetLifeCycleTagTransitionAction action)
        {
            // validate all tags exists
            if (action.TagIds != null && action.TagIds.Count > 0)
            {
                _tagManager.GetTagValues(groupId, action.TagIds.Select(x => (long)x).ToList(), 0, 1000, out int totalItemsCount);
                if (totalItemsCount != action.TagIds.Count)
                {
                    return new Status(eResponseStatus.Error, "tag doesn't exists");
                }
            }
            
            return Status.Ok;
        }

        private Status ValidateAssetLifeCycleBusinessModuleTransitionAction(int groupId, AssetLifeCycleBuisnessModuleTransitionAction action, List<MediaFileType> mediaFileTypes)
        {
            // validate all file types exists
            if (action.Transitions.FileTypeIds != null && action.Transitions.FileTypeIds.Count > 0)
            {
                var res = ValidateFileTypeIds(action.Transitions.FileTypeIds.Select(x => (long)x).ToHashSet(), mediaFileTypes);
                if (!res.IsOkStatusCode())
                {
                    return res;
                }
            }

            // validate all ppv exists
            if (action.Transitions.PpvIds != null && action.Transitions.PpvIds.Count > 0)
            {
                var ppvModuleList = _ppvModuleManager.GetPPVModuleList(groupId);
                if (ppvModuleList.HasObjects())
                {
                    var ids = ppvModuleList.Objects.Select(x => int.Parse(x.m_sObjectCode)).ToHashSet();
                    return AllExists(action.Transitions.PpvIds, ids, eResponseStatus.Error);
                }
            }

            return Status.Ok;
        }

        private static Status ValidateFileTypeIds(HashSet<long> fileTypeIds, List<MediaFileType> mediaFileTypes)
        {
            var ids = mediaFileTypes.Select(x => x.Id).ToHashSet();
            return AllExists(fileTypeIds, ids, eResponseStatus.Error);
        }

        private static Status ValidateFilterFileByLabel(FilterFileByLabel action, HashSet<string> labels)
        {
            return AllExists(action.Labels, labels, eResponseStatus.LabelDoesNotExist);
        }
        
        private static Status ValidateFilterAssetByKsql(FilterAssetByKsql action)
        {
            BooleanPhraseNode n = null;
            return BooleanPhraseNode.ParseSearchExpression(action.Ksql, ref n);
        }
    }
}
