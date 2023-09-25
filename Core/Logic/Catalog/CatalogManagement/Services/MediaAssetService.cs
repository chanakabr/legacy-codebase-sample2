using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Phx.Lib.Log;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TVinciShared;

namespace Core.Catalog.CatalogManagement
{
    public class MediaAssetService : IMediaAssetService
    {
        private static readonly IKLogger Log = new KLogger(nameof(MediaAssetService));
        private static readonly Lazy<MediaAssetService> Lazy = new Lazy<MediaAssetService>(() => new MediaAssetService(CatalogManager.Instance), LazyThreadSafetyMode.PublicationOnly);

        private readonly ICatalogManager _catalogManager;

        public static MediaAssetService Instance => Lazy.Value;

        public MediaAssetService(ICatalogManager catalogManager)
        {
            _catalogManager = catalogManager;
        }

        public MediaAsset CreateMediaAsset(
            long groupId,
            DataTable basicTable,
            DataTable metasTable,
            DataTable tagsTable,
            DataTable newTagsTable,
            DataTable filesTable,
            DataTable labelsTable,
            DataTable mediaFileDynamicDataTable,
            DataTable imagesTable,
            DataTable updateDateTable,
            DataTable linearAssetTable,
            DataTable nameRelatedEntitiesTable,
            DataTable liveToVodAssetTable,
            bool isForIndex = false,
            bool isForMigration = false,
            bool isMinimalOutput = false)
        {
            if (!_catalogManager.TryGetCatalogGroupCacheFromCache((int)groupId, out var catalogGroupCache))
            {
                Log.Error($"{nameof(ICatalogManager.TryGetCatalogGroupCacheFromCache)} failed with parameters ({groupId}).");

                return null;
            }

            if (basicTable == null)
            {
                Log.LogWarning($"{nameof(CreateMediaAsset)} failed because {nameof(basicTable)} is not specified.");

                return null;
            }

            var basicDataRow = basicTable.Rows[0];

            var id = ODBCWrapper.Utils.GetLongSafeVal(basicDataRow, "ID", 0);
            var assetStructId = ODBCWrapper.Utils.GetLongSafeVal(basicDataRow, "ASSET_STRUCT_ID", 0);
            if (!TryGetMediaTypeFromAssetStructId(groupId, assetStructId, catalogGroupCache, out var mediaType))
            {
                Log.LogWarning($"Media type (assetStruct) is not valid for media with Id: {id}, assetStructId: {assetStructId}.");

                return null;
            }

            var createDate = ODBCWrapper.Utils.GetNullableDateSafeVal(basicDataRow, "CREATE_DATE");
            var finalEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(basicDataRow, "FINAL_END_DATE");
            var startDate = ODBCWrapper.Utils.GetNullableDateSafeVal(basicDataRow, "START_DATE");
            var endDate = ODBCWrapper.Utils.GetNullableDateSafeVal(basicDataRow, "END_DATE");
            var catalogStartDate = ODBCWrapper.Utils.GetNullableDateSafeVal(basicDataRow, "CATALOG_START_DATE");
            var maxUpdateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(basicDataRow, "UPDATE_DATE");
            var fallbackEpgIdentifier = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "EPG_IDENTIFIER");
            var inheritancePolicy = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "INHERITANCE_POLICY");
            var assetInheritancePolicy = (AssetInheritancePolicy)Enum.ToObject(typeof(AssetInheritancePolicy), inheritancePolicy);

            // Metas and Tags table
            List<Metas> metas = null;
            List<Tags> tags = null;
            List<RelatedEntities> relatedEntitiesList = null;

            if (!isMinimalOutput && !TryGetMetasAndTags(groupId, catalogGroupCache.LanguageMapById.Values.ToList(), metasTable, tagsTable, ref metas, ref tags, ref maxUpdateDate))
            {
                Log.LogWarning("CreateMediaAssetFromDataSet - failed to get media metas and tags for Id: {0}", id);

                return null;
            }

