using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using TVinciShared;
using ImageUploadHandler.WS_API;
using ApiObjects;
using System.Web;

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
                    UpdateImageStatus(request, WS_API.eTableStatus.Failed);

                    log.ErrorFormat("error inserting image. data: {0}", data);

                    throw new Exception(string.Format("error inserting image. data: {0}", data));
                }
                else
                {
                    log.DebugFormat("post image success. {0}", data);

                    // update image status
                    UpdateImageStatus(request, WS_API.eTableStatus.OK);

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

        private static void UpdateImageStatus(RemoteImageUploadRequest request, WS_API.eTableStatus status)
        {
            // post success - update ws_api
            string url = WS_Utils.GetTcmConfigValue("WS_API");
            string username = string.Empty;
            string password = string.Empty;
            TasksCommon.RemoteTasksUtils.GetCredentials(request.GroupId, ref username, ref password, ApiObjects.eWSModules.API);

            // update catalog
            using (API apiClient = new API())
            {
                if (!string.IsNullOrEmpty(url))
                    apiClient.Url = url;

                bool success = apiClient.UpdateImageState(username, password, request.RowId, request.Version, (ImageUploadHandler.WS_API.eMediaType)request.MediaType, status);

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
        }

        private static void RemoveRecoveryMessage(int groupId, string bucket, string key)
        {
            // remove CB document through WS API
            string url = WS_Utils.GetTcmConfigValue("WS_API");
            string username = string.Empty;
            string password = string.Empty;
            TasksCommon.RemoteTasksUtils.GetCredentials(groupId, ref username, ref password, ApiObjects.eWSModules.API);

            using (API apiClient = new API())
            {
                if (!string.IsNullOrEmpty(url))
                    apiClient.Url = url;

                bool success = apiClient.ModifyCB(username, password, bucket, key, WS_API.eDbActionType.Delete, null, 0);
                if (!success)
                    log.ErrorFormat("Error while trying to remove CB document. bucket: {0}, key: {1}", bucket, key);
                else
                    log.DebugFormat("Successfully removed CB document. bucket: {0}, key: {1}", bucket, key);
            }
        }

        private string HttpPost(string uri, string parameters, string contentType = null)
        {
            try
            {
                WebRequest request = WebRequest.Create(uri);

                request.Method = "POST";
                string postData = parameters;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                if (contentType == null)
                    request.ContentType = "application/x-www-form-urlencoded";
                else
                    request.ContentType = contentType;

                request.ContentLength = byteArray.Length;

                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();

                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                reader.Close();
                dataStream.Close();
                response.Close();

                return responseFromServer;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error on post request. URL: {0}, Parameter: {1}. Error: {2}", uri, parameters, ex);
            }
            return null;
        }
    }
}


namespace ImageUploadHandler.WS_API
{
    // adding request ID to header
    public partial class API
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);

            if (request.Headers != null &&
                request.Headers[Constants.REQUEST_ID_KEY] == null &&
                HttpContext.Current.Items[Constants.REQUEST_ID_KEY] != null)
            {
                request.Headers.Add(Constants.REQUEST_ID_KEY, HttpContext.Current.Items[Constants.REQUEST_ID_KEY].ToString());
            }
            return request;
        }
    }
}
