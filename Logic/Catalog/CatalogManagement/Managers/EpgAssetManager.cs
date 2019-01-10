using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Epg;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Catalog.Cache;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Tvinci.Core.DAL;
using TVinciShared;

namespace Core.Catalog.CatalogManagement
{
    public class EpgAssetManager
    {
        #region Constants and Read-only

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly HashSet<string> BasicMetasSystemNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            AssetManager.NAME_META_SYSTEM_NAME, 
            AssetManager.DESCRIPTION_META_SYSTEM_NAME,
            AssetManager.EXTERNAL_ID_META_SYSTEM_NAME,
            START_DATE_META_SYSTEM_NAME,
            END_DATE_META_SYSTEM_NAME,
            SERIES_NAME_META_SYSTEM_NAME,
            SERIES_ID_META_SYSTEM_NAME,
            EPISODE_NUMBER_META_SYSTEM_NAME,
            SEASON_NUMBER_META_SYSTEM_NAME,
            CRID_META_SYSTEM_NAME
        };

        private const string EPGS_PROGRAM_DATES_ERROR = "Error at EPG Program Start/End Dates";
        private const string META_DOES_NOT_EXIST = "{0}: {1} does not exist for this group";
        private const string INVALID_LANGUAGE = "Invalid language: {0}. Only languages specified in the name of the asset can be associated.";
        private const string DUPLICATE_VALUE = "Duplicate {0}:{1} sent for {2}.";

        internal const string START_DATE_META_SYSTEM_NAME = "StartDate";
        internal const string END_DATE_META_SYSTEM_NAME = "EndDate";
        internal const string SERIES_NAME_META_SYSTEM_NAME = "SeriesName";
        internal const string SERIES_ID_META_SYSTEM_NAME = "SeriesID";
        internal const string EPISODE_NUMBER_META_SYSTEM_NAME = "EpisodeNumber";
        internal const string SEASON_NUMBER_META_SYSTEM_NAME = "SeasonNumber";
        internal const string CRID_META_SYSTEM_NAME = "Crid";

        internal static readonly int MaxDescriptionSize = 1024;
        internal static readonly int MaxNameSize = 255;

        #endregion

        #region Internal Methods

