using ApiLogic.Notification.Managers;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Epg;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.GroupManagers;
using EpgBL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ApiLogic.Catalog.CatalogManagement.Helpers;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Catalog.CatalogManagement.Services;
using Core.Api;
using ElasticSearch.Utilities;
using EventBus.Kafka;
using OTT.Lib.Kafka;
using Tvinci.Core.DAL;
using TVinciShared;
using MetaType = ApiObjects.MetaType;

namespace Core.Catalog.CatalogManagement
{
    public interface IEpgAssetManager
    {
        IEnumerable<EpgAsset> GetEpgAssets(long groupId, IEnumerable<long> epgIds, IEnumerable<string> languages);
    }

    public class EpgAssetManager : IEpgAssetManager
    {
        #region Constants and Read-only

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<EpgAssetManager> Lazy = new Lazy<EpgAssetManager>(() => new EpgAssetManager(), LazyThreadSafetyMode.PublicationOnly);

        private const string EPGS_PROGRAM_DATES_ERROR = "Error at EPG Program Start/End Dates";
        private const string META_DOES_NOT_EXIST = "{0}: {1} does not exist for this group";
        private const string INVALID_LANGUAGE = "Invalid language: {0}. Only languages specified in the name of the asset can be associated.";
        private const string DUPLICATE_VALUE = "Duplicate {0}:{1} sent for {2}.";
        private const string EXTERNAL_ID_META_NAME = "External Asset ID";
        private const string CRID_META_NAME = "CRID";
        private const string START_DATE_META_NAME = "Program Start";
        private const string END_DATE_META_NAME = "Program End";
        public const string NAME_META_SYSTEM_NAME = "Name";
        public const string DESCRIPTION_META_SYSTEM_NAME = "Description";
        public const string START_DATE_META_SYSTEM_NAME = "StartDate";
        public const string END_DATE_META_SYSTEM_NAME = "EndDate";
        public const string SERIES_NAME_META_SYSTEM_NAME = "SeriesName";
        private const string SERIES_NAME_META_NAME = "Series Name";
        public const string SERIES_ID_META_SYSTEM_NAME = "SeriesID";
        private const string SERIES_ID_META_NAME = "Series ID";
        public const string EPISODE_NUMBER_META_SYSTEM_NAME = "EpisodeNumber";
        private const string EPISODE_NUMBER_META_NAME = "Episode Number";
        public const string SEASON_NUMBER_META_SYSTEM_NAME = "SeasonNumber";
        private const string SEASON_NUMBER_META_NAME = "Season Number";
        public const string PARENTAL_RATING_META_SYSTEM_NAME = "ParentalRating";
        private const string PARENTAL_RATING_META_NAME = "Parental Rating";
        public const string GENRE_META_SYSTEM_NAME = "Genre";
        public const string CRID_META_SYSTEM_NAME = "Crid";
        public const string CRID_META_SYSTEM_NAME_UPPER = "CRID";
        public const string EXTERNAL_ID_META_SYSTEM_NAME = "ExternalID";
        public const string STATUS_META_SYSTEM_NAME = "Status";
        public const string EXTERNAL_OFFER_IDS_META_SYSTEM_NAME = "ExternalOfferIds";
        private static readonly int MaxDescriptionSize = 1024;
        private static readonly int MaxNameSize = 255;
        private static IProgramAssetCrudMessageService _messageService;

        public static readonly Dictionary<string, string> BasicProgramMetasSystemNameToName = new Dictionary<string, string>()
        {
            { NAME_META_SYSTEM_NAME, NAME_META_SYSTEM_NAME },
            { DESCRIPTION_META_SYSTEM_NAME, DESCRIPTION_META_SYSTEM_NAME },
            { START_DATE_META_SYSTEM_NAME, START_DATE_META_NAME },
            { END_DATE_META_SYSTEM_NAME, END_DATE_META_NAME },
            { SERIES_NAME_META_SYSTEM_NAME, SERIES_NAME_META_NAME },
            { SERIES_ID_META_SYSTEM_NAME, SERIES_ID_META_NAME },
            { EPISODE_NUMBER_META_SYSTEM_NAME, EPISODE_NUMBER_META_NAME },
            { SEASON_NUMBER_META_SYSTEM_NAME, SEASON_NUMBER_META_NAME },
            { PARENTAL_RATING_META_SYSTEM_NAME, PARENTAL_RATING_META_NAME },
            { GENRE_META_SYSTEM_NAME, GENRE_META_SYSTEM_NAME },
            { CRID_META_SYSTEM_NAME, CRID_META_NAME },
            { EXTERNAL_ID_META_SYSTEM_NAME, EXTERNAL_ID_META_NAME }
        };

        internal static readonly HashSet<string> TopicsInBasicProgramTable = new HashSet<string>()
        {
            NAME_META_SYSTEM_NAME, DESCRIPTION_META_SYSTEM_NAME, EXTERNAL_ID_META_SYSTEM_NAME, CRID_META_SYSTEM_NAME, START_DATE_META_SYSTEM_NAME, END_DATE_META_SYSTEM_NAME, STATUS_META_SYSTEM_NAME, EXTERNAL_OFFER_IDS_META_SYSTEM_NAME
        };

        internal static readonly Dictionary<string, string> BasicMetasSystemNamesToType = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { NAME_META_SYSTEM_NAME, MetaType.MultilingualString.ToString() },
            { START_DATE_META_SYSTEM_NAME, MetaType.DateTime.ToString() },
            { END_DATE_META_SYSTEM_NAME, MetaType.DateTime.ToString() },
            { CRID_META_SYSTEM_NAME, MetaType.String.ToString() }
        };

