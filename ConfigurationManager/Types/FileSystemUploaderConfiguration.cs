using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager.Types
{
    public class FileSystemUploaderConfiguration : ConfigurationValue
    {
        public StringConfigurationValue DestPath;
        public StringConfigurationValue PublicUrl;
        public BooleanConfigurationValue ShouldDeleteSourceFile;

        public FileSystemUploaderConfiguration(string key) : base(key)
        {
            DestPath = new StringConfigurationValue("destPath", this)
            {
                DefaultValue = string.Empty,
                OriginalKey = "FileSystemUploader.DestPath"
            };
            PublicUrl = new StringConfigurationValue("publicUrl", this)
            {
                DefaultValue = string.Empty,
                OriginalKey = "FileSystemUploader.PublicUrl"
            };
            ShouldDeleteSourceFile = new BooleanConfigurationValue("shouldDeleteSourceFile", this)
            {
                DefaultValue = false,
                OriginalKey = "FileSystemUploader.ShouldDeleteSourceFile"
            };
        }
    }
}
