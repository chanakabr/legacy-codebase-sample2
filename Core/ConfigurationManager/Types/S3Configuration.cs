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
                ShouldAllowEmpty = false
            };
            AccessKey = new StringConfigurationValue("accessKey", this)
            {
                ShouldAllowEmpty = false
            };
            SecretKey = new StringConfigurationValue("secretKey", this)
            {
                ShouldAllowEmpty = false
            };
            BucketName = new StringConfigurationValue("bucketName", this)
            {
                ShouldAllowEmpty = false
            };
            Path = new StringConfigurationValue("path", this)
            {
                ShouldAllowEmpty = false
            };
            NumberOfRetries = new NumericConfigurationValue("numberOfRetries", this)
            {
                DefaultValue = 1,
            };
        }
    }
}
