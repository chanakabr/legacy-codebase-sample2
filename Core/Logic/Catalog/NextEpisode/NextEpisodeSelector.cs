using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects.NextEpisode;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Api;
using Core.Catalog;
using Core.Catalog.Response;
using TVinciShared;
using OrderDir = ApiObjects.SearchObjects.OrderDir;

namespace ApiLogic.Catalog.NextEpisode
{
    public class NextEpisodeSelector : INextEpisodeSelector
    {
        private static readonly Lazy<INextEpisodeSelector> LazyInstance = new Lazy<INextEpisodeSelector>(
            () => new NextEpisodeSelector(),
            LazyThreadSafetyMode.PublicationOnly);

        public static INextEpisodeSelector Instance => LazyInstance.Value;

        public GenericResponse<UnifiedSearchResult> SelectNextEpisode(
            ExtendedSearchResult lastWatchedAsset,
            NextEpisodeSelectorInput input)
        {
            int.TryParse(api.GetStringParamFromExtendedSearchResult(
                lastWatchedAsset,
                $"metas.{input.EpisodeNumberMetaName}"),
                out var episodeNumber);
            var isSeasonNumberSet = int.TryParse(
                api.GetStringParamFromExtendedSearchResult(lastWatchedAsset, $"metas.{input.SeasonNumberMetaName}"),
                out var seasonNumber);
            var episodeFilter = isSeasonNumberSet
                ? GetNextEpisodeFilter(input, seasonNumber, episodeNumber)
                : GetNextEpisodeFilter(input, episodeNumber);
            var orderings = isSeasonNumberSet
                ? GetOrderingsForNotEmptySeason(input)
                : GetOrderingsForEmptySeason(input);
            var response = GetUnifiedSearchResponse(input, episodeFilter, orderings);
            return response?.searchResults?.Count == 1
                ? new GenericResponse<UnifiedSearchResult>(Status.Ok, response.searchResults.First())
                : SelectEpisodeByAllWatchedStrategy(lastWatchedAsset, input);
        }

        public GenericResponse<UnifiedSearchResult> SelectEpisodeByNotWatchedStrategy(NextEpisodeSelectorInput input)
        {
            switch (input.Context.NotWatchedReturnStrategy)
            {
                case NotWatchedReturnStrategy.FirstEpisode:
                    return SelectFirstEpisode(input);
                default:
                    return new GenericResponse<UnifiedSearchResult>(
                        eResponseStatus.NoNextEpisode,
                        "User have not started watching this TV series");
            }
        }

        private static GenericResponse<UnifiedSearchResult> SelectFirstEpisode(NextEpisodeSelectorInput input)
        {
            var filter = new KsqlBuilder()
                .And(x => x
                    .Or(y => y.Values(x.Equal, CatalogLogic.ASSET_TYPE, input.AssetStructIds))
                    .Equal(input.SeriesIdMetaName, input.SeriesId))
                .Build();
            var orderings = GetOrderingsForEmptySeason(input);
            var response = GetUnifiedSearchResponse(input, filter, orderings);

            return response?.searchResults?.Count == 1
                ? new GenericResponse<UnifiedSearchResult>(Status.Ok, response.searchResults.First())
                : new GenericResponse<UnifiedSearchResult>();
        }

        private GenericResponse<UnifiedSearchResult> SelectEpisodeByAllWatchedStrategy(UnifiedSearchResult lastWatchedAsset, NextEpisodeSelectorInput input)
        {
            switch (input.Context.WatchedAllReturnStrategy)
            {
                case WatchedAllReturnStrategy.LastEpisode:
                    return new GenericResponse<UnifiedSearchResult>(Status.Ok, lastWatchedAsset);
                case WatchedAllReturnStrategy.FirstEpisode:
                    return SelectFirstEpisode(input);
                default:
                    return new GenericResponse<UnifiedSearchResult>(
                        eResponseStatus.NoNextEpisode,
                        "User has already watched all episodes of TV series");
            }
        }

        private static string GetNextEpisodeFilter(
            NextEpisodeSelectorInput input,
            int seasonNumber,
            int episodeNumber)
        {
            return new KsqlBuilder()
                .And(x => x
                    .Or(y => y.Values(x.Equal, CatalogLogic.ASSET_TYPE, input.AssetStructIds))
                    .Equal(input.SeriesIdMetaName, input.SeriesId)
                    .Or(y => y
                        .And(k => k.Equal(input.SeasonNumberMetaName, seasonNumber).Greater(input.EpisodeNumberMetaName, episodeNumber))
                        .And(k => k.Greater(input.SeasonNumberMetaName, seasonNumber).Greater(input.EpisodeNumberMetaName, 0))))
                .Build();
        }

        private static UnifiedSearchResponse GetUnifiedSearchResponse(
            NextEpisodeSelectorInput input,
            string filter,
            IReadOnlyCollection<AssetOrder> orderingParameters)
            => api.SearchAssetsExtended(
                input.Context.GroupId,
                filter,
                0,
                1,
                true,
                0,
                true,
                string.Empty,
                string.Empty,
                string.Empty,
                0,
                input.ExactGroupId,
                false,
                false,
                GetExtraReturnFields(input),
                null,
                orderingParameters);

        private static string GetNextEpisodeFilter(
            NextEpisodeSelectorInput input,
            int episodeNumber)
        {
            return new KsqlBuilder()
                .And(x => x
                    .Or(y => y.Values(x.Equal, CatalogLogic.ASSET_TYPE, input.AssetStructIds))
                    .Equal(input.SeriesIdMetaName, input.SeriesId)
                    .Or(y => y
                        .And(k => k.NotExists(input.SeasonNumberMetaName).Greater(input.EpisodeNumberMetaName, episodeNumber))
                        .And(k => k.Greater(input.SeasonNumberMetaName, 0).Greater(input.EpisodeNumberMetaName, 0))))
                .Build();
        }

        private static IReadOnlyCollection<AssetOrder> GetOrderingsForNotEmptySeason(NextEpisodeSelectorInput input) =>
            new List<AssetOrder>
            {
                new AssetOrderByMeta { Direction = OrderDir.ASC, MetaName = input.SeasonNumberMetaName, Field = OrderBy.META },
                new AssetOrderByMeta { Direction = OrderDir.ASC, MetaName = input.EpisodeNumberMetaName, Field = OrderBy.META }
            };

        private static IReadOnlyCollection<AssetOrder> GetOrderingsForEmptySeason(NextEpisodeSelectorInput input) =>
            new List<AssetOrder>
            {
                new AssetOrderByMeta
                {
                    Direction = OrderDir.ASC,
                    MetaName = input.SeasonNumberMetaName,
                    Field = OrderBy.META,
                    IsMissingFirst = true
                },
                new AssetOrderByMeta { Direction = OrderDir.ASC, MetaName = input.EpisodeNumberMetaName, Field = OrderBy.META }
            };

        private static List<string> GetExtraReturnFields(NextEpisodeSelectorInput input)
            => new List<string> { $"metas.{input.SeasonNumberMetaName}", $"metas.{input.EpisodeNumberMetaName}" };
    }
}