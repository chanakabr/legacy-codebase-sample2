using System;
using System.Linq;
using System.Threading;
using ApiObjects;
using ApiObjects.Response;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.GroupManagers;
using WebAPI.Exceptions;
using WebAPI.Mappers;
using WebAPI.Models.Catalog;

namespace WebAPI.Validation
{
    public class NextEpisodeValidator : INextEpisodeValidator
    {
        private static readonly Lazy<INextEpisodeValidator> Lazy = new Lazy<INextEpisodeValidator>(
            () => new NextEpisodeValidator(CatalogManager.Instance, GroupSettingsManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly ICatalogManager _catalogManager;
        private readonly IGroupSettingsManager _groupSettingsManager;

        public NextEpisodeValidator(ICatalogManager catalogManager, IGroupSettingsManager groupSettingsManager)
        {
            _catalogManager = catalogManager;
            _groupSettingsManager = groupSettingsManager;
        }

        public static INextEpisodeValidator Instance => Lazy.Value;

        public void Validate(int groupId, KalturaSeriesIdArguments seriesIdArguments)
        {
            if (seriesIdArguments == null || !_groupSettingsManager.IsOpc(groupId))
            {
                return;
            }

            if (string.IsNullOrEmpty(seriesIdArguments.AssetTypeIdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, nameof(seriesIdArguments.AssetTypeIdIn));
            }

            var assetStructIds = Utils.Utils.ParseCommaSeparatedValues<long>(seriesIdArguments.AssetTypeIdIn, nameof(seriesIdArguments.AssetTypeIdIn), true);
            if (!assetStructIds.Any())
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, nameof(seriesIdArguments.AssetTypeIdIn));
            }

            if (!_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out var cache))
            {
                throw new ClientException(Status.ErrorMessage($"Failed to get catalogGroupCache for groupId: {groupId} when calling NextEpisodeValidator.Validate"));
            }

            var seriesIdTopic = GetTopic(cache, seriesIdArguments.SeriesIdMetaName, NextEpisodeMapper.SERIES_ID_TOPIC_SYSTEM_NAME);
            var seasonNumberTopic = GetTopic(cache, seriesIdArguments.SeasonNumberMetaName, NextEpisodeMapper.SEASON_NUMBER_TOPIC_SYSTEM_NAME);
            var episodeNumberTopic = GetTopic(cache, seriesIdArguments.EpisodeNumberMetaName, NextEpisodeMapper.EPISODE_NUMBER_TOPIC_SYSTEM_NAME);
            foreach (var assetStructId in assetStructIds)
            {
                if (!cache.AssetStructsMapById.TryGetValue(assetStructId, out var assetStruct))
                {
                    throw new ClientException(new Status(eResponseStatus.AssetStructDoesNotExist, $"Asset struct {assetStructId} does not exist"));
                }

                if (!assetStruct.MetaIds.Contains(seriesIdTopic.Id)
                    || !assetStruct.MetaIds.Contains(seasonNumberTopic.Id)
                    || !assetStruct.MetaIds.Contains(episodeNumberTopic.Id))
                {
                    throw new ClientException(
                        new Status(
                            eResponseStatus.InvalidAssetStruct,
                            $"Asset struct {assetStructId} does not contain series Id, season number or episode number metas"));
                }
            }
        }

        private static Topic GetTopic(CatalogGroupCache cache, string topicName, string defaultTopicName)
        {
            var topic = !string.IsNullOrEmpty(topicName)
                ? topicName
                : defaultTopicName;

            return cache.TopicsMapBySystemNameAndByType.TryGetValue(topic, out var topics)
                ? topics.Values.First()
                : throw new ClientException(new Status(eResponseStatus.MetaDoesNotExist, $"{topic} meta does not exist"));;
        }
    }
}