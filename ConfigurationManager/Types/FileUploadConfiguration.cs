using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;

namespace ConfigurationManager.Types
{
    public class FileUploadConfiguration : BaseConfig<FileUploadConfiguration>
    {
        public override string TcmKey => Key;
        public const string Key = "FileUpload";

        public S3Configuration S3 = new S3Configuration();
        public FileSystemConfiguration FileSystem = new FileSystemConfiguration();

        public BaseValue<eFileUploadType> Type = new BaseValue<eFileUploadType>("type", eFileUploadType.None, false, "description");
        public BaseValue<bool> ShouldDeleteSourceFile = new BaseValue<bool>("shouldDeleteSourceFile", true, true, "shouldDeleteSourceFile  description");



        public override void SetActualValues(JToken token)
        {
            if (token != null)
            {
                SetActualValue(token, Type);
                SetActualValue(token, ShouldDeleteSourceFile);
            }
        }


    }

    public enum eFileUploadType
    {
        None,
        S3,
        FileSystem
    }
}