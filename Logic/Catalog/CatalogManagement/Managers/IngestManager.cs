using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.CDNAdapter;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Tvinci.Core.DAL;
using TVinciShared;
using TagValue = ApiObjects.SearchObjects.TagValue;

namespace Core.Catalog.CatalogManagement
{
    public class IngestManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly XmlSerializer xmlIngestFeedSerializer = new XmlSerializer(typeof(IngestVODFeed));

        #region Consts

        // ERRORS
        private const string MISSING_EXTERNAL_IDENTIFIER = "External identifier is missing ";
        private const string MISSING_ENTRY_ID = "entry_id is missing";
        private const string MISSING_ACTION = "action is missing";
        private const string ITEM_TYPE_NOT_RECOGNIZED = "Item type not recognized";
        private const string WATCH_PERMISSION_RULE_NOT_RECOGNIZED = "Watch permission rule not recognized";
        private const string GEO_BLOCK_RULE_NOT_RECOGNIZED = "Geo block rule not recognized";
        private const string DEVICE_RULE_NOT_RECOGNIZED = "Device rule not recognized";
        private const string PLAYERS_RULE_NOT_RECOGNIZED = "Players rule not recognized ";
        private const string UPDATE_INDEX_FAILED = "Update index failed";
        private const string MEDIA_ID_NOT_EXIST = "Media Id not exist";
        private const string CANNOT_BE_EMPTY = "{0} cannot be empty";

        private const long USER_ID = 999;
        private const string DELETE_ACTION = "delete";
        private const string INSERT_ACTION = "insert";
        private const string UPDATE_ACTION = "update";
        private const string ASSET_FILE_DATE_FORMAT = "dd/MM/yyyy HH:mm:ss";
        private const string TRUE = "true";
        private const string FALSE = "false";

        #endregion

        public static IngestResponse HandleMediaIngest(int groupId, string xml)
        {
            log.DebugFormat("Start HandleMediaIngest. groupId:{0}", groupId);
            IngestResponse ingestResponse = IngestResponse.Default;

            var feedResponse = DeserializeXmlToFeed(xml, groupId, ref ingestResponse);
            if (!feedResponse.HasObject())
            {
                ingestResponse.IngestStatus = feedResponse.Status;
                return ingestResponse;
            }

            CatalogGroupCache cache;
            if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out cache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling HandleMediaIngest", groupId);
                return ingestResponse;
            }

            // get data for group
            var mediaFileTypes = FileManager.GetMediaFileTypes(groupId);
            var groupDefaultRatio = ImageUtils.GetGroupDefaultRatioName(groupId);
            var groupRatioNamesToImageTypes = ImageManager.GetImageTypesMapBySystemName(groupId);
            var tagsTranslations = new Dictionary<string, TagsTranslations>();
            var assetsWithNoTags = new Dictionary<int, bool>();
            var cdnAdapters = GetCDNAdaptersMapping(groupId);

