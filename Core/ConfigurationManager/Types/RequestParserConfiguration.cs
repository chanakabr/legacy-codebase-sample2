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

        //=======
        //        public StringConfigurationValue AccessTokenKeyFormat;
        //        public NumericConfigurationValue AccessTokenLength;
        //        public StringConfigurationValue TempUploadFolder;
        //        private StringConfigurationValue KsSecretsPrimary;
        //        private StringConfigurationValue KsSecretSecondary;
        //        public List<string> KsSecrets { get; private set; }

        //        public RequestParserConfiguration(string key) : base(key)
        //        {
        //            AccessTokenKeyFormat = new StringConfigurationValue("access_token_key_format", this)
        //            {
        //                DefaultValue = "access_{0}",
        //                OriginalKey = "access_token_key_format"
        //            };
        //            AccessTokenLength = new NumericConfigurationValue("access_token_length", this)
        //            {
        //                DefaultValue = 32,
        //                OriginalKey = "access_token_length"
        //            };
        //            TempUploadFolder = new StringConfigurationValue("tempUploadFolder", this)
        //            {
        //                DefaultValue = "c:\\tmp\\src",
        //                ShouldAllowEmpty = false
        //            };
        //            KsSecretsPrimary = new StringConfigurationValue("ks_secrets_primary", this)
        //            {
        //                DefaultValue = "528d222c-f124-4448-93ad-68ca2e4d706e",
        //                ShouldAllowEmpty = true,
        //                OriginalKey = "ks_secrets_primary"
        //            };
        //            KsSecretSecondary = new StringConfigurationValue("ks_secrets_secondary", this)
        //            {
        //                DefaultValue = string.Empty,
        //                ShouldAllowEmpty = true,
        //                OriginalKey = "ks_secrets_secondary"
        //            };
        //            KsSecrets = new List<string> { this.KsSecretsPrimary?.Value, this.KsSecretSecondary?.Value };
        //        }
        //>>>>>>> origin/master
    }
}