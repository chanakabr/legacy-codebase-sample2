using ApiLogic.Catalog.CatalogManagement.Repositories;
using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using ApiLogic.Catalog.CatalogManagement.Services;
using CategoriesSchema;
using Microsoft.Extensions.Logging;
using Tvinci.Core.DAL;
using TvinciImporter;
using TVinciShared;

namespace Core.Catalog.CatalogManagement
{
    public interface IMediaFileTypeManager
    {
        GenericListResponse<MediaFileType> GetMediaFileTypes(int groupId);
        GenericResponse<AssetFile> InsertMediaFile(int groupId, long userId, AssetFile assetFileToAdd, bool isFromIngest = false);
        Status DeleteMediaFile(int groupId, long userId, long id, bool isFromIngest = false);
        GenericResponse<AssetFile> UpdateMediaFile(int groupId, AssetFile assetFileToUpdate, long userId, bool isFromIngest = false, AssetFile currentAssetFile = null);
        void DoFreeItemIndexUpdateIfNeeded(int groupId, int assetId, DateTime? previousStartDate, DateTime? startDate, DateTime? previousEndDate, DateTime? endDate);
        GenericListResponse<AssetFile> GetMediaFiles(int groupId, long id, long assetId);
        Status InvalidateAssetAfterFilesUpdated(int groupId, long assetId, long userId);
    }
    public class FileManager : IMediaFileTypeManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly ILabelRepository _labelRepository = LabelRepository.Instance;

        private static readonly Lazy<FileManager> lazy = new Lazy<FileManager>(() => new FileManager(), LazyThreadSafetyMode.PublicationOnly);

        public static FileManager Instance => lazy.Value;

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

