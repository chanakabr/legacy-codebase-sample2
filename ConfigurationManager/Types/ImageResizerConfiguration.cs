using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class ImageResizerConfiguration : ConfigurationValue
    {
        public StringConfigurationValue ProxyAddress;
        public StringConfigurationValue ProxyUsername;
        public StringConfigurationValue ProxyPassword;
        public BooleanConfigurationValue UseFileSystem;
        public StringConfigurationValue ImagesBasePath;

        public ImageResizerConfiguration(string key) : base(key)
        {
            ProxyAddress = new ConfigurationManager.StringConfigurationValue("proxy_address")
            {
                ShouldAllowEmpty = true
            };
            ProxyUsername = new ConfigurationManager.StringConfigurationValue("proxy_username")
            {
                ShouldAllowEmpty = true
            };
            ProxyPassword = new StringConfigurationValue("proxy_password")
            {
                ShouldAllowEmpty = true
            };
            UseFileSystem = new ConfigurationManager.BooleanConfigurationValue("use_file_system")
            {
                ShouldAllowEmpty = true,
                DefaultValue = true
            };
            ImagesBasePath = new ConfigurationManager.StringConfigurationValue("images_base_path")
            {
                ShouldAllowEmpty = true
            };
        }
    }
}