            // Files table
            List<AssetFile> files = null;
            if (!isForIndex && filesTable != null && filesTable.Rows.Count > 0)
            {
                files = FileManager.CreateAssetFilesFromDataTables((int)groupId, filesTable, labelsTable, mediaFileDynamicDataTable);
                // get only active files
                files = files.Where(x => x.IsActive.HasValue && x.IsActive.Value).ToList();

                var maxFilesUpdateDate = files.Max(x => x.UpdateDate);

                if (maxFilesUpdateDate.HasValue && (!maxUpdateDate.HasValue || maxFilesUpdateDate.Value > maxUpdateDate.Value))
                {
                    maxUpdateDate = maxFilesUpdateDate;
                }
            }

            // Images table
            List<Image> images = new List<Image>();
            if (imagesTable != null && imagesTable?.Rows.Count > 0)
            {
                GenericListResponse<Image> imageResponse = ImageManager.CreateImageListResponseFromDataTable((int)groupId, imagesTable, true);
                if (!imageResponse.HasObjects())
                {
                    Log.LogWarning("CreateMediaAssetFromDataSet - failed to get images for Id: {0}", id);
                    return null;
                }

                // get only active images
                images = imageResponse.Objects.Any(x => x.Status == eTableStatus.OK) ? imageResponse.Objects.Where(x => x.Status == eTableStatus.OK).ToList() : new List<Image>();
            }

            List<Image> groupDefaultImages = ImageManager.GetGroupDefaultImages((int)groupId);
            HashSet<long> assetImageTypes = new HashSet<long>(images.Select(x => x.ImageTypeId).ToList());
            images.AddRange(groupDefaultImages.Where(x => !assetImageTypes.Contains(x.ImageTypeId)));

            // new tags
            if (!isForMigration && newTagsTable!=null && newTagsTable?.Rows.Count > 0)
            {
                foreach (DataRow dr in newTagsTable.Rows)
                {
                    int topicId = ODBCWrapper.Utils.GetIntSafeVal(dr, "topic_id");
                    int tagId = ODBCWrapper.Utils.GetIntSafeVal(dr, "tag_id");
                    int languageId = ODBCWrapper.Utils.GetIntSafeVal(dr, "language_id");
                    string translation = ODBCWrapper.Utils.GetSafeStr(dr, "translation");
                    DateTime tagCreateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "create_date");
                    DateTime tagUpdateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "update_date");
                    ApiObjects.SearchObjects.TagValue tag = new ApiObjects.SearchObjects.TagValue()
                    {
                        createDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(tagCreateDate),
                        languageId = languageId,
                        tagId = tagId,
                        topicId = topicId,
                        updateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(tagUpdateDate),
                        value = translation
                    };

