using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager.Types
{
    public class FileSystemConfiguration : ConfigurationValue
    {
        public StringConfigurationValue DestPath;
        public StringConfigurationValue PublicUrl;

        public FileSystemConfiguration(string key) 
            : base(key)
        {
            Initialize();
        }

        public FileSystemConfiguration(string key, ConfigurationValue parent) 
            : base(key, parent)
        {
            Initialize();
        }

        protected void Initialize()
        {
            DestPath = new StringConfigurationValue("destPath", this)
            {
                DefaultValue = string.Empty,
            };
            PublicUrl = new StringConfigurationValue("publicUrl", this)
            {
                DefaultValue = string.Empty,
            };
        }
    }
}
