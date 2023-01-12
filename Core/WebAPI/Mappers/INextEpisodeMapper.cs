using ApiObjects.Base;
using ApiObjects.NextEpisode;
using WebAPI.Models.Catalog;

namespace WebAPI.Mappers
{
    public interface INextEpisodeMapper
    {
        NextEpisodeContext MapToContext(
            ContextData contextData,
            KalturaNotWatchedReturnStrategy? notWatchedReturnStrategy,
            KalturaWatchedAllReturnStrategy? watchedAllReturnStrategy);

        SeriesType MapToSeriesType(KalturaSeriesIdArguments input);
    }
}