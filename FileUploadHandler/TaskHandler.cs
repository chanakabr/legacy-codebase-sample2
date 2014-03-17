using Newtonsoft.Json;
using RemoteTasksCommon;

namespace FileUploadHandler
{
    public class TaskHandler : ITaskHandler
    {
        public string HandleTask(string data)
        {
            string res = null;

            UploadFileRequest request = JsonConvert.DeserializeObject<UploadFileRequest>(data);

            BaseUploader uploader = UploaderFactory.GetUploader(request);

            uploader.Upload(request.file, request.file_name);

            return res;
        }
    }
}
