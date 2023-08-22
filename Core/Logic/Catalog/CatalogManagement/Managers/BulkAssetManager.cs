using ApiObjects;
using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CachingProvider.LayeredCache;
using Tvinci.Core.DAL;
using TVinciShared;

namespace Core.Catalog.CatalogManagement
{
    public class BulkAssetManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static int GetMediaIdByCoGuid(int groupId, string coGuid)
        {
            DataTable existingExternalIdsDt = CatalogDAL.ValidateExternalIdsExist(groupId, new List<string>() { coGuid });
            if (existingExternalIdsDt != null && existingExternalIdsDt.Rows != null && existingExternalIdsDt.Rows.Count > 0)
            {
                DataRow dr = existingExternalIdsDt.Rows[0];
                return ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
            }

            return 0;
        }

        public static GenericListResponse<Status> UpsertMediaAsset<T>(int groupId, ref T mediaAsset, long userId, Dictionary<long, Image> images,
            Dictionary<int, Tuple<AssetFile, string>> assetFiles, string dateFormat, bool needToEraseMedia, bool isFromIngest = false, HashSet<long> topicIdsToRemove = null)
            where T : MediaAsset
        {
            var response = new GenericListResponse<Status>();
            try
            {
                log.Debug($"BEO-8698 UpsertMediaAsset id {mediaAsset.Id}");

                if (mediaAsset.Id == 0)
                {
                    var addAsset = AssetManager.Instance.AddAsset(groupId, mediaAsset, userId, true);
                    if (!addAsset.HasObject())
                    {
                        log.Debug("UpsertMediaAsset - AddAsset faild");
                        response.SetStatus(addAsset.Status);
                        return response;
                    }
                    mediaAsset = addAsset.Object as T;
                    response.Objects = UpsertMediaAssetImagesAndFiles(groupId, false, mediaAsset.Id, images, true, assetFiles, dateFormat, userId);
                }
                else
                {
                    var isCleared = false;

                    if (needToEraseMedia)
                    {
                        if (!(isCleared = AssetManager.ClearAsset(groupId, mediaAsset.Id, eAssetTypes.MEDIA, userId)))
                        {
                            log.Debug($"UpsertMediaAsset - ClearAsset faild for media id: {mediaAsset.Id}");
                        }
                    }

                    if (isFromIngest && topicIdsToRemove != null && topicIdsToRemove.Count > 0)
                    {
                        var removeTopicsResponse = AssetManager.RemoveTopicsFromAsset(groupId, mediaAsset.Id, eAssetTypes.MEDIA, topicIdsToRemove, userId, isFromIngest);
                        if (!removeTopicsResponse.IsOkStatusCode())
                        {
                            response.Objects.Add(removeTopicsResponse);
                        }
                    }

                    var updateAsset = AssetManager.Instance.UpdateAsset(groupId, mediaAsset.Id, mediaAsset, userId, true, isCleared);
                    if (!updateAsset.HasObject())
                    {
                        log.Debug("UpdateMediaAsset - UpdateAsset faild");
                        response.SetStatus(updateAsset.Status);
                        return response;
                    }
                    mediaAsset = updateAsset.Object as T;
                    response.Objects.AddRange(UpsertMediaAssetImagesAndFiles(groupId, true, mediaAsset.Id, images, needToEraseMedia, assetFiles, dateFormat, userId));
                }

                if (!isFromIngest)
                {
                    // UpdateIndex
                    bool indexingResult = IndexManagerFactory.Instance.GetIndexManager(groupId).UpsertMedia(mediaAsset.Id);
                    if (!indexingResult)
                    {
                        log.Error($"Failed UpsertMedia index for assetId: {mediaAsset.Id}, groupId: {groupId} after Ingest");
                    }
                    
                    //extracted it from upsertMedia it was called also for OPC accounts,searchDefinitions
                    //not sure it's required but better be safe
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetMediaInvalidationKey(groupId, mediaAsset.Id));

                    // invalidate asset
                    AssetManager.Instance.InvalidateAsset(eAssetTypes.MEDIA, groupId, (int)mediaAsset.Id);
                }

                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in UpsertMediaAsset. groupId:{groupId}, CoGuid:{mediaAsset.CoGuid}, userId:{userId}.", ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        private static List<Status> UpsertMediaAssetImagesAndFiles(int groupId, bool isUpdateRequest, long mediaAssetId, Dictionary<long, Image> images, bool eraseExistingFiles,
                                                                   Dictionary<int, Tuple<AssetFile, string>> assetFiles, string dateFormat, long userId)
        {
            List<Status> warnings = new List<Status>();
            if (images != null && images.Count > 0)
            {
                warnings.AddRange(HandleAssetImages(groupId, images, mediaAssetId, eAssetImageType.Media, userId, isUpdateRequest));
            }

            if (assetFiles != null && assetFiles.Count > 0)
            {
                warnings.AddRange(HandleAssetFiles(groupId, assetFiles, mediaAssetId, eraseExistingFiles, dateFormat, userId));
            }

            return warnings;
        }

