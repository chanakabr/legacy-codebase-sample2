using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects.Rules;
using Core.Catalog.CatalogManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ApiLogic.Api.Managers
{
    public interface IConditionValidator
    {
        Status Validate(int groupId, RuleCondition condition);
    }
    public class ConditionValidator : IConditionValidator
    {
        private static readonly Lazy<ConditionValidator> LazyInstance = new Lazy<ConditionValidator>(() =>
            new ConditionValidator(Core.Catalog.CatalogManagement.FileManager.Instance,
                ChannelManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IConditionValidator Instance => LazyInstance.Value;

        private readonly IMediaFileTypeManager _mediaFileTypeManager;
        private readonly IChannelManager _channelManager;

        public ConditionValidator(IMediaFileTypeManager mediaFileTypeManager,
            IChannelManager channelManager)
        {
            _mediaFileTypeManager = mediaFileTypeManager;
            _channelManager = channelManager;
        }

        public Status Validate(int groupId, RuleCondition condition)
        {
            switch (condition)
            {
                case ChannelCondition c: return Validate(groupId, c);
                case FileTypeCondition c: return Validate(groupId, c);
                case OrCondition c: return Validate(groupId, c);
                default: return new Status(eResponseStatus.OK);
            }
        }

        private Status Validate(int groupId, OrCondition condition)
        {
            return condition.Conditions
            .Select(x => Validate(groupId, x))
            .FirstOrDefault(status => !status.IsOkStatusCode()) ?? Status.Ok;
        }

        private Status Validate(int groupId, ChannelCondition condition)
        {
            var contextData = new ContextData(groupId);
            var channels = _channelManager.GetChannelsListResponseByChannelIds(contextData, condition.ChannelIds.Select(x => (int)x).ToList(), true, null, true);

            if (!channels.IsOkStatusCode())
            {
                return new Status(eResponseStatus.ChannelDoesNotExist, "The channel does not exist");
            }

            if (channels.Objects?.Count == 0)
            {
                return new Status(eResponseStatus.ChannelDoesNotExist, "The channel does not exist");
            }

            var channelsMap = channels.Objects.Select(x => (long)x.m_nChannelID).ToHashSet();
            return AllExists(condition.ChannelIds, channelsMap, "ChannelIds", eResponseStatus.ChannelDoesNotExist);
        }

        private Status Validate(int groupId, FileTypeCondition condition)
        {
            var fileTypes = _mediaFileTypeManager.GetMediaFileTypes(groupId);

            if (!fileTypes.IsOkStatusCode())
            {
                return fileTypes.Status;
            }

            if (fileTypes.Objects?.Count == 0)
            {
                return new Status(eResponseStatus.MediaFileTypeDoesNotExist, "The Asset File Type Does Not Exist");
            }

            var fileTypesMap = fileTypes.Objects.Select(x => x.Id).ToHashSet();
            return AllExists(condition.FileTypeIds, fileTypesMap, "FileTypes", eResponseStatus.MediaFileTypeDoesNotExist);
        }

        private Status AllExists<T>(IEnumerable<T> idsToCheck, HashSet<T> exisitingIds, string objectName, eResponseStatus errorStatus)
        {
            var nonExistingIds = idsToCheck.Where(x => !exisitingIds.Contains(x)).ToList();
            return nonExistingIds.Count > 0 ?
                new Status(errorStatus, $"{objectName} ids {string.Join(", ", nonExistingIds)} does not exist") : Status.Ok;
        }
    }
}