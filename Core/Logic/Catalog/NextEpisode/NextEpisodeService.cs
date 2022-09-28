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
using Core.GroupManagers;

namespace ApiLogic.Catalog.NextEpisode
{
    public static class NextEpisodeService
    {
        public static GenericResponse<UserWatchHistory> GetNextEpisodeForOpc(int groupId, string siteGuid, long assetId, Func<List<WatchHistory>> getWatchHistoryFunc)
        {
            var response = new GenericResponse<UserWatchHistory>();

            string seriesId = string.Empty;
            var assetResponse = AssetManager.Instance.GetAsset(groupId, assetId, eAssetTypes.MEDIA, false);
            if (!assetResponse.HasObject())
            {
                response.SetStatus(assetResponse.Status);
                return response;
            }

            if (!(assetResponse.Object is MediaAsset))
            {
                response.SetStatus(eResponseStatus.InvalidAssetType, "asset is not media type");
                return response;
            }

            var media = (MediaAsset) assetResponse.Object;
            AssetStruct episodeStruct = null;
            string seasonNumberMeta = null;
            string episodeNumberMeta = null;

            if (CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId,
                out CatalogGroupCache catalogGroupCache))
            {
                var seriesStruct =
                    catalogGroupCache.AssetStructsMapById.Values.FirstOrDefault(s => s.IsSeriesAssetStruct);

                // BEO-12018 - Validation for old behavior with single Series struct.
                if (seriesStruct != null && media.MediaType.m_nTypeID != seriesStruct.Id)
                {
                    response.SetStatus(eResponseStatus.InvalidAssetStruct, "AssetStruct is not from Series type");
                    return response;
                }

                // BEO-12018 - Series asset struct is an asset struct with child asset struct. No requirement to have "series" system name.
                // Same for episode. First check with "episode" system name to be sure that are using correct children if it exists.
                episodeStruct =
                    catalogGroupCache.AssetStructsMapById.Values.FirstOrDefault(x =>
                        x.ParentId == media.MediaType.m_nTypeID && x.SystemName.ToLower() == "episode") ??
                    catalogGroupCache.AssetStructsMapById.Values.FirstOrDefault(x =>
                        x.ParentId == media.MediaType.m_nTypeID);

                if (episodeStruct == null)
                {
                    response.SetStatus(eResponseStatus.InvalidAssetStruct, "Episode AssetStruct does not exist");
                    return response;
                }

                if (episodeStruct.ConnectingMetaId == null)
                {
                    response.SetStatus(eResponseStatus.InvalidAssetStruct, "Episode AssetStruct has no ConnectedMetaId");
                    return response;
                }

                if (!catalogGroupCache.TopicsMapById.ContainsKey(episodeStruct.ConnectingMetaId.Value))
                {
                    response.SetStatus(eResponseStatus.TopicNotFound, "Series Topic does not exist");
                    return response;
                }

                var seriesTopic = catalogGroupCache.TopicsMapById[episodeStruct.ConnectingMetaId.Value];
                var seriesMeta = media.Metas.FirstOrDefault(x => x.m_oTagMeta.m_sName == seriesTopic.SystemName);
                if (seriesMeta != null)
                {
                    seriesId = seriesMeta.m_sValue;
                }

                (seasonNumberMeta, episodeNumberMeta) = GetOpcEpisodeMetaNames(groupId, catalogGroupCache, episodeStruct);

                if (string.IsNullOrEmpty(seasonNumberMeta) || string.IsNullOrEmpty(episodeNumberMeta))
                {
                    response.SetStatus(eResponseStatus.InvalidAssetStruct, "Episode AssetStruct has no Episode Number or Season Number meta");
                    return response;
                }
            }

            if (string.IsNullOrEmpty(seriesId))
            {
                response.SetStatus(eResponseStatus.MetaDoesNotExist, "SeriesId meta does not exist");
                return response;
            }

            if (episodeStruct == null)
            {
                response.SetStatus(eResponseStatus.InvalidAssetStruct, "Episode AssetStruct does not exist");
                return response;
            }

            var episodeStructId = episodeStruct.Id;

            var permittedWatchRules = string.Empty;

            var seriesFilter = $"seriesId='{seriesId.ToLower()}'";

            return GetNextEpisode(
                groupId,
                siteGuid,
                episodeStructId,
                seasonNumberMeta,
                episodeNumberMeta,
                eFieldType.StringMeta,
                permittedWatchRules,
                seriesFilter,
                groupId,
                getWatchHistoryFunc);
        }

