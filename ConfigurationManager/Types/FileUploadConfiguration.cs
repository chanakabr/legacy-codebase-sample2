using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;

namespace ConfigurationManager.Types
{
    public class FileUploadConfiguration : BaseConfig<FileUploadConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.FileUpload;

        public override string[] TcmPath => new string[] { TcmKey };


        public S3Configuration S3 = new S3Configuration();
        public FileSystemConfiguration FileSystem = new FileSystemConfiguration();

        public BaseValue<eFileUploadType> Type = new BaseValue<eFileUploadType>("type", eFileUploadType.None, false, "description");
        public BaseValue<bool> ShouldDeleteSourceFile = new BaseValue<bool>("shouldDeleteSourceFile", true, true, "shouldDeleteSourceFile  description");






    }

    public enum eFileUploadType
    {
        None,
        S3,
        FileSystem
    }
}