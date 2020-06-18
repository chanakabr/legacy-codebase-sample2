using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using System.Collections.Generic;

namespace ConfigurationManager
{
    public class RequestParserConfiguration : BaseConfig<RequestParserConfiguration>
    {
//<<<<<<< HEAD
        public override string TcmKey => TcmObjectKeys.RequestParserConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<string> AccessTokenKeyFormat = new BaseValue<string>("access_token_key_format", "access_{0}");
        public BaseValue<string> TempUploadFolder = new BaseValue<string>("tempUploadFolder", "c:\\tmp\\src", false);
        public BaseValue<int> AccessTokenLength = new BaseValue<int>("access_token_length", 32);
        public BaseValue<string> KsSecretsPrimary = new BaseValue<string>("ks_secrets_primary", "528d222c-f124-4448-93ad-68ca2e4d706e");
        public BaseValue<string> KsSecretsSecondary = new BaseValue<string>("ks_secrets_secondary", string.Empty);
        //this is added due to task https://kaltura.atlassian.net/browse/GEN-848
        public BaseValue<bool> ShouldSaveAsFile = new BaseValue<bool>("Should_save_as_file",false,
            description:"By default bulk uploaded files will passed as memeory stream and wont be saved until needed." +
            "In case value will be set to True,the uploaded data will be saved to file.");

        private List<string> _ksSecrets;
        public List<string> KsSecrets
        { 
            get
            {
                if (_ksSecrets == null)
                {
                    _ksSecrets = new List<string> { this.KsSecretsPrimary?.Value, this.KsSecretsSecondary?.Value };
                }

                return _ksSecrets;
            }
        }      
    }
}