        /// <summary>
        /// Handle asset images for ingest
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <param name="assetId">the asset id</param>
        /// <param name="imagesToHandle">the images of the asset that need handling</param>
        /// <param name="isUpdateRequest">is the request for updating the asset</param>
        /// <returns></returns>
        private static List<Status> HandleAssetImages(int groupId, Dictionary<long, Image> imagesToHandle, long assetId, eAssetImageType imageObjectType, long userId, bool isUpdateRequest)
        {
            // TODO - UPDATE PREFORMENCE HandleAssetImages
            List<Status> warnings = new List<Status>();
            List<Image> imagesToAdd = null;
            List<Image> imagesToUpdate = new List<Image>();

            try
            {
                // get the images we need to update
                if (assetId > 0 && isUpdateRequest)
                {
                    GenericListResponse<Image> assetImagesResponse = ImageManager.Instance.GetImagesByObject(groupId, assetId, imageObjectType);
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
                        GenericResponse<Image> addImageResponse = ImageManager.Instance.AddImage(groupId, imageToAdd, userId);
                        if (addImageResponse == null || !addImageResponse.HasObject() || addImageResponse.Object.Id == 0)
                        {
                            log.ErrorFormat("Failed adding image with imageTypeId {0} for assetId {1}, groupId: {2}", imageToAdd.ImageTypeId, assetId, groupId);

                            addImageResponse.Status.AddArg("imageToAdd.ImageTypeId", imageToAdd.ImageTypeId.ToString());
                            addImageResponse.Status.AddArg("imageToAdd.Url", imageToAdd.Url);
                            warnings.Add(addImageResponse.Status);
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
                        Status setContentResponse = ImageManager.Instance.SetContent(groupId, userId, imageToUpdate.Id, imageToUpdate.Url);
                        if (setContentResponse == null || !setContentResponse.IsOkStatusCode())
                        {
                            log.ErrorFormat("Failed setContent for image with Id {0}, ImageTypeId {1} for assetId {2} and groupId: {3}",
                                             imageToUpdate.Id, imageToUpdate.ImageTypeId, assetId, groupId);

                            setContentResponse.AddArg("imageToUpdate.ImageTypeId", imageToUpdate.ImageTypeId.ToString());
                            setContentResponse.AddArg("imageToUpdate.Url", imageToUpdate.Url);
                            warnings.Add(setContentResponse);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = string.Format("Exception while HandleAssetImages for assetId: {0}", assetId);
                log.Error(errorMsg, ex);
                warnings.Add(new Status((int)eResponseStatus.Error, errorMsg));
            }

            return warnings;
        }

        /// <summary>
        /// Handle asset files for ingest
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="assetId"></param>
        /// <param name="filesToHandle">key: assetFileToHandle.TypeId, value: { assetFileToHandle, ppvModule } </param>
        /// <param name="eraseExistingFiles"></param>
        /// <param name="ingestResponse"></param>
        /// <param name="index"></param>
        private static List<Status> HandleAssetFiles(int groupId, Dictionary<int, Tuple<AssetFile, string>> filesToHandle, long assetId, bool eraseExistingFiles, string dateFormat, long userId)
        {
            List<Status> warnings = new List<Status>();
            try
            {
                // handle existing assetFiles
                GenericListResponse<AssetFile> assetFilesResponse = FileManager.Instance.GetMediaFiles(groupId, 0, assetId);
                if (assetFilesResponse != null && assetFilesResponse.HasObjects())
                {
                    foreach (var assetFile in assetFilesResponse.Objects)
                    {
                        int assetFileTypeId = assetFile.TypeId.Value;
                        if (filesToHandle.ContainsKey(assetFileTypeId))
                        {
                            filesToHandle[assetFileTypeId].Item1.AssetId = assetId;
                            var fileTypeName = filesToHandle[assetFileTypeId].Item1.GetTypeName();
                            if (eraseExistingFiles)
                            {
                                if (!CatalogDAL.DeleteMediaFile(groupId, userId, assetFile.Id))
                                {
                                    string errMsg = string.Format("Failed DeleteMediaFile befor InsertMediaFile (required to erase) for assetId {0}, assetFile.typeName {1}.",
                                                                  assetId, fileTypeName);
                                    log.Error(errMsg);
                                    warnings.Add(new Status((int)eResponseStatus.Error, errMsg, new List<ApiObjects.KeyValuePair>() { new ApiObjects.KeyValuePair("assetFileToDelete.typeName", fileTypeName) }));
                                }
                                else
                                {
                                    var insertMediaFileResponse = FileManager.Instance.InsertMediaFile(groupId, userId, filesToHandle[assetFileTypeId].Item1, true);
                                    if (!insertMediaFileResponse.HasObject())
                                    {
                                        log.ErrorFormat("Failed InsertMediaFile after DeleteMediaFile (required to erase) for assetId {0}, assetFile.typeName {1}.", assetId, fileTypeName);
                                        insertMediaFileResponse.Status.AddArg("assetFileToInsert.typeName", fileTypeName);
                                        warnings.Add(insertMediaFileResponse.Status);
                                    }
                                    else
                                    {
                                        HandleAssetFilePpvModels(filesToHandle[assetFileTypeId].Item2, groupId, insertMediaFileResponse.Object.Id, filesToHandle[assetFileTypeId].Item1.AssetId, dateFormat);
                                    }
                                }
                            }
                            else
                            {
                                filesToHandle[assetFileTypeId].Item1.Id = assetFile.Id;
                                var updateMediaFileResponse = FileManager.Instance.UpdateMediaFile(groupId, filesToHandle[assetFileTypeId].Item1, userId, true, assetFile);
                                if (!updateMediaFileResponse.HasObject())
                                {
                                    log.ErrorFormat("Failed UpdateMediaFile for assetId {0}, assetFile.typeName {1}.", assetId, fileTypeName);
                                    updateMediaFileResponse.Status.AddArg("assetFileToUpdate.typeName", fileTypeName);
                                    warnings.Add(updateMediaFileResponse.Status);
                                }
                                else
                                {
                                    HandleAssetFilePpvModels(filesToHandle[assetFileTypeId].Item2, groupId, updateMediaFileResponse.Object.Id, filesToHandle[assetFileTypeId].Item1.AssetId, dateFormat);
                                }
                            }

                            filesToHandle.Remove(assetFile.TypeId.Value);
                        }
                        else if (eraseExistingFiles && !CatalogDAL.DeleteMediaFile(groupId, userId, assetFile.Id))
                        {
                            var fileTypeName = assetFile.GetTypeName();
                            string errMsg = string.Format("Failed DeleteMediaFile (required to erase) for assetId {0}, assetFile.typeName {1}.", assetId, fileTypeName);
                            log.Error(errMsg);
                            warnings.Add(new Status((int)eResponseStatus.Error, errMsg, new List<ApiObjects.KeyValuePair>() { new ApiObjects.KeyValuePair("assetFileToDelete.typeName", fileTypeName) }));
                        }
                    }
                }

                // handle new assetFiles
                foreach (var assetFileToAdd in filesToHandle)
                {
                    assetFileToAdd.Value.Item1.AssetId = assetId;
                    var insertMediaFileResponse = FileManager.Instance.InsertMediaFile(groupId, userId, assetFileToAdd.Value.Item1, true);
                    if (!insertMediaFileResponse.HasObject())
                    {
                        log.ErrorFormat("Failed InsertMediaFile for assetId {0}, assetFile.typeName {1}.", assetId, assetFileToAdd.Value.Item1.GetTypeName());
                        insertMediaFileResponse.Status.AddArg("assetFileToInsert.typeName", assetFileToAdd.Value.Item1.GetTypeName());
                        warnings.Add(insertMediaFileResponse.Status);
                        continue;
                    }

                    HandleAssetFilePpvModels(assetFileToAdd.Value.Item2, groupId, insertMediaFileResponse.Object.Id, assetFileToAdd.Value.Item1.AssetId, dateFormat);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = string.Format("Exception while HandleAssetFiles for assetId: {0}", assetId);
                log.Error(errorMsg, ex);
                warnings.Add(new Status((int)eResponseStatus.Error, errorMsg));
            }

            return warnings;
        }

        private static void HandleAssetFilePpvModels(string ppvModuleName, int groupId, long assetFileId, long assetId, string assetFileDateFormat)
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

                        DateTime? ppvStartDate = DateUtils.TryExtractDate(parameters[i + 1], assetFileDateFormat);
                        DateTime? ppvEndDate = DateUtils.TryExtractDate(parameters[i + 2], assetFileDateFormat);

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
                                if (!RabbitHelper.InsertFreeItemsIndexUpdate(groupId, eObjectType.Media, new List<long>() { assetId }, ppvStartDate.Value))
                                {
                                    log.Error(string.Format("Failed inserting free items index update for startDate: {0}, mediaID: {1}, groupID: {2}", ppvStartDate.Value, assetId, groupId));
                                }
                            }

                            // check if changes in the end date require future index update call, incase ppvEndDate is in more than 2 years we don't update the index (per Ira's request)
                            if (RabbitHelper.IsFutureIndexUpdate(prevPPVFileEndDate, ppvStartDate))
                            {
                                if (!RabbitHelper.InsertFreeItemsIndexUpdate(groupId, eObjectType.Media, new List<long>() { assetId }, ppvEndDate.Value))
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

                //BEO-14429
                string invalidationKey = LayeredCacheKeys.GetPPVsforFileInvalidationKey(groupId, assetFileId);
                LayeredCache.Instance.SetInvalidationKey(invalidationKey);
            }
        }

        // TODO - use good method
        private static int GetPPVModuleID(string moduleName, int groupId)
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

        // TODO - use good method
        private static bool InsertFilePPVModule(int ppvModule, long fileID, int groupId, DateTime? startDate, DateTime? endDate, bool clear)
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

        public static Dictionary<int, Tuple<AssetFile, string>> CreateAssetFilesMap(List<AssetFile> files)
        {
            var assetFiles = new Dictionary<int, Tuple<AssetFile, string>>(); ;

            if (files != null)
            {
                foreach (var assetFile in files)
                {
                    if (assetFile.TypeId.HasValue && !assetFiles.ContainsKey(assetFile.TypeId.Value))
                    {
                        assetFiles.Add(assetFile.TypeId.Value, new Tuple<AssetFile, string>(assetFile, assetFile.PpvModule));
                    }
                }
            }

            return assetFiles;
        }

        public static Dictionary<long, Image> CreateImagesMap(List<Image> images)
        {
            Dictionary<long, Image> imagesDic = new Dictionary<long, Image>();
            if (images != null)
            {
                foreach (var image in images)
                {
                    if (!imagesDic.ContainsKey(image.ImageTypeId))
                    {
                        imagesDic.Add(image.ImageTypeId, image);
                    }
                }
            }

            return imagesDic;
        }
    }
}