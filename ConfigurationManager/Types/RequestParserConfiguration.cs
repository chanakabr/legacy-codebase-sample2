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
        }
    }
}