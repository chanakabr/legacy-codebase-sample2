using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.NextEpisode;
using ApiObjects;
using ApiObjects.MediaMarks;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Catalog;
using Core.Catalog.Cache;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Core.GroupManagers;
using Core.Notification;
using DAL;
using GroupsCacheManager;
using Phx.Lib.Log;
using TVinciShared;

namespace ApiLogic.Catalog
{
    public class UserWatchHistoryManager : IUserWatchHistoryManager
    {
        private static readonly IKLogger Logger = new KLogger(nameof(UserWatchHistoryManager));

        private readonly IMediaMarksDAL _mediaMarksDal;
        private readonly ICatalogManager _catalogManager;
        private readonly NotificationCache _notificationCache;
        private readonly IGeneralPartnerConfigManager _generalPartnerConfigManager;
        private readonly IIndexManagerFactory _indexManagerFactory;
        private readonly IGroupSettingsManager _groupSettingsManager;
        private readonly ICatalogCache _catalogCache;

        private static readonly Lazy<UserWatchHistoryManager> LazyInstance = new Lazy<UserWatchHistoryManager>(
            () => new UserWatchHistoryManager(
                MediaMarksDAL.Instance,
                CatalogManager.Instance,
                NotificationCache.Instance(),
                GeneralPartnerConfigManager.Instance,
                IndexManagerFactory.Instance,
                GroupSettingsManager.Instance,
                CatalogCache.Instance()),
            LazyThreadSafetyMode.PublicationOnly);

        public UserWatchHistoryManager(
            IMediaMarksDAL mediaMarksDal,
            ICatalogManager catalogManager,
            NotificationCache notificationCache,
            IGeneralPartnerConfigManager generalPartnerConfigManager,
            IIndexManagerFactory indexManagerFactory,
            IGroupSettingsManager groupSettingsManager, ICatalogCache catalogCache)
        {
            _mediaMarksDal = mediaMarksDal;
            _catalogManager = catalogManager;
            _notificationCache = notificationCache;
            _generalPartnerConfigManager = generalPartnerConfigManager;
            _indexManagerFactory = indexManagerFactory;
            _groupSettingsManager = groupSettingsManager;
            _catalogCache = catalogCache;
        }

        public static UserWatchHistoryManager Instance => LazyInstance.Value;

        public async Task CleanByRetention(long userId)
        {
            var userWatchHistory = _mediaMarksDal.GetUserMediaMarks(userId);
            var assets = userWatchHistory?.mediaMarks?
                    .Select(x => new KeyValuePair<int, eAssetTypes>(x.AssetId, x.AssetType))
                ?? Enumerable.Empty<KeyValuePair<int, eAssetTypes>>();
            if (assets.Any() && !await _mediaMarksDal.CleanMediaMarkLogsAsync(userId, assets))
            {
                throw new Exception($"Clean process of media mark logs is failed. UserId: {userId}");
            }

            if (!await _mediaMarksDal.CleanUserMediaMarks(userId))
            {
                throw new Exception($"Clean process of media marks is failed. UserId: {userId}");
            }
        }

