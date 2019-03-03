using ApiObjects.BulkUpload;
using ApiObjects.Response;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MediaAssetBulkUploadHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("starting AssetBulkUpload task. data={0}.", data);
                var request = JsonConvert.DeserializeObject<MediaAssetBulkUploadRequest>(data);
                var bulkUploadResponse = BulkUploadManager.GetBulkUpload(request.GroupID, request.BulkUploadId);
                if (!bulkUploadResponse.HasObject())
                {
                    // TODO SHIR - RETURN ERROR eResponseStatus.BulkUploadDoesNotExist
                }

                if (request.ResultIndex >= bulkUploadResponse.Object.Results.Count)
                {
                    // TODO SHIR - RETURN ERROR eResponseStatus.BulkUploadResultIsMissing
                }

                if (request.ObjectData != null)
                {
                    if (request.ObjectData.CoGuid.Equals("*"))
                    {
                        request.ObjectData.CoGuid = "kaltura_" + Guid.NewGuid().ToString();
                    }
                    else
                    {
                        request.ObjectData.Id = BulkAssetManager.GetMediaIdByCoGuid(request.GroupID, request.ObjectData.CoGuid);
                    }
                    
                    GenericListResponse<Status> jobActionResponse = new GenericListResponse<Status>();
                    switch (request.JobAction)
                    {
                        case BulkUploadJobAction.Upsert:
                            var images = GetImages(request.ObjectData.Images);
                            var assetFiles = GetAssetFiles(request.ObjectData.Files);
                            jobActionResponse = BulkAssetManager.UpsertMediaAsset(request.GroupID, request.ObjectData, request.UserId, images, assetFiles, ExcelManager.DATE_FORMAT, false);
                            break;
                        case BulkUploadJobAction.Delete:
                            break;
                    }

                    if (!jobActionResponse.IsOkStatusCode())
                    {
                        bulkUploadResponse.Object.Results[request.ResultIndex].Status = BulkUploadResultStatus.Error;
                        bulkUploadResponse.Object.Results[request.ResultIndex].SetError(jobActionResponse.Status);
                    }
                    else
                    {
                        bulkUploadResponse.Object.Results[request.ResultIndex].Status = BulkUploadResultStatus.Ok;
                        // TODO SHIR - ASK IDDO WHAT TO DO WHIT ERRORS ON IMAGES AND FILES
                    }

                    bulkUploadResponse = BulkUploadManager.UpdateBulkUpload(request.GroupID, request.UserId, request.BulkUploadId, bulkUploadResponse.Object.Status, bulkUploadResponse.Object.Results[request.ResultIndex]);
                    if (!bulkUploadResponse.HasObject())
                    { 

                        throw new Exception(string.Format("MediaAssetBulkUpload task did not finish successfully. {0}", jobActionResponse != null ? jobActionResponse.ToString() : string.Empty));
                    }
                    else
                    {
                        result = "success";
                    }
                }
                else
                {
                    // TODO SHIR - ASK IDDO WHAT TO DO IF NO OBJECT 
                    bulkUploadResponse = BulkUploadManager.UpdateBulkUpload(request.GroupID, request.UserId, request.BulkUploadId, BulkUploadJobStatus.Fatal);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in MediaAssetBulkUploadHandler.HandleTask. data={0}.", data), ex);
                throw ex;
            }

            return result;
        }

        private Dictionary<int, Tuple<AssetFile, string>> GetAssetFiles(List<AssetFile> files)
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

        private Dictionary<long, Image> GetImages(List<Image> images)
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