using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using GroupsCacheManager;
using KLogMonitor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OPC_Migration
{
    public class MigrateManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private int groupId;
        private long sequenceId;
        private bool shouldBackup;
        private static readonly List<string> GROUP_ID_NEEDED_TABLES = new List<string>() { "channel_media_types", "channels", "channels_media", "drm_adapters", "EPG_channel_deafults_values",
                                                                                            "epg_channels", "epg_channels_schedule", "epg_channels_schedule_translate", "epg_channels_translate",
                                                                                            "epg_comments", "EPG_fields_mapping", "EPG_metas_types", "epg_multi_pictures", "EPG_pics", "EPG_pics_sizes",
                                                                                            "EPG_program_metas", "EPG_program_tags", "EPG_tags", "EPG_tags_types", "EPG_tags_types_defaults",
                                                                                            "geo_block_types", "geo_block_types_countries", "group_epg_ratios", "group_rule_settings",
                                                                                            "groups_passwords", "groups_rules", "groups_rules_values", "media", "media_comments", "media_files",
                                                                                            "media_types", "pics", "streaming_companies", "streaming_companies_settings", "tags", "device_rules",
                                                                                            "device_rules_brands", "geo_block_types", "geo_block_types_countries", "groups_media_type",
                                                                                            "parental_rules", "parental_rule_tag_values", "tags_translate", "group_ratios" };

        public MigrateManager(int groupId, long sequenceId, bool shouldBackup)
        {
            this.groupId = groupId;
            this.sequenceId = sequenceId;
            this.shouldBackup = shouldBackup;
        }

        public eMigrationResultStatus PerformMigration(Group group, HashSet<long> groupIdsToSave, ref Dictionary<string, Core.Catalog.Ratio> groupRatios, ref Dictionary<string, ImageType> groupImageTypes,
                                        ref Dictionary<long, MediaFileType> groupMediaFileTypes, ref Dictionary<int, long> mediaTypeIdToMediaFileTypeIdMap, ref Dictionary<string, Dictionary<string, Topic>> groupTopics,
                                        Dictionary<string, AssetStruct> groupAssetStructs, Dictionary<long, Dictionary<string, string>> assetStructTopicsMap, List<MediaAsset> assets,
                                        Dictionary<long, string> picIdToImageTypeNameMap, Dictionary<long, Dictionary<string, string>> assetsImageTypesToAdd,
                                        Dictionary<long, string> picIdToUpdatedContentIdValue, List<Channel> groupChannels, HashSet<long> groupExtraLanguageIdsToSave)
        {            
            eMigrationResultStatus result = eMigrationResultStatus.OK;
            List<int> groupIds = GroupsCacheManager.Utils.Get_SubGroupsTree(groupId);
            if (!UpdateGroupExtraLanguages(groupId, groupIds, groupExtraLanguageIdsToSave))
            {
                log.Error("UpdateGroupExtraLanguages failed");
                Console.WriteLine("UpdateGroupExtraLanguages failed");
                return eMigrationResultStatus.UpdateGroupExtraLanguagesFailed;
            }

            log.Debug("UpdateGroupExtraLanguages succeeded");
            Console.WriteLine("UpdateGroupExtraLanguages succeeded");

            if (!UpdateGroupPicsIds(groupId, groupIds, groupIdsToSave))
            {
                log.Error("UpdateGroupPicsIds failed");
                Console.WriteLine("UpdateGroupPicsIds failed");
                return eMigrationResultStatus.UpdateGroupPicsIdsFailed;
            }

            log.Debug("UpdateGroupPicsIds succeeded");
            Console.WriteLine("UpdateGroupPicsIds succeeded");
            if (!UpdateGroupIdInNeededTables())
            {
                log.Error("UpdateGroupIdInNeededTables failed");
                Console.WriteLine("UpdateGroupIdInNeededTables failed");
                return eMigrationResultStatus.UpdateGroupIdInNeededTablesFailed;
            }

            log.Debug("UpdateGroupIdInNeededTables succeeded");
            Console.WriteLine("UpdateGroupIdInNeededTables succeeded");
            if (!Utils.ClearAllCaches(groupId))
            {
                log.Error("ClearAllCaches failed");
                Console.WriteLine("ClearAllCaches failed");
                return eMigrationResultStatus.ClearAllCachesFailed;
            }

            log.Debug("ClearAllCaches succeeded");
            Console.WriteLine("ClearAllCaches succeeded");
            if (!InsertGroupRatios(ref groupRatios))
            {
                log.Error("InsertGroupRatios failed");
                Console.WriteLine("InsertGroupRatios failed");
                return eMigrationResultStatus.InsertGroupRatiosFailed;
            }

            log.Debug("InsertGroupRatios succeeded");
            Console.WriteLine("InsertGroupRatios succeeded");
            if (!InsertGroupImageTypes(ref groupImageTypes, groupRatios))
            {
                log.Error("InsertGroupImageTypes failed");
                Console.WriteLine("InsertGroupImageTypes failed");
                return eMigrationResultStatus.InsertGroupImageTypesFailed;
            }

            log.Debug("InsertGroupImageTypes succeeded");
            Console.WriteLine("InsertGroupImageTypes succeeded");
            if (!UpdateGroupMediaFileTypes(ref groupMediaFileTypes))
            {
                log.Error("UpdateGroupMediaFileTypes failed");
                Console.WriteLine("UpdateGroupMediaFileTypes failed");
                return eMigrationResultStatus.UpdateGroupMediaFileTypesFailed;
            }

            log.Debug("UpdateGroupMediaFileTypes succeeded");
            Console.WriteLine("UpdateGroupMediaFileTypes succeeded");
            if (!InsertGroupTopics(ref groupTopics))
            {
                log.Error("InsertGroupTopics failed");
                Console.WriteLine("InsertGroupTopics failed");
                return eMigrationResultStatus.InsertGroupTopicsFailed;
            }

            List<KeyValuePair<long, long>> tagTypeToTopicIdMap = new List<KeyValuePair<long, long>>();
            if (group.m_oGroupTags != null && group.m_oGroupTags.Count > 0)
            {
                foreach (KeyValuePair<int, string> tagType in group.m_oGroupTags)
                {
                    if (groupTopics.ContainsKey(tagType.Value) && groupTopics[tagType.Value].ContainsKey(ApiObjects.MetaType.Tag.ToString()) 
                        && groupTopics[tagType.Value][ApiObjects.MetaType.Tag.ToString()].Id > 0)
                    {
                        tagTypeToTopicIdMap.Add(new KeyValuePair<long, long>(tagType.Key, groupTopics[tagType.Value][ApiObjects.MetaType.Tag.ToString()].Id));
                    }
                }
            }

            log.Debug("InsertGroupTopics succeeded");
            Console.WriteLine("InsertGroupTopics succeeded");
            if (!UpdateTagTableWithTopicIds(groupId, tagTypeToTopicIdMap))
            {
                log.Error("UpdateTagTableWithTopicIds failed");
                Console.WriteLine("UpdateTagTableWithTopicIds failed");
                return eMigrationResultStatus.UpdateTagTableWithTopicIdsFailed;
            }

            log.Debug("UpdateTagTableWithTopicIds succeeded");
            Console.WriteLine("UpdateTagTableWithTopicIds succeeded");
            if (!UpdateMediaConcurrencyRulesTableWithTopicIds(groupId, tagTypeToTopicIdMap))
            {
                log.Error("UpdateMediaConcurrencyRulesTableWithTopicIds failed");
                Console.WriteLine("UpdateMediaConcurrencyRulesTableWithTopicIds failed");
                return eMigrationResultStatus.UpdateMediaConcurrencyRulesTableWithTopicIdsFailed;
            }

            log.Debug("UpdateMediaConcurrencyRulesTableWithTopicIds succeeded");
            Console.WriteLine("UpdateMediaConcurrencyRulesTableWithTopicIds succeeded");
            if (!UpdateParentalRulesTableWithTopicIds(groupId, tagTypeToTopicIdMap, group.m_oEpgGroupSettings, groupTopics))
            {
                log.Error("UpdateParentalRulesTableWithTopicIds failed");
                Console.WriteLine("UpdateParentalRulesTableWithTopicIds failed");
                return eMigrationResultStatus.UpdateParentalRulesTableWithTopicIdsFailed;
            }

            log.Debug("UpdateParentalRulesTableWithTopicIds succeeded");
            Console.WriteLine("UpdateParentalRulesTableWithTopicIds succeeded");
            if (!UpdateGroupChannels(groupChannels))
            {
                log.Error("UpdateGroupChannels failed");
                Console.WriteLine("UpdateGroupChannels failed");
                return eMigrationResultStatus.UpdateGroupChannelsFailed;
            }

            log.Debug("UpdateGroupChannels succeeded");
            Console.WriteLine("UpdateGroupChannels succeeded");
            if (!Utils.ClearAllCaches(groupId))
            {
                log.Error("ClearAllCaches failed");
                Console.WriteLine("ClearAllCaches failed");
                return eMigrationResultStatus.ClearAllCachesFailed;
            }

            log.Debug("ClearAllCaches succeeded");
            Console.WriteLine("ClearAllCaches succeeded");
            if (!UpdateDuplicateTagValuesAndTagTranslations(groupId))
            {
                log.Error("UpdateDuplicateTagValuesAndParentalRuleTagValues failed");
                Console.WriteLine("UpdateDuplicateTagValuesAndParentalRuleTagValues failed");
                return eMigrationResultStatus.UpdateDuplicateTagValuesAndTagTranslationsFailed;
            }

            log.Debug("UpdateDuplicateTagValuesAndParentalRuleTagValues succeeded");
            Console.WriteLine("UpdateDuplicateTagValuesAndParentalRuleTagValues succeeded");
            if (!Utils.ClearAllCaches(groupId))
            {
                log.Error("ClearAllCaches failed");
                Console.WriteLine("ClearAllCaches failed");
                return eMigrationResultStatus.ClearAllCachesFailed;
            }

            log.Debug("ClearAllCaches succeeded");
            Console.WriteLine("ClearAllCaches succeeded");
            if (!AddOrUpdateGroupAssetStructs(groupAssetStructs, groupTopics, assetStructTopicsMap))
            {
                log.Error("AddOrUpdateGroupAssetStructs failed");
                Console.WriteLine("AddOrUpdateGroupAssetStructs failed");
                return eMigrationResultStatus.AddOrUpdateGroupAssetStructsFailed;
            }

            log.Debug("AddOrUpdateGroupAssetStructs succeeded");
            Console.WriteLine("AddOrUpdateGroupAssetStructs succeeded");
            if (!Utils.ClearAllCaches(groupId))
            {
                log.Error("ClearAllCaches failed");
                Console.WriteLine("ClearAllCaches failed");
                return eMigrationResultStatus.ClearAllCachesFailed;
            }

            log.Debug("ClearAllCaches succeeded");
            Console.WriteLine("ClearAllCaches succeeded");
            if (!UpdateGroupMediaAssets(assets))
            {
                log.Error("UpdateGroupMediaAssets failed");
                Console.WriteLine("UpdateGroupMediaAssets failed");
                return eMigrationResultStatus.UpdateGroupMediaAssetsFailed;
            }

            log.Debug("UpdateGroupMediaAssets succeeded");
            Console.WriteLine("UpdateGroupMediaAssets succeeded");
            if (!Utils.ClearAllCaches(groupId))
            {
                log.Error("ClearAllCaches failed");
                Console.WriteLine("ClearAllCaches failed");
                return eMigrationResultStatus.ClearAllCachesFailed;
            }

            log.Debug("ClearAllCaches succeeded");
            Console.WriteLine("ClearAllCaches succeeded");
            if (!AddImagesToAsset(assetsImageTypesToAdd, groupImageTypes))
            {
                log.Error("AddImagesToAsset failed");
                Console.WriteLine("AddImagesToAsset failed");
                return eMigrationResultStatus.AddImagesToAssetFailed;
            }

            log.Debug("AddImagesToAsset succeeded");
            Console.WriteLine("AddImagesToAsset succeeded");
            if (!Utils.ClearAllCaches(groupId))
            {
                log.Error("ClearAllCaches failed");
                Console.WriteLine("ClearAllCaches failed");
                return eMigrationResultStatus.ClearAllCachesFailed;
            }

            log.Debug("ClearAllCaches succeeded");
            Console.WriteLine("ClearAllCaches succeeded");
            if (!UpdateGroupMediaAssetsImages(picIdToImageTypeNameMap, groupImageTypes))
            {
                log.Error("UpdateGroupMediaAssetsImages failed");
                Console.WriteLine("UpdateGroupMediaAssetsImages failed");
                return eMigrationResultStatus.FailedValidationOfPicSizesFailed;
            }

            log.Debug("UpdateGroupMediaAssetsImages succeeded");
            Console.WriteLine("UpdateGroupMediaAssetsImages succeeded");
            if (!Utils.ClearAllCaches(groupId))
            {
                log.Error("ClearAllCaches failed");
                Console.WriteLine("ClearAllCaches failed");
                return eMigrationResultStatus.ClearAllCachesFailed;
            }

            log.Debug("ClearAllCaches succeeded");
            Console.WriteLine("ClearAllCaches succeeded");
            if (!UpdatePicsContentIdBaseUrl(picIdToUpdatedContentIdValue))
            {
                log.Error("UpdatePicsContentIdBaseUrl failed");
                Console.WriteLine("UpdatePicsContentIdBaseUrl failed");
                return eMigrationResultStatus.UpdatePicsContentIdBaseUrlFailed;
            }

            log.Debug("UpdatePicsContentIdBaseUrl succeeded");
            Console.WriteLine("UpdatePicsContentIdBaseUrl succeeded");
            if (!Utils.ClearAllCaches(groupId))
            {
                log.Error("ClearAllCaches failed");
                Console.WriteLine("ClearAllCaches failed");
                return eMigrationResultStatus.ClearAllCachesFailed;
            }

            log.Debug("ClearAllCaches succeeded");
            Console.WriteLine("ClearAllCaches succeeded");
            if (!UpdateGroupMediaAssetsFiles(mediaTypeIdToMediaFileTypeIdMap))
            {
                log.Error("UpdateGroupMediaAssetsFiles failed");
                Console.WriteLine("UpdateGroupMediaAssetsFiles failed");
                return eMigrationResultStatus.UpdateGroupMediaAssetsFilesFailed;
            }

            log.Debug("UpdateGroupMediaAssetsFiles succeeded");
            Console.WriteLine("UpdateGroupMediaAssetsFiles succeeded");
            if (!Utils.ClearAllCaches(groupId))
            {
                log.Error("ClearAllCaches failed");
                Console.WriteLine("ClearAllCaches failed");
                return eMigrationResultStatus.ClearAllCachesFailed;
            }

            log.Debug("ClearAllCaches succeeded");
            Console.WriteLine("ClearAllCaches succeeded");
            return result;
        }

        private bool UpdateGroupExtraLanguages(int groupId, List<int> groupIds, HashSet<long> groupExtraLanguageIdsToSave)
        {
            bool res = true;
            try
            {
                if (groupExtraLanguageIdsToSave != null && groupExtraLanguageIdsToSave.Count > 0)
                {
                    res = OPCMigrationDAL.UpdateGroupExtraLanguages(groupId, groupIds, groupExtraLanguageIdsToSave, Utils.UPDATING_USER_ID, sequenceId, shouldBackup);
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed OPCMigrationDAL.UpdateGroupExtraLanguages", ex);
                res = false;
            }

            return res;
        }

        private bool UpdateGroupPicsIds(int groupId, List<int> groupIds, HashSet<long> groupIdsToSave)
        {
            bool res = true;
            try
            {
                if (groupIdsToSave != null && groupIdsToSave.Count > 0)
                {
                    res = OPCMigrationDAL.UpdateGroupPicIds(groupId, groupIds, groupIdsToSave, Utils.UPDATING_USER_ID, sequenceId, shouldBackup);
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed OPCMigrationDAL.UpdateGroupPicIds", ex);
                res = false;
            }

            return res;
        }

        private bool InsertGroupRatios(ref Dictionary<string, Core.Catalog.Ratio> groupRatios)
        {
            bool res = true;
            try
            {
                foreach (Core.Catalog.Ratio ratio in groupRatios.Values)
                {
                    RatioResponse response = Core.Catalog.CatalogManagement.ImageManager.AddRatio(groupId, Utils.UPDATING_USER_ID, ratio);
                    if (response == null || response.Status == null || response.Status.Code != (int)eResponseStatus.OK || response.Ratio == null || response.Ratio.Id == 0)
                    {
                        log.ErrorFormat("Failed adding ratio with name: {0}", ratio.Name);
                        res = false;
                        break;
                    }
                    else
                    {
                        ratio.Id = response.Ratio.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("failed ImageManager.AddRatio", ex);
                res = false;
            }

            return res;
        }

        private bool InsertGroupImageTypes(ref Dictionary<string, ImageType> groupImageTypes, Dictionary<string, Core.Catalog.Ratio> groupRatios)
        {
            bool res = true;
            try
            {
                foreach (KeyValuePair<string, ImageType> imageType in groupImageTypes)
                {
                    Core.Catalog.Ratio ratio = groupRatios.ContainsKey(imageType.Key) ? groupRatios[imageType.Key] : null;
                    if (ratio != null && ratio.Id > 0)
                    {
                        imageType.Value.RatioId = ratio.Id;
                    }

                    GenericResponse <ImageType> response = Core.Catalog.CatalogManagement.ImageManager.AddImageType(groupId, imageType.Value, Utils.UPDATING_USER_ID);
                    if (!response.HasObject() || response.Object.Id == 0)
                    {
                        log.ErrorFormat("Failed adding imageType with name: {0}", imageType.Key);
                        res = false;
                        break;
                    }
                    else
                    {
                        imageType.Value.Id = response.Object.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("failed ImageManager.AddImageType", ex);
                res = false;
            }

            return res;
        }

        private bool UpdateGroupMediaFileTypes(ref Dictionary<long, MediaFileType> groupMediaFileTypes)
        {
            bool res = true;
            try
            {
                foreach (MediaFileType mediaFileType in groupMediaFileTypes.Values)
                {
                    GenericResponse<MediaFileType> response = FileManager.UpdateMediaFileType(groupId, mediaFileType.Id, mediaFileType, Utils.UPDATING_USER_ID);
                    if (!response.HasObject() || response.Object.Id == 0)
                    {
                        log.ErrorFormat("Failed updating media file type with id: {0}", mediaFileType.Id);
                        res = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("failed FileManager.UpdateMediaFileType", ex);
                res = false;
            }

            return res;
        }

        private bool InsertGroupTopics(ref Dictionary<string, Dictionary<string, Topic>> groupTopics)
        {
            bool res = true;
            try
            {
                foreach (Dictionary<string, Topic> pair in groupTopics.Values)
                {
                    foreach (Topic topicToAdd in pair.Values)
                    {
                        GenericResponse<Topic> response = CatalogManager.AddTopic(groupId, topicToAdd, Utils.UPDATING_USER_ID, false);
                        if (!response.HasObject() || response.Object.Id == 0)
                        {
                            log.ErrorFormat("Failed adding topic with systemName: {0}", topicToAdd.SystemName);
                            res = false;
                            break;
                        }
                        else
                        {
                            topicToAdd.Id = response.Object.Id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("failed CatalogManager.AddTopic", ex);
                res = false;
            }

            return res;
        }

        private bool UpdateTagTableWithTopicIds(int groupId, List<KeyValuePair<long, long>> tagTypeToTopicIdMap)
        {
            bool res = true;
            try
            {
                if (tagTypeToTopicIdMap.Count > 0)
                {
                    if (!OPCMigrationDAL.UpdateTagTableWithTopicIds(groupId, tagTypeToTopicIdMap, Utils.UPDATING_USER_ID, sequenceId, shouldBackup))
                    {
                        log.Error("Failed to update tags table with topic ids");
                        res = false;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("failed !OPCMigrationDAL.UpdateTagTableWithTopicIds", ex);
                res = false;
            }

            return res;
        }

        private bool UpdateParentalRulesTableWithTopicIds(int groupId, List<KeyValuePair<long, long>> tagTypeToTopicIdMap, GroupsCacheManager.EpgGroupSettings epgGroupSettings, Dictionary<string, Dictionary<string, Topic>> groupTopics)
        {
            bool res = true;
            try
            {
                if (tagTypeToTopicIdMap.Count > 0)
                {
                    if (!OPCMigrationDAL.UpdateParentalRulesTableWithTopicIds(groupId, tagTypeToTopicIdMap, false, Utils.UPDATING_USER_ID, sequenceId))
                    {
                        log.Error("Failed to update media tag types for parental rules");
                        res = false;
                    }
                }

                if (res && epgGroupSettings != null && epgGroupSettings.tags != null && epgGroupSettings.tags.Count > 0)
                {
                    List<KeyValuePair<long, long>> epgTagTypeToTopicIdMap = new List<KeyValuePair<long, long>>();
                    foreach (KeyValuePair<long, string> epgTagType in epgGroupSettings.tags)
                    {
                        if (groupTopics.ContainsKey(epgTagType.Value) && groupTopics[epgTagType.Value].ContainsKey(ApiObjects.MetaType.Tag.ToString())
                            && groupTopics[epgTagType.Value][ApiObjects.MetaType.Tag.ToString()].Id > 0)
                        {
                            epgTagTypeToTopicIdMap.Add(new KeyValuePair<long, long>(epgTagType.Key, groupTopics[epgTagType.Value][ApiObjects.MetaType.Tag.ToString()].Id));
                        }
                    }

                    if (epgTagTypeToTopicIdMap.Count > 0)
                    {
                        if (!OPCMigrationDAL.UpdateParentalRulesTableWithTopicIds(groupId, epgTagTypeToTopicIdMap, true, Utils.UPDATING_USER_ID, sequenceId))
                        {
                            res = false;
                            log.Error("Failed to update epg tag types for parental rules");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("failed OPCMigrationDAL.UpdateParentalRulesTableWithTopicIds", ex);
                res = false;
            }

            return res;
        }

        private bool UpdateMediaConcurrencyRulesTableWithTopicIds(int groupId, List<KeyValuePair<long, long>> tagTypeToTopicIdMap)
        {
            bool res = true;
            try
            {
                if (tagTypeToTopicIdMap.Count > 0)
                {
                    if (!OPCMigrationDAL.UpdateMediaConcurrencyRulesTableWithTopicIds(groupId, tagTypeToTopicIdMap, Utils.UPDATING_USER_ID, sequenceId, shouldBackup))
                    {
                        log.Error("Failed to update media tag types for media concurrency rules");
                        res = false;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("failed OPCMigrationDAL.UpdateMediaConcurrencyRulesTableWithTopicIds", ex);
                res = false;
            }

            return res;
        }

        private bool AddOrUpdateGroupAssetStructs(Dictionary<string, AssetStruct> groupAssetStructs, Dictionary<string, Dictionary<string, Topic>> groupTopics, Dictionary<long, Dictionary<string, string>> assetStructTopicsMap)
        {
            bool res = true;
            try
            {
                if (groupAssetStructs != null && groupAssetStructs.Count > 0)
                {
                    if (shouldBackup)
                    {
                        List<long> idsToUpdateOnBackupTable = groupAssetStructs.Values.Where(x => x.Id > 0).Select(x => x.Id).ToList();
                        if (!OPCMigrationDAL.UpdateIdsAsMigratedInOpcBackupTable("media_types", idsToUpdateOnBackupTable, sequenceId))
                        {
                            log.ErrorFormat("Failed UpdateIdsAsMigratedInOpcBackupTable for media_types table, ids: {0}, SequenceId: {1}", string.Join(",", idsToUpdateOnBackupTable), sequenceId);
                            return false;
                        }
                    }

                    foreach (AssetStruct assetStruct in groupAssetStructs.Values)
                    {
                        if (assetStructTopicsMap.ContainsKey(assetStruct.Id))
                        {
                            foreach (KeyValuePair<string, string> topicSystemNameAndType in assetStructTopicsMap[assetStruct.Id])
                            {
                                if (groupTopics.ContainsKey(topicSystemNameAndType.Key) 
                                    && groupTopics[topicSystemNameAndType.Key].ContainsKey(topicSystemNameAndType.Value))
                                {
                                    assetStruct.MetaIds.Add(groupTopics[topicSystemNameAndType.Key][topicSystemNameAndType.Value].Id);
                                }
                                else
                                {
                                    // THINK WHAT TO DO HERE OTHER THEN LOG
                                    log.ErrorFormat("topic {0} does not exist on groupTopics but exist on assetStruct {1}", topicSystemNameAndType, assetStruct.SystemName);
                                }
                            }
                        }

                        GenericResponse<AssetStruct> response = null;
                        // id = 0 for program or linear
                        if (assetStruct.Id > 0)
                        {
                            response = CatalogManager.UpdateAssetStruct(groupId, assetStruct.Id, assetStruct, true, Utils.UPDATING_USER_ID, false);
                        }
                        else
                        {
                            bool isProgramStruct = assetStruct.SystemName == Utils.PROGRAM_ASSET_STRUCT;
                            response = CatalogManager.AddAssetStruct(groupId, assetStruct, Utils.UPDATING_USER_ID, isProgramStruct);
                        }

                        if (response == null || response.Status == null || response.Status.Code != (int)eResponseStatus.OK || response.Object == null || response.Object.Id == 0)
                        {
                            log.ErrorFormat("Failed to add/update assetStruct with systemName: {0}", assetStruct.SystemName);
                            res = false;
                            break;
                        }
                        else
                        {
                            assetStruct.Id = response.Object.Id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("failed CatalogManager.UpdateAssetStruct / CatalogManager.AddAssetStruct", ex);
                res = false;
            }

            return res;
        }

        private bool UpdateGroupMediaAssets(List<MediaAsset> assets)
        {
            bool res = true;
            try
            {
                if (assets != null && assets.Count > 0)
                {
                    ConcurrentDictionary<long, ConcurrentBag<long>> idsToUpdateOnBackupTable = new ConcurrentDictionary<long, ConcurrentBag<long>>();
                    ConcurrentDictionary<long, ConcurrentBag<long>> idsToUpdateOnEpgChannelsBackupTable = new ConcurrentDictionary<long, ConcurrentBag<long>>();
                    ConcurrentBag<MediaAsset> failedMediaAssets = new ConcurrentBag<MediaAsset>();
                    ConcurrentBag<LiveAsset> failedLiveAssets = new ConcurrentBag<LiveAsset>();
                    int bulkIdsToUpdateSize = Utils.GetBulkIdsUpdateSize();
                    Parallel.ForEach(assets, (mediaAsset, state) =>
                    {
                        int pageIndex = 0;
                        bool addedToBulkUpdateTableCollection = false;
                        while (!addedToBulkUpdateTableCollection)
                        {
                            if (!idsToUpdateOnBackupTable.ContainsKey(pageIndex))
                            {
                                idsToUpdateOnBackupTable.TryAdd(pageIndex, new ConcurrentBag<long>());
                            }

                            if (idsToUpdateOnBackupTable[pageIndex].Count < bulkIdsToUpdateSize)
                            {
                                idsToUpdateOnBackupTable[pageIndex].Add(mediaAsset.Id);
                                addedToBulkUpdateTableCollection = true;
                            }
                            else
                            {
                                pageIndex++;
                            }
                        }

                        bool isLiveAsset = mediaAsset.MediaAssetType == ApiObjects.MediaAssetType.Linear;
                        LiveAsset liveAsset = null;
                        if (isLiveAsset)
                        {
                            liveAsset = mediaAsset as LiveAsset;
                            if (liveAsset.EpgChannelId > 0)
                            {
                                int epgChannelspageIndex = 0;
                                bool addedToBulkUpdateEpgChannelTableCollection = false;
                                while (!addedToBulkUpdateEpgChannelTableCollection)
                                {
                                    if (!idsToUpdateOnEpgChannelsBackupTable.ContainsKey(epgChannelspageIndex))
                                    {
                                        idsToUpdateOnEpgChannelsBackupTable.TryAdd(epgChannelspageIndex, new ConcurrentBag<long>());
                                    }

                                    if (idsToUpdateOnEpgChannelsBackupTable[epgChannelspageIndex].Count < bulkIdsToUpdateSize)
                                    {
                                        idsToUpdateOnEpgChannelsBackupTable[epgChannelspageIndex].Add(liveAsset.EpgChannelId);
                                        addedToBulkUpdateEpgChannelTableCollection = true;
                                    }
                                    else
                                    {
                                        epgChannelspageIndex++;
                                    }
                                }
                            }
                        }


                        GenericResponse<Asset> response = AssetManager.UpdateAsset(groupId, mediaAsset.Id, isLiveAsset ? liveAsset : mediaAsset, Utils.UPDATING_USER_ID, false, true, true);
                        if (!response.HasObject() || response.Object.Id != mediaAsset.Id)
                        {
                            log.ErrorFormat("failed to update asset with id, will retry synchronously: {0}", mediaAsset.Id);
                            if (isLiveAsset)
                            {
                                failedLiveAssets.Add(liveAsset);
                            }
                            else
                            {
                                failedMediaAssets.Add(mediaAsset);
                            }
                        }

                    });

                    if (failedLiveAssets.Count > 0)
                    {
                        log.DebugFormat("Retrying update of failed liveAssets");
                        foreach (LiveAsset assetToRetry in failedLiveAssets)
                        {
                            GenericResponse<Asset> response = AssetManager.UpdateAsset(groupId, assetToRetry.Id, assetToRetry, Utils.UPDATING_USER_ID, false, true, true);
                            if (!response.HasObject() || response.Object.Id != assetToRetry.Id)
                            {
                                log.ErrorFormat("failed to update asset with id: {0}", assetToRetry.Id);
                                return false;
                            }
                        }
                    }

                    if (failedMediaAssets.Count > 0)
                    {
                        log.DebugFormat("Retrying update of failed mediaAssets");
                        foreach (MediaAsset assetToRetry in failedMediaAssets)
                        {
                            GenericResponse<Asset> response = AssetManager.UpdateAsset(groupId, assetToRetry.Id, assetToRetry, Utils.UPDATING_USER_ID, false, true, true);
                            if (!response.HasObject() || response.Object.Id != assetToRetry.Id)
                            {
                                log.ErrorFormat("failed to update asset with id: {0}", assetToRetry.Id);
                                return false;
                            }
                        }
                    }

                    if (shouldBackup)
                    {
                        ConcurrentBag<ConcurrentBag<long>> failedMediaIdsToUpdate = new ConcurrentBag<ConcurrentBag<long>>();
                        Parallel.ForEach(idsToUpdateOnBackupTable, (pageToUpdate, state) =>
                        {
                            if (!OPCMigrationDAL.UpdateIdsAsMigratedInOpcBackupTable("media", pageToUpdate.Value.ToList(), sequenceId))
                            {
                                log.ErrorFormat("Failed UpdateIdsAsMigratedInOpcBackupTable for media table, will retry synchronously, ids: {0}, SequenceId: {1}",
                                                string.Join(",", pageToUpdate.Value), sequenceId);
                                failedMediaIdsToUpdate.Add(pageToUpdate.Value);
                            }
                        });

                        if (failedMediaIdsToUpdate.Count > 0)
                        {
                            log.DebugFormat("Retrying update of failed ids on media backup table");
                            foreach (ConcurrentBag<long> ids in failedMediaIdsToUpdate)
                            {
                                if (!OPCMigrationDAL.UpdateIdsAsMigratedInOpcBackupTable("media", ids.ToList(), sequenceId))
                                {
                                    log.ErrorFormat("Failed UpdateIdsAsMigratedInOpcBackupTable for media table, ids: {0}, SequenceId: {1}",
                                                    string.Join(",", ids), sequenceId);
                                    return false;
                                }
                            }
                        }

                        ConcurrentBag<ConcurrentBag<long>> failedEpgChannelIdsToUpdate = new ConcurrentBag<ConcurrentBag<long>>();
                        Parallel.ForEach(idsToUpdateOnEpgChannelsBackupTable, (pageToUpdate, state) =>
                        {
                            if (!OPCMigrationDAL.UpdateIdsAsMigratedInOpcBackupTable("epg_channels", pageToUpdate.Value.ToList(), sequenceId))
                            {
                                log.ErrorFormat("Failed UpdateIdsAsMigratedInOpcBackupTable for epg_channels table, will retry synchronously, id: {0}, SequenceId: {1}", pageToUpdate.Value, sequenceId);
                                failedEpgChannelIdsToUpdate.Add(pageToUpdate.Value);
                            }
                        });

                        if (failedMediaIdsToUpdate.Count > 0)
                        {
                            log.DebugFormat("Retrying update of failed ids on epg_channels backup table");
                            foreach (ConcurrentBag<long> ids in failedEpgChannelIdsToUpdate)
                            {
                                if (!OPCMigrationDAL.UpdateIdsAsMigratedInOpcBackupTable("epg_channels", ids.ToList(), sequenceId))
                                {
                                    log.ErrorFormat("Failed UpdateIdsAsMigratedInOpcBackupTable for epg_channels table, ids: {0}, SequenceId: {1}",
                                                    string.Join(",", ids), sequenceId);
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("failed AssetManager.UpdateAsset", ex);
                res = false;
            }

            return res;
        }

        private bool AddImagesToAsset(Dictionary<long, Dictionary<string, string>> assetsImageTypesToAdd, Dictionary<string, ImageType> groupImageTypes)
        {
            bool res = true;
            try
            {
                foreach (KeyValuePair<long, Dictionary<string, string>> assetImages in assetsImageTypesToAdd)
                {
                    if (!res)
                    {
                        break;
                    }

                    foreach (KeyValuePair<string, string> imageTypeAndBaseUrl in assetImages.Value)
                    {
                        if (!res)
                        {
                            break;
                        }

                        if (groupImageTypes.ContainsKey(imageTypeAndBaseUrl.Key))
                        {
                            Image imageToAdd = new Image()
                            {
                                ImageObjectType = ApiObjects.eAssetImageType.Media,
                                ImageTypeId = groupImageTypes[imageTypeAndBaseUrl.Key].Id,
                                ImageObjectId = assetImages.Key
                            };

                            GenericResponse<Image> response = Core.Catalog.CatalogManagement.ImageManager.AddImage(groupId, imageToAdd, Utils.UPDATING_USER_ID);
                            if (!response.HasObject())
                            {
                                res = false;
                                log.ErrorFormat("Failed to add image for mediaId: {0}, imageTypeId: {1}", imageToAdd.ImageObjectId, imageToAdd.ImageTypeId);
                                break;
                            }

                            res = TVinciShared.ImageUtils.UpdateImageState(groupId, response.Object.ReferenceId, 0, ApiObjects.eMediaType.VOD, ApiObjects.eTableStatus.OK,
                                                                        (int)Utils.UPDATING_USER_ID, imageTypeAndBaseUrl.Value);
                        }
                        else
                        {
                            res = false;
                            log.ErrorFormat("couldn't find imageType {0} on group image types, failed to add image to asset {1}", imageTypeAndBaseUrl, assetImages.Key);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("failed ImageManager.AddImage / TVinciShared.ImageUtils.UpdateImageState", ex);
                res = false;
            }

            return res;
        }

        private bool UpdatePicsContentIdBaseUrl(Dictionary<long, string> contentIdToUpdatedContentIdValue)
        {
            bool res = true;
            try
            {
                if (contentIdToUpdatedContentIdValue != null && contentIdToUpdatedContentIdValue.Count > 0)
                {
                    List<KeyValuePair<long, string>> picIdToUpdatedBaseUrl = contentIdToUpdatedContentIdValue.Select(x => new KeyValuePair<long, string>(x.Key, x.Value)).ToList();
                    if (picIdToUpdatedBaseUrl.Count > 0)
                    {
                        int bulkSize = 500;
                        bool updateMorePicsBaseUrls = true;
                        while (updateMorePicsBaseUrls)
                        {
                            int amountToTake = Math.Min(bulkSize, picIdToUpdatedBaseUrl.Count);
                            if (!OPCMigrationDAL.UpdatePicContentIdsBaseUrl(picIdToUpdatedBaseUrl.Take(amountToTake).ToList(), Utils.UPDATING_USER_ID, sequenceId, shouldBackup))
                            {
                                updateMorePicsBaseUrls = false;
                                res = false;                                
                            }
                            else
                            {
                                picIdToUpdatedBaseUrl.RemoveRange(0, amountToTake);
                                if (picIdToUpdatedBaseUrl.Count == 0)
                                {
                                    updateMorePicsBaseUrls = false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("failed OPCMigrationDAL.UpdatePicContentIdsBaseUrl", ex);
                res = false;
            }

            return res;
        }

        private bool UpdateGroupMediaAssetsImages(Dictionary<long, string> picIdToImageTypeNameMap, Dictionary<string, ImageType> groupImageTypes)
        {
            bool res = true;
            try
            {
                if (picIdToImageTypeNameMap != null && picIdToImageTypeNameMap.Count > 0)
                {
                    List<KeyValuePair<long, long>> picIdToImageTypeId = new List<KeyValuePair<long, long>>();
                    foreach (KeyValuePair<long, string> picToUpdate in picIdToImageTypeNameMap)
                    {
                        if (groupImageTypes.ContainsKey(picToUpdate.Value))
                        {
                            picIdToImageTypeId.Add(new KeyValuePair<long, long>(picToUpdate.Key, groupImageTypes[picToUpdate.Value].Id));
                        }
                    }

                    if (picIdToImageTypeId.Count > 0)
                    {
                        int bulkSize = 500;
                        bool updateMorePics = true;
                        while (updateMorePics)
                        {
                            int amountToTake = Math.Min(bulkSize, picIdToImageTypeId.Count);
                            if (!OPCMigrationDAL.UpdatePicsTableWithImageTypeId(picIdToImageTypeId.Take(amountToTake).ToList(), Utils.UPDATING_USER_ID, sequenceId, shouldBackup))
                            {
                                updateMorePics = false;
                                res = false;
                            }
                            else
                            {
                                picIdToImageTypeId.RemoveRange(0, amountToTake);
                                if (picIdToImageTypeId.Count == 0)
                                {
                                    updateMorePics = false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("failed OPCMigrationDAL.UpdatePicsTableWithImageTypeId", ex);
                res = false;
            }

            return res;
        }

        private bool UpdateGroupMediaAssetsFiles(Dictionary<int, long> mediaTypeIdToMediaFileTypeIdMap)
        {
            bool res = true;
            try
            {
                if (mediaTypeIdToMediaFileTypeIdMap.Count > 0)
                {
                    res = OPCMigrationDAL.UpdateMediaFilesTableWithMediaTypeId(groupId, mediaTypeIdToMediaFileTypeIdMap.ToList(), Utils.UPDATING_USER_ID, sequenceId);
                }
            }
            catch (Exception ex)
            {
                log.Error("failed OPCMigrationDAL.UpdateMediaFilesTableWithMediaTypeId", ex);
                res = false;
            }

            return res;
        }

        private bool UpdateGroupIdInNeededTables()
        {
            bool res = true;
            try
            {
                ConcurrentBag<string> failedTables = new ConcurrentBag<string>();
                Parallel.ForEach(GROUP_ID_NEEDED_TABLES, (tableName) =>
                {
                    if (string.IsNullOrEmpty(tableName))
                    {
                        res = false;
                    }
                    else
                    {
                        bool withUpdateUserId = true;
                        bool withUpdateDate = true;
                        switch (tableName)
                        {
                            case "EPG_fields_mapping":
                            case "EPG_pics_sizes":
                                withUpdateDate = false;
                                break;
                            case "EPG_tags_types_defaults":
                                withUpdateUserId = false;
                                withUpdateDate = false;
                                break;
                            default:
                                break;
                        }

                        if (OPCMigrationDAL.UpdateTableGroupId(tableName, groupId, sequenceId, withUpdateUserId ? Utils.UPDATING_USER_ID : 0, withUpdateDate ? 1 : 0, shouldBackup))
                        {
                            log.DebugFormat("table {0} update groupId is done", tableName);
                        }
                        else
                        {
                            failedTables.Add(tableName);
                            log.WarnFormat("table {0} update groupId has failed, retry will be tried once", tableName);
                        }
                    }
                });

                if (failedTables.Count > 0)
                {
                    log.DebugFormat("starting update group id retry for all failed tables");
                    foreach (string tableName in failedTables)
                    {
                        if (string.IsNullOrEmpty(tableName))
                        {
                            res = false;
                        }
                        else
                        {                            
                            bool withUpdateUserId = true;
                            bool withUpdateDate = true;
                            switch (tableName)
                            {
                                case "EPG_fields_mapping":
                                case "EPG_pics_sizes":
                                    withUpdateDate = false;
                                    break;
                                case "EPG_tags_types_defaults":
                                    withUpdateUserId = false;
                                    withUpdateDate = false;
                                    break;
                                default:
                                    break;
                            }

                            log.DebugFormat("retrying update groupId for table {0}", tableName);
                            if (OPCMigrationDAL.UpdateTableGroupId(tableName, groupId, sequenceId, withUpdateUserId ? Utils.UPDATING_USER_ID : 0, withUpdateDate ? 1 : 0, shouldBackup))
                            {
                                log.DebugFormat("table {0} update groupId is done", tableName);
                            }
                            else
                            {                                
                                log.WarnFormat("table {0} update groupId retry has failed", tableName);
                                res = false;
                            }
                        }
                    }
                }

                if (res)
                {
                    res = OPCMigrationDAL.UpdateGroupIdInNeededTables(groupId, Utils.UPDATING_USER_ID, sequenceId, shouldBackup);
                }
            }
            catch (Exception ex)
            {
                log.Error("failed OPCMigrationDAL.UpdateGroupIdInNeededTables", ex);
                res = false;
            }

            return res;
        }

        private bool UpdateGroupChannels(List<Channel> groupChannels)
        {
            bool res = true;
            try
            {
                if (groupChannels != null && groupChannels.Count > 0)
                {
                    ConcurrentDictionary<long, ConcurrentBag<long>> idsToUpdateOnBackupTable = new ConcurrentDictionary<long, ConcurrentBag<long>>();
                    ConcurrentBag<Channel> failedChannels = new ConcurrentBag<Channel>();
                    int bulkIdsToUpdateSize = Utils.GetBulkIdsUpdateSize();
                    Parallel.ForEach(groupChannels, (channel, state) =>
                    {
                        int pageIndex = 0;
                        bool addedToBulkUpdateTableCollection = false;
                        while (!addedToBulkUpdateTableCollection)
                        {
                            if (!idsToUpdateOnBackupTable.ContainsKey(pageIndex))
                            {
                                idsToUpdateOnBackupTable.TryAdd(pageIndex, new ConcurrentBag<long>());
                            }

                            if (idsToUpdateOnBackupTable[pageIndex].Count < bulkIdsToUpdateSize)
                            {
                                idsToUpdateOnBackupTable[pageIndex].Add(channel.m_nChannelID);
                                addedToBulkUpdateTableCollection = true;
                            }
                            else
                            {
                                pageIndex++;
                            }
                        }
                        
                        GenericResponse<Channel> response = ChannelManager.UpdateChannel(groupId, channel.m_nChannelID, channel, Utils.UPDATING_USER_ID, true);
                        if (!response.HasObject() || response.Object.m_nChannelID != channel.m_nChannelID)
                        {
                            log.ErrorFormat("Failed to update channel with id: {0}, will retry synchronously", channel.m_nChannelID);
                            failedChannels.Add(channel);
                        }
                    });

                    if (failedChannels.Count > 0)
                    {
                        log.DebugFormat("Retrying update of failed channels");
                        foreach (Channel channel in failedChannels)
                        {
                            GenericResponse<Channel> response = ChannelManager.UpdateChannel(groupId, channel.m_nChannelID, channel, Utils.UPDATING_USER_ID, true);
                            if (!response.HasObject() || response.Object.m_nChannelID != channel.m_nChannelID)
                            {
                                log.ErrorFormat("Failed to update channel with id: {0}", channel.m_nChannelID);
                                return false;
                            }
                        }
                    }

                    if (shouldBackup)
                    {
                        ConcurrentBag<ConcurrentBag<long>> failedIdsToUpdate = new ConcurrentBag<ConcurrentBag<long>>();
                        Parallel.ForEach(idsToUpdateOnBackupTable, (pageToUpdate, state) =>
                        {
                            if (!OPCMigrationDAL.UpdateIdsAsMigratedInOpcBackupTable("channels", pageToUpdate.Value.ToList(), sequenceId))
                            {
                                log.ErrorFormat("Failed UpdateIdsAsMigratedInOpcBackupTable for channels table, will retry synchronously. id: {0}, SequenceId: {1}", string.Join(",", pageToUpdate.Value), sequenceId);
                                failedIdsToUpdate.Add(pageToUpdate.Value);
                            }
                        });

                        if (failedIdsToUpdate.Count > 0)
                        {
                            log.DebugFormat("Retrying update of failed ids on channels backup table");
                            foreach (ConcurrentBag<long> ids in failedIdsToUpdate)
                            {
                                if (!OPCMigrationDAL.UpdateIdsAsMigratedInOpcBackupTable("channels", ids.ToList(), sequenceId))
                                {
                                    log.ErrorFormat("Failed UpdateIdsAsMigratedInOpcBackupTable for channels table. id: {0}, SequenceId: {1}", string.Join(",", ids), sequenceId);
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("failed ChannelManager.UpdateChannel", ex);
                res = false;
            }

            return res;
        }

        private bool UpdateDuplicateTagValuesAndTagTranslations(int groupId)
        {
            bool res = true;
            try
            {
                if (!OPCMigrationDAL.UpdateDuplicateTagValuesAndTagTranslations(groupId, Utils.UPDATING_USER_ID, sequenceId, shouldBackup))
                {
                    log.Error("Failed to update duplicate tag values and tag translations");
                    res = false;
                }
            }
            catch (Exception ex)
            {
                log.Error("failed OPCMigrationDAL.UpdateDuplicateTagValuesAndTagTranslations", ex);
                res = false;
            }

            return res;
        }

    }
}
