using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using KLogMonitor;
using Newtonsoft.Json;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Tvinci.Core.DAL;
using TVinciShared;
using System.Linq;
using ApiObjects.Excel;
using System.IO;

namespace Core.Catalog.CatalogManagement
{
    public class BulkUploadManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        // TODO SHIR - ASK IDDO AND THEN PUT IN DR
        private const uint BULK_UPLOAD_TTL = 5184000; // 60 DAYS after all results in bulk upload are in status=?
        
        public static GenericResponse<BulkUpload> GetBulkUpload(int groupId, long bulkUploadId)
        {
            GenericResponse<BulkUpload> response = new GenericResponse<BulkUpload>();
            try
            {
                DataTable dt = CatalogDAL.GetBulkUpload(groupId, bulkUploadId);
                BulkUpload bulkUpload = CreateBulkUploadFromDataTable(dt);
                if (bulkUpload == null)
                {
                    response.SetStatus(eResponseStatus.BulkUploadDoesNotExist);
                    return response;
                }
                
                BulkUpload bulkUploadWithResults = CatalogDAL.GetBulkUploadCB(bulkUploadId);
                if (bulkUploadWithResults != null || bulkUploadWithResults.Results != null && bulkUploadWithResults.Results.Count > 0)
                {
                    bulkUpload.Results = bulkUploadWithResults.Results;
                }

                response.Object = bulkUpload;
                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in GetBulkUpload. groupId:{0}, bulkUploadId:{1}.", groupId, bulkUploadId), ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        public static GenericResponse<BulkUpload> AddBulkUpload(int groupId, string filePath, long userId, Type objectType, BulkUploadJobAction action, FileType fileType)
        {
            GenericResponse<BulkUpload> response = new GenericResponse<BulkUpload>();
            try
            {
                // create new BulkUpload (with no results)
                response.Object = new BulkUpload()
                {
                    Status = BulkUploadJobStatus.PENDING,
                    Action = action,
                    FileType = fileType,
                    GroupId = groupId
                };

                // save the new BulkUpload in DB
                DataTable dt = CatalogDAL.AddBulkUpload(response.Object, userId);
                response.Object = CreateBulkUploadFromDataTable(dt);
                if (response.Object == null)
                {
                    response.SetStatus(eResponseStatus.BulkUploadDoesNotExist);
                    return response;
                }

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

                response = UpdateBulkUpload(groupId, userId, response.Object.Id, BulkUploadJobStatus.UPLOADED, null, saveFileResponse.Object);

                if (!response.HasObject())
                {
                    return response;
                }

                // Enqueue to CeleryQueue new BulkUpload (the remote will handle the file and its content).
                if (EnqueueBulkUpload(groupId, response.Object, userId))
                {
                    response = UpdateBulkUpload(groupId, userId, response.Object.Id, BulkUploadJobStatus.QUEUED);
                }
                else
                {
                    // TODO SHIR - ASK IDDO WHEN STATUS IS FATAL?
                    response.SetStatus(eResponseStatus.EnqueueFailed, string.Format("Failed to enqueue BulkUpload, BulkUpload.Id:{0}", response.Object.Id));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in AddBulkUpload. groupId:{0}, filePath:{1}, userId:{2}, action:{3}, fileType:{4}, objectType:{5}.", 
                                        groupId, filePath, userId, action, fileType, objectType), ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }
        
        public static Status ProcessBulkUpload(int groupId, long userId, long bulkUploadId)
        {
            // TODO SHIR - ASK IDDO FOR STATUS
            // update status to PROCESSING
            var bulkUploadResponse = new GenericResponse<BulkUpload>();// UpdateBulkUpload(groupId, userId, bulkUploadId, BulkUploadJobStatus.PROCESSING);
            if (!bulkUploadResponse.HasObject())
            {
                return bulkUploadResponse.Status;
            }

            // parse file to objects list (with validation)
            GenericListResponse<IBulkUploadObject> objectsListResponse = new GenericListResponse<IBulkUploadObject>();
            switch (bulkUploadResponse.Object.FileType)
            {
                case FileType.Excel:
                    // TODO SHIR - get file stream from file_handler by id and file extention
                    //var deserializeListResponse = ExcelManager.Deserialize(uploadTokenId);
                    //if (!deserializeListResponse.HasObjects())
                    //{
                    //    objectsListResponse.SetStatus(deserializeListResponse.Status);
                    //}
                    //else
                    //{
                    //    objectsListResponse.Objects.AddRange(deserializeListResponse.Objects);
                    //}
                    break;
                case FileType.Xml:
                    // TODO ARTHUR
                    break;
                default:
                    break;
                }

            if (!objectsListResponse.Status.IsOkStatusCode())
            {
                return objectsListResponse.Status;
            }

            bulkUploadResponse.Object.NumOfObjects = objectsListResponse.Objects.Count;
            if (bulkUploadResponse.Object.NumOfObjects == 0)
            {
                log.ErrorFormat("ProcessBulkUpload Deserialize file with no objects. groupId:{0}, bulkUploadId:{1}.",
                               groupId, bulkUploadId);
                // TODO SHIR - ASK IDDO FOR STATUS
                //bulkUploadResponse = UpdateBulkUpload(groupId, userId, bulkUploadId, BulkUploadJobStatus.FINISHED_WITH_NO_OBJECTS);
                return bulkUploadResponse.Status;
            }
            else
            {
                // TODO SHIR - ASK IDDO FOR STATUS
                //bulkUploadResponse = UpdateBulkUpload(groupId, userId, bulkUploadId, BulkUploadJobStatus.FinishToPROCESSING, null, null, bulkUploadResponse.Object.NumOfObjects);
            }

            // run over all deserialized bulkUpload objects
            foreach (var bulkUploadObject in objectsListResponse.Objects)
            {
                // create new result in status PENDING
                // TODO SHIR - ASK IDDO FOR STATUS
                var bulkUploadResult = bulkUploadObject.GetNewBulkUploadResult(bulkUploadResponse.Object.Id, BulkUploadResultStatus.IN_PROGRESS);

                // add current result to bulkUpload results list and update it in DB.
                // TODO SHIR - ASK IDDO FOR STATUS
                //bulkUploadResponse = UpdateBulkUpload(groupId, userId, bulkUploadId, BulkUploadJobStatus.PROCESSING, bulkUploadResult);
                if (!bulkUploadResponse.HasObject())
                {
                    // TODO SHIR - ASK IDDO FOR STATUS
                    log.ErrorFormat("First try to add bulkUploadResult to Results list failed, bulkUploadResult:{0}", bulkUploadResult.ToString());
                    //bulkUploadResponse = UpdateBulkUpload(groupId, userId, bulkUploadId, BulkUploadJobStatus.PROCESSING, bulkUploadResult);
                    if (!bulkUploadResponse.HasObject())
                    {
                        log.ErrorFormat("Seconed try to add bulkUploadResult to Results list failed, bulkUploadResult:{0}", bulkUploadResult.ToString());
                        continue;
                    }
                }
                // Enqueue to CeleryQueue current bulkUploadObject (the remote will handle each bulkUploadObject in separate).
                int resultIndex = bulkUploadResponse.Object.Results.Count - 1;
                if (bulkUploadObject.Enqueue(groupId, userId, bulkUploadId, bulkUploadResponse.Object.Action, resultIndex))
                {
                    log.ErrorFormat("Success enqueue bulkUploadObject. bulkUploadId:{0}, resultIndex:{1}", bulkUploadId, resultIndex);
                    // update curr result status to QUEUED and save in in DB
                    bulkUploadResult.Index = resultIndex;
                    // TODO SHIR - ASK IDDO WHAT TODO for status
                    //bulkUploadResult.Status = BulkUploadResultStatus.QUEUED;
                    //bulkUploadResponse = UpdateBulkUpload(groupId, userId, bulkUploadId, BulkUploadJobStatus.PROCESSING, bulkUploadResult);
                    //if (!bulkUploadResponse.HasObject())
                    //{
                    //    // TODO SHIR - ASK IDDO WHAT TODO WHEN update current result faild
                    //}
                }
                else
                {
                    log.ErrorFormat("Failed enqueue bulkUploadObject. bulkUploadId:{0}, resultIndex:{1}", bulkUploadId, resultIndex);
                    // TODO SHIR - ASK IDDO WHAT TODO WHEN enqueue FAILED
                }
            }

            // TODO SHIR - ASK IDDO FOR STATUS
            // update status to PROCESSED
            //bulkUploadResponse = UpdateBulkUpload(groupId, userId, bulkUploadId, BulkUploadJobStatus.PROCESSED);
            if (!bulkUploadResponse.HasObject())
            {
                // TODO SHIR - ASK IDDO WHAT TODO WHEN update to PROCESSED failed
                //return bulkUploadResponse.Status;
            }

            return bulkUploadResponse.Status;
        }

        private static BulkUpload CreateBulkUploadFromDataTable(DataTable dt)
        {
            BulkUpload bulkUpload = null;

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                long id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID");
                
                if (id > 0)
                {
                    bulkUpload = new BulkUpload()
                    {
                        Id = id,
                        FileName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "FILE_NAME"),
                        Status = (BulkUploadJobStatus)ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "STATUS"),
                        Action = (BulkUploadJobAction)ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "ACTION"),
                        FileType = (FileType)ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "FILE_TYPE"),
                        NumOfObjects = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "NUM_OF_OBJECTS"),
                        GroupId = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "GROUP_ID"),
                        CreateDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0], "CREATE_DATE"),
                        UpdateDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0], "UPDATE_DATE")
                    };
                }
            }

            return bulkUpload;
        }

        private static GenericResponse<BulkUpload> UpdateBulkUpload(int groupId, long userId, long bulkUploadId, BulkUploadJobStatus status, BulkUploadResult result = null, string fileName = null, int? numOfObjects = null)
        {
            GenericResponse<BulkUpload> response = new GenericResponse<BulkUpload>();
            try
            {
                var dt = CatalogDAL.UpdateBulkUploadStatus(groupId, fileName, bulkUploadId, status, userId, numOfObjects);
                response.Object = CreateBulkUploadFromDataTable(dt);
                if (response.Object == null)
                {
                    response.SetStatus(eResponseStatus.BulkUploadDoesNotExist);
                    return response;
                }

                var bulkUploadCB = CatalogDAL.GetBulkUploadCB(bulkUploadId);
                if (bulkUploadCB != null)
                {
                    response.Object.Results = bulkUploadCB.Results;

                    // check if need to update result
                    if (result != null)
                    {
                        if (response.Object.Results == null)
                        {
                            response.Object.Results = new List<BulkUploadResult>();
                        }

                        // check if result exists
                        if (result.Index < 0)
                        {
                            result.Index = response.Object.Results.Count;
                            response.Object.Results.Add(result);
                        }
                        else if (result.Index < response.Object.Results.Count)
                        {
                            response.Object.Results[result.Index] = result;
                        }
                    }

                    // TODO SHIR - ASK IRA WHEN TO UPDATE STATUS TO FINISHED
                    uint ttl = 0;
                    if (response.Object.NumOfObjects != 0 &&
                        response.Object.NumOfObjects == response.Object.Results.Count &&
                        response.Object.Results.All(x => x.Status == BulkUploadResultStatus.OK))
                    {
                        ttl = BULK_UPLOAD_TTL;
                    }

                    if (!CatalogDAL.SaveBulkUploadCB(response.Object, ttl))
                    {
                        log.ErrorFormat("Error while saving BulkUpload to CB. groupId:{0}, bulkUploadId:{1}.", groupId, bulkUploadId);
                        return response;
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
    }
}