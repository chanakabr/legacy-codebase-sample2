
using EventBus.Abstraction;
using System;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class UploadFileRequest : ServiceEvent
    {
        public string file_name;
        public string file;
        public UploadSettings upload_settings;
    }
    [Serializable]
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