        public List<WatchHistory> Get(
            int groupId,
            long userId,
            int domainId,
            List<int> assetTypes,
            List<string> assetIds,
            List<int> excludedAssetTypes,
            eWatchStatus filterStatus,
            int numOfDays,
            ApiObjects.SearchObjects.OrderDir orderDir,
            int pageIndex,
            int pageSize,
            bool suppress,
            string filterQuery,
            out int totalItems)
        {
            Logger.DebugFormat("Start getting user watch history for user {0} in groupId {1}", userId, groupId);

            List<WatchHistory> usersWatchHistory = new List<WatchHistory>();

            totalItems = 0;

            try
            {
                if (!string.IsNullOrEmpty(filterQuery)) //BEO - 9621.use filterQuery
                {
                    assetTypes = assetTypes ?? new List<int>();
                    assetIds = assetIds ?? new List<string>();
                }

                var unfilteredResult = GetRaw(groupId, userId, assetTypes, assetIds,
                    excludedAssetTypes, filterStatus, numOfDays);
                Dictionary<string, WatchHistory> seriesMap = new Dictionary<string, WatchHistory>();

                if (unfilteredResult != null && unfilteredResult.Count > 0)
                {
                    unfilteredResult = unfilteredResult.Where(x => x.AssetTypeId != (int)eAssetTypes.UNKNOWN).ToList();

                    var groupManager = new GroupManager();
                    var group = groupManager.GetGroup(groupId);

                    string watchRules = string.Empty;
                    bool isOpc = _catalogManager.DoesGroupUsesTemplates(groupId);

                    if ((group.m_sPermittedWatchRules != null && group.m_sPermittedWatchRules.Count > 0) || isOpc)
                    {
                        if (!isOpc)
                        {
                            watchRules = string.Join(" ", group.m_sPermittedWatchRules);
                        }

                        // validate media on ES
                        UnifiedSearchDefinitions searchDefinitions = new UnifiedSearchDefinitions()
                        {
                            groupId = groupId,
                            permittedWatchRules = watchRules,
                            specificAssets = new Dictionary<eAssetTypes, List<string>>(),
                            shouldAddIsActiveTerm = true,
                            shouldIgnoreDeviceRuleID = true,
                            extraReturnFields = new HashSet<string>()
                        };

                        if (!string.IsNullOrEmpty(filterQuery)) //BEO - 9621.use filterQuery
                        {
                            string filter = filterQuery;
                            BooleanPhraseNode filterTree = null;
                            BooleanPhraseNode.ParseSearchExpression(filter, ref filterTree);

                            searchDefinitions.filterPhrase = filterTree;

                            BaseRequest request = new BaseRequest()
                            {
                                m_sSiteGuid = userId.ToString(),
                                m_nGroupID = groupId,
                                m_dServerTime = DateTime.UtcNow
                                //,m_sUserIP
                            };

                            CatalogLogic.UpdateNodeTreeFields(request, ref filterTree, searchDefinitions, group,
                                groupId); //BEO-10234
                        }

                        int elasticSearchPageSize = 0;
                        string seriesIdExtraReturnField = "metas.seriesid";

                        List<string> listOfMedia = unfilteredResult.Where(x => x.AssetTypeId > 1)
                            .Select(x => x.AssetId.ToString()).ToList();
                        if (listOfMedia.Count > 0)
                        {
                            searchDefinitions.specificAssets.Add(eAssetTypes.MEDIA, listOfMedia);
                            searchDefinitions.shouldSearchMedia = true;
                            searchDefinitions.shouldUseFinalEndDate = true;
                            searchDefinitions.shouldUseStartDateForMedia = true;
                            elasticSearchPageSize += listOfMedia.Count;
                            if (suppress)
                            {
                                if (!isOpc)
                                {
                                    string episodeAssociationTagName = _notificationCache.GetEpisodeAssociationTagName(groupId);

                                    if (!string.IsNullOrEmpty(episodeAssociationTagName))
                                    {
                                        var prefix = GetElasticPrefixByFieldType(episodeAssociationTagName, group);
                                        seriesIdExtraReturnField = $"{prefix}.{episodeAssociationTagName.ToLower()}";
                                    }
                                    else
                                    {
                                        suppress = false;
                                    }
                                }

                                if (suppress)
                                {
                                    searchDefinitions.extraReturnFields.Add(seriesIdExtraReturnField);
                                    searchDefinitions.extraReturnFields.Add("media_type_id");
                                }
                            }
                        }

                        List<string> listOfEpg = unfilteredResult.Where(x => x.AssetTypeId == (int)eAssetTypes.EPG)
                            .Select(x => x.AssetId.ToString()).ToList();
                        if (listOfEpg.Count > 0)
                        {
                            searchDefinitions.specificAssets.Add(eAssetTypes.EPG, listOfEpg);
                            searchDefinitions.shouldSearchEpg = true;
                            searchDefinitions.shouldUseSearchEndDate = true;
                            searchDefinitions.shouldUseStartDateForEpg = false;
                            searchDefinitions.shouldUseEndDateForEpg = false;
                            elasticSearchPageSize += listOfEpg.Count;
                        }

                        searchDefinitions.pageSize = elasticSearchPageSize;
                        searchDefinitions.shouldReturnExtendedSearchResult =
                            searchDefinitions.extraReturnFields?.Count > 0;

                        searchDefinitions.EpgFeatureVersion = _groupSettingsManager.GetEpgFeatureVersion(groupId);

                        if (elasticSearchPageSize > 0)
                        {
                            List<int> activeMediaIds = new List<int>();
                            List<int> activeEpg = new List<int>();

                            int parentGroupId = _catalogCache.GetParentGroup(groupId);
                            var indexManager = _indexManagerFactory.GetIndexManager(parentGroupId);
                            int esTotalItems = 0;
                            var searchResults = indexManager.UnifiedSearch(searchDefinitions, ref esTotalItems);

                            long? episodeStructId = 0;

                            if (searchResults != null && searchResults.Count > 0)
                            {
                                foreach (var searchResult in searchResults)
                                {
                                    int assetId = int.Parse(searchResult.AssetId);

                                    if (searchResult.AssetType == eAssetTypes.MEDIA)
                                    {
                                        bool addToList = true;
                                        var watched = unfilteredResult.First(x => int.Parse(x.AssetId) == assetId && x.AssetTypeId > 1);
                                        watched.UpdateDate = searchResult.m_dUpdateDate;

                                        if (suppress)
                                        {
                                            ExtendedSearchResult ecr = (ExtendedSearchResult)searchResult;
                                            string seriesId = Core.Api.api.GetStringParamFromExtendedSearchResult(ecr, seriesIdExtraReturnField);

                                            if (!string.IsNullOrEmpty(seriesId))
                                            {
                                                if (episodeStructId == 0)
                                                {
                                                    if (isOpc)
                                                    {
                                                        if (_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out var catalogGroupCache))
                                                        {
                                                            var seriesStructId = catalogGroupCache.AssetStructsMapById
                                                                .Values.FirstOrDefault(x => x.IsSeriesAssetStruct)?.Id;

                                                            episodeStructId = seriesStructId == null
                                                                ? null
                                                                : catalogGroupCache.AssetStructsMapById
                                                                    .Values.FirstOrDefault(x =>
                                                                        x.ParentId > 0 && x.ParentId == seriesStructId)
                                                                    ?.Id;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        episodeStructId = _notificationCache.GetEpisodeMediaTypeId(groupId);
                                                    }
                                                }

                                                string mediaTypeIdValue = Core.Api.api.GetStringParamFromExtendedSearchResult(ecr, "media_type_id");
                                                if (long.TryParse(mediaTypeIdValue, out var mediaTypeId)
                                                    && (mediaTypeId == episodeStructId || (episodeStructId == null && IsMediaTypeEpisodeLike(isOpc, groupId, mediaTypeId))))
                                                {
                                                    if (!seriesMap.ContainsKey(seriesId))
                                                    {
                                                        seriesMap.Add(seriesId, watched);
                                                    }
                                                    else
                                                    {
                                                        if (watched.LastWatch > seriesMap[seriesId].LastWatch)
                                                        {
                                                            activeMediaIds.Remove(int.Parse(seriesMap[seriesId].AssetId));
                                                            seriesMap[seriesId] = watched;
                                                        }
                                                        else
                                                        {
                                                            addToList = false;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (addToList)
                                        {
                                            activeMediaIds.Add(assetId);
                                        }
                                    }

                                    if (searchResult.AssetType == eAssetTypes.EPG)
                                    {
                                        activeEpg.Add(assetId);
                                        unfilteredResult.First(x => int.Parse(x.AssetId) == assetId && x.AssetTypeId == (int)eAssetTypes.EPG).UpdateDate
                                            = searchResult.m_dUpdateDate;
                                    }
                                }
                            }

                            //remove medias that are not active
                            unfilteredResult.RemoveAll(x => x.AssetTypeId > 1 && !activeMediaIds.Contains(int.Parse(x.AssetId)));

                            //remove programs that are not active
                            unfilteredResult.RemoveAll(x => x.AssetTypeId == (int)eAssetTypes.EPG && !activeEpg.Contains(int.Parse(x.AssetId)));
                        }

                        var unFilteredRecordings = unfilteredResult.Where(x => x.AssetTypeId == (int)eAssetTypes.NPVR);

                        if (unFilteredRecordings != null)
                        {
                            var recordings = Core.ConditionalAccess.Module.SearchDomainRecordings(
                                groupId,
                                userId.ToString(),
                                domainId,
                                new[] { TstvRecordingStatus.Recorded },
                                string.Empty,
                                0,
                                0,
                                new OrderObj { m_eOrderBy = OrderBy.ID, m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC },
                                true,
                                null);

                            if (recordings != null && recordings.Recordings?.Count > 0)
                            {
                                foreach (var item in unFilteredRecordings)
                                {
                                    var recording = recordings.Recordings.FirstOrDefault(x => x.Id.ToString().Equals(item.AssetId));
                                    if (recording != null)
                                    {
                                        item.EpgId = recording.EpgId;
                                    }
                                }
                            }

                            //remove recordings that are not active
                            unfilteredResult.RemoveAll(x => x.AssetTypeId == (int)eAssetTypes.NPVR && x.EpgId == 0);
                        }
                    }

                    // order list
                    switch (orderDir)
                    {
                        case ApiObjects.SearchObjects.OrderDir.ASC:

                            unfilteredResult = unfilteredResult.OrderBy(x => x.LastWatch).ToList();
                            break;
                        case ApiObjects.SearchObjects.OrderDir.DESC:
                        case ApiObjects.SearchObjects.OrderDir.NONE:
                        default:

                            unfilteredResult = unfilteredResult.OrderByDescending(x => x.LastWatch).ToList();
                            break;
                    }

                    // update total items
                    totalItems = unfilteredResult.Count;

                    // page index /size. if size and index are 0 return all
                    if (pageSize == 0 && pageIndex == 0)
                    {
                        usersWatchHistory = unfilteredResult;
                    }
                    else
                    {
                        usersWatchHistory = unfilteredResult.Skip(pageSize * pageIndex).Take(pageSize).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(
                    $"An Exception was occurred on getting user watch history. groupId:{groupId}, siteGuid:{userId}, filterStatus:{filterStatus}, numOfDays:{numOfDays}.",
                    ex);
                throw;
            }

            return usersWatchHistory;
        }

        public List<WatchHistory> GetRaw(int groupId, long userId, List<int> assetTypes,
            List<string> assetIds, List<int> excludedAssetTypes, eWatchStatus filterStatus, int numOfDays)
        {
            List<WatchHistory> unfilteredResult = new List<WatchHistory>();

            try
            {
                var mediaMarkLogs = GetUserMediaMarks(groupId, userId, numOfDays).ToList();
                if (mediaMarkLogs.Count == 0) return unfilteredResult;

                int finishedPercent = CatalogLogic.FINISHED_PERCENT_THRESHOLD;
                var generalPartnerConfig = _generalPartnerConfigManager.GetGeneralPartnerConfig(groupId);
                if (generalPartnerConfig?.FinishedPercentThreshold.HasValue == true)
                {
                    finishedPercent = generalPartnerConfig.FinishedPercentThreshold.Value;
                }

                unfilteredResult = mediaMarkLogs.Select(mediaMarkLog =>
                {
                    int.TryParse(mediaMarkLog.NpvrID, out var recordingId);

                    return new WatchHistory()
                    {
                        AssetId = mediaMarkLog.AssetID.ToString(),
                        Duration = mediaMarkLog.FileDuration,
                        AssetTypeId = mediaMarkLog.AssetTypeId,
                        LastWatch = mediaMarkLog.CreatedAtEpoch,
                        Location = mediaMarkLog.AssetAction.ToLower().Equals("finish") ? mediaMarkLog.FileDuration : mediaMarkLog.Location,
                        RecordingId = recordingId,
                        UserID = (int)userId,
                        IsFinishedWatching = mediaMarkLog.IsFinished(finishedPercent)
                    };
                }).ToList();

                // filter status
                switch (filterStatus)
                {
                    case eWatchStatus.Progress:
                        // remove all finished
                        unfilteredResult.RemoveAll(x => x.IsFinishedWatching);
                        break;
                    case eWatchStatus.Done:
                        // remove all in progress
                        unfilteredResult.RemoveAll(x => !x.IsFinishedWatching);
                        break;
                    case eWatchStatus.All:
                    default:
                        break;
                }

                // filter asset types
                if (assetTypes != null && assetTypes.Count > 0)
                    unfilteredResult = unfilteredResult.Where(x => assetTypes.Contains(x.AssetTypeId)).ToList();

                // filter asset ids
                if (assetIds != null && assetIds.Count > 0)
                    unfilteredResult = unfilteredResult.Where(x => assetIds.Contains(x.AssetId)).ToList();

                // filter excluded asset types
                if (excludedAssetTypes != null && excludedAssetTypes.Count > 0)
                    unfilteredResult.RemoveAll(x => excludedAssetTypes.Contains(x.AssetTypeId));
            }
            catch (Exception)
            {
                // ignored
            }

            return unfilteredResult;
        }

        public IEnumerable<UserMediaMark> GetUserMediaMarks(int groupId, long userId, int numOfDays)
        {
            var userAssetMarks = _mediaMarksDal.GetUserMediaMarks(userId);
            if (userAssetMarks?.mediaMarks == null || userAssetMarks.mediaMarks.Count == 0)
            {
                return Enumerable.Empty<UserMediaMark>();
            }

            // build date filter
            long minFilterDate = numOfDays > 0 ? DateTime.UtcNow.AddDays(-numOfDays).ToUtcUnixTimestampSeconds() : 0;
            var utcNow = DateUtils.GetUtcUnixTimestampNow();
            var dateFilteredResult = userAssetMarks.mediaMarks
                .Where(mark => mark.CreatedAt > minFilterDate && (mark.ExpiredAt == 0 || mark.ExpiredAt > utcNow));

            return GetAssetMarks(dateFilteredResult, userId, groupId);
        }

        //CleanUserAssetHistory - new method
        public Status Clean(int groupId, long userId, List<KeyValuePair<int, eAssetTypes>> assets)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                if (MediaMarksNewModel.Enabled(groupId))
                {
                    if (!_mediaMarksDal.CleanUserMediaMarks(userId, assets))
                    {
                        return response;
                    }
                }

                if (_mediaMarksDal.CleanMediaMarkLogs(userId, assets))
                {
                    response = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                Logger.Error("CleanUserHistory - Error = " + ex.Message, ex);
            }

            return response;
        }

        // TODO aggressive migration
        private static IEnumerable<UserMediaMark> GetAssetMarks(IEnumerable<AssetAndLocation> mediaMarks, long userId, int groupId)
        {
            var oldModelMediaMarks = new List<AssetAndLocation>();
            var mediaMarksNewModelEnabled = MediaMarksNewModel.Enabled(groupId);
            foreach (var mediaMark in mediaMarks)
            {
                var e = mediaMark.Extra;
                if (!mediaMarksNewModelEnabled || e == null)
                {
                    oldModelMediaMarks.Add(mediaMark);
                    continue;
                }

                yield return new UserMediaMark
                {
                    UDID = e.UDID,
                    AssetID = mediaMark.AssetId,
                    UserID = (int)userId,
                    Location = e.Location,
                    CreatedAt = DateTimeOffset.FromUnixTimeSeconds(mediaMark.CreatedAt).UtcDateTime,
                    NpvrID = mediaMark.NpvrId,
                    playType = e.PlayType.ToString(),
                    FileDuration = e.FileDuration,
                    AssetAction = e.AssetAction.ToString(),
                    AssetTypeId = e.AssetTypeId,
                    CreatedAtEpoch = mediaMark.CreatedAt,
                    MediaConcurrencyRuleIds = null, // never read
                    AssetType = mediaMark.AssetType,
                    ExpiredAt = mediaMark.ExpiredAt,
                    LocationTagValue = e.LocationTagValue
                };
            }

            var mediaMarkLogsDictionary = MediaMarksDAL.GetMediaMarkLogs(userId, oldModelMediaMarks);
            foreach (var kv in mediaMarkLogsDictionary)
            {
                yield return kv.Value.LastMark;
            }
        }

        private static string GetElasticPrefixByFieldType(string fieldName, Group group)
        {
            var eFieldType = NextEpisodeService.GetFieldType(fieldName, group);
            switch (eFieldType)
            {
                case eFieldType.Tag:
                    return "tags";
                case eFieldType.StringMeta:
                    return "metas";
                default:
                    throw new Exception($"Unknown field type for fieldName={fieldName}");
            }
        }

        private bool IsMediaTypeEpisodeLike(bool isOpc, int groupId, long mediaTypeId)
        {
            if (isOpc && _catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out var catalogGroupCache))
            {
                if (!catalogGroupCache.AssetStructsMapById.ContainsKey(mediaTypeId))
                {
                    return false;
                }

                return catalogGroupCache.AssetStructsMapById[mediaTypeId].ParentId != null;
            }

            return false;
        }
    }
}