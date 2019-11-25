using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;

namespace ConfigurationManager.Types
{
    public class S3Configuration : BaseConfig<S3Configuration>
    {
        public override string TcmKey => "S3";

        public BaseValue<string> Region = new BaseValue<string>("region", "region", false, "region");
        public BaseValue<string> AccessKey = new BaseValue<string>("accessKey", "accessKey", false, "accessKey");
        public BaseValue<string> SecretKey = new BaseValue<string>("secretKey", "secretKey", false, "secretKey");
        public BaseValue<string> BucketName = new BaseValue<string>("bucketName", "bucketName", false, "bucketName");
        public BaseValue<string> Path = new BaseValue<string>("path", "path", false, "path");
        public BaseValue<int> NumberOfRetries = new BaseValue<int>("numberOfRetries", 90, false, "NumberOfRetries");

        public override void SetActualValues(JToken token)
        {
            if (token != null)
            {
                SetActualValue(token, Region);
                SetActualValue(token, AccessKey);
                SetActualValue(token, SecretKey);
                SetActualValue(token, NumberOfRetries);
                SetActualValue(token, Path);
            }
        }
    }
}