        private static HashSet<string> CreateMediaFileTypeDynamicDataKeys(long mediaFileTypeId, DataTable dynamicDataKeysTable)
        {
            return dynamicDataKeysTable?
                .Select($"MEDIA_FILE_TYPE_ID={mediaFileTypeId}")
                .Select(x => ODBCWrapper.Utils.GetSafeStr(x, "KEY"))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static MediaFileType CreateMediaFileType(long id, DataRow dr, DataTable dynamicDataKeysTable)
        {
            MediaFileType result = null; ;
            int qualityType = ODBCWrapper.Utils.GetIntSafeVal(dr, "QUALITY", 0);
            if (id > 0 && typeof(MediaFileTypeQuality).IsEnumDefined(qualityType))
            {
                result = new MediaFileType
                {
                    Id = id,
                    Name = ODBCWrapper.Utils.GetSafeStr(dr, "NAME"),
                    Description = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION"),
                    IsActive = ODBCWrapper.Utils.ExtractBoolean(dr, "IS_ACTIVE"),
                    CreateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE")),
                    UpdateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(ODBCWrapper.Utils.GetDateSafeVal(dr, "UPDATE_DATE")),
                    IsTrailer = ODBCWrapper.Utils.ExtractBoolean(dr, "IS_TRAILER"),
                    StreamerType = (StreamerType)Enum.Parse(typeof(StreamerType), ODBCWrapper.Utils.GetIntSafeVal(dr, "STREAMER_TYPE").ToString()),
                    DrmId = ODBCWrapper.Utils.GetIntSafeVal(dr, "DRM_ID"),
                    Quality = (MediaFileTypeQuality)qualityType,
                    VideoCodecs = CreateMappedHashSetForMediaFileType(ODBCWrapper.Utils.GetSafeStr(dr, "VIDEO_CODECS")),
                    AudioCodecs = CreateMappedHashSetForMediaFileType(ODBCWrapper.Utils.GetSafeStr(dr, "AUDIO_CODECS")),
                    DynamicDataKeys = CreateMediaFileTypeDynamicDataKeys(id, dynamicDataKeysTable)
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
            if (ds?.Tables != null && ds.Tables.Count > 0)
            {
                var dt = ds.Tables[0];
                var dynamicDataKeysTable = ds.Tables.Count > 1 ? ds.Tables[1] : null;
                if (dt?.Rows != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                        if (id > 0)
                        {
                            var  mediaFileType = CreateMediaFileType(id, dr, dynamicDataKeysTable);
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

        private static GenericResponse<MediaFileType> CreateMediaFileTypeResponseFromDataSet(DataSet ds)
        {
            GenericResponse<MediaFileType> response = new GenericResponse<MediaFileType>();
            if (ds?.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt?.Rows != null && dt.Rows.Count == 1)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID", 0);
                    if (id > 0)
                    {
                        var dynamicDataKeysTable = ds.Tables.Count > 1 ? ds.Tables[1] : null;
                        response.Object = CreateMediaFileType(id, dt.Rows[0], dynamicDataKeysTable);
                        if (response.Object != null)
                        {
                            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                        }
                    }
                    else
                    {
                        response.SetStatus(CreateMediaFileTypeResponseStatusFromResult(id));
                        return response;
                    }
                }
                /// MediaFileType does not exist (on update)
                else
                {
                    response.SetStatus(eResponseStatus.MediaFileTypeDoesNotExist, eResponseStatus.MediaFileTypeDoesNotExist.ToString());
                }
            }

            return response;
        }

        private static GenericResponse<AssetFile> CreateAssetFileResponseFromDataSet(int groupId, DataSet ds, bool shouldAddBaseUrl = true)
        {
            GenericResponse<AssetFile> response = new GenericResponse<AssetFile>();

            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows.Count == 1)
                {
                    var assetFileRow = dt.Rows[0];
                    var labelsTable = ds.Tables.Count > 1 ? ds.Tables[1] : null;
                    var dynamicDataTable = ds.Tables.Count > 2 ? ds.Tables[2] : null;
                    response.Object = CreateAssetFile(groupId, assetFileRow, labelsTable, dynamicDataTable, shouldAddBaseUrl);
                    if (response.Object != null)
                    {
                        response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
                /// AssetFile does not exist
                else
                {
                    response.SetStatus(eResponseStatus.MediaFileDoesNotExist, eResponseStatus.MediaFileDoesNotExist.ToString());
                }
            }

            return response;
        }

        private static AssetFile CreateAssetFile(int groupId, DataRow dr, DataTable labelsTable, DataTable dynamicDataTable, bool shouldAddBaseUrl)
        {
            string typeName = string.Empty;
            int typeId = ODBCWrapper.Utils.GetIntSafeVal(dr, "MEDIA_TYPE_ID");
            MediaFileType fileType = GetMediaFileType(groupId, typeId);
            // backward compatability for older vesions that use FileMedia object instead of AssetFile object
            if (fileType != null && !string.IsNullOrEmpty(fileType.Name))
            {
                typeName = fileType.Name;
            }

            DateTime? catalogEndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "CATALOG_END_DATE");
            AssetFile res = new AssetFile(typeName)
            {
                AdditionalData = ODBCWrapper.Utils.GetSafeStr(dr, "ADDITIONAL_DATA"),
                AltExternalId = ODBCWrapper.Utils.GetSafeStr(dr, "ALT_CO_GUID"),
                AltStreamingCode = ODBCWrapper.Utils.GetSafeStr(dr, "ALT_STREAMING_CODE"),
                AlternativeCdnAdapaterProfileId = ODBCWrapper.Utils.GetNullableLong(dr, "ALT_STREAMING_SUPLIER_ID"),
                AssetId = ODBCWrapper.Utils.GetLongSafeVal(dr, "MEDIA_ID"),
                BillingType = ODBCWrapper.Utils.GetLongSafeVal(dr, "BILLING_TYPE_ID"),
                Duration = ODBCWrapper.Utils.GetNullableLong(dr, "DURATION"),
                EndDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "END_DATE"),
                ExternalId = ODBCWrapper.Utils.GetSafeStr(dr, "CO_GUID"),
                ExternalStoreId = ODBCWrapper.Utils.GetSafeStr(dr, "PRODUCT_CODE"),
                FileSize = ODBCWrapper.Utils.GetNullableLong(dr, "FILE_SIZE"),
                Id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID"),
                IsDefaultLanguage = ODBCWrapper.Utils.ExtractBoolean(dr, "IS_DEFAULT_LANGUAGE"),
                Language = ODBCWrapper.Utils.GetSafeStr(dr, "LANGUAGE"),
                OrderNum = ODBCWrapper.Utils.GetNullableInt(dr, "ORDER_NUM"),
                StartDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "START_DATE"),
                CdnAdapaterProfileId = ODBCWrapper.Utils.GetNullableLong(dr, "STREAMING_SUPLIER_ID"),
                TypeId = typeId,
                Url = ODBCWrapper.Utils.GetSafeStr(dr, "STREAMING_CODE"),
                IsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_ACTIVE") == 1,
                CatalogEndDate = catalogEndDate.HasValue ? catalogEndDate : new DateTime(2099, 1, 1),
                Opl = ODBCWrapper.Utils.GetSafeStr(dr, "opl"),
                UpdateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "UPDATE_DATE")
            };

            if (labelsTable != null)
            {
                var labelValues = new List<string>();
                var labelRows = labelsTable.Select($"MEDIA_FILE_ID={res.Id}");
                foreach (var labelRow in labelRows)
                {
                    var labelValue = ODBCWrapper.Utils.GetSafeStr(labelRow, "VALUE");
                    labelValues.Add(labelValue);
                }

                SetLabels(res, labelValues);
            }

            if (dynamicDataTable != null)
            {
                res.DynamicData = dynamicDataTable.Select($"MEDIA_FILE_ID={res.Id}")
                    .Select(x => new KeyValuePair<string, string>(
                        ODBCWrapper.Utils.GetSafeStr(x, "KEY"),
                        ODBCWrapper.Utils.GetSafeStr(x, "VALUE")))
                    .GroupBy(x => x.Key)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(_ => _.Value).ToArray().AsEnumerable());
            }

            if (shouldAddBaseUrl)
            {
                var baseUrl = ODBCWrapper.Utils.GetSafeStr(dr, "BASE_URL");
                if (!res.Url.StartsWith(baseUrl))
                {
                    res.Url = string.Concat(baseUrl, res.Url);
                }

                var altBaseUrl = ODBCWrapper.Utils.GetSafeStr(dr, "ALT_BASE_URL");
                if (!res.AltStreamingCode.StartsWith(altBaseUrl))
                {
                    res.AltStreamingCode = string.Concat(altBaseUrl, res.AltStreamingCode);
                }
            }

            return res;
            ;
        }

