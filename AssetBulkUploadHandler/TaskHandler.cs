using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
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
                
                if (request.ObjectData != null)
                {
                    Status jobActionStatus = null;
                    switch (request.JobAction)
                    {
                        case BulkUploadJobAction.Upsert:
                            jobActionStatus = IngestManager.UpsertMediaAsset(request.ObjectData, request.GroupID);
                            break;
                        case BulkUploadJobAction.Delete:
                            break;
                        default:
                            break;
                    }

                    if (jobActionStatus == null || !jobActionStatus.IsOkStatusCode())
                    {
                        throw new Exception(string.Format("MediaAssetBulkUpload task did not finish successfully. {0}", jobActionStatus != null ? jobActionStatus.ToString() : string.Empty));
                    }
                    else
                    {
                        result = "success";
                    }
                }
                else
                {
                    // TODO SHIR - ASK IDDO WHAT TO DO IF NO OBJECT 
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in MediaAssetBulkUploadHandler.HandleTask. data={0}.", data), ex);
                throw ex;
            }

            return result;
        }
    }
}