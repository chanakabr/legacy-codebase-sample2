using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager.Types
{
    public class S3Configuration : ConfigurationValue
    {
        public StringConfigurationValue Region;
        public StringConfigurationValue AccessKey;
        public StringConfigurationValue SecretKey;
        public StringConfigurationValue BucketName;
        public StringConfigurationValue Path;
        public NumericConfigurationValue NumberOfRetries;
        public BooleanConfigurationValue ShouldDeleteSourceFile;

        public S3Configuration(string key) 
            : base(key)
        {
            Initialize();
        }

        public S3Configuration(string key, ConfigurationValue parent) 
            : base(key, parent)
        {
            Initialize();
        }

        protected void Initialize()
        {
            Region = new StringConfigurationValue("region", this)
            {
                DefaultValue = string.Empty,
            };
            AccessKey = new StringConfigurationValue("accessKey", this)
            {
                DefaultValue = string.Empty,
            };
            SecretKey = new StringConfigurationValue("secretKey", this)
            {
                DefaultValue = string.Empty,
            };
            BucketName = new StringConfigurationValue("bucketName", this)
            {
                DefaultValue = string.Empty,
            };
            Path = new StringConfigurationValue("path", this)
            {
                DefaultValue = string.Empty,
            };
            NumberOfRetries = new NumericConfigurationValue("numberOfRetries", this)
            {
                DefaultValue = 1,
            };
            ShouldDeleteSourceFile = new BooleanConfigurationValue("shouldDeleteSourceFile", this)
            {
                DefaultValue = false,
            };
        }
    }
}
