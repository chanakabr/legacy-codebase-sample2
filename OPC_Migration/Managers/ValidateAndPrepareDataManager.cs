using ApiObjects;
using ApiObjects.Catalog;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using GroupsCacheManager;
using Phx.Lib.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace OPC_Migration
{
    public class ValidateAndPrepareDataManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string TOPIC_SEARCH_RELATED_FEATURE = "searchRelated";
        private const string TOPIC_METADATA_FEATURE = "metadata";
        private const string TOPIC_DUPLICATE_FEATURE = "duplicate";
        const string GROUP_TAG_FREE_CB_VALUE = "free";
        const string GROUP_TAG_FREE_DB_VALUE = "Free";

        private int groupId;
        private int regularGroupId;
        private int linearMediaTypeId;
        private int programMediaTypeId;

        // Will be used in case the account has more than 1 linear media types
        public HashSet<int> linearMediaTypeIds = new HashSet<int>();

        public ValidateAndPrepareDataManager(int groupId, int regularGroupId, int linearMediaTypeId, int programMediaTypeId)
        {
            this.groupId = groupId;
            this.regularGroupId = regularGroupId;
            this.linearMediaTypeId = linearMediaTypeId;
            this.programMediaTypeId = programMediaTypeId;
        }

        public List<eMigrationResultStatus> PrepareMigrationData(
            Group group, 
            ref Dictionary<string, Core.Catalog.Ratio> groupRatios,
            ref Dictionary<string, ImageType> groupImageTypes, 
            ref Dictionary<long, MediaFileType> groupMediaFileTypes,
            ref Dictionary<int, long> mediaTypeIdToMediaFileTypeIdMap, 
            ref Dictionary<string, Dictionary<string, Topic>> groupTopics, 
            ref Dictionary<string, AssetStruct> groupAssetStructs,
            ref Dictionary<long, Dictionary<string, string>> assetStructTopicsMap, 
            ref List<MediaAsset> assets, 
            ref Dictionary<long, string> picIdToImageTypeNameMap,
            ref List<Channel> groupChannels, 
            ref Dictionary<long, Dictionary<string, string>> assetsImageTypesToAdd,
            ref Dictionary<long, string> picIdToUpdatedContentIdValue)
        {
            List<eMigrationResultStatus> result = new List<eMigrationResultStatus>();
            List<int> groupIds = GroupsCacheManager.Utils.Get_SubGroupsTree(groupId);
            // Get all linear media types so we will know when migrating medias
            linearMediaTypeIds = GetGroupLinearMediaTypeIds(groupIds);

            if (!CreateGroupRatios(groupIds, ref groupRatios))
            {
                log.Error("CreateGroupRatios failed");
                result.Add(eMigrationResultStatus.FailedValidationOfGroupRatios);
            }
            else
            {
                log.Debug("CreateGroupRatios succeeded");
            }

            if (!CreateGroupImageTypes(groupRatios, ref groupImageTypes))
            {
                log.Error("CreateGroupImageTypes failed");
                result.Add(eMigrationResultStatus.FailedValidationOfGroupImageTypes);
            }
            else
            {
                log.Debug("CreateGroupImageTypes succeeded");
            }

            if (!CreateGroupFilesTypes(groupIds, ref groupMediaFileTypes, ref mediaTypeIdToMediaFileTypeIdMap))
            {
                log.Error("CreateGroupFilesTypes failed");
                result.Add(eMigrationResultStatus.FailedValidationOfGroupFileTypes);
            }
            else
            {
                log.Debug("CreateGroupFilesTypes succeeded");
            }

            Dictionary<string, string> epgAssetStructTopics = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!CreateGroupTopicsMapping(group, ref groupTopics, ref epgAssetStructTopics))
            {
                log.Error("CreateGroupTopicsMapping failed");
                result.Add(eMigrationResultStatus.FailedValidationOfGroupTopics);
            }
            else
            {
                log.Debug("CreateGroupTopicsMapping succeeded");
            }

            if (!CreateGroupAssetStructsMapping(ref groupAssetStructs))
            {
                log.Error("CreateGroupAssetStructsMapping failed");
                result.Add(eMigrationResultStatus.FailedValidationOfGroupAssetStructs);
            }
            else
            {
                log.Debug("CreateGroupAssetStructsMapping succeeded");
            }

            if (!CreateAssetStructTopicsMapping(group.m_oMetasValuesByGroupId, ref groupAssetStructs, groupTopics, ref assetStructTopicsMap, epgAssetStructTopics))
            {
                log.Error("CreateAssetStructTopicsMapping failed");
                result.Add(eMigrationResultStatus.FailedValidationOfAssetStructToTopicsMapping);
            }
            else
            {
                log.Debug("CreateAssetStructTopicsMapping succeeded");
            }

            if (!CreateAssets(groupIds, ref assets, ref picIdToImageTypeNameMap, ref assetsImageTypesToAdd, ref picIdToUpdatedContentIdValue))
            {
                log.Error("CreateAssets failed");
                result.Add(eMigrationResultStatus.FailedValidationOfAssets);
            }
            else
            {
                log.Debug("CreateAssets succeeded");
            }

            if (!CreateGroupChannelsMapping(groupIds, group.GetLangauges(), ref groupChannels))
            {
                log.Error("CreateGroupChannelsMapping failed");
                result.Add(eMigrationResultStatus.FailedValidationOfChannels);
            }
            else
            {
                log.Debug("CreateGroupChannelsMapping succeeded");
            }

            if (result.Count == 0)
            {
                result.Add(eMigrationResultStatus.OK);
            }

            return result;
        }

        private HashSet<int> GetGroupLinearMediaTypeIds(List<int> groupIds)
        {
            HashSet<int> result = new HashSet<int>();
            try
            {
                DataTable dt = OPCMigrationDAL.GetGroupLinearMediaTypeIds(groupIds);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "id", 0);
                        if (id > 0 && !result.Contains(id))
                        {
                            result.Add(id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed GetGroupLinearMediaTypeIds", ex);
                throw ex;
            }

            return result;
        }

        private bool GetExtraLanguages(List<int> groupIds, ref HashSet<long> groupExtraLanguageIdsToSave)
        {
            log.DebugFormat("starting GetExtraLanguages");
            bool result = true;
            try
            {
                if (groupIds != null && groupIds.Count > 0)
                {
                    DataTable dt = OPCMigrationDAL.GetGroupExtraLanguagesForMigration(groupIds);
                    HashSet<long> languageIds = new HashSet<long>();
                    if (dt != null && dt.Rows != null)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "id");
                            long languageId = ODBCWrapper.Utils.GetLongSafeVal(dr, "language_id");
                            if (!languageIds.Contains(languageId))
                            {
                                languageIds.Add(languageId);
                                groupExtraLanguageIdsToSave.Add(id);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed ValidateExtraLanguages", ex);
                return false;
            }

            return result;
        }

        private bool CreateGroupRatios(List<int> groupIds, ref Dictionary<string, Core.Catalog.Ratio> groupRatios)
        {
            try
            {
                if (groupIds != null && groupIds.Count > 0)
                {
                    DataTable dt = OPCMigrationDAL.GetAllGroupsRatios(groupIds);
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            string ratioName = ODBCWrapper.Utils.GetSafeStr(dr, "ratio");
                            int height = ODBCWrapper.Utils.GetIntSafeVal(dr, "height");
                            int width = ODBCWrapper.Utils.GetIntSafeVal(dr, "width");
                            if (!string.IsNullOrEmpty(ratioName) && !groupRatios.ContainsKey(ratioName) && height > 0 && width > 0)
                            {
                                Core.Catalog.Ratio ratio = new Core.Catalog.Ratio() { Name = ratioName, Width = width, Height = height, PrecisionPrecentage = 0 };
                                groupRatios.Add(ratioName, ratio);
                            }
                            else
                            {
                                log.WarnFormat("found ratioName {0} with height {1} and width {2}", ratioName, height, width);
                            }
                        }
                    }
                }

                return groupRatios.Count > 0;
            }
            catch (Exception ex)
            {
                log.Error("Failed CreateGroupRatios", ex);
                return false;
            }
        }

        private bool CreateGroupImageTypes(Dictionary<string, Core.Catalog.Ratio> groupRatios, ref Dictionary<string, ImageType> groupImageTypes)
        {
            try
            {
                if (groupRatios != null && groupRatios.Count > 0)
                {
                    foreach (string ratioName in groupRatios.Keys)
                    {
                        if (!groupImageTypes.ContainsKey(ratioName))
                        {
                            ImageType imageType = new ImageType() { Name = ratioName, SystemName = ratioName };
                            groupImageTypes.Add(ratioName, imageType);
                        }
                    }
                }

                return groupImageTypes.Count > 0;
            }
            catch (Exception ex)
            {
                log.Error("Failed CreateGroupImageTypes", ex);
                return false;
            }
        }

        private Dictionary<long, int> CreateMediaFileTypesQualityMap(List<int> groupIds)
        {
            Dictionary<long, int> result = new Dictionary<long, int>();
            try
            {
                DataTable dt = OPCMigrationDAL.GetMediaFileTypeToQualityMap(groupIds);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long mediaFileTypeId = ODBCWrapper.Utils.GetLongSafeVal(dr, "media_file_type_id");
                        int qualityEnumId = ODBCWrapper.Utils.GetIntSafeVal(dr, "quality_enum_id");
                        if (!result.ContainsKey(mediaFileTypeId))
                        {
                            result.Add(mediaFileTypeId, qualityEnumId);
                        }
                    }
                }
                else
                {
                    log.Warn("Didn't find any mapping between fileType to quality");
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed CreateMediaFileTypesQualityMap", ex);
                return null;
            }

            return result;
        }

        private bool CreateGroupFilesTypes(List<int> groupIds, ref Dictionary<long, MediaFileType> groupMediaFileTypes, ref Dictionary<int, long> mediaTypeIdToMediaFileTypeIdMap)
        {
            try
            {
                if (groupIds != null && groupIds.Count > 0)
                {
                    Dictionary<long, int> mediaFileTypesQualityMap = CreateMediaFileTypesQualityMap(groupIds);
                    if (mediaFileTypesQualityMap == null)
                    {
                        log.Error("Failed CreateMediaFileTypesQualityMap while executing CreateGroupFilesTypes");
                        return false;
                    }

                    DataTable dt = OPCMigrationDAL.GetAllGroupsFileTypes(groupIds);
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "id");
                            int mediaTypeId = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_type_id");
                            if (!mediaTypeIdToMediaFileTypeIdMap.ContainsKey(mediaTypeId))
                            {
                                mediaTypeIdToMediaFileTypeIdMap.Add(mediaTypeId, id);
                            }

                            if (!groupMediaFileTypes.ContainsKey(id))
                            {
                                string fileTypeDescription = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION");
                                bool isActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_active") == 1;
                                bool isTrailer = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_trailer") == 1;
                                StreamerType streamerType = (StreamerType)Enum.Parse(typeof(StreamerType), ODBCWrapper.Utils.GetIntSafeVal(dr, "STREAMER_TYPE").ToString());
                                int drmId = ODBCWrapper.Utils.GetIntSafeVal(dr, "drm_id");

                                if (!string.IsNullOrEmpty(fileTypeDescription))
                                {
                                    MediaFileType mediaFileType = new MediaFileType()
                                    {
                                        Id = id,
                                        Description = fileTypeDescription,
                                        Name = fileTypeDescription,
                                        IsActive = isActive,
                                        IsTrailer = isTrailer,
                                        StreamerType = streamerType,
                                        DrmId = drmId,
                                    };

                                    if (mediaFileTypesQualityMap.ContainsKey(id))
                                    {
                                        mediaFileType.Quality = (MediaFileTypeQuality)Enum.Parse(typeof(MediaFileTypeQuality), mediaFileTypesQualityMap[id].ToString());
                                    }

                                    groupMediaFileTypes.Add(id, mediaFileType);
                                }
                            }
                        }
                    }
                }

                log.DebugFormat("groupMediaFileTypes count = {0}, mediaTypeIdToMediaFileTypeIdMap count = {1}", groupMediaFileTypes.Count, mediaTypeIdToMediaFileTypeIdMap.Count);
                return groupMediaFileTypes.Count > 0 && mediaTypeIdToMediaFileTypeIdMap.Count == groupMediaFileTypes.Count;
            }
            catch (Exception ex)
            {
                log.Error("Failed CreateGroupFilesTypes", ex);
                return false;
            }
        }

        private bool CreateGroupTopicsMapping(Group group, ref Dictionary<string, Dictionary<string, Topic>> groupTopics, ref Dictionary<string, string> epgAssetStructTopics)
        {
            bool result = true;
            try
            {
                // go over meta's for each group            
                if (!AddGroupMediaMetasAsTopics(group.m_oMetasValuesByGroupId, ref groupTopics))
                {
                    log.Warn("Failed when calling AddGroupMediaMetasAsTopics");
                    return false;
                }

                Dictionary<int, bool> tagIdToSearchRelatedMap = group.m_oGroupTags.Keys != null ? PopulateTagIdToSearchRelatedMap(group.m_oGroupTags.Keys.ToList()) : new Dictionary<int, bool>();
                if (!AddGroupMediaTagsAsTopics(group.m_oGroupTags, ref groupTopics, tagIdToSearchRelatedMap))
                {
                    log.Warn("Failed when calling AddGroupMediaTagsAsTopics");
                    result = false;
                }
                /* update free to Free due to DB tag type name not compatible with Group
                 * object tag type name (DEFAULT_GROUP_TAG_FREE on ChannelRepository.cs) */
                else if (groupTopics.ContainsKey(GROUP_TAG_FREE_CB_VALUE) &&
                        groupTopics[GROUP_TAG_FREE_CB_VALUE].ContainsKey(MetaType.Tag.ToString()))
                {
                    Topic updatedTopic = new Topic(groupTopics[GROUP_TAG_FREE_CB_VALUE][MetaType.Tag.ToString()]);
                    updatedTopic.SystemName = GROUP_TAG_FREE_DB_VALUE;
                    updatedTopic.Name = GROUP_TAG_FREE_DB_VALUE;
                    groupTopics[GROUP_TAG_FREE_CB_VALUE].Remove(MetaType.Tag.ToString());
                    groupTopics[GROUP_TAG_FREE_DB_VALUE].Add(MetaType.Tag.ToString(), updatedTopic);
                }

                if (group.m_oEpgGroupSettings != null)
                {
                    if (!AddGroupEpgMetasOrTagsAsTopics(ref epgAssetStructTopics, group.m_oEpgGroupSettings.MetasDisplayName, MetaType.MultilingualString, ref groupTopics))
                    {
                        log.Warn("Failed when calling AddGroupEpgMetasOrTagsAsTopics");
                        result = false;
                    }

                    if (!AddGroupEpgMetasOrTagsAsTopics(ref epgAssetStructTopics, group.m_oEpgGroupSettings.TagsDisplayName, MetaType.Tag, ref groupTopics))
                    {
                        log.Warn("Failed when calling AddGroupEpgMetasOrTagsAsTopics");
                        result = false;
                    }
                }

                Dictionary<string, string> notUsed = new Dictionary<string, string>();
                if (!AddBasicTopics(ref groupTopics, true, ref notUsed))
                {
                    log.Warn("Failed when calling AddBasicTopics");
                    result = false;
                }

                // Mark duplications
                foreach (Dictionary<string, Topic> pair in groupTopics.Values)
                {
                    if (pair.Count > 1)
                    {
                        foreach (Topic topic in pair.Values)
                        {
                            topic.Features.Add(TOPIC_DUPLICATE_FEATURE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed CreateGroupTopicsMapping", ex);
                return false;
            }

            return result;
        }

        private bool CreateGroupAssetStructsMapping(ref Dictionary<string, AssetStruct> groupAssetStructs)
        {
            bool result = true;
            try
            {
                Dictionary<int, string> mediaTypeIdToNameMap;
                Dictionary<string, int> mediaTypeNameToIdMap;
                Dictionary<int, int> parentMediaTypes;
                List<int> linearMediaTypes;
                CatalogDAL.GetMediaTypes(groupId, out mediaTypeIdToNameMap, out mediaTypeNameToIdMap, out parentMediaTypes, out linearMediaTypes);
                foreach (KeyValuePair<string, int> mediaType in mediaTypeNameToIdMap)
                {
                    if (groupAssetStructs.ContainsKey(mediaType.Key))
                    {
                        // THINK WHAT TO DO HERE OTHER THEN LOG
                        log.ErrorFormat("mediaType {0} already exists", mediaType.Key);
                        result = false;
                    }
                    else
                    {
                        long? parentId = null;
                        if (parentMediaTypes.ContainsKey(mediaType.Value) && parentMediaTypes[mediaType.Value] > 0)
                        {
                            parentId = parentMediaTypes[mediaType.Value];
                        }

                        AssetStruct assetStruct = new AssetStruct()
                        {
                            Id = mediaType.Value,
                            Name = mediaType.Key,
                            SystemName = mediaType.Key,
                            ParentId = parentId,
                            IsLinearAssetStruct = linearMediaTypeId == mediaType.Value
                        };

                        groupAssetStructs.Add(mediaType.Key, assetStruct);
                    }
                }

                // create ProgramAssetStruct if it doesn't exist
                if (!groupAssetStructs.ContainsKey(Utils.PROGRAM_ASSET_STRUCT) && !groupAssetStructs.Values.Any(x => x.IsProgramAssetStruct))
                {
                    AssetStruct assetStruct = new AssetStruct()
                    {
                        Id = 0,
                        Name = Utils.PROGRAM_ASSET_STRUCT,
                        SystemName = Utils.PROGRAM_ASSET_STRUCT,
                        ParentId = null,
                        IsLinearAssetStruct = false
                    };

                    groupAssetStructs.Add(Utils.PROGRAM_ASSET_STRUCT, assetStruct);
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed CreateGroupAssetStructsMapping", ex);
                return false;
            }

            return result;
        }

        private bool CreateAssetStructTopicsMapping(Dictionary<int, Dictionary<string, string>> metasByGroupId, ref Dictionary<string, AssetStruct> groupAssetStructs, Dictionary<string, Dictionary<string, Topic>> groupTopics,
                                                    ref Dictionary<long, Dictionary<string, string>> assetStructTopicsMap, Dictionary<string, string> epgAssetStructTopics)
        {
            try
            {
                Dictionary<long, int> assetStructToGroupId = GetMediaTypesIdToGroupIdMap();
                List<Topic> basicMediaTopics = new List<Topic>();
                foreach (Dictionary<string, Topic> pair in groupTopics.Values)
                {
                    if (pair.Values.Any(x => x.IsPredefined.HasValue && x.IsPredefined.Value))
                    {
                        basicMediaTopics.AddRange(pair.Values.Where(x => x.IsPredefined.HasValue && x.IsPredefined.Value));
                    }
                }

                if (basicMediaTopics != null && basicMediaTopics.Count > 0 && assetStructToGroupId != null && assetStructToGroupId.Count > 0)
                {
                    foreach (AssetStruct assetStruct in groupAssetStructs.Values.Where(x => !x.IsProgramAssetStruct))
                    {
                        if (assetStructToGroupId.ContainsKey(assetStruct.Id) && metasByGroupId.ContainsKey(assetStructToGroupId[assetStruct.Id]))
                        {
                            //bool shouldIncludeBasicDateTopics = assetStructToGroupId[assetStruct.Id] == regularGroupId;
                            // According to Ruby Schechter (product of OPC) request we will always create also the basic date topics, previously we didn't (see line above)
                            // This was done per ticket https://kaltura.atlassian.net/browse/GEN-1721 - good luck to us all...
                            Dictionary<string, string> assetStructTopicsSystemNameToType = GetTopicsByMediaTypeId(basicMediaTopics, true, metasByGroupId[assetStructToGroupId[assetStruct.Id]], assetStruct.Id);
                            if (assetStructTopicsSystemNameToType.Count > 0 && !assetStructTopicsMap.ContainsKey(assetStruct.Id))
                            {
                                assetStructTopicsMap.Add(assetStruct.Id, assetStructTopicsSystemNameToType);
                            }
                        }
                    }
                }
                else
                {
                    log.Error("no basic media topics or media types found for the account");
                    return false;
                }


                Dictionary<string, string> programAssetStructTopics = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (!AddBasicTopics(ref groupTopics, false, ref programAssetStructTopics))
                {
                    log.Error("failed AddBasicTopics");
                    return false;
                }


                Dictionary<string, string> ProgramTopics = groupTopics.Where(x => epgAssetStructTopics.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => epgAssetStructTopics[x.Key]);
                if (ProgramTopics != null && ProgramTopics.Count > 0)
                {
                    foreach (KeyValuePair<string, string> topicSystemNameAndType in ProgramTopics)
                    {
                        if (!programAssetStructTopics.ContainsKey(topicSystemNameAndType.Key))
                        {
                            programAssetStructTopics.Add(topicSystemNameAndType.Key, topicSystemNameAndType.Value);
                        }
                        else if (programAssetStructTopics[topicSystemNameAndType.Key] != topicSystemNameAndType.Value)
                        {
                            log.WarnFormat("Program asset struct already contains topic {0} with different type, current is {1} and the other is {2}",
                                            topicSystemNameAndType.Key, programAssetStructTopics[topicSystemNameAndType.Key], topicSystemNameAndType.Value);
                        }
                    }
                }

                if (programAssetStructTopics != null && programAssetStructTopics.Count > 0)
                {
                    assetStructTopicsMap.Add(programMediaTypeId, programAssetStructTopics);
                }
                else
                {
                    log.Error("no topics found for program asset struct");
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed CreateAssetStructTopicsMapping", ex);
                return false;
            }

            return true;
        }

        private bool CreateAssets(List<int> groupIds, ref List<MediaAsset> assets, ref Dictionary<long, string> picIdToImageTypeNameMap,
                                    ref Dictionary<long, Dictionary<string, string>> assetsImageTypesToAdd, ref Dictionary<long, string> picIdToUpdatedContentIdValue)
        {
            try
            {
                bool gotExistingGroupAssets = false;
                ConcurrentBag<Core.Catalog.Response.MediaObj> existingAssets = GetExistingGroupsAssets(groupIds, ref gotExistingGroupAssets);
                if (!gotExistingGroupAssets)
                {
                    log.Error("failed GetExistingGroupsAssets");
                    return false;
                }
                else if (existingAssets.Count == 0)
                {
                    return true;
                }
                else
                {
                    log.DebugFormat("Finished getting group existing assets, total of {0} assets found starting to validate them", existingAssets.Count);
                    ConcurrentDictionary<string, string> picContentIdToImageTypeNameMap = new ConcurrentDictionary<string, string>();
                    ConcurrentDictionary<string, string> picContentIdToUpdatedContentId = new ConcurrentDictionary<string, string>();
                    ConcurrentDictionary<string, string> mediaIdToEpgChannelMap = new ConcurrentDictionary<string, string>();
                    ConcurrentDictionary<string, string> epgChannelToMediaIdMap = new ConcurrentDictionary<string, string>();
                    ConcurrentDictionary<string, bool> coGuids = new ConcurrentDictionary<string, bool>();
                    ApiObjects.TimeShiftedTv.TimeShiftedTvPartnerSettings accountTstvSettings = Core.ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
                    ConcurrentBag<MediaAsset> opcMediaAssets = new ConcurrentBag<MediaAsset>();
                    ConcurrentDictionary<long, ConcurrentDictionary<string, string>> opcAssetsImageTypesToAdd = new ConcurrentDictionary<long, ConcurrentDictionary<string, string>>();
                    bool validateAssetsResult = true;
                    Parallel.ForEach(existingAssets, (mediaObj) =>
                    {
                        if (mediaObj == null)
                        {
                            return;
                        }

                        if (string.IsNullOrEmpty(mediaObj.m_sName))
                        {
                            log.WarnFormat("media with id {0} doesn't have a name, name will be set to its id", mediaObj.AssetId);
                            mediaObj.m_sName = mediaObj.AssetId;
                            mediaObj.Name = null;
                        }

                        if (string.IsNullOrEmpty(mediaObj.CoGuid))
                        {
                            string newCoGuid = string.Format("{0}_{1}", mediaObj.AssetId, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
                            log.WarnFormat("media with id {0} doesn't have a CoGuid, CoGuid will be set to {1}", mediaObj.AssetId, newCoGuid);
                            mediaObj.CoGuid = newCoGuid;
                        }

                        if (coGuids.ContainsKey(mediaObj.CoGuid))
                        {
                            log.ErrorFormat("media with CoGuid {0} already exists, please remove duplicate coGuid", mediaObj.CoGuid);
                            validateAssetsResult = validateAssetsResult && false;
                            return;
                        }
                        else
                        {
                            coGuids.TryAdd(mediaObj.CoGuid, true);
                        }

                        MediaAsset mediaAsset = new MediaAsset(regularGroupId, mediaObj);
                        mediaAsset.MediaAssetType = linearMediaTypeIds.Contains(mediaObj.m_oMediaType.m_nTypeID) ? MediaAssetType.Linear : MediaAssetType.Media;

                        if (mediaAsset.MediaAssetType == MediaAssetType.Linear)
                        {
                            int epg_channel_id = 0;
                            if (!string.IsNullOrEmpty(mediaObj.m_ExternalIDs) && int.TryParse(mediaObj.m_ExternalIDs, out epg_channel_id) && epg_channel_id > 0)
                            {
                                if (!epgChannelToMediaIdMap.ContainsKey(mediaObj.m_ExternalIDs) && !mediaIdToEpgChannelMap.ContainsKey(mediaObj.AssetId))
                                {
                                    epgChannelToMediaIdMap.TryAdd(mediaObj.m_ExternalIDs, mediaObj.AssetId);
                                    mediaIdToEpgChannelMap.TryAdd(mediaObj.AssetId, mediaObj.m_ExternalIDs);
                                    LiveAsset liveAsset = new LiveAsset(mediaAsset);
                                    long epgChannelId = 0;
                                    if (long.TryParse(mediaObj.m_ExternalIDs, out epgChannelId) && epgChannelId > 0)
                                    {
                                        liveAsset.EpgChannelId = epgChannelId;
                                    }

                                    opcMediaAssets.Add(liveAsset);
                                }
                                else
                                {
                                    log.ErrorFormat("epg channel {0} is connected to more than one media", mediaObj.m_ExternalIDs);
                                    validateAssetsResult = validateAssetsResult && false;
                                    return;
                                }
                            }
                            else
                            {
                                log.ErrorFormat("media with id {0} is defined as linear but the connected epg_channel isn't defined correctly on the table", mediaObj.AssetId);
                                validateAssetsResult = validateAssetsResult && false;
                                return;
                            }
                        }
                        else
                        {
                            opcMediaAssets.Add(mediaAsset);
                        }

                        if (mediaObj.m_lPicture != null && mediaObj.m_lPicture.Count > 0)
                        {
                            long assetId = long.Parse(mediaObj.AssetId);
                            // we save this only so we will have the first image on a migrated image server ratioId
                            foreach (Core.Catalog.Picture pic in mediaObj.m_lPicture)
                            {
                                // we are not adding default images, only at the end we add the PARENT default images (integrator should configure whats needed to parent only)
                                if (!pic.isDefault)
                                {
                                    string[] picId = pic.id.Split('_');
                                    if (picId != null && picId.Length == 2 && !string.IsNullOrEmpty(pic.ratio))
                                    {
                                        string key = picId[0];
                                        if (!picContentIdToImageTypeNameMap.ContainsKey(key))
                                        {
                                            picContentIdToImageTypeNameMap.TryAdd(key, pic.ratio);
                                            picContentIdToUpdatedContentId.TryAdd(key, pic.id);
                                        }
                                        /* support account that used image server migration (ratio_id=0 on pics table) but making sure
                                         * it isn't just another pic size because the ratio id will be different in this case unlike 
                                         * pic sizes where the ratio id is the same */
                                        else if (picContentIdToImageTypeNameMap[key] != pic.ratio)
                                        {
                                            // only do this once and not foreach pic
                                            if (!opcAssetsImageTypesToAdd.ContainsKey(assetId))
                                            {
                                                opcAssetsImageTypesToAdd.TryAdd(assetId, new ConcurrentDictionary<string, string>());
                                            }

                                            if (!opcAssetsImageTypesToAdd[assetId].ContainsKey(pic.ratio))
                                            {
                                                opcAssetsImageTypesToAdd[assetId].TryAdd(pic.ratio, pic.id);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        log.ErrorFormat("picture for media {0} is null or data returned incorrectly", mediaObj.AssetId);
                                        validateAssetsResult = validateAssetsResult && false;
                                        return;
                                    }
                                }
                            }
                        }
                    });

                    if (!validateAssetsResult)
                    {
                        return false;
                    }

                    assets = opcMediaAssets.ToList();
                    assetsImageTypesToAdd = opcAssetsImageTypesToAdd.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Key, y => y.Value));

                    if (mediaIdToEpgChannelMap != null && mediaIdToEpgChannelMap.Count > 0)
                    {
                        DataTable dt = OPCMigrationDAL.GetAllEpgChannelsOfMediaIds(groupIds, mediaIdToEpgChannelMap.Keys);
                        if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
                        {
                            log.ErrorFormat("epg channels doesn't have mediaIds defined on them");
                            return false;
                        }

                        mediaIdToEpgChannelMap.Clear();
                        bool epgChannelsTableIsValid = true;
                        foreach (DataRow dr in dt.Rows)
                        {
                            string mediaId = ODBCWrapper.Utils.GetSafeStr(dr, "MEDIA_ID");
                            string epgChannelId = ODBCWrapper.Utils.GetSafeStr(dr, "ID");
                            if (!string.IsNullOrEmpty(epgChannelId) && epgChannelToMediaIdMap.ContainsKey(epgChannelId))
                            {
                                if (string.IsNullOrEmpty(mediaId))
                                {
                                    log.ErrorFormat("epg channel {0} has no media Id connected to it but is supposed to be connected to mediaId {1}", epgChannelId, epgChannelToMediaIdMap[epgChannelId]);
                                    epgChannelsTableIsValid = false;
                                }
                                else
                                {
                                    if (epgChannelToMediaIdMap[epgChannelId] != mediaId)
                                    {
                                        log.ErrorFormat("epg channel {0} is connected to mediaId {1} but is supposed to be connected to mediaId {2}", epgChannelId, mediaId, epgChannelToMediaIdMap[epgChannelId]);
                                        epgChannelsTableIsValid = false;
                                    }

                                    if (mediaIdToEpgChannelMap.ContainsKey(mediaId))
                                    {
                                        log.ErrorFormat("media with id {0} is already defined for epg channel {1} but is also connected to epg channel {2}", mediaId, mediaIdToEpgChannelMap[mediaId], epgChannelId);
                                        epgChannelsTableIsValid = false;
                                    }

                                    mediaIdToEpgChannelMap[mediaId] = epgChannelId;
                                }
                            }
                        }

                        if (!epgChannelsTableIsValid)
                        {
                            return false;
                        }
                    }

                    if (picContentIdToImageTypeNameMap != null && picContentIdToImageTypeNameMap.Count > 0)
                    {
                        picIdToImageTypeNameMap = CreatePicIdToImageTypeNameMap(picContentIdToImageTypeNameMap);
                        if (picIdToImageTypeNameMap == null)
                        {
                            return false;
                        }
                    }

                    if (picContentIdToUpdatedContentId != null && picContentIdToUpdatedContentId.Count > 0)
                    {
                        picIdToUpdatedContentIdValue = CreatePicIdToImageTypeNameMap(picContentIdToUpdatedContentId);
                        if (picIdToUpdatedContentIdValue == null)
                        {
                            return false;
                        }
                    }

                    return AddDefaultImages(groupIds, ref picIdToImageTypeNameMap);
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed CreateAssets", ex);
                return false;
            }
        }

        private ConcurrentBag<Core.Catalog.Response.MediaObj> GetExistingGroupsAssets(List<int> groupIds, ref bool gotExistingGroupAssets)
        {
            ConcurrentBag<Core.Catalog.Response.MediaObj> existingAssets = new ConcurrentBag<Core.Catalog.Response.MediaObj>();
            int assetPageSize = Utils.GetAssetsPageSize();
            try
            {
                string signString = Guid.NewGuid().ToString();
                Dictionary<int, List<long>> pageToMediaIds = new Dictionary<int, List<long>>();
                DataTable dt = OPCMigrationDAL.GetAllGroupMediaIds(groupIds);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    int pageIndex = 0;
                    int pageSize = assetPageSize;
                    pageToMediaIds.Add(pageIndex, new List<long>());
                    foreach (DataRow dr in dt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                        if (id > 0)
                        {
                            if (pageSize == 0)
                            {
                                pageIndex++;
                                pageToMediaIds.Add(pageIndex, new List<long>());
                                pageSize = assetPageSize;
                            }

                            pageToMediaIds[pageIndex].Add(id);
                            pageSize--;
                        }
                    }

                    foreach (KeyValuePair<int, List<long>> pageIndexToIds in pageToMediaIds)
                    {
                        Core.Catalog.Request.AssetInfoRequest assetsRequest = new Core.Catalog.Request.AssetInfoRequest()
                        {
                            epgIds = new List<long>(),
                            mediaIds = pageIndexToIds.Value,
                            m_nGroupID = groupId,
                            m_nPageIndex = pageIndexToIds.Key,
                            m_nPageSize = assetPageSize,
                            m_oFilter = new Core.Catalog.Filter()
                            {
                                m_nLanguage = 0,
                                m_bUseStartDate = false,
                                m_bOnlyActiveMedia = false
                            },
                            m_sSignature = Utils.GetSignature(signString),
                            m_sSignString = signString,
                            ManagementData = true
                        };

                        Core.Catalog.Response.AssetInfoResponse response = null;
                        try
                        {
                            Core.Catalog.Response.BaseResponse baseResponse = assetsRequest.GetResponse(assetsRequest);
                            if (baseResponse != null && baseResponse is Core.Catalog.Response.AssetInfoResponse)
                            {
                                response = baseResponse as Core.Catalog.Response.AssetInfoResponse;
                            }

                            if (response == null || response.mediaList == null)
                            {
                                log.ErrorFormat("AssetInfoRequest didn't return any medias, expected mediaIds to be returned: {0}", string.Join(",", pageIndexToIds.Value));
                            }
                            else
                            {
                                if (response.mediaList.Count != pageIndexToIds.Value.Count)
                                {
                                    var missingMediaIds = pageIndexToIds.Value.Except(response.mediaList.Select(x => long.Parse(x.AssetId)).ToList());
                                    log.ErrorFormat("AssetInfoRequest didn't return these mediaIds: {0}, need to check if they are needed for migration", string.Join(",", missingMediaIds));
                                }

                                foreach (Core.Catalog.Response.MediaObj mediaToAdd in response.mediaList)
                                {
                                    existingAssets.Add(mediaToAdd);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error(string.Format("Failed calling AssetInfoRequest as part of GetExistingGroupsAssetsWithParallel for mediaIds: {0}, pageIndex {1}",
                                                    string.Join(",", pageIndexToIds.Value), pageIndexToIds.Key), ex);
                        }
                    }
                }
                else
                {
                    log.Warn("No media assets found for the account");
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed GetExistingGroupsAssetsWithParallel", ex);
                gotExistingGroupAssets = false;
            }

            gotExistingGroupAssets = true;
            return existingAssets;
        }

        private Dictionary<int, bool> PopulateTagIdToSearchRelatedMap(List<int> tagTypeIds)
        {
            Dictionary<int, bool> result = new Dictionary<int, bool>();
            try
            {
                if (tagTypeIds != null && tagTypeIds.Count > 0)
                {
                    DataTable dt = OPCMigrationDAL.GetTagTypesSearchRelatedInfo(tagTypeIds);
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID", 0);
                            // tag free has id = 0 so check > -1..
                            if (id > -1 && !result.ContainsKey(id))
                            {
                                bool isSearchRelated = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_RELATED", 0) == 1;
                                result.Add(id, isSearchRelated);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed PopulateTagIdToSearchRelatedMap", ex);
            }

            return result;
        }

        private bool AddGroupMediaMetasAsTopics(Dictionary<int, Dictionary<string, string>> metasValuesByGroupId, ref Dictionary<string, Dictionary<string, Topic>> groupTopics)
        {
            bool result = true;
            try
            {
                if (metasValuesByGroupId != null)
                {
                    foreach (KeyValuePair<int, Dictionary<string, string>> pair in metasValuesByGroupId)
                    {
                        DataTable dt = OPCMigrationDAL.GetGroupMetasSearchRelatedInfo(pair.Key);
                        if (dt != null && dt.Rows.Count == 1 && dt.Rows[0] != null)
                        {
                            DataRow groupDr = dt.Rows[0];
                            if (pair.Value != null && pair.Value.Count > 0)
                            {
                                Dictionary<string, string> metas = pair.Value;
                                foreach (KeyValuePair<string, string> meta in metas)
                                {
                                    MetaType metaType = Utils.GetTopicType(meta.Key);
                                    if (metaType != MetaType.All)
                                    {
                                        Topic topic = new Topic()
                                        {
                                            Type = metaType,
                                            SystemName = meta.Value,
                                            Name = meta.Value,
                                            IsPredefined = false,
                                            SearchRelated = metaType != MetaType.DateTime ? Utils.CheckIfMetaIsSearchRelated(meta.Key, groupDr) : false,
                                            Features = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { TOPIC_METADATA_FEATURE }
                                        };

                                        if (topic.SearchRelated)
                                        {
                                            topic.Features.Add(TOPIC_SEARCH_RELATED_FEATURE);
                                        }

                                        if (groupTopics.ContainsKey(meta.Value) && !groupTopics[meta.Value].ContainsKey(metaType.ToString()))
                                        {
                                            log.WarnFormat("meta {0} appears twice, once as type {1} and once as type {2}", meta.Value, metaType.ToString(), groupTopics[meta.Value].Keys.First());
                                        }
                                        else if (!groupTopics.ContainsKey(meta.Value))
                                        {
                                            groupTopics.Add(meta.Value, new Dictionary<string, Topic>(StringComparer.OrdinalIgnoreCase));
                                        }

                                        if (!groupTopics[meta.Value].ContainsKey(metaType.ToString()))
                                        {
                                            groupTopics[meta.Value].Add(metaType.ToString(), topic);
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
                log.Error("Failed AddGroupMediaMetasAsTopics", ex);
                return false;
            }

            return result;
        }

        private bool AddGroupMediaTagsAsTopics(Dictionary<int, string> groupMediaTags, ref Dictionary<string, Dictionary<string, Topic>> groupTopics, Dictionary<int, bool> tagIdToSearchRelatedMap)
        {
            try
            {
                if (groupMediaTags != null)
                {
                    MetaType metaType = MetaType.Tag;
                    foreach (KeyValuePair<int, string> tag in groupMediaTags)
                    {
                        // tag free has id = 0 so check > -1...
                        if (tag.Key > -1 && !string.IsNullOrEmpty(tag.Value))
                        {
                            Topic topic = new Topic()
                            {
                                Type = metaType,
                                SystemName = tag.Value,
                                Name = tag.Value,
                                IsPredefined = false,
                                SearchRelated = tagIdToSearchRelatedMap.ContainsKey(tag.Key) ? tagIdToSearchRelatedMap[tag.Key] : false,
                                Features = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { TOPIC_METADATA_FEATURE }
                            };

                            if (topic.SearchRelated)
                            {
                                topic.Features.Add(TOPIC_SEARCH_RELATED_FEATURE);
                            }

                            if (groupTopics.ContainsKey(tag.Value) && !groupTopics[tag.Value].ContainsKey(metaType.ToString()))
                            {
                                log.WarnFormat("tag {0} appears twice, once as type {1} and once as tag", tag.Value, groupTopics[tag.Value].Keys.First());
                            }
                            else if (!groupTopics.ContainsKey(tag.Value))
                            {
                                groupTopics.Add(tag.Value, new Dictionary<string, Topic>(StringComparer.OrdinalIgnoreCase));
                            }

                            if (!groupTopics[tag.Value].ContainsKey(metaType.ToString()))
                            {
                                groupTopics[tag.Value].Add(metaType.ToString(), topic);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed AddGroupMediaTagsAsTopics", ex);
                return false;
            }

            return true;
        }

        private bool AddGroupEpgMetasOrTagsAsTopics(ref Dictionary<string, string> epgAssetStructTopics, List<string> metasOrTags, MetaType metaType, ref Dictionary<string, Dictionary<string, Topic>> groupTopics)
        {
            bool res = true;
            try
            {
                if (metasOrTags != null)
                {
                    foreach (string meta in metasOrTags)
                    {
                        if (metaType != MetaType.All)
                        {
                            Topic topic = new Topic()
                            {
                                Type = metaType,
                                SystemName = meta,
                                Name = meta,
                                IsPredefined = false,
                                SearchRelated = false,
                                Features = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { TOPIC_METADATA_FEATURE }
                            };

                            if (groupTopics.ContainsKey(meta) && !groupTopics[meta].ContainsKey(metaType.ToString()))
                            {
                                log.WarnFormat("EPG meta or Tag {0} appears twice, once as type {1} and once as type {2}", meta, metaType.ToString(), groupTopics[meta].Keys.First());
                            }
                            else if (!groupTopics.ContainsKey(meta))
                            {
                                groupTopics.Add(meta, new Dictionary<string, Topic>(StringComparer.OrdinalIgnoreCase));
                            }

                            if (!groupTopics[meta].ContainsKey(metaType.ToString()))
                            {
                                groupTopics[meta].Add(metaType.ToString(), topic);
                            }

                            if (!epgAssetStructTopics.ContainsKey(meta))
                            {
                                epgAssetStructTopics.Add(meta, metaType.ToString());
                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed AddGroupEpgMetasOrTagsAsTopics", ex);
                return false;
            }

            return res;
        }

        private bool AddBasicTopics(ref Dictionary<string, Dictionary<string, Topic>> groupTopics, bool isMedia, ref Dictionary<string, string> epgBasicTopics)
        {
            bool result = true;
            try
            {
                List<Topic> basicTopics = isMedia ? AssetManager.GetBasicMediaAssetTopics() : EpgAssetManager.GetBasicTopics();
                foreach (Topic topicToAdd in basicTopics)
                {
                    if (topicToAdd.Type != MetaType.All)
                    {
                        if (groupTopics.ContainsKey(topicToAdd.SystemName) && !groupTopics[topicToAdd.SystemName].ContainsKey(topicToAdd.Type.ToString()))
                        {
                            // THINK WHAT TO DO HERE OTHER THEN LOG
                            log.WarnFormat("BASIC meta {0} appears twice, once as type {1} and once as type {2}",
                                            topicToAdd.SystemName, topicToAdd.Type, groupTopics[topicToAdd.SystemName].Keys.First());
                        }

                        if (!groupTopics.ContainsKey(topicToAdd.SystemName))
                        {
                            groupTopics.Add(topicToAdd.SystemName, new Dictionary<string, Topic>(StringComparer.OrdinalIgnoreCase));
                        }

                        groupTopics[topicToAdd.SystemName][topicToAdd.Type.ToString()] = topicToAdd;

                        if (!isMedia && !epgBasicTopics.ContainsKey(topicToAdd.SystemName))
                        {
                            epgBasicTopics.Add(topicToAdd.SystemName, topicToAdd.Type.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed AddBasicTopics", ex);
                return false;
            }

            return result;
        }

        private Dictionary<long, int> GetMediaTypesIdToGroupIdMap()
        {
            Dictionary<long, int> result = new Dictionary<long, int>();
            try
            {
                DataTable mediaTypes = CatalogDAL.GetMediaTypesTable(groupId);

                if (mediaTypes != null)
                {
                    foreach (DataRow dr in mediaTypes.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                        int mediaTypeGroupId = ODBCWrapper.Utils.GetIntSafeVal(dr, "group_id");
                        if (id > 0 && mediaTypeGroupId > 0 && !result.ContainsKey(id))
                        {
                            result.Add(id, mediaTypeGroupId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed GetMediaTypesIdToGroupIdMap", ex);
            }

            return result;
        }

        private Dictionary<int, List<int>> GetGroupIdToMediaTypeIdsMap()
        {
            Dictionary<int, List<int>> result = new Dictionary<int, List<int>>();
            try
            {
                DataTable mediaTypes = CatalogDAL.GetMediaTypesTable(groupId);

                if (mediaTypes != null)
                {
                    foreach (DataRow dr in mediaTypes.Rows)
                    {
                        int mediaTypeId = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                        int groupId = ODBCWrapper.Utils.GetIntSafeVal(dr, "group_id");
                        if (mediaTypeId > 0 && groupId > 0)
                        {
                            if (!result.ContainsKey(groupId))
                            {
                                result.Add(groupId, new List<int>());
                            }

                            result[groupId].Add(mediaTypeId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed GetGroupIdToMediaTypeIdsMap", ex);
            }

            return result;
        }

        private Dictionary<string, string> GetTopicsByMediaTypeId(List<Topic> basicTopics, bool shouldIncludeBasicDateTopics, Dictionary<string, string> metasMapping, long mediaTypeId)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (Topic topic in basicTopics)
                {
                    if (!result.ContainsKey(topic.SystemName) && (topic.Type != MetaType.DateTime || shouldIncludeBasicDateTopics))
                    {
                        result.Add(topic.SystemName, topic.Type.ToString());
                    }
                }

                DataSet ds = OPCMigrationDAL.GetMediaTypeMetasAndTags(mediaTypeId);
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable metas = ds.Tables[0];
                    DataRow metasRow = metas.Rows != null && metas.Rows.Count == 1 ? metas.Rows[0] : null;
                    DataTable tags = ds.Tables.Count > 1 ? ds.Tables[1] : null;
                    DataTable dateMetas = ds.Tables.Count > 2 ? ds.Tables[2] : null;

                    if (metasMapping != null && metasMapping.Count > 0 && metasRow != null)
                    {
                        foreach (DataColumn dc in metas.Columns)
                        {
                            string metaName = dc.ColumnName.ToUpper();
                            int numOfUsingAssets = ODBCWrapper.Utils.GetIntSafeVal(metasRow, metaName);
                            if (numOfUsingAssets > 0 && metasMapping.ContainsKey(metaName) && !result.ContainsKey(metasMapping[metaName]))
                            {
                                MetaType metaType = Utils.GetTopicType(metaName);
                                result.Add(metasMapping[metaName], metaType.ToString());
                            }
                            else if (numOfUsingAssets > 0 && !metasMapping.ContainsKey(metaName))
                            {
                                log.WarnFormat("meta {0} does not exist on metasMapping for mediaTypeID {1}", metaName, mediaTypeId);
                            }
                        }
                    }

                    if (dateMetas != null && dateMetas.Rows != null && dateMetas.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dateMetas.Rows)
                        {
                            string metaName = string.Format("date_{0}", ODBCWrapper.Utils.GetSafeStr(dr, "date_meta_id"));
                            if (metasMapping.ContainsKey(metaName) && !result.ContainsKey(metasMapping[metaName]))
                            {
                                result.Add(metasMapping[metaName], MetaType.DateTime.ToString());
                            }
                        }
                    }

                    if (tags != null && tags.Rows != null && tags.Rows.Count > 0)
                    {
                        foreach (DataRow dr in tags.Rows)
                        {
                            string topicName = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                            if (!string.IsNullOrEmpty(topicName))
                            {
                                if (!result.ContainsKey(topicName))
                                {
                                    result.Add(topicName, MetaType.Tag.ToString());
                                }
                                else
                                {
                                    Console.WriteLine("Duplicate found for meta : {0} on assetStruct with id: {1}, what to use?: \n 1.Meta\n 2.Tag\n", topicName, mediaTypeId);
                                    string typeToUse = Console.ReadLine();
                                    int response;
                                    if (int.TryParse(typeToUse, out response))
                                    {
                                        if (response == 2)
                                        {
                                            result[topicName] = MetaType.Tag.ToString();
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
                log.Error("Failed GetTopicsByMediaTypeId", ex);
            }

            return result;
        }

        private Dictionary<long, string> CreatePicIdToImageTypeNameMap(ConcurrentDictionary<string, string> picContentIdToImageTypeNameMap)
        {
            Dictionary<long, string> result = new Dictionary<long, string>();
            try
            {
                List<string> baseUrls = picContentIdToImageTypeNameMap.Keys.ToList();
                int bulkSize = 500;
                bool getMore = true;
                while (getMore)
                {
                    int amountToTake = Math.Min(bulkSize, baseUrls.Count);
                    DataTable dt = OPCMigrationDAL.GetPicIdsFromPicContentIds(baseUrls.Take(amountToTake).ToList());
                    if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
                    {
                        result = null;
                        getMore = false;
                    }
                    else
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                            string contentId = ODBCWrapper.Utils.GetSafeStr(dr, "BASE_URL");
                            if (!string.IsNullOrEmpty(contentId) && picContentIdToImageTypeNameMap.ContainsKey(contentId)
                                && id > 0 && !result.ContainsKey(id))
                            {
                                result.Add(id, picContentIdToImageTypeNameMap[contentId]);
                            }
                        }

                        baseUrls.RemoveRange(0, amountToTake);
                        if (baseUrls.Count == 0)
                        {
                            getMore = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed CreatePicIdToImageTypeNameMap", ex);
                return null;
            }

            return result;
        }

        private bool CreateGroupChannelsMapping(List<int> groupIds, List<LanguageObj> groupLanguages, ref List<Channel> groupChannels)
        {
            bool result = true;
            try
            {
                HashSet<int> channelIds = GetGroupChannelIds(groupIds);
                if (channelIds != null && channelIds.Count > 0)
                {
                    Dictionary<int, List<LanguageContainer>> channelsNameTranslationsMapping = new Dictionary<int, List<LanguageContainer>>();
                    Dictionary<int, List<LanguageContainer>> channelsDescriptionTranslationsMapping = new Dictionary<int, List<LanguageContainer>>();
                    if (!PopulateChannelsTranslationsMapping(groupIds, channelIds, groupLanguages, ref channelsNameTranslationsMapping, ref channelsDescriptionTranslationsMapping))
                    {
                        log.Error("Failed to populate channel translations");
                        result = false;
                        return result;
                    }

                    Dictionary<int, List<int>> groupIdToMediaTypeIdsMap = GetGroupIdToMediaTypeIdsMap();
                    GroupManager groupManager = new GroupManager();
                    List<Channel> channels = groupManager.GetChannels(channelIds.ToList(), groupId, true);
                    log.DebugFormat("Finished getting group existing channels, total of {0} channels found starting to validate them", channels.Count);
                    if (channels == null || channels.Count == 0)
                    {
                        log.Warn("Failed getting channels from groupManager.GetChannels");

                    }

                    ConcurrentDictionary<string, Channel> channelNamesToChannelMap = new ConcurrentDictionary<string, Channel>();
                    Parallel.ForEach(channels, (channel) =>
                    {
                        if (channel != null && channel.m_nChannelID > 0)
                        {
                            // Automatic channel needs to be changed to KSQL channel
                            if (channel.m_nChannelTypeID == (int)ChannelType.Automatic)
                            {
                                StringBuilder sb = new StringBuilder();
                                bool hasMediaTypes = true;
                                if (channel.m_nMediaType == null || channel.m_nMediaType.Count == 0)
                                {
                                    // if the channel does not have any filter we add it's "group" media types because we merged to a single group id
                                    if (groupIdToMediaTypeIdsMap.ContainsKey(channel.m_nGroupID))
                                    {
                                        channel.m_nMediaType = new List<int>(groupIdToMediaTypeIdsMap[channel.m_nGroupID]);
                                    }
                                    else
                                    {
                                        sb = new StringBuilder(string.Format("({0} asset_type='media' ", ApiObjects.SearchObjects.CutWith.AND.ToString().ToLower()));
                                        hasMediaTypes = false;
                                    }
                                }

                                if (channel.m_lChannelTags != null && channel.m_lChannelTags.Count > 0)
                                {
                                    sb.AppendFormat("({0} ", channel.m_eCutWith.ToString().ToLower());
                                    var hs = new HashSet<string>();
                                    foreach (ApiObjects.SearchObjects.SearchValue searchValue in channel.m_lChannelTags)
                                    {
                                        if (!string.IsNullOrEmpty(searchValue.m_sKey) && searchValue.m_lValue != null && searchValue.m_lValue.Count > 0)
                                        {
                                            // added inner cut with to the ksql query
                                            sb.AppendFormat("({0} ", searchValue.m_eInnerCutWith.ToString().ToLower());
                                            foreach (string value in searchValue.m_lValue)
                                            {
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    string key = $"K{searchValue.m_sKey}_V{value}";
                                                    if (!hs.Contains(key))
                                                    {
                                                        sb.AppendFormat("{0}='{1}' ", searchValue.m_sKey, value);
                                                        hs.Add(key);
                                                    }
                                                }
                                            }

                                            sb.Append(")");
                                        }
                                    }

                                    sb.Append(")");
                                }

                                if (!hasMediaTypes)
                                {
                                    sb.Append(" )");
                                }

                                channel.filterQuery = sb.ToString();
                                channel.m_nChannelTypeID = (int)ChannelType.KSQL;
                            }

                            channel.SystemName = channel.m_sName;
                            if (channelsNameTranslationsMapping.ContainsKey(channel.m_nChannelID))
                            {
                                channel.NamesInOtherLanguages = channelsNameTranslationsMapping[channel.m_nChannelID];
                            }

                            if (channelsDescriptionTranslationsMapping.ContainsKey(channel.m_nChannelID))
                            {
                                channel.DescriptionInOtherLanguages = channelsDescriptionTranslationsMapping[channel.m_nChannelID];
                            }

                            if (channelNamesToChannelMap.ContainsKey(channel.SystemName))
                            {
                                log.ErrorFormat("channel systemName with value {0} exists for more than one channel", channel.SystemName);
                                result = false;
                            }
                            else
                            {
                                channelNamesToChannelMap.TryAdd(channel.SystemName, channel);
                            }
                        }
                    });

                    if (channelNamesToChannelMap != null && channelNamesToChannelMap.Count > 0)
                    {
                        groupChannels = channelNamesToChannelMap.Values.ToList();
                    }
                }
                else
                {
                    log.Warn("No channels were found for the account");
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed CreateGroupChannelsMapping", ex);
                result = false;
            }

            return result;
        }

        private HashSet<int> GetGroupChannelIds(List<int> groupIds)
        {
            HashSet<int> channelIds = new HashSet<int>();
            try
            {
                DataTable dt = OPCMigrationDAL.GetGroupChannelIdsIncludeInActive(groupIds);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                        if (id > 0 && !channelIds.Contains(id))
                        {
                            channelIds.Add(id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed GetGroupChannelIds", ex);
            }

            return channelIds;
        }

        private bool PopulateChannelsTranslationsMapping(List<int> groupIds, HashSet<int> channelIds, List<LanguageObj> groupLanguages,
                                                            ref Dictionary<int, List<LanguageContainer>> channelsNameTranslationsMapping,
                                                            ref Dictionary<int, List<LanguageContainer>> channelsDescriptionTranslationsMapping)
        {
            bool res = true;
            try
            {
                DataTable dt = OPCMigrationDAL.GetChannelsTranslations(groupIds, channelIds);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    Dictionary<int, string> languageIdToLanguageCode = new Dictionary<int, string>();
                    if (groupLanguages != null && groupLanguages.Count > 0)
                    {
                        languageIdToLanguageCode = groupLanguages.ToDictionary(x => (int)x.ID, x => x.Code);
                        foreach (DataRow dr in dt.Rows)
                        {
                            int channelId = ODBCWrapper.Utils.GetIntSafeVal(dr, "CHANNEL_ID");
                            int languageId = ODBCWrapper.Utils.GetIntSafeVal(dr, "LANGUAGE_ID");
                            string name = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                            string description = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION");
                            bool hasNameTrans = !string.IsNullOrEmpty(name);
                            bool hasDescTrans = !string.IsNullOrEmpty(description);

                            if (channelId > 0 && languageId > 0 && languageIdToLanguageCode.ContainsKey(languageId) && (hasNameTrans || hasDescTrans))
                            {
                                string langCode = languageIdToLanguageCode[languageId];
                                if (hasNameTrans)
                                {
                                    LanguageContainer langContName = new LanguageContainer(langCode, name);
                                    if (!channelsNameTranslationsMapping.ContainsKey(channelId))
                                    {
                                        channelsNameTranslationsMapping.Add(channelId, new List<LanguageContainer>());
                                    }

                                    channelsNameTranslationsMapping[channelId].Add(langContName);
                                }

                                if (hasDescTrans)
                                {
                                    LanguageContainer langContDesc = new LanguageContainer(langCode, description);
                                    if (!channelsDescriptionTranslationsMapping.ContainsKey(channelId))
                                    {
                                        channelsDescriptionTranslationsMapping.Add(channelId, new List<LanguageContainer>());
                                    }

                                    channelsDescriptionTranslationsMapping[channelId].Add(langContDesc);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed PopulateChannelsTranslationsMapping", ex);
                res = false;
            }

            return res;
        }

        private bool AddDefaultImages(List<int> groupIds, ref Dictionary<long, string> picIdToImageTypeNameMap)
        {
            bool result = true;
            try
            {
                HashSet<string> defaultRatiosImages = new HashSet<string>();
                foreach (int groupId in groupIds)
                {
                    List<Core.Catalog.PicData> defaultGroupPics = Core.Catalog.Cache.CatalogCache.Instance().GetDefaultImages(groupId);
                    if (defaultGroupPics != null && defaultGroupPics.Count > 0)
                    {
                        foreach (Core.Catalog.PicData picData in defaultGroupPics)
                        {
                            if (defaultRatiosImages.Contains(picData.Ratio))
                            {
                                log.ErrorFormat("ratio {0} has more than 1 default image for the account", picData.Ratio);
                                result = false;
                            }
                            else
                            {
                                defaultRatiosImages.Add(picData.Ratio);
                                if (picIdToImageTypeNameMap.ContainsKey(picData.PicId))
                                {
                                    log.ErrorFormat("picId {0} was already added as a default image", picData.PicId);
                                    result = false;
                                }
                                else
                                {
                                    picIdToImageTypeNameMap.Add(picData.PicId, picData.Ratio);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("failed Core.Catalog.Cache.CatalogCache.Instance().GetDefaultImages", ex);
                result = false;
            }

            return result;
        }

    }
}