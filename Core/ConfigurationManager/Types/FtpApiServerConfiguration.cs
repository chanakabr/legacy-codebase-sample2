using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class FtpApiServerConfiguration : ConfigurationValue
    {
        public NumericConfigurationValue FtpServerPort;
        public StringConfigurationValue FtpServerAddress;
        public StringConfigurationValue PhoenixServerUrl;

        public FtpApiServerConfiguration(string key) : base(key)
        {
            FtpServerPort = new NumericConfigurationValue("ftp_server_port", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = 21,
            };

            FtpServerAddress = new StringConfigurationValue("ftp_server_address", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = "localhost",
            };

            PhoenixServerUrl = new StringConfigurationValue("phoenix_server_url", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = "localhost:8080",
            };
        }
    }
}
