using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;

namespace ConfigurationManager.Types
{
    public class FileSystemConfiguration : BaseConfig<FileSystemConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.FileSystem;

        public override string[] TcmPath => new string[] { TcmObjectKeys.FileUpload, TcmKey };

        public BaseValue<string> DestPath = new BaseValue<string>("destPath", "Tzachi", false, "description");
        public BaseValue<string> PublicUrl = new BaseValue<string>("publicUrl", "publicUrl", false, "description");


    }
}
