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
            ProxyAddress = new ConfigurationManager.StringConfigurationValue("proxy_address", this)
            {
                ShouldAllowEmpty = true
            };
            ProxyUsername = new ConfigurationManager.StringConfigurationValue("proxy_username", this)
            {
                ShouldAllowEmpty = true
            };
            ProxyPassword = new StringConfigurationValue("proxy_password", this)
            {
                ShouldAllowEmpty = true
            };
            UseFileSystem = new ConfigurationManager.BooleanConfigurationValue("use_file_system", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = true
            };
            ImagesBasePath = new ConfigurationManager.StringConfigurationValue("images_base_path", this)
            {
                ShouldAllowEmpty = true
            };
        }
    }
}