            for (int i = 0; i < feedResponse.Object.Export.MediaList.Count; i++)
            {
                IngestMedia media = feedResponse.Object.Export.MediaList[i];
                ingestResponse.AssetsStatus.Add(IngestAssetStatus.Default);

                // check media.CoGuid
                if (string.IsNullOrEmpty(media.CoGuid))
                {
                    ingestResponse.AssetsStatus[i].Status.Set((int)eResponseStatus.MissingExternalIdentifier, MISSING_EXTERNAL_IDENTIFIER);
                    ingestResponse.Set(string.Empty, "Missing co_guid", "FAILED", 0);
                    log.ErrorFormat("Error import mediaIndex{0}, ErrorMessage:{1}", i, ingestResponse.Description);
                    continue;
                }

                try
                {
                    int mediaId = BulkAssetManager.GetMediaIdByCoGuid(groupId, media.CoGuid);
                    List<Tags> currentTags;
                    if (ValidateMedia(i, media, groupId, cache, mediaId, ref ingestResponse, out currentTags))
                    {
                        if (media.Action.Equals(DELETE_ACTION))
                        {
                            if (!DeleteMediaAsset(mediaId, media.CoGuid, groupId, ref ingestResponse, i))
                                continue;
                        }
                        else
                        {
                            bool isMediaExists = mediaId > 0 ;
                            MediaAsset mediaAsset = CreateMediaAsset(groupId, mediaId, media, cache, currentTags);
                            var images = GetImages(media.Basic, groupId, groupDefaultRatio, groupRatioNamesToImageTypes);
                            var assetFiles = GetAssetFiles(media.Files, mediaFileTypes, cdnAdapters);

                            var upsertStatus = BulkAssetManager.UpsertMediaAsset(groupId, mediaAsset, USER_ID, images, assetFiles, ASSET_FILE_DATE_FORMAT, TRUE.Equals(media.Erase), true);
                            if (!upsertStatus.IsOkStatusCode())
                            {
                                ingestResponse.AssetsStatus[i].Status = upsertStatus.Status;
                                ingestResponse.Set(mediaAsset.CoGuid, "UpsertMediaAsset faild", "FAILED", (int)mediaAsset.Id);
                                continue;
                            }

                            ingestResponse.AssetsStatus[i].Warnings.AddRange(upsertStatus.Objects);
                            ingestResponse.Set(mediaAsset.CoGuid, "succeeded Upsert media", "OK", (int)mediaAsset.Id);
                            ingestResponse.AssetsStatus[i].InternalAssetId = (int)mediaAsset.Id;
                            if (mediaAsset.Tags.Count == 0)
                            {
                                assetsWithNoTags.Add((int)mediaAsset.Id, isMediaExists);
                            }
                            else
                            {
                                AddTagsToTranslations(currentTags, (int)mediaAsset.Id, isMediaExists, ref tagsTranslations);
                            }
                        }

                        //// update notification 
                        //if (mediaAsset.IsActive.HasValue && mediaAsset.IsActive.Value)
                        //{
                        //    UpdateNotificationsRequests(groupId, mediaAsset.Id);
                        //}

                        // succeeded import media
                        ingestResponse.AssetsStatus[i].Status.Set(eResponseStatus.OK);
                        log.DebugFormat("succeeded import media. CoGuid:{0}, MediaID:{1}, ErrorMessage:{2}", media.CoGuid, mediaId, media.IsActive, ingestResponse.Description);
                    }
                    else
                    {
                        ingestResponse.Set(media.CoGuid, "Media data is not valid", "FAILED", mediaId);
                        log.ErrorFormat("Media data is not valid. mediaIndex{0}. CoGuid:{1}, MediaID:{2}, ErrorMessage:{3}", i, media.CoGuid, mediaId, ingestResponse.Description);
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = string.Format("Exception while HandleMediaIngest for mediaIndex: {0}, Exception:{1}", i, ex);
                    log.Error(errorMsg);
                    ingestResponse.AssetsStatus[i].Status.Set((int)eResponseStatus.Error, errorMsg);
                }
            }

            if (ingestResponse.AssetsStatus.All(x => x.Status != null && x.Status.Code == (int)eResponseStatus.OK))
            {
                ingestResponse.IngestStatus.Set(eResponseStatus.OK);
            }

            if (tagsTranslations.Count > 0)
            {
                HandleTagsTranslations(tagsTranslations, groupId, cache, ref ingestResponse);
            }

            if (assetsWithNoTags.Count > 0)
            {
                IndexAndInvalidateAssets(groupId, assetsWithNoTags);
            }
            
            log.DebugFormat("End HandleMediaIngest. groupId:{0}", groupId);

            return ingestResponse;
        }
        