        public static readonly HashSet<string> RecordingFieldsSystemName = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            SERIES_ID_META_SYSTEM_NAME, SEASON_NUMBER_META_SYSTEM_NAME, EPISODE_NUMBER_META_SYSTEM_NAME
        };

        public static EpgAssetManager Instance => Lazy.Value;

        #endregion

        #region Internal Methods

        internal static List<EpgAsset> GetEpgAssetsFromCache(List<long> epgIds, int groupId, List<string> languageCodes = null,
            Dictionary<string, string> epgIdToDocumentId = null)
        {
            Dictionary<string, EpgAsset> epgAssets = new Dictionary<string, EpgAsset>();

            try
            {
                eAssetTypes assetType = eAssetTypes.EPG;
                Dictionary<string, string> keyToOriginalValueMap = LayeredCacheKeys.GetAssetsKeyMap(assetType.ToString(), epgIds);
                Dictionary<string, List<string>> invalidationKeysMap = LayeredCacheKeys.GetAssetsInvalidationKeysMap(groupId, assetType.ToString(), epgIds);

                if (!LayeredCache.Instance.GetValues<EpgAsset>(keyToOriginalValueMap,
                                                               ref epgAssets,
                                                               GetEpgAssets,
                                                               new Dictionary<string, object>()
                                                               {
                                                                  { "groupId", groupId },
                                                                  { "epgIds", epgIds },
                                                                  { "languageCodes", languageCodes },
                                                                  { "epgIdToDocumentId", epgIdToDocumentId ?? new Dictionary<string, string>() }
                                                               },
                                                               groupId,
                                                               LayeredCacheConfigNames.GET_EPG_ASSETS_CACHE_CONFIG_NAME,
                                                               invalidationKeysMap))
                {
                    log.ErrorFormat("Failed getting GetEpgAssetsFromCache from LayeredCache, groupId: {0}, epgIds: {1}", groupId, string.Join(",", epgIds));
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetEpgAssetsFromCache with groupId: {0}, epgIds: {1}", groupId, string.Join(",", epgIds)), ex);
            }

            return epgAssets.Values.ToList();
        }

        internal static GenericResponse<Asset> AddEpgAsset(int groupId, EpgAsset epgAssetToAdd, long userId, CatalogGroupCache catalogGroupCache)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            long newEpgId = 0;

            try
            {
                Dictionary<string, string> allNames;
                Dictionary<string, Dictionary<string, List<string>>> epgMetas;
                List<int> epgTagsIds;
                Dictionary<FieldTypes, Dictionary<string, int>> mappingFields = GetMappingFields(groupId);

                Status validateStatus = ValidateEpgAssetForInsert(groupId, userId, epgAssetToAdd, catalogGroupCache, mappingFields, out allNames, out epgMetas, out epgTagsIds);

                if (!validateStatus.IsOkStatusCode())
                {
                    result.SetStatus(validateStatus);
                    return result;
                }

                // get Description meta values
                var allDescriptions = GetSystemTopicValues(epgAssetToAdd.Description, epgAssetToAdd.DescriptionsWithLanguages,
                                                           catalogGroupCache, AssetManager.DESCRIPTION_META_SYSTEM_NAME, true, allNames);

                if (!allDescriptions.HasObject())
                {
                    result.SetStatus(allDescriptions.Status);
                    return result;
                }

                string defaultLanguageCode = catalogGroupCache.GetDefaultLanguage().Code;
                DateTime dateTimeNow = DateTime.UtcNow;
                EpgCB epgCbToAdd = CreateEpgCbFromEpgAsset(epgAssetToAdd, groupId, dateTimeNow, dateTimeNow);

                epgCbToAdd.Name = allNames[defaultLanguageCode];
                epgCbToAdd.Description = allDescriptions.Object.ContainsKey(defaultLanguageCode) ? allDescriptions.Object[defaultLanguageCode] : string.Empty;

                Dictionary<long, List<string>> epgMetaIdToValues = new Dictionary<long, List<string>>();
                if (epgMetas != null)
                {
                    foreach (var item in epgMetas[defaultLanguageCode])
                    {
                        if (mappingFields.ContainsKey(FieldTypes.Meta) && mappingFields[FieldTypes.Meta].ContainsKey(item.Key.ToLower()))
                        {
                            var metaId = (long)mappingFields[FieldTypes.Meta][item.Key.ToLower()];
                            if (!epgMetaIdToValues.ContainsKey(metaId))
                            {
                                epgMetaIdToValues.Add(metaId, new List<string>());
                            }

                            epgMetaIdToValues[metaId].AddRange(item.Value);
                        }
                    }
                }

                var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(groupId);
                if (epgFeatureVersion == EpgFeatureVersion.V1)
                {
                    newEpgId = EpgDal.InsertEpgToDB(epgCbToAdd, userId, dateTimeNow, epgMetaIdToValues, catalogGroupCache.GetDefaultLanguage().ID, epgTagsIds);
                    if (newEpgId == 0)
                    {
                        log.Error("Inesrt epg to epg_channels_schedule failed");
                        return result;
                    }
                }
                else // V2 , V3 using CB to generate IDs and do not use Sql
                {
                    var epgBl = new TvinciEpgBL(groupId);
                    newEpgId = (long)epgBl.GetNewEpgId();

                    // TODO: Why is this field assigned, it is never in use.
                    epgCbToAdd.IsIngestV2 = true;
                }

                epgCbToAdd.EpgID = (ulong)newEpgId;

                // insert epg tags to DB in main lang
                var epgTags = GetEpgTags(epgAssetToAdd.Tags, allNames, defaultLanguageCode);

                // insert epgCb to CB in all languages
                var epgsToIndex = SaveEpgCbToCB(groupId, epgCbToAdd, defaultLanguageCode, allNames, allDescriptions.Object, epgMetas, epgTags, true);

                var linearChannelSettingsForEpgCb = Cache.CatalogCache.Instance().GetLinearChannelSettings(groupId, new List<string>() { epgCbToAdd.ChannelID.ToString() });
                bool indexingResult = IndexManagerFactory.Instance.GetIndexManager(groupId).UpsertProgram(
                                      epgsToIndex,
                                      linearChannelSettingsForEpgCb
                                      );

                InvalidateEpgs(groupId,
                    epgsToIndex.Select(x => (long)x.EpgID), CatalogManager.Instance.DoesGroupUsesTemplates(groupId),
                    epgsToIndex.Select(item => item.ChannelID.ToString()).Distinct().ToList(), false);

                if (!indexingResult)
                {
                    log.ErrorFormat("Failed UpsertProgram index for epg ExternalId: {0}, groupId: {1} after AddEpgAsset", epgAssetToAdd.EpgIdentifier, groupId);
                }

                SendActionEvent(groupId, newEpgId, eAction.On);

                result = AssetManager.Instance.GetAsset(groupId, newEpgId, eAssetTypes.EPG, true);
                if (result.IsOkStatusCode())
                {
                    _messageService.PublishCreateEventAsync(groupId, newEpgId, userId)?.GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed AddEpgAsset for groupId: {groupId}, epg ExternalId: {epgAssetToAdd.EpgIdentifier}, Exception: [{ex}].");
            }

            return result;
        }

        internal static GenericResponse<Asset> UpdateEpgAsset(int groupId, EpgAsset epgAssetToUpdate, long userId, EpgAsset oldEpgAsset, CatalogGroupCache catalogGroupCache)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            try
            {
                DateTime updateDate = DateTime.UtcNow;
                string defaultLanguageCode = catalogGroupCache.GetDefaultLanguage().Code;
                EpgCB epgCBToUpdate = null;

                bool needToUpdateBasicData;
                Dictionary<string, string> allNames;
                Dictionary<string, Dictionary<string, List<string>>> epgMetas;
                bool needToUpdateMetas;
                List<int> epgTagsIds;
                bool needToUpdateTags;
                bool validateSystemTopicDescription;
                Dictionary<FieldTypes, Dictionary<string, int>> mappingFields = GetMappingFields(groupId);

                // TODO - MERGE OLD TAGS AND METAS ONLY AFTER VALIDATION
                // TAGS AND METAS IN DB CAN CONTAIN ONLY GOOD
                // TAGS AND METAS IN CB CAN CONTAIN ALL
                Status validateStatus = ValidateEpgAssetForUpdate(
                    groupId,
                    userId,
                    epgAssetToUpdate,
                    oldEpgAsset,
                    catalogGroupCache,
                    mappingFields,
                    out needToUpdateBasicData,
                    out allNames,
                    out epgMetas,
                    out needToUpdateMetas,
                    out epgTagsIds,
                    out needToUpdateTags,
                    out validateSystemTopicDescription);

                if (!validateStatus.IsOkStatusCode())
                {
                    result.SetStatus(validateStatus);
                    return result;
                }

                if (epgAssetToUpdate.CreateDate == null)
                {
                    log.Error($"Failed UpdateEpgAsset for groupId: {groupId}, epgAsset.Id: {epgAssetToUpdate.Id}, because create date is empty");
                    result.SetStatus(eResponseStatus.Error);
                    return result;
                }

                var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(groupId);

                // update Epg_channels_schedule table (basic data)
                epgCBToUpdate = CreateEpgCbFromEpgAsset(epgAssetToUpdate, groupId, epgAssetToUpdate.CreateDate.Value, updateDate);

                if (needToUpdateBasicData)
                {
                    epgCBToUpdate.Language = defaultLanguageCode;
                    epgCBToUpdate.Name = epgAssetToUpdate.Name;
                    epgCBToUpdate.Description = epgAssetToUpdate.Description;

                    if (epgFeatureVersion == EpgFeatureVersion.V1)
                    {
                        DataTable dtEpgChannelsScheduleToUpdate = GetEpgChannelsScheduleTable();
                        dtEpgChannelsScheduleToUpdate.Rows.Add(GetEpgChannelsScheduleRow(epgCBToUpdate, dtEpgChannelsScheduleToUpdate, updateDate, userId));
                        if (!EpgDal.UpdateEpgChannelSchedule(dtEpgChannelsScheduleToUpdate))
                        {
                            log.Error("UpdateEpgChannelSchedule Failed");
                            return result;
                        }
                    }
                }

                if (needToUpdateMetas)
                {
                    Dictionary<long, List<string>> epgMetaIdToValues = new Dictionary<long, List<string>>();
                    foreach (var item in epgMetas[defaultLanguageCode])
                    {
                        if (mappingFields.ContainsKey(FieldTypes.Meta) && mappingFields[FieldTypes.Meta].ContainsKey(item.Key.ToLower()))
                        {
                            var metaId = (long)mappingFields[FieldTypes.Meta][item.Key.ToLower()];
                            if (!epgMetaIdToValues.ContainsKey(metaId))
                            {
                                epgMetaIdToValues.Add(metaId, new List<string>());
                            }

                            epgMetaIdToValues[metaId].AddRange(item.Value);
                        }
                    }

                    if (epgFeatureVersion == EpgFeatureVersion.V1)
                    {
                        EpgDal.UpdateEpgMetas(epgAssetToUpdate.Id, epgMetaIdToValues, userId, updateDate, groupId, catalogGroupCache.GetDefaultLanguage().ID);
                    }
                }

                if (needToUpdateTags)
                {
                    if (epgFeatureVersion == EpgFeatureVersion.V1)
                    {
                        EpgDal.UpdateEpgTags(epgAssetToUpdate.Id, epgTagsIds, userId, updateDate, groupId);
                    }
                }

                Dictionary<string, Dictionary<string, List<string>>> epgTags = GetEpgTags(epgAssetToUpdate.Tags, allNames, defaultLanguageCode);

                var allDescriptions = GetSystemTopicValues(epgAssetToUpdate.Description, epgAssetToUpdate.DescriptionsWithLanguages,
                                                     catalogGroupCache, AssetManager.DESCRIPTION_META_SYSTEM_NAME, validateSystemTopicDescription, allNames);
                if (!allDescriptions.HasObject())
                {
                    result.SetStatus(allDescriptions.Status);
                    return result;
                }

                // Save old pictures - if any
                UpdateEpgImages(groupId, oldEpgAsset, epgCBToUpdate);

                // update epgCb in CB for all languages
                var epgsToIndex = SaveEpgCbToCB(groupId, epgCBToUpdate,
                    defaultLanguageCode, allNames, allDescriptions.Object, epgMetas, epgTags, false);

                // delete index, if EPG moved to another day
                var indexManager = IndexManagerFactory.Instance.GetIndexManager(groupId);
                if (epgFeatureVersion == EpgFeatureVersion.V2 && epgAssetToUpdate.StartDate?.Date != oldEpgAsset.StartDate?.Date)
                {
                    var epgIds = new List<long> { epgAssetToUpdate.Id };
                    var deleteIndexResult = indexManager.DeleteProgram(epgIds);
                    if (deleteIndexResult)
                    {
                        var epgChannelIds = new List<string> { epgAssetToUpdate.EpgChannelId.ToString() };
                        // invalidate epg's for OPC and NON-OPC accounts
                        InvalidateEpgs(groupId, epgIds, CatalogManager.Instance.DoesGroupUsesTemplates(groupId), epgChannelIds, true);
                    }
                    else
                    {
                        log.ErrorFormat("Failed {0} index for groupId: {1}, assetId: {2}, channelId: {3} after {4}.",
                            nameof(indexManager.DeleteProgram),
                            groupId,
                            epgAssetToUpdate.Id,
                            epgAssetToUpdate.EpgChannelId,
                            nameof(SaveEpgCbToCB));
                    }
                }

                // update index
                var upsertIndexingResult = indexManager.UpsertProgram(epgsToIndex, Cache.CatalogCache.Instance().GetLinearChannelSettings(groupId, new List<string>() { epgCBToUpdate.ChannelID.ToString() }));
                if (!upsertIndexingResult)
                {
                    log.Error($"Failed to upsert to index for groupId: {groupId}, assetId: {epgAssetToUpdate.Id} after SaveEpgCbToCB.");
                }
                else
                {
                    InvalidateEpgs(groupId,
                        epgsToIndex.Select(x => (long)x.EpgID), CatalogManager.Instance.DoesGroupUsesTemplates(groupId),
                        epgsToIndex.Select(item => item.ChannelID.ToString()).Distinct().ToList(), false);
                }

                SendActionEvent(groupId, oldEpgAsset.Id, eAction.Update);

                // get updated epgAsset
                result = AssetManager.Instance.GetAsset(groupId, epgAssetToUpdate.Id, eAssetTypes.EPG, true);
                if (result.IsOkStatusCode())
                {
                    _messageService.PublishUpdateEventAsync(groupId, epgAssetToUpdate.Id, userId).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed UpdateEpgAsset for groupId: {groupId}, epgAsset.Id: {epgAssetToUpdate.Id}", ex);
            }

            return result;
        }

        internal static Status DeleteEpgAsset(int groupId, long epgId, long userId)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(groupId);
            // only Ingest V1 is using DB
            if (epgFeatureVersion == EpgFeatureVersion.V1)
            {
                var isEpgDeletedFromDB = EpgDal.DeleteEpgAsset(epgId, userId);
                if (!isEpgDeletedFromDB)
                {
                    log.ErrorFormat("Failed to delete epg asset with id: {0}, groupId: {1}", epgId, groupId);
                    return result;
                }
            }

            result.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            //update CB
            if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out var catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling DeleteEpgAsset", groupId);
                return null;
            }

            var groupEpgPicturesSizes = ImageManager.GetGroupEpgPicturesSizes(groupId);
            var languages = GetLanguagesObj(new List<string>() { "*" }, catalogGroupCache);
            var docIds = GetEpgCBKeys(groupId, epgId, languages);
            var epgCbList = EpgDal.GetEpgCBList(docIds);

            foreach (EpgCB epgCB in epgCbList)
            {
                var docId = GetEpgCBKey(groupId, epgId, epgCB.Language,
                    catalogGroupCache.GetDefaultLanguage().Code);
                if (!EpgDal.DeleteEpgCB(docId, epgCB))
                {
                    log.ErrorFormat("Failed to DeleteEpgCB for epgId: {0}", epgId);
                }
            }

            SendActionEvent(groupId, epgId, eAction.Delete);

            var epgAsset = new EpgAsset(epgCbList, catalogGroupCache.GetDefaultLanguage().Code, groupEpgPicturesSizes, groupId);
            _messageService.PublishDeleteEventAsync(groupId, epgAsset, userId).GetAwaiter().GetResult();

            // Delete Index
            var indexManager = IndexManagerFactory.Instance.GetIndexManager(groupId);
            var epgIds = new List<long>() { epgId };
            var indexingResult = indexManager.DeleteProgram(epgIds);
            if (!indexingResult)
            {
                log.ErrorFormat("Failed to delete epg index for assetId: {0}, groupId: {1} after DeleteEpgAsset", epgId, groupId);
            }
            else
            {
                // invalidate epg's for OPC and NON-OPC accounts
                var epgChannelIds = epgCbList.Select(x => x.ChannelID.ToString());
                InvalidateEpgs(groupId, epgIds, CatalogManager.Instance.DoesGroupUsesTemplates(groupId), epgChannelIds, true);
            }

            return result;
        }

        internal static void SendActionEvent(int groupId, long epgId, eAction action, EPGChannelProgrammeObject epg = null)
        {
            log.DebugFormat("Calling IngestRecording for groupId: {0}, epgId: {1}, action: {2}", groupId, epgId, action);
            LogContextData contextData = new LogContextData();
            Task.Factory.StartNew(() =>
            {
                contextData.Load();
                Status IngestRecordingStatus = Core.ConditionalAccess.Module.IngestRecording(groupId, new long[1] { epgId }, action, epg);
                log.DebugFormat("IngestRecording result for groupId: {0}, epgId: {1}, action: {2} is: {3}",
                                groupId, epgId, action, IngestRecordingStatus != null ? IngestRecordingStatus.Message : string.Empty);
            });
        }

        internal static Status RemoveTopicsFromProgram(int groupId, HashSet<long> topicIds, long userId, CatalogGroupCache catalogGroupCache, Asset asset)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                // validate topicsIds exist on asset
                if (asset is EpgAsset epgAsset)
                {
                    var existingTopicsIds = epgAsset.Metas
                        .Where(x => catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(x.m_oTagMeta.m_sName))
                        .SelectMany(x => catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName].Select(v => v.Value.Id))
                        .ToList();
                    existingTopicsIds.AddRange(
                        epgAsset.Tags
                            .Where(x => catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(x.m_oTagMeta.m_sName) && catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName].ContainsKey(x.m_oTagMeta.m_sType))
                            .Select(x => catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName][x.m_oTagMeta.m_sType].Id));

                    var noneExistingMetaIds = topicIds.Except(existingTopicsIds).ToArray();
                    if (noneExistingMetaIds.Any())
                    {
                        result = new Status((int)eResponseStatus.MetaIdsDoesNotExistOnAsset, $"{eResponseStatus.MetaIdsDoesNotExistOnAsset.ToString()} for the following Meta Ids: {string.Join(",", noneExistingMetaIds)}");
                        return result;
                    }

                    // get topics to removed
                    var topics = catalogGroupCache.TopicsMapById
                        .Where(x => topicIds.Contains(x.Key) && !CatalogManager.TopicsToIgnore.Contains(x.Value.SystemName.ToLower()))
                        .Select(x => x.Value)
                        .ToArray();
                    var mappingFields = GetMappingFields(groupId);

                    var metaTopics = topics.Where(t => mappingFields.ContainsKey(FieldTypes.Meta) && mappingFields[FieldTypes.Meta].ContainsKey(t.SystemName.ToLower())).ToArray();
                    var programMetaIds = metaTopics.Select(x => mappingFields[FieldTypes.Meta][x.SystemName.ToLower()]).ToList();
                    var metasToRemoveByName = metaTopics.Select(x => x.SystemName.ToLower()).ToList();

                    var tagTopics = topics.Where(t => mappingFields.ContainsKey(FieldTypes.Tag) && mappingFields[FieldTypes.Tag].ContainsKey(t.SystemName.ToLower())).ToArray();
                    var programTagIds = tagTopics.Select(x => mappingFields[FieldTypes.Tag][x.SystemName.ToLower()]).ToList();
                    var tagsToRemoveByName = tagTopics.Select(x => x.SystemName.ToLower()).ToList();

                    var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(groupId);
                    if (epgFeatureVersion == EpgFeatureVersion.V1)
                    {
                        var metasAndTagsRemoved = EpgDal.RemoveMetasAndTagsFromProgram(groupId, epgAsset.Id, programMetaIds, programTagIds, userId);
                        if (!metasAndTagsRemoved)
                        {
                            log.ErrorFormat("Failed to remove topics from program with id: {0}, type: {1}, groupId: {2}", epgAsset.Id, eAssetTypes.EPG.ToString(), groupId);

                            return result;
                        }
                    }

                    // update Epg metas and tags
                    List<EpgCB> epgsToUpdate = RemoveTopicsFromProgramEpgCBs(groupId, epgAsset.Id, metasToRemoveByName, tagsToRemoveByName, userId);

                    // invalidate asset
                    AssetManager.Instance.InvalidateAsset(eAssetTypes.EPG, groupId, epgAsset.Id);
                    var linearChannelSettingsForEpgCb = Cache.CatalogCache.Instance().GetLinearChannelSettings(groupId, new List<string>() { epgsToUpdate?.First().ChannelID.ToString() });

                    // UpdateIndex
                    bool indexingResult = IndexManagerFactory.Instance.GetIndexManager(groupId).UpsertProgram(epgsToUpdate, linearChannelSettingsForEpgCb);

                    if (!indexingResult)
                    {
                        log.ErrorFormat("Failed UpsertProgram index for assetId: {0}, type: {1}, groupId: {2} after RemoveTopicsFromProgram", epgAsset.Id, eAssetTypes.EPG.ToString(), groupId);
                    }
                    else
                    {
                        InvalidateEpgs(groupId,
                            epgsToUpdate.Select(x => (long)x.EpgID), CatalogManager.Instance.DoesGroupUsesTemplates(groupId),
                            epgsToUpdate.Select(item => item.ChannelID.ToString()).Distinct().ToList(), false);
                    }

                    _messageService.PublishUpdateEventAsync(groupId, epgAsset.Id, userId).GetAwaiter().GetResult();
                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed RemoveTopicsFromProgram for groupId:{0}, id:{1}, assetType:{2}", groupId, asset.Id, eAssetTypes.EPG.ToString()), ex);
            }

            return result;
        }

        internal static void InvalidateEpgs(int groupId, IEnumerable<long> epgIds, bool doesGroupUsesTemplates, IEnumerable<string> epgChannelIds, bool shouldGetChannelIds, [System.Runtime.CompilerServices.CallerMemberName] string callingMethod = "")
        {
            if (epgIds != null)
            {
                if (shouldGetChannelIds && epgChannelIds == null && epgIds.Count() > 0)
                {
                    var epgList = GetEpgAssetsFromCache(epgIds.ToList(), groupId);
                    if (epgList != null && epgList.Count > 0)
                    {
                        epgChannelIds = epgList.Where(x => x.EpgChannelId.HasValue && x.EpgChannelId.Value > 0).Select(x => x.EpgChannelId.Value.ToString()).ToList();
                    }
                }

                string assetType = eAssetTypes.EPG.ToString();
                foreach (var currEpgId in epgIds)
                {
                    string invalidationKey;
                    if (doesGroupUsesTemplates)
                    {
                        invalidationKey = LayeredCacheKeys.GetAssetInvalidationKey(groupId, assetType, currEpgId);
                    }
                    else
                    {
                        invalidationKey = LayeredCacheKeys.GetEpgInvalidationKey(groupId, currEpgId);
                    }

                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to invalidate epg with invalidationKey: {0} after {1}.", invalidationKey, callingMethod);
                    }
                }

                if (epgChannelIds != null)
                {
                    foreach (var epgChannelId in epgChannelIds)
                    {
                        var invalidationKey = LayeredCacheKeys.GetAdjacentProgramsInvalidationKey(groupId, epgChannelId);
                        if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                        {
                            log.Error($"Failed to invalidate AdjacentPrograms with invalidationKey: {invalidationKey} after {callingMethod}.");
                        }
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        private static Tuple<Dictionary<string, EpgAsset>, bool> GetEpgAssets(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, EpgAsset> epgAssets = new Dictionary<string, EpgAsset>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("epgIds") && funcParams.ContainsKey("languageCodes"))
                {
                    string key = string.Empty;
                    int? groupId = funcParams["groupId"] as int?;
                    List<long> epgIds = funcParams["epgIds"] as List<long>;
                    List<string> languageCodes = funcParams["languageCodes"] as List<string>;

                    if (groupId.HasValue && epgIds != null && epgIds.Count > 0)
                    {
                        CatalogGroupCache catalogGroupCache;
                        if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId.Value, out catalogGroupCache))
                        {
                            log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetEpgAssets", groupId);
                            return null;
                        }

                        List<LanguageObj> languages = GetLanguagesObj(languageCodes, catalogGroupCache);

                        var groupEpgPicturesSizes = ImageManager.GetGroupEpgPicturesSizes(groupId.Value);

                        var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(groupId.Value);
                        // isNewEpgIngestEnabled = means epg v2 or v3
                        var isNewEpgIngestEnabled = epgFeatureVersion != EpgFeatureVersion.V1;

                        Dictionary<string, string> epgIdToDocumentId = null;
                        if (isNewEpgIngestEnabled && funcParams.ContainsKey("epgIdToDocumentId"))
                        {
                            epgIdToDocumentId = funcParams["epgIdToDocumentId"] as Dictionary<string, string>;
                        }

                        foreach (var epgId in epgIds)
                        {
                            var docIds = GetEpgCBKeys(groupId.Value, epgId, languages, false, epgIdToDocumentId);
                            List<EpgCB> epgCbList = EpgDal.GetEpgCBList(docIds, isNewEpgIngestEnabled);

                            if (epgCbList != null && epgCbList.Count > 0)
                            {
                                EpgAsset epgAsset = new EpgAsset(epgCbList, catalogGroupCache.GetDefaultLanguage().Code, groupEpgPicturesSizes, groupId.Value);
                                epgAsset.IndexStatus = AssetIndexStatus.Ok;
                                string epgAssetKey = LayeredCacheKeys.GetAssetKey(eAssetTypes.EPG.ToString(), epgAsset.Id);
                                epgAssets.Add(epgAssetKey, epgAsset);
                            }
                        }

                        if (epgAssets.Count != epgIds.Count)
                        {
                            List<long> missingAssetIds = epgIds.Where(i => !epgAssets.Values.Any(e => i == e.Id)).ToList();

                            if (missingAssetIds?.Count > 0)
                            {
                                foreach (var missingAssetId in missingAssetIds)
                                {
                                    epgAssets.TryAdd(LayeredCacheKeys.GetAssetKey(eAssetTypes.EPG.ToString(), missingAssetId),
                                        new EpgAsset() { Id = missingAssetId, IndexStatus = AssetIndexStatus.Deleted, AssetType = eAssetTypes.EPG });

                                    log.DebugFormat("Get Deleted EpgAsset {0}, groupId {1}", missingAssetId, groupId);
                                }
                            }
                        }

                        res = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetEpgAssets failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, EpgAsset>, bool>(epgAssets, res);
        }

        private static List<EpgCB> SaveEpgCbToCB(
            int groupId,
            EpgCB epg,
            string defaultLanguageCode,
            Dictionary<string, string> allNames,
            Dictionary<string, string> allDescriptions,
            Dictionary<string, Dictionary<string, List<string>>> epgMetas,
            Dictionary<string, Dictionary<string, List<string>>> epgTags,
            bool isAddAction)
        {
            var programs = new List<(string docId, EpgCB epg)>();
            var suppressesIndexes = Api.api.GetMediaSuppressedIndexes(groupId)?.Object;

            foreach (var currLang in allNames)
            {
                var epgCB = new EpgCB(epg);
                epgCB.Language = currLang.Key;
                epgCB.Name = currLang.Value;

                epgCB.Description = null;
                if (allDescriptions.ContainsKey(currLang.Key))
                {
                    epgCB.Description = allDescriptions[currLang.Key];
                }

                epgCB.Metas = null;
                if (epgMetas != null && epgMetas.ContainsKey(currLang.Key))
                {
                    epgCB.Metas = epgMetas[currLang.Key];
                }

                epgCB.Tags = null;
                if (epgTags != null && epgTags.ContainsKey(currLang.Key))
                {
                    epgCB.Tags = epgTags[currLang.Key];
                }

                if (suppressesIndexes != null && suppressesIndexes.Any())
                {
                    epgCB.Suppressed = ApiLogic.IndexManager.Helpers.IndexManagerCommonHelpers.GetSuppressedIndex(epgCB, suppressesIndexes);
                }

                var docId = GetEpgCBKey(epgCB.ParentGroupID, (long)epgCB.EpgID, epgCB.Language, defaultLanguageCode, isAddAction);
                programs.Add((docId, epgCB));
            }

            CatalogManager.Instance.GetLinearChannelValues(programs.Select(p => p.epg).ToList(), groupId, _ => { });

            foreach (var program in programs)
            {
                if (!EpgDal.SaveEpgCB(program.docId, program.epg, cb => TtlService.Instance.GetEpgCouchbaseTtlSeconds(cb)))
                {
                    log.ErrorFormat("Failed to SaveEpgCbToCB for epgId: {0}, languageCode: {1} in EpgAssetManager", program.epg.EpgID, program.epg.Language);
                }
            }

            return programs.Select(kvp => kvp.epg).ToList();
        }

        private static List<LanguageObj> GetLanguagesObj(List<string> languageCodes, CatalogGroupCache catalogGroupCache)
        {
            var languages = new List<LanguageObj>();

            if (languageCodes != null && languageCodes.Count > 0)
            {
                if (languageCodes.Count == 1 && languageCodes[0].Equals("*"))
                {
                    languages.AddRange(catalogGroupCache.LanguageMapById.Values);
                }
                else
                {
                    foreach (var langCode in languageCodes)
                    {
                        if (catalogGroupCache.LanguageMapByCode.ContainsKey(langCode))
                        {
                            languages.Add(catalogGroupCache.LanguageMapByCode[langCode]);
                        }
                    }
                }
            }
            else
            {
                // in case no language was found - return defalut language
                languages.Add(catalogGroupCache.GetDefaultLanguage());
            }

            return languages;
        }

        /// <summary>
        /// Create EpgCb From EpgAsset without initializing
        /// PicUrl, ParentGroupID, PicID, BasicData, Statistics, pictures, SearchEndDate, Name, Description, Metas, Tags, Language, DocumentId
        /// </summary>
        /// <param name="epgAsset"></param>
        /// <param name="groupId"></param>
        /// <param name="createDate"></param>
        /// <param name="updateDate"></param>
        /// <returns></returns>
        private static EpgCB CreateEpgCbFromEpgAsset(EpgAsset epgAsset, int groupId, DateTime createDate, DateTime updateDate)
        {
            int channelId = epgAsset.EpgChannelId.HasValue ? (int)epgAsset.EpgChannelId.Value : 0;

            return new EpgCB()
            {
                EpgID = (ulong)epgAsset.Id,
                ParentGroupID = groupId,
                EpgIdentifier = epgAsset.EpgIdentifier,
                ChannelID = channelId,
                LinearMediaId = epgAsset.LinearAssetId.GetValueOrDefault(0), // TODO review, was it a bug or not?
                StartDate = epgAsset.StartDate ?? DateTime.MinValue,
                EndDate = epgAsset.EndDate ?? DateTime.MinValue,
                UpdateDate = updateDate,
                CreateDate = createDate,
                Status = 1,
                IsActive = true,
                IsIngestV2 = epgAsset.IsIngestV2,
                GroupID = groupId,
                ExtraData = new EpgExtraData() { MediaID = epgAsset.RelatedMediaId.HasValue ? (int)epgAsset.RelatedMediaId.Value : 0 },
                Crid = epgAsset.Crid,
                CoGuid = epgAsset.CoGuid,
                EnableCDVR = GetEnableData(epgAsset.CdvrEnabled),
                EnableCatchUp = GetEnableData(epgAsset.CatchUpEnabled),
                EnableStartOver = GetEnableData(epgAsset.StartOverEnabled),
                EnableTrickPlay = GetEnableData(epgAsset.TrickPlayEnabled),
                ExternalOfferIds = new List<string>(epgAsset.ExternalOfferIds ?? new List<string>())
            };
        }

        private static int GetEnableData(bool? enableDataFromEpgAsset)
        {
            if (!enableDataFromEpgAsset.HasValue)
                return 0; // get value from LinearChannelSettings

            if (enableDataFromEpgAsset.Value)
                return 1;

            return 2;
        }

        private static Status ValidateEpgAssetForUpdate(int groupId, long userId, EpgAsset epgAssetToUpdate, EpgAsset oldEpgAsset, CatalogGroupCache catalogGroupCache,
                                                        Dictionary<FieldTypes, Dictionary<string, int>> mappingFields, out bool updateBasicData, out Dictionary<string, string> allNames,
                                                        out Dictionary<string, Dictionary<string, List<string>>> epgMetas, out bool updateMetas, out List<int> epgTagsIds, out bool updateTags,
                                                        out bool validateSystemTopicDescription)
        {
            updateBasicData = false;
            allNames = null;
            epgMetas = null;
            updateMetas = true;
            epgTagsIds = null;
            updateTags = true;
            validateSystemTopicDescription = true;

            if (!string.IsNullOrEmpty(epgAssetToUpdate.EpgIdentifier) && !epgAssetToUpdate.EpgIdentifier.Equals(oldEpgAsset.EpgIdentifier))
            {
                return new Status((int)eResponseStatus.Error, "cannot update EpgIdentifier");
            }

            updateBasicData = epgAssetToUpdate.UpdateFields(oldEpgAsset);

            if (!epgAssetToUpdate.IsStartAndEndDatesAreValid())
            {
                return new Status(eResponseStatus.StartDateShouldBeLessThanEndDate, eResponseStatus.StartDateShouldBeLessThanEndDate.ToString());
            }

            bool validateSystemTopic = true;
            if (epgAssetToUpdate.NamesWithLanguages == null || epgAssetToUpdate.NamesWithLanguages.Count == 0)
            {
                epgAssetToUpdate.NamesWithLanguages = oldEpgAsset.NamesWithLanguages;
                validateSystemTopic = false;
            }

            if (epgAssetToUpdate.DescriptionsWithLanguages == null || epgAssetToUpdate.DescriptionsWithLanguages.Count == 0)
            {
                epgAssetToUpdate.DescriptionsWithLanguages = oldEpgAsset.DescriptionsWithLanguages;
                validateSystemTopicDescription = false;
            }

            EpgAssetMultilingualMutator.Instance.PrepareEpgAsset(groupId, epgAssetToUpdate, catalogGroupCache.DefaultLanguage, catalogGroupCache.LanguageMapByCode);

            var nameValues = GetSystemTopicValues(epgAssetToUpdate.Name, epgAssetToUpdate.NamesWithLanguages, catalogGroupCache,
                                                  AssetManager.NAME_META_SYSTEM_NAME, validateSystemTopic);
            if (!nameValues.HasObject())
            {
                return nameValues.Status;
            }

            allNames = nameValues.Object;

            // update EPG_program_metas table (metas)
            if (epgAssetToUpdate.Metas == null || epgAssetToUpdate.Metas.Count == 0)
            {
                // needToValidateMetas
                updateMetas = false;
            }

            // check for missing metas at epgAssetToUpdate vs.  oldEpgAsset and update epgAssetToUpdate
            epgAssetToUpdate.Metas = SetEpgMetaToUpdate(catalogGroupCache, epgAssetToUpdate.Metas, oldEpgAsset.Metas);

            // update EPG_program_tags table (tags)
            if (epgAssetToUpdate.Tags == null || epgAssetToUpdate.Tags.Count == 0)
            {
                // needToValidateTags
                updateTags = false;
            }

            // check for missing tags at epgAssetToUpdate vs.  oldEpgAsset and update epgAssetToUpdate
            epgAssetToUpdate.Tags = SetEpgTagsToUpdate(catalogGroupCache, epgAssetToUpdate.Tags, oldEpgAsset.Tags);

            Status assetStructValidationStatus = ValidateEpgAssetStruct(groupId, userId, epgAssetToUpdate, catalogGroupCache, updateMetas, mappingFields, allNames, out epgMetas, out epgTagsIds);
            if (!assetStructValidationStatus.IsOkStatusCode())
            {
                return assetStructValidationStatus;
            }

            epgAssetToUpdate.LinearAssetId = oldEpgAsset.LinearAssetId;
            epgAssetToUpdate.IsIngestV2 = oldEpgAsset.IsIngestV2;

            return new Status((int)eResponseStatus.OK);
        }

        private static List<Metas> SetEpgMetaToUpdate(CatalogGroupCache catalogGroupCache, List<Metas> epgMetasToUpdate, List<Metas> oldMetasAsset)
        {
            if (epgMetasToUpdate == null)
            {
                epgMetasToUpdate = new List<Metas>();
            }

            if (catalogGroupCache.GetProgramAssetStructId() == 0 || oldMetasAsset == null || oldMetasAsset.Count == 0)
            {
                return epgMetasToUpdate;
            }

            List<Metas> excluded = oldMetasAsset.Where(x => catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(x.m_oTagMeta.m_sName) &&
                                                            catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName].ContainsKey(x.m_oTagMeta.m_sType) &&
                                                            catalogGroupCache.AssetStructsMapById[catalogGroupCache.GetProgramAssetStructId()].AssetStructMetas
                                                                .ContainsKey(catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName][x.m_oTagMeta.m_sType].Id) &&
                                                            !epgMetasToUpdate.Contains(x, new MetasComparer())).ToList();

            if (excluded.Count > 0)
            {
                // get all program asset struct topic systemNames and types mapping
                Dictionary<string, string> programStructTopicSystemNamesToType =
                    catalogGroupCache.TopicsMapById.Where(x => catalogGroupCache.AssetStructsMapById[catalogGroupCache.GetProgramAssetStructId()].AssetStructMetas.ContainsKey(x.Key))
                                                   .ToDictionary(x => x.Value.SystemName, x => x.Value.Type.ToString());
                // set Metas original m_sType
                foreach (Metas meta in excluded)
                {
                    meta.m_oTagMeta.m_sType = programStructTopicSystemNamesToType[meta.m_oTagMeta.m_sName];
                }

                epgMetasToUpdate.AddRange(excluded);
            }

            return epgMetasToUpdate;
        }

        private static List<Tags> SetEpgTagsToUpdate(CatalogGroupCache catalogGroupCache, List<Tags> epgTagsToUpdate, List<Tags> oldTagsAsset)
        {
            if (epgTagsToUpdate == null)
            {
                epgTagsToUpdate = new List<Tags>();
            }

            if (catalogGroupCache.GetProgramAssetStructId() == 0 || oldTagsAsset == null || oldTagsAsset.Count == 0)
            {
                return epgTagsToUpdate;
            }

            List<Tags> excluded = oldTagsAsset.Where(x => catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(x.m_oTagMeta.m_sName) &&
                                                          catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName].ContainsKey(x.m_oTagMeta.m_sType) &&
                                                          catalogGroupCache.AssetStructsMapById[catalogGroupCache.GetProgramAssetStructId()].AssetStructMetas
                                                            .ContainsKey(catalogGroupCache.TopicsMapBySystemNameAndByType[x.m_oTagMeta.m_sName][x.m_oTagMeta.m_sType].Id) &&
                                                          !epgTagsToUpdate.Contains(x, new TagsComparer())).ToList();

            if (excluded.Count > 0)
            {
                epgTagsToUpdate.AddRange(excluded);
            }

            return epgTagsToUpdate;
        }

        private static Status ValidateEpgAssetForInsert(int groupId, long userId, EpgAsset epgAssetToAdd, CatalogGroupCache catalogGroupCache, Dictionary<FieldTypes, Dictionary<string, int>> mappingFields,
                                                        out Dictionary<string, string> allNames, out Dictionary<string, Dictionary<string, List<string>>> epgMetas, out List<int> epgTagsIds)
        {
            allNames = null;
            epgMetas = null;
            epgTagsIds = null;

            if (!epgAssetToAdd.StartDate.HasValue || !epgAssetToAdd.EndDate.HasValue)
            {
                return new Status((int)eResponseStatus.EPGSProgramDatesError, EPGS_PROGRAM_DATES_ERROR);
            }

            if (!epgAssetToAdd.IsStartAndEndDatesAreValid())
            {
                return new Status(eResponseStatus.StartDateShouldBeLessThanEndDate, eResponseStatus.StartDateShouldBeLessThanEndDate.ToString());
            }

            long linearAssetId = epgAssetToAdd.LinearAssetId ?? 0;
            var linearAssetResult = AssetManager.Instance.GetAsset(groupId, linearAssetId, eAssetTypes.MEDIA, true);

            if (!linearAssetResult.HasObject() || !(linearAssetResult.Object is LiveAsset))
            {
                return new Status((int)eResponseStatus.AssetDoesNotExist, "The LiveAsset for this program does not exist");
            }

            epgAssetToAdd.EpgChannelId = (linearAssetResult.Object as LiveAsset).EpgChannelId;
            if (!epgAssetToAdd.EpgChannelId.HasValue || epgAssetToAdd.EpgChannelId.Value == 0)
            {
                return new Status((int)eResponseStatus.ChannelDoesNotExist, "Could not find the channel for linear Asset Id: " + linearAssetId.ToString());
            }

            DataTable dtEpgIDGUID = EpgDal.Get_EpgIDbyEPGIdentifier(new List<string>() { epgAssetToAdd.EpgIdentifier }, (int)epgAssetToAdd.EpgChannelId.Value);
            if (dtEpgIDGUID != null && dtEpgIDGUID.Rows != null && dtEpgIDGUID.Rows.Count > 0)
            {
                return new Status((int)eResponseStatus.AssetExternalIdMustBeUnique, eResponseStatus.AssetExternalIdMustBeUnique.ToString());
            }

            EpgAssetMultilingualMutator.Instance.PrepareEpgAsset(groupId, epgAssetToAdd, catalogGroupCache.DefaultLanguage, catalogGroupCache.LanguageMapByCode);

            // Add Name meta values
            var nameValues = GetSystemTopicValues(epgAssetToAdd.Name, epgAssetToAdd.NamesWithLanguages, catalogGroupCache,
                                                  AssetManager.NAME_META_SYSTEM_NAME, true);
            if (!nameValues.HasObject())
            {
                return nameValues.Status;
            }

            if (!nameValues.Object.ContainsKey(catalogGroupCache.GetDefaultLanguage().Code))
            {
                return new Status((int)eResponseStatus.NameRequired, "Name in default language is required");
            }

            allNames = nameValues.Object;

            Status assetStructValidationStatus = ValidateEpgAssetStruct(groupId, userId, epgAssetToAdd, catalogGroupCache, true, mappingFields, allNames, out epgMetas, out epgTagsIds);
            if (!assetStructValidationStatus.IsOkStatusCode())
            {
                return assetStructValidationStatus;
            }

            return new Status((int)eResponseStatus.OK);
        }

        private static Status ValidateEpgAssetStruct(int groupId, long userId, EpgAsset epgAsset, CatalogGroupCache catalogGroupCache, bool needToValidateMetas,
                                                     Dictionary<FieldTypes, Dictionary<string, int>> mappingFields, Dictionary<string, string> allNames,
                                                     out Dictionary<string, Dictionary<string, List<string>>> epgMetas, out List<int> epgTagsIds)
        {
            Status status = null;
            epgMetas = null;
            epgTagsIds = null;

            if (catalogGroupCache.GetProgramAssetStructId() == 0)
            {
                return new Status((int)eResponseStatus.AssetStructDoesNotExist, "Program AssetStruct does not exist");
            }

            AssetStruct programAssetStruct = catalogGroupCache.AssetStructsMapById[catalogGroupCache.GetProgramAssetStructId()];
            string mainCode = catalogGroupCache.GetDefaultLanguage().Code;

            if (epgAsset.Metas != null && epgAsset.Metas.Count > 0)
            {
                status = HandleEpgAssetMetas(mainCode, epgAsset.Metas, mappingFields[FieldTypes.Meta], needToValidateMetas, catalogGroupCache, programAssetStruct, allNames, out epgMetas);
                if (!status.IsOkStatusCode())
                {
                    return status;
                }
            }

            if (epgAsset.Tags != null && epgAsset.Tags.Count > 0)
            {
                status = HandleEpgAssetTags(groupId, userId, mainCode, epgAsset.Tags, mappingFields[FieldTypes.Tag], catalogGroupCache, programAssetStruct, out epgTagsIds);
                if (!status.IsOkStatusCode())
                {
                    return status;
                }
            }

            return new Status((int)eResponseStatus.OK);
        }

        private static Status HandleEpgAssetTags(int groupId, long userId, string mainCode, List<Tags> tags, Dictionary<string, int> tagNameToIdMapping,
                                                 CatalogGroupCache catalogGroupCache, AssetStruct programAssetStruct, out List<int> epgTagsIds)
        {
            var distinctTopics = new HashSet<string>();
            epgTagsIds = new List<int>();

            Dictionary<int, List<string>> tagsAndValues = new Dictionary<int, List<string>>();

            foreach (var tag in tags)
            {
                var validateTopicStatus = ValidateTopic(catalogGroupCache, programAssetStruct, tag.m_oTagMeta, MetaType.Tag, ref distinctTopics, "tag", null);
                if (!validateTopicStatus.IsOkStatusCode())
                {
                    return validateTopicStatus;
                }

                //A get all tags Ids
                if (!tagNameToIdMapping.ContainsKey(tag.m_oTagMeta.m_sName.ToLower()))
                {
                    var errorMsg = string.Format(META_DOES_NOT_EXIST, MetaType.Tag, tag.m_oTagMeta.m_sName);
                    return new Status((int)eResponseStatus.MetaDoesNotExist, errorMsg);
                }

                int tagTypeId = tagNameToIdMapping[tag.m_oTagMeta.m_sName.ToLower()];
                if (tagsAndValues.ContainsKey(tagTypeId))
                {
                    tagsAndValues[tagTypeId].AddRange(tag.m_lValues);
                }
                else
                {
                    tagsAndValues.Add(tagTypeId, tag.m_lValues);
                }
            }

            Dictionary<int, Dictionary<string, int>> dicTagTypeWithValues = GetTagTypeWithRelevantValues(groupId, tagsAndValues);

            // Create missing tags
            Dictionary<int, List<string>> tagTypeWithMissingValues = new Dictionary<int, List<string>>();
            foreach (var tag in tags)
            {
                int tagTypeId = tagNameToIdMapping[tag.m_oTagMeta.m_sName.ToLower()];
                foreach (string tagValue in tag.m_lValues)
                {
                    if (!dicTagTypeWithValues.ContainsKey(tagTypeId) || !dicTagTypeWithValues[tagTypeId].ContainsKey(tagValue.ToLower()))
                    {
                        if (!tagTypeWithMissingValues.ContainsKey(tagTypeId))
                        {
                            tagTypeWithMissingValues.Add(tagTypeId, new List<string>());
                        }

                        tagTypeWithMissingValues[tagTypeId].Add(tagValue);
                    }
                }
            }

            if (tagTypeWithMissingValues != null)
            {
                DataTable dtEpgTagsValues = InitEPGTagsValues();
                DateTime utcDateTime = DateTime.UtcNow;
                foreach (var item in tagTypeWithMissingValues)
                {
                    foreach (var value in item.Value)
                    {
                        FillEpgTagValueTable(groupId, item.Key, value, userId, utcDateTime, utcDateTime, ref dtEpgTagsValues);
                    }
                }

                InsertNewTagValues(dtEpgTagsValues, groupId, ref dicTagTypeWithValues);
            }

            foreach (var tag in tags)
            {
                int tagTypeId = tagNameToIdMapping[tag.m_oTagMeta.m_sName.ToLower()];

                foreach (var value in tag.m_lValues)
                {
                    if (dicTagTypeWithValues.ContainsKey(tagTypeId) && dicTagTypeWithValues[tagTypeId].ContainsKey(value.ToLower()))
                    {
                        int tagId = dicTagTypeWithValues[tagTypeId][value.ToLower()];
                        epgTagsIds.Add(tagId);
                    }
                }
            }

            return new Status((int)eResponseStatus.OK);
        }

        private static Status HandleEpgAssetMetas(string mainCode, List<Metas> metas, Dictionary<string, int> metaNameToIdMapping, bool needToValidateMetas,
                                                    CatalogGroupCache catalogGroupCache, AssetStruct programAssetStruct, Dictionary<string, string> allNames,
                                                    out Dictionary<string, Dictionary<string, List<string>>> epgMetas)
        {
            var distinctTopics = new HashSet<string>();

            epgMetas = new Dictionary<string, Dictionary<string, List<string>>>
            {
                { mainCode, new Dictionary<string, List<string>>() }
            };

            foreach (var meta in metas)
            {
                if (needToValidateMetas)
                {
                    var validateTopicStatus = ValidateTopic(catalogGroupCache, programAssetStruct, meta.m_oTagMeta, MetaType.All, ref distinctTopics, "meta", meta.m_sValue);
                    if (!validateTopicStatus.IsOkStatusCode())
                    {
                        return validateTopicStatus;
                    }
                }
                // check that meta exist at mapping
                if (!metaNameToIdMapping.ContainsKey(meta.m_oTagMeta.m_sName.ToLower()))
                {
                    var errorMsg = string.Format(META_DOES_NOT_EXIST, "meta", meta.m_oTagMeta.m_sName);
                    return new Status((int)eResponseStatus.MetaDoesNotExist, errorMsg);
                }

                // get epg metas
                if (epgMetas[mainCode].ContainsKey(meta.m_oTagMeta.m_sName))
                {
                    epgMetas[mainCode][meta.m_oTagMeta.m_sName].Add(meta.m_sValue);
                }
                else
                {
                    epgMetas[mainCode].Add(meta.m_oTagMeta.m_sName, new List<string>() { meta.m_sValue });
                }

                if (meta.Value != null && meta.Value.Length > 0)
                {
                    foreach (var otherLanguageMeta in meta.Value)
                    {
                        if (needToValidateMetas && !allNames.ContainsKey(otherLanguageMeta.m_sLanguageCode3))
                        {
                            return new Status((int)eResponseStatus.Error, string.Format(INVALID_LANGUAGE, otherLanguageMeta.m_sLanguageCode3));
                        }

                        if (!epgMetas.ContainsKey(otherLanguageMeta.m_sLanguageCode3))
                        {
                            epgMetas.Add(otherLanguageMeta.m_sLanguageCode3, new Dictionary<string, List<string>>());
                        }

                        if (epgMetas[otherLanguageMeta.m_sLanguageCode3].ContainsKey(meta.m_oTagMeta.m_sName))
                        {
                            if (!epgMetas[otherLanguageMeta.m_sLanguageCode3][meta.m_oTagMeta.m_sName].Contains(otherLanguageMeta.m_sValue))

                            {
                                epgMetas[otherLanguageMeta.m_sLanguageCode3][meta.m_oTagMeta.m_sName].Add(otherLanguageMeta.m_sValue);
                            }
                        }
                        else
                        {
                            epgMetas[otherLanguageMeta.m_sLanguageCode3].Add(meta.m_oTagMeta.m_sName, new List<string>() { otherLanguageMeta.m_sValue });
                        }
                    }
                }
            }

            distinctTopics.Clear();

            return new Status((int)eResponseStatus.OK);

        }

        internal static Dictionary<FieldTypes, Dictionary<string, int>> GetMappingFields(int groupId)
        {
            Dictionary<FieldTypes, Dictionary<string, int>> allFieldTypeMapping = new Dictionary<FieldTypes, Dictionary<string, int>>();

            try
            {
                DataSet ds = EpgDal.GetEpgMappingFields(new List<int> { groupId }, groupId);

                if (ds != null && ds.Tables != null && ds.Tables.Count >= 5)
                {
                    //basic
                    if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                    {
                        allFieldTypeMapping.Add(FieldTypes.Basic, GetFields(ds.Tables[0]));
                    }

                    //metas
                    if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                    {
                        allFieldTypeMapping.Add(FieldTypes.Meta, GetFields(ds.Tables[1]));
                    }

                    //Tags
                    if (ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
                    {
                        allFieldTypeMapping.Add(FieldTypes.Tag, GetFields(ds.Tables[2]));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetMappingFields with groupId: {0}.", groupId), ex);
            }

            return allFieldTypeMapping;
        }

        private static Dictionary<string, int> GetFields(DataTable dataTable)
        {
            Dictionary<string, int> fields = new Dictionary<string, int>(dataTable.Rows.Count);
            foreach (DataRow dr in dataTable.Rows)
            {
                string name = ODBCWrapper.Utils.GetSafeStr(dr, "Name").ToLower();
                if (!fields.ContainsKey(name))
                {
                    fields.Add(name, ODBCWrapper.Utils.GetIntSafeVal(dr, "ID"));
                }
            }

            return fields;
        }

        private static Dictionary<int, Dictionary<string, int>> GetTagTypeWithRelevantValues(int groupID, Dictionary<int, List<string>> tagsAndValues)
        {
            //per tag type, their values and IDs
            Dictionary<int, Dictionary<string, int>> dicTagTypeWithValues = new Dictionary<int, Dictionary<string, int>>();

            DataTable dtTagValueID = EpgDal.Get_EPGTagValueIDs(groupID, tagsAndValues);

            if (dtTagValueID != null && dtTagValueID.Rows != null)
            {
                for (int i = 0; i < dtTagValueID.Rows.Count; i++)
                {
                    DataRow row = dtTagValueID.Rows[i];

                    if (row != null)
                    {
                        int tagTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "epg_tag_type_id");
                        string value = ODBCWrapper.Utils.GetSafeStr(row, "VALUE");
                        int id = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");

                        if (dicTagTypeWithValues.ContainsKey(tagTypeID))
                        {
                            //check if the value exists already in the dictionary (maybe in UpperCase\LowerCase)
                            if (!dicTagTypeWithValues[tagTypeID].ContainsKey(value.ToLower()))
                            {
                                dicTagTypeWithValues[tagTypeID].Add(value.ToLower(), id);
                            }
                        }
                        else
                        {
                            Dictionary<string, int> values = new Dictionary<string, int>();
                            values.Add(value.ToLower(), id);
                            dicTagTypeWithValues.Add(tagTypeID, values);
                        }
                    }
                }
            }
            else
            {
                log.ErrorFormat("Get_EPGTagValueIDs return empty response. gruop={0}", groupID);
            }

            return dicTagTypeWithValues;
        }

        private static void InsertNewTagValues(DataTable dtEpgTagsValues, int groupId, ref Dictionary<int, Dictionary<string, int>> tagsIds)
        {
            if (dtEpgTagsValues != null && dtEpgTagsValues.Rows != null && dtEpgTagsValues.Rows.Count > 0)
            {
                Dictionary<int, List<string>> dicTagTypeIDAndValues = new Dictionary<int, List<string>>();

                //insert all New tag values from dtEpgTagsValues to DB
                EpgDal.InsertBulk(dtEpgTagsValues, "EPG_tags");

                //return back all the IDs of the new Tags_Values
                for (int k = 0; k < dtEpgTagsValues.Rows.Count; k++)
                {
                    DataRow row = dtEpgTagsValues.Rows[k];
                    string sTagValue = ODBCWrapper.Utils.GetSafeStr(row, "value");
                    int nTagTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "epg_tag_type_id");
                    if (!dicTagTypeIDAndValues.Keys.Contains(nTagTypeID))
                    {
                        dicTagTypeIDAndValues.Add(nTagTypeID, new List<string>() { sTagValue });
                    }
                    else
                    {
                        dicTagTypeIDAndValues[nTagTypeID].Add(sTagValue);
                    }
                }

                DataTable dtTagValueID = EpgDal.Get_EPGTagValueIDs(groupId, dicTagTypeIDAndValues);

                //update the IDs in tagValueWithID
                if (dtTagValueID != null && dtTagValueID.Rows != null)
                {
                    for (int i = 0; i < dtTagValueID.Rows.Count; i++)
                    {
                        DataRow row = dtTagValueID.Rows[i];
                        if (row != null)
                        {
                            int tagId = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                            string tagValue = ODBCWrapper.Utils.GetSafeStr(row, "VALUE");
                            int tagTypeId = ODBCWrapper.Utils.GetIntSafeVal(row, "epg_tag_type_id");

                            if (!tagsIds.ContainsKey(tagTypeId))
                            {
                                tagsIds.Add(tagTypeId, new Dictionary<string, int>());
                            }

                            tagsIds[tagTypeId].Add(tagValue.ToLower(), tagId);
                        }
                    }
                }
            }
        }

        private static Status ValidateTopic(CatalogGroupCache catalogGroupCache, AssetStruct programAssetStruct, TagMeta tagMeta,
                                            MetaType metaType, ref HashSet<string> distinctTopics, string topicType, string metaValue)
        {
            // validate duplicates do not exist
            if (distinctTopics.Contains(tagMeta.m_sName.ToLower()))
            {
                var errorMsg = string.Format(DUPLICATE_VALUE, topicType, tagMeta.m_sName, topicType);
                return new Status((int)eResponseStatus.Error, errorMsg);
            }

            distinctTopics.Add(tagMeta.m_sName);

            // validate meta exists on group
            if (!catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(tagMeta.m_sName))
            {
                var errorMsg = string.Format(META_DOES_NOT_EXIST, topicType, tagMeta.m_sName);
                return new Status((int)eResponseStatus.MetaDoesNotExist, errorMsg);
            }

            Topic topic = catalogGroupCache.TopicsMapBySystemNameAndByType[tagMeta.m_sName][tagMeta.m_sType];

            // validate meta exists on asset struct
            if (!programAssetStruct.AssetStructMetas.ContainsKey(topic.Id))
            {
                var errorMsg = string.Format("{0}: {1} is not part of assetStruct", topicType, tagMeta.m_sName);
                return new Status((int)eResponseStatus.Error, errorMsg);
            }

            // validate correct MetaType was sent
            MetaType sentMetaType;
            if (!Enum.TryParse<MetaType>(tagMeta.m_sType, out sentMetaType) ||
                (metaType != MetaType.Tag && (sentMetaType == MetaType.All || sentMetaType == MetaType.Tag)) ||
                (metaType == MetaType.Tag && sentMetaType != MetaType.Tag))
            {
                var errorMsg = string.Format("{0} was sent for {1}: {2}", eResponseStatus.InvalidMetaType.ToString(), topicType, tagMeta.m_sName);
                return new Status((int)eResponseStatus.InvalidMetaType, errorMsg);
            }

            //Validate Meta Topic Values
            if (metaType != MetaType.Tag)
            {
                bool isMetaValueValid = false;
                switch (sentMetaType)
                {
                    case MetaType.String:
                        isMetaValueValid = true;
                        break;
                    case MetaType.MultilingualString:
                        isMetaValueValid = true;
                        break;
                    case MetaType.Number:
                        double doubleVal;
                        isMetaValueValid = double.TryParse(metaValue, out doubleVal);
                        break;
                    case MetaType.Bool:
                        bool boolVal;
                        isMetaValueValid = BoolUtils.TryConvert(metaValue, out boolVal);
                        break;
                    case MetaType.DateTime:
                        DateTime dateTimeVal;
                        isMetaValueValid = DateTime.TryParse(metaValue, out dateTimeVal);
                        break;
                    default:
                        break;
                }

                if (!isMetaValueValid)
                {
                    var errorMsg = string.Format("{0} metaName: {1}", eResponseStatus.InvalidValueSentForMeta.ToString(), tagMeta.m_sName);
                    return new Status((int)eResponseStatus.InvalidValueSentForMeta, errorMsg);
                }
            }

            return new Status((int)eResponseStatus.OK);
        }

        private static Dictionary<string, Dictionary<string, List<string>>> GetEpgTags(List<Tags> tagsList, Dictionary<string, string> languageCodes, string mainCode)
        {
            Dictionary<string, Dictionary<string, List<string>>> epgTags = new Dictionary<string, Dictionary<string, List<string>>>();

            try
            {
                if (tagsList != null && tagsList.Count > 0)
                {
                    epgTags.Add(mainCode, new Dictionary<string, List<string>>());

                    foreach (Tags tags in tagsList)
                    {
                        if (epgTags[mainCode].ContainsKey(tags.m_oTagMeta.m_sName))
                        {
                            epgTags[mainCode][tags.m_oTagMeta.m_sName].AddRange(tags.m_lValues);
                        }
                        else
                        {
                            epgTags[mainCode].Add(tags.m_oTagMeta.m_sName, tags.m_lValues);
                        }

                        if (tags.Values != null && tags.Values.Count > 0)
                        {
                            foreach (var otherTagsValues in tags.Values)
                            {
                                foreach (var otherTagsValue in otherTagsValues)
                                {
                                    if (languageCodes.ContainsKey(otherTagsValue.m_sLanguageCode3) && !string.IsNullOrEmpty(otherTagsValue.m_sValue))
                                    {
                                        if (!epgTags.ContainsKey(otherTagsValue.m_sLanguageCode3))
                                        {
                                            epgTags.Add(otherTagsValue.m_sLanguageCode3, new Dictionary<string, List<string>>());
                                        }

                                        if (epgTags[otherTagsValue.m_sLanguageCode3].ContainsKey(tags.m_oTagMeta.m_sName))
                                        {
                                            if (!epgTags[otherTagsValue.m_sLanguageCode3][tags.m_oTagMeta.m_sName].Contains(otherTagsValue.m_sValue))
                                            {
                                                epgTags[otherTagsValue.m_sLanguageCode3][tags.m_oTagMeta.m_sName].Add(otherTagsValue.m_sValue);
                                            }
                                        }
                                        else
                                        {
                                            epgTags[otherTagsValue.m_sLanguageCode3].Add(tags.m_oTagMeta.m_sName, new List<string>() { otherTagsValue.m_sValue });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetEpgTags - " + string.Format("faild due ex={0}", ex.Message), ex);
            }

            return epgTags;
        }

        /// <summary>
        /// return a dictionary map to topic language code and its translation value
        /// </summary>
        /// <param name="mainLangValue"></param>
        /// <param name="otherLanguages"></param>
        /// <param name="catalogGroupCache"></param>
        /// <param name="topicSystemName"></param>
        /// <returns></returns>
        private static GenericResponse<Dictionary<string, string>> GetSystemTopicValues
            (string mainLangValue, List<LanguageContainer> otherLanguages, CatalogGroupCache catalogGroupCache, string topicSystemName,
             bool validateSystemTopic, Dictionary<string, string> epgNames = null)
        {
            GenericResponse<Dictionary<string, string>> topicValues = new GenericResponse<Dictionary<string, string>>();

            if (validateSystemTopic && !catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(topicSystemName))
            {
                var errorMsg = string.Format(META_DOES_NOT_EXIST, "SystemName", topicSystemName);
                topicValues.SetStatus(eResponseStatus.MetaDoesNotExist, errorMsg);
                return topicValues;
            }

            topicValues.Object = new Dictionary<string, string>();
            topicValues.SetStatus(eResponseStatus.OK);

            if (!string.IsNullOrEmpty(mainLangValue))
            {
                topicValues.Object.Add(catalogGroupCache.GetDefaultLanguage().Code, mainLangValue);

                if (otherLanguages != null && otherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in otherLanguages)
                    {
                        if (validateSystemTopic)
                        {
                            if (epgNames != null && !epgNames.ContainsKey(language.m_sLanguageCode3))
                            {
                                topicValues.SetStatus(eResponseStatus.Error, string.Format(INVALID_LANGUAGE, language.m_sLanguageCode3));
                                return topicValues;
                            }
                            else if (!catalogGroupCache.LanguageMapByCode.ContainsKey(language.m_sLanguageCode3))
                            {
                                var errorMsg = string.Format("language: {0} is not part of group supported languages", language.m_sLanguageCode3);
                                topicValues.SetStatus(eResponseStatus.GroupDoesNotContainLanguage, errorMsg);
                                return topicValues;
                            }

                            if (topicValues.Object.ContainsKey(language.m_sLanguageCode3))
                            {
                                var errorMsg = string.Format(DUPLICATE_VALUE, "language code", language.m_sLanguageCode3, topicSystemName);
                                topicValues.SetStatus(eResponseStatus.Error, errorMsg);
                                return topicValues;
                            }
                        }

                        topicValues.Object.Add(language.m_sLanguageCode3, language.m_sValue);
                    }
                }
            }

            return topicValues;
        }

        private static void FillEpgTagValueTable(int groupId, long tagTypeId, string value, long updaterId, DateTime createDate, DateTime updateDate, ref DataTable dtEPGTagValue)
        {
            DataRow row = dtEPGTagValue.NewRow();
            row["value"] = value;
            row["epg_tag_type_id"] = tagTypeId;
            row["group_id"] = groupId;
            row["status"] = 1;
            row["updater_id"] = updaterId;
            row["create_date"] = createDate;
            row["update_date"] = updateDate;
            dtEPGTagValue.Rows.Add(row);
        }

        private static DataTable InitEPGTagsValues()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("value", typeof(string));
            dt.Columns.Add("epg_tag_type_id", typeof(string));
            dt.Columns.Add("group_id", typeof(int));
            dt.Columns.Add("status", typeof(int));
            dt.Columns.Add("updater_id", typeof(int));
            dt.Columns.Add("create_date", typeof(DateTime));
            dt.Columns.Add("update_date", typeof(DateTime));
            return dt;
        }

        private static DataTable GetEpgChannelsScheduleTable()
        {
            DataTable dt = new DataTable();
            // Add three column objects to the table.
            DataColumn ID = new DataColumn
            {
                DataType = typeof(long),
                ColumnName = "ID",
                Unique = true
            };

            dt.Columns.Add(ID);
            dt.Columns.Add("EPG_CHANNEL_ID", typeof(long));
            dt.Columns.Add("EPG_IDENTIFIER", typeof(string));
            dt.Columns.Add("NAME", typeof(string));
            dt.Columns.Add("DESCRIPTION", typeof(string));
            dt.Columns.Add("START_DATE", typeof(DateTime));
            dt.Columns.Add("END_DATE", typeof(DateTime));
            dt.Columns.Add("PIC_ID", typeof(long));
            dt.Columns.Add("STATUS", typeof(int));
            dt.Columns.Add("IS_ACTIVE", typeof(int));
            dt.Columns.Add("GROUP_ID", typeof(long));
            dt.Columns.Add("UPDATER_ID", typeof(long));
            dt.Columns.Add("UPDATE_DATE", typeof(DateTime));
            dt.Columns.Add("PUBLISH_DATE", typeof(DateTime));
            dt.Columns.Add("CREATE_DATE", typeof(DateTime));
            dt.Columns.Add("EPG_TAG", typeof(string));
            dt.Columns.Add("media_id", typeof(long));
            dt.Columns.Add("FB_OBJECT_ID", typeof(string));
            dt.Columns.Add("like_counter", typeof(long));
            dt.Columns.Add("ENABLE_CDVR", typeof(int));
            dt.Columns.Add("ENABLE_CATCH_UP", typeof(int));
            dt.Columns.Add("ENABLE_START_OVER", typeof(int));
            dt.Columns.Add("ENABLE_TRICK_PLAY", typeof(int));
            dt.Columns.Add("CRID", typeof(string));

            return dt;
        }

        private static DataRow GetEpgChannelsScheduleRow(EpgCB epgCb, DataTable dtEPG, DateTime dPublishDate, long updaterId)
        {
            DataRow row = dtEPG.NewRow();
            row["EPG_CHANNEL_ID"] = epgCb.ChannelID;
            row["EPG_IDENTIFIER"] = epgCb.EpgIdentifier;

            if (string.IsNullOrEmpty(epgCb.Name))
                epgCb.Name = string.Empty;

            epgCb.Name = epgCb.Name.Replace("\r", "").Replace("\n", "");
            if (epgCb.Name.Length >= MaxNameSize)
                row["NAME"] = epgCb.Name.Substring(0, MaxNameSize); //insert only 255 chars (limitation of the column in the DB)
            else
                row["NAME"] = epgCb.Name;

            if (string.IsNullOrEmpty(epgCb.Description))
                epgCb.Description = string.Empty;

            epgCb.Description = epgCb.Description.Replace("\r", "").Replace("\n", "");
            if (epgCb.Description.Length >= MaxDescriptionSize)
                row["DESCRIPTION"] = epgCb.Description.Substring(0, MaxDescriptionSize); //insert only 1024 chars (limitation of the column in the DB)
            else
                row["DESCRIPTION"] = epgCb.Description;

            row["START_DATE"] = epgCb.StartDate;
            row["END_DATE"] = epgCb.EndDate;
            row["PIC_ID"] = epgCb.PicID;
            row["STATUS"] = epgCb.Status;
            row["IS_ACTIVE"] = epgCb.IsActive;
            row["GROUP_ID"] = epgCb.GroupID;
            row["UPDATER_ID"] = updaterId;
            row["UPDATE_DATE"] = epgCb.UpdateDate;
            row["PUBLISH_DATE"] = dPublishDate;
            row["CREATE_DATE"] = epgCb.CreateDate;
            row["EPG_TAG"] = null;
            row["media_id"] = epgCb.ExtraData.MediaID;
            row["FB_OBJECT_ID"] = epgCb.ExtraData.FBObjectID;
            row["like_counter"] = epgCb.Statistics.Likes;

            if (row.Table.Columns.Contains("ID") && epgCb.EpgID > 0)
            {
                row["ID"] = epgCb.EpgID;
            }

            row["ENABLE_CATCH_UP"] = epgCb.EnableCatchUp;
            row["ENABLE_CDVR"] = epgCb.EnableCDVR;
            row["ENABLE_START_OVER"] = epgCb.EnableStartOver;
            row["ENABLE_TRICK_PLAY"] = epgCb.EnableTrickPlay;
            row["CRID"] = epgCb.Crid;

            return row;
        }

        private static List<EpgCB> RemoveTopicsFromProgramEpgCBs(int groupId, long epgId, List<string> programMetas, List<string> programTags, long userId)
        {
            if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out var catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling RemoveTopicsFromProgramEpgCBs", groupId);
                return new List<EpgCB>();
            }

            var languages = GetLanguagesObj(new List<string>() { "*" }, catalogGroupCache);
            var docIds = GetEpgCBKeys(groupId, epgId, languages);
            var epgCbList = EpgDal.GetEpgCBList(docIds);

            foreach (EpgCB epgCB in epgCbList)
            {
                RemoveTopicsFromProgramEpgCB(epgCB, programMetas, programTags, catalogGroupCache.GetDefaultLanguage().Code);
            }

            return epgCbList;
        }

        private static void RemoveTopicsFromProgramEpgCB(EpgCB epgCB, List<string> programMetas, List<string> programTags, string defaultLanguageCode)
        {
            try
            {
                if (epgCB.Metas != null)
                {
                    foreach (string systemName in programMetas)
                    {
                        var meta = epgCB.Metas.FirstOrDefault(x => x.Key.ToLower().Equals(systemName));
                        if (!string.IsNullOrEmpty(meta.Key))
                        {
                            epgCB.Metas.Remove(meta.Key);
                        }
                    }
                }

                if (epgCB.Tags != null)
                {
                    foreach (string systemName in programTags)
                    {
                        var tag = epgCB.Tags.FirstOrDefault(x => x.Key.ToLower().Equals(systemName));
                        if (!string.IsNullOrEmpty(tag.Key))
                        {
                            epgCB.Tags.Remove(tag.Key);
                        }
                    }
                }

                var docId = GetEpgCBKey(epgCB.ParentGroupID, (long)epgCB.EpgID, epgCB.Language, defaultLanguageCode);
                if (!EpgDal.SaveEpgCB(docId, epgCB, cb => TtlService.Instance.GetEpgCouchbaseTtlSeconds(cb)))
                {
                    log.ErrorFormat("RemoveTopicsFromProgramEpgCB - Failed to SaveEpgCB for epgId: {0}.", epgCB.EpgID);
                }
            }
            catch (Exception exc)
            {
                log.Error(string.Format("Exception at RemoveTopicsFromProgramEpgCB for epgId: {0}.", epgCB.EpgID), exc);
            }
        }

        internal static void UpdateProgramAssetPictures(int groupId, long userId, Image image, string sourceUrl)
        {
            var docId = GetEpgCBKey(groupId, image.ImageObjectId);
            var program = EpgDal.GetEpgCB(docId);

            if (program != null)
            {
                if (image.Version > 0)
                {
                    if (program.pictures != null)
                    {
                        var pic = program.pictures.FirstOrDefault(x => x.IsProgramImage && x.PicID == image.ReferenceId);
                        if (pic != null)
                        {
                            pic.Version = image.Version;
                            pic.ImageTypeId = image.ImageTypeId;
                            pic.SourceUrl = sourceUrl;
                        }
                    }
                }
                else
                {
                    if (program.pictures == null)
                    {
                        program.pictures = new List<EpgPicture>();
                    }

                    var pic = new EpgPicture()
                    {
                        PicID = (int)image.ReferenceId,
                        IsProgramImage = true,
                        Url = image.ContentId,
                        Version = 0,
                        Ratio = image.RatioName,
                        ImageTypeId = image.ImageTypeId,
                        SourceUrl = sourceUrl
                    };

                    bool picReplaced = false;
                    for (int picIndex = 0; picIndex < program.pictures.Count; picIndex++)
                    {
                        if (!program.pictures[picIndex].IsProgramImage && program.pictures[picIndex].Ratio == image.RatioName)
                        {
                            program.pictures[picIndex] = pic;
                            picReplaced = true;
                        }
                    }

                    if (!picReplaced)
                    {
                        program.pictures.Add(pic);

                    }
                }

                if (EpgDal.SaveEpgCB(docId, program, cb => TtlService.Instance.GetEpgCouchbaseTtlSeconds(cb)))
                {
                    EpgNotificationManager.Instance().ChannelWasUpdated(
                        KLogger.GetRequestId(),
                        groupId,
                        userId,
                        program.LinearMediaId,
                        program.ChannelID,
                        program.StartDate,
                        program.EndDate,
                        false);
                }
                else
                {
                    log.ErrorFormat("Error while update epgCB at SetContent. imageId:{0}", image.Id);
                }
            }
        }

        /// <summary>
        /// Update if original asset has images
        /// </summary>
        private static void UpdateEpgImages(int groupId, EpgAsset oldEpgAsset, EpgCB epgCBToUpdate)
        {
            if (oldEpgAsset.Images != null && oldEpgAsset.Images.Count > 0)
            {
                var docId = GetEpgCBKey(groupId, oldEpgAsset.Id);
                var program = EpgDal.GetEpgCB(docId);

                if (program != null && program.pictures?.Count > 0)
                {
                    epgCBToUpdate.PicID = program.PicID;
                    epgCBToUpdate.PicUrl = program.PicUrl;

                    epgCBToUpdate.pictures = program.pictures;
                }
                else
                {
                    log.Debug($"Couldn't update Epg's images for asset: {oldEpgAsset.Id}, doc: {docId} has no images or wasn't found");
                }
            }
        }
        #endregion

        #region public Methods

        public static List<Topic> GetBasicTopics()
        {
            List<Topic> result = new List<Topic>();
            foreach (KeyValuePair<string, string> meta in BasicProgramMetasSystemNameToName)
            {
                Topic topicToAdd = new Topic(meta.Key, true, meta.Value);
                switch (meta.Key)
                {
                    case NAME_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.MultilingualString);
                        topicToAdd.SearchRelated = true;
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_MANDATORY);
                        break;
                    case DESCRIPTION_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.MultilingualString);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_TEXTAREA);
                        break;
                    case EXTERNAL_ID_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.String);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        break;
                    case CRID_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.String);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_MANDATORY);
                        break;
                    case START_DATE_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.DateTime);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_AVAILABILITY);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_MANDATORY);
                        break;
                    case END_DATE_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.DateTime);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_AVAILABILITY);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_MANDATORY);
                        break;
                    case SERIES_NAME_META_SYSTEM_NAME:
                        topicToAdd.SearchRelated = true;
                        topicToAdd.SetType(MetaType.Tag);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        break;
                    case SERIES_ID_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.String);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        break;
                    case EPISODE_NUMBER_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.Number);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        break;
                    case SEASON_NUMBER_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.Number);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        break;
                    case PARENTAL_RATING_META_SYSTEM_NAME:
                        topicToAdd.SetType(MetaType.Tag);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        break;
                    case GENRE_META_SYSTEM_NAME:
                        topicToAdd.SearchRelated = true;
                        topicToAdd.SetType(MetaType.Tag);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_METADATA);
                        topicToAdd.Features.Add(CatalogManager.OPC_UI_MANDATORY);
                        break;
                    default:
                        throw new Exception(string.Format("missing mapping for metaSystemName: {0} on EpgAssetManager.GetBasicTopics", meta.Key));
                }

                result.Add(topicToAdd);
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="epgId"></param>
        /// <param name="langCodes"></param>
        /// <param name="isAddAction">Are we trying to get EPG CB key for an EPG that we are adding right now or to an existing one</param>
        /// <returns></returns>
        public static List<string> GetEpgCBKeys(int groupId, long epgId, IEnumerable<LanguageObj> langCodes, bool isAddAction = false,
            Dictionary<string, string> epgIdToDocumentId = null)
        {
            //epgIdToDocumentId -> {[100005191, epg_11709_eng_100005191]}
            if (epgIdToDocumentId?.Count > 0 && epgIdToDocumentId.ContainsKey(epgId.ToString()))
            {
                var response = new List<string>();
                var docId = epgIdToDocumentId[epgId.ToString()];
                if (EpgDal.IsIngestV2Format(docId))
                {
                    var split = docId.Split('_');
                    foreach (var langCode in langCodes)
                    {
                        var temp = split;
                        temp[2] = langCode.Code;
                        response.Add(string.Join("_", temp));
                    }
                }
                else
                {
                    //Support non-ingest
                    foreach (var langCode in langCodes)
                    {
                        if (langCode.IsDefault)
                        {
                            response.Add($"{epgId}");
                        }
                        else
                        {
                            response.Add($"epg_{epgId}_lang_{langCode.Code.ToLower()}");
                        }
                    }
                }

                if (response.Count > 0)
                {
                    return response;
                }
            }

            var epgBL = new TvinciEpgBL(groupId);
            return epgBL.GetEpgsCBKeys(groupId, new[] { epgId }, langCodes, isAddAction);
        }

        public static string GetEpgCBKey(int groupId, long epgId)
        {
            var keys = GetEpgCBKeys(groupId, epgId, null, false);

            return keys.FirstOrDefault();
        }

        public IEnumerable<EpgAsset> GetEpgAssets(long groupId, IEnumerable<long> epgIds, IEnumerable<string> languages)
            => GetEpgAssetsFromCache(epgIds.ToList(), (int)groupId, languages?.ToList());

        public static void InitProgramAssetCrudMessageService(IKafkaContextProvider contextProvider)
        {
            _messageService = new ProgramAssetCrudMessageService(
                AssetManager.Instance,
                Instance,
                ProgramAssetCrudEventMapper.Instance,
                KafkaProducerFactoryInstance.Get(),
                contextProvider,
                new KLogger(nameof(ProgramAssetCrudMessageService)));
        }

        private static string GetEpgCBKey(int groupId, long epgId, string langCode, string defaultLangCode, bool isAddAction = false)
        {
            // The documents with the main language are saved without a language code so we will send null to get key
            var langs = defaultLangCode.Equals(langCode) ? null : new[] { new LanguageObj() { Code = langCode } };
            var keys = GetEpgCBKeys(groupId, epgId, langs, isAddAction);

            return keys.FirstOrDefault();
        }
        #endregion
    }
}