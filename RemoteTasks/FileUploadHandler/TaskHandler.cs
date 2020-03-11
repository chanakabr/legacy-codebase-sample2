using System.Reflection;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;

namespace FileUploadHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string res = null;

            UploadFileRequest request = JsonConvert.DeserializeObject<UploadFileRequest>(data);
            BaseUploader uploader = UploaderFactory.GetUploader(request);

            try
            {
                uploader.Upload(request.file, request.file_name);
                log.DebugFormat("successfully uploaded file. request: {0}", data);
            }
            catch (System.Exception ex)
            {
                log.ErrorFormat("Error uploading request. request: {0}, ex: {1}", data, ex);
            }
            return res;
        }
    }
}
