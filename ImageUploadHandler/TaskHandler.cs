using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace ImageUploadHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.InfoFormat("Export request. data={0}", data);

                ImageUploadRequest request = JsonConvert.DeserializeObject<ImageUploadRequest>(data);

                if (request == null)
                    throw new Exception(string.Format("Failed to desterilized image upload request. data = {0}", data != null ? data : string.Empty));

                //get library URL to use.
                object picsBasePath = TVinciShared.PageUtils.GetTableSingleVal("groups", "PICS_REMOTE_BASE_URL", request.GroupId);

                if (picsBasePath == null || !string.IsNullOrEmpty(picsBasePath.ToString()))
                {
                    throw new Exception(string.Format("Failed to get PICS_REMOTE_BASE_URL. groupId = {0}", request.GroupId));
                }

                result = HttpPost(picsBasePath.ToString(), data);
            }

            catch (Exception ex)
            {
                throw ex;
            }

            return result;
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