        public static GenericResponse<UserWatchHistory> GetNextEpisodeForTvm(int groupId, string siteGuid, long assetId, Func<List<WatchHistory>> getWatchHistoryFunc)
        {
            var seriesName = string.Empty;

            var groupManager = new GroupManager();
            var group = groupManager.GetGroup(groupId);

            var episodeAssociationTagName = NotificationCache.Instance().GetEpisodeAssociationTagName(groupId);
            var episodeStructId = NotificationCache.Instance().GetEpisodeMediaTypeId(groupId);

            var assetFilter = new Filter()
            {
                m_bOnlyActiveMedia = true,
                m_bUseStartDate = true
            };

            var assetsToRetrieve = new List<BaseObject>() { new BaseObject() { AssetId = assetId.ToString(), AssetType = eAssetTypes.MEDIA } };

            var assets = Core.Catalog.Utils.GetOrderedAssets(groupId, assetsToRetrieve, assetFilter, false);

            if (assets == null || assets.Count == 0)
            {
                var response = new GenericResponse<UserWatchHistory>();
                response.SetStatus(eResponseStatus.AssetDoesNotExist, $"Asset with assetId = {assetId} not found");
                return response;
            }

            var mo = (MediaObj)assets.Single();
            seriesName = mo.m_sName;

            var permittedWatchRules = string.Join(" ", group.m_sPermittedWatchRules);

            var seriesFilter = $"{episodeAssociationTagName}='{seriesName}'".ToLower();

            var seriesFilterFieldType = GetFieldType(episodeAssociationTagName, group);

            var seasonNumberMeta = NotificationCache.Instance().GetSeasonNumberMeta(groupId);
            var episodeNumberMeta = NotificationCache.Instance().GetEpisodeNumberMeta(groupId);

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
                groupId,
                siteGuid,
                episodeStructId,
                seasonNumberMeta,
                episodeNumberMeta,
                seriesFilterFieldType,
                permittedWatchRules,
                seriesFilter,
                exactGroupId: 0,
                getWatchHistoryFunc);
        }

        private static GenericResponse<UserWatchHistory> GetNextEpisode(
            int groupId,
            string siteGuid,
            long episodeStructId,
            string seasonNumberMetaName,
            string episodeNumberMetaName,
            eFieldType seriesFilterFieldType,
            string permittedWatchRules,
            string seriesFilter,
            int exactGroupId,
            Func<List<WatchHistory>> getWatchHistoryFunc)
        {
            var response = new GenericResponse<UserWatchHistory>();

            response.SetStatus(eResponseStatus.NoNextEpisode, "User have not started watching this TV series");

            var usersWatchHistory = getWatchHistoryFunc();

            if (usersWatchHistory?.Count > 0)
            {
                string filter = seriesFilter.ToLower();
                BooleanPhraseNode filterTree = null;
                var status = BooleanPhraseNode.ParseSearchExpression(filter, ref filterTree);
                ((BooleanLeaf)filterTree).fieldType = seriesFilterFieldType;
                var specificAssets = new Dictionary<eAssetTypes, List<string>>();
                specificAssets.Add(eAssetTypes.MEDIA, usersWatchHistory.Select(x => x.AssetId).ToList());

                UnifiedSearchDefinitions searchDefinitions = new UnifiedSearchDefinitions()
                {
                    groupId = groupId,
                    permittedWatchRules = permittedWatchRules,
                    specificAssets = specificAssets,
                    shouldSearchMedia = true,
                    shouldUseFinalEndDate = true,
                    shouldUseStartDateForMedia = true,
                    shouldAddIsActiveTerm = true,
                    filterPhrase = filterTree,
                    extraReturnFields = new HashSet<string>() {$"metas.{episodeNumberMetaName}", $"metas.{seasonNumberMetaName}"},
                    shouldReturnExtendedSearchResult = true,
                    EpgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(groupId),
                    
                };

                ((BooleanLeaf)filterTree).shouldLowercase = true;

                int parentGroupId = CatalogCache.Instance().GetParentGroup(groupId);
                var indexManager = IndexManagerFactory.Instance.GetIndexManager(parentGroupId);
                int esTotalItems = 0;
                var searchResults = indexManager.UnifiedSearch(searchDefinitions, ref esTotalItems);

                if (searchResults != null && searchResults.Count > 0)
                {
                    var episodes = new HashSet<string>(searchResults.Select(x => x.AssetId).ToList());

                    var latest = usersWatchHistory.Where(x => episodes.Contains(x.AssetId))
                        .OrderByDescending(i => i.LastWatch).First();

                    response.SetStatus(eResponseStatus.OK);
                    response.Object = ConvertToUserWatchHistory(latest);

                    if (!latest.IsFinishedWatching)
                    {
                        return response;
                    }

                    var sr = (ExtendedSearchResult) searchResults.First(x => x.AssetId == latest.AssetId);

                    int episodeNumber;
                    int seasonNumber;

                    int.TryParse(Core.Api.api.GetStringParamFromExtendedSearchResult(sr, $"metas.{episodeNumberMetaName}"), out episodeNumber);
                    int.TryParse(Core.Api.api.GetStringParamFromExtendedSearchResult(sr, $"metas.{seasonNumberMetaName}"), out seasonNumber);

                    filter =
                        $"(and asset_type='{episodeStructId}' {seriesFilter}" +
                        $" (or (and {seasonNumberMetaName}='{seasonNumber}' {episodeNumberMetaName}='{episodeNumber + 1}')" +
                        $" (and {seasonNumberMetaName}='{seasonNumber + 1}' {episodeNumberMetaName}='1') ) )";

                    var nextAsset = Core.Api.api.SearchAssetsExtended(groupId, filter, 0, 1, true, 0, true, "", "", "",
                        0, exactGroupId, false, false,
                        new List<string>() {$"metas.{episodeNumberMetaName}", $"metas.{seasonNumberMetaName}"},
                        new OrderObj()
                        {
                            m_eOrderBy = OrderBy.META,
                            m_sOrderValue = seasonNumberMetaName,
                            m_eOrderDir = OrderDir.ASC
                        });

                    if (nextAsset != null && nextAsset.searchResults?.Count > 0)
                    {
                        var item = nextAsset.searchResults[0];
                        //int assetId = int.Parse(searchResults[0].AssetId);
                        if (episodes.Contains(item.AssetId))
                        {
                            response.Object = ConvertToUserWatchHistory(usersWatchHistory.First(x => x.AssetId == item.AssetId));
                            return response;
                        }

                        response.Object = new UserWatchHistory
                        {
                            AssetId = item.AssetId,
                            AssetTypeId = (int) eAssetTypes.MEDIA,
                            m_dUpdateDate = searchResults[0].m_dUpdateDate,
                            UserID = int.Parse(siteGuid),
                        };
                    }
                }
            }

            return response;
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
    }
}