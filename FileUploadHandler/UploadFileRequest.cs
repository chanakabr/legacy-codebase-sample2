
namespace FileUploadHandler
{
    public class UploadFileRequest
    {
        public string file_name;
        public string file;
        public UploadSettings upload_settings;
    }

    public class UploadSettings
    {
        public string type;
        public string address;
        public string user_name;
        public string password;
        public string prefix;
        public string region;
    }
}
