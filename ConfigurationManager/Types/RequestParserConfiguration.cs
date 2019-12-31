using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class RequestParserConfiguration : ConfigurationValue
    {
        public StringConfigurationValue AccessTokenKeyFormat;
        public NumericConfigurationValue AccessTokenLength;
        public StringConfigurationValue TempUploadFolder;
        private StringConfigurationValue KsSecretsPrimary;
        private StringConfigurationValue KsSecretSecondary;
        public List<string> KsSecrets { get; private set; }

        public RequestParserConfiguration(string key) : base(key)
        {
            AccessTokenKeyFormat = new StringConfigurationValue("access_token_key_format", this)
            {
                DefaultValue = "access_{0}",
                OriginalKey = "access_token_key_format"
            };
            AccessTokenLength = new NumericConfigurationValue("access_token_length", this)
            {
                DefaultValue = 32,
                OriginalKey = "access_token_length"
            };
            TempUploadFolder = new StringConfigurationValue("tempUploadFolder", this)
            {
                DefaultValue = "c:\\tmp\\src",
                ShouldAllowEmpty = false
            };
            KsSecretsPrimary = new StringConfigurationValue("ks_secrets_primary", this)
            {
                DefaultValue = string.Empty,
                ShouldAllowEmpty = true,
                OriginalKey = "ks_secrets_primary"
            };
            KsSecretSecondary = new StringConfigurationValue("ks_secrets_secondary", this)
            {
                DefaultValue = string.Empty,
                ShouldAllowEmpty = true,
                OriginalKey = "ks_secrets_secondary"
            };
            KsSecrets = new List<string> { this.KsSecretsPrimary?.Value, this.KsSecretSecondary?.Value };
        }
    }
}