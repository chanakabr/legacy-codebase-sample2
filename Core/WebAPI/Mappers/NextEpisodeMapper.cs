using System;
using System.Threading;
using ApiObjects.Base;
using ApiObjects.NextEpisode;
using WebAPI.InternalModels;
using WebAPI.Models.Catalog;

namespace WebAPI.Mappers
{
    public class NextEpisodeMapper : INextEpisodeMapper
    {
        public const string SEASON_NUMBER_TOPIC_SYSTEM_NAME = "seasonnumber";
        public const string EPISODE_NUMBER_TOPIC_SYSTEM_NAME = "episodenumber";
        public const string SERIES_ID_TOPIC_SYSTEM_NAME = "seriesId";

        private static readonly Lazy<INextEpisodeMapper> Lazy = new Lazy<INextEpisodeMapper>(
            () => new NextEpisodeMapper(),
            LazyThreadSafetyMode.PublicationOnly);

        public static INextEpisodeMapper Instance => Lazy.Value;

        public NextEpisodeContext MapToContext(
            ContextData contextData,
            KalturaNotWatchedReturnStrategy? notWatchedReturnStrategy,
            KalturaWatchedAllReturnStrategy? watchedAllReturnStrategy)
            => new NextEpisodeContext
            {
                GroupId = contextData.GroupId,
                UserId = contextData.UserId.Value,
                NotWatchedReturnStrategy = Map(notWatchedReturnStrategy),
                WatchedAllReturnStrategy = Map(watchedAllReturnStrategy)
            };

        public SeriesType MapToSeriesType(KalturaSeriesIdArguments input)
            => new SeriesType
            {
                AssetStructIds =
                    Utils.Utils.ParseCommaSeparatedValues<long>(input.AssetTypeIdIn, nameof(input.AssetTypeIdIn)),
                SeriesIdMeta = !string.IsNullOrEmpty(input.SeriesIdMetaName)
                    ? input.SeriesIdMetaName
                    : SERIES_ID_TOPIC_SYSTEM_NAME,
                SeasonNumberMeta = !string.IsNullOrEmpty(input.SeasonNumberMetaName)
                    ? input.SeasonNumberMetaName
                    : SEASON_NUMBER_TOPIC_SYSTEM_NAME,
                EpisodeNumberMeta = !string.IsNullOrEmpty(input.EpisodeNumberMetaName)
                    ? input.EpisodeNumberMetaName
                    : EPISODE_NUMBER_TOPIC_SYSTEM_NAME
            };

        private static WatchedAllReturnStrategy Map(KalturaWatchedAllReturnStrategy? source)
        {
            switch (source)
            {
                case KalturaWatchedAllReturnStrategy.RETURN_FIRST_EPISODE:
                    return WatchedAllReturnStrategy.FirstEpisode;
                case KalturaWatchedAllReturnStrategy.RETURN_NO_NEXT_EPISODE:
                    return WatchedAllReturnStrategy.NoNextEpisode;
                default:
                    return WatchedAllReturnStrategy.LastEpisode;
            }
        }

        private static NotWatchedReturnStrategy Map(KalturaNotWatchedReturnStrategy? source)
        {
            switch (source)
            {
                case KalturaNotWatchedReturnStrategy.RETURN_FIRST_EPISODE:
                    return NotWatchedReturnStrategy.FirstEpisode;
                default:
                    return NotWatchedReturnStrategy.NoNextEpisode;
            }
        }
    }
}