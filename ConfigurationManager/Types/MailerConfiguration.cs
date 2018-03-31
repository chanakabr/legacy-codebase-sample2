using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class MailerConfiguration : ConfigurationValue
    {
        public StringConfigurationValue MCKey;
        public StringConfigurationValue MCURL;

        public MailerConfiguration(string key) : base(key)
        {
            MCKey = new StringConfigurationValue("MCKey", this)
            {
                DefaultValue = "5DcCPYFCdFMpSi_994pa4w",
                OriginalKey = "MCKey"
            };
            MCURL = new StringConfigurationValue("MCURL", this)
            {
                DefaultValue = "https://mandrillapp.com/api/1.0/messages/send-template.json",
                OriginalKey = "MCURL"
            };
        }
    }
}