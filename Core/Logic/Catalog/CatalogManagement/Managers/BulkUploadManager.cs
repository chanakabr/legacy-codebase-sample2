using ApiLogic;
using ApiLogic.Catalog;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.EventBus;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using ElasticSearch.Common;
using EventBus.RabbitMQ;
using Phx.Lib.Log;
using QueueWrapper;
using Synchronizer;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Helpers;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiObjects.EventBus.EpgIngest;
using Core.Api;
using Core.Catalog.CatalogManagement.Services;
using EventBus.Kafka;
using Tvinci.Core.DAL;
using TVinciShared;
using ESUtils = ElasticSearch.Common.Utils;

namespace Core.Catalog.CatalogManagement
{
    public class BulkUploadManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const uint BULK_UPLOAD_CB_TTL = 5184000; // 60 DAYS after all results in bulk upload are in status Success (in sec)

        private static readonly HashSet<BulkUploadJobStatus> FinishedBulkUploadStatuses = new HashSet<BulkUploadJobStatus>()
        {
            BulkUploadJobStatus.Success,
            BulkUploadJobStatus.Partial,
            BulkUploadJobStatus.Failed,
            BulkUploadJobStatus.Fatal
        };

        public static GenericResponse<BulkUpload> GetBulkUpload(int groupId, long bulkUploadId)
        {
            GenericResponse<BulkUpload> response = new GenericResponse<BulkUpload>();
            try
            {
                DataTable dt = CatalogDAL.GetBulkUpload(bulkUploadId, groupId);
                response.Object = CreateBulkUploadFromDataTable(dt, groupId, true);
                if (response.Object == null)
                {
                    response.SetStatus(eResponseStatus.BulkUploadDoesNotExist);
                    return response;
                }

                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in GetBulkUpload. bulkUploadId:{0}.", bulkUploadId), ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        public static GenericResponse<BulkUploadSummary> GetBulkUploadSummary(long groupId, string bulkObjectType, long CreateDateGreaterThanOrEqual)
        {
            var response = new GenericResponse<BulkUploadSummary>();
            var fileObjectTypeName = ApiLogic.FileManager.Instance.GetFileObjectTypeName(bulkObjectType);
            if (!fileObjectTypeName.HasObject())
            {
                response.SetStatus(fileObjectTypeName.Status);
                return response;
            }

            var dt = CatalogDAL.GetBulkUploadSummary(groupId, fileObjectTypeName.Object, CreateDateGreaterThanOrEqual);
            var summary = new BulkUploadSummary();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                var summaryDict = new Dictionary<BulkUploadJobStatus, long>();
                foreach (DataRow row in dt.Rows)
                {
                    var status = (BulkUploadJobStatus)ODBCWrapper.Utils.GetIntSafeVal(row, "STATUS");
                    summaryDict[status] = ODBCWrapper.Utils.GetLongSafeVal(row, "BULK_UPLOAD_COUNT");
                }

                summary.Pending = summaryDict.GetValueOrDefault(BulkUploadJobStatus.Pending);
                summary.Uploaded = summaryDict.GetValueOrDefault(BulkUploadJobStatus.Uploaded);
                summary.Queued = summaryDict.GetValueOrDefault(BulkUploadJobStatus.Queued);
                summary.Parsing = summaryDict.GetValueOrDefault(BulkUploadJobStatus.Parsing);
                summary.Processing = summaryDict.GetValueOrDefault(BulkUploadJobStatus.Processing);
                summary.Processed = summaryDict.GetValueOrDefault(BulkUploadJobStatus.Processed);
                summary.Success = summaryDict.GetValueOrDefault(BulkUploadJobStatus.Success);
                summary.Partial = summaryDict.GetValueOrDefault(BulkUploadJobStatus.Partial);
                summary.Failed = summaryDict.GetValueOrDefault(BulkUploadJobStatus.Failed);
                summary.Fatal = summaryDict.GetValueOrDefault(BulkUploadJobStatus.Fatal);
            }

            response.Object = summary;
            response.SetStatus(Status.Ok);
            return response;
        }

        public static GenericListResponse<BulkUpload> GetBulkUploads(int groupId, string bulkObjectType, DateTime createDate, List<BulkUploadJobStatus> statuses = null, long? userId = null)
        {
            var response = new GenericListResponse<BulkUpload>();
            try
            {
                var fileObjectTypeName = ApiLogic.FileManager.Instance.GetFileObjectTypeName(bulkObjectType);
                if (!fileObjectTypeName.HasObject())
                {
                    response.SetStatus(fileObjectTypeName.Status);
                    return response;
                }

                // get all statuses if parameter is null or empty
                if (statuses == null)
                {
                    statuses = new List<BulkUploadJobStatus>();
                }
                if (statuses.Count == 0)
                {
                    statuses.Add(BulkUploadJobStatus.Pending);
                    statuses.Add(BulkUploadJobStatus.Uploaded);
                    statuses.Add(BulkUploadJobStatus.Queued);
                    statuses.Add(BulkUploadJobStatus.Parsing);
                    statuses.Add(BulkUploadJobStatus.Processing);
                    statuses.Add(BulkUploadJobStatus.Processed);
                    statuses.Add(BulkUploadJobStatus.Success);
                    statuses.Add(BulkUploadJobStatus.Partial);
                    statuses.Add(BulkUploadJobStatus.Failed);
                    statuses.Add(BulkUploadJobStatus.Fatal);
                }

                Dictionary<string, string> keyToOriginalValueMap = new Dictionary<string, string>(statuses.Count);
                Dictionary<string, List<string>> invalidationKeysMap = new Dictionary<string, List<string>>(statuses.Count);
                List<int> statusesIn = new List<int>(statuses.Count);
                foreach (var status in statuses)
                {
                    var nStatus = (int)status;
                    var bulkUploadKey = LayeredCacheKeys.GetBulkUploadsKey(groupId, fileObjectTypeName.Object, nStatus);
                    keyToOriginalValueMap.Add(bulkUploadKey, ((int)status).ToString());
                    invalidationKeysMap.Add(bulkUploadKey, new List<string>() { LayeredCacheKeys.GetBulkUploadsInvalidationKey(groupId, fileObjectTypeName.Object, nStatus) });
                    statusesIn.Add(nStatus);
                }

                var bulkUploadsMap = new Dictionary<string, List<BulkUpload>>();

                if (!LayeredCache.Instance.GetValues(keyToOriginalValueMap,
                                                     ref bulkUploadsMap,
                                                     GetBulkUploadsFromCache,
                                                     new Dictionary<string, object>()
                                                     {
                                                         { "groupId", groupId },
                                                         { "bulkObjectType", fileObjectTypeName.Object },
                                                         { "statusesIn", statusesIn }
                                                     },
                                                     groupId,
                                                     LayeredCacheConfigNames.GET_BULK_UPLOADS_FROM_CACHE,
                                                     invalidationKeysMap))
                {
                    log.ErrorFormat("GetBulkUploads - GetBulkUploadsFromCache - Failed get data from cache. groupId: {0}", groupId);
                    return response;
                }

                if (bulkUploadsMap != null && bulkUploadsMap.Count > 0)
                {
                    response.Objects.AddRange(bulkUploadsMap.SelectMany(x => x.Value).Where(x => x.CreateDate >= createDate));

                    if (response.Objects.Count > 0 && userId.HasValue)
                    {
                        response.Objects = response.Objects.Where(x => x.UpdaterId == userId.Value).ToList();
                    }
                }

                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in GetBulkUploads. groupId:{0}, fileObjectType:{1}, userId:{2}, uploadDate:{3}.",
                                        groupId, bulkObjectType, userId, createDate), ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        public static GenericResponse<BulkUpload> AddBulkUpload(int groupId,
            long userId,
            string objectTypeName,
            BulkUploadJobAction action,
            BulkUploadJobData jobData,
            BulkUploadObjectData objectData,
            OTTBasicFile fileData)
        {
            var response = new GenericResponse<BulkUpload>();
            try
            {
                // create and save the new BulkUpload in DB (with no results)
                var dt = CatalogDAL.AddBulkUpload(groupId, userId, action, fileData.Name);
                response.Object = CreateBulkUploadFromDataTable(dt, groupId);
                if (response.Object == null)
                {
                    response.SetStatus(eResponseStatus.BulkUploadDoesNotExist);
                    return response;
                }

                response.Object.JobData = jobData;
                objectData.GroupId = groupId;
                response.Object.ObjectData = objectData;

                if (!CatalogDAL.SaveBulkUploadCB(response.Object, BULK_UPLOAD_CB_TTL))
                {
                    log.ErrorFormat("Error while saving BulkUpload to CB. groupId: {0}, BulkUpload.Id:{1}", groupId, response.Object.Id);
                    return response;
                }

                // save the bulkUpload file to server (cut it from iis) and set fileURL                                
                GenericResponse<string> saveFileResponse = ApiLogic.FileManager.Instance.SaveFile(response.Object.Id, fileData, "KalturaBulkUpload");
                if (!saveFileResponse.HasObject())
                {
                    log.ErrorFormat("Error while saving BulkUpload File to file server. groupId: {0}, BulkUpload.Id:{1}", groupId, response.Object.Id);
                    response.SetStatus(saveFileResponse.Status);
                    return response;
                }
                response.Object.FileURL = saveFileResponse.Object;

                var objectTypeNameWithoutKalturaPrefix = ApiLogic.FileManager.Instance.GetFileObjectTypeName(objectTypeName);
                response.Object.BulkObjectType = objectTypeNameWithoutKalturaPrefix.Object;
                response = UpdateBulkUpload(response.Object, BulkUploadJobStatus.Uploaded);

                //
                // We are in the beginning of a transition to .NET core and new scheduled tasks architecture.
                // Previous setting is using celery and enqueuing a celery-based message to rabbit (with QueueWrapper of old .NET)
                // Next setting is using our own handlers (consumers) on rabbit (using EventBus.RabbitMQ of .NET core)
                // Currently Ingest Job Data uses the next setting, and excel job data still uses the previous settings
                // Everything in our system will move to the next setting as soon as possible - but gradually
                //

                // Enqueue to CeleryQueue new BulkUpload (the remote will handle the file and its content).
                if (jobData is BulkUploadIngestJobData ingestJobData)
                {
                    var epgFeatureVersion = GroupManagers.GroupSettingsManager.Instance.GetEpgFeatureVersion(groupId);
                    if (epgFeatureVersion == EpgFeatureVersion.V1)
                    {
                        var msg = $"AddBulkUpload > GroupId :[{groupId}]. epg ingest using bulk upload is not supported for this account";
                        log.Warn(msg);
                        response.Object.AddError(eResponseStatus.Error, msg);
                        response = UpdateBulkUpload(response.Object, BulkUploadJobStatus.Fatal);
                        response.SetStatus(eResponseStatus.AccountEpgIngestVersionDoesNotSupportBulk, msg);
                        return response;
                    }

                    response = UpdateBulkUploadStatusWithVersionCheck(response.Object, BulkUploadJobStatus.Queued);
                    SendTransformationEventToServiceEventBus(groupId, userId, response.Object.Id,
                        ingestJobData.IngestProfileId, response.Object.FileName, response.Object.CreateDate);
                }
                else
                {
                    //use the regular celery-remoteTasks mechanism to avoid breaks in exsisting upload from excel
                    // TODO: Arthur make the excel upload work using the service bus as well.
                    response = EnqueueExcelBulkUploadJobUsingCelery(groupId, userId, response);
                }
            }
            catch (Exception ex)
            {
                log.Debug($"An Exception was occurred in AddBulkUpload. details:{ex.ToString()}.");
                log.Error($"An Exception was occurred in AddBulkUpload. groupId:{groupId}, filename:{fileData.Name}, userId:{userId}, action:{action}, objectType:{objectTypeName}.", ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        private static void SendTransformationEventToServiceEventBus(int groupId, long userId, long bulkUploadId,
            int? ingestProfileId, string ingestFileName, DateTime createdDate)
        {
            var publisher = EventBusPublisherRabbitMQ.GetInstanceUsingTCMConfiguration();
            var transformationEvent = new BulkUploadTransformationEvent
            {
                RequestId = KLogger.GetRequestId(),
                GroupId = groupId,
                UserId = userId,
                BulkUploadId = bulkUploadId,
            };
            publisher.Publish(transformationEvent);
            var parameters = new EpgIngestStartedParameters
            {
                GroupId = groupId,
                UserId = userId,
                BulkUploadId = bulkUploadId,
                IngestProfileId = ingestProfileId,
                IngestFileName = ingestFileName,
                CreatedDate = createdDate
            };

            EpgIngestMessaging.Instance.EpgIngestStarted(parameters);
        }

        private static GenericResponse<BulkUpload> EnqueueExcelBulkUploadJobUsingCelery(int groupId, long userId, GenericResponse<BulkUpload> response)
        {
            if (EnqueueBulkUpload(groupId, response.Object, userId))
            {
                response = UpdateBulkUploadStatusWithVersionCheck(response.Object, BulkUploadJobStatus.Queued);
            }
            else
            {
                if (EnqueueBulkUpload(groupId, response.Object, userId))
                {
                    response = UpdateBulkUploadStatusWithVersionCheck(response.Object, BulkUploadJobStatus.Queued);
                }
                else
                {
                    string msg = string.Format("Failed to enqueue BulkUpload, BulkUpload.Id:{0}", response.Object.Id);
                    response.Object.AddError(eResponseStatus.Error, msg);
                    response = UpdateBulkUploadStatusWithVersionCheck(response.Object, BulkUploadJobStatus.Fatal);
                    response.SetStatus(eResponseStatus.EnqueueFailed, msg);
                }
            }

            return response;
        }

        // TODO: Arthur, remove this overload when the excel bulk upload is using the event bus instead of celery
        public static Status ProcessBulkUpload(int groupId, long userId, long bulkUploadId)
        {
            var bulkUploadResponse = GetBulkUpload(groupId, bulkUploadId);
            if (!bulkUploadResponse.HasObject())
            {
                return bulkUploadResponse.Status;
            }

            var status = ProcessBulkUpload(groupId, userId, bulkUploadResponse.Object);
            return status;
        }

        public static Status ProcessBulkUpload(int groupId, long userId, BulkUpload bulkUpload)
        {
            var bulkUploadResponse = new GenericResponse<BulkUpload> { Object = bulkUpload };
            bulkUploadResponse.SetStatus(eResponseStatus.OK);

            try
            {
                bulkUploadResponse.Object.UpdaterId = userId;
                bulkUploadResponse = UpdateBulkUploadStatusWithVersionCheck(bulkUploadResponse.Object, BulkUploadJobStatus.Parsing);

                var bulkUploadValidationStatus = ValidateBulkUpload(groupId, bulkUpload, bulkUploadResponse);
                if (!bulkUploadValidationStatus.IsOkStatusCode()) { return bulkUploadValidationStatus; }

                var objectsListResponse = ParseBulkUploadData(bulkUpload, bulkUploadResponse);
                if (!objectsListResponse.IsOkStatusCode()) { return objectsListResponse.Status; }

                bulkUploadResponse.Object.NumOfObjects = objectsListResponse.Objects.Count;
                if (bulkUploadResponse.Object.NumOfObjects == 0)
                {
                    log.Error($"ProcessBulkUpload Deserialize file with no objects. groupId:{groupId}, bulkUploadId:{bulkUpload.Id}.");
                    bulkUploadResponse = UpdateBulkUpload(bulkUploadResponse.Object, BulkUploadJobStatus.Success);
                    return bulkUploadResponse.Status;
                }

                bulkUploadResponse.Object.Results = objectsListResponse.Objects;
                var hasErrors = bulkUploadResponse.Object.Results.Any(o => o.Status == BulkUploadResultStatus.Error);
                if (hasErrors)
                {
                    log.Debug($"ProcessBulkUpload some seserialize objects has errors therefor all bulk objects will not enqueue. groupId:{groupId}, bulkUploadId:{bulkUpload.Id}.");
                    bulkUploadResponse = UpdateBulkUpload(bulkUploadResponse.Object, BulkUploadJobStatus.Failed);
                }
                else
                {
                    bulkUploadResponse = UpdateBulkUpload(bulkUploadResponse.Object, BulkUploadJobStatus.Processing);
                    bulkUploadResponse.Object.ObjectData.EnqueueObjects(bulkUploadResponse.Object, objectsListResponse.Objects);
                    bulkUploadResponse = UpdateBulkUploadStatusWithVersionCheck(bulkUploadResponse.Object, BulkUploadJobStatus.Processed);
                    log.Debug($"ProcessBulkUpload finish to Enqueue all BulkUpload Objects. groupId:{groupId}, bulkUploadId:{bulkUpload.Id}.");
                }

                log.Debug($"finish to ProcessBulkUpload. groupId:{groupId}, bulkUploadId:{bulkUpload.Id}.");
            }
            catch (Exception ex)
            {
                string msg = $"An Exception was occurred in ProcessBulkUpload. groupId:{groupId}, userId:{userId}, bulkUploadId:{bulkUpload.Id}.";
                if (ex is OverlapException)
                {
                    msg += ex.Message;
                }
                bulkUploadResponse.Object.AddError(eResponseStatus.Error, msg);
                bulkUploadResponse = UpdateBulkUploadStatusWithVersionCheck(bulkUploadResponse.Object, BulkUploadJobStatus.Fatal);
                log.Error(msg, ex);
                bulkUploadResponse.SetStatus(eResponseStatus.Error);
            }

            return bulkUploadResponse.Status;
        }

        private static Status ValidateBulkUpload(int groupId, BulkUpload bulkUpload, GenericResponse<BulkUpload> bulkUploadResponse)
        {
            if (bulkUploadResponse.Object.JobData != null && bulkUploadResponse.Object.ObjectData != null)
            {
                return Status.Ok;
            }

            // else update the bulk upload object that an error occured
            var errorMessage = string.Format("ProcessBulkUpload cannot Deserialize file because JobData or ObjectData are null for groupId:{0}, bulkUploadId:{1}.", groupId, bulkUpload.Id);
            log.Error(errorMessage);
            bulkUploadResponse.Object.AddError(eResponseStatus.Error, $"Error validate bulk upload. groupId: {groupId}, bulkUploadId: {bulkUpload.Id}");
            UpdateBulkUploadStatusWithVersionCheck(bulkUploadResponse.Object, BulkUploadJobStatus.Fatal);
            bulkUploadResponse.SetStatus(eResponseStatus.Error, errorMessage);
            return bulkUploadResponse.Status;
        }

        private static GenericListResponse<BulkUploadResult> ParseBulkUploadData(BulkUpload bulkUpload, GenericResponse<BulkUpload> bulkUploadResponse)
        {
            var objectsListResponse = bulkUploadResponse.Object.JobData.Deserialize(bulkUpload.GroupId, bulkUpload.Id, bulkUploadResponse.Object.FileURL, bulkUploadResponse.Object.ObjectData);

            if (objectsListResponse.IsOkStatusCode())
            {
                if (objectsListResponse.Objects.Any(x => x.Errors?.Length > 0))
                {
                    bulkUploadResponse.Object.AddError(eResponseStatus.Error, "Errors found during deserialization, review errors on result items.");
                }
                return objectsListResponse;
            }
            else
            {
                bulkUploadResponse.SetStatus(objectsListResponse.Status);
            }

            var errorMessage = string.Format("Error while trying to deserialize file. bulkUpload.Id:[{0}],  bulkUploadResponse.Status.Code:[{1}] bulkUploadResponse.Status.Message:[{2}].",
                bulkUpload.Id, bulkUploadResponse.Status.Code, bulkUploadResponse.Status.Message);
            log.Error(errorMessage);
            bulkUploadResponse.Object.AddError(eResponseStatus.Error, errorMessage);
            UpdateBulkUploadStatusWithVersionCheck(bulkUploadResponse.Object, BulkUploadJobStatus.Failed);

            return objectsListResponse;
        }

        public static Status UpdateBulkUploadResults(IEnumerable<BulkUploadResult> results, out BulkUploadJobStatus status)
        {
            status = BulkUploadJobStatus.Processing;
            var response = new Status((int)eResponseStatus.Error);

            try
            {
                var resultsToSave = results.ToList();
                var isSuccess = CatalogDAL.SaveBulkUploadResultsCB(resultsToSave, BULK_UPLOAD_CB_TTL, out status);
                if (!isSuccess)
                {
                    log.ErrorFormat("UpdateBulkUploadResults - Error while saving to CB. results:[{0}]", string.Join(",", results));
                    response.Set(eResponseStatus.Error);
                    return response;
                }

                response.Set(eResponseStatus.OK);
            }
            catch (Exception)
            {

                log.Error(string.Format("An Exception was occurred in UpdateBulkUploadResult. results:[{0}]", string.Join(",", results)));
                response.Set(eResponseStatus.Error);
            }

            return response;
        }

        public static Status UpdateBulkUploadResult(int groupId, long bulkUploadId, int resultIndex, Status errorStatus = null, long? objectId = null, List<Status> warnings = null)
        {
            var response = new Status((int)eResponseStatus.Error);
            try
            {
                var bulkUploadResponse = GetBulkUpload(groupId, bulkUploadId);
                var originalStatus = bulkUploadResponse.Object.Status;
                bulkUploadResponse.Object.Results[resultIndex].ObjectId = objectId;

                if (errorStatus == null)
                {
                    bulkUploadResponse.Object.Results[resultIndex].Status = BulkUploadResultStatus.Ok;
                }
                else
                {
                    bulkUploadResponse.Object.Results[resultIndex].AddError(errorStatus);
                }

                if (warnings != null && warnings.Count > 0)
                {
                    bulkUploadResponse.Object.Results[resultIndex].Warnings = warnings.ToArray();
                }

                BulkUploadJobStatus updatedStatus;
                if (!CatalogDAL.SaveBulkUploadResultCB(bulkUploadResponse.Object, resultIndex, BULK_UPLOAD_CB_TTL, out updatedStatus))
                {
                    log.ErrorFormat("UpdateBulkUploadResult - Error while saving BulkUpload to CB. bulkUploadId:{0}, resultIndex:{1}.", bulkUploadId, resultIndex);
                }
                bulkUploadResponse.Object.Status = updatedStatus;

                UpdateBulkUploadInSqlAndInvalidateKeys(bulkUploadResponse.Object, originalStatus);
                response.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in UpdateBulkUploadResult. groupId:{0}, bulkUploadId:{1}, resultIndex:{2}",
                                        groupId, bulkUploadId, resultIndex), ex);
                response.Set(eResponseStatus.Error);
            }

            return response;
        }

        public static GenericResponse<BulkUpload> UpdateBulkUpload(BulkUpload bulkUploadToUpdate, BulkUploadJobStatus newStatus)
        {
            var response = new GenericResponse<BulkUpload>();
            try
            {
                var originalStatus = bulkUploadToUpdate.Status;
                response.Object = bulkUploadToUpdate;
                response.Object.Status = newStatus;

                if (!CatalogDAL.SaveBulkUploadCB(response.Object, BULK_UPLOAD_CB_TTL))
                {
                    log.ErrorFormat("UpdateBulkUpload - Error while saving BulkUpload to CB. bulkUploadId:{0}, status:{1}.", response.Object.Id, bulkUploadToUpdate.Status);
                }

                UpdateBulkUploadInSqlAndInvalidateKeys(response.Object, originalStatus);
                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in UpdateBulkUpload. bulkUpload:{0}", response.Object.ToString()), ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        /// <summary>
        /// Updates only BulkUpload status
        /// </summary>
        /// <param name="bulkUploadToUpdate"></param>
        /// <param name="newStatus"></param>
        /// <returns></returns>
        public static GenericResponse<BulkUpload> UpdateBulkUploadStatusWithVersionCheck(BulkUpload bulkUploadToUpdate, BulkUploadJobStatus newStatus)
        {
            var response = new GenericResponse<BulkUpload>();
            try
            {
                var originalStatus = bulkUploadToUpdate.Status;
                response.Object = bulkUploadToUpdate;
                response.Object.Status = newStatus;
                BulkUploadJobStatus updatedStatus;
                if (!CatalogDAL.SaveBulkUploadStatusAndErrorsCB(response.Object, BULK_UPLOAD_CB_TTL, out updatedStatus))
                {
                    log.ErrorFormat("UpdateBulkUploadStatusWithVersionCheck > Error while saving BulkUpload to CB. bulkUploadId:{0}, status:{1}.", response.Object.Id, newStatus);
                }
                log.Debug($"UpdateBulkUploadStatusWithVersionCheck > status by results is:[{updatedStatus}], status to set:[{newStatus}]");
                response.Object.Status = updatedStatus;

                UpdateBulkUploadInSqlAndInvalidateKeys(response.Object, originalStatus);


                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in UpdateBulkUploadStatusWithVersionCheck. bulkUploadId:{0}, status:{1}", response.Object.Id, newStatus), ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        private static BulkUpload CreateBulkUploadFromDataTable(DataTable dt, int groupId, bool shouldGetValuesFromCB = false)
        {
            BulkUpload bulkUpload = null;

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                bulkUpload = CreateBulkUploadFromRow(dt.Rows[0], groupId, shouldGetValuesFromCB);
            }

            return bulkUpload;
        }

        private static BulkUpload CreateBulkUploadFromRow(DataRow row, int groupId, bool shouldGetValuesFromCB = false)
        {
            BulkUpload bulkUpload = null;

            if (row != null)
            {
                long id = ODBCWrapper.Utils.GetLongSafeVal(row, "ID");

                if (id > 0)
                {
                    bulkUpload = new BulkUpload()
                    {
                        Id = id,
                        FileURL = ODBCWrapper.Utils.GetSafeStr(row, "FILE_URL"),
                        FileName = ODBCWrapper.Utils.GetSafeStr(row, "FILE_NAME"),
                        Status = (BulkUploadJobStatus)ODBCWrapper.Utils.GetIntSafeVal(row, "STATUS"),
                        Action = (BulkUploadJobAction)ODBCWrapper.Utils.GetIntSafeVal(row, "ACTION"),
                        NumOfObjects = ODBCWrapper.Utils.GetNullableInt(row, "NUM_OF_OBJECTS"),
                        GroupId = groupId,
                        CreateDate = ODBCWrapper.Utils.GetDateSafeVal(row, "CREATE_DATE"),
                        UpdateDate = ODBCWrapper.Utils.GetDateSafeVal(row, "UPDATE_DATE"),
                        UpdaterId = ODBCWrapper.Utils.GetLongSafeVal(row, "UPDATER_ID"),
                        BulkObjectType = ODBCWrapper.Utils.GetSafeStr(row, "BULK_OBJECT_TYPE")
                    };

                    if (shouldGetValuesFromCB || FinishedBulkUploadStatuses.Contains(bulkUpload.Status))
                    {
                        BulkUpload bulkUploadWithResults = CatalogDAL.GetBulkUploadCB(bulkUpload.Id);
                        if (bulkUploadWithResults != null)
                        {
                            bulkUpload.JobData = bulkUploadWithResults.JobData;
                            bulkUpload.ObjectData = bulkUploadWithResults.ObjectData;
                            bulkUpload.Results = bulkUploadWithResults.Results ?? new List<BulkUploadResult>();
                            bulkUpload.AffectedObjects = bulkUploadWithResults.AffectedObjects ?? new List<IAffectedObject>();
                            bulkUpload.AddedObjects = bulkUploadWithResults.AddedObjects ?? new List<IAffectedObject>();
                            bulkUpload.UpdatedObjects = bulkUploadWithResults.UpdatedObjects ?? new List<IAffectedObject>();
                            bulkUpload.DeletedObjects = bulkUploadWithResults.DeletedObjects ?? new List<IAffectedObject>();
                            bulkUpload.Errors = bulkUploadWithResults.Errors;
                        }
                    }
                }
            }

            return bulkUpload;
        }

        private static bool EnqueueBulkUpload(int groupId, BulkUpload bulkUploadToEnqueue, long userId)
        {
            var queue = new GenericCeleryQueue();
            var data = new BulkUploadData(groupId, bulkUploadToEnqueue.Id, userId);
            var result = queue.Enqueue(data, data.GetRoutingKey());
            if (!result)
            {
                log.ErrorFormat("Failed to enqueue BulkUpload. data: {0}", data);
            }
            else
            {
                log.DebugFormat("Success to enqueue BulkUpload. data: {0}", data);
            }

            return result;
        }

        private static Tuple<Dictionary<string, List<BulkUpload>>, bool> GetBulkUploadsFromCache(Dictionary<string, object> funcParams)
        {
            Dictionary<string, List<BulkUpload>> statusToBulkUploadList = null;

            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("bulkObjectType") && funcParams.ContainsKey("statusesIn"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    string bulkObjectType = funcParams["bulkObjectType"] as string;
                    List<int> statusesIn;

                    if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                    {
                        statusesIn = (funcParams[LayeredCache.MISSING_KEYS] as List<string>).Select(x => int.Parse(x)).ToList();
                    }
                    else
                    {
                        statusesIn = funcParams["statusesIn"] as List<int>;
                    }

                    if (groupId.HasValue && !string.IsNullOrEmpty(bulkObjectType) && statusesIn != null && statusesIn.Count > 0)
                    {
                        DataTable dt = CatalogDAL.GetBulkUploadsList(groupId.Value, bulkObjectType, statusesIn);
                        statusToBulkUploadList = CreateBulkUploadsMapFromDataTable(dt, groupId.Value);
                        log.DebugFormat("GetBulkUploadsFromCache success, params: {0}.", string.Join(";", funcParams.Keys));
                    }
                }
            }
            catch (Exception ex)
            {
                statusToBulkUploadList = null;
                log.Error(string.Format("GetBulkUploadsFromCache faild, params: {0}.", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, List<BulkUpload>>, bool>(statusToBulkUploadList, statusToBulkUploadList != null);
        }

        private static Dictionary<string, List<BulkUpload>> CreateBulkUploadsMapFromDataTable(DataTable dt, int groupId)
        {
            var statusToBulkUploadList = new Dictionary<string, List<BulkUpload>>();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var bulkUpload = CreateBulkUploadFromRow(row, groupId);
                    if (bulkUpload != null)
                    {
                        var bulkUploadsKey = LayeredCacheKeys.GetBulkUploadsKey(groupId, bulkUpload.BulkObjectType, (int)bulkUpload.Status);

                        if (statusToBulkUploadList.ContainsKey(bulkUploadsKey))
                        {
                            statusToBulkUploadList[bulkUploadsKey].Add(bulkUpload);
                        }
                        else
                        {
                            statusToBulkUploadList.Add(bulkUploadsKey, new List<BulkUpload>() { bulkUpload });
                        }
                    }
                }
            }

            return statusToBulkUploadList;
        }

        private static void UpdateBulkUploadInSqlAndInvalidateKeys(BulkUpload bulkUpload, BulkUploadJobStatus originalStatus)
        {
            if (originalStatus != bulkUpload.Status)
            {
                CatalogDAL.UpdateBulkUpload(bulkUpload.Id, bulkUpload.Status, bulkUpload.UpdaterId, bulkUpload.FileURL, bulkUpload.BulkObjectType, bulkUpload.NumOfObjects);
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetBulkUploadsInvalidationKey(bulkUpload.GroupId, bulkUpload.BulkObjectType, (int)originalStatus));
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetBulkUploadsInvalidationKey(bulkUpload.GroupId, bulkUpload.BulkObjectType, (int)bulkUpload.Status));

                if (bulkUpload.IsProcessCompleted)
                {
                    log.Debug($"UpdateBulkUploadInSqlAndInvalidateKeys > bulk upload proccess is finnished, sending notification to consumers (ps) calculated bulkUpload.Status:[{bulkUpload.Status}]");
                    var updatedBulkUploadResponse = GetBulkUpload(bulkUpload.GroupId, bulkUpload.Id);
                    if (updatedBulkUploadResponse.IsOkStatusCode())
                    {
                        updatedBulkUploadResponse.Object.Notify(eKalturaEventTime.After, BulkUpload.NOTIFY_EVENT_NAME);
                    }
                    else
                    {
                        log.Error($"UpdateBulkUploadInSqlAndInvalidateKeys > Failed to send notification to consumers, failed to detch updated bulkUpload object.");
                    }

                    // every day of ingest wil gradually unlock now .. so we dont want to unloc the keys if they were started already by another ingest
                    //if (bulkUpload.JobData is BulkUploadIngestJobData ingestJobData)
                    //{
                    //    var locker = new DistributedLock(bulkUpload.GroupId);
                    //    locker.Unlock(ingestJobData.LockKeys);
                    //}
                }
            }
        }
    }
}