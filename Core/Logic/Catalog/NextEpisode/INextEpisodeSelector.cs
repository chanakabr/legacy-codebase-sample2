using ApiObjects.Response;
using Core.Catalog.Response;

namespace ApiLogic.Catalog.NextEpisode
{
    public interface INextEpisodeSelector
    {
        GenericResponse<UnifiedSearchResult> SelectNextEpisode(
            ExtendedSearchResult lastWatchedAsset,
            NextEpisodeSelectorInput input);

        GenericResponse<UnifiedSearchResult> SelectEpisodeByNotWatchedStrategy(NextEpisodeSelectorInput input);
    }
}