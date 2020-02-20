using Core.Catalog.CatalogManagement;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Reflection;

namespace BulkUploadHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.Debug($"starting BulkUpload task. data={data}.");
                var request = JsonConvert.DeserializeObject<BulkUploadRequest>(data);
                var processBulkUploadStatus = BulkUploadManager.ProcessBulkUpload(request.GroupID, request.UserId, request.BulkUploadId);

                if (!processBulkUploadStatus.IsOkStatusCode())
                {
                    throw new Exception($"BulkUpload task did not finish successfully. ProcessBulkUpload error: {processBulkUploadStatus.ToString()}.");
                }
                else
                {
                    result = "success";
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in BulkUploadHandler.HandleTask. data={data}, exception: {ex.ToString()}.");
                throw ex;
            }

            return result;
        }
    }
}