        private static GenericResponse<IngestVODFeed> DeserializeXmlToFeed(string xml, int groupId, ref IngestResponse ingestResponse)
        {
            var response = new GenericResponse<IngestVODFeed>();
            Object deserializeObject = null;

            try
            {
                using (StringReader stringReader = new StringReader(xml))
                {
                    using (XmlTextReader xmlReader = new XmlTextReader(stringReader))
                    {
                        deserializeObject = xmlIngestFeedSerializer.Deserialize(xmlReader);
                    }
                }
            }
            catch (XmlException ex)
            {
                response.SetStatus(eResponseStatus.IllegalXml, "XML file with wrong format");
                log.ErrorFormat("XML file with wrong format: {0}. groupId:{1}. Exception: {2}", xml, groupId, ex);
                return response;
            }
            catch (Exception ex)
            {
                response.SetStatus(eResponseStatus.IllegalXml, "Error while loading file");
                log.ErrorFormat("Failed loading file: {0}. groupId:{1}. Exception: {2}", xml, groupId, ex);
                return response;
            }

            if (deserializeObject == null || !(deserializeObject is IngestVODFeed))
            {
                response.SetStatus(eResponseStatus.IllegalXml, "TODO - SET ERROR MSG");
                log.ErrorFormat("XML file with wrong format: {0}. groupId:{1}.", xml, groupId);
                return response;
            }

            var feed = deserializeObject as IngestVODFeed;
            if (feed == null || feed.Export == null || feed.Export.MediaList == null || feed.Export.MediaList.Count == 0)
            {
                response.SetStatus(eResponseStatus.IllegalXml);
                return response;
            }

            response.Object = feed;
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        private static MediaAsset CreateMediaAsset(int groupId, int mediaId, IngestMedia media, CatalogGroupCache cache, List<Tags> tags)
        {
            DateTime startDate = GetDateTimeFromString(media.Basic.Dates.Start, DateTime.UtcNow);
            DateTime endDate = GetDateTimeFromString(media.Basic.Dates.CatalogEnd, new DateTime(2099, 1, 1));

            string mediaType = media.Basic.MediaType;
            if (mediaId != 0)
            {
                var assetRespone = AssetManager.GetAsset(groupId, mediaId, eAssetTypes.MEDIA, true);
                if (assetRespone.HasObject() && assetRespone.Object is MediaAsset)
                {
                    mediaType = (assetRespone.Object as MediaAsset).MediaType.m_sTypeName;
                }
            }

            MediaAsset mediaAsset = new MediaAsset()
            {
                Id = mediaId,
                AssetType = eAssetTypes.MEDIA,
                MediaAssetType = MediaAssetType.Media,
                CoGuid = media.CoGuid,
                EntryId = media.EntryId,
                IsActive = StringUtils.TryConvertTo<bool>(media.IsActive),
                MediaType = new MediaType(mediaType, (int)cache.AssetStructsMapBySystemName[mediaType].Id),
                Name = GetMainLanguageValue(cache.DefaultLanguage.Code, media.Basic.Name),
                NamesWithLanguages = GetOtherLanguages(cache.DefaultLanguage.Code, media.Basic.Name),
                Description = GetMainLanguageValue(cache.DefaultLanguage.Code, media.Basic.Description),
                DescriptionsWithLanguages = GetOtherLanguages(cache.DefaultLanguage.Code, media.Basic.Description),
                StartDate = startDate,
                CatalogStartDate = GetDateTimeFromString(media.Basic.Dates.CatalogStart, startDate),
                EndDate = endDate,
                FinalEndDate = GetDateTimeFromString(media.Basic.Dates.End, endDate),
                GeoBlockRuleId = (int?)TvmRuleManager.GetGeoBlockRuleId(groupId, media.Basic.Rules.GeoBlockRule),
                DeviceRuleId = (int?)TvmRuleManager.GetDeviceRuleId(groupId, media.Basic.Rules.DeviceRule),
                Metas = GetMetasList(media.Structure, cache.DefaultLanguage.Code, cache),
                Tags = tags
                // TODO - ASK IRA IF SOMEONE SENT IT AND THE MEDIA IS NEW, NEED EXAMPLE
                //string sEpgIdentifier = GetNodeValue(ref theItem, "basic/epg_identifier");
            };

            return mediaAsset;
        }

        private static void AddTagsToTranslations(List<Tags> tags, int mediaId, bool isMediaExists, ref Dictionary<string, TagsTranslations> tagsTranslations)
        {
            var slimTags = new List<Tuple<string, string, LanguageContainer[]>>();
            var otherLanguagesIndex = 0;
            foreach (var tag in tags)
            {
                var translations = tag.Values != null && tag.Values.Count > 0 && tag.Values.Count > otherLanguagesIndex ? tag.Values[otherLanguagesIndex++] : null;
                slimTags.AddRange(tag.m_lValues.Select(x => new Tuple<string, string, LanguageContainer[]>(tag.m_oTagMeta.m_sName, x, translations)));
            }

            var existingTagsTranslations = new List<Tuple<string, LanguageContainer>>();
            foreach (var slimTag in slimTags)
            {
                var topicSystemName = slimTag.Item1;
                var defaultTagValue = slimTag.Item2;
                var translations = slimTag.Item3;

                var tagKey = TagsTranslations.GetKey(topicSystemName, defaultTagValue);
                if (tagsTranslations.ContainsKey(tagKey))
                {
                    tagsTranslations[tagKey].AssetsToInvalidate.Add(new KeyValuePair<int, bool>(mediaId, isMediaExists));
                    if (translations != null && translations.Length > 0)
                    {
                        existingTagsTranslations.AddRange(translations.Select(x => new Tuple<string, LanguageContainer>(tagKey, x)));
                    }
                }
                else
                {
                    tagsTranslations.Add(tagKey, new TagsTranslations(topicSystemName, defaultTagValue, translations, mediaId, isMediaExists));
                }
            }
            
            if (existingTagsTranslations.Count > 0)
            {
                foreach (var translation in existingTagsTranslations)
                {
                    var tagKey = translation.Item1;
                    var language = translation.Item2;
                    if (tagsTranslations[tagKey].Translations.ContainsKey(language.LanguageCode))
                    {
                        tagsTranslations[tagKey].Translations[language.LanguageCode].Value = language.Value;
                    }
                    else
                    {
                        tagsTranslations[tagKey].Translations.Add(language.LanguageCode, language);
                    }
                }
            }
        }
        
        private static void HandleTagsTranslations(Dictionary<string, TagsTranslations> tagsTranslations, int groupId, CatalogGroupCache cache, ref IngestResponse response)
        {
            var tagMetaType = MetaType.Tag.ToString();
            List<TagToInvalidate> tagsToInvalidate = new List<TagToInvalidate>();
            var defaultLanguageId = cache.DefaultLanguage.ID;

            foreach (var tag in tagsTranslations)
            {
                if (cache.TopicsMapBySystemNameAndByType.ContainsKey(tag.Value.TopicSystemName) &&
                    cache.TopicsMapBySystemNameAndByType[tag.Value.TopicSystemName].ContainsKey(tagMetaType))
                {
                    Topic topicTag = cache.TopicsMapBySystemNameAndByType[tag.Value.TopicSystemName][tagMetaType];
                    tag.Value.TopicId = topicTag.Id;
                    
                    var tagResponse = CatalogManager.SearchTags(groupId, true, tag.Value.DefaultTagValue, (int)topicTag.Id, defaultLanguageId, 0, 1);

                    if (!tagResponse.HasObjects())
                    {
                        tagsToInvalidate.Add(AddTagTranslations(groupId, tag.Value, defaultLanguageId, ref response));
                    }
                    else
                    {
                        tagsToInvalidate.Add(UpdateTagTranslations(groupId, tag.Value, defaultLanguageId, tagResponse.Objects[0], ref response));
                    }
                }
            }

            // invalidate assets and tags
            IndexAndInvalidateTags(groupId, tagsToInvalidate, cache);
        }

        private static TagToInvalidate AddTagTranslations(int groupId, TagsTranslations tag, int defaultLanguageId, ref IngestResponse response)
        {
            var tagToInvalidate = tag.GetTagToInvalidate(false, defaultLanguageId); 

            var addTagResponse = CatalogManager.AddTag(groupId, tagToInvalidate.TagValue, USER_ID, true);
            if (!addTagResponse.HasObject())
            {
                string errorMsg = string.Format("AddTagTranslations faild. topicName: {0}, topicId: {1}, tagValue: {2}, addTagStatus: {3}.",
                                                tag.TopicSystemName, tag.TopicId, tag.DefaultTagValue, addTagResponse.ToStringStatus());
                response.AddError(errorMsg);
                log.Debug(errorMsg);
                tagToInvalidate.TagValue.tagId = -1;
                return tagToInvalidate;
            }

            tagToInvalidate.TagValue.tagId = addTagResponse.Object.tagId;
            return tagToInvalidate;
        }

        private static TagToInvalidate UpdateTagTranslations(int groupId, TagsTranslations tag, int defaultLanguageId, TagValue oldTagValue, ref IngestResponse response)
        {
            oldTagValue.TagsInOtherLanguages.ForEach(x =>
            {
                if (!tag.Translations.ContainsKey(x.LanguageCode))
                {
                    tag.Translations.Add(x.LanguageCode, x);
                }
            });

            var tagToInvalidate = tag.GetTagToInvalidate(true, defaultLanguageId);
            tagToInvalidate.TagValue.tagId = oldTagValue.tagId;

            if (oldTagValue.IsNeedToUpdate(tagToInvalidate.TagValue))
            {
                var updateTagResponse = CatalogManager.UpdateTag(groupId, tagToInvalidate.TagValue, USER_ID, true);
                if (!updateTagResponse.HasObject() && updateTagResponse.Status.Code != (int)eResponseStatus.NoValuesToUpdate)
                {
                    string errorMsg = string.Format("UpdateTagTranslations faild. topicName: {0}, topicId: {1}, tagValue: {2}, tagId: {3}, updateTagStatus: {4}.",
                                                    tag.TopicSystemName, tag.TopicId, tag.DefaultTagValue, oldTagValue.tagId, updateTagResponse.ToStringStatus());
                    response.AddError(errorMsg);
                    log.Debug(errorMsg);
                    tagToInvalidate.TagValue.tagId = -1;
                }
            }
            else
            {
                tagToInvalidate.TagValue.tagId = -1;
            }

            return tagToInvalidate;
        }
        
        private static void IndexAndInvalidateTags(int groupId, List<TagToInvalidate> tagsToInvalidate, CatalogGroupCache catalogGroupCache)
        {
            var wrapper = new ElasticsearchWrapper();
            var assetsToInvalidate = new Dictionary<int, bool>();

            foreach (var tag in tagsToInvalidate)
            {
                tag.AssetsToInvalidate.ForEach(x =>
                {
                    if (!assetsToInvalidate.ContainsKey(x.Key))
                    {
                        assetsToInvalidate.Add(x.Key, x.Value);
                    }
                });

                // Index And Invalidate Tag only if tag Is Need To be Update (id > 0)
                if (tag.TagValue.tagId > 0)
                {
                    if (tag.IsTagExists)
                    {
                        // get all assets whom contains this tag for Index and Invalidate them
                        var ds = CatalogDAL.GetTagAssets(groupId, tag.TagValue.tagId);
                        List<int> mediaIds, epgIds;
                        CatalogManager.CreateAssetsListForUpdateIndexFromDataSet(ds, out mediaIds, out epgIds);
                        mediaIds.ForEach(x =>
                        {
                            if (!assetsToInvalidate.ContainsKey(x))
                            {
                                assetsToInvalidate.Add(x, true);
                            }
                        });
                    }

                    var result = wrapper.UpdateTag(groupId, catalogGroupCache, tag.TagValue);
                    if (!result.IsOkStatusCode())
                    {
                        log.ErrorFormat("Failed UpdateTag index for tag: {0}, groupId: {1}, error: {2} after IndexAndInvalidateTags", tag.TagValue.ToString(), groupId, result.ToString());
                    }
                }
            }

            IndexAndInvalidateAssets(groupId, assetsToInvalidate);
        }
        
        private static void IndexAndInvalidateAssets(int groupId, Dictionary<int, bool> assetsToInvalidate)
        {
            foreach (var asset in assetsToInvalidate)
            {
                if (!IndexManager.UpsertMedia(groupId, asset.Key))
                {
                    log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after IndexAndInvalidateAssets", asset.Key, groupId);
                }

                // if asset is exists
                if (asset.Value)
                {
                    AssetManager.InvalidateAsset(eAssetTypes.MEDIA, asset.Key);
                }
            }
        }

        private static bool DeleteMediaAsset(int mediaId, string coGuid, int groupId, ref IngestResponse ingestResponse, int mediaIndex)
        {
            if (mediaId == 0)
            {
                ingestResponse.AssetsStatus[mediaIndex].Warnings.Add(new Status((int)IngestWarnings.MediaIdNotExist, MEDIA_ID_NOT_EXIST));
                log.DebugFormat("DeleteMediaAsset - {0}", MEDIA_ID_NOT_EXIST);
                ingestResponse.Set(coGuid, "Cant delete. the item is not exist", "OK", mediaId);
                return false;
            }

            log.DebugFormat("Delete Media:{0}", mediaId);

            Status deleteStatus = AssetManager.DeleteAsset(groupId, mediaId, eAssetTypes.MEDIA, USER_ID);
            if (deleteStatus.Code != (int)eResponseStatus.OK)
            {
                ingestResponse.AssetsStatus[mediaIndex].Status = deleteStatus;
                log.Debug("DeleteMediaAsset - faild");
                ingestResponse.Set(coGuid, "DeleteAsset faild", "FAILED", mediaId);
                return false;
            }

            ingestResponse.Set(coGuid, "succeeded delete media", "OK", mediaId);
            ingestResponse.AssetsStatus[mediaIndex].InternalAssetId = mediaId;
            return true;
        }
        
        private static string GetMainLanguageValue(string mainLanguageName, IngestMultilingual multilingual)
        {
            if (multilingual != null && multilingual.Values != null && multilingual.Values.Count > 0)
            {
                var mainLanguage = multilingual.Values.FirstOrDefault(x => x.LangCode.Equals(mainLanguageName));
                if (mainLanguage != null)
                {
                    return mainLanguage.Text;
                }
            }

            return null;
        }

        private static List<LanguageContainer> GetOtherLanguages(string mainLanguageCode, IngestMultilingual multilingual)
        {
            if (multilingual != null && multilingual.Values != null && multilingual.Values.Count > 0)
            {
                return new List<LanguageContainer>(multilingual.Values.Where(x => !x.LangCode.Equals(mainLanguageCode)).Select(x => new LanguageContainer(x.LangCode, x.Text)));
            }

            return null;
        }

        private static List<Metas> GetMetasList(IngestStructure structure, string mainLanguageCode, CatalogGroupCache catalogGroupCache)
        {
            List<Metas> metas = new List<Metas>();

            // add metas-doubles
            if (structure.Doubles != null && structure.Doubles.Metas != null && structure.Doubles.Metas.Count > 0)
            {
                metas.AddRange(structure.Doubles.Metas.Select(doubleMeta => 
                    new Metas(new TagMeta(doubleMeta.Name, MetaType.Number.ToString()), doubleMeta.Value)));
            }

            // add metas-bools
            if (structure.Booleans != null && structure.Booleans.Metas != null && structure.Booleans.Metas.Count > 0)
            {
                metas.AddRange(structure.Booleans.Metas.Select
                    (boolMeta => new Metas(new TagMeta(boolMeta.Name, MetaType.Bool.ToString()), boolMeta.Value.Equals(TRUE) ? "1" : "0")));
            }

            // add metas-dates
            if (structure.Dates != null && structure.Dates.Metas != null && structure.Dates.Metas.Count > 0)
            {
                metas.AddRange(structure.Dates.Metas.Select
                    (dateMeta => new Metas(new TagMeta(dateMeta.Name, MetaType.DateTime.ToString()), dateMeta.Value)));
            }

            // add metas-strings
            if (structure.Strings != null && structure.Strings.MetaStrings != null && structure.Strings.MetaStrings.Count > 0)
            {
                foreach (var stringMeta in structure.Strings.MetaStrings)
                {
                    MetaType metaType = MetaType.MultilingualString;
                    if (catalogGroupCache.TopicsMapBySystemNameAndByType.ContainsKey(stringMeta.Name) &&
                        catalogGroupCache.TopicsMapBySystemNameAndByType[stringMeta.Name].Any(x => x.Key == MetaType.String.ToString() || x.Key == MetaType.MultilingualString.ToString()))
                    {
                        metaType = catalogGroupCache.TopicsMapBySystemNameAndByType[stringMeta.Name].First(x => x.Key == MetaType.String.ToString()
                                                                                                            || x.Key == MetaType.MultilingualString.ToString()).Value.Type;
                    }

                    metas.Add(new Metas(new TagMeta(stringMeta.Name, metaType.ToString()),
                                        stringMeta.Values.FirstOrDefault(x => x.LangCode.Equals(mainLanguageCode)).Text,
                                        stringMeta.Values.Where(x => !x.LangCode.Equals(mainLanguageCode))
                                                         .Select(x => new LanguageContainer(x.LangCode, x.Text))));
                }
            }

            return metas;
        }
        
        private static bool ValidateMedia(int mediaIndex, IngestMedia media, int groupId, CatalogGroupCache cache, int mediaId, ref IngestResponse response, out List<Tags> tags)
        {
            tags = null;
            if (string.IsNullOrEmpty(media.EntryId))
            {
                response.AssetsStatus[mediaIndex].Warnings.Add(new Status((int)IngestWarnings.MissingEntryId, MISSING_ENTRY_ID));
            }

            if (string.IsNullOrEmpty(media.Action))
            {
                response.AssetsStatus[mediaIndex].Warnings.Add(new Status((int)IngestWarnings.MissingAction, MISSING_ACTION));
            }

            if (string.IsNullOrEmpty(media.IsActive))
            {
                log.DebugFormat("ValidateMedia - media with no activation indication. co-guid: {0}.", media.CoGuid);
            }

            if (!media.Action.Equals(DELETE_ACTION))
            {
                if (media.Basic == null)
                {
                    string errMsg = string.Format(CANNOT_BE_EMPTY, "media.Basic");
                    response.AddError(errMsg);
                    response.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.NameRequired, errMsg);
                    return false;
                }

                // CHECK RULES
                if (media.Basic.Rules == null)
                {
                    response.AddError("media.Basic.Rules cannot be empty");
                    response.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.NameRequired, "media.Basic.Rules cannot be empty");
                    return false;
                }

                if (!string.IsNullOrEmpty(media.Basic.Rules.GeoBlockRule))
                {
                    var geoBlockRuleId = TvmRuleManager.GetGeoBlockRuleId(groupId, media.Basic.Rules.GeoBlockRule);
                    if (!geoBlockRuleId.HasValue || geoBlockRuleId.Value == 0)
                    {
                        response.AddError(GEO_BLOCK_RULE_NOT_RECOGNIZED);
                        log.DebugFormat("ValidateMedia - Geo block rule not recognized. mediaIndex:{0}, GeoBlockRule:{1}", mediaIndex, media.Basic.Rules.GeoBlockRule);
                        response.AssetsStatus[mediaIndex].Warnings.Add(new Status((int)IngestWarnings.NotRecognizedGeoBlockRule, GEO_BLOCK_RULE_NOT_RECOGNIZED));
                    }
                }

                if (!string.IsNullOrEmpty(media.Basic.Rules.DeviceRule))
                {
                    var deviceRuleId = TvmRuleManager.GetDeviceRuleId(groupId, media.Basic.Rules.DeviceRule);
                    if (!deviceRuleId.HasValue || deviceRuleId.Value == 0)
                    {
                        response.AddError(DEVICE_RULE_NOT_RECOGNIZED);
                        log.DebugFormat("ValidateMedia - Device rule not recognized. mediaIndex:{0}, DeviceRule:{1}", mediaIndex, media.Basic.Rules.DeviceRule);
                        response.AssetsStatus[mediaIndex].Warnings.Add(new Status((int)IngestWarnings.NotRecognizedDeviceRule, DEVICE_RULE_NOT_RECOGNIZED));
                    }
                }

                // check dates  
                if (media.Basic.Dates == null)
                {
                    response.AddError("media.Basic.Dates cannot be empty");
                    response.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.NameRequired, "media.Basic.Dates cannot be empty");
                    return false;
                }

