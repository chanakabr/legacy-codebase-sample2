using ApiObjects.BulkUpload;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using Phx.Lib.Log;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Reflection;
using TVinciShared;

namespace LiveAssetBulkUploadHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            try
            {
                log.Debug($"starting LiveAssetBulkUpload task. data={data}.");
                var request = JsonConvert.DeserializeObject<LiveAssetBulkUploadRequest>(data);

                if (request.ObjectData == null)
                {
                    var errorMassage = $"{eResponseStatus.BulkUploadResultIsMissing.ToString()} for BulkUploadId:{request.BulkUploadId}, ResultIndex:{request.ResultIndex}.";
                    log.Error(errorMassage);
                    BulkUploadManager.UpdateBulkUploadResult(request.GroupID, request.BulkUploadId, request.ResultIndex, new Status((int)eResponseStatus.BulkUploadResultIsMissing, "BulkUploadResult Is Missing"));
                    return errorMassage;
                }

                log.Debug($"LiveAssetBulkUpload details - bulkUploadId:{request.BulkUploadId}, ResultIndex:{request.ResultIndex}.");

                if (request.ObjectData.CoGuid.Equals("*"))
                {
                    request.ObjectData.CoGuid = "kaltura_" + Guid.NewGuid().ToString();
                }
                else
                {
                    request.ObjectData.Id = BulkAssetManager.GetMediaIdByCoGuid(request.GroupID, request.ObjectData.CoGuid);
                }

                var jobActionResponse = new GenericListResponse<Status>();
                switch (request.JobAction)
                {
                    case BulkUploadJobAction.Upsert:
                        var images = BulkAssetManager.CreateImagesMap(request.ObjectData.Images);
                        var assetFiles = BulkAssetManager.CreateAssetFilesMap(request.ObjectData.Files);
                        var liveAsset = request.ObjectData;
                        jobActionResponse = BulkAssetManager.UpsertMediaAsset(request.GroupID, ref liveAsset, request.UserId, images, assetFiles, DateUtils.MAIN_FORMAT, false);
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
                    var errorMassage = $"LiveAssetBulkUpload task for BulkUploadId:{request.BulkUploadId}, ResultIndex:{request.ResultIndex} did not finish successfully. resultStatus:{resultStatus.ToString()}";
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
                log.Error($"An Exception was occurred in LiveAssetBulkUploadHandler.HandleTask. data={data}.", ex);
                throw ex;
            }
        }
    }
}