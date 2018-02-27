using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Tvinci.Core.DAL;
using TVinciShared;

namespace Core.Catalog.CatalogManagement
{
    public class FileManager
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Private Methods

        private static HashSet<string> CreateMappedHashSetForMediaFileType(string codecs)
        {
            HashSet<string> result = null;
            if (!string.IsNullOrEmpty(codecs))
            {
                string[] codecValues = codecs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (codecValues != null && codecValues.Length > 0)
                {
                    result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (string codec in codecValues)
                    {
                        if (!result.Contains(codec))
                        {
                            result.Add(codec);
                        }
                    }
                }
            }

            return result;
        }

        private static MediaFileType CreateMediaFileType(long id, DataRow dr)
        {
            MediaFileType result = null; ;
            int qualityType = ODBCWrapper.Utils.GetIntSafeVal(dr, "QUALITY", 0);
            if (id > 0 && typeof(MediaFileTypeQuality).IsEnumDefined(qualityType))
            {
                result = new MediaFileType()
                {
                    Id = id,
                    Name = ODBCWrapper.Utils.GetSafeStr(dr, "NAME"),
                    Description = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION"),
                    IsActive = ODBCWrapper.Utils.ExtractBoolean(dr, "IS_ACTIVE"),
                    CreateDate = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE")),
                    UpdateDate = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(ODBCWrapper.Utils.GetDateSafeVal(dr, "UPDATE_DATE")),
                    IsTrailer = ODBCWrapper.Utils.ExtractBoolean(dr, "IS_TRAILER"),
                    StreamerType = (StreamerType)Enum.Parse(typeof(StreamerType), ODBCWrapper.Utils.GetIntSafeVal(dr, "STREAMER_TYPE").ToString()),
                    DrmId = ODBCWrapper.Utils.GetIntSafeVal(dr, "DRM_ID"),
                    Quality = (MediaFileTypeQuality)qualityType,
                    VideoCodecs = CreateMappedHashSetForMediaFileType(ODBCWrapper.Utils.GetSafeStr(dr, "VIDEO_CODECS")),
                    AudioCodecs = CreateMappedHashSetForMediaFileType(ODBCWrapper.Utils.GetSafeStr(dr, "AUDIO_CODECS"))
                };
            }

            return result;
        }

        private static Status CreateMediaFileTypeResponseStatusFromResult(long result)
        {
            Status responseStatus = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            switch (result)
            {
                case -222:
                    responseStatus = new Status((int)eResponseStatus.MediaFileTypeNameAlreadyInUse, eResponseStatus.MediaFileTypeNameAlreadyInUse.ToString());
                    break;
                default:
                    break;
            }

            return responseStatus;
        }

