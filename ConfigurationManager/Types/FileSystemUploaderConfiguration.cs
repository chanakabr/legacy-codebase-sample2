using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;

namespace ConfigurationManager.Types
{
    public class FileSystemConfiguration : BaseConfig<FileSystemConfiguration>
    {
        public override string TcmKey => "FileSystem";

        public BaseValue<string> DestPath = new BaseValue<string>("destPath", "Tzachi", false, "description");
        public BaseValue<string> PublicUrl = new BaseValue<string>("publicUrl", "publicUrl", false, "description");

        public override void SetActualValues(JToken token)
        {
            if (token != null)
            {
                SetActualValue(token, DestPath);
                SetActualValue(token, PublicUrl);
            }
        }
    }
}