        internal static List<EpgAsset> GetEpgAssetsFromCache(List<long> epgIds, int groupId, List<string> languageCodes = null)
        {
            Dictionary<string, EpgAsset> epgAssets = new Dictionary<string, EpgAsset>();

            if (epgIds == null || epgIds.Count == 0)
            {
                return epgAssets.Values.ToList();
            }

            try
            {
                eAssetTypes assetType = eAssetTypes.EPG;
                Dictionary<string, string> keyToOriginalValueMap = LayeredCacheKeys.GetAssetsKeyMap(assetType.ToString(), epgIds);
                Dictionary<string, List<string>> invalidationKeysMap = LayeredCacheKeys.GetEpgInvalidationKeysMap(groupId, assetType.ToString(), epgIds);

                if (!LayeredCache.Instance.GetValues<EpgAsset>(keyToOriginalValueMap,
                                                               ref epgAssets,
                                                               GetEpgAssets,
                                                               new Dictionary<string, object>()
                                                               {
                                                                  { "groupId", groupId },
                                                                  { "epgIds", epgIds },
                                                                  { "languageCodes", languageCodes }
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

                string defaultLanguageCode = catalogGroupCache.DefaultLanguage.Code;
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

                long newEpgId = EpgDal.InsertEpgToDB(epgCbToAdd, userId, dateTimeNow, epgMetaIdToValues, catalogGroupCache.DefaultLanguage.ID, epgTagsIds);
                if (newEpgId == 0)
                {
                    log.Error("Inesrt epg to epg_channels_schedule failed");
                    return result;
                }

                epgCbToAdd.EpgID = (ulong)newEpgId;

                // insert epg tags to DB in main lang
                var epgTags = GetEpgTags(epgAssetToAdd.Tags, allNames, defaultLanguageCode);

                // insert epgCb to CB in all languages
                SaveEpgCbToCB(epgCbToAdd, defaultLanguageCode, allNames, allDescriptions.Object, epgMetas, epgTags);

                bool indexingResult = IndexManager.UpsertProgram(groupId, new List<int>() { (int)newEpgId });
                if (!indexingResult)
                {
                    log.ErrorFormat("Failed UpsertProgram index for epg ExternalId: {0}, groupId: {1} after AddEpgAsset", epgAssetToAdd.EpgIdentifier, groupId);
                }

                result = AssetManager.GetAsset(groupId, newEpgId, eAssetTypes.EPG, true);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddEpgAsset for groupId: {0}, epg ExternalId: {1}", groupId, epgAssetToAdd.EpgIdentifier), ex);
            }

            return result;
        }

        internal static GenericResponse<Asset> UpdateEpgAsset(int groupId, EpgAsset epgAssetToUpdate, long userId, EpgAsset oldEpgAsset, CatalogGroupCache catalogGroupCache)
        {
            GenericResponse<Asset> result = new GenericResponse<Asset>();
            try
            {
                DateTime updateDate = DateTime.UtcNow;
                string defaultLanguageCode = catalogGroupCache.DefaultLanguage.Code;
                EpgCB epgCBToUpdate = null;

                bool needToUpdateBasicData;
                Dictionary<string, string> allNames;
                Dictionary<string, Dictionary<string, List<string>>> epgMetas;
                bool needToUpdateMetas;
                List<int> epgTagsIds;
                bool needToUpdateTags;
                Dictionary<FieldTypes, Dictionary<string, int>> mappingFields = GetMappingFields(groupId);

                Status validateStatus = ValidateEpgAssetForUpdate(groupId, userId, epgAssetToUpdate, oldEpgAsset, catalogGroupCache, mappingFields, out needToUpdateBasicData,
                                                                  out allNames, out epgMetas, out needToUpdateMetas, out epgTagsIds, out needToUpdateTags);

                if (!validateStatus.IsOkStatusCode())
                {
                    result.SetStatus(validateStatus);
                    return result;
                }
                
                // update Epg_channels_schedule table (basic data)
                epgCBToUpdate = CreateEpgCbFromEpgAsset(epgAssetToUpdate, groupId, epgAssetToUpdate.CreateDate.Value, updateDate);

                if (needToUpdateBasicData)
                {
                    epgCBToUpdate.Language = defaultLanguageCode;
                    epgCBToUpdate.Name = epgAssetToUpdate.Name;
                    epgCBToUpdate.Description = epgAssetToUpdate.Description;
                    DataTable dtEpgChannelsScheduleToUpdate = GetEpgChannelsScheduleTable();
                    dtEpgChannelsScheduleToUpdate.Rows.Add(GetEpgChannelsScheduleRow(epgCBToUpdate, dtEpgChannelsScheduleToUpdate, updateDate, userId));
                    if (!EpgDal.UpdateEpgChannelSchedule(dtEpgChannelsScheduleToUpdate))
                    {
                        log.Error("UpdateEpgChannelSchedule Failed");
                        return result;
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

                    EpgDal.UpdateEpgMetas(epgAssetToUpdate.Id, epgMetaIdToValues, userId, updateDate, groupId, catalogGroupCache.DefaultLanguage.ID);
                }

                if (needToUpdateTags)
                {
                    EpgDal.UpdateEpgTags(epgAssetToUpdate.Id, epgTagsIds, userId, updateDate, groupId);
                }

                Dictionary<string, Dictionary<string, List<string>>> epgTags = GetEpgTags(epgAssetToUpdate.Tags, allNames, defaultLanguageCode);

                bool validateSystemTopic = true;
                if (epgAssetToUpdate.DescriptionsWithLanguages == null || epgAssetToUpdate.DescriptionsWithLanguages.Count == 0)
                {
                    epgAssetToUpdate.DescriptionsWithLanguages = oldEpgAsset.DescriptionsWithLanguages;
                    validateSystemTopic = false;
                }
                var allDescriptions = GetSystemTopicValues(epgAssetToUpdate.Description, epgAssetToUpdate.DescriptionsWithLanguages,
                                                     catalogGroupCache, AssetManager.DESCRIPTION_META_SYSTEM_NAME, validateSystemTopic, allNames);
                if (!allDescriptions.HasObject())
                {
                    result.SetStatus(allDescriptions.Status);
                    return result;
                }
                
                // update epgCb in CB for all languages
                SaveEpgCbToCB(epgCBToUpdate, defaultLanguageCode, allNames, allDescriptions.Object, epgMetas, epgTags);

                // update index
                bool indexingResult = IndexManager.UpsertProgram(groupId, new List<int>() { (int)epgAssetToUpdate.Id });
                if (!indexingResult)
                {
                    log.ErrorFormat("Failed UpsertProgram index for assetId: {0}, groupId: {1} after UpdateEpgAsset", epgAssetToUpdate.Id, groupId);
                }

                // get updated epgAsset
                result = AssetManager.GetAsset(groupId, epgAssetToUpdate.Id, eAssetTypes.EPG, true);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed UpdateEpgAsset for groupId: {0} and epgAsset.Id: {1}. ex: {2}", groupId, epgAssetToUpdate.Id, ex);
            }

            return result;
        }

        internal static Status DeleteEpgAsset(int groupId, long epgId, long userId)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            if (EpgDal.DeleteEpgAsset(epgId, userId))
            {
                result.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                //update CB
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling DeleteEpgAsset", groupId);
                    return null;
                }

                List<LanguageObj> languages = GetLanguagesObj(new List<string>() { "*" }, catalogGroupCache);

                List<EpgCB> epgCbList = EpgDal.GetEpgCBList(epgId, languages);

                foreach (EpgCB epgCB in epgCbList)
                {
                    if (!EpgDal.DeleteEpgCB(epgCB, epgCB.Language.Equals(catalogGroupCache.DefaultLanguage.Code)))
                    {
                        log.ErrorFormat("Failed to DeleteEpgCB for epgId: {0}", epgId);
                    }
                }

                // Delete Index
                bool indexingResult = IndexManager.DeleteProgram(groupId, new List<long>() { epgId });
                if (!indexingResult)
                {
                    log.ErrorFormat("Failed to delete epg index for assetId: {0}, groupId: {1} after DeleteEpgAsset", epgId, groupId);
                }
            }
            else
            {
                log.ErrorFormat("Failed to delete epg asset with id: {0}, groupId: {1}", epgId, groupId);
            }

            return result;
        }

        internal static Status RemoveTopicsFromProgram(int groupId, HashSet<long> topicIds, long userId, CatalogGroupCache catalogGroupCache, Asset asset)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                // validate topicsIds exist on asset
                EpgAsset epgAsset = asset as EpgAsset;
                if (epgAsset != null)
                {
                    List<long> existingTopicsIds = epgAsset.Metas.Where(x => catalogGroupCache.TopicsMapBySystemName.ContainsKey(x.m_oTagMeta.m_sName))
                                                                  .Select(x => catalogGroupCache.TopicsMapBySystemName[x.m_oTagMeta.m_sName].Id).ToList();
                    existingTopicsIds.AddRange(epgAsset.Tags.Where(x => catalogGroupCache.TopicsMapBySystemName.ContainsKey(x.m_oTagMeta.m_sName))
                                                              .Select(x => catalogGroupCache.TopicsMapBySystemName[x.m_oTagMeta.m_sName].Id).ToList());
                    List<long> noneExistingMetaIds = topicIds.Except(existingTopicsIds).ToList();
                    if (noneExistingMetaIds != null && noneExistingMetaIds.Count > 0)
                    {
                        result = new Status((int)eResponseStatus.MetaIdsDoesNotExistOnAsset, string.Format("{0} for the following Meta Ids: {1}",
                                                    eResponseStatus.MetaIdsDoesNotExistOnAsset.ToString(), string.Join(",", noneExistingMetaIds)));
                        return result;
                    }

                    // get topics to removed             
                    List<Topic> topics = catalogGroupCache.TopicsMapById.Where(x => topicIds.Contains(x.Key) && !CatalogManager.TopicsToIgnore.Contains(x.Value.SystemName.ToLower())).Select(x => x.Value).ToList();

                    Dictionary<FieldTypes, Dictionary<string, int>> mappingFields = GetMappingFields(groupId);
                    
                    List<int> programMetaIds = new List<int>(topics.Where(t => mappingFields.ContainsKey(FieldTypes.Meta) &&
                                                                               mappingFields[FieldTypes.Meta].ContainsKey(t.SystemName.ToLower()))
                                                                   .Select(x => mappingFields[FieldTypes.Meta][x.SystemName.ToLower()]));

                    List<string> metasToRemoveByName = new List<string>(topics.Where(t => mappingFields.ContainsKey(FieldTypes.Meta) &&
                                                                                          mappingFields[FieldTypes.Meta].ContainsKey(t.SystemName.ToLower()))
                                                                              .Select(x => x.SystemName.ToLower()));
                    
                    List<int> programTagIds = new List<int>(topics.Where(t => mappingFields.ContainsKey(FieldTypes.Tag) &&
                                                                               mappingFields[FieldTypes.Tag].ContainsKey(t.SystemName.ToLower()))
                                                                  .Select(x => mappingFields[FieldTypes.Tag][x.SystemName.ToLower()]));

                    List<string> tagsToRemoveByName = new List<string>(topics.Where(t => mappingFields.ContainsKey(FieldTypes.Tag) &&
                                                                                          mappingFields[FieldTypes.Tag].ContainsKey(t.SystemName.ToLower()))
                                                                             .Select(x => x.SystemName.ToLower()));

                    if (EpgDal.RemoveMetasAndTagsFromProgram(groupId, epgAsset.Id, programMetaIds, programTagIds, userId))
                    {
                        result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                        // update Epg metas and tags 
                        RemoveTopicsFromProgramEpgCBs(groupId, epgAsset.Id, metasToRemoveByName, tagsToRemoveByName);

                        // UpdateIndex
                        bool indexingResult = IndexManager.UpsertProgram(groupId, new List<int>() { (int)epgAsset.Id });
                        if (!indexingResult)
                        {
                            log.ErrorFormat("Failed UpsertProgram index for assetId: {0}, type: {1}, groupId: {2} after RemoveTopicsFromProgram", epgAsset.Id, eAssetTypes.EPG.ToString(), groupId);
                        }
                    }
                    else
                    {
                        log.ErrorFormat("Failed to remove topics from program with id: {0}, type: {1}, groupId: {2}", epgAsset.Id, eAssetTypes.EPG.ToString(), groupId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed RemoveTopicsFromProgram for groupId:{0}, id:{1}, assetType:{2}", groupId, asset.Id, eAssetTypes.EPG.ToString()), ex);
            }

            return result;
        }

        internal static void InvalidateEpgs(int groupId, IEnumerable<long> epgIds, [System.Runtime.CompilerServices.CallerMemberName] string callingMethod = "")
        {
            if (epgIds != null)
            {
                foreach (var currEpgId in epgIds)
                {
                    string invalidationKey = LayeredCacheKeys.GetEpgInvalidationKey(groupId, currEpgId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to invalidate epg with invalidationKey: {0} after {1}.", invalidationKey, callingMethod);
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
                        if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId.Value, out catalogGroupCache))
                        {
                            log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetEpgAssets", groupId);
                            return null;
                        }

                        List<LanguageObj> languages = GetLanguagesObj(languageCodes, catalogGroupCache);

                        var ratios = ImageManager.GetRatios(groupId.Value);

                        foreach (var epgId in epgIds)
                        {
                            List<EpgCB> epgCbList = EpgDal.GetEpgCBList(epgId, languages);

                            if (epgCbList != null && epgCbList.Count > 0)
                            {
                                EpgAsset epgAsset = new EpgAsset(epgCbList, catalogGroupCache.DefaultLanguage.Code, ratios.Objects, groupId.Value);
                                string epgAssetKey = LayeredCacheKeys.GetAssetKey(eAssetTypes.EPG.ToString(), epgAsset.Id);
                                epgAssets.Add(epgAssetKey, epgAsset);
                            }
                        }

                        res = epgAssets.Count == epgIds.Count;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetEpgAssets failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, EpgAsset>, bool>(epgAssets, res);
        }

        private static void SaveEpgCbToCB(EpgCB epgCB, string defaultLanguageCode,
                                          Dictionary<string, string> allNames,
                                          Dictionary<string, string> allDescriptions,
                                          Dictionary<string, Dictionary<string, List<string>>> epgMetas,
                                          Dictionary<string, Dictionary<string, List<string>>> epgTags)
        {
            foreach (var currLang in allNames)
            {
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

                if (!EpgDal.SaveEpgCB(epgCB, currLang.Key.Equals(defaultLanguageCode)))
                {
                    log.ErrorFormat("Failed to SaveEpgCbToCB for epgId: {0}, languageCode: {1} in EpgAssetManager", epgCB.EpgID, currLang.Key);
                }
            }
        }
        
        private static List<LanguageObj> GetLanguagesObj(List<string> languageCodes, CatalogGroupCache catalogGroupCache)
        {
            List<LanguageObj> languages = new List<LanguageObj>();

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
                languages.Add(catalogGroupCache.DefaultLanguage);
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
                EpgIdentifier = epgAsset.EpgIdentifier,
                ChannelID = channelId,
                StartDate = epgAsset.StartDate ?? DateTime.MinValue,
                EndDate = epgAsset.EndDate ?? DateTime.MinValue,
                UpdateDate = updateDate,
                CreateDate = createDate,
                Status = 1,
                IsActive = true,
                GroupID = groupId,
                ExtraData = new EpgExtraData() { MediaID = epgAsset.RelatedMediaId.HasValue ? (int)epgAsset.RelatedMediaId.Value : 0 },
                Crid = epgAsset.Crid,
                CoGuid = epgAsset.CoGuid,
                EnableCDVR = GetEnableData(epgAsset.CdvrEnabled),
                EnableCatchUp = GetEnableData(epgAsset.CatchUpEnabled),
                EnableStartOver = GetEnableData(epgAsset.StartOverEnabled),
                EnableTrickPlay = GetEnableData(epgAsset.TrickPlayEnabled)
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
                                                        out Dictionary<string, Dictionary<string, List<string>>> epgMetas, out bool updateMetas, out List<int> epgTagsIds, out bool updateTags)
        {
            updateBasicData = false;
            allNames = null;
            epgMetas = null;
            updateMetas = true;
            epgTagsIds = null;
            updateTags = true;

            if (!string.IsNullOrEmpty(epgAssetToUpdate.EpgIdentifier) && !epgAssetToUpdate.EpgIdentifier.Equals(oldEpgAsset.EpgIdentifier))
            {
                return new Status((int)eResponseStatus.Error, "cannot update EpgIdentifier");
            }

            updateBasicData = epgAssetToUpdate.UpdateFields(oldEpgAsset);

            bool validateSystemTopic = true;
            if (epgAssetToUpdate.NamesWithLanguages == null || epgAssetToUpdate.NamesWithLanguages.Count == 0)
            {
                epgAssetToUpdate.NamesWithLanguages = oldEpgAsset.NamesWithLanguages;
                validateSystemTopic = false;
            }

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

            return new Status((int)eResponseStatus.OK);
        }

        private static List<Metas> SetEpgMetaToUpdate(CatalogGroupCache catalogGroupCache, List<Metas> epgMetasToUpdate, List<Metas> oldMetasAsset)
        {
            if (epgMetasToUpdate == null)
            {
                epgMetasToUpdate = new List<Metas>();
            }

            List<Metas> excluded = oldMetasAsset != null && oldMetasAsset.Count > 0 ?
                oldMetasAsset.Where(x => catalogGroupCache.TopicsMapBySystemName.ContainsKey(x.m_oTagMeta.m_sName) &&
                                         catalogGroupCache.AssetStructsMapById.ContainsKey(catalogGroupCache.ProgramAssetStructId) &&
                                         catalogGroupCache.AssetStructsMapById[catalogGroupCache.ProgramAssetStructId].AssetStructMetas.ContainsKey(catalogGroupCache.TopicsMapBySystemName[x.m_oTagMeta.m_sName].Id) &&
                                         !epgMetasToUpdate.Contains(x, new MetasComparer())).ToList() : null;

            if (excluded != null && excluded.Count > 0)
            {
                // set Metas original m_sType 
                foreach (Metas meta in excluded)
                {
                    meta.m_oTagMeta.m_sType = catalogGroupCache.TopicsMapBySystemName[meta.m_oTagMeta.m_sName].Type.ToString();
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

            List<Tags> excluded = oldTagsAsset != null && oldTagsAsset.Count > 0 ?
                oldTagsAsset.Where(x => catalogGroupCache.TopicsMapBySystemName.ContainsKey(x.m_oTagMeta.m_sName) &&
                                         catalogGroupCache.AssetStructsMapById.ContainsKey(catalogGroupCache.ProgramAssetStructId) &&
                                         catalogGroupCache.AssetStructsMapById[catalogGroupCache.ProgramAssetStructId].AssetStructMetas.ContainsKey(catalogGroupCache.TopicsMapBySystemName[x.m_oTagMeta.m_sName].Id) &&
                                         !epgTagsToUpdate.Contains(x, new TagsComparer())).ToList() : null;

            if (excluded != null && excluded.Count > 0)
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

            long linearAssetId = epgAssetToAdd.LinearAssetId ?? 0;
            var linearAssetResult = AssetManager.GetAsset(groupId, linearAssetId, eAssetTypes.MEDIA, true);

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

            // Add Name meta values
            var nameValues = GetSystemTopicValues(epgAssetToAdd.Name, epgAssetToAdd.NamesWithLanguages, catalogGroupCache,
                                                  AssetManager.NAME_META_SYSTEM_NAME, true);
            if (!nameValues.HasObject())
            {
                return nameValues.Status;
            }

            if (!nameValues.Object.ContainsKey(catalogGroupCache.DefaultLanguage.Code))
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

            AssetStruct programAssetStruct = null;
            if (catalogGroupCache.AssetStructsMapById.ContainsKey(catalogGroupCache.ProgramAssetStructId))
            {
                programAssetStruct = catalogGroupCache.AssetStructsMapById[catalogGroupCache.ProgramAssetStructId];
            }
            if (programAssetStruct == null)
            {
                return new Status((int)eResponseStatus.AssetStructDoesNotExist, "Program AssetStruct does not exist");
            }

            string mainCode = catalogGroupCache.DefaultLanguage.Code;

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
                        if (needToValidateMetas && !allNames.ContainsKey(otherLanguageMeta.LanguageCode))
                        {
                            return new Status((int)eResponseStatus.Error, string.Format(INVALID_LANGUAGE, otherLanguageMeta.LanguageCode));
                        }

                        if (!epgMetas.ContainsKey(otherLanguageMeta.LanguageCode))
                        {
                            epgMetas.Add(otherLanguageMeta.LanguageCode, new Dictionary<string, List<string>>());
                        }

                        if (epgMetas[otherLanguageMeta.LanguageCode].ContainsKey(meta.m_oTagMeta.m_sName))
                        {
                            if (!epgMetas[otherLanguageMeta.LanguageCode][meta.m_oTagMeta.m_sName].Contains(otherLanguageMeta.Value))

                            {
                                epgMetas[otherLanguageMeta.LanguageCode][meta.m_oTagMeta.m_sName].Add(otherLanguageMeta.Value);
                            }
                        }
                        else
                        {
                            epgMetas[otherLanguageMeta.LanguageCode].Add(meta.m_oTagMeta.m_sName, new List<string>() { otherLanguageMeta.Value });
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
            if (!catalogGroupCache.TopicsMapBySystemName.ContainsKey(tagMeta.m_sName))
            {
                var errorMsg = string.Format(META_DOES_NOT_EXIST, topicType, tagMeta.m_sName);
                return new Status((int)eResponseStatus.MetaDoesNotExist, errorMsg);
            }

            var topic = catalogGroupCache.TopicsMapBySystemName[tagMeta.m_sName];

            // validate meta exists on asset struct
            if (!programAssetStruct.AssetStructMetas.ContainsKey(topic.Id))
            {
                var errorMsg = string.Format("{0}: {1} is not part of assetStruct", topicType, tagMeta.m_sName);
                return new Status((int)eResponseStatus.Error, errorMsg);
            }

            // validate correct MetaType was sent
            MetaType sentMetaType;
            if (!Enum.TryParse<MetaType>(tagMeta.m_sType, out sentMetaType) ||
                !topic.Type.ToString().ToLower().Equals(tagMeta.m_sType.ToLower()) ||
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
                                    if (languageCodes.ContainsKey(otherTagsValue.LanguageCode) && !string.IsNullOrEmpty(otherTagsValue.Value))
                                    {
                                        if (!epgTags.ContainsKey(otherTagsValue.LanguageCode))
                                        {
                                            epgTags.Add(otherTagsValue.LanguageCode, new Dictionary<string, List<string>>());
                                        }

                                        if (epgTags[otherTagsValue.LanguageCode].ContainsKey(tags.m_oTagMeta.m_sName))
                                        {
                                            if (!epgTags[otherTagsValue.LanguageCode][tags.m_oTagMeta.m_sName].Contains(otherTagsValue.Value))
                                            {
                                                epgTags[otherTagsValue.LanguageCode][tags.m_oTagMeta.m_sName].Add(otherTagsValue.Value);
                                            }
                                        }
                                        else
                                        {
                                            epgTags[otherTagsValue.LanguageCode].Add(tags.m_oTagMeta.m_sName, new List<string>() { otherTagsValue.Value });
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

            if (validateSystemTopic && !catalogGroupCache.TopicsMapBySystemName.ContainsKey(topicSystemName))
            {
                var errorMsg = string.Format(META_DOES_NOT_EXIST, "SystemName", topicSystemName);
                topicValues.SetStatus(eResponseStatus.MetaDoesNotExist, errorMsg);
                return topicValues;
            }

            topicValues.Object = new Dictionary<string, string>();
            topicValues.SetStatus(eResponseStatus.OK);

            if (!string.IsNullOrEmpty(mainLangValue))
            {
                topicValues.Object.Add(catalogGroupCache.DefaultLanguage.Code, mainLangValue);

                if (otherLanguages != null && otherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in otherLanguages)
                    {
                        if (validateSystemTopic)
                        {
                            if (epgNames != null && !epgNames.ContainsKey(language.LanguageCode))
                            {
                                topicValues.SetStatus(eResponseStatus.Error, string.Format(INVALID_LANGUAGE, language.LanguageCode));
                                return topicValues;
                            }
                            else if (!catalogGroupCache.LanguageMapByCode.ContainsKey(language.LanguageCode))
                            {
                                var errorMsg = string.Format("language: {0} is not part of group supported languages", language.LanguageCode);
                                topicValues.SetStatus(eResponseStatus.GroupDoesNotContainLanguage, errorMsg);
                                return topicValues;
                            }

                            if (topicValues.Object.ContainsKey(language.LanguageCode))
                            {
                                var errorMsg = string.Format(DUPLICATE_VALUE, "language code", language.LanguageCode, topicSystemName);
                                topicValues.SetStatus(eResponseStatus.Error, errorMsg);
                                return topicValues;
                            }
                        }

                        topicValues.Object.Add(language.LanguageCode, language.Value);
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

        private static void RemoveTopicsFromProgramEpgCBs(int groupId, long epgId, List<string> programMetas, List<string> programTags)
        {
            CatalogGroupCache catalogGroupCache;
            if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling RemoveTopicsFromProgramEpgCBs", groupId);
                return;
            }

            List<LanguageObj> languages = GetLanguagesObj(new List<string>() { "*" }, catalogGroupCache);
            List<EpgCB> epgCbList = EpgDal.GetEpgCBList(epgId, languages);

            foreach (EpgCB epgCB in epgCbList)
            {
                RemoveTopicsFromProgramEpgCB(epgCB, programMetas, programTags);
            }
        }

        private static void RemoveTopicsFromProgramEpgCB(EpgCB epgCB, List<string> programMetas, List<string> programTags)
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

                if (!EpgDal.SaveEpgCB(epgCB, true))
                {
                    log.ErrorFormat("RemoveTopicsFromProgramEpgCB - Failed to SaveEpgCB for epgId: {0}.", epgCB.EpgID);
                }
            }
            catch (Exception exc)
            {
                log.Error(string.Format("Exception at RemoveTopicsFromProgramEpgCB for epgId: {0}.", epgCB.EpgID), exc);
            }
        }

        #endregion
    }
}