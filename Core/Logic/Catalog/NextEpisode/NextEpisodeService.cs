using System;
using ApiObjects;
using ApiObjects.MediaMarks;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Catalog;
using Core.Catalog.Cache;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Response;
using Core.Notification;
using GroupsCacheManager;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.Catalog;
using ApiObjects.NextEpisode;
using Core.Api;
using Core.GroupManagers;
using TVinciShared;

namespace ApiLogic.Catalog.NextEpisode
{
    public static class NextEpisodeService
    {
        public static GenericResponse<UserWatchHistory> GetNextEpisodeByAssetIdForOpc(
            NextEpisodeContext input,
            long assetId)
        {
            var response = new GenericResponse<UserWatchHistory>();
            if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(input.GroupId, out var catalogGroupCache))
            {
                return new GenericResponse<UserWatchHistory>(eResponseStatus.MetaDoesNotExist, "SeriesId meta does not exist");
            }

            var seriesStruct =
                catalogGroupCache.AssetStructsMapById.Values.FirstOrDefault(s => s.IsSeriesAssetStruct);

            var mediaAssetResponse = GetMediaAsset(input, assetId);
            if (!mediaAssetResponse.IsOkStatusCode())
            {
                return new GenericResponse<UserWatchHistory>(mediaAssetResponse.Status);
            }

            var media = mediaAssetResponse.Object;
            // BEO-12018 - Validation for old behavior with single Series struct.
            if (seriesStruct != null && media.MediaType.m_nTypeID != seriesStruct.Id)
            {
                response.SetStatus(eResponseStatus.InvalidAssetStruct, "AssetStruct is not from Series type");
                return response;
            }

            var episodeStructResponse = GetEpisodeAssetStruct(media.MediaType.m_nTypeID, catalogGroupCache);
            if (!episodeStructResponse.IsOkStatusCode())
            {
                return new GenericResponse<UserWatchHistory>(episodeStructResponse.Status);
            }

            var episodeStruct = episodeStructResponse.Object;
            var seriesTopic = catalogGroupCache.TopicsMapById[episodeStruct.ConnectingMetaId.Value];
            var seriesMeta = media.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName == seriesTopic.SystemName);
            var seriesId = seriesMeta?.m_sValue;

            var (seasonNumberMeta, episodeNumberMeta) = GetOpcEpisodeMetaNames(input.GroupId, catalogGroupCache, episodeStruct);
            if (string.IsNullOrEmpty(seasonNumberMeta) || string.IsNullOrEmpty(episodeNumberMeta))
            {
                response.SetStatus(eResponseStatus.InvalidAssetStruct, "Episode AssetStruct has no Episode Number or Season Number meta");
                return response;
            }

            if (string.IsNullOrEmpty(seriesId))
            {
                response.SetStatus(eResponseStatus.MetaDoesNotExist, "SeriesId meta does not exist");
                return response;
            }

