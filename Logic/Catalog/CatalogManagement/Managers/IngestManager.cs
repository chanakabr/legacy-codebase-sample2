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
            
            if (ingestResponse == null || string.IsNullOrEmpty(ingestResponse.Description))
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

            string mainLanguageName = GetMainLanguageName(groupId);
            if (string.IsNullOrEmpty(mainLanguageName))
            {
                // TODO SHIR - SET SOME ERROR
            }

            // get data for group
            GenericListResponse<ImageType> imageTypes = ImageManager.GetImageTypes(groupId, false, null);
            GenericListResponse<MediaFileType> mediaFileTypes = FileManager.GetMediaFileTypes(groupId);
            int groupDefaultRatioId = ImageUtils.GetGroupDefaultRatio(groupId);
            Dictionary<string, int> groupRatios = GetGroupRatios(groupId);
            HashSet<string> groupLanguageCodes = GetGroupLanguageCodes(groupId);

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

                    if (ValidateMedia(i, media, groupId, catalogGroupCache, mediaId, mainLanguageName, groupLanguageCodes, ref ingestResponse))
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
                                IsActive = StringUtils.ConvertTo<bool>(media.IsActive),
                                MediaType = new MediaType(media.Basic.MediaType, (int)catalogGroupCache.AssetStructsMapBySystemName[media.Basic.MediaType].Id),
                                Name = GetMainLanguageValue(mainLanguageName, media.Basic.Name),
                                NamesWithLanguages = GetOtherLanguages(mainLanguageName, media.Basic.Name),
                                Description = GetMainLanguageValue(mainLanguageName, media.Basic.Description),
                                DescriptionsWithLanguages = GetOtherLanguages(mainLanguageName, media.Basic.Description),
                                StartDate = startDate,
                                CatalogStartDate = GetDateTimeFromString(media.Basic.Dates.CatalogStart, startDate),
                                EndDate = endDate,
                                FinalEndDate = GetDateTimeFromString(media.Basic.Dates.End, endDate),
                                GeoBlockRuleId = CatalogLogic.GetGeoBlockRuleId(groupId, media.Basic.Rules.GeoBlockRule),
                                DeviceRuleId = CatalogLogic.GetDeviceRuleId(groupId, media.Basic.Rules.DeviceRule),
                                Metas = GetMetasList(media.Structure, mainLanguageName, catalogGroupCache),
                                Tags = GetTagsList(media.Structure.Metas, mainLanguageName),
                            };

                            // TODO SHIR - ASK IRA IF SOMEONE SENT IT AND THE MEDIA IS NEW, NEED EXAMPLE
                            //string sEpgIdentifier = GetNodeValue(ref theItem, "basic/epg_identifier");

                            if (mediaAsset.Id == 0)
                            {
                                if (!InsertMediaAsset(mediaAsset, media, groupId, imageTypes, mediaFileTypes, groupDefaultRatioId, 
                                                      groupRatios, ref ingestResponse, i))
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                if (!UpdateMediaAsset(mediaAsset, media, groupId, imageTypes, mediaFileTypes, !FALSE.Equals(media.Erase), 
                                                      groupDefaultRatioId, groupRatios, ref ingestResponse, i))
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
                    log.ErrorFormat("Converting media from xml to MediaAsset Failed. mediaIndex: {0}, Exception:{1}", i, ex);
                }
            }

            if (ingestResponse.AssetsStatus.All(x => x.Status != null && x.Status.Code == (int)eResponseStatus.OK))
            {
                ingestResponse.IngestStatus.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return ingestResponse;
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

        private static bool InsertMediaAsset(MediaAsset mediaAsset, IngestMedia media, int groupId, GenericListResponse<ImageType> imageTypes, GenericListResponse<MediaFileType> mediaFileTypes, 
                                             int groupDefaultRatioId, Dictionary<string, int> groupRatios, ref IngestResponse ingestResponse, int mediaIndex)
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
            
            if (UpsertMediaAssetImagesAndFiles(false, mediaAsset.Id, true, groupId, media, imageTypes, groupDefaultRatioId, groupRatios, mediaFileTypes, ref ingestResponse, mediaIndex))
            {
                ingestResponse.Set(mediaAsset.CoGuid, "succeeded insert media", "OK", (int)mediaAsset.Id);
                ingestResponse.AssetsStatus[mediaIndex].InternalAssetId = (int)mediaAsset.Id;
                return true;
            }

            return false;
        }
        
        private static bool UpdateMediaAsset(MediaAsset mediaAsset, IngestMedia media, int groupId, GenericListResponse<ImageType> imageTypes, GenericListResponse<MediaFileType> mediaFileTypes, 
                                             bool eraseMedia, int groupDefaultRatioId, Dictionary<string, int> groupRatios, ref IngestResponse ingestResponse, int mediaIndex)
        {
            if (eraseMedia)
            {
                if (!AssetManager.ClearAsset(groupId, mediaAsset.Id, eAssetTypes.MEDIA, USER_ID))
                {
                    // TODO SHIR - SET SOME ERROR
                }
            }
            
            GenericResponse<Asset> genericResponse = AssetManager.UpdateAsset(groupId, mediaAsset.Id, eAssetTypes.MEDIA, mediaAsset, USER_ID);
            if (genericResponse.Status.Code != (int)eResponseStatus.OK)
            {
                ingestResponse.AssetsStatus[mediaIndex].Status = genericResponse.Status;
                log.Debug("UpdateMediaAsset - UpdateAsset faild");
                ingestResponse.Set(mediaAsset.CoGuid, "UpdateAsset faild", "FAILED", (int)mediaAsset.Id);
                return false;
            }

            mediaAsset.Id = genericResponse.Object.Id;

            if (UpsertMediaAssetImagesAndFiles(true, mediaAsset.Id, eraseMedia, groupId, media, imageTypes, groupDefaultRatioId, groupRatios, mediaFileTypes, ref ingestResponse, mediaIndex))
            {
                ingestResponse.Set(mediaAsset.CoGuid, "succeeded update media", "OK", (int)mediaAsset.Id);
                ingestResponse.AssetsStatus[mediaIndex].InternalAssetId = (int)mediaAsset.Id;
                return true;
            }

            return false;
        }

        private static bool UpsertMediaAssetImagesAndFiles(bool isUpdateRequest, long mediaAssetId, bool eraseExistingFiles, int groupId, IngestMedia media, GenericListResponse<ImageType> imageTypes,
                                                          int groupDefaultRatioId, Dictionary<string, int> groupRatios, GenericListResponse<MediaFileType> mediaFileTypes,
                                                          ref IngestResponse ingestResponse, int mediaIndex)
        {
            var images = GetImages(media.Basic, groupId, imageTypes, groupDefaultRatioId, groupRatios);
            if (images != null && images.Count > 0)
            {
                Status handleAssetImagesStatus = HandleAssetImages(groupId, mediaAssetId, eAssetImageType.Media, images, isUpdateRequest);
                if (handleAssetImagesStatus != null && handleAssetImagesStatus.Code != (int)eResponseStatus.OK)
                {
                    // TODO SHIR - ASK IRA IF NEED TO STOP HERE - RETURN FALSE
                    ingestResponse.AssetsStatus[mediaIndex].Warnings.Add(handleAssetImagesStatus);
                }
            }
            else
            {
                // TODO SHIR - SET SOME ERROR
            }

            var assetFiles = GetAssetFiles(media.Files, mediaFileTypes);
            if (assetFiles != null && assetFiles.Count > 0)
            {
                Status HandleAssetFilesStatus = HandleAssetFiles(groupId, mediaAssetId, assetFiles, eraseExistingFiles);
                if (HandleAssetFilesStatus != null && HandleAssetFilesStatus.Code != (int)eResponseStatus.OK)
                {
                    // TODO SHIR - ASK IRA IF NEED TO STOP HERE - RETURN FALSE
                    ingestResponse.AssetsStatus[mediaIndex].Warnings.Add(HandleAssetFilesStatus);
                }
            }
            else
            {
                // TODO SHIR - SET SOME ERROR
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

        private static HashSet<string> GetGroupLanguageCodes(int groupId)
        {
            HashSet<string> groupLanguageCodes = null;

            var groupLanguages = Api.Module.GetGroupLanguages(groupId);
            if (groupLanguages != null && groupLanguages.Count > 0)
            {
                groupLanguageCodes = new HashSet<string>(groupLanguages.Select(x => x.Code));
            }

            return groupLanguageCodes;
        }

        private static Dictionary<string, int> GetGroupRatios(int groupId)
        {
            Dictionary<string, int> groupRatios = null;

            var ratios = CatalogCache.Instance().GetGroupRatios(groupId);
            if (ratios != null && ratios.Count > 0)
            {
                groupRatios = new Dictionary<string, int>(ratios.Count);
                foreach (var ratio in ratios)
                {
                    if (!groupRatios.ContainsKey(ratio.Name))
                    {
                        groupRatios.Add(ratio.Name, ratio.Id);
                    }
                }
            }

            return groupRatios;
        }

        private static List<LanguageContainer> GetOtherLanguages(string mainLanguageName, IngestMultilingual multilingual)
        {
            if (multilingual != null && multilingual.Values != null && multilingual.Values.Count > 0)
            {
                return new List<LanguageContainer>(multilingual.Values.Where(x => !x.LangCode.Equals(mainLanguageName)).Select(x => new LanguageContainer(x.LangCode, x.Text)));
            }

            return null;
        }
        
        private static List<Metas> GetMetasList(IngestStructure structure, string mainLanguageName, CatalogGroupCache catalogGroupCache)
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
                    (boolMeta => new Metas(new TagMeta(boolMeta.Name, MetaType.Bool.ToString()), boolMeta.Value)));
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
                    Topic topic = catalogGroupCache.TopicsMapBySystemName[stringMeta.Name];

                    if (topic != null)
                    {
                        if (metas == null)
                        {
                            metas = new List<Metas>();
                        }
                        
                        metas.Add(new Metas(new TagMeta(stringMeta.Name, topic.Type.ToString()),
                                            stringMeta.Values.FirstOrDefault(x => x.LangCode.Equals(mainLanguageName)).Text,
                                            stringMeta.Values.Where(x => !x.LangCode.Equals(mainLanguageName))
                                                             .Select(x => new LanguageContainer(x.LangCode, x.Text))));
                    }
                }
            }

            return metas;
        }

        private static List<Tags> GetTagsList(IngestMetas metas, string mainLanguageName)
        {
            List<Tags> tags = null;

            if (metas != null && metas.MetaTags != null && metas.MetaTags.Count > 0)
            {
                // add metas-tags
                foreach (var metaTag in metas.MetaTags)
                {
                    List<string> finalValues = new List<string>();

                    foreach (var container in metaTag.Containers)
                    {
                        IEnumerable<string[]> mainLanguageValues =
                            container.Values.Where(x => mainLanguageName.Equals(x.LangCode) && !string.IsNullOrEmpty(x.Text))
                                            .Select(x => x.Text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

                        if (mainLanguageValues != null)
                        {
                            foreach (var value in mainLanguageValues)
                            {
                                finalValues.AddRange(value);
                            }
                        }
                    }

                    if (finalValues.Count > 0)
                    {
                        if (tags == null)
                        {
                            tags = new List<Tags>();
                        }

                        tags.Add(new Tags() { m_oTagMeta = new TagMeta(metaTag.Name, MetaType.Tag.ToString()), m_lValues = finalValues });
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
                    ingestResponse.AddError("media.Basic cannot be empty");
                    ingestResponse.AssetsStatus[mediaIndex].Status.Set((int)eResponseStatus.NameRequired, "media.Basic cannot be empty");
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
        static protected string GetMainLanguageName(int groupId)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            try
            {
                selectQuery += "select ll.id,ll.code3 from lu_languages ll (nolock),groups g (nolock) where g.LANGUAGE_ID=ll.id and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.id", "=", groupId);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        return selectQuery.Table("query").DefaultView[0].Row["code3"].ToString();
                    }
                }
            }
            catch (Exception)
            {
                log.ErrorFormat("GetMainLanguageName failed for groupId: {0}.", groupId);
            }
            finally
            {
                selectQuery.Finish();
                selectQuery = null;
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
        private static Status HandleAssetFiles(int groupId, long assetId, List<AssetFile> filesToHandle, bool eraseExistingFiles)
        {
            // TODO SHIR - UPDATE PREFORMENCE HandleAssetImages
            List<AssetFile> filesToAdd = null;
            List<AssetFile> filesToUpdate = null;
            try
            {
                // get the files we need to update
                if (assetId > 0 && !eraseExistingFiles)
                {
                    GenericListResponse<AssetFile> assetFilesResponse = FileManager.GetMediaFiles(groupId, 0, assetId);
                    HashSet<string> filesToHandleExternalIds = new HashSet<string>(filesToHandle.Select(x => x.ExternalId), StringComparer.OrdinalIgnoreCase);
                    Dictionary<string, long> updateFilesExternalIdToId = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
                    if (assetFilesResponse != null && assetFilesResponse.HasObjects())
                    {
                        if (assetFilesResponse.Objects.Any(x => filesToHandleExternalIds.Contains(x.ExternalId)))
                        {
                            updateFilesExternalIdToId = assetFilesResponse.Objects.Where(x => filesToHandleExternalIds.Contains(x.ExternalId)).ToDictionary(x => x.ExternalId,
                                                                                                                                                    x => x.Id, StringComparer.OrdinalIgnoreCase);
                            filesToUpdate = filesToHandle.Where(x => updateFilesExternalIdToId.ContainsKey(x.ExternalId)).ToList();
                            foreach (AssetFile assetFile in filesToUpdate)
                            {
                                assetFile.Id = updateFilesExternalIdToId[assetFile.ExternalId];
                                assetFile.AssetId = assetId;
                            }
                        }
                    }

                    filesToAdd = filesToHandle.Where(x => !updateFilesExternalIdToId.ContainsKey(x.ExternalId)).ToList();
                }
                else
                {
                    filesToAdd = filesToHandle;
                }

                // handle files to add
                if (filesToAdd != null && filesToAdd.Count > 0)
                {
                    foreach (AssetFile assetFileToAdd in filesToAdd)
                    {
                        assetFileToAdd.AssetId = assetId;
                        GenericResponse<AssetFile> addFileResponse = FileManager.InsertMediaFile(groupId, USER_ID, assetFileToAdd, true);
                        if (addFileResponse == null || !addFileResponse.HasObject() || addFileResponse.Object.Id == 0)
                        {
                            log.ErrorFormat("Failed adding asset file with externalId {0} for asset with id {1}, groupId: {2}", assetFileToAdd.ExternalId, assetId, groupId);
                            return addFileResponse.Status;
                        }
                    }
                }

                // handle files to update
                if (filesToUpdate != null && filesToUpdate.Count > 0)
                {
                    foreach (AssetFile assetFileToUpdate in filesToUpdate)
                    {
                        GenericResponse<AssetFile> updateFileResponse = FileManager.UpdateMediaFile(groupId, assetFileToUpdate, USER_ID, true);
                        if (updateFileResponse == null || !updateFileResponse.HasObject() || updateFileResponse.Object.Id == 0)
                        {
                            log.ErrorFormat("Failed updating asset file with externalId {0}, fileId: {1} for asset with id {2}, groupId: {3}",
                                            assetFileToUpdate.ExternalId, assetFileToUpdate.Id, assetId, groupId);
                            updateFileResponse.Status.Args = new List<KeyValuePair>()
                            {
                                new KeyValuePair("externalId", assetFileToUpdate.ExternalId),
                                new KeyValuePair("fileId", assetFileToUpdate.Id.ToString()),
                                new KeyValuePair("assetId", assetId.ToString()),
                                new KeyValuePair("groupId", groupId.ToString()),
                            };
                            return updateFileResponse.Status;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return new Status((int)eResponseStatus.Error, string.Format("HandleAssetFiles faild for assetId {0}", assetId));
            }

            return new Status((int)eResponseStatus.OK);
        }

        /// <summary>
        /// Handle asset images for ingest
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <param name="assetId">the asset id</param>
        /// <param name="imagesToHandle">the images of the asset that need handling</param>
        /// <param name="isUpdateRequest">is the request for updating the asset</param>
        /// <returns></returns>
        private static Status HandleAssetImages(int groupId, long assetId, eAssetImageType imageObjectType, Dictionary<long, Image> imagesToHandle, bool isUpdateRequest)
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
                            return addImageResponse.Status;
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
                                new KeyValuePair("ImageTypeId", imageToUpdate.ImageTypeId.ToString()),
                                new KeyValuePair("assetId", assetId.ToString()),
                                new KeyValuePair("groupId", groupId.ToString()),
                            };
                            return setContentResponse;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return new Status((int)eResponseStatus.Error, string.Format("HandleAssetImages faild for assetId {0}", assetId));
            }

            return new Status((int)eResponseStatus.OK);
        }

        private static Dictionary<long, Image> GetImages(IngestBasic basic, int groupId, GenericListResponse<ImageType> imageTypes, int groupDefaultRatioId,
                                             Dictionary<string, int> groupRatios)
        {
            Dictionary<long, Image> images = null;

            if (imageTypes != null && imageTypes.HasObjects())
            {
                if (basic.Thumb != null && !string.IsNullOrEmpty(basic.Thumb.Url) && groupDefaultRatioId != 0)
                {
                    var imageType = imageTypes.Objects.FirstOrDefault(x => x.RatioId.HasValue && x.RatioId.Value == groupDefaultRatioId);
                    
                    if (imageType != null)
                    {
                        Image image = new Image()
                        {
                            Url = basic.Thumb.Url,
                            ImageTypeId = imageType.Id
                        };

                        images = new Dictionary<long, Image>() { { imageType.Id, image } };
                    }
                }

                if (basic.PicsRatio.Ratios != null && basic.PicsRatio.Ratios.Count > 0 && groupRatios != null && groupRatios.Count > 0)
                {
                    foreach (var ratio in basic.PicsRatio.Ratios)
                    {
                        if (groupRatios.ContainsKey(ratio.RatioText))
                        {
                            var imageType = imageTypes.Objects.FirstOrDefault(x => x.RatioId.HasValue && x.RatioId.Value == groupRatios[ratio.RatioText]);
                            if (imageType != null)
                            {
                                if (images == null)
                                {
                                    images = new Dictionary<long, Image>();
                                }

                                if (images.ContainsKey(imageType.Id))
                                {
                                    images[imageType.Id].Url = ratio.Thumb;
                                }
                                else
                                {
                                    images.Add(imageType.Id, new Image() { ImageTypeId = imageType.Id, Url = ratio.Thumb });
                                }
                            }
                        }
                    }
                }
            }

            return images;
        }

        private static List<AssetFile> GetAssetFiles(IngestFiles files, GenericListResponse<MediaFileType> mediaFileTypes)
        {
            List<AssetFile> assetFiles = null;

            if (files != null && files.MediaFiles != null && files.MediaFiles.Count > 0 && mediaFileTypes != null && mediaFileTypes.HasObjects())
            {
                foreach (var mediaFile in files.MediaFiles)
                {
                    var mediaFileType = mediaFileTypes.Objects.FirstOrDefault(x => x.Name.Equals(mediaFile.Type));
                    if (mediaFileType != null)
                    {
                        if (assetFiles == null)
                        {
                            assetFiles = new List<AssetFile>();
                        }
                        
                        assetFiles.Add(new AssetFile(mediaFile.Type)
                        {
                            TypeId = (int)mediaFileType.Id,
                            Url = mediaFile.CdnCode,
                            Duration = StringUtils.ConvertTo<long>(mediaFile.AssetDuration),
                            ExternalId = mediaFile.CoGuid,
                            AltExternalId = mediaFile.AltCoGuid,
                            ExternalStoreId = mediaFile.ProductCode,
                            AltStreamingCode = mediaFile.AltCdnCode,
                            BillingType = GetBillingTypeIdByName(mediaFile.BillingType),
                            Language = mediaFile.Language,
                            IsDefaultLanguage = StringUtils.ConvertTo<bool>(mediaFile.IsDefaultLanguage),
                            // TODO SHIR - ASK LIOR ABOUT OutputProtecationLevel
                            // OutputProtecationLevel = mediaFile.OutputProtecationLevel,
                            StartDate = ExtractDate(mediaFile.FileStartDate, ASSET_FILE_DATE_FORMAT),
                            EndDate = ExtractDate(mediaFile.FileEndDate, ASSET_FILE_DATE_FORMAT),
                            FileSize = StringUtils.ConvertTo<long>(mediaFile.FileSize),
                            IsActive = true
                        });
                    }
                }
            }

            return assetFiles;
        }
    }
}