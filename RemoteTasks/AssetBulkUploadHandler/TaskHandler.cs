using ApiObjects.BulkUpload;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Reflection;
using TVinciShared;

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

                if (request.ObjectData == null)
                {
                    var errorMassage = string.Format("{0} for BulkUploadId:{1}, ResultIndex:{2}.", eResponseStatus.BulkUploadResultIsMissing.ToString(), request.BulkUploadId, request.ResultIndex);
                    log.Error(errorMassage);
                    BulkUploadManager.UpdateBulkUploadResult(request.GroupID, request.BulkUploadId, request.ResultIndex, new Status((int)eResponseStatus.BulkUploadResultIsMissing, "BulkUploadResult Is Missing"));
                    return errorMassage;
                }

                log.DebugFormat("MediaAssetBulkUpload details - bulkUploadId:{0}, ResultIndex:{1}.", request.BulkUploadId, request.ResultIndex);

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
                        var images = BulkAssetManager.CreateImagesMap(request.ObjectData.Images);
                        var assetFiles = BulkAssetManager.CreateAssetFilesMap(request.ObjectData.Files);
                        var mediaAsset = request.ObjectData;
                        jobActionResponse = BulkAssetManager.UpsertMediaAsset(request.GroupID, ref mediaAsset, request.UserId, images, assetFiles, DateUtils.MAIN_FORMAT, false);
                        break;
                    case BulkUploadJobAction.Delete:
                        break;
                }

                Status errorStatus = null;
                if (!jobActionResponse.IsOkStatusCode())
                {
                    errorStatus = jobActionResponse.Status;
                }

                var resultStatus = BulkUploadManager.UpdateBulkUploadResult(request.GroupID, request.BulkUploadId, request.ResultIndex, errorStatus, request.ObjectData.Id, jobActionResponse.Objects);
                if (!resultStatus.IsOkStatusCode())
                {
                    var errorMassage = string.Format("MediaAssetBulkUpload task for BulkUploadId:{0}, ResultIndex:{1} did not finish successfully. resultStatus:{2}",
                                                      request.BulkUploadId, request.ResultIndex, resultStatus.ToString());
                    log.Error(errorMassage);
                    throw new Exception(errorMassage);
                }
                else
                {
                    return "success";
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in MediaAssetBulkUploadHandler.HandleTask. data={0}.", data), ex);
                throw ex;
            }
        }
    }
}