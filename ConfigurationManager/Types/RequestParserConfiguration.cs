using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class RequestParserConfiguration : BaseConfig<RequestParserConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.RequestParserConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<string> AccessTokenKeyFormat = new BaseValue<string>("access_token_key_format", "access_{0}");
        public BaseValue<string> TempUploadFolder = new BaseValue<string>("tempUploadFolder", "c:\\tmp\\src", false);
        public BaseValue<int> AccessTokenLength = new BaseValue<int>("access_token_length", 32);

    
    }
}