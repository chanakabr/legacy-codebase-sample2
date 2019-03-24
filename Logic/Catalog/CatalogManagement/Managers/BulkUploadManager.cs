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
                DataTable dt = CatalogDAL.GetBulkUpload(bulkUploadId);
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
        
        public static GenericListResponse<BulkUpload> GetBulkUploads(int groupId, string bulkObjectType, DateTime createDate, List<BulkUploadJobStatus> statuses = null, long? userId = null)
        {
            var response = new GenericListResponse<BulkUpload>();
            try
            {
                var fileObjectTypeName = FileHandler.Instance.GetFileObjectTypeName(bulkObjectType);
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

        public static GenericResponse<BulkUpload> AddBulkUpload(int groupId, string fileName, long userId, string filePath, Type bulkObjectType, BulkUploadJobAction action, BulkUploadJobData jobData, BulkUploadObjectData objectData)
        {
            GenericResponse<BulkUpload> response = new GenericResponse<BulkUpload>();
            try
            {
                // create and save the new BulkUpload in DB (with no results)
                DataTable dt = CatalogDAL.AddBulkUpload(groupId, userId, action, fileName);
                response.Object = CreateBulkUploadFromDataTable(dt, groupId);
                if (response.Object == null)
                {
                    response.SetStatus(eResponseStatus.BulkUploadDoesNotExist);
                    return response;
                }

                response.Object.JobData = jobData;
                objectData.GroupId = groupId;
                response.Object.ObjectData = objectData;

                BulkUploadJobStatus updatedStatus;
                if (!CatalogDAL.SaveBulkUploadCB(response.Object, BULK_UPLOAD_CB_TTL, out updatedStatus))
                {
                    log.ErrorFormat("Error while saving BulkUpload to CB. groupId: {0}, BulkUpload.Id:{1}", groupId, response.Object.Id);
                    return response;
                }
                response.Object.Status = updatedStatus;

                // save the bulkUpload file to server (cut it from iis) and set fileURL
                FileInfo fileInfo = new FileInfo(filePath);
                GenericResponse<string> saveFileResponse = FileHandler.Instance.SaveFile(response.Object.Id, fileInfo, bulkObjectType);
                if (!saveFileResponse.HasObject())
                {
                    log.ErrorFormat("Error while saving BulkUpload File to file server. groupId: {0}, BulkUpload.Id:{1}", groupId, response.Object.Id);
                    response.SetStatus(saveFileResponse.Status);
                    return response;
                }
                response.Object.FileURL = saveFileResponse.Object;

                var objectTypeName = FileHandler.Instance.GetFileObjectTypeName(bulkObjectType.Name);
                response.Object.BulkObjectType = objectTypeName.Object;
                response = UpdateBulkUpload(response.Object, BulkUploadJobStatus.Uploaded);
                
                // Enqueue to CeleryQueue new BulkUpload (the remote will handle the file and its content).
                if (EnqueueBulkUpload(groupId, response.Object, userId))
                {
                    response = UpdateBulkUpload(response.Object, BulkUploadJobStatus.Queued);
                }
                else
                {
                    if (EnqueueBulkUpload(groupId, response.Object, userId))
                    {
                        response = UpdateBulkUpload(response.Object, BulkUploadJobStatus.Queued);
                    }
                    else
                    {
                        response = UpdateBulkUpload(response.Object, BulkUploadJobStatus.Fatal);
                        response.SetStatus(eResponseStatus.EnqueueFailed, string.Format("Failed to enqueue BulkUpload, BulkUpload.Id:{0}", response.Object.Id));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in AddBulkUpload. groupId:{0}, filePath:{1}, userId:{2}, action:{3}, objectType:{4}.", 
                                        groupId, filePath, userId, action, bulkObjectType), ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }
        
        public static Status ProcessBulkUpload(int groupId, long userId, long bulkUploadId)
        {
            // update status to PROCESSING
            var bulkUploadResponse = GetBulkUpload(groupId, bulkUploadId);
            if (!bulkUploadResponse.HasObject())
            {
                return bulkUploadResponse.Status;
            }

            try
            {
                bulkUploadResponse.Object.UpdaterId = userId;

                bulkUploadResponse = UpdateBulkUpload(bulkUploadResponse.Object, BulkUploadJobStatus.Parsing);

                // parse file to objects list (with validation)
                var objectsListResponse = new GenericListResponse<Tuple<Status, IBulkUploadObject>>();
                if (bulkUploadResponse.Object.JobData != null && bulkUploadResponse.Object.ObjectData != null)
                {
                    objectsListResponse = bulkUploadResponse.Object.JobData.Deserialize(bulkUploadId, bulkUploadResponse.Object.FileURL, bulkUploadResponse.Object.ObjectData);
                    if (!objectsListResponse.IsOkStatusCode())
                    {
                        UpdateBulkUpload(bulkUploadResponse.Object, BulkUploadJobStatus.Failed);
                        return objectsListResponse.Status;
                    }
                }
                else
                {
                    var errorMessage = string.Format("ProcessBulkUpload cannot Deserialize file because JobData or ObjectData are null for groupId:{0}, bulkUploadId:{1}.",
                                                     groupId, bulkUploadId);
                    log.Error(errorMessage);
                    UpdateBulkUpload(bulkUploadResponse.Object, BulkUploadJobStatus.Fatal);
                    bulkUploadResponse.SetStatus(eResponseStatus.Error, errorMessage);
                    return bulkUploadResponse.Status;
                }

                bulkUploadResponse.Object.NumOfObjects = objectsListResponse.Objects.Count;
                if (bulkUploadResponse.Object.NumOfObjects == 0)
                {
                    log.ErrorFormat("ProcessBulkUpload Deserialize file with no objects. groupId:{0}, bulkUploadId:{1}.",
                                   groupId, bulkUploadId);
                    bulkUploadResponse = UpdateBulkUpload(bulkUploadResponse.Object, BulkUploadJobStatus.Success);
                    return bulkUploadResponse.Status;
                }

                bulkUploadResponse = UpdateBulkUpload(bulkUploadResponse.Object, BulkUploadJobStatus.Processing);

                // run over all deserialized bulkUpload objects and create the results
                for (int i = 0; i < objectsListResponse.Objects.Count; i++)
                {
                    // create new result in status IN_PROGRESS
                    var resultStatus = BulkUploadResultStatus.InProgress;
                    Status errorStatus = null;
                    if (!objectsListResponse.Objects[i].Item1.IsOkStatusCode())
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
                    bulkUploadResponse = UpdateBulkUpload(bulkUploadResponse.Object, bulkUploadResponse.Object.Status, bulkUploadResult);
                }

                // run over all results and Enqueue them
                for (int i = 0; i < objectsListResponse.Objects.Count; i++)
                {
                    if (bulkUploadResponse.Object.Results[i].Status == BulkUploadResultStatus.InProgress)
                    {
                        // Enqueue to CeleryQueue current bulkUploadObject (the remote will handle each bulkUploadObject in separate).
                        if (objectsListResponse.Objects[i].Item2.EnqueueBulkUploadResult(bulkUploadResponse.Object, i))
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
                bulkUploadResponse = UpdateBulkUpload(bulkUploadResponse.Object, BulkUploadJobStatus.Processed);
            }
            catch (Exception ex)
            {
                bulkUploadResponse = UpdateBulkUpload(bulkUploadResponse.Object, BulkUploadJobStatus.Fatal);
                log.Error(string.Format("An Exception was occurred in ProcessBulkUpload. groupId:{0}, userId:{1}, bulkUploadId:{2}.",
                                        groupId, userId, bulkUploadId), ex);
                bulkUploadResponse.SetStatus(eResponseStatus.Error);
            }
           
            return bulkUploadResponse.Status;
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
                    bulkUploadResponse.Object.Results[resultIndex].SetError(errorStatus);
                }

                if (warnings != null && warnings.Count > 0)
                {
                    bulkUploadResponse.Object.Results[resultIndex].Warnings = warnings;
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
        
        public static GenericResponse<BulkUpload> UpdateBulkUpload(BulkUpload bulkUploadToUpdate, BulkUploadJobStatus newStatus, BulkUploadResult result = null)
        {
            var response = new GenericResponse<BulkUpload>();
            try
            {
                var originalStatus = bulkUploadToUpdate.Status;
                response.Object = bulkUploadToUpdate;
                response.Object.Status = newStatus;

                // check update result
                if (result != null)
                {
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
                            emptyBulkUploadResult.BulkUploadId = response.Object.Id;
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

                BulkUploadJobStatus updatedStatus;
                if (!CatalogDAL.SaveBulkUploadCB(response.Object, BULK_UPLOAD_CB_TTL, out updatedStatus))
                {
                    log.ErrorFormat("UpdateBulkUpload - Error while saving BulkUpload to CB. bulkUploadId:{0}, status:{1}.", response.Object.Id, newStatus);
                }
                response.Object.Status = updatedStatus;

                UpdateBulkUploadInSqlAndInvalidateKeys(response.Object, originalStatus);
                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in UpdateBulkUpload. bulkUpload:{0}, status:{1}",
                                        response.Object.ToString(), newStatus), ex);
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
                        if (bulkUploadWithResults != null || bulkUploadWithResults.Results != null && bulkUploadWithResults.Results.Count > 0)
                        {
                            bulkUpload.Results = bulkUploadWithResults.Results;
                            bulkUpload.JobData = bulkUploadWithResults.JobData;
                            bulkUpload.ObjectData = bulkUploadWithResults.ObjectData;
                        }
                    }
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
            }
        }
    }
}