        private static List<MediaFileType> CreateMediaFileTypeListFromDataSet(DataSet ds)
        {
            List<MediaFileType> response = new List<MediaFileType>();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                        if (id > 0)
                        {
                            MediaFileType mediaFileType = CreateMediaFileType(id, dr);
                            if (mediaFileType != null)
                            {
                                response.Add(mediaFileType);
                            }
                        }
                    }
                }
            }

            return response;
        }

        private static List<MediaFileType> GetGroupMediaFileTypes(int groupId)
        {
            List<MediaFileType> result = null;

            string key = LayeredCacheKeys.GetGroupMediaFileTypesKey(groupId);

            bool cacheResult = LayeredCache.Instance.Get<List<MediaFileType>>(
                key, ref result, GetMediaFileTypes, new Dictionary<string, object>() { { "groupId", groupId } },
                groupId, LayeredCacheConfigNames.GET_MEDIA_FILE_TYPES_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetGroupMediaFileTypesInvalidationKey(groupId) });

            if (!cacheResult)
            {
                log.Error(string.Format("GetGroupMediaFileTypes - Failed get data from cache groupId = {0}", groupId));
                result = null;
            }

            return result;
        }

        private static Tuple<List<MediaFileType>, bool> GetMediaFileTypes(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<MediaFileType> mediaFileType = null;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue && groupId.Value > 0)
                    {
                        DataSet ds = CatalogDAL.GetMediaFileTypesByGroupId(groupId.Value);
                        mediaFileType = CreateMediaFileTypeListFromDataSet(ds);

                        res = mediaFileType != null;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetMediaFileTypes failed params : {0}", funcParams != null ? string.Join(";",
                         funcParams.Select(x => string.Format("key:{0}, value: {1}", x.Key, x.Value.ToString())).ToList()) : string.Empty), ex);
            }

            return new Tuple<List<MediaFileType>, bool>(mediaFileType, res);
        }

        private static MediaFileTypeResponse CreateMediaFileTypeResponseFromDataSet(DataSet ds)
        {
            MediaFileTypeResponse response = new MediaFileTypeResponse();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID", 0);
                    if (id > 0)
                    {
                        response.MediaFileType = CreateMediaFileType(id, dt.Rows[0]);
                        if (response.MediaFileType != null)
                        {
                            response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                        }
                    }
                    else
                    {
                        response.Status = CreateMediaFileTypeResponseStatusFromResult(id);
                        return response;
                    }
                }
                /// MediaFileType does not exist (on update)
                else
                {
                    response.Status = new Status((int)eResponseStatus.MediaFileTypeDoesNotExist, eResponseStatus.MediaFileTypeDoesNotExist.ToString());
                }
            }

            return response;
        }

        private static AssetFileResponse CreateAssetFileResponseFromDataSet(int groupId, DataSet ds)
        {
            AssetFileResponse response = new AssetFileResponse() { Status = new Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() } };
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
                {
                    response.File = CreateAssetFile(groupId, dt.Rows[0]);
                    if (response.File != null)
                    {
                        response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
                /// AssetFile does not exist
                else
                {
                    response.Status = new Status((int)eResponseStatus.MediaFileDoesNotExist, eResponseStatus.MediaFileDoesNotExist.ToString());
                }
            }

            return response;
        }        

        private static AssetFile CreateAssetFile(int groupId, DataRow dr)
        {
            string typeName = string.Empty;
            int typeId = ODBCWrapper.Utils.GetIntSafeVal(dr, "MEDIA_TYPE_ID");
            MediaFileType fileType = GetMediaFileType(groupId, typeId);
            // backward compatability for older vesions that use FileMedia object instead of AssetFile object
            if (fileType != null && !string.IsNullOrEmpty(fileType.Name))
            {
                typeName = fileType.Name;
            }

            return new AssetFile(typeName)
            {
                AdditionalData = ODBCWrapper.Utils.GetSafeStr(dr, "ADDITIONAL_DATA"),
                AltExternalId = ODBCWrapper.Utils.GetSafeStr(dr, "ALT_CO_GUID"),
                AltStreamingCode = ODBCWrapper.Utils.GetSafeStr(dr, "ALT_STREAMING_CODE"),
                AlternativeCdnAdapaterProfileId = ODBCWrapper.Utils.GetLongSafeVal(dr, "ALT_STREAMING_SUPLIER_ID"),
                AssetId = ODBCWrapper.Utils.GetLongSafeVal(dr, "MEDIA_ID"),
                BillingType = ODBCWrapper.Utils.GetLongSafeVal(dr, "BILLING_TYPE_ID"),
                Duration = ODBCWrapper.Utils.GetLongSafeVal(dr, "DURATION"),
                EndDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "END_DATE"),
                ExternalId = ODBCWrapper.Utils.GetSafeStr(dr, "CO_GUID"),
                ExternalStoreId = ODBCWrapper.Utils.GetSafeStr(dr, "PRODUCT_CODE"),
                FileSize = ODBCWrapper.Utils.GetLongSafeVal(dr, "FILE_SIZE"),
                Id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID"),
                IsDefaultLanguage = ODBCWrapper.Utils.ExtractBoolean(dr, "IS_DEFAULT_LANGUAGE"),
                Language = ODBCWrapper.Utils.GetSafeStr(dr, "LANGUAGE"),
                OrderNum = ODBCWrapper.Utils.GetIntSafeVal(dr, "ORDER_NUM"),
                OutputProtecationLevel = ODBCWrapper.Utils.GetIntSafeVal(dr, "OUTPUT_PROTECTION_LEVEL"),
                StartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "START_DATE"),
                CdnAdapaterProfileId = ODBCWrapper.Utils.GetLongSafeVal(dr, "STREAMING_SUPLIER_ID"),
                TypeId = typeId,
                Url = ODBCWrapper.Utils.GetSafeStr(dr, "STREAMING_CODE"),
                IsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_ACTIVE") == 1
            };
        }

        private static List<AssetFile> GetAssetFilesById(int groupId, long id)
        {
            List<AssetFile> files = new List<AssetFile>();
            AssetFileResponse result = new AssetFileResponse();

            DataSet ds = CatalogDAL.GetMediaFile(groupId, id);
            result = CreateAssetFileResponseFromDataSet(groupId, ds);

            if (result == null || (result != null && result.Status != null && result.Status.Code != (int)eResponseStatus.OK))
            {
                return files;
            }

            files.Add(result.File);
            return files;
        }

        private static List<AssetFile> GetAssetFilesByAssetId(int groupId, long assetId)
        {
            List<AssetFile> files = new List<AssetFile>();
            DataSet ds = CatalogDAL.GetMediaFilesByAssetIds(groupId, new List<long>() { assetId });
            if (ds != null && ds.Tables != null && ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
            {
                files = CreateAssetFileListResponseFromDataTable(groupId, ds.Tables[0]);
            }

            return files;
        }

        private static Status ValidateMediaFileExternalIdUniqueness(int groupId, AssetFile assetFile)
        {
            Status status = new Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };

            if (!string.IsNullOrEmpty(assetFile.ExternalId) && !string.IsNullOrEmpty(assetFile.AltExternalId) && assetFile.ExternalId.ToLower() == assetFile.AltExternalId.ToLower())
            {
                status.Code = (int)eResponseStatus.ExternaldAndAltExternalIdMustBeUnique;
                status.Message = eResponseStatus.ExternaldAndAltExternalIdMustBeUnique.ToString();
                return status;
            }


            DataSet ds = CatalogDAL.GetMediaFilesByExternalIdAndAltExternalId(groupId, assetFile.ExternalId, assetFile.AltExternalId);

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                long id = 0;
                if (ds.Tables[0].Rows.Count > 0)
                {
                    // get all mediaFiles with  ExternalId
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                        if (id != assetFile.Id)
                        {
                            status.Code = (int)eResponseStatus.MediaFileExternalIdMustBeUnique;
                            status.Message = eResponseStatus.MediaFileExternalIdMustBeUnique.ToString();
                            return status;
                        }
                    }
                }

                // get all mediaFiles with  AltExternalId
                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID");
                        if (id != assetFile.Id)
                        {
                            status.Code = (int)eResponseStatus.MediaFileAltExternalIdMustBeUnique;
                            status.Message = eResponseStatus.MediaFileAltExternalIdMustBeUnique.ToString();
                            return status;
                        }
                    }
                }
            }

            return status;
        }

        private static void DoFreeItemIndexUpdateIfNeeded(int groupId, int assetId, DateTime? previousStartDate, DateTime? startDate, DateTime? previousEndDate, DateTime? endDate)
        {
            // check if changes in the start date require future index update call, incase updatedStartDate is in more than 2 years we don't update the index (per Ira's request)
            if (RabbitHelper.IsFutureIndexUpdate(previousStartDate, startDate))
            {
                if (!RabbitHelper.InsertFreeItemsIndexUpdate(groupId, ApiObjects.eObjectType.Media, new List<int>() { assetId }, startDate.Value))
                {
                    log.ErrorFormat("Failed inserting free items index update for startDate: {0}, mediaID: {1}, groupID: {2}", startDate.Value, assetId, groupId);
                }
            }

            // check if changes in the end date require future index update call, incase updatedEndDate is in more than 2 years we don't update the index (per Ira's request)
            if (RabbitHelper.IsFutureIndexUpdate(previousEndDate, endDate))
            {
                if (!RabbitHelper.InsertFreeItemsIndexUpdate(groupId, ApiObjects.eObjectType.Media, new List<int>() { assetId }, endDate.Value))
                {
                    log.ErrorFormat("Failed inserting free items index update for endDate: {0}, mediaID: {1}, groupID: {2}", endDate.Value, assetId, groupId);
                }
            }
        }

        private static MediaFileType GetMediaFileType(int groupId, int id)
        {
            MediaFileType result = null;
            AssetFileTypeListResponse mediaFileTypesResponse = GetMediaFileTypes(groupId);
            if (mediaFileTypesResponse != null && mediaFileTypesResponse.Status != null && mediaFileTypesResponse.Status.Code == (int)eResponseStatus.OK && mediaFileTypesResponse.Types.Count > 0)
            {
                result = mediaFileTypesResponse.Types.Where(x => x.Id == id).Count() == 1 ? mediaFileTypesResponse.Types.Where(x => x.Id == id).First() : null;
            }

            return result;
        }

        private static Status ValidateCdnAdapterProfileIds(int groupId, long? cdnAdapterProfileId, long? altcdnAdapterProfileId)
        {
            Status result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            // validate CdnAdapterProfileId if exists (0 = default adapter) or set StreamingSuplierId to default cdn adapter
            if (cdnAdapterProfileId.HasValue && cdnAdapterProfileId.Value > 0 && CachingHelpers.CdnAdapterCache.Instance.GetCdnAdapter(groupId, (int)cdnAdapterProfileId.Value, true) == null)
            {
                result = new Status((int)eResponseStatus.CdnAdapterProfileDoesNotExist, eResponseStatus.CdnAdapterProfileDoesNotExist.ToString());
                return result;
            }

            // validate AltStreamingSupplierId if exists (0 = default adapter)
            if (altcdnAdapterProfileId.HasValue && altcdnAdapterProfileId.Value > 0 && CachingHelpers.CdnAdapterCache.Instance.GetCdnAdapter(groupId, (int)altcdnAdapterProfileId.Value, true) == null)
            {
                result = new Status((int)eResponseStatus.CdnAdapterProfileDoesNotExist, "AltStreamingCdnAdapterProfileDoesNotExist");
                return result;
            }

            return result;
        }

        #endregion

        #region Internal Methods

        internal static List<AssetFile> CreateAssetFileListResponseFromDataTable(int groupId, DataTable dt)
        {
            List<AssetFile> response = new List<AssetFile>();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        AssetFile assetFile = CreateAssetFile(groupId, dr);
                        if (assetFile != null)
                        {
                            response.Add(assetFile);
                        }
                    }
                }
            }

            return response;
        }

        #endregion

        #region Public Methods

        public static AssetFileTypeListResponse GetMediaFileTypes(int groupId)
        {
            AssetFileTypeListResponse response = new AssetFileTypeListResponse();
            try
            {
                response.Types = GetGroupMediaFileTypes(groupId);
                if (response.Types != null)
                {
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetMediaFileTypes with groupId: {0}", groupId), ex);
            }

            return response;
        }        

        public static MediaFileTypeResponse AddMediaFileType(int groupId, MediaFileType mediaFileTypeToAdd, long userId)
        {
            MediaFileTypeResponse result = new MediaFileTypeResponse();
            try
            {
                DataSet ds = CatalogDAL.InsertMediaFileType(groupId, mediaFileTypeToAdd.Name, mediaFileTypeToAdd.Description, mediaFileTypeToAdd.IsActive, mediaFileTypeToAdd.IsTrailer,
                                                            (int)mediaFileTypeToAdd.StreamerType, mediaFileTypeToAdd.DrmId, mediaFileTypeToAdd.Quality, mediaFileTypeToAdd.VideoCodecs,
                                                            mediaFileTypeToAdd.AudioCodecs, userId);
                result = CreateMediaFileTypeResponseFromDataSet(ds);

                if (result.Status.Code == (int)eResponseStatus.OK)
                {
                    string invalidationKey = LayeredCacheKeys.GetGroupMediaFileTypesInvalidationKey(groupId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on AddMediaFileType, key = {0}", invalidationKey);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddMediaFileType for groupId: {0} and mediaFileType: {1}", groupId, mediaFileTypeToAdd.ToString()), ex);
            }

            return result;
        }

        public static MediaFileTypeResponse UpdateMediaFileType(int groupId, long id, MediaFileType mediaFileTypeToUpdate, long userId)
        {
            MediaFileTypeResponse result = new MediaFileTypeResponse();
            try
            {
                DataSet ds = CatalogDAL.UpdateMediaFileType(groupId, id, mediaFileTypeToUpdate.Name, mediaFileTypeToUpdate.Description, mediaFileTypeToUpdate.IsActive, mediaFileTypeToUpdate.Quality,
                                                            mediaFileTypeToUpdate.VideoCodecs, mediaFileTypeToUpdate.AudioCodecs, userId);
                result = CreateMediaFileTypeResponseFromDataSet(ds);
                if (result.Status.Code == (int)eResponseStatus.OK)
                {
                    string invalidationKey = LayeredCacheKeys.GetGroupMediaFileTypesInvalidationKey(groupId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on UpdateAssetFileType, key = {0}", invalidationKey);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateMediaFileType for groupId: {0}, id: {1} and mediaFileType: {2}", groupId, id, mediaFileTypeToUpdate.ToString()), ex);
            }

            return result;
        }

        public static Status DeleteMediaFileType(int groupId, long id, long userId)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                if (CatalogDAL.DeleteMediaFileType(groupId, id, userId))
                {
                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    if (result.Code == (int)eResponseStatus.OK)
                    {
                        string invalidationKey = LayeredCacheKeys.GetGroupMediaFileTypesInvalidationKey(groupId);
                        if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                        {
                            log.ErrorFormat("Failed to set invalidation key on DeleteMediaFileType key = {0}", invalidationKey);
                        }
                    }
                }
                /// MediaFileType does not exist (on delete)
                else
                {
                    result = new Status((int)eResponseStatus.MediaFileTypeDoesNotExist, eResponseStatus.MediaFileTypeDoesNotExist.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteMediaFileType for groupId: {0} and mediaFileType id: {1}", groupId, id), ex);
            }

            return result;

        }

        public static AssetFileResponse InsertMediaFile(int groupId, long userId, AssetFile assetFileToAdd)
        {
            AssetFileResponse result = new AssetFileResponse();
            try
            {
                // validate that asset exist
                AssetResponse assetResponse = AssetManager.GetAsset(groupId, assetFileToAdd.AssetId, eAssetTypes.MEDIA);
                if (assetResponse == null || assetResponse.Status == null || assetResponse.Status.Code != (int)eResponseStatus.OK)
                {
                    result.Status = new Status((int)eResponseStatus.AssetDoesNotExist, eResponseStatus.AssetDoesNotExist.ToString());
                    return result;
                }

                // validate that asset file type exist 
                List<MediaFileType> mediaFileTypes = GetGroupMediaFileTypes(groupId);
                if (mediaFileTypes == null || mediaFileTypes.Count < 1)
                {
                    result.Status = new Status((int)eResponseStatus.MediaFileTypeDoesNotExist, eResponseStatus.MediaFileTypeDoesNotExist.ToString());
                    return result;
                }

                MediaFileType mediaFileType = mediaFileTypes.Where(x => x.Id == assetFileToAdd.TypeId).SingleOrDefault();
                if (mediaFileType == null)
                {
                    result.Status = new Status((int)eResponseStatus.MediaFileTypeDoesNotExist, eResponseStatus.MediaFileTypeDoesNotExist.ToString());
                    return result;
                }

                // validate media doesn't already have a file with this type
                List<AssetFile> assetFiles = GetAssetFilesByAssetId(groupId, assetFileToAdd.AssetId);
                if (assetFiles != null && assetFiles.Count > 0 && assetFiles.Where(x => x.TypeId == assetFileToAdd.TypeId).Count() > 0)
                {
                    result.Status = new Status((int)eResponseStatus.MediaFileWithThisTypeAlreadyExistForAsset, eResponseStatus.MediaFileWithThisTypeAlreadyExistForAsset.ToString());
                    return result;
                }

                // validate ExternalId and AltExternalId  are unique 
                result.Status = ValidateMediaFileExternalIdUniqueness(groupId, assetFileToAdd);
                if (result.Status.Code != (int)eResponseStatus.OK)
                {
                    return result;
                }

                // validate CdnAdapaterProfileId (0 or null for setting group default adapter) \ AlternativeCdnAdapaterProfileId     
                Status validateCdnReponse = ValidateCdnAdapterProfileIds(groupId, assetFileToAdd.CdnAdapaterProfileId, assetFileToAdd.AlternativeCdnAdapaterProfileId);
                if (validateCdnReponse.Code != (int)eResponseStatus.OK)
                {
                    result.Status = validateCdnReponse;
                    return result;
                }
                else if (!assetFileToAdd.CdnAdapaterProfileId.HasValue || assetFileToAdd.CdnAdapaterProfileId.Value == 0)
                {
                    ApiObjects.CDNAdapter.CDNAdapterResponse GroupDefaultCdnAdapter = Core.Api.api.GetGroupDefaultCdnAdapter(groupId, eAssetTypes.MEDIA);
                    if (GroupDefaultCdnAdapter == null || GroupDefaultCdnAdapter.Status == null || GroupDefaultCdnAdapter.Status.Code != (int)eResponseStatus.OK
                        || GroupDefaultCdnAdapter.Adapter == null || GroupDefaultCdnAdapter.Adapter.ID <= 0)
                    {
                        result.Status = new Status((int)eResponseStatus.DefaultCdnAdapterProfileNotConfigurd, eResponseStatus.DefaultCdnAdapterProfileNotConfigurd.ToString());
                        return result;
                    }

                    assetFileToAdd.CdnAdapaterProfileId = GroupDefaultCdnAdapter.Adapter.ID;
                }

                DateTime startDate = assetFileToAdd.StartDate.HasValue ? assetFileToAdd.StartDate.Value : DateTime.UtcNow;
                DateTime endDate = assetFileToAdd.EndDate.HasValue ? assetFileToAdd.EndDate.Value : startDate;

                DataSet ds = CatalogDAL.InsertMediaFile(groupId, userId, assetFileToAdd.AdditionalData, assetFileToAdd.AltStreamingCode, assetFileToAdd.AlternativeCdnAdapaterProfileId, assetFileToAdd.AssetId,
                                                        assetFileToAdd.BillingType, assetFileToAdd.Duration, endDate, assetFileToAdd.ExternalId, assetFileToAdd.ExternalStoreId, assetFileToAdd.FileSize,
                                                        assetFileToAdd.IsDefaultLanguage, assetFileToAdd.Language, assetFileToAdd.OrderNum, assetFileToAdd.OutputProtecationLevel, startDate,
                                                        assetFileToAdd.Url, assetFileToAdd.CdnAdapaterProfileId, assetFileToAdd.TypeId, assetFileToAdd.AltExternalId, assetFileToAdd.IsActive);
                result = CreateAssetFileResponseFromDataSet(groupId, ds);

                if (result.Status.Code == (int)eResponseStatus.OK)
                {
                    // UpdateIndex
                    bool indexingResult = IndexManager.UpsertMedia(groupId, (int)assetFileToAdd.AssetId);
                    if (!indexingResult)
                    {
                        log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after InsertMediaFile", assetFileToAdd.AssetId, groupId);
                    }

                    // invalidate asset
                    string invalidationKey = LayeredCacheKeys.GetAssetInvalidationKey(eAssetTypes.MEDIA.ToString(), assetFileToAdd.AssetId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to invalidate asset with id: {0}, mediaFileId: {1}, invalidationKey: {2} after InsertMediaFile",
                                            assetFileToAdd.AssetId, assetFileToAdd.Id, invalidationKey);
                    }

                    // free item index update 
                    DoFreeItemIndexUpdateIfNeeded(groupId, (int)assetFileToAdd.AssetId, null, assetFileToAdd.StartDate, null, assetFileToAdd.EndDate);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddAssetFile for groupId: {0} and AssetFile: {1}", groupId, JsonConvert.SerializeObject(assetFileToAdd)), ex);
            }

            return result;
        }

        public static Status DeleteMediaFile(int groupId, long userId, long id)
        {
            Status result = null;
            AssetFileResponse assetFileResponse = null;
            try
            {

                DataSet ds = CatalogDAL.GetMediaFile(groupId, id);
                assetFileResponse = CreateAssetFileResponseFromDataSet(groupId, ds);

                if (assetFileResponse != null && assetFileResponse.Status != null && assetFileResponse.Status.Code != (int)eResponseStatus.OK)
                {
                    return assetFileResponse.Status;
                }

                if (CatalogDAL.DeleteMediaFile(groupId, userId, id))
                {
                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    if (result.Code == (int)eResponseStatus.OK)
                    {
                        // UpdateIndex
                        bool indexingResult = IndexManager.UpsertMedia(groupId, (int)assetFileResponse.File.AssetId);
                        if (!indexingResult)
                        {
                            log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after DeleteMediaFile", assetFileResponse.File.AssetId, groupId);
                        }

                        //string invalidationKey = LayeredCacheKeys.GetGroupAssetFileTypesInvalidationKey(groupId);
                        //if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                        //{
                        //    log.ErrorFormat("Failed to set invalidation key on DeleteAssetFileType key = {0}", invalidationKey);
                        //}

                        // TODO
                        // InvalidateCatalogGroupCache(groupId, result, false);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteAssetFile for groupId: {0} and AssetFileId: {1}", groupId, id), ex);
            }

            return result;
        }

        public static AssetFileResponse UpdateMediaFile(int groupId, AssetFile assetFileToUpdate, long userId)
        {
            AssetFileResponse result = new AssetFileResponse();
            try
            {
                DataSet ds = CatalogDAL.GetMediaFile(groupId, assetFileToUpdate.Id);
                AssetFileResponse currentAssetFile = CreateAssetFileResponseFromDataSet(groupId, ds);

                if (currentAssetFile != null && currentAssetFile.Status != null && currentAssetFile.Status.Code != (int)eResponseStatus.OK)
                {
                    return currentAssetFile;
                }

                if (currentAssetFile.File != null && currentAssetFile.File.AssetId != assetFileToUpdate.AssetId)
                {                 
                    result.Status = new Status() { Code = (int)eResponseStatus.MediaFileNotBelongToAsset, Message = eResponseStatus.MediaFileNotBelongToAsset.ToString() };
                    return result;
                }

                //// validate that asset file type exist
                List<MediaFileType> mediaFileTypes = GetGroupMediaFileTypes(groupId);
                if (mediaFileTypes == null || mediaFileTypes.Count < 1)
                {
                    result.Status = new Status((int)eResponseStatus.MediaFileTypeDoesNotExist, eResponseStatus.MediaFileTypeDoesNotExist.ToString());
                    return result;
                }

                MediaFileType mediaFileType = mediaFileTypes.Where(x => x.Id == assetFileToUpdate.TypeId).SingleOrDefault();
                if (mediaFileType == null)
                {
                    result.Status = new Status((int)eResponseStatus.MediaFileTypeDoesNotExist, eResponseStatus.MediaFileTypeDoesNotExist.ToString());
                    return result;
                }

                // validate media doesn't already have a file with this type
                if (assetFileToUpdate.TypeId.HasValue)
                {
                    List<AssetFile> assetFiles = GetAssetFilesByAssetId(groupId, assetFileToUpdate.AssetId);
                    if (assetFiles != null && assetFiles.Count > 0 && assetFiles.Where(x => x.TypeId == assetFileToUpdate.TypeId && x.Id != assetFileToUpdate.Id).Count() > 0)
                    {
                        result.Status = new Status((int)eResponseStatus.MediaFileWithThisTypeAlreadyExistForAsset, eResponseStatus.MediaFileWithThisTypeAlreadyExistForAsset.ToString());
                        return result;
                    }
                }

                // ExternalId and AltExternalId cannot be the same value
                if (string.IsNullOrEmpty(assetFileToUpdate.ExternalId))
                {
                    assetFileToUpdate.ExternalId = currentAssetFile.File.ExternalId;
                }

                if (string.IsNullOrEmpty(assetFileToUpdate.AltExternalId) && !string.IsNullOrEmpty(currentAssetFile.File.AltExternalId))
                {
                    assetFileToUpdate.AltExternalId = currentAssetFile.File.AltExternalId;
                }

                // validate ExternalId and AltExternalId  are unique 
                result.Status = ValidateMediaFileExternalIdUniqueness(groupId, assetFileToUpdate);
                if (result.Status.Code != (int)eResponseStatus.OK)
                {
                    return result;
                }

                // validate CdnAdapaterProfileId + AlternativeCdnAdapaterProfileId
                Status validateCdnReponse = ValidateCdnAdapterProfileIds(groupId, assetFileToUpdate.CdnAdapaterProfileId, assetFileToUpdate.AlternativeCdnAdapaterProfileId);
                if (validateCdnReponse.Code != (int)eResponseStatus.OK)
                {
                    result.Status = validateCdnReponse;
                    return result;
                }

                DateTime startDate = assetFileToUpdate.StartDate.HasValue ? assetFileToUpdate.StartDate.Value : DateTime.UtcNow;
                DateTime endDate = assetFileToUpdate.EndDate.HasValue ? assetFileToUpdate.EndDate.Value : startDate;

                ds = CatalogDAL.UpdateMediaFile(groupId, assetFileToUpdate.Id, userId, assetFileToUpdate.AdditionalData, assetFileToUpdate.AltStreamingCode, assetFileToUpdate.AlternativeCdnAdapaterProfileId,
                                                assetFileToUpdate.AssetId, assetFileToUpdate.BillingType, assetFileToUpdate.Duration, endDate, assetFileToUpdate.ExternalId,
                                                assetFileToUpdate.ExternalStoreId, assetFileToUpdate.FileSize, assetFileToUpdate.IsDefaultLanguage, assetFileToUpdate.Language,
                                                assetFileToUpdate.OrderNum, assetFileToUpdate.OutputProtecationLevel, startDate, assetFileToUpdate.Url, assetFileToUpdate.CdnAdapaterProfileId,
                                                assetFileToUpdate.TypeId, assetFileToUpdate.AltExternalId, assetFileToUpdate.IsActive);

                result = CreateAssetFileResponseFromDataSet(groupId, ds);

                if (result.Status.Code == (int)eResponseStatus.OK)
                {
                    // UpdateIndex
                    bool indexingResult = IndexManager.UpsertMedia(groupId, (int)assetFileToUpdate.AssetId);
                    if (!indexingResult)
                    {
                        log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after UpdateMediaFile", assetFileToUpdate.AssetId, groupId);
                    }

                    // invalidate asset
                    string invalidationKey = LayeredCacheKeys.GetAssetInvalidationKey(eAssetTypes.MEDIA.ToString(), assetFileToUpdate.AssetId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to invalidate asset with id: {0}, mediaFileId: {1}, invalidationKey: {2} after UpdateMediaFile",
                                            assetFileToUpdate.AssetId, assetFileToUpdate.Id, invalidationKey);
                    }

                    // free item index update 
                    DoFreeItemIndexUpdateIfNeeded(groupId, (int)assetFileToUpdate.AssetId, currentAssetFile.File.StartDate, assetFileToUpdate.StartDate,
                                                    currentAssetFile.File.EndDate, assetFileToUpdate.EndDate);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateAssetFile for groupId: {0} and AssetFile: {1}", groupId, JsonConvert.SerializeObject(assetFileToUpdate)), ex);
            }

            return result;
        }

        public static AssetFileListResponse GetMediaFiles(int groupId, long id, long assetId)
        {
            AssetFileListResponse response = new AssetFileListResponse();
            try
            {
                if (id > 0)
                {
                    response.Files = GetAssetFilesById(groupId, id);
                }
                else
                {
                    response.Files = GetAssetFilesByAssetId(groupId, assetId);
                }

                if (response.Files != null)
                {
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetFileTypes with groupId: {0}", groupId), ex);
            }

            return response;
        }

        public static List<FileMedia> ConvertFiles(List<AssetFile> assetFiles, int groupId)
        {
            List<FileMedia> result = new List<FileMedia>();
            if (assetFiles != null && assetFiles.Count > 0)
            {
                foreach (AssetFile file in assetFiles)
                {
                    MediaFileType mediaFileType = FileManager.GetMediaFileType(groupId, file.TypeId.Value);
                    if (mediaFileType != null)
                    {
                        result.Add(new FileMedia(file, mediaFileType));
                    }
                }
            }

            return result;
        }

        #endregion

    }
}