        public Status InvalidateAssetAfterFilesUpdated(int groupId, long assetId, long userId)
        {
            try
            {
                // UpdateIndex
                bool indexingResult = IndexManagerFactory.Instance.GetIndexManager(groupId).UpsertMedia(assetId);
                if (!indexingResult)
                {
                    log.ErrorFormat("Failed UpsertMedia index for assetId: {0}, groupId: {1} after UpdateMediaFile", assetId, groupId);
                }

                //extracted it from upsertMedia it was called also for OPC accounts,searchDefinitions
                //not sure it's required but better be safe
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetMediaInvalidationKey(groupId, assetId));

                // invalidate asset
                AssetManager.Instance.InvalidateAsset(eAssetTypes.MEDIA, groupId, assetId);

                return Status.Ok;
            }
            catch (Exception e)
            {
                log.LogError(e, $"Asset invalidation after files updated is failed. AssetId: {assetId}");

                return Status.Error;
            }
        }

        public void DoFreeItemIndexUpdateIfNeeded(int groupId, int assetId, DateTime? previousStartDate, DateTime? startDate, DateTime? previousEndDate, DateTime? endDate)
        {
            // check if changes in the start date require future index update call, incase updatedStartDate is in more than 2 years we don't update the index (per Ira's request)
            if (RabbitHelper.IsFutureIndexUpdate(previousStartDate, startDate))
            {
                if (!RabbitHelper.InsertFreeItemsIndexUpdate(groupId, ApiObjects.eObjectType.Media, new List<long>() { assetId }, startDate.Value))
                {
                    log.ErrorFormat("Failed inserting free items index update for startDate: {0}, mediaID: {1}, groupID: {2}", startDate.Value, assetId, groupId);
                }
            }

            // check if changes in the end date require future index update call, incase updatedEndDate is in more than 2 years we don't update the index (per Ira's request)
            if (RabbitHelper.IsFutureIndexUpdate(previousEndDate, endDate))
            {
                if (!RabbitHelper.InsertFreeItemsIndexUpdate(groupId, ApiObjects.eObjectType.Media, new List<long>() { assetId }, endDate.Value))
                {
                    log.ErrorFormat("Failed inserting free items index update for endDate: {0}, mediaID: {1}, groupID: {2}", endDate.Value, assetId, groupId);
                }
            }
        }

        private static MediaFileType GetMediaFileType(int groupId, int id)
        {
            MediaFileType result = null;
            GenericListResponse<MediaFileType> mediaFileTypesResponse = Instance.GetMediaFileTypes(groupId);
            if (mediaFileTypesResponse != null &&
                mediaFileTypesResponse.Status != null &&
                mediaFileTypesResponse.Status.Code == (int)eResponseStatus.OK &&
                mediaFileTypesResponse.Objects.Count > 0)
            {
                result = mediaFileTypesResponse.Objects.Where(x => x.Id == id).Count() == 1 ? mediaFileTypesResponse.Objects.Where(x => x.Id == id).First() : null;
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

        private static void TryInvalidateLabels(long groupId, AssetFile assetFile)
        {
            var assetLabelValues = GetLabelValues(assetFile.Labels);
            var labelsResult = _labelRepository.List(groupId);
            if (labelsResult.IsOkStatusCode()
                && assetLabelValues.Any(x => labelsResult.Objects.All(_ => x != _.Value && _.EntityAttribute == EntityAttribute.MediaFileLabels)))
            {
                _labelRepository.InvalidateCache(groupId);
            }
        }

        private static IReadOnlyCollection<string> GetLabelValues(string labels)
        {
            return string.IsNullOrEmpty(labels)
                ? new string[0]
                : labels
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
        }

        private static IReadOnlyCollection<string> GetValidLabelValues(string labels)
        {
            const int labelValueMaxLength = 128;
            const int maxMediaFileLabelsCount = 25;

            var labelValues = GetLabelValues(labels).OrderBy(x => x.Length).ToArray();
            if (labelValues.Length > maxMediaFileLabelsCount)
            {
                log.Warn($"Media file can contain up to {maxMediaFileLabelsCount} labels but is attempts to save {labelValues.Length} labels. Only first {maxMediaFileLabelsCount} valid labels will be saved.");
            }

            var validLabelValues = labelValues.Where(x => x.Length <= labelValueMaxLength).Take(maxMediaFileLabelsCount).ToArray();
            var invalidLabelValues = labelValues.Except(validLabelValues);

            foreach (var invalidLabelValue in invalidLabelValues)
            {
                if (invalidLabelValue.Length > labelValueMaxLength)
                {
                    log.Warn($"Label {invalidLabelValue} can not be saved. Maximum length of a label value is {labelValueMaxLength}.");
                }
                else
                {
                    log.Warn($"Label {invalidLabelValue} can not be saved.");
                }
            }

            return validLabelValues;
        }

        private static void SetLabels(AssetFile assetFile, IEnumerable<string> labelValues)
        {
            assetFile.Labels = labelValues == null
                ? string.Empty
                : string.Join(",", labelValues);
        }

        private static IEnumerable<KeyValuePair<string, string>> GetValidDynamicDataList(IDictionary<string, IEnumerable<string>> dynamicData)
        {
            return dynamicData
                .SelectMany(x => x.Value.Select(_ => new KeyValuePair<string, string>(x.Key, _)))
                .ToArray();
        }

        private static bool ValidateMediaFileDynamicData(MediaFileType mediaFileType, IDictionary<string, IEnumerable<string>> dynamicData, out Status status)
        {
            if (dynamicData == null || dynamicData.Count == 0)
            {
                status = Status.Ok;
            }
            else
            {
                var validKeys = mediaFileType.DynamicDataKeys ?? new HashSet<string>();
                var missingKeys = dynamicData.Keys.Except(validKeys).ToArray();
                if (missingKeys.Any())
                {
                    status = new Status(
                        eResponseStatus.DynamicDataKeyDoesNotExist,
                        $"DynamicData key does not exist in mediaFileType [{mediaFileType.Id}] for the following keys: [{string.Join(",", missingKeys)}].");
                }
                else
                {
                    status = Status.Ok;
                }
            }

            return status.IsOkStatusCode();
        }

        #endregion

        #region Internal Methods

        internal static List<AssetFile> CreateAssetFileListResponseFromDataTable(int groupId, DataTable dt, DataTable labelsTable, DataTable dynamicDataTable, bool shouldAddBaseUrl = true)
        {
            var response = new List<AssetFile>();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    AssetFile assetFile = CreateAssetFile(groupId, dr, labelsTable, dynamicDataTable, shouldAddBaseUrl);
                    if (assetFile != null)
                    {
                        response.Add(assetFile);
                    }
                }
            }

            return response;
        }

        internal static AssetFile GetAssetFileById(int groupId, long id, bool shouldAddBaseUrl = true)
        {
            var ds = CatalogDAL.GetMediaFile(groupId, id);
            var result = CreateAssetFileResponseFromDataSet(groupId, ds, shouldAddBaseUrl);

            return result?.IsOkStatusCode() == false
                ? null
                : result.Object;
        }

        internal static List<AssetFile> GetAssetFilesByAssetId(int groupId, long assetId, bool shouldAddBaseUrl = true)
        {
            List<AssetFile> files = new List<AssetFile>();
            DataSet ds = CatalogDAL.GetMediaFilesByAssetIds(groupId, new List<long> { assetId });
            if (ds?.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
            {
                var assetsTable = ds.Tables[0];
                var labelsTable = ds.Tables.Count > 1 ? ds.Tables[1] : null;
                var dynamicDataTable = ds.Tables.Count > 2 ? ds.Tables[2] : null;
                files = CreateAssetFileListResponseFromDataTable(groupId, assetsTable, labelsTable, dynamicDataTable, shouldAddBaseUrl);
            }

            return files;
        }
        #endregion

        #region Public Methods

        public class FileTypes
        {
            private readonly Lazy<Dictionary<int, MediaFileType>> _fileTypeMap;

            public FileTypes(int groupId, IMediaFileTypeManager fileManager)
            {
                _fileTypeMap = new Lazy<Dictionary<int, MediaFileType>>(() =>
                    fileManager.GetMediaFileTypes(groupId).GetOrThrow().ToDictionary(k => (int) k.Id, v => v));
            }

            public MediaFileType GetFileType(int? fileTypeId)
            {
                return fileTypeId == null ? null : _fileTypeMap.Value.GetValueOrDefault(fileTypeId.Value);
            }
        }

        public GenericListResponse<MediaFileType> GetMediaFileTypes(int groupId)
        {
            GenericListResponse<MediaFileType> response = new GenericListResponse<MediaFileType>();
            try
            {
                response.Objects = GetGroupMediaFileTypes(groupId);
                if (response.Objects != null)
                {
                    response.TotalItems = response.Objects.Count;
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetMediaFileTypes with groupId: {0}", groupId), ex);
            }

            return response;
        }

        public static GenericResponse<MediaFileType> AddMediaFileType(int groupId, MediaFileType mediaFileTypeToAdd, long userId)
        {
            GenericResponse<MediaFileType> result = new GenericResponse<MediaFileType>();
            try
            {
                DataSet ds = CatalogDAL.InsertMediaFileType(groupId, mediaFileTypeToAdd.Name, mediaFileTypeToAdd.Description, mediaFileTypeToAdd.IsActive, mediaFileTypeToAdd.IsTrailer,
                                                            (int)mediaFileTypeToAdd.StreamerType, mediaFileTypeToAdd.DrmId, mediaFileTypeToAdd.Quality, mediaFileTypeToAdd.VideoCodecs,
                                                            mediaFileTypeToAdd.AudioCodecs, mediaFileTypeToAdd.DynamicDataKeys, userId);
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

        public static GenericResponse<MediaFileType> UpdateMediaFileType(int groupId, long id, MediaFileType mediaFileTypeToUpdate, long userId)
        {
            GenericResponse<MediaFileType> result = new GenericResponse<MediaFileType>();
            try
            {
                DataSet ds = CatalogDAL.UpdateMediaFileType(groupId, id, mediaFileTypeToUpdate.Name, mediaFileTypeToUpdate.Description, mediaFileTypeToUpdate.IsActive, mediaFileTypeToUpdate.Quality,
                                                            mediaFileTypeToUpdate.VideoCodecs, mediaFileTypeToUpdate.AudioCodecs, mediaFileTypeToUpdate.DynamicDataKeys, userId);
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
                DataSet ds = CatalogDAL.DeleteMediaFileType(groupId, id, userId);
                if (ds?.Tables != null && ds.Tables.Count > 0)
                {
                    /* We don't care about the first table, we just checked to see count > 0 to know if the media file type was deleted successfully
                       The second table is needed to invalidate the assets that had media files of this file type  */

                    result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    string invalidationKey = LayeredCacheKeys.GetGroupMediaFileTypesInvalidationKey(groupId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on DeleteMediaFileType key = {0}", invalidationKey);
                    }

                    DataTable mediaFilesDt = ds.Tables.Count > 1 && ds.Tables[1] != null ? ds.Tables[1] : null;
                    if (mediaFilesDt != null && mediaFilesDt.Rows != null && mediaFilesDt.Rows.Count > 0)
                    {
                        // preparing media list for updating index and invalidating cache
                        List<int> mediaIds = new List<int>();
                        foreach (DataRow dr in mediaFilesDt.Rows)
                        {
                            int mediaId = ODBCWrapper.Utils.GetIntSafeVal(dr, "MEDIA_ID");
                            if (mediaId > 0)
                            {
                                mediaIds.Add(mediaId);
                            }
                        }

                        if (!CatalogManager.InvalidateCacheAndUpdateIndexForAssets(groupId, false, mediaIds, null))
                        {
                            log.ErrorFormat("Failed InvalidateCacheAndUpdateIndexForAssets after DeleteMediaFileType, groupId: {0}, mediaFileType: {1}", groupId, id);
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

        public GenericResponse<AssetFile> InsertMediaFile(int groupId, long userId, AssetFile assetFileToAdd, bool isFromIngest = false)
        {
            GenericResponse<AssetFile> result = new GenericResponse<AssetFile>();
            try
            {
                if (!isFromIngest)
                {
                    // validate that asset exist - isAllowedToViewInactiveAssets = true becuase only operator can insert media file
                    GenericResponse<Asset> assetResponse = AssetManager.Instance.GetAsset(groupId, assetFileToAdd.AssetId, eAssetTypes.MEDIA, true);
                    if (assetResponse == null || !assetResponse.HasObject())
                    {
                        result.SetStatus(eResponseStatus.AssetDoesNotExist, eResponseStatus.AssetDoesNotExist.ToString());
                        return result;
                    }

                    // validate that asset file type exist
                    List<MediaFileType> mediaFileTypes = GetGroupMediaFileTypes(groupId);
                    if (mediaFileTypes == null || mediaFileTypes.Count < 1)
                    {
                        result.SetStatus(eResponseStatus.MediaFileTypeDoesNotExist, eResponseStatus.MediaFileTypeDoesNotExist.ToString());
                        return result;
                    }

                    var mediaFileType = mediaFileTypes.FirstOrDefault(x => x.Id == assetFileToAdd.TypeId);
                    if (mediaFileType == null)
                    {
                        result.SetStatus(eResponseStatus.MediaFileTypeDoesNotExist, eResponseStatus.MediaFileTypeDoesNotExist.ToString());
                        return result;
                    }

                    if (!ValidateMediaFileDynamicData(mediaFileType, assetFileToAdd.DynamicData, out var mediaFileDynamicDataStatus))
                    {
                        result.SetStatus(mediaFileDynamicDataStatus);
                        return result;
                    }

                    // validate media doesn't already have a file with this type
                    List<AssetFile> assetFiles = GetAssetFilesByAssetId(groupId, assetFileToAdd.AssetId);
                    if (assetFiles != null && assetFiles.Count > 0 && assetFiles.Any(x => x.TypeId == assetFileToAdd.TypeId))
                    {
                        result.SetStatus(eResponseStatus.MediaFileWithThisTypeAlreadyExistForAsset, eResponseStatus.MediaFileWithThisTypeAlreadyExistForAsset.ToString());
                        return result;
                    }
                }

                // validate CdnAdapaterProfileId (0 or null for setting group default adapter) \ AlternativeCdnAdapaterProfileId
                Status validateCdnReponse = ValidateCdnAdapterProfileIds(groupId, assetFileToAdd.CdnAdapaterProfileId, assetFileToAdd.AlternativeCdnAdapaterProfileId);
                if (validateCdnReponse.Code != (int)eResponseStatus.OK)
                {
                    result.SetStatus(validateCdnReponse);
                    return result;
                }
                else if (!assetFileToAdd.CdnAdapaterProfileId.HasValue || assetFileToAdd.CdnAdapaterProfileId.Value == 0)
                {
                    ApiObjects.CDNAdapter.CDNAdapterResponse GroupDefaultCdnAdapter = Core.Api.api.GetGroupDefaultCdnAdapter(groupId, eAssetTypes.MEDIA);
                    if (GroupDefaultCdnAdapter == null || GroupDefaultCdnAdapter.Status == null || GroupDefaultCdnAdapter.Status.Code != (int)eResponseStatus.OK
                        || GroupDefaultCdnAdapter.Adapter == null || GroupDefaultCdnAdapter.Adapter.ID <= 0)
                    {
                        result.SetStatus(eResponseStatus.DefaultCdnAdapterProfileNotConfigurd, eResponseStatus.DefaultCdnAdapterProfileNotConfigurd.ToString());
                        return result;
                    }

                    assetFileToAdd.CdnAdapaterProfileId = GroupDefaultCdnAdapter.Adapter.ID;
                }

                DateTime startDate = assetFileToAdd.StartDate ?? DateTime.UtcNow;

                DataSet ds = CatalogDAL.InsertMediaFile(groupId, userId, assetFileToAdd.AdditionalData, assetFileToAdd.AltStreamingCode, assetFileToAdd.AlternativeCdnAdapaterProfileId, assetFileToAdd.AssetId,
                                                        assetFileToAdd.BillingType, assetFileToAdd.Duration, assetFileToAdd.EndDate, assetFileToAdd.ExternalId, assetFileToAdd.ExternalStoreId, assetFileToAdd.FileSize,
                                                        assetFileToAdd.IsDefaultLanguage, assetFileToAdd.Language, assetFileToAdd.OrderNum, startDate, assetFileToAdd.Url, assetFileToAdd.CdnAdapaterProfileId,
                                                        assetFileToAdd.TypeId, assetFileToAdd.AltExternalId, assetFileToAdd.IsActive, assetFileToAdd.CatalogEndDate, GetValidLabelValues(assetFileToAdd.Labels),
                                                        GetValidDynamicDataList(assetFileToAdd.DynamicData), assetFileToAdd.Opl);
                result = CreateAssetFileResponseFromDataSet(groupId, ds);

                if (result.Status.Code == (int)eResponseStatus.OK)
                {
                    string errorMsg = string.Empty;
                    ImporterImpl.SetPolicyToFile(assetFileToAdd.OutputProtecationLevel, groupId, assetFileToAdd.ExternalId, ref errorMsg);
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        log.ErrorFormat("Failed to SetPolicyToFile for assetId: {0}, groupId: {1} after InsertMediaFile with error message: {2}", assetFileToAdd.AssetId, groupId, errorMsg);
                    }

                    if (!isFromIngest)
                    {
                        InvalidateAssetAfterFilesUpdated(groupId, assetFileToAdd.AssetId, userId);

                        // publish asset updated event to Kafka
                        MediaAssetCrudMessageService.Instance.PublishKafkaUpdateEvent(groupId, assetFileToAdd.AssetId, userId);
                    }

                    // free item index update
                    DoFreeItemIndexUpdateIfNeeded(groupId, (int)assetFileToAdd.AssetId, null, assetFileToAdd.StartDate, null, assetFileToAdd.EndDate);

                    TryInvalidateLabels(groupId, result.Object);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddAssetFile for groupId: {0}", groupId), ex);
            }

            return result;
        }

        public Status DeleteMediaFile(int groupId, long userId, long id, bool isFromIngest = false)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                DataSet ds = CatalogDAL.GetMediaFile(groupId, id);
                GenericResponse<AssetFile> assetFileResponse = CreateAssetFileResponseFromDataSet(groupId, ds);
                if (assetFileResponse != null && assetFileResponse.Status != null && assetFileResponse.Status.Code != (int)eResponseStatus.OK)
                {
                    return assetFileResponse.Status;
                }

                if (CatalogDAL.DeleteMediaFile(groupId, userId, id))
                {
                    result.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    if (!isFromIngest)
                    {
                        InvalidateAssetAfterFilesUpdated(groupId, assetFileResponse.Object.AssetId, userId);

                        // publish asset updated event to Kafka
                        MediaAssetCrudMessageService.Instance.PublishKafkaUpdateEvent(groupId, assetFileResponse.Object.AssetId, userId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed DeleteAssetFile for groupId: {0} and AssetFileId: {1}", groupId, id), ex);
            }

            return result;
        }

        public GenericResponse<AssetFile> UpdateMediaFile(int groupId, AssetFile assetFileToUpdate, long userId, bool isFromIngest = false, AssetFile currentAssetFile = null)
        {
            GenericResponse<AssetFile> result = new GenericResponse<AssetFile>();
            try
            {
                if (!isFromIngest || currentAssetFile == null)
                {
                    DataSet dsCurrMediaFile = CatalogDAL.GetMediaFile(groupId, assetFileToUpdate.Id);
                    GenericResponse<AssetFile> currentAssetFileResponse = CreateAssetFileResponseFromDataSet(groupId, dsCurrMediaFile);

                    if (currentAssetFileResponse != null && currentAssetFileResponse.Status != null && currentAssetFileResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        return currentAssetFileResponse;
                    }

                    currentAssetFile = currentAssetFileResponse.Object;

                    if (currentAssetFile != null && currentAssetFile.AssetId != assetFileToUpdate.AssetId)
                    {
                        result.SetStatus(eResponseStatus.MediaFileNotBelongToAsset, eResponseStatus.MediaFileNotBelongToAsset.ToString());
                        return result;
                    }

                    // validate that asset file type exist
                    List<MediaFileType> mediaFileTypes = GetGroupMediaFileTypes(groupId);
                    if (mediaFileTypes == null || mediaFileTypes.Count < 1)
                    {
                        result.SetStatus(eResponseStatus.MediaFileTypeDoesNotExist, eResponseStatus.MediaFileTypeDoesNotExist.ToString());
                        return result;
                    }

                    MediaFileType mediaFileType = mediaFileTypes.FirstOrDefault(x => x.Id == assetFileToUpdate.TypeId);
                    if (mediaFileType == null)
                    {
                        result.SetStatus(eResponseStatus.MediaFileTypeDoesNotExist, eResponseStatus.MediaFileTypeDoesNotExist.ToString());
                        return result;
                    }

                    if (!ValidateMediaFileDynamicData(mediaFileType, assetFileToUpdate.DynamicData, out var mediaFileDynamicDataStatus))
                    {
                        result.SetStatus(mediaFileDynamicDataStatus);
                        return result;
                    }

                    // validate media doesn't already have a file with this type
                    if (assetFileToUpdate.TypeId.HasValue && assetFileToUpdate.Id != currentAssetFile.Id)
                    {
                        List<AssetFile> assetFiles = GetAssetFilesByAssetId(groupId, assetFileToUpdate.AssetId);
                        if (assetFiles != null && assetFiles.Count > 0 && assetFiles.Any(x => x.TypeId == assetFileToUpdate.TypeId && x.Id != assetFileToUpdate.Id))
                        {
                            result.SetStatus(eResponseStatus.MediaFileWithThisTypeAlreadyExistForAsset, eResponseStatus.MediaFileWithThisTypeAlreadyExistForAsset.ToString());
                            return result;
                        }
                    }
                }

                // ExternalId and AltExternalId cannot be the same value
                if (string.IsNullOrEmpty(assetFileToUpdate.ExternalId))
                {
                    assetFileToUpdate.ExternalId = currentAssetFile.ExternalId;
                }

                if (string.IsNullOrEmpty(assetFileToUpdate.AltExternalId) && !string.IsNullOrEmpty(currentAssetFile.AltExternalId))
                {
                    assetFileToUpdate.AltExternalId = currentAssetFile.AltExternalId;
                }

                // validate CdnAdapaterProfileId (0 or null for setting group default adapter) \ AlternativeCdnAdapaterProfileId
                Status validateCdnReponse = ValidateCdnAdapterProfileIds(groupId, assetFileToUpdate.CdnAdapaterProfileId, assetFileToUpdate.AlternativeCdnAdapaterProfileId);
                if (validateCdnReponse.Code != (int)eResponseStatus.OK)
                {
                    result.SetStatus(validateCdnReponse);
                    return result;
                }
                else if (assetFileToUpdate.CdnAdapaterProfileId.HasValue && assetFileToUpdate.CdnAdapaterProfileId.Value == 0)
                {
                    ApiObjects.CDNAdapter.CDNAdapterResponse GroupDefaultCdnAdapter = Core.Api.api.GetGroupDefaultCdnAdapter(groupId, eAssetTypes.MEDIA);
                    if (GroupDefaultCdnAdapter == null || GroupDefaultCdnAdapter.Status == null || GroupDefaultCdnAdapter.Status.Code != (int)eResponseStatus.OK
                        || GroupDefaultCdnAdapter.Adapter == null || GroupDefaultCdnAdapter.Adapter.ID <= 0)
                    {
                        result.SetStatus(eResponseStatus.DefaultCdnAdapterProfileNotConfigurd, eResponseStatus.DefaultCdnAdapterProfileNotConfigurd.ToString());
                        return result;
                    }

                    assetFileToUpdate.CdnAdapaterProfileId = GroupDefaultCdnAdapter.Adapter.ID;
                }

                var ds = CatalogDAL.UpdateMediaFile(groupId, assetFileToUpdate.Id, userId, assetFileToUpdate.AdditionalData, assetFileToUpdate.AltStreamingCode,
                                                    assetFileToUpdate.AlternativeCdnAdapaterProfileId, assetFileToUpdate.AssetId, assetFileToUpdate.BillingType,
                                                    assetFileToUpdate.Duration, assetFileToUpdate.EndDate, assetFileToUpdate.ExternalId, assetFileToUpdate.ExternalStoreId, assetFileToUpdate.FileSize,
                                                    assetFileToUpdate.IsDefaultLanguage, assetFileToUpdate.Language, assetFileToUpdate.OrderNum,
                                                    assetFileToUpdate.StartDate, assetFileToUpdate.Url, assetFileToUpdate.CdnAdapaterProfileId,
                                                    assetFileToUpdate.TypeId, assetFileToUpdate.AltExternalId, assetFileToUpdate.IsActive, assetFileToUpdate.CatalogEndDate,
                                                    assetFileToUpdate.Opl, GetValidLabelValues(assetFileToUpdate.Labels), GetValidDynamicDataList(assetFileToUpdate.DynamicData));

                result = CreateAssetFileResponseFromDataSet(groupId, ds);

                if (result.Status.Code == (int)eResponseStatus.OK)
                {
                    string errorMsg = string.Empty;
                    ImporterImpl.SetPolicyToFile(assetFileToUpdate.OutputProtecationLevel, groupId, assetFileToUpdate.ExternalId, ref errorMsg);
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        log.ErrorFormat("Failed to SetPolicyToFile for assetId: {0}, groupId: {1} after UpdateMediaFile with error message: {2}", result.Object.AssetId, groupId, errorMsg);
                    }

                    if (!isFromIngest)
                    {
                        InvalidateAssetAfterFilesUpdated(groupId, assetFileToUpdate.AssetId, userId);
                        // publish asset updated event to Kafka
                        MediaAssetCrudMessageService.Instance.PublishKafkaUpdateEvent(groupId, assetFileToUpdate.AssetId, userId);
                    }

                    // free item index update
                    DoFreeItemIndexUpdateIfNeeded(groupId, (int)assetFileToUpdate.AssetId, currentAssetFile.StartDate, assetFileToUpdate.StartDate,
                                                  currentAssetFile.EndDate, assetFileToUpdate.EndDate);

                    TryInvalidateLabels(groupId, result.Object);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateAssetFile for groupId: {0}", groupId), ex);
            }

            return result;
        }

        public GenericListResponse<AssetFile> GetMediaFiles(int groupId, long id, long assetId)
        {
            GenericListResponse<AssetFile> response = new GenericListResponse<AssetFile>();
            try
            {
                response.Objects = id > 0
                    ? new List<AssetFile> { GetAssetFileById(groupId, id, false) }
                    : GetAssetFilesByAssetId(groupId, assetId, false);

                if (response.Objects != null)
                {
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
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