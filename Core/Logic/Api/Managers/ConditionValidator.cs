using ApiLogic.Pricing.Handlers;
using ApiLogic.Segmentation;
using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects.Rules;
using Core.Catalog.CatalogManagement;
using Core.Pricing;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ApiLogic.Api.Managers
{
    public interface IConditionValidator
    {
        Status Validate(ContextData contextData, RuleCondition condition, long? assetUserRuleId);
    }

    public class ConditionValidator : IConditionValidator
    {
        private static readonly Lazy<ConditionValidator> LazyInstance = new Lazy<ConditionValidator>(() =>
            new ConditionValidator(Core.Catalog.CatalogManagement.FileManager.Instance,
                                   ChannelManager.Instance,
                                   SegmentationTypeLogic.Instance,
                                   CollectionManager.Instance,
                                   PpvManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IConditionValidator Instance => LazyInstance.Value;

        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly IMediaFileTypeManager _mediaFileTypeManager;
        private readonly IChannelManager _channelManager;
        private readonly ISegmentationTypeLogic _segmentationTypeLogic;
        private readonly ICollectionManager _collectionManager;
        private readonly IPpvManager _ppvManager;

        public ConditionValidator(IMediaFileTypeManager mediaFileTypeManager, 
                                  IChannelManager channelManager,
                                  ISegmentationTypeLogic segmentationTypeLogic,
                                  ICollectionManager collectionManager,
                                  IPpvManager ppvManager)
        {
            _mediaFileTypeManager = mediaFileTypeManager;
            _channelManager = channelManager;
            _segmentationTypeLogic = segmentationTypeLogic;
            _collectionManager = collectionManager;
            _ppvManager = ppvManager;
        }

        public Status Validate(ContextData contextData, RuleCondition condition, long? assetUserRuleId)
        {
            switch (condition)
            {
                case ChannelCondition c: return ValidateChannelCondition(contextData, c, assetUserRuleId);
                case FileTypeCondition c: return ValidateFileTypeCondition(contextData, c, assetUserRuleId);
                case SegmentsCondition c: return ValidateSegmentsCondition(contextData, c, assetUserRuleId);
                case BusinessModuleCondition c: return ValidateBusinessModuleCondition(contextData, c, assetUserRuleId);
                case OrCondition c: return ValidateOrCondition(contextData, c, assetUserRuleId);
                default: return new Status(eResponseStatus.OK);
            }
        }

        private Status ValidateOrCondition(ContextData contextData, OrCondition condition, long? assetUserRuleId)
        {
            return condition.Conditions
            .Select(x => Validate(contextData, x, assetUserRuleId))
            .FirstOrDefault(status => !status.IsOkStatusCode()) ?? Status.Ok;
        }

        private Status ValidateChannelCondition(ContextData contextData, ChannelCondition condition, long? assetUserRuleId)
        {
            var channelIds = condition.ChannelIds.Select(x => (int)x).ToList();
            var channels = _channelManager.GetChannelsListResponseByChannelIds(contextData, channelIds, true, null, true);
            if (!channels.HasObjects())
            {
                return new Status(eResponseStatus.ChannelDoesNotExist, "The channel does not exist");
            }

            var channelsMap = channels.Objects.ToDictionary(x => (long)x.m_nChannelID, y => y.AssetUserRuleId);
            return AllExists(condition.ChannelIds, channelsMap, "ChannelIds", eResponseStatus.ChannelDoesNotExist, assetUserRuleId, true);
        }

        private Status ValidateFileTypeCondition(ContextData contextData, FileTypeCondition condition, long? assetUserRuleId)
        {
            var fileTypes = _mediaFileTypeManager.GetMediaFileTypes(contextData.GroupId);

            if (!fileTypes.IsOkStatusCode())
            {
                return fileTypes.Status;
            }

            if (fileTypes.Objects?.Count == 0)
            {
                return new Status(eResponseStatus.MediaFileTypeDoesNotExist, "The Asset File Type Does Not Exist");
            }

            var fileTypesMap = fileTypes.Objects.ToDictionary(x => x.Id, y => (long?)null);
            return AllExists(condition.FileTypeIds, fileTypesMap, "FileTypes", eResponseStatus.MediaFileTypeDoesNotExist, null, true);
        }

        private Status AllExists<T>(IEnumerable<T> idsToCheck, Dictionary<T, long?> exisitingIds, string objectName, eResponseStatus errorStatus, long? assetUserRuleId, bool mustToBeInShop)
        {
            var nonExistingIds = idsToCheck.Where(x => !exisitingIds.ContainsKey(x));
            if (nonExistingIds.Any())
            {
                new Status(errorStatus, $"{objectName} ids {string.Join(", ", nonExistingIds)} does not exist");
            }
                
            if (assetUserRuleId.HasValue)
            {
                IEnumerable<T> notInShopIds;
                if (mustToBeInShop)
                {
                    notInShopIds = exisitingIds.Where(x => !x.Value.HasValue || x.Value.Value != assetUserRuleId.Value).Select(x => x.Key);
                }
                else
                {
                    notInShopIds = exisitingIds.Where(x => x.Value.HasValue && x.Value.Value != assetUserRuleId.Value).Select(x => x.Key);
                }
                
                if (notInShopIds.Any())
                {
                    return new Status(eResponseStatus.EntityIsNotAssociatedWithShop, $"{objectName} ids {string.Join(",", notInShopIds)} is not associated with shop [{assetUserRuleId}].");
                }
            }

            return Status.Ok;
        }

        private Status ValidateSegmentsCondition(ContextData contextData, SegmentsCondition condition, long? assetUserRuleId)
        {
            if (!assetUserRuleId.HasValue) { return Status.Ok; }
            
            var segments = _segmentationTypeLogic.ListBySegmentIds(contextData.GroupId, condition.SegmentIds, 0, condition.SegmentIds.Count, out var _, 0, null);
            if (segments == null || !segments.Any())
            {
                return new Status(eResponseStatus.SegmentsIdsDoesNotExist, $"Segment ids {string.Join(", ", condition.SegmentIds)} does not exist");
            }

            var segmentsMap = segments.ToDictionary(x => x.Value.GetSegmentId(), y => y.AssetUserRuleId);
            return AllExists(condition.SegmentIds, segmentsMap, "Segment", eResponseStatus.SegmentsIdsDoesNotExist, assetUserRuleId, false);
        }

        private Status ValidateBusinessModuleCondition(ContextData contextData, BusinessModuleCondition condition, long? assetUserRuleId)
        {
            if (!assetUserRuleId.HasValue) { return Status.Ok; }

            switch (condition.BusinessModuleType)
            {
                case ApiObjects.eTransactionType.PPV:
                    return ValidatePpv(contextData, condition.BusinessModuleId, assetUserRuleId);
                case ApiObjects.eTransactionType.Collection:
                    return ValidateCollection(contextData, condition.BusinessModuleId, assetUserRuleId);
                case ApiObjects.eTransactionType.Subscription:
                case ApiObjects.eTransactionType.ProgramAssetGroupOffer:
                default: return Status.Ok;
            }
        }

        private Status ValidatePpv(ContextData contextData, long ppvId, long? assetUserRuleId)
        {
            var response = _ppvManager.GetPPVModules(contextData, new List<long> { ppvId }, false, null, true, ApiObjects.Pricing.PPVOrderBy.NameAsc, 0, 30, true);
            return IsExists(response, "Ppv", ppvId, eResponseStatus.PpvModuleNotExist, assetUserRuleId);
        }

        private Status ValidateCollection(ContextData contextData, long collectionId, long? assetUserRuleId)
        {
            var response = _collectionManager.GetCollectionsData(contextData, new string[] { collectionId.ToString() }, string.Empty, 0, 30, true, null, true);
            return IsExists(response, "Collection", collectionId, eResponseStatus.CollectionNotExist, assetUserRuleId);
        }

        private Status IsExists<T>(GenericListResponse<T> response, string objectName, long id, eResponseStatus notExistError, long? assetUserRuleId) where T : PPVModule
        {
            if (!response.HasObjects())
            {
                return new Status(notExistError, $"{objectName} id {id} does not exist");
            }

            // validate in shop
            if (assetUserRuleId.HasValue && (!response.Objects[0].AssetUserRuleId.HasValue || response.Objects[0].AssetUserRuleId.Value != assetUserRuleId.Value))
            {
                return new Status(eResponseStatus.EntityIsNotAssociatedWithShop, $"{objectName} id {id} is not associated with shop [{assetUserRuleId}].");
            }

            return response.Status;
        }
    }
}
