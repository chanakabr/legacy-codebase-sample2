using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using KLogMonitor;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Tvinci.Core.DAL;
using TVinciShared;
using System.Linq;
using System.IO;
using ApiObjects.BulkUpload;
using CachingProvider.LayeredCache;

namespace Core.Catalog.CatalogManagement
{
    public class BulkUploadManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const uint BULK_UPLOAD_CB_TTL = 5184000; // 60 DAYS after all results in bulk upload are in status Success
        private const double MIN_RECORD_DAYS_TO_WATCH = 60;

        public static readonly List<int> OnGoingBulkUploadJobStatuses = new List<int>()
        {
            (int)BulkUploadJobStatus.Pending,
            (int)BulkUploadJobStatus.Uploaded,
            (int)BulkUploadJobStatus.Queued,
            (int)BulkUploadJobStatus.Parsing,
            (int)BulkUploadJobStatus.Processing,
            (int)BulkUploadJobStatus.Processed
        };

        public static readonly List<int> FinishBulkUploadJobStatuses = new List<int>()
        {
            (int)BulkUploadJobStatus.Success,
            (int)BulkUploadJobStatus.Partial,
            (int)BulkUploadJobStatus.Failed,
            (int)BulkUploadJobStatus.Fatal
        };

        public static GenericResponse<BulkUpload> GetBulkUpload(int groupId, long bulkUploadId)
        {
            GenericResponse<BulkUpload> response = new GenericResponse<BulkUpload>();
            try
            {
                DataTable dt = CatalogDAL.GetBulkUpload(bulkUploadId);
                response.Object = CreateBulkUploadFromDataTable(dt, groupId);
                if (response.Object == null)
                {
                    response.SetStatus(eResponseStatus.BulkUploadDoesNotExist);
                    return response;
                }
                
                BulkUpload bulkUploadWithResults = CatalogDAL.GetBulkUploadCB(bulkUploadId);
                if (bulkUploadWithResults != null || bulkUploadWithResults.Results != null && bulkUploadWithResults.Results.Count > 0)
                {
                    response.Object.Results = bulkUploadWithResults.Results;
                    response.Object.JobData = bulkUploadWithResults.JobData;
                    response.Object.ObjectData = bulkUploadWithResults.ObjectData;
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
        
        public static GenericListResponse<BulkUpload> GetBulkUploads(int groupId, string fileObjectType, long? userId, DateTime? uploadDate, bool shouldGetOnGoingBulkUploads)
        {
            var response = new GenericListResponse<BulkUpload>();
            try
            {
                var fileObjectTypeName = FileHandler.Instance.GetFileObjectTypeName(fileObjectType);
                if (!fileObjectTypeName.HasObject())
                {
                    response.SetStatus(fileObjectTypeName.Status);
                    return response;
                }

                var bulkUploads = new List<BulkUpload>();
                string bulkUploadsKey = LayeredCacheKeys.GetBulkUploadsKey(groupId, fileObjectTypeName.Object, shouldGetOnGoingBulkUploads);
                string bulkUploadsInvalidationKey = LayeredCacheKeys.GetBulkUploadsInvalidationKey(groupId, fileObjectTypeName.Object, shouldGetOnGoingBulkUploads);

                if (!LayeredCache.Instance.Get(bulkUploadsKey,
                                               ref bulkUploads,
                                               GetBulkUploadsFromCache,
                                               new Dictionary<string, object>()
                                               {
                                                   { "groupId", groupId },
                                                   { "fileObjectType", fileObjectType },
                                                   { "shouldGetOnGoingBulkUploads", shouldGetOnGoingBulkUploads }
                                               },
                                               groupId,
                                               LayeredCacheConfigNames.GET_BULK_UPLOADS_FROM_CACHE,
                                               new List<string>() { bulkUploadsInvalidationKey }))
                {
                    log.ErrorFormat("GetBulkUploads - GetBulkUploadsFromCache - Failed get data from cache. groupId: {0}", groupId);
                    return response;
                }
                
                response.Objects = bulkUploads;

                if (uploadDate.HasValue)
                {
                    response.Objects = response.Objects.Where(x => x.CreateDate >= uploadDate.Value).ToList();
                }

                if (userId.HasValue)
                {
                    response.Objects = response.Objects.Where(x => x.UpdaterId == userId.Value).ToList();
                }

                if (!shouldGetOnGoingBulkUploads)
                {
                    foreach (var bulkUpload in response.Objects)
                    {
                        BulkUpload bulkUploadWithResults = CatalogDAL.GetBulkUploadCB(bulkUpload.Id);
                        if (bulkUploadWithResults != null || bulkUploadWithResults.Results != null && bulkUploadWithResults.Results.Count > 0)
                        {
                            bulkUpload.Results = bulkUploadWithResults.Results;
                            bulkUpload.JobData = bulkUploadWithResults.JobData;
                            bulkUpload.ObjectData = bulkUploadWithResults.ObjectData;
                        }
                    }
                }
                
                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in GetBulkUploads. groupId:{0}, fileObjectType:{1}, userId:{2}, uploadDate:{3}.",
                                        groupId, fileObjectType, userId, uploadDate), ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        public static GenericResponse<BulkUpload> AddBulkUpload(int groupId, string filePath, long userId, Type objectType, BulkUploadJobAction action, BulkUploadJobData jobData, BulkUploadObjectData objectData)
        {
            GenericResponse<BulkUpload> response = new GenericResponse<BulkUpload>();
            try
            {
                // create and save the new BulkUpload in DB (with no results)
                DataTable dt = CatalogDAL.AddBulkUpload(groupId, userId, action);
                response.Object = CreateBulkUploadFromDataTable(dt, groupId);
                if (response.Object == null)
                {
                    response.SetStatus(eResponseStatus.BulkUploadDoesNotExist);
                    return response;
                }

                response.Object.JobData = jobData;
                response.Object.ObjectData = objectData;

                if (!CatalogDAL.SaveBulkUploadCB(response.Object))
                {
                    log.ErrorFormat("Error while saving BulkUpload to CB. groupId: {0}, BulkUpload.Id:{1}", groupId, response.Object.Id);
                    return response;
                }

                // save the bulkUpload file to server (cut it from iis)
                FileInfo fileInfo = new FileInfo(filePath);
                GenericResponse<string> saveFileResponse = FileHandler.Instance.SaveFile(response.Object.Id, fileInfo, objectType);
                if (!saveFileResponse.HasObject())
                {
                    log.ErrorFormat("Error while saving BulkUpload File to file server. groupId: {0}, BulkUpload.Id:{1}", groupId, response.Object.Id);
                    response.SetStatus(saveFileResponse.Status);
                    return response;
                }

                var objectTypeName = FileHandler.Instance.GetFileObjectTypeName(objectType.Name);
                response = UpdateBulkUpload(groupId, userId, response.Object.Id, BulkUploadJobStatus.Uploaded, null, saveFileResponse.Object, objectTypeName.Object);

                if (!response.HasObject())
                {
                    return response;
                }

                // Enqueue to CeleryQueue new BulkUpload (the remote will handle the file and its content).
                if (EnqueueBulkUpload(groupId, response.Object, userId))
                {
                    response = UpdateBulkUpload(groupId, userId, response.Object.Id, BulkUploadJobStatus.Queued);
                }
                else
                {
                    if (EnqueueBulkUpload(groupId, response.Object, userId))
                    {
                        response = UpdateBulkUpload(groupId, userId, response.Object.Id, BulkUploadJobStatus.Queued);
                    }
                    else
                    {
                        response = UpdateBulkUpload(groupId, userId, response.Object.Id, BulkUploadJobStatus.Fatal);
                        response.SetStatus(eResponseStatus.EnqueueFailed, string.Format("Failed to enqueue BulkUpload, BulkUpload.Id:{0}", response.Object.Id));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in AddBulkUpload. groupId:{0}, filePath:{1}, userId:{2}, action:{3}, objectType:{4}.", 
                                        groupId, filePath, userId, action, objectType), ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }
        
        public static Status ProcessBulkUpload(int groupId, long userId, long bulkUploadId)
        {
            // update status to PROCESSING
            var bulkUploadResponse = UpdateBulkUpload(groupId, userId, bulkUploadId, BulkUploadJobStatus.Parsing);
            if (!bulkUploadResponse.HasObject())
            {
                return bulkUploadResponse.Status;
            }

            // parse file to objects list (with validation)
            var objectsListResponse = new GenericListResponse<Tuple<Status, IBulkUploadObject>>();
            if (bulkUploadResponse.Object.JobData != null && bulkUploadResponse.Object.ObjectData != null)
            {
                objectsListResponse = bulkUploadResponse.Object.JobData.Deserialize(groupId, bulkUploadResponse.Object.FileURL, bulkUploadResponse.Object.ObjectData);
                if (!objectsListResponse.IsOkStatusCode())
                {
                    UpdateBulkUpload(groupId, userId, bulkUploadId, BulkUploadJobStatus.Failed);
                    return objectsListResponse.Status;
                }
            }
            else
            {
                log.ErrorFormat("ProcessBulkUpload cannot Deserialize file because JobData or ObjectData are null for groupId:{0}, bulkUploadId:{1}.",
                                groupId, bulkUploadId);
                bulkUploadResponse = UpdateBulkUpload(groupId, userId, bulkUploadId, BulkUploadJobStatus.Fatal);
                return bulkUploadResponse.Status;
            }
            
            bulkUploadResponse.Object.NumOfObjects = objectsListResponse.Objects.Count;
            if (bulkUploadResponse.Object.NumOfObjects == 0)
            {
                log.ErrorFormat("ProcessBulkUpload Deserialize file with no objects. groupId:{0}, bulkUploadId:{1}.",
                               groupId, bulkUploadId);
                bulkUploadResponse = UpdateBulkUpload(groupId, userId, bulkUploadId, BulkUploadJobStatus.Success);
                return bulkUploadResponse.Status;
            }
            else
            {
                bulkUploadResponse = UpdateBulkUpload(groupId, userId, bulkUploadId, BulkUploadJobStatus.Processing, null, null, null, bulkUploadResponse.Object.NumOfObjects);
            }

            // run over all deserialized bulkUpload objects
            for (int i = 0; i < objectsListResponse.Objects.Count; i++)
            {
                // create new result in status IN_PROGRESS
                var resultStatus = BulkUploadResultStatus.InProgress;
                Status errorStatus = null;
                if (objectsListResponse.Objects[i].Item1.Code != (int)eResponseStatus.OK)
                {
                    resultStatus = BulkUploadResultStatus.Error;
                    errorStatus = objectsListResponse.Objects[i].Item1;
                }

                var bulkUploadResult = objectsListResponse.Objects[i].Item2.GetNewBulkUploadResult(bulkUploadResponse.Object.Id, resultStatus, i, errorStatus);
                if (bulkUploadResult == null)
                {
                    log.ErrorFormat("bulkUploadResult is null for bulkUploadId:{0}, index:{1}", bulkUploadResponse.Object.Id, i);
                    continue;
                }

                // add current result to bulkUpload results list and update it in DB.
                bulkUploadResponse = UpdateBulkUpload(groupId, userId, bulkUploadId, bulkUploadResponse.Object.Status, bulkUploadResult);
                if (!bulkUploadResponse.HasObject())
                {
                    log.ErrorFormat("First try to add bulkUploadResult to Results list failed, bulkUploadResult:{0}", bulkUploadResult.ToString());
                    bulkUploadResponse = UpdateBulkUpload(groupId, userId, bulkUploadId, bulkUploadResponse.Object.Status, bulkUploadResult);
                    if (!bulkUploadResponse.HasObject())
                    {
                        log.ErrorFormat("Seconed try to add bulkUploadResult to Results list failed, bulkUploadResult:{0}", bulkUploadResult.ToString());
                        continue;
                    }
                }

                if (resultStatus == BulkUploadResultStatus.InProgress)
                {
                    // Enqueue to CeleryQueue current bulkUploadObject (the remote will handle each bulkUploadObject in separate).
                    if (objectsListResponse.Objects[i].Item2.Enqueue(groupId, userId, bulkUploadId, bulkUploadResponse.Object.Action, i))
                    {
                        log.DebugFormat("Success enqueue bulkUploadObject. bulkUploadId:{0}, resultIndex:{1}", bulkUploadId, i);
                    }
                    else
                    {
                        log.DebugFormat("Failed enqueue bulkUploadObject. bulkUploadId:{0}, resultIndex:{1}", bulkUploadId, i);
                    }
                }
            }

            // update status to PROCESSED
            bulkUploadResponse = UpdateBulkUpload(groupId, userId, bulkUploadId, BulkUploadJobStatus.Processed);
            return bulkUploadResponse.Status;
        }

        public static GenericResponse<BulkUpload> UpdateBulkUpload(int groupId, long userId, long bulkUploadId, BulkUploadJobStatus status, BulkUploadResult result = null, string fileURL = null, string fileObjectType = null, int? numOfObjects = null)
        {
            GenericResponse<BulkUpload> response = new GenericResponse<BulkUpload>();
            try
            {
                response = GetBulkUpload(groupId, bulkUploadId);
                if (!response.HasObject())
                {
                    return response;
                }

                bool shouldSetInvalidationKey = false;
                if (response.Object.Status != status)
                {
                    shouldSetInvalidationKey = true;
                }

                response.Object.Status = status;
                response.Object.UpdaterId = userId;
                
                // check if need to update result
                if (result != null)
                {
                    if (response.Object.Results == null)
                    {
                        response.Object.Results = new List<BulkUploadResult>();
                    }

                    // set result 
                    if (result.Index == response.Object.Results.Count)
                    {
                        response.Object.Results.Add(result);
                    }
                    else if (result.Index >= 0 && result.Index < response.Object.Results.Count)
                    {
                        response.Object.Results[result.Index] = result;
                    }
                    // check if other results are missing and add them with error 
                    else if (result.Index > response.Object.Results.Count)
                    {
                        var emptyResult = Activator.CreateInstance(result.GetType());
                        if (emptyResult != null && emptyResult is BulkUploadResult)
                        {
                            var resultError = new Status((int)eResponseStatus.BulkUploadResultIsMissing, eResponseStatus.BulkUploadResultIsMissing.ToString());
                            var emptyBulkUploadResult = emptyResult as BulkUploadResult;
                            emptyBulkUploadResult.Index = response.Object.Results.Count;
                            emptyBulkUploadResult.BulkUploadId = bulkUploadId;
                            emptyBulkUploadResult.SetError(resultError);
                            response.Object.Results.Add(emptyBulkUploadResult);

                            while (result.Index > response.Object.Results.Count)
                            {
                                emptyBulkUploadResult.Index = response.Object.Results.Count;
                                response.Object.Results.Add(emptyBulkUploadResult);
                            }
                            response.Object.Results.Add(result);
                        }
                    }
                }
                
                if (response.Object.NumOfObjects != 0 &&
                    response.Object.NumOfObjects == response.Object.Results.Count)
                {
                    if (response.Object.Results.All(x => x.Status == BulkUploadResultStatus.Ok))
                    {
                        response.Object.Status = BulkUploadJobStatus.Success;
                    }
                    else if (response.Object.Results.All(x => x.Status == BulkUploadResultStatus.Error))
                    {
                        response.Object.Status = BulkUploadJobStatus.Failed;
                    }
                    else if (response.Object.Results.All(x => x.Status == BulkUploadResultStatus.Ok || x.Status == BulkUploadResultStatus.Error))
                    {
                        response.Object.Status = BulkUploadJobStatus.Partial;
                    }

                    if (response.Object.Status != status)
                    {
                        shouldSetInvalidationKey = true;
                    }
                }

                uint ttl = 0;
                if (response.Object.Status == BulkUploadJobStatus.Success ||
                    response.Object.Status == BulkUploadJobStatus.Partial ||
                    response.Object.Status == BulkUploadJobStatus.Failed ||
                    response.Object.Status == BulkUploadJobStatus.Fatal)
                {
                    ttl = BULK_UPLOAD_CB_TTL;
                }

                var dt = CatalogDAL.UpdateBulkUpload(bulkUploadId, response.Object.Status, userId, fileURL, fileObjectType, numOfObjects);
                var updatedBulkUpload = CreateBulkUploadFromDataTable(dt, groupId);
                if (updatedBulkUpload == null)
                {
                    response.SetStatus(eResponseStatus.BulkUploadDoesNotExist);
                    return response;
                }

                response.Object.UpdaterId = updatedBulkUpload.UpdaterId;
                response.Object.FileURL = updatedBulkUpload.FileURL;
                response.Object.NumOfObjects = updatedBulkUpload.NumOfObjects;
                response.Object.UpdateDate = updatedBulkUpload.UpdateDate;
                response.Object.FileObjectType = updatedBulkUpload.FileObjectType;

                if (!CatalogDAL.SaveBulkUploadCB(response.Object, ttl))
                {
                    log.ErrorFormat("Error while saving BulkUpload to CB. groupId:{0}, bulkUploadId:{1}.", groupId, bulkUploadId);
                    return response;
                }

                if (shouldSetInvalidationKey)
                {
                    if (ttl == 0)
                    {
                        LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetBulkUploadsInvalidationKey(groupId, response.Object.FileObjectType, true));
                    }
                    else
                    {
                        LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetBulkUploadsInvalidationKey(groupId, response.Object.FileObjectType, false));
                    }
                }

                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in UpdateBulkUpload. groupId:{0}, bulkUploadId:{1}, userId:{2}, status:{3}",
                                        groupId, bulkUploadId, userId, status), ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        private static List<BulkUpload> CreateBulkUploadsFromDataTable(DataTable dt, int groupId)
        {
            List<BulkUpload> bulkUploads = null;

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                bulkUploads = new List<BulkUpload>();
                foreach (DataRow row in dt.Rows)
                {
                    var bulkUpload = CreateBulkUploadFromRow(row, groupId);
                    if (bulkUpload != null)
                    {
                        bulkUploads.Add(bulkUpload);
                    }
                }
                
            }

            return bulkUploads;
        }

        private static BulkUpload CreateBulkUploadFromDataTable(DataTable dt, int groupId)
        {
            BulkUpload bulkUpload = null;

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                bulkUpload = CreateBulkUploadFromRow(dt.Rows[0], groupId);
            }

            return bulkUpload;
        }

        private static BulkUpload CreateBulkUploadFromRow(DataRow row, int groupId)
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
                        Status = (BulkUploadJobStatus)ODBCWrapper.Utils.GetIntSafeVal(row, "STATUS"),
                        Action = (BulkUploadJobAction)ODBCWrapper.Utils.GetIntSafeVal(row, "ACTION"),
                        NumOfObjects = ODBCWrapper.Utils.GetNullableInt(row, "NUM_OF_OBJECTS"),
                        GroupId = groupId,
                        CreateDate = ODBCWrapper.Utils.GetDateSafeVal(row, "CREATE_DATE"),
                        UpdateDate = ODBCWrapper.Utils.GetDateSafeVal(row, "UPDATE_DATE"),
                        UpdaterId = ODBCWrapper.Utils.GetLongSafeVal(row, "UPDATER_ID"),
                        FileObjectType = ODBCWrapper.Utils.GetSafeStr(row, "FILE_OBJECT_TYPE")
                    };
                }
            }