                    // Update Tag Index
                    var indexManager = IndexManagerFactory.Instance.GetIndexManager((int)groupId);
                    Status updateTagResponse = indexManager.UpdateTag(tag);
                    if (updateTagResponse == null || updateTagResponse.Code != (int)eResponseStatus.OK)
                    {
                        Log.LogWarning("CreateMediaAsset - failed to update tag, tag : {0}, addTagResult: {1}", tag.ToString(), updateTagResponse != null ? updateTagResponse.Message : "null");
                    }
                }
            }

            // update dates
            if (updateDateTable != null && updateDateTable?.Rows.Count > 0)
            {
                DateTime? assetUpdateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(updateDateTable.Rows[0], "UPDATE_DATE");
                // override existing update with the max update date from all possible tables (from GetAssetUpdateDate stored procedure)
                if (assetUpdateDate.HasValue)
                {
                    maxUpdateDate = assetUpdateDate;
                }
            }

            // Handle new relatedEntities
            if (nameRelatedEntitiesTable != null && nameRelatedEntitiesTable?.Rows.Count > 0)
            {
                if (!TryGetRelatedEntitiesList(groupId, id, catalogGroupCache, nameRelatedEntitiesTable, ref relatedEntitiesList))
                {
                    Log.LogWarning("CreateMediaAsset - failed to get media RelatedEntities for Id: {0}", id);
                    return null;
                }
            }

            string name = string.Empty;
            string description = null;
            List<LanguageContainer> namesWithLanguages = null;
            List<LanguageContainer> descriptionsWithLanguages = null;
            if (!isMinimalOutput && !ExtractMediaAssetNamesAndDescriptionsFromMetas(metas, ref name, ref description, ref namesWithLanguages, ref descriptionsWithLanguages))
            {
                Log.LogWarning("Name is not valid for media with Id: {0}", id);

                return null;
            }

            string entryId = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "ENTRY_ID");
            string coGuid = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "CO_GUID");
            bool isActive = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "IS_ACTIVE", 0) == 1;
            int? deviceRuleId = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "DEVICE_RULE_ID", -1);
            int? geoBlockRuleId = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "GEO_BLOCK_RULE_ID", -1);
            string userTypes = ODBCWrapper.Utils.GetSafeStr(basicDataRow, "user_types");

            var result = new MediaAsset(
                id,
                eAssetTypes.MEDIA,
                name,
                namesWithLanguages,
                description,
                descriptionsWithLanguages,
                createDate,
                maxUpdateDate,
                startDate,
                endDate,
                metas,
                tags,
                images,
                coGuid,
                isActive,
                catalogStartDate,
                finalEndDate,
                mediaType,
                entryId,
                deviceRuleId == -1 ? null : deviceRuleId,
                geoBlockRuleId == -1 ? null : geoBlockRuleId,
                files,
                userTypes,
                assetInheritancePolicy,
                fallbackEpgIdentifier)
            {
                RelatedEntities = relatedEntitiesList
            };


            // if media is linear we also have the epg channel table returned
            if (!isForIndex && linearAssetTable != null && linearAssetTable?.Rows.Count > 0)
            {
                result = CreateLinearMediaAsset(groupId, result, linearAssetTable);
            }
            else if (liveToVodAssetTable?.Rows.Count > 0)
            {
                result = CreateLiveToVodAsset(groupId, result, liveToVodAssetTable);
            }

            return result;
        }

        public IEnumerable<MediaAsset> CreateMediaAssets(long groupId, DataSet dataSet)
        {
            List<MediaAsset> result = null;
            try
            {
                if (dataSet == null || dataSet.Tables.Count < 6)
                {
                    Log.LogWarning("CreateMediaAssets didn't receive dataset with 7 or more tables");

                    return null;
                }

                // Basic details table
                if (dataSet.Tables[0] == null || dataSet.Tables[0].Rows.Count <= 0)
                {
                    Log.LogWarning("CreateMediaAssets - basic details table is not valid");

                    return null;
                }

                result = new List<MediaAsset>();

                var metasTable = GetDataRows(dataSet, 1);
                var tagsTable = GetDataRows(dataSet, 2);
                var filesTable = GetDataRows(dataSet, 3);
                var filesLabelsTable = GetDataRows(dataSet, 4);
                var filesDynamicDataTable = GetDataRows(dataSet, 5);
                var imagesTable = GetDataRows(dataSet, 6);
                var linearMediasTable = GetDataRows(dataSet, 7);
                var relatedEntitiesTable = GetDataRows(dataSet, 8);
                var liveToVodMediasTable = GetDataRows(dataSet, 9);

                foreach (DataRow basicDataRow in dataSet.Tables[0].Rows)
                {
                    var id = ODBCWrapper.Utils.GetIntSafeVal(basicDataRow, "ID", 0);
                    if (id > 0)
                    {
                        var basicDataTable = dataSet.Tables[0].Clone();
                        basicDataTable.ImportRow(basicDataRow);

                        var mediaAsset = CreateMediaAsset(
                            groupId,
                            basicDataTable,
                            GetTableByAssetDataRows(metasTable, "ASSET_ID", id, dataSet, 1),
                            GetTableByAssetDataRows(tagsTable, "ASSET_ID", id, dataSet, 2),
                            null,
                            GetTableByAssetDataRows(filesTable, "MEDIA_ID", id, dataSet, 3),
                            GetTableByAssetDataRows(filesLabelsTable, "MEDIA_ID", id, dataSet, 4),
                            GetTableByAssetDataRows(filesDynamicDataTable, "MEDIA_ID", id, dataSet, 5),
                            GetTableByAssetDataRows(imagesTable, "ASSET_ID", id, dataSet, 6),
                            null,
                            GetTableByAssetDataRows(linearMediasTable, "MEDIA_ID", id, dataSet, 7),
                            GetTableByAssetDataRows(relatedEntitiesTable, "ASSET_ID", id, dataSet, 8),
                            GetTableByAssetDataRows(liveToVodMediasTable, "MEDIA_ID", id, dataSet, 9));
                        if (mediaAsset != null)
                        {
                            mediaAsset.IndexStatus = AssetIndexStatus.Ok;
                            result.Add(mediaAsset);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed CreateMediaAssets for groupId: {groupId}", e);
            }

            return result;
        }

        public LiveAsset CreateLinearMediaAsset(long groupId, MediaAsset mediaAsset, DataTable dataTable, long? epgChannelId = null)
        {
            if (dataTable == null || dataTable.Rows.Count != 1)
            {
                Log.LogWarning("CreateLinearMediaAssetResponseFromDataTable - returned table is not valid");
                return null;
            }

            var dataRow = dataTable.Rows[0];
            var enableCdvr = (TstvState)ODBCWrapper.Utils.GetIntSafeVal(dataRow, "ENABLE_CDVR");
            var enableCatchUp = (TstvState)ODBCWrapper.Utils.GetIntSafeVal(dataRow, "ENABLE_CATCH_UP");
            var enableStartOver = (TstvState)ODBCWrapper.Utils.GetIntSafeVal(dataRow, "ENABLE_START_OVER");
            var enableTrickPlay = (TstvState)ODBCWrapper.Utils.GetIntSafeVal(dataRow, "ENABLE_TRICK_PLAY");
            var enableRecordingPlaybackNonEntitledChannel = (TstvState)ODBCWrapper.Utils.GetIntSafeVal(dataRow, "enable_recording_playback_non_entitled");
            var catchUpBuffer = ODBCWrapper.Utils.GetLongSafeVal(dataRow, "CATCH_UP_BUFFER", 0);
            var paddingBeforeProgramStats = ODBCWrapper.Utils.GetLongSafeVal(dataRow, "PADDING_BEFORE_PROGRAM_STARTS", 0);
            var paddingAfterProgramEnds = ODBCWrapper.Utils.GetLongSafeVal(dataRow, "PADDING_AFTER_PROGRAM_ENDS", 0);
            var trickPlayBuffer = ODBCWrapper.Utils.GetLongSafeVal(dataRow, "TRICK_PLAY_BUFFER", 0);
            var externalIngestId = ODBCWrapper.Utils.GetSafeStr(dataRow, "CHANNEL_ID");
            var externalCdvrId = ODBCWrapper.Utils.GetSafeStr(dataRow, "CDVR_ID");
            var channelType = (LinearChannelType)ODBCWrapper.Utils.GetIntSafeVal(dataRow, "epg_channel_type");
            var accountTstvSettings = ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings((int)groupId);
            if (!epgChannelId.HasValue)
            {
                epgChannelId = ODBCWrapper.Utils.GetLongSafeVal(dataRow, "ID");
            }

            var updateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dataRow, "UPDATE_DATE");

            if (updateDate.HasValue && (!mediaAsset.UpdateDate.HasValue || updateDate.Value > mediaAsset.UpdateDate.Value))
            {
                mediaAsset.UpdateDate = updateDate;
            }

            var result = new LiveAsset(
                epgChannelId.Value,
                enableCdvr,
                enableCatchUp,
                enableStartOver,
                enableTrickPlay,
                enableRecordingPlaybackNonEntitledChannel,
                catchUpBuffer,
                paddingBeforeProgramStats,
                paddingAfterProgramEnds,
                trickPlayBuffer,
                externalIngestId,
                externalCdvrId,
                mediaAsset,
                accountTstvSettings,
                channelType);

            return result;
        }

        public LiveToVodAsset CreateLiveToVodAsset(long partnerId, MediaAsset mediaAsset, DataTable dataTable)
        {
            var dataRow = dataTable.Rows[0];
            var liveToVodAsset = new LiveToVodAsset(mediaAsset)
            {
                EpgId = ODBCWrapper.Utils.GetLongSafeVal(dataRow, "EPG_ID"),
                EpgIdentifier = ODBCWrapper.Utils.GetSafeStr(dataRow, "EPG_IDENTIFIER"),
                LinearAssetId = ODBCWrapper.Utils.GetLongSafeVal(dataRow, "LINEAR_ASSET_ID"),
                EpgChannelId = ODBCWrapper.Utils.GetLongSafeVal(dataRow, "EPG_CHANNEL_ID"),
                Crid = ODBCWrapper.Utils.GetSafeStr(dataRow, "CRID"),
                OriginalStartDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "ORIGINAL_START_DATE"),
                OriginalEndDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "ORIGINAL_END_DATE"),
                PaddingBeforeProgramStarts = ODBCWrapper.Utils.GetLongSafeVal(dataRow, "PADDING_BEFORE_PROGRAM_STARTS"),
                PaddingAfterProgramEnds = ODBCWrapper.Utils.GetLongSafeVal(dataRow, "PADDING_AFTER_PROGRAM_ENDS")
            };

            var updateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dataRow, "UPDATE_DATE");
            if (updateDate.HasValue && (!mediaAsset.UpdateDate.HasValue || updateDate.Value > mediaAsset.UpdateDate.Value))
            {
                liveToVodAsset.UpdateDate = updateDate;
            }

            return liveToVodAsset;
        }

        private bool TryGetMediaTypeFromAssetStructId(long groupId, long assetStructId, CatalogGroupCache catalogGroupCache, out MediaType mediaType)
        {
            if (!catalogGroupCache.AssetStructsMapById.ContainsKey(assetStructId))
            {
                Log.LogWarning("assetStructId: {0} doesn't exist for groupId: {1}", assetStructId, groupId);

                mediaType = null;
                return false;
            }

            mediaType = new MediaType(catalogGroupCache.AssetStructsMapById[assetStructId].SystemName, (int)assetStructId);
            return true;
        }

        public bool TryGetMetasAndTags(long groupId, IEnumerable<LanguageObj> groupLanguages, DataTable metasTable, DataTable tagsTable, ref List<Metas> metas, ref List<Tags> tags, ref DateTime? maxUpdateDate)
        {
            bool res = false;
            Dictionary<long, List<LanguageContainer>> topicIdToMeta = new Dictionary<long, List<LanguageContainer>>();
            Dictionary<long, LanguageObj> languagesDictionary = new Dictionary<long, LanguageObj>();

            foreach (var language in groupLanguages)
            {
                languagesDictionary[language.ID] = language;
            }

            foreach (DataRow dr in metasTable.Rows)
            {
                long topicId = ODBCWrapper.Utils.GetLongSafeVal(dr, "topic_id");
                long languageId = ODBCWrapper.Utils.GetLongSafeVal(dr, "language_id");
                string translation = ODBCWrapper.Utils.GetSafeStr(dr, "translation");
                DateTime? updateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "UPDATE_DATE");

                if (updateDate.HasValue && (!maxUpdateDate.HasValue || updateDate.Value > maxUpdateDate.Value))
                {
                    maxUpdateDate = updateDate;
                }

                if (languagesDictionary.ContainsKey(languageId))
                {
                    LanguageObj language = languagesDictionary[languageId];

                    if (!topicIdToMeta.ContainsKey(topicId))
                    {
                        topicIdToMeta.Add(topicId, new List<LanguageContainer> { new LanguageContainer(language.Code, translation, language.IsDefault) });
                    }
                    else
                    {
                        topicIdToMeta[topicId].Add(new LanguageContainer(language.Code, translation, language.IsDefault));
                    }
                }
            }

            // TODO Lior - Remove TagId, not needed
            Dictionary<long, Dictionary<long, List<LanguageContainer>>> topicIdToTag = new Dictionary<long, Dictionary<long, List<LanguageContainer>>>();
            foreach (DataRow dr in tagsTable.Rows)
            {
                long topicId = ODBCWrapper.Utils.GetLongSafeVal(dr, "topic_id");
                long tagId = ODBCWrapper.Utils.GetLongSafeVal(dr, "tag_id");
                long languageId = ODBCWrapper.Utils.GetLongSafeVal(dr, "language_id");
                string translation = ODBCWrapper.Utils.GetSafeStr(dr, "translation");
                DateTime? updateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "UPDATE_DATE");

                if (updateDate.HasValue && (!maxUpdateDate.HasValue || updateDate.Value > maxUpdateDate.Value))
                {
                    maxUpdateDate = updateDate;
                }


                if (languagesDictionary.ContainsKey(languageId))
                {
                    LanguageObj language = languagesDictionary[languageId];

                    if (!topicIdToTag.ContainsKey(topicId))
                    {
                        topicIdToTag.Add(topicId, new Dictionary<long, List<LanguageContainer>>());
                        topicIdToTag[topicId].Add(tagId, new List<LanguageContainer>() { new LanguageContainer(language.Code, translation, language.IsDefault) });
                    }
                    else if (!topicIdToTag[topicId].ContainsKey(tagId))
                    {
                        topicIdToTag[topicId].Add(tagId, new List<LanguageContainer>() { new LanguageContainer(language.Code, translation, language.IsDefault) });
                    }
                    else
                    {
                        topicIdToTag[topicId][tagId].Add(new LanguageContainer(language.Code, translation, language.IsDefault));
                    }
                }
            }

            List<long> topicIds = new List<long>();
            topicIds.AddRange(topicIdToMeta.Keys);
            topicIds.AddRange(topicIdToTag.Keys);
            if (topicIds.Count > 0)
            {
                GenericListResponse<Topic> groupTopicsResponse = TopicManager.Instance.GetTopicsByIds((int)groupId, topicIds, MetaType.All);

                if (groupTopicsResponse?.Status != null && groupTopicsResponse.Status.Code == (int)eResponseStatus.OK && groupTopicsResponse.Objects != null && groupTopicsResponse.Objects.Count > 0)
                {
                    metas = new List<Metas>();
                    tags = new List<Tags>();
                    foreach (Topic topic in groupTopicsResponse.Objects)
                    {
                        if (topic.Type == MetaType.Tag)
                        {
                            if (topicIdToTag.ContainsKey(topic.Id))
                            {
                                Dictionary<long, List<LanguageContainer>> topicTags = topicIdToTag[topic.Id];
                                List<LanguageContainer> defaultLanguageValues = topicTags.SelectMany(x => x.Value.Where(y => y.IsDefault)).ToList();
                                List<string> defaultValues = defaultLanguageValues.Select(x => x.m_sValue).ToList();
                                List<LanguageContainer[]> tagLanguages = topicIdToTag[topic.Id].Select(x => x.Value.Select(y => y).ToArray()).ToList();
                                tags.Add(new Tags(new TagMeta(topic.SystemName, topic.Type.ToString()), defaultValues, tagLanguages));
                            }
                        }
                        else
                        {
                            if (topicIdToMeta.ContainsKey(topic.Id))
                            {
                                IEnumerable<LanguageContainer> topicLanguages = null;
                                string defaultValue = topicIdToMeta[topic.Id].FirstOrDefault(x => x.IsDefault)?.m_sValue;
                                if (topic.Type == MetaType.MultilingualString)
                                {
                                    topicLanguages = topicIdToMeta[topic.Id].Where(x => !x.IsDefault).Select(x => x);
                                }

                                metas.Add(new Metas(new TagMeta(topic.SystemName, topic.Type.ToString()), defaultValue, topicLanguages));
                            }
                        }
                    }

                    res = true;
                }
            }

            return res;
        }

        private bool TryGetRelatedEntitiesList(long groupId, long id, CatalogGroupCache catalogGroupCache, DataTable relatedEntitiesTable, ref List<RelatedEntities> relatedEntitiesList)
        {
            if (relatedEntitiesTable?.Rows.Count > 0)
            {
                relatedEntitiesList = new List<RelatedEntities>();

                foreach (DataRow dr in relatedEntitiesTable.Rows)
                {
                    List<RelatedEntity> relatedEntityList = null;
                    var topicId = ODBCWrapper.Utils.GetLongSafeVal(dr, "topic_id");
                    var value = ODBCWrapper.Utils.GetSafeStr(dr, "value");

                    try
                    {
                        relatedEntityList = JsonConvert.DeserializeObject<List<RelatedEntity>>(value);
                    }
                    catch (Exception exc)
                    {
                        Log.ErrorFormat("Error while DeserializeObject<List<RelatedEntity> at TryGetRelatedEntitiesList. topicId {0}, groupId {1}, assetId {2}. exc {3}", topicId, groupId, id, exc.Message);
                    }

                    if (catalogGroupCache.TopicsMapById.ContainsKey(topicId))
                    {
                        var topic = catalogGroupCache.TopicsMapById[topicId];
                        var relatedEntities = new RelatedEntities
                        {
                            TagMeta = new TagMeta { m_sName = topic.SystemName, m_sType = topic.Type.ToString() },
                            Items = relatedEntityList
                        };

                        relatedEntitiesList.Add(relatedEntities);
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public const string NAME_META_SYSTEM_NAME = "Name";
        public const string DESCRIPTION_META_SYSTEM_NAME = "Description";

        private static bool ExtractMediaAssetNamesAndDescriptionsFromMetas(List<Metas> metas, ref string name, ref string description, ref List<LanguageContainer> namesWithLanguages,
            ref List<LanguageContainer> descriptionsWithLanguages)
        {
            if (metas != null && metas.Count > 0)
            {
                Metas nameMeta = metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower() == NAME_META_SYSTEM_NAME.ToLower());
                if (nameMeta != null && !string.IsNullOrEmpty(nameMeta.m_sValue))
                {
                    name = nameMeta.m_sValue;
                    if (nameMeta.Value != null && nameMeta.Value.Length > 0)
                    {
                        namesWithLanguages = new List<LanguageContainer>(nameMeta.Value);
                    }

                    metas.Remove(nameMeta);
                }
                else
                {
                    return false;
                }

                Metas descMeta = metas.FirstOrDefault(x => x.m_oTagMeta.m_sName.ToLower() == DESCRIPTION_META_SYSTEM_NAME.ToLower());
                if (descMeta != null)
                {
                    description = descMeta.m_sValue;
                    if (descMeta.Value != null && descMeta.Value.Length > 0)
                    {
                        descriptionsWithLanguages = new List<LanguageContainer>(descMeta.Value);
                    }

                    metas.Remove(descMeta);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private static EnumerableRowCollection<DataRow> GetDataRows(DataSet ds, int tableIndex)
        {
            var dataRows = new DataTable().AsEnumerable();
            if (ds.Tables.Count > tableIndex && ds.Tables[tableIndex] != null && ds.Tables[tableIndex].Rows.Count > 0)
            {
                dataRows = ds.Tables[tableIndex].AsEnumerable();
            }

            return dataRows;
        }

        private static DataTable GetTableByAssetDataRows(EnumerableRowCollection<DataRow> dataRows, string assetIdColumn, int assetId, DataSet ds, int tableIndex)
        {
            var dataRowsByAsset = dataRows.Where(row => (long)row[assetIdColumn] == assetId);
            if (dataRowsByAsset != null && dataRowsByAsset.Any())
            {
                return dataRowsByAsset.CopyToDataTable();
            }

            if (ds.Tables.Count > tableIndex)
            {
                return ds.Tables[tableIndex].Clone();
            }

            return null;
        }
    }
}