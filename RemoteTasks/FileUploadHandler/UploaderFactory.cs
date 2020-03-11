
namespace FileUploadHandler
{

    public class UploaderFactory
    {
        public static BaseUploader GetUploader(UploadFileRequest request)
        {
            BaseUploader uploader = null;

            switch (request.upload_settings.type.ToLower())
            {
                case "ftp":
                    uploader = new FTPUploader(request.upload_settings.address, request.upload_settings.user_name, request.upload_settings.password, request.upload_settings.prefix);  
                    break;
                case "amazons3":
                    uploader = new AmazonS3Uploader(request.upload_settings.address, request.upload_settings.user_name, request.upload_settings.password, request.upload_settings.prefix, request.upload_settings.region);
                    break;
                case "local":
                default:
                    uploader = new LocalUploader();
                    break;
            }

            return uploader;
        }
    }
}
