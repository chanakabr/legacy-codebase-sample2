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
            try
            {
                log.DebugFormat("starting MediaAssetBulkUpload task. data={0}.", data);
                var request = JsonConvert.DeserializeObject<MediaAssetBulkUploadRequest>(data);
                var bulkUploadResponse = BulkUploadManager.GetBulkUpload(request.GroupID, request.BulkUploadId);
                if (!bulkUploadResponse.HasObject())
                {
                    var errorMassage = string.Format("{0} for BulkUploadId:{1}.", eResponseStatus.BulkUploadDoesNotExist.ToString(), request.BulkUploadId);
                    log.Error(errorMassage);
                    return errorMassage;
                }

                if (request.ResultIndex >= bulkUploadResponse.Object.Results.Count)
                {
                    var errorMassage = string.Format("{0} for BulkUploadId:{1}, ResultIndex:{2}.", eResponseStatus.BulkUploadResultIsMissing.ToString(), request.BulkUploadId, request.ResultIndex);
                    log.Error(errorMassage);
                    return errorMassage;
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
                    log.DebugFormat("before request.ObjectData.Id:{0}", request.ObjectData.Id);
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

                    // TODO SHIR - CHECK IF THIS IS WORKS
                    log.DebugFormat("after request.ObjectData.Id:{0}", request.ObjectData.Id);
                    bulkUploadResponse.Object.Results[request.ResultIndex].ObjectId = request.ObjectData.Id;

                    if (!jobActionResponse.IsOkStatusCode())
                    {
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
                        var errorMassage = string.Format("MediaAssetBulkUpload task did not finish successfully. {0}", jobActionResponse != null ? jobActionResponse.ToString() : string.Empty);
                        log.Error(errorMassage);
                        throw new Exception(errorMassage);
                    }
                    else
                    {
                        return "success";
                    }
                }
                else
                {
                    var bulkUploadResult = bulkUploadResponse.Object.Results[request.ResultIndex];
                    bulkUploadResult.SetError(new Status((int)eResponseStatus.BulkUploadResultIsMissing, "BulkUploadResult Is Missing"));
                    log.ErrorFormat("bulkUploadResult:{0}", bulkUploadResult.ToString());
                    BulkUploadManager.UpdateBulkUpload(request.GroupID, request.UserId, request.BulkUploadId, bulkUploadResponse.Object.Status, bulkUploadResult);
                    return "BulkUploadResult Is Missing";
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in MediaAssetBulkUploadHandler.HandleTask. data={0}.", data), ex);
                throw ex;
            }
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