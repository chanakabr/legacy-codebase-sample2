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

        public S3Configuration(string key) : base(key)
        {
            Region = new StringConfigurationValue("region", this)
            {
                DefaultValue = string.Empty,
                OriginalKey = "S3.Region"
            };
            AccessKey = new StringConfigurationValue("accessKey", this)
            {
                DefaultValue = string.Empty,
                OriginalKey = "S3.AccessKey"
            };
            SecretKey = new StringConfigurationValue("secretKey", this)
            {
                DefaultValue = string.Empty,
                OriginalKey = "S3.SecretKey"
            };
            BucketName = new StringConfigurationValue("bucketName", this)
            {
                DefaultValue = string.Empty,
                OriginalKey = "S3.BucketName"
            };
            Path = new StringConfigurationValue("path", this)
            {
                DefaultValue = string.Empty,
                OriginalKey = "S3.Path"
            };
            NumberOfRetries = new NumericConfigurationValue("numberOfRetries", this)
            {
                DefaultValue = 1,
                OriginalKey = "S3.NumberOfRetries"
            };
        }
    }
}
