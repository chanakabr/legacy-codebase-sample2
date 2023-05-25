using Phx.Lib.Log;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using TVinciShared;
using ApiObjects;
using System.Net.Http;

namespace ImageUploadHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string RECOVERY_MESSAGE_BUCKET = "scheduled_tasks";

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("Image upload request. data={0}", data);

                RemoteImageUploadRequest request = JsonConvert.DeserializeObject<RemoteImageUploadRequest>(data);

                if (request == null)
                    throw new Exception(string.Format("Failed to desterilized image upload request. data = {0}", data != null ? data : string.Empty));

                // create post request
                ImageServerUploadRequest imageServerReq = new ImageServerUploadRequest()
                {
                    GroupId = request.GroupId,
                    Id = request.ImageId,
                    SourcePath = request.SourcePath,
                    Version = request.Version
                };

                // post image
                result = HttpPost(request.ImageServerUrl, JsonConvert.SerializeObject(imageServerReq), "application/json");

                // check result
                if (string.IsNullOrEmpty(result) || result.ToLower() != "true")
                {
                    // update image status
                    UpdateImageStatus(request, eTableStatus.Failed);

                    log.ErrorFormat("error inserting image. data: {0}", data);

                    throw new Exception(string.Format("error inserting image. data: {0}", data));
                }
                else
                {
                    log.DebugFormat("post image success. {0}", data);

                    result = "success";

                    // update image status
                    UpdateImageStatus(request, eTableStatus.OK);

                    // remove from message from recovery queue
                    string recoveryKey = ImageUploadData.BuildMessageRecoveryKey((ApiObjects.eMediaType)request.MediaType, request.RowId, request.Version);

                    // remove CB document through WS API
                    RemoveRecoveryMessage(request.GroupId, RECOVERY_MESSAGE_BUCKET, recoveryKey);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("error inserting image. data: {0}, Exception:{1}", data, ex);
                throw ex;
            }

            return result;
        }

        private static void UpdateImageStatus(RemoteImageUploadRequest request, eTableStatus status)
        {
            // post success - update ws_api
            bool success = Core.Api.Module.UpdateImageState(request.GroupId, request.RowId, request.Version, (eMediaType)request.MediaType, status);

            if (!success)
                throw new Exception(string.Format("Error while updating image status. RowId: {0}, Version {1}, MediaType: {2}, State: {3}",
                request.RowId,                                          // {0}
                request.Version,                                        // {1}
                ((ApiObjects.eMediaType)request.MediaType).ToString(),  // {2}
                status.ToString()));                                    // {3}
            else
                log.DebugFormat("update image state success. RowId: {0}, Version {1}, MediaType: {2}, State: {3}",
                    request.RowId,                                         // {0}
                    request.Version,                                       // {1}
                    ((ApiObjects.eMediaType)request.MediaType).ToString(), // {2}
                    status.ToString());                                    // {3}
        }

        private static void RemoveRecoveryMessage(int groupId, string bucket, string key)
        {
            // remove CB document through WS API
            bool success = Core.Api.Module.ModifyCB(groupId, bucket, key, eDbActionType.Delete, null, 0);
            if (!success)
                log.ErrorFormat("Error while trying to remove CB document. bucket: {0}, key: {1}", bucket, key);
            else
                log.DebugFormat("Successfully removed CB document. bucket: {0}, key: {1}", bucket, key);
        }

        private string HttpPost(string uri, string parameters, string contentType = null)
        {
            string responseFromServer = null;
            try
            {
                contentType = contentType ?? "application/x-www-form-urlencoded";
                using (var postData = new StringContent(parameters, Encoding.UTF8, contentType))
                using (var httpClient = HttpClientUtil.GetHttpClientFromFactory())
                {
                    using (var response = httpClient.PostAsync(uri, postData).ExecuteAndWait())
                    {
                        response.EnsureSuccessStatusCode();
                        responseFromServer = response.Content.ReadAsStringAsync().ExecuteAndWait();
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    string innerMessage = !string.IsNullOrEmpty(ex.InnerException.Message) ? ex.InnerException.Message : string.Empty;
                    log.ErrorFormat("Error on post request. URL: {0}, Parameter: {1}. InnerMessage, innerException: {2}", uri, parameters, innerMessage, ex.InnerException);
                }
                else
                {
                    log.ErrorFormat("Error on post request. URL: {0}, Parameter: {1}. Error: {2}", uri, parameters, ex);
                }
            }

            return responseFromServer;
        }
    }
}