                if (media.Structure == null)
                {
                    response.AddError("media.Structure cannot be empty");
                    response.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.NameRequired, "media.Structure cannot be empty");
                    return false;
                }

                // ingest action is Insert
                if (mediaId == 0)
                {
                    // check MediaType
                    if (string.IsNullOrEmpty(media.Basic.MediaType) ||
                        !cache.AssetStructsMapBySystemName.ContainsKey(media.Basic.MediaType) ||
                        cache.AssetStructsMapBySystemName[media.Basic.MediaType].Id == 0)
                    {
                        response.AddError(ITEM_TYPE_NOT_RECOGNIZED);
                        log.DebugFormat("ValidateMedia - {0}. co-guid:{1}", ITEM_TYPE_NOT_RECOGNIZED, media.CoGuid);
                        response.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.InvalidMediaType, string.Format("Invalid media type \"{0}\"", media.Basic.MediaType));
                        return false;
                    }

                    if (media.Basic.Name == null)
                    {
                        response.AddError("media.Basic.Name cannot be empty");
                        response.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.NameRequired, "media.Basic.Name cannot be empty");
                        return false;
                    }

                    Status nameValidationStatus = media.Basic.Name.Validate("media.basic.name", cache.DefaultLanguage.Code, cache.LanguageMapByCode);
                    if (!nameValidationStatus.IsOkStatusCode())
                    {
                        response.AddError(nameValidationStatus.Message);
                        response.AssetsStatus[mediaIndex].Status = nameValidationStatus;
                        return false;
                    }

                    if (media.Basic.Description == null)
                    {
                        response.AddError("media.Basic.Description cannot be empty");
                        response.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.NameRequired, "media.Basic.Description cannot be empty");
                        return false;
                    }

                    Status descriptionValidationStatus = media.Basic.Description.Validate("media.basic.description", cache.DefaultLanguage.Code, cache.LanguageMapByCode);
                    if (!descriptionValidationStatus.IsOkStatusCode())
                    {
                        response.AddError(descriptionValidationStatus.Message);
                        response.AssetsStatus[mediaIndex].Status = descriptionValidationStatus;
                        return false;
                    }

                    Status stringsValidationStatus = media.Structure.ValidateStrings(cache.TopicsMapBySystemNameAndByType, cache.DefaultLanguage.Code, cache.LanguageMapByCode);
                    if (!stringsValidationStatus.IsOkStatusCode())
                    {
                        response.AddError(stringsValidationStatus.Message);
                        response.AssetsStatus[mediaIndex].Status = stringsValidationStatus;
                        return false;
                    }

                    var metasValidation = media.Structure.ValidateMetaTags(cache.DefaultLanguage, cache.LanguageMapByCode);
                    if (!metasValidation.IsOkStatusCode())
                    {
                        response.AddError(metasValidation.Status.Message);
                        response.AssetsStatus[mediaIndex].Status = metasValidation.Status;
                        return false;
                    }
                    tags = metasValidation.Objects;

                    if (!TRUE.Equals(media.IsActive) && !FALSE.Equals(media.IsActive))
                    {
                        response.AddError("media.IsActive cannot be empty");
                        response.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.NameRequired, "media.IsActive cannot be empty");
                        return false;
                    }
                }
                // ingest action is Update
                else
                {
                    if (media.Basic.Name != null && media.Basic.Name.Values != null && media.Basic.Name.Values.Count > 0)
                    {
                        // TODO - SET METHOD COMMON FOR ALL ERROS
                        Status nameValidationStatus = media.Basic.Name.Validate("media.basic.name", cache.DefaultLanguage.Code, cache.LanguageMapByCode);
                        if (!nameValidationStatus.IsOkStatusCode())
                        {
                            response.AddError(nameValidationStatus.Message);
                            response.AssetsStatus[mediaIndex].Status = nameValidationStatus;
                            return false;
                        }
                    }

                    if (media.Basic.Description != null && media.Basic.Description.Values != null && media.Basic.Description.Values.Count > 0)
                    {
                        Status descriptionValidationStatus = media.Basic.Description.Validate("media.basic.description", cache.DefaultLanguage.Code, cache.LanguageMapByCode);
                        if (!descriptionValidationStatus.IsOkStatusCode())
                        {
                            response.AddError(descriptionValidationStatus.Message);
                            response.AssetsStatus[mediaIndex].Status = descriptionValidationStatus;
                            return false;
                        }
                    }

                    Status stringsValidationStatus = media.Structure.ValidateStrings(cache.TopicsMapBySystemNameAndByType, cache.DefaultLanguage.Code, cache.LanguageMapByCode);
                    if (!stringsValidationStatus.IsOkStatusCode())
                    {
                        response.AddError(stringsValidationStatus.Message);
                        response.AssetsStatus[mediaIndex].Status = stringsValidationStatus;
                        return false;
                    }

                    var metasValidation = media.Structure.ValidateMetaTags(cache.DefaultLanguage, cache.LanguageMapByCode);
                    if (!metasValidation.IsOkStatusCode())
                    {
                        response.AddError(metasValidation.Status.Message);
                        response.AssetsStatus[mediaIndex].Status = metasValidation.Status;
                        return false;
                    }
                    tags = metasValidation.Objects;
                }
            }

            return true;
        }
        
        private static DateTime GetDateTimeFromString(string date, DateTime defaultDate)
        {
            try
            {
                if (string.IsNullOrEmpty(date))
                {
                    return defaultDate;
                }

                string sTime = "";
                string[] timeHour = date.Split(' ');
                if (timeHour.Length == 2)
                {
                    date = timeHour[0];
                    sTime = timeHour[1];
                }
                else
                    return DateTime.UtcNow;
                string[] splited = date.Split('/');

                Int32 nYear = 1;
                Int32 nMounth = 1;
                Int32 nDay = 1;
                Int32 nHour = 0;
                Int32 nMin = 0;
                Int32 nSec = 0;
                nYear = int.Parse(splited[2].ToString());
                nMounth = int.Parse(splited[1].ToString());
                nDay = int.Parse(splited[0].ToString());
                if (timeHour.Length == 2)
                {
                    string[] splited1 = sTime.Split(':');
                    nHour = int.Parse(splited1[0].ToString());
                    nMin = int.Parse(splited1[1].ToString());
                    nSec = int.Parse(splited1[2].ToString());
                }

                return new DateTime(nYear, nMounth, nDay, nHour, nMin, nSec);
            }
            catch
            {
                return DateTime.UtcNow;
            }
        }
        
        // TODO - use good method
        private static int GetBillingTypeIdByName(string billingTypeName)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            try
            {
                selectQuery += "select id from lu_billing_type where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(API_VAL)))", "=", billingTypeName.Trim().ToLower());
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        return int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    }
                }
            }
            catch (Exception)
            {
                log.ErrorFormat("GetBillingIdByName failed for billingName: {0}.", billingTypeName);
            }
            finally
            {
                selectQuery.Finish();
                selectQuery = null;
            }

            return 0;
        }
        
        private static Dictionary<long, Image> GetImages(IngestBasic basic, int groupId, string groupDefaultRatio, Dictionary<string, ImageType> groupRatioNamesToImageTypes)
        {
            Dictionary<long, Image> images = null;

            if (groupRatioNamesToImageTypes != null && groupRatioNamesToImageTypes.Count > 0)
            {
                if (basic.Thumb != null && !string.IsNullOrEmpty(basic.Thumb.Url) && !string.IsNullOrEmpty(groupDefaultRatio) && groupRatioNamesToImageTypes.ContainsKey(groupDefaultRatio))
                {
                    Image image = new Image()
                    {
                        Url = basic.Thumb.Url,
                        ImageTypeId = groupRatioNamesToImageTypes[groupDefaultRatio].Id
                    };

                    images = new Dictionary<long, Image>() { { image.ImageTypeId, image } };
                }

                if (basic.PicsRatio != null && basic.PicsRatio.Ratios != null && basic.PicsRatio.Ratios.Count > 0)
                {
                    foreach (var ratio in basic.PicsRatio.Ratios)
                    {
                        if (groupRatioNamesToImageTypes.ContainsKey(ratio.RatioText))
                        {
                            if (images == null)
                            {
                                images = new Dictionary<long, Image>();
                            }

                            long imageTypeId = groupRatioNamesToImageTypes[ratio.RatioText].Id;

                            if (images.ContainsKey(imageTypeId))
                            {
                                images[imageTypeId].Url = ratio.Thumb;
                            }
                            else
                            {
                                images.Add(imageTypeId, new Image() { ImageTypeId = imageTypeId, Url = ratio.Thumb });
                            }
                        }
                    }
                }
            }

            return images;
        }

        private static Dictionary<int, Tuple<AssetFile, string>> GetAssetFiles(IngestFiles files, GenericListResponse<MediaFileType> mediaFileTypes, Dictionary<string, CDNAdapter> cdnAdapters)
        {
            Dictionary<int, Tuple<AssetFile, string>> assetFiles = null;

            if (files != null && files.MediaFiles != null && files.MediaFiles.Count > 0 && mediaFileTypes != null && mediaFileTypes.HasObjects())
            {
                foreach (var mediaFile in files.MediaFiles)
                {
                    var mediaFileType = mediaFileTypes.Objects.FirstOrDefault(x => x.Name.Equals(mediaFile.Type));
                    if (mediaFileType != null)
                    {
                        if (assetFiles == null)
                        {
                            assetFiles = new Dictionary<int, Tuple<AssetFile, string>>();
                        }

                        int mediaFileTypeId = (int)mediaFileType.Id;
                        if (!assetFiles.ContainsKey(mediaFileTypeId))
                        {
                            assetFiles.Add(mediaFileTypeId, new Tuple<AssetFile, string>(new AssetFile(mediaFile.Type)
                            {
                                //Id
                                //AssetId
                                TypeId = mediaFileTypeId,
                                Url = mediaFile.CdnCode,
                                Duration = StringUtils.TryConvertTo<long>(mediaFile.AssetDuration),
                                ExternalId = mediaFile.CoGuid,
                                AltExternalId = mediaFile.AltCoGuid,
                                ExternalStoreId = mediaFile.ProductCode,
                                CdnAdapaterProfileId = !string.IsNullOrEmpty(mediaFile.CdnName) && cdnAdapters.ContainsKey(mediaFile.CdnName) ? cdnAdapters[mediaFile.CdnName].ID : (long?)null,
                                AltStreamingCode = mediaFile.AltCdnCode,
                                AlternativeCdnAdapaterProfileId = !string.IsNullOrEmpty(mediaFile.AltCdnName) && cdnAdapters.ContainsKey(mediaFile.AltCdnName) ? cdnAdapters[mediaFile.AltCdnName].ID : (long?)null,
                                //AdditionalData
                                BillingType = GetBillingTypeIdByName(mediaFile.BillingType),
                                //OrderNum
                                Language = mediaFile.Language,
                                IsDefaultLanguage = StringUtils.TryConvertTo<bool>(mediaFile.IsDefaultLanguage),
                                OutputProtecationLevel = StringUtils.ConvertTo<int>(mediaFile.OutputProtecationLevel),
                                StartDate = DateUtils.TryExtractDate(mediaFile.FileStartDate, ASSET_FILE_DATE_FORMAT),
                                EndDate = DateUtils.TryExtractDate(mediaFile.FileEndDate, ASSET_FILE_DATE_FORMAT),
                                FileSize = StringUtils.TryConvertTo<long>(mediaFile.FileSize),
                                IsActive = true,
                                CatalogEndDate = DateUtils.TryExtractDate(mediaFile.FileCatalogEndDate, ASSET_FILE_DATE_FORMAT),
                            }, mediaFile.PpvModule));
                        }
                    }
                }
            }

            return assetFiles;
        }

        private static Dictionary<string, CDNAdapter> GetCDNAdaptersMapping(int groupId)
        {
            Dictionary<string, CDNAdapter> cdnAdapterMapping = new Dictionary<string, CDNAdapter>();
            var cdnAdapterList = DAL.ApiDAL.GetCDNAdapters(groupId);

            foreach (var cdnAdapter in cdnAdapterList)
            {
                if (!cdnAdapterMapping.ContainsKey(cdnAdapter.Name))
                {
                    cdnAdapterMapping.Add(cdnAdapter.Name, cdnAdapter);
                }
            }

            return cdnAdapterMapping;
        }
    }
}