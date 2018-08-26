using ApiObjects;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Data;
using Tvinci.Core.DAL;
using System.Linq;
using ApiObjects.Catalog;
using TVinciShared;
using Core.Catalog.Cache;

namespace Core.Catalog.CatalogManagement
{
    public class IngestManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
        // TODO SHIR - ADD MORE CONSTS ERRORS 

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
            string notifyXml = string.Empty;

            IngestResponse ingestResponse = ConvertToMediaAssets(xml, groupId);
            
            if (ingestResponse == null)
            {
                log.Warn("For input " + xml + " response is empty");
                return new IngestResponse() { Status = "ERROR" };
            }

            if (ingestResponse.IngestStatus == null || ingestResponse.IngestStatus.Code == (int)eResponseStatus.Error)
            {
                // TODO SHIR - SET SOME ERROR
            }

            log.DebugFormat("End HandleMediaIngest. groupId:{0}", groupId);

            return ingestResponse;
        }

        private static IngestResponse ConvertToMediaAssets(string xml, int groupId)
        {
            IngestResponse ingestResponse = new IngestResponse()
            {
                IngestStatus = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() },
                AssetsStatus = new List<IngestAssetStatus>(),
                Description = string.Empty
            };

            IngestFeed feed = DeserializeXmlToFeed(xml, groupId, ref ingestResponse);
            if (feed == null || feed.Export == null || feed.Export.MediaList == null || feed.Export.MediaList.Count == 0 ||
                ingestResponse.IngestStatus.Code == (int)eResponseStatus.IllegalXml)
            {
                return ingestResponse;
            }

            CatalogGroupCache catalogGroupCache;
            if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling ConvertToMediaAssets", groupId);
                return ingestResponse;
            }
            
            // get data for group
            GenericListResponse<MediaFileType> mediaFileTypes = FileManager.GetMediaFileTypes(groupId);
            string groupDefaultRatio = groupDefaultRatio = ImageUtils.GetGroupDefaultRatioName(groupId);
            Dictionary<string, ImageType> groupRatioNamesToImageTypes = GetGroupRatioNamesToImageTypes(groupId);
            string mainLanguageCode = string.Empty;
            HashSet<string> groupLanguageCodes = GetGroupLanguageCodes(groupId, out mainLanguageCode);
            Dictionary<string, Dictionary<string, Dictionary<string, string>>> tagsTranslations = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            for (int i = 0; i < feed.Export.MediaList.Count; i++)
            {
                IngestMedia media = feed.Export.MediaList[i];
                ingestResponse.AssetsStatus.Add(new IngestAssetStatus()
                {
                    Warnings = new List<Status>(),
                    Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() }
                });

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
                    int mediaId = GetMediaIdByCoGuid(groupId, media.CoGuid);

                    if (ValidateMedia(i, media, groupId, catalogGroupCache, mediaId, mainLanguageCode, groupLanguageCodes, ref ingestResponse))
                    {
                        if (media.Action.Equals(DELETE_ACTION))
                        {
                            if (!DeleteMediaAsset(mediaId, media.CoGuid, groupId, ref ingestResponse, i))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            DateTime startDate = GetDateTimeFromString(media.Basic.Dates.Start, DateTime.UtcNow);
                            DateTime endDate = GetDateTimeFromString(media.Basic.Dates.CatalogEnd, new DateTime(2099, 1, 1));

                            // CREATE mediaAsset
                            MediaAsset mediaAsset = new MediaAsset()
                            {
                                Id = mediaId,
                                AssetType = eAssetTypes.MEDIA,
                                MediaAssetType = MediaAssetType.Media,
                                CoGuid = media.CoGuid,
                                EntryId = media.EntryId,
                                IsActive = StringUtils.TryConvertTo<bool>(media.IsActive),
                                MediaType = new MediaType(media.Basic.MediaType, (int)catalogGroupCache.AssetStructsMapBySystemName[media.Basic.MediaType].Id),
                                Name = GetMainLanguageValue(mainLanguageCode, media.Basic.Name),
                                NamesWithLanguages = GetOtherLanguages(mainLanguageCode, media.Basic.Name),
                                Description = GetMainLanguageValue(mainLanguageCode, media.Basic.Description),
                                DescriptionsWithLanguages = GetOtherLanguages(mainLanguageCode, media.Basic.Description),
                                StartDate = startDate,
                                CatalogStartDate = GetDateTimeFromString(media.Basic.Dates.CatalogStart, startDate),
                                EndDate = endDate,
                                FinalEndDate = GetDateTimeFromString(media.Basic.Dates.End, endDate),
                                GeoBlockRuleId = CatalogLogic.GetGeoBlockRuleId(groupId, media.Basic.Rules.GeoBlockRule),
                                DeviceRuleId = CatalogLogic.GetDeviceRuleId(groupId, media.Basic.Rules.DeviceRule),
                                Metas = GetMetasList(media.Structure, mainLanguageCode, catalogGroupCache),
                                Tags = GetTagsList(media.Structure.Metas, mainLanguageCode, ref tagsTranslations)
                            };

                            // TODO SHIR - ASK IRA IF SOMEONE SENT IT AND THE MEDIA IS NEW, NEED EXAMPLE
                            //string sEpgIdentifier = GetNodeValue(ref theItem, "basic/epg_identifier");

                            if (mediaAsset.Id == 0)
                            {
                                if (!InsertMediaAsset(mediaAsset, media, groupId, mediaFileTypes, groupDefaultRatio, groupRatioNamesToImageTypes, ref ingestResponse, i))
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                if (!UpdateMediaAsset(mediaAsset, media, groupId, mediaFileTypes, !FALSE.Equals(media.Erase), groupDefaultRatio, groupRatioNamesToImageTypes, ref ingestResponse, i))
                                {
                                    continue;
                                }
                            }
                            
                            // UpdateIndex
                            bool indexingResult = IndexManager.UpsertMedia(groupId, (int)mediaAsset.Id);
                            if (!indexingResult)
                            {
                                log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after Ingest", mediaAsset.Id, groupId);
                            }
                        }

                        // TODO SHIR - ASK IRA ABOUT THIS
                        // Update record in Catalog (see the flow inside Update Index
                        //change eAction.Delete
                        //if (ImporterImpl.UpdateIndex(new List<int>() { nMediaID }, nParentGroupID, eAction.Update))
                        //{
                        //    log.DebugFormat("UpdateIndex: Succeeded. CoGuid:{0}, MediaID:{1}, isActive:{2}, ErrorMessage:{3}", sCoGuid, nMediaID, isActive.ToString(), sErrorMessage);
                        //}
                        //else
                        //{
                        //    log.ErrorFormat("UpdateIndex: Failed. CoGuid:{0}, MediaID:{1}, isActive:{2}, ErrorMessage:{3}", sCoGuid, nMediaID, isActive.ToString(), sErrorMessage);
                        //    ingestAssetStatus.Warnings.Add(new Status() { Code = (int)IngestWarnings.UpdateIndexFailed, Message = UPDATE_INDEX_FAILED });
                        //}

                        //// update notification 
                        //if (mediaAsset.IsActive.HasValue && mediaAsset.IsActive.Value)
                        //{
                        //    UpdateNotificationsRequests(groupId, mediaAsset.Id);
                        //}

                        // succeeded import media
                        ingestResponse.AssetsStatus[i].Status.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
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
                    string errorMsg = string.Format("Exception while ConvertToMediaAssets for mediaIndex: {0}, Exception:{1}", i, ex);
                    log.Error(errorMsg);
                    ingestResponse.AssetsStatus[i].Status.Set((int)eResponseStatus.Error, errorMsg);
                }
            }
            
            if (ingestResponse.AssetsStatus.All(x => x.Status != null && x.Status.Code == (int)eResponseStatus.OK))
            {
                ingestResponse.IngestStatus.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            HandleTagsTranslations(tagsTranslations, groupId, mainLanguageCode, catalogGroupCache);
                
            return ingestResponse;
        }

        private static void HandleTagsTranslations(Dictionary<string, Dictionary<string, Dictionary<string, string>>> tagsTranslations, int groupId, string mainLanguageCode, 
                                                   CatalogGroupCache catalogGroupCache)
        {
            foreach (var topic in tagsTranslations)
            {
                // tagsTranslation.Key == Genre
                if (catalogGroupCache.TopicsMapBySystemName.ContainsKey(topic.Key))
                {
                    Topic catalogTopic = catalogGroupCache.TopicsMapBySystemName[topic.Key];

                    foreach (var tag in topic.Value)
                    {
                        //topic.Key == drama
                        var tagValues = CatalogManager.SearchTags(groupId, true, tag.Key, (int)catalogTopic.Id, 0, 0, 0);

                        if (tagValues == null || !tagValues.HasObjects())
                        {
                            ApiObjects.SearchObjects.TagValue tagValueToAdd = new ApiObjects.SearchObjects.TagValue()
                            {
                                topicId = (int)catalogTopic.Id,
                                value = tag.Key,
                                languageId = catalogGroupCache.LanguageMapByCode[mainLanguageCode].ID,
                                TagsInOtherLanguages = new List<LanguageContainer>(tag.Value.Select(x => new LanguageContainer(x.Key, x.Value)))
                            };

                            CatalogManager.AddTag(groupId, tagValueToAdd, USER_ID);
                        }
                        else
                        {
                            foreach (var otherLanguage in tag.Value)
                            {
                                int otherLanguageIndex = tagValues.Objects[0].TagsInOtherLanguages.FindIndex(x => x.LanguageCode.Equals(otherLanguage.Key));
                                if (otherLanguageIndex == -1)
                                {
                                    tagValues.Objects[0].TagsInOtherLanguages.Add(new LanguageContainer(otherLanguage.Key, otherLanguage.Value));
                                }
                                else
                                {
                                    tagValues.Objects[0].TagsInOtherLanguages[otherLanguageIndex].Value = otherLanguage.Value;
                                }
                            }

                            CatalogManager.UpdateTag(groupId, catalogTopic.Id, tagValues.Objects[0], USER_ID);
                        }
                    }
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

        private static bool InsertMediaAsset(MediaAsset mediaAsset, IngestMedia media, int groupId, GenericListResponse<MediaFileType> mediaFileTypes, string groupDefaultRatio, 
                                             Dictionary<string, ImageType> groupRatioNamesToImageTypes, ref IngestResponse ingestResponse, int mediaIndex)
        {
            GenericResponse<Asset> genericResponse = AssetManager.AddAsset(groupId, eAssetTypes.MEDIA, mediaAsset, USER_ID, true);
            if (genericResponse.Status.Code != (int)eResponseStatus.OK)
            {
                ingestResponse.AssetsStatus[mediaIndex].Status = genericResponse.Status;
                log.Debug("InsertMediaAsset - AddAsset faild");
                ingestResponse.Set(mediaAsset.CoGuid, "AddAsset faild", "FAILED", 0);
                return false; ;
            }

            mediaAsset.Id = genericResponse.Object.Id;
            
            if (UpsertMediaAssetImagesAndFiles(false, mediaAsset.Id, true, groupId, media, groupDefaultRatio, groupRatioNamesToImageTypes, mediaFileTypes, ref ingestResponse, mediaIndex))
            {
                ingestResponse.Set(mediaAsset.CoGuid, "succeeded insert media", "OK", (int)mediaAsset.Id);
                ingestResponse.AssetsStatus[mediaIndex].InternalAssetId = (int)mediaAsset.Id;
                return true;
            }

            return false;
        }
        
        private static bool UpdateMediaAsset(MediaAsset mediaAsset, IngestMedia media, int groupId, GenericListResponse<MediaFileType> mediaFileTypes, bool eraseMedia, string groupDefaultRatio,
                                             Dictionary<string, ImageType> groupRatioNamesToImageTypes, ref IngestResponse ingestResponse, int mediaIndex)
        {
            if (eraseMedia)
            {
                if (!AssetManager.ClearAsset(groupId, mediaAsset.Id, eAssetTypes.MEDIA, USER_ID))
                {
                    // TODO SHIR - SET SOME ERROR
                }
            }
            
            GenericResponse<Asset> genericResponse = AssetManager.UpdateAsset(groupId, mediaAsset.Id, eAssetTypes.MEDIA, mediaAsset, USER_ID, true);
            if (genericResponse.Status.Code != (int)eResponseStatus.OK)
            {
                ingestResponse.AssetsStatus[mediaIndex].Status = genericResponse.Status;
                log.Debug("UpdateMediaAsset - UpdateAsset faild");
                ingestResponse.Set(mediaAsset.CoGuid, "UpdateAsset faild", "FAILED", (int)mediaAsset.Id);
                return false;
            }

            mediaAsset.Id = genericResponse.Object.Id;

            if (UpsertMediaAssetImagesAndFiles(true, mediaAsset.Id, eraseMedia, groupId, media, groupDefaultRatio, groupRatioNamesToImageTypes, mediaFileTypes, ref ingestResponse, mediaIndex))
            {
                ingestResponse.Set(mediaAsset.CoGuid, "succeeded update media", "OK", (int)mediaAsset.Id);
                ingestResponse.AssetsStatus[mediaIndex].InternalAssetId = (int)mediaAsset.Id;
                return true;
            }

            return false;
        }

        private static bool UpsertMediaAssetImagesAndFiles(bool isUpdateRequest, long mediaAssetId, bool eraseExistingFiles, int groupId, IngestMedia media, string groupDefaultRatio,
                                                           Dictionary<string, ImageType> groupRatioNamesToImageTypes, GenericListResponse<MediaFileType> mediaFileTypes,
                                                           ref IngestResponse ingestResponse, int mediaIndex)
        {
            var images = GetImages(media.Basic, groupId, groupDefaultRatio, groupRatioNamesToImageTypes);
            if (images != null && images.Count > 0)
            {
                HandleAssetImages(groupId, mediaAssetId, eAssetImageType.Media, images, isUpdateRequest, ref ingestResponse, mediaIndex);
            }

            var assetFiles = GetAssetFiles(media.Files, mediaFileTypes);
            if (assetFiles != null && assetFiles.Count > 0)
            {
                HandleAssetFiles(groupId, mediaAssetId, assetFiles, eraseExistingFiles, ref ingestResponse, mediaIndex);
            }

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

        private static HashSet<string> GetGroupLanguageCodes(int groupId, out string mainLanguageCode)
        {
            HashSet<string> groupLanguageCodes = null;
            mainLanguageCode = string.Empty;

            var groupLanguages = Api.Module.GetGroupLanguages(groupId);
            if (groupLanguages != null && groupLanguages.Count > 0)
            {
                groupLanguageCodes = new HashSet<string>(groupLanguages.Select(x => x.Code));
                mainLanguageCode = groupLanguages.FirstOrDefault(x => x.IsDefault).Code;
            }

            return groupLanguageCodes;
        }
        
        private static Dictionary<string, ImageType> GetGroupRatioNamesToImageTypes(int groupId)
        {
            Dictionary<string, ImageType> groupRatioNamesToImageTypes = null;
           
            GenericListResponse<ImageType> imageTypes = ImageManager.GetImageTypes(groupId, false, null);
            if (imageTypes != null && imageTypes.HasObjects())
            {
                var groupRatios = ImageManager.GetRatios(groupId);

                if (groupRatios != null && groupRatios.HasObjects())
                {
                    groupRatioNamesToImageTypes = new Dictionary<string, ImageType>();

                    foreach (var imageType in imageTypes.Objects)
                    {
                        var currRatio = groupRatios.Objects.FirstOrDefault(x => imageType.RatioId.HasValue && imageType.RatioId.Value == x.Id);
                        if (currRatio != null && !groupRatioNamesToImageTypes.ContainsKey(currRatio.Name))
                        {
                            groupRatioNamesToImageTypes.Add(currRatio.Name, imageType);
                        }
                    }
                }
            }
            
            return groupRatioNamesToImageTypes;
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
            List<Metas> metas = null;

            // add metas-doubles
            if (structure.Doubles != null && structure.Doubles.Metas != null && structure.Doubles.Metas.Count > 0)
            {
                metas = new List<Metas>(structure.Doubles.Metas.Select
                    (doubleMeta => new Metas(new TagMeta(doubleMeta.Name, MetaType.Number.ToString()), doubleMeta.Value)));
            }

            // add metas-bools
            if (structure.Booleans != null && structure.Booleans.Metas != null && structure.Booleans.Metas.Count > 0)
            {
                if (metas == null)
                {
                    metas = new List<Metas>();
                }
                
                metas.AddRange(structure.Booleans.Metas.Select
                    (boolMeta => new Metas(new TagMeta(boolMeta.Name, MetaType.Bool.ToString()), boolMeta.Value.Equals(TRUE) ? "1" : "0")));
            }

            // add metas-dates
            if (structure.Dates != null && structure.Dates.Metas != null && structure.Dates.Metas.Count > 0)
            {
                if (metas == null)
                {
                    metas = new List<Metas>();
                }

                metas.AddRange(structure.Dates.Metas.Select
                    (dateMeta => new Metas(new TagMeta(dateMeta.Name, MetaType.DateTime.ToString()), dateMeta.Value)));
            }

            // add metas-strings
            if (structure.Strings != null && structure.Strings.MetaStrings != null && structure.Strings.MetaStrings.Count > 0)
            {
                foreach (var stringMeta in structure.Strings.MetaStrings)
                {
                    if (catalogGroupCache.TopicsMapBySystemName.ContainsKey(stringMeta.Name))
                    {
                        if (metas == null)
                        {
                            metas = new List<Metas>();
                        }

                        metas.Add(new Metas(new TagMeta(stringMeta.Name, catalogGroupCache.TopicsMapBySystemName[stringMeta.Name].Type.ToString()),
                                            stringMeta.Values.FirstOrDefault(x => x.LangCode.Equals(mainLanguageCode)).Text,
                                            stringMeta.Values.Where(x => !x.LangCode.Equals(mainLanguageCode))
                                                             .Select(x => new LanguageContainer(x.LangCode, x.Text))));
                    }
                }
            }

            return metas;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metas"></param>
        /// <param name="mainLanguageCode"></param>
        /// <param name="tagsTranslations">{Genre, {Drama, {[heb, דרמה], [jap, ドラマ]}, }}</param>
        /// <returns></returns>
        private static List<Tags> GetTagsList(IngestMetas metas, string mainLanguageCode, ref Dictionary<string, Dictionary<string, Dictionary<string, string>>> tagsTranslations)
        {
            List<Tags> tags = null;

            if (metas != null && metas.MetaTags != null && metas.MetaTags.Count > 0)
            {
                // add metas-tags
                foreach (var metaTag in metas.MetaTags)
                {
                    List<string> finalMainValues = new List<string>();

                    if (!tagsTranslations.ContainsKey(metaTag.Name))
                    {
                        tagsTranslations.Add(metaTag.Name, new Dictionary<string, Dictionary<string, string>>());
                    }
                    
                    foreach (var container in metaTag.Containers)
                    {
                        Dictionary<string, string> translations = new Dictionary<string, string>();

                        foreach (var langValue in container.Values)
                        {
                            if (string.IsNullOrEmpty(langValue.Text))
                            {
                                continue;
                            }

                            if (mainLanguageCode.Equals(langValue.LangCode))
                            {
                                if (!tagsTranslations[metaTag.Name].ContainsKey(langValue.Text) && langValue.Text.IndexOf(';') == -1)
                                {
                                    tagsTranslations[metaTag.Name].Add(langValue.Text, translations);
                                }
                                
                                finalMainValues.AddRange(langValue.Text.Split(';'));           
                            }
                            else
                            {
                                translations.Add(langValue.LangCode, langValue.Text);
                            }
                        }
                    }
                    
                    if (finalMainValues.Count > 0)
                    {
                        if (tags == null)
                        {
                            tags = new List<Tags>();
                        }

                        tags.Add(new Tags() { m_oTagMeta = new TagMeta(metaTag.Name, MetaType.Tag.ToString()), m_lValues = finalMainValues });
                    }
                }
            }

            return tags;
        }

        private static bool ValidateMedia(int mediaIndex, IngestMedia media, int groupId, CatalogGroupCache catalogGroupCache,
                                          int mediaId, string mainLanguageName, HashSet<string> groupLanguageCodes, ref IngestResponse ingestResponse)
        {
            if (string.IsNullOrEmpty(media.EntryId))
            {
                ingestResponse.AssetsStatus[mediaIndex].Warnings.Add(new Status((int)IngestWarnings.MissingEntryId, MISSING_ENTRY_ID));
            }

            if (string.IsNullOrEmpty(media.Action))
            {
                ingestResponse.AssetsStatus[mediaIndex].Warnings.Add(new Status((int)IngestWarnings.MissingAction, MISSING_ACTION));
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
                    ingestResponse.AddError(errMsg);
                    ingestResponse.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.NameRequired, errMsg);
                    return false;
                }

                // check MediaType
                if (string.IsNullOrEmpty(media.Basic.MediaType) ||
                    !catalogGroupCache.AssetStructsMapBySystemName.ContainsKey(media.Basic.MediaType) ||
                    catalogGroupCache.AssetStructsMapBySystemName[media.Basic.MediaType].Id == 0)
                {
                    ingestResponse.AddError("Item type not recognized");
                    log.DebugFormat("ValidateMedia - Item type not recognized. co-guid:{0}", media.CoGuid);
                    ingestResponse.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.InvalidMediaType, string.Format("Invalid media type \"{0}\"", media.Basic.MediaType));
                    return false;
                }

                // CHECK RULES
                if (media.Basic.Rules == null)
                {
                    ingestResponse.AddError("media.Basic.Rules cannot be empty");
                    ingestResponse.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.NameRequired, "media.Basic.Rules cannot be empty");
                    return false;
                }

                if (!string.IsNullOrEmpty(media.Basic.Rules.GeoBlockRule))
                {
                    int? geoBlockRuleId = CatalogLogic.GetGeoBlockRuleId(groupId, media.Basic.Rules.GeoBlockRule);
                    if (!geoBlockRuleId.HasValue || geoBlockRuleId.Value == 0)
                    {
                        ingestResponse.AddError(GEO_BLOCK_RULE_NOT_RECOGNIZED);
                        log.DebugFormat("ValidateMedia - Geo block rule not recognized. mediaIndex:{0}, GeoBlockRule:{1}", mediaIndex, media.Basic.Rules.GeoBlockRule);
                        ingestResponse.AssetsStatus[mediaIndex].Warnings.Add(new Status((int)IngestWarnings.NotRecognizedGeoBlockRule, GEO_BLOCK_RULE_NOT_RECOGNIZED));
                    }
                }

                if (!string.IsNullOrEmpty(media.Basic.Rules.DeviceRule))
                {
                    int? deviceRuleId = CatalogLogic.GetDeviceRuleId(groupId, media.Basic.Rules.DeviceRule);
                    if (!deviceRuleId.HasValue || deviceRuleId.Value == 0)
                    {
                        ingestResponse.AddError(DEVICE_RULE_NOT_RECOGNIZED);
                        log.DebugFormat("ValidateMedia - Device rule not recognized. mediaIndex:{0}, DeviceRule:{1}", mediaIndex, media.Basic.Rules.DeviceRule);
                        ingestResponse.AssetsStatus[mediaIndex].Warnings.Add(new Status((int)IngestWarnings.NotRecognizedDeviceRule, DEVICE_RULE_NOT_RECOGNIZED));
                    }
                }

                // check dates  
                if (media.Basic.Dates == null)
                {
                    ingestResponse.AddError("media.Basic.Dates cannot be empty");
                    ingestResponse.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.NameRequired, "media.Basic.Dates cannot be empty");
                    return false;
                }

                if (media.Structure == null)
                {
                    ingestResponse.AddError("media.Structure cannot be empty");
                    ingestResponse.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.NameRequired, "media.Structure cannot be empty");
                    return false;
                }

                // ingest action is Insert
                if (mediaId == 0)
                {
                    if (media.Basic.Name == null)
                    {
                        ingestResponse.AddError("media.Basic.Name cannot be empty");
                        ingestResponse.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.NameRequired, "media.Basic.Name cannot be empty");
                        return false;
                    }

                    Status nameValidationStatus = media.Basic.Name.Validate("media.basic.name", mainLanguageName, groupLanguageCodes);
                    if (nameValidationStatus != null && nameValidationStatus.Code != (int)eResponseStatus.OK)
                    {
                        ingestResponse.AddError(nameValidationStatus.Message);
                        ingestResponse.AssetsStatus[mediaIndex].Status = nameValidationStatus;
                        return false;
                    }

                    if (media.Basic.Description != null && media.Basic.Description.Values != null && media.Basic.Description.Values.Count == 0)
                    {
                        ingestResponse.AddError("media.Basic.Description cannot be empty");
                        ingestResponse.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.NameRequired, "media.Basic.Description cannot be empty");
                        return false;
                    }

                    if (media.Basic.Description != null)
                    {
                        Status descriptionValidationStatus = media.Basic.Description.Validate("media.basic.description", mainLanguageName, groupLanguageCodes);
                        if (descriptionValidationStatus != null && descriptionValidationStatus.Code != (int)eResponseStatus.OK)
                        {
                            ingestResponse.AddError(descriptionValidationStatus.Message);
                            ingestResponse.AssetsStatus[mediaIndex].Status = descriptionValidationStatus;
                            return false;
                        }
                    }
                    
                    Status stringsValidationStatus = media.Structure.ValidateStrings(catalogGroupCache.TopicsMapBySystemName, mainLanguageName, groupLanguageCodes);
                    if (stringsValidationStatus != null && stringsValidationStatus.Code != (int)eResponseStatus.OK)
                    {
                        ingestResponse.AddError(stringsValidationStatus.Message);
                        ingestResponse.AssetsStatus[mediaIndex].Status = stringsValidationStatus;
                        return false;
                    }

                    Status metasValidationStatus = media.Structure.ValidateMetaTags(mainLanguageName, groupLanguageCodes);
                    if (metasValidationStatus != null && metasValidationStatus.Code != (int)eResponseStatus.OK)
                    {
                        ingestResponse.AddError(metasValidationStatus.Message);
                        ingestResponse.AssetsStatus[mediaIndex].Status = metasValidationStatus;
                        return false;
                    }

                    if (!media.IsActive.Equals(TRUE) && !media.IsActive.Equals(FALSE))
                    {
                        ingestResponse.AddError("media.IsActive cannot be empty");
                        ingestResponse.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.NameRequired, "media.IsActive cannot be empty");
                        return false;
                    }
                }
                // ingest action is Update
                else
                {
                    if (media.Basic.Name != null)
                    {
                        // TODO SHIR - SET METHOD COMMON FOR ALL ERROS
                        Status nameValidationStatus = media.Basic.Name.Validate("media.basic.name", mainLanguageName, groupLanguageCodes);
                        if (nameValidationStatus != null && nameValidationStatus.Code != (int)eResponseStatus.OK)
                        {
                            ingestResponse.AddError(nameValidationStatus.Message);
                            ingestResponse.AssetsStatus[mediaIndex].Status = nameValidationStatus;
                            return false;
                        }
                    }

                    if (media.Basic.Description != null)
                    {
                        Status descriptionValidationStatus = media.Basic.Description.Validate("media.basic.description", mainLanguageName, groupLanguageCodes);
                        if (descriptionValidationStatus != null && descriptionValidationStatus.Code != (int)eResponseStatus.OK)
                        {
                            ingestResponse.AddError(descriptionValidationStatus.Message);
                            ingestResponse.AssetsStatus[mediaIndex].Status = descriptionValidationStatus;
                            return false;
                        }
                    }
                    
                    Status stringsValidationStatus = media.Structure.ValidateStrings(catalogGroupCache.TopicsMapBySystemName, mainLanguageName, groupLanguageCodes);
                    if (stringsValidationStatus != null && stringsValidationStatus.Code != (int)eResponseStatus.OK)
                    {
                        ingestResponse.AddError(stringsValidationStatus.Message);
                        ingestResponse.AssetsStatus[mediaIndex].Status = stringsValidationStatus;
                        return false;
                    }

                    Status metasValidationStatus = media.Structure.ValidateMetaTags(mainLanguageName, groupLanguageCodes);
                    if (metasValidationStatus != null && metasValidationStatus.Code != (int)eResponseStatus.OK)
                    {
                        ingestResponse.AddError(metasValidationStatus.Message);
                        ingestResponse.AssetsStatus[mediaIndex].Status = metasValidationStatus;
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        private static IngestFeed DeserializeXmlToFeed(string xml, int groupId, ref IngestResponse ingestResponse)
        {
            Object deserializeObject = null;

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(IngestFeed));
                using (StringReader stringReader = new StringReader(xml))
                {
                    using (XmlTextReader xmlReader = new XmlTextReader(stringReader))
                    {
                        deserializeObject = xmlSerializer.Deserialize(xmlReader);
                    }
                }
            }
            catch (XmlException ex)
            {
                ingestResponse.IngestStatus.Set((int)eResponseStatus.IllegalXml, "XML file with wrong format");
                log.ErrorFormat("XML file with wrong format: {0}. groupId:{1}. Exception: {2}", xml, groupId, ex);
                return null;
            }
            catch (Exception ex)
            {
                ingestResponse.IngestStatus.Set((int)eResponseStatus.IllegalXml, "Error while loading file");
                log.ErrorFormat("Failed loading file: {0}. groupId:{1}. Exception: {2}", xml, groupId, ex);
                return null;
            }

            if (deserializeObject == null || !(deserializeObject is IngestFeed))
            {
                ingestResponse.IngestStatus.Set((int)eResponseStatus.IllegalXml, "TOEDO SHIE SET ERROR MSG");
                log.ErrorFormat("XML file with wrong format: {0}. groupId:{1}.", xml, groupId);
                return null;
            }

            return deserializeObject as IngestFeed;
        }

        private static int GetMediaIdByCoGuid(int groupId, string coGuid)
        {
            DataTable existingExternalIdsDt = CatalogDAL.ValidateExternalIdsExist(groupId, new List<string>() { coGuid });
            if (existingExternalIdsDt != null && existingExternalIdsDt.Rows != null && existingExternalIdsDt.Rows.Count > 0)
            {
                DataRow dr = existingExternalIdsDt.Rows[0];
                return ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
            }

            return 0;
        }

        // TODO SHIR - CHECK IF METHOD IS CURRECT
        public static DateTime GetDateTimeFromString(string date, DateTime defaultDate)
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
                    return DateTime.Now;
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
                return DateTime.Now;
            }
        }
        
        public static DateTime? ExtractDate(string dateTime, string format)
        {
            DateTime result;
            if (DateTime.TryParseExact(dateTime, format, null, System.Globalization.DateTimeStyles.None, out result))
            {
                return result;
            }

            return null;
        }
        
        // TODO SHIR - use good method
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
        
        /// <summary>
        /// Handle asset files for ingest
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <param name="assetId">the asset id</param>
        /// <param name="filesToHandle">the files of the asset that need handling</param>
        /// <param name="eraseExistingFiles">should erase asset existing files, relevant only when updating existing asset</param>
        /// <returns></returns>
        private static void HandleAssetFiles(int groupId, long assetId, List<Tuple<AssetFile, string>> filesToHandle, bool eraseExistingFiles, ref IngestResponse ingestResponse, int index)
        {
            // TODO SHIR - UPDATE PREFORMENCE HandleAssetImages
            List<Tuple<AssetFile, string>> filesToAdd = null;
            List<Tuple<AssetFile, string>> filesToUpdate = null;
            try
            {
                // get the files we need to update
                if (assetId > 0 && !eraseExistingFiles)
                {
                    GenericListResponse<AssetFile> assetFilesResponse = FileManager.GetMediaFiles(groupId, 0, assetId);
                    HashSet<string> filesToHandleExternalIds = new HashSet<string>(filesToHandle.Select(x => x.Item1.ExternalId), StringComparer.OrdinalIgnoreCase);
                    Dictionary<string, long> updateFilesExternalIdToId = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
                    if (assetFilesResponse != null && assetFilesResponse.HasObjects())
                    {
                        if (assetFilesResponse.Objects.Any(x => filesToHandleExternalIds.Contains(x.ExternalId)))
                        {
                            updateFilesExternalIdToId = assetFilesResponse.Objects.Where(x => filesToHandleExternalIds.Contains(x.ExternalId)).ToDictionary(x => x.ExternalId,
                                                                                                                                                    x => x.Id, StringComparer.OrdinalIgnoreCase);
                            filesToUpdate = filesToHandle.Where(x => updateFilesExternalIdToId.ContainsKey(x.Item1.ExternalId)).ToList();

                            if (filesToUpdate != null && filesToUpdate.Count > 0)
                            {
                                // handle files to update
                                foreach (var assetFileToUpdate in filesToUpdate)
                                {
                                    assetFileToUpdate.Item1.Id = updateFilesExternalIdToId[assetFileToUpdate.Item1.ExternalId];
                                    assetFileToUpdate.Item1.AssetId = assetId;

                                    GenericResponse<AssetFile> updateFileResponse = FileManager.UpdateMediaFile(groupId, assetFileToUpdate.Item1, USER_ID, true);
                                    if (updateFileResponse == null || !updateFileResponse.HasObject() || updateFileResponse.Object.Id == 0)
                                    {
                                        log.ErrorFormat("Failed updating asset file with externalId {0}, fileId: {1} for asset with id {2}, groupId: {3}",
                                                        assetFileToUpdate.Item1.ExternalId, assetFileToUpdate.Item1.Id, assetId, groupId);

                                        updateFileResponse.Status.Args = new List<KeyValuePair>()
                                        {
                                            new KeyValuePair("assetFileToUpdate.ExternalId", assetFileToUpdate.Item1.ExternalId),
                                            new KeyValuePair("assetFileToUpdate.Id", assetFileToUpdate.Item1.Id.ToString()),
                                            new KeyValuePair("assetFileToUpdate.TypeId", assetFileToUpdate.Item1.TypeId.HasValue ? assetFileToUpdate.Item1.TypeId.Value.ToString() : string.Empty),
                                        };
                                        ingestResponse.AssetsStatus[index].Warnings.Add(updateFileResponse.Status);
                                        continue;
                                    }

                                    HandleAssetFilePpvModels(assetFileToUpdate.Item2, groupId, assetFileToUpdate.Item1.Id, assetFileToUpdate.Item1.AssetId);
                                }
                            }
                        }
                    }

                    filesToAdd = filesToHandle.Where(x => !updateFilesExternalIdToId.ContainsKey(x.Item1.ExternalId)).ToList();
                }
                else
                {
                    filesToAdd = filesToHandle;
                }

                // handle files to add
                if (filesToAdd != null && filesToAdd.Count > 0)
                {
                    foreach (var assetFileToAdd in filesToAdd)
                    {
                        assetFileToAdd.Item1.AssetId = assetId;
                        GenericResponse<AssetFile> addFileResponse = FileManager.InsertMediaFile(groupId, USER_ID, assetFileToAdd.Item1, true);
                        if (addFileResponse == null || !addFileResponse.HasObject() || addFileResponse.Object.Id == 0)
                        {
                            log.ErrorFormat("Failed adding asset file with externalId {0} for asset with id {1}, groupId: {2}", assetFileToAdd.Item1.ExternalId, assetId, groupId);
                            addFileResponse.Status.Args = new List<KeyValuePair>()
                            {
                                new KeyValuePair("assetFileToAdd.ExternalId", assetFileToAdd.Item1.ExternalId),
                                new KeyValuePair("assetFileToAdd.TypeId", assetFileToAdd.Item1.TypeId.HasValue ? assetFileToAdd.Item1.TypeId.Value.ToString() : string.Empty),
                            };
                            ingestResponse.AssetsStatus[index].Warnings.Add(addFileResponse.Status);
                            continue;
                        }

                        HandleAssetFilePpvModels(assetFileToAdd.Item2, groupId, addFileResponse.Object.Id, assetFileToAdd.Item1.AssetId);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = string.Format("Exception while HandleAssetFiles for assetId: {0}", assetId);
                log.Error(errorMsg, ex);
                ingestResponse.AssetsStatus[index].Warnings.Add(new Status((int)eResponseStatus.Error, errorMsg));
            }
        }

        /// <summary>
        /// Handle asset images for ingest
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <param name="assetId">the asset id</param>
        /// <param name="imagesToHandle">the images of the asset that need handling</param>
        /// <param name="isUpdateRequest">is the request for updating the asset</param>
        /// <returns></returns>
        private static void HandleAssetImages(int groupId, long assetId, eAssetImageType imageObjectType, Dictionary<long, Image> imagesToHandle, bool isUpdateRequest, 
                                              ref IngestResponse ingestResponse, int index)
        {
            // TODO SHIR - UPDATE PREFORMENCE HandleAssetImages
            List<Image> imagesToAdd = null;
            List<Image> imagesToUpdate = new List<Image>();

            try
            {
                // get the images we need to update
                if (assetId > 0 && isUpdateRequest)
                {
                    GenericListResponse<Image> assetImagesResponse = ImageManager.GetImagesByObject(groupId, assetId, imageObjectType);
                    Dictionary<long, long> imageTypeIdsToIdsMapToUpdate = new Dictionary<long, long>();
                    if (assetImagesResponse != null && assetImagesResponse.HasObjects())
                    {
                        if (assetImagesResponse.Objects.Any(x => imagesToHandle.ContainsKey(x.ImageTypeId)))
                        {
                            imageTypeIdsToIdsMapToUpdate = assetImagesResponse.Objects.Where(x => imagesToHandle.ContainsKey(x.ImageTypeId)).ToDictionary(x => x.ImageTypeId, x => x.Id);
                            imagesToUpdate = imagesToHandle.Where(x => imageTypeIdsToIdsMapToUpdate.ContainsKey(x.Key)).Select(x => x.Value).ToList();
                            foreach (Image image in imagesToUpdate)
                            {
                                image.Id = imageTypeIdsToIdsMapToUpdate[image.ImageTypeId];
                                image.ImageObjectId = assetId;
                            }
                        }
                    }

                    imagesToAdd = imagesToHandle.Where(x => !imageTypeIdsToIdsMapToUpdate.ContainsKey(x.Key)).Select(x => x.Value).ToList();
                }
                else
                {
                    imagesToAdd = imagesToHandle.Values.ToList();
                }

                // handle images to add
                if (imagesToAdd != null && imagesToAdd.Count > 0)
                {
                    foreach (var imageToAdd in imagesToAdd)
                    {
                        imageToAdd.ImageObjectId = assetId;
                        GenericResponse<Image> addImageResponse = ImageManager.AddImage(groupId, imageToAdd, USER_ID);
                        if (addImageResponse == null || !addImageResponse.HasObject() || addImageResponse.Object.Id == 0)
                        {
                            log.ErrorFormat("Failed adding image with imageTypeId {0} for asset with id {1}, groupId: {2}", imageToAdd.ImageTypeId, assetId, groupId);
                            
                            addImageResponse.Status.Args = new List<KeyValuePair>()
                            {
                                new KeyValuePair("imageToAdd.Id", imageToAdd.Id.ToString()),
                                new KeyValuePair("imageToAdd.Url", imageToAdd.Url),
                            };

                            ingestResponse.AssetsStatus[index].Warnings.Add(addImageResponse.Status);
                        }
                        else
                        {
                            imageToAdd.Id = addImageResponse.Object.Id;
                            imagesToUpdate.Add(imageToAdd);
                        }
                    }
                }

                // handle images to update
                if (imagesToUpdate != null && imagesToUpdate.Count > 0)
                {
                    foreach (Image imageToUpdate in imagesToUpdate)
                    {
                        Status setContentResponse = ImageManager.SetContent(groupId, USER_ID, imageToUpdate.Id, imageToUpdate.Url);
                        if (setContentResponse == null || !setContentResponse.IsOkStatusCode())
                        {
                            log.ErrorFormat("Failed setContent for image with Id {0}, ImageTypeId {1} for assetId {2} and groupId: {3}",
                                             imageToUpdate.Id, imageToUpdate.ImageTypeId, assetId, groupId);
                            
                            setContentResponse.Args = new List<KeyValuePair>()
                            {
                                new KeyValuePair("imageToUpdate.Id", imageToUpdate.Id.ToString()),
                                new KeyValuePair("imageToUpdate.Url", imageToUpdate.Url),
                            };

                            ingestResponse.AssetsStatus[index].Warnings.Add(setContentResponse);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = string.Format("Exception while HandleAssetImages for assetId: {0}", assetId);
                log.Error(errorMsg, ex);
                ingestResponse.AssetsStatus[index].Warnings.Add(new Status((int)eResponseStatus.Error, errorMsg));
            }
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

                if (basic.PicsRatio.Ratios != null && basic.PicsRatio.Ratios.Count > 0)
                {
                    foreach (var ratio in basic.PicsRatio.Ratios)
                    {
                        if (groupRatioNamesToImageTypes.ContainsKey(ratio.RatioText))
                        {
                            if (images == null)
                            {
                                images = new Dictionary<long, Image>();
                            }

                            if (images.ContainsKey(groupRatioNamesToImageTypes[ratio.RatioText].Id))
                            {
                                images[groupRatioNamesToImageTypes[ratio.RatioText].Id].Url = ratio.Thumb;
                            }
                            else
                            {
                                images.Add(groupRatioNamesToImageTypes[ratio.RatioText].Id, new Image() { ImageTypeId = groupRatioNamesToImageTypes[ratio.RatioText].Id, Url = ratio.Thumb });
                            }
                        }
                    }
                }
            }

            return images;
        }

        private static List<Tuple<AssetFile, string>> GetAssetFiles(IngestFiles files, GenericListResponse<MediaFileType> mediaFileTypes)
        {
            List<Tuple<AssetFile, string>> assetFiles = null;

            if (files != null && files.MediaFiles != null && files.MediaFiles.Count > 0 && mediaFileTypes != null && mediaFileTypes.HasObjects())
            {
                foreach (var mediaFile in files.MediaFiles)
                {
                    var mediaFileType = mediaFileTypes.Objects.FirstOrDefault(x => x.Name.Equals(mediaFile.Type));
                    if (mediaFileType != null)
                    {
                        if (assetFiles == null)
                        {
                            assetFiles = new List<Tuple<AssetFile, string>>();
                        }
                        
                        assetFiles.Add(new Tuple<AssetFile, string>(new AssetFile(mediaFile.Type)
                        {
                            TypeId = (int)mediaFileType.Id,
                            Url = mediaFile.CdnCode,
                            Duration = StringUtils.TryConvertTo<long>(mediaFile.AssetDuration),
                            ExternalId = mediaFile.CoGuid,
                            AltExternalId = mediaFile.AltCoGuid,
                            ExternalStoreId = mediaFile.ProductCode,
                            AltStreamingCode = mediaFile.AltCdnCode,
                            BillingType = GetBillingTypeIdByName(mediaFile.BillingType),
                            Language = mediaFile.Language,
                            IsDefaultLanguage = StringUtils.TryConvertTo<bool>(mediaFile.IsDefaultLanguage),
                            OutputProtecationLevel = StringUtils.ConvertTo<int>(mediaFile.OutputProtecationLevel),
                            StartDate = ExtractDate(mediaFile.FileStartDate, ASSET_FILE_DATE_FORMAT),
                            EndDate = ExtractDate(mediaFile.FileEndDate, ASSET_FILE_DATE_FORMAT),
                            FileSize = StringUtils.TryConvertTo<long>(mediaFile.FileSize),
                            IsActive = true
                        }, mediaFile.PpvModule));
                    }
                }
            }

            return assetFiles;
        }

        private static void HandleAssetFilePpvModels(string ppvModuleName, int groupId, long assetFileId, long assetId)
        {
            if (!string.IsNullOrEmpty(ppvModuleName))
            {
                if (ppvModuleName.Contains(";"))
                {
                    string ParsedPPVModuleName = string.Empty;
                    string[] parameters = ppvModuleName.Split(';');

                    for (int i = 0; i < parameters.Length; i += 3)
                    {
                        int ppvID = GetPPVModuleID(parameters[i], groupId);

                        if (ppvID <= 0)
                        {
                            continue;
                        }

                        DateTime? ppvStartDate = ExtractDate(parameters[i + 1], ASSET_FILE_DATE_FORMAT);
                        DateTime? ppvEndDate = ExtractDate(parameters[i + 2], ASSET_FILE_DATE_FORMAT);

                        DateTime? prevPPVFileStartDate = null;
                        DateTime? prevPPVFileEndDate = null;

                        if (ppvStartDate.HasValue && ppvEndDate.HasValue)
                        {
                            DataRow updatedppvModuleMediaFileDetails = 
                                ODBCWrapper.Utils.GetTableSingleRowColumnsByParamValue("ppv_modules_media_files", 
                                                                                       "media_file_id", 
                                                                                       assetFileId.ToString(), 
                                                                                       new List<string>() { "start_date", "end_date" }, 
                                                                                       "pricing_connection");

                            prevPPVFileStartDate = ODBCWrapper.Utils.GetNullableDateSafeVal(updatedppvModuleMediaFileDetails, "start_date");
                            prevPPVFileEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(updatedppvModuleMediaFileDetails, "end_date");
                        }

                        if (InsertFilePPVModule(ppvID, assetFileId, groupId, ppvStartDate, ppvEndDate, (i == 0)))
                        {
                            // check if changes in the start date require future index update call, incase ppvStartDate is in more than 2 years we don't update the index (per Ira's request)
                            if (RabbitHelper.IsFutureIndexUpdate(prevPPVFileStartDate, ppvStartDate))
                            {
                                if (!RabbitHelper.InsertFreeItemsIndexUpdate(groupId, eObjectType.Media, new List<int>() { (int)assetId }, ppvStartDate.Value))
                                {
                                    log.Error(string.Format("Failed inserting free items index update for startDate: {0}, mediaID: {1}, groupID: {2}", ppvStartDate.Value, assetId, groupId));
                                }
                            }

                            // check if changes in the end date require future index update call, incase ppvEndDate is in more than 2 years we don't update the index (per Ira's request)
                            if (RabbitHelper.IsFutureIndexUpdate(prevPPVFileEndDate, ppvStartDate))
                            {
                                if (!RabbitHelper.InsertFreeItemsIndexUpdate(groupId, eObjectType.Media, new List<int>() { (int)assetId }, ppvEndDate.Value))
                                {
                                    log.Error(string.Format("Failed inserting free items index update for endDate: {0}, mediaID: {1}, groupID: {2}", ppvEndDate.Value, assetId, groupId));
                                }
                            }
                        }
                    }
                }
                else
                {
                    int ppvID = GetPPVModuleID(ppvModuleName, groupId);
                    InsertFilePPVModule(ppvID, assetFileId, groupId, null, null, true);
                }
            }
        }

        // TODO SHIR - use good method
        static private int GetPPVModuleID(string moduleName, int groupId)
        {
            int nRet = 0;

            if (string.IsNullOrEmpty(moduleName))
            {
                return 0;
            }
            
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery += "select id from ppv_modules where IS_ACTIVE = 1 and STATUS = 1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Name", "=", moduleName);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupId);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCopunt = selectQuery.Table("query").DefaultView.Count;
                if (nCopunt > 0)
                    nRet = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", 0);
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        // TODO SHIR - use good method
        static private bool InsertFilePPVModule(int ppvModule, long fileID, int groupId, DateTime? startDate, DateTime? endDate, bool clear)
        {
            bool res = false;
            if (ppvModule == 0)
            {
                return res;
            }

            //First initialize all previous entries.
            if (clear)
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("ppv_modules_media_files");
                updateQuery.SetConnectionKey("pricing_connection");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 0);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 0);
                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", fileID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }

            int ppvFileID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery += "select id from ppv_modules_media_files (nolock) where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", fileID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PPV_MODULE_ID", "=", ppvModule);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    ppvFileID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            //If doesnt exist - create new entry
            if (ppvFileID == 0)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ppv_modules_media_files");
                insertQuery.SetConnectionKey("pricing_connection");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", fileID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PPV_MODULE_ID", "=", ppvModule);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupId);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);

                if (startDate.HasValue)
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", startDate);
                }

                if (endDate.HasValue)
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", endDate);
                }

                res = insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
            else
            {
                //Update status of previous entry
                ODBCWrapper.UpdateQuery updateOldQuery = new ODBCWrapper.UpdateQuery("ppv_modules_media_files");
                updateOldQuery.SetConnectionKey("pricing_connection");
                updateOldQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
                updateOldQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                updateOldQuery += ODBCWrapper.Parameter.NEW_PARAM("PPV_MODULE_ID", "=", ppvModule);

                if (startDate.HasValue)
                {
                    updateOldQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", startDate);
                }

                if (endDate.HasValue)
                {
                    updateOldQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", endDate);
                }

                updateOldQuery += "where";
                updateOldQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", ppvFileID);
                res = updateOldQuery.Execute();
                updateOldQuery.Finish();
                updateOldQuery = null;
            }

            return res;
        }
    }
}