            return GetNextEpisode(
                input,
                new [] { episodeStruct.Id },
                "seriesId",
                seasonNumberMeta,
                episodeNumberMeta,
                seriesId,
                eFieldType.StringMeta,
                string.Empty,
                input.GroupId);
        }

        public static GenericResponse<UserWatchHistory> GetNextEpisodeBySeriesIdForOpc(NextEpisodeContext context, string seriesId, SeriesType seriesType)
        {
            if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(context.GroupId, out var catalogGroupCache))
            {
                return new GenericResponse<UserWatchHistory>(eResponseStatus.MetaDoesNotExist, "SeriesId meta does not exist");
            }

            var seriesIdAssetStructs = new HashSet<long>();
            foreach (var assetStructId in seriesType.AssetStructIds)
            {
                if (!catalogGroupCache.AssetStructsMapById.TryGetValue(assetStructId, out var assetStruct) || !assetStruct.ParentId.HasValue)
                {
                    return new GenericResponse<UserWatchHistory>(eResponseStatus.InvalidAssetStruct, $"Episode AssetStruct {assetStructId} must be a child of series AssetStruct");
                }

                seriesIdAssetStructs.Add(assetStruct.ParentId.Value);
            }

            var seriesAssetSearchResult =
                FindSeriesAsset(context.GroupId, seriesIdAssetStructs, seriesType.SeriesIdMeta, seriesId);
            if (!seriesAssetSearchResult.status.IsOkStatusCode())
            {
                return new GenericResponse<UserWatchHistory>(seriesAssetSearchResult.status);
            }

            if (!seriesAssetSearchResult.searchResults.Any())
            {
                return new GenericResponse<UserWatchHistory>(eResponseStatus.AssetDoesNotExist, $"Series '{seriesId}' not found");
            }

            return GetNextEpisode(
                context,
                seriesType.AssetStructIds,
                seriesType.SeriesIdMeta,
                seriesType.SeasonNumberMeta,
                seriesType.EpisodeNumberMeta,
                seriesId,
                eFieldType.StringMeta,
                string.Empty,
                context.GroupId);
        }

        private static GenericResponse<AssetStruct> GetEpisodeAssetStruct(long seriesAssetStructId, CatalogGroupCache catalogGroupCache)
        {
            var episodeStruct =
                catalogGroupCache.AssetStructsMapById.Values.FirstOrDefault(x =>
                    x.ParentId == seriesAssetStructId && x.SystemName.ToLower() == "episode") ??
                catalogGroupCache.AssetStructsMapById.Values.FirstOrDefault(x =>
                    x.ParentId == seriesAssetStructId);

            if (episodeStruct == null)
            {
                return new GenericResponse<AssetStruct>(eResponseStatus.InvalidAssetStruct, "Episode AssetStruct does not exist");
            }

            if (episodeStruct.ConnectingMetaId == null)
            {
                return new GenericResponse<AssetStruct>(eResponseStatus.InvalidAssetStruct, "Episode AssetStruct has no ConnectedMetaId");
            }

            return !catalogGroupCache.TopicsMapById.ContainsKey(episodeStruct.ConnectingMetaId.Value)
                ? new GenericResponse<AssetStruct>(eResponseStatus.TopicNotFound, "Series Topic does not exist")
                : new GenericResponse<AssetStruct>(Status.Ok, episodeStruct);
        }

        private static GenericResponse<MediaAsset> GetMediaAsset(NextEpisodeContext input, long assetId)
        {
            var assetResponse = AssetManager.Instance.GetAsset(input.GroupId, assetId, eAssetTypes.MEDIA, false);
            if (!assetResponse.HasObject())
            {
                return new GenericResponse<MediaAsset>(assetResponse.Status);
            }

            if (!(assetResponse.Object is MediaAsset asset))
            {
                return new GenericResponse<MediaAsset>(eResponseStatus.InvalidAssetType, "asset is not media type");
            }

            return new GenericResponse<MediaAsset>(Status.Ok, asset);
        }

        public static GenericResponse<UserWatchHistory> GetNextEpisodeByAssetIdForTvm(NextEpisodeContext input, long assetId)
        {
            var assetFilter = new Filter()
            {
                m_bOnlyActiveMedia = true,
                m_bUseStartDate = true
            };

            var assetsToRetrieve = new List<BaseObject> { new BaseObject() { AssetId = assetId.ToString(), AssetType = eAssetTypes.MEDIA } };

            var assets = Core.Catalog.Utils.GetOrderedAssets(input.GroupId, assetsToRetrieve, assetFilter, false);

            if (assets == null || assets.Count == 0)
            {
                var response = new GenericResponse<UserWatchHistory>();
                response.SetStatus(eResponseStatus.AssetDoesNotExist, $"Asset with assetId = {assetId} not found");
                return response;
            }

            var mo = (MediaObj)assets.Single();

            return GetNextEpisodeBySeriesIdForTvm(input, mo.m_sName);
        }

        public static GenericResponse<UserWatchHistory> GetNextEpisodeBySeriesIdForTvm(NextEpisodeContext input, string seriesId)
        {
            var groupManager = new GroupManager();
            var group = groupManager.GetGroup(input.GroupId);

            var episodeAssociationTagName = NotificationCache.Instance().GetEpisodeAssociationTagName(input.GroupId);
            var episodeStructId = NotificationCache.Instance().GetEpisodeMediaTypeId(input.GroupId);

            var permittedWatchRules = string.Join(" ", group.m_sPermittedWatchRules);

            var seriesFilterFieldType = GetFieldType(episodeAssociationTagName, group);

            var seasonNumberMeta = NotificationCache.Instance().GetSeasonNumberMeta(input.GroupId);
            var episodeNumberMeta = NotificationCache.Instance().GetEpisodeNumberMeta(input.GroupId);

            if (string.IsNullOrEmpty(episodeNumberMeta))
            {
                var response = new GenericResponse<UserWatchHistory>();
                response.SetStatus(eResponseStatus.MetaDoesNotExist, "Episode Number meta does not exist");
                return response;
            }

            if (string.IsNullOrEmpty(seasonNumberMeta))
            {
                var response = new GenericResponse<UserWatchHistory>();
                response.SetStatus(eResponseStatus.MetaDoesNotExist, "Season Number meta does not exist");
                return response;
            }

            return GetNextEpisode(
                input,
                new long[] { episodeStructId },
                episodeAssociationTagName.ToLower(),
                seasonNumberMeta,
                episodeNumberMeta,
                seriesId.ToLower(),
                seriesFilterFieldType,
                permittedWatchRules,
                exactGroupId: 0);
        }

        private static GenericResponse<UserWatchHistory> GetNextEpisode(
            NextEpisodeContext context,
            IEnumerable<long> assetStructIds,
            string seriesIdMetaName,
            string seasonNumberMetaName,
            string episodeNumberMetaName,
            string seriesId,
            eFieldType seriesFilterFieldType,
            string permittedWatchRules,
            int exactGroupId)
        {
            var nextEpisodeSelectorInput = new NextEpisodeSelectorInput
            {
                Context = context,
                AssetStructIds = assetStructIds,
                SeriesIdMetaName = seriesIdMetaName.ToLower(),
                SeasonNumberMetaName = seasonNumberMetaName.ToLower(),
                EpisodeNumberMetaName = episodeNumberMetaName.ToLower(),
                ExactGroupId = exactGroupId,
                SeriesId = seriesId.ToLower()
            };

            var usersWatchHistory = CatalogLogic.GetRawUserWatchHistory(
                context.GroupId,
                context.UserId.ToString(),
                new List<int>(),
                new List<string>(),
                new List<int>(),
                eWatchStatus.All,
                0);

            if (usersWatchHistory == null || !usersWatchHistory.Any())
            {
                return GetNextEpisodeByNotWatchedStrategy(nextEpisodeSelectorInput);;
            }

            var searchResults = GetWatchedAssets(
                nextEpisodeSelectorInput,
                usersWatchHistory,
                seriesFilterFieldType,
                permittedWatchRules);
            if (searchResults == null || !searchResults.Any())
            {
                return GetNextEpisodeByNotWatchedStrategy(nextEpisodeSelectorInput);;
            }

            return GetNextEpisodeByWatchHistory(nextEpisodeSelectorInput, searchResults, usersWatchHistory);
        }

        private static GenericResponse<UserWatchHistory> GetNextEpisodeByNotWatchedStrategy(NextEpisodeSelectorInput input)
        {
            var notWatchedResponse = NextEpisodeSelector.Instance.SelectEpisodeByNotWatchedStrategy(input);

            return !notWatchedResponse.IsOkStatusCode()
                ? new GenericResponse<UserWatchHistory>(notWatchedResponse.Status)
                : new GenericResponse<UserWatchHistory>(Status.Ok, Map(notWatchedResponse.Object, input.Context.UserId));
        }

        private static List<UnifiedSearchResult> GetWatchedAssets(
            NextEpisodeSelectorInput input,
            IEnumerable<WatchHistory> usersWatchHistory,
            eFieldType seriesFilterFieldType,
            string permittedWatchRules)
        {
            var filter = new KsqlBuilder().Equal(input.SeriesIdMetaName, input.SeriesId).Build();
            BooleanPhraseNode filterTree = null;
            BooleanPhraseNode.ParseSearchExpression(filter, ref filterTree);
            ((BooleanLeaf)filterTree).fieldType = seriesFilterFieldType;
            var specificAssets = new Dictionary<eAssetTypes, List<string>>
            {
                { eAssetTypes.MEDIA, usersWatchHistory.Select(x => x.AssetId).ToList() }
            };

            var searchDefinitions = new UnifiedSearchDefinitions()
            {
                groupId = input.Context.GroupId,
                permittedWatchRules = permittedWatchRules,
                specificAssets = specificAssets,
                shouldSearchMedia = true,
                shouldUseFinalEndDate = true,
                shouldUseStartDateForMedia = true,
                shouldAddIsActiveTerm = true,
                filterPhrase = filterTree,
                extraReturnFields = new HashSet<string> {$"metas.{input.EpisodeNumberMetaName}", $"metas.{input.SeasonNumberMetaName}"},
                shouldReturnExtendedSearchResult = true,
                EpgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(input.Context.GroupId),
            };

            ((BooleanLeaf)filterTree).shouldLowercase = true;

            int parentGroupId = CatalogCache.Instance().GetParentGroup(input.Context.GroupId);
            var indexManager = IndexManagerFactory.Instance.GetIndexManager(parentGroupId);
            int esTotalItems = 0;

            return indexManager.UnifiedSearch(searchDefinitions, ref esTotalItems);
        }

        private static GenericResponse<UserWatchHistory> GetNextEpisodeByWatchHistory(
            NextEpisodeSelectorInput input,
            List<UnifiedSearchResult> searchResults,
            List<WatchHistory> usersWatchHistory)
        {
            var episodes = new HashSet<string>(searchResults.Select(x => x.AssetId).ToList());

            var latest = usersWatchHistory
                .Where(x => episodes.Contains(x.AssetId))
                .OrderByDescending(i => i.LastWatch)
                .First();

                if (!latest.IsFinishedWatching)
                {
                    return new GenericResponse<UserWatchHistory>(Status.Ok, ConvertToUserWatchHistory(latest));
                }

                var sr = (ExtendedSearchResult)searchResults.First(x => x.AssetId == latest.AssetId);
                var nextEpisodeResponse = NextEpisodeSelector.Instance.SelectNextEpisode(sr, input);
                if (!nextEpisodeResponse.IsOkStatusCode())
                {
                    return new GenericResponse<UserWatchHistory>(nextEpisodeResponse.Status);
                }

                var result = episodes.Contains(nextEpisodeResponse.Object.AssetId)
                    ? ConvertToUserWatchHistory(
                        usersWatchHistory.First(x => x.AssetId == nextEpisodeResponse.Object.AssetId))
                    : Map(nextEpisodeResponse.Object, input.Context.UserId);

                return new GenericResponse<UserWatchHistory>(Status.Ok, result);
        }

        private static UserWatchHistory ConvertToUserWatchHistory(WatchHistory watchHistory)
        {
            var userWatchHistory = new UserWatchHistory
            {
                AssetId = watchHistory.AssetId,
                AssetType = eAssetTypes.MEDIA,
                AssetTypeId = watchHistory.AssetTypeId,
                Duration = watchHistory.Duration,
                IsFinishedWatching = watchHistory.IsFinishedWatching,
                LastWatch = watchHistory.LastWatch,
                Location = watchHistory.Location,
                m_dUpdateDate = watchHistory.UpdateDate,
                UserID = watchHistory.UserID
            };

            return userWatchHistory;
        }

        private static UserWatchHistory Map(UnifiedSearchResult source, long userId) =>
            new UserWatchHistory
            {
                AssetId = source.AssetId,
                AssetTypeId = (int)eAssetTypes.MEDIA,
                m_dUpdateDate = source.m_dUpdateDate,
                UserID = (int)userId
            };

        public static eFieldType GetFieldType(string fieldName, Group group)
        {
            if (group.m_oGroupTags.Values.Contains(fieldName, StringComparer.OrdinalIgnoreCase))
            {
                return eFieldType.Tag;
            }

            var metas = group.m_oMetasValuesByGroupId.Select(i => i.Value).SelectMany(d => d.Values).ToList();

            if (metas.Contains(fieldName, StringComparer.OrdinalIgnoreCase))
            {
                return eFieldType.StringMeta;
            }

            throw new Exception($"Unknown eFieldType for fieldName='{fieldName}'");
        }

        private static (string seasonNumberMeta, string episodeNumberMeta) GetOpcEpisodeMetaNames(int groupId, CatalogGroupCache catalogGroupCache, AssetStruct episodeStruct)
        {
            var seasonNumberTopic = catalogGroupCache.TopicsMapById.Values.FirstOrDefault(t =>
                t.SystemName.ToLower() == "seasonnumber");
            var episodeNumberTopic = catalogGroupCache.TopicsMapById.Values.FirstOrDefault(t =>
                t.SystemName.ToLower() == "episodenumber");

            if (episodeNumberTopic != null && seasonNumberTopic != null &&
                episodeStruct.MetaIds.Contains(episodeNumberTopic.Id) &&
                episodeStruct.MetaIds.Contains(seasonNumberTopic.Id))
            {
                return ("seasonnumber", "episodenumber");
            }

            // Use TCM metas if default metas are not available.
            var seasonNumberMeta = NotificationCache.Instance().GetSeasonNumberMeta(groupId);
            var episodeNumberMeta = NotificationCache.Instance().GetEpisodeNumberMeta(groupId);

            return (seasonNumberMeta, episodeNumberMeta);
        }

        private static UnifiedSearchResponse FindSeriesAsset(int groupId, IEnumerable<long> seriesIdAssetStructs, string seriesIdMeta, string seriesId)
        {
            var filterQuery = new KsqlBuilder()
                .And(x => x
                    .Or(y => y.Values(x.Equal, CatalogLogic.ASSET_TYPE, seriesIdAssetStructs))
                    .Equal(seriesIdMeta, seriesId))
                .Build();

            return api.SearchAssetsExtended(
                groupId,
                filterQuery,
                0,
                1,
                true,
                0,
                true,
                string.Empty,
                string.Empty,
                string.Empty,
                0,
                groupId,
                false);
        }
    }
}