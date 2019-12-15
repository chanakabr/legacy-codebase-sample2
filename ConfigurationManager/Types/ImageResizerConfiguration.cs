using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class ImageResizerConfiguration : BaseConfig<ImageResizerConfiguration>
    {
        public override string[] TcmPath => new string[] { TcmKey };
        public override string TcmKey => TcmObjectKeys.ImageResizerConfiguration;

        public BaseValue<string> ProxyAddress = new BaseValue<string>("proxy_address", null);
        public BaseValue<string> ProxyUsername = new BaseValue<string>("proxy_username", null);
        public BaseValue<string> ProxyPassword = new BaseValue<string>("proxy_password", TcmObjectKeys.Stub,true);
        public BaseValue<string> ImagesBasePath = new BaseValue<string>("use_file_system", null);
        public BaseValue<bool>   UseFileSystem = new BaseValue<bool>("use_file_system", true);

    }
}
