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
                ShouldAllowEmpty = true,
                OriginalKey = "proxyAddress"
            };
            ProxyUsername = new ConfigurationManager.StringConfigurationValue("proxy_username", this)
            {
                ShouldAllowEmpty = true,
                OriginalKey = "proxyUsername"
            };
            ProxyPassword = new StringConfigurationValue("proxy_password", this)
            {
                ShouldAllowEmpty = true,
                OriginalKey = "proxyPassword"
            };
            UseFileSystem = new ConfigurationManager.BooleanConfigurationValue("use_file_system", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = true,
                OriginalKey = "TASK_HANDLERS.IMAGE_RESIZER.USE_FILE_SYSTEM"
            };
            ImagesBasePath = new ConfigurationManager.StringConfigurationValue("images_base_path", this)
            {
                ShouldAllowEmpty = true,
                OriginalKey = "TASK_HANDLERS.IMAGE_RESIZER.IMAGES_BASE_PATH"
            };
        }
    }
}