            return bulkUpload;
        }

        private static bool EnqueueBulkUpload(int groupId, BulkUpload bulkUploadToEnqueue, long userId)
        {
            GenericCeleryQueue queue = new GenericCeleryQueue();
            BulkUploadData data = new BulkUploadData(groupId, bulkUploadToEnqueue.Id, userId);
            bool enqueueSuccessful = queue.Enqueue(data, data.GetRoutingKey());
            if (!enqueueSuccessful)
            {
                log.ErrorFormat("Failed to enqueue BulkUpload. data: {0}", data);
            }
            else
            {
                log.DebugFormat("Success to enqueue BulkUpload. data: {0}", data);
            }

            return enqueueSuccessful;
        }

        private static Tuple<List<BulkUpload>, bool> GetBulkUploadsFromCache(Dictionary<string, object> funcParams)
        {
            List<BulkUpload> bulkUploadList = new List<BulkUpload>();

            try
            {
                if (funcParams != null && funcParams.Count == 3)
                {
                    if (funcParams.ContainsKey("groupId") && funcParams.ContainsKey("fileObjectType") && funcParams.ContainsKey("shouldGetOnGoingBulkUploads"))
                    {
                        int? groupId = funcParams["groupId"] as int?;
                        string fileObjectType = funcParams["fileObjectType"] as string;
                        bool? shouldGetOnGoingBulkUploads = funcParams["shouldGetOnGoingBulkUploads"] as bool?;
                        if (groupId.HasValue && !string.IsNullOrEmpty(fileObjectType) && shouldGetOnGoingBulkUploads.HasValue)
                        {
                            var statusesIn = new List<int>();
                            if (shouldGetOnGoingBulkUploads.Value)
                            {
                                statusesIn = OnGoingBulkUploadJobStatuses;
                            }
                            else
                            {
                                statusesIn = FinishBulkUploadJobStatuses;
                            }
                            
                            DataTable dt = CatalogDAL.GetBulkUploadsList(groupId.Value, fileObjectType, statusesIn);
                            bulkUploadList = CreateBulkUploadsFromDataTable(dt, groupId.Value);
                            log.DebugFormat("GetBulkUploadsFromCache success, params: {0}.", string.Join(";", funcParams.Keys));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bulkUploadList = null;
                log.Error(string.Format("GetBulkUploadsFromCache faild, params: {0}.", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<BulkUpload>, bool>(bulkUploadList, bulkUploadList != null);
        }
    }
}