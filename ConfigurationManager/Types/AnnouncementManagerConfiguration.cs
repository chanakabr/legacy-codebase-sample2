using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class AnnouncementManagerConfiguration : ConfigurationValue
    {
        public StringConfigurationValue PushDomainName;
        public StringConfigurationValue PushServerKey;
        public StringConfigurationValue PushServerIV;

        public AnnouncementManagerConfiguration(string key) : base(key)
        {
            PushDomainName = new StringConfigurationValue("PushDomainName", this)
            {
                OriginalKey = "PushDomainName"
            };
            PushServerKey = new StringConfigurationValue("PushServerKey", this)
            {
                OriginalKey = "PushServerKey"
            };
            PushServerIV = new StringConfigurationValue("PushServerIV", this)
            {
                OriginalKey = "PushServerIV"
            };
        }
    }
}
