using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;

namespace ConfigurationManager.Types
{
    public class S3Configuration : BaseConfig<S3Configuration>
    {
        public override string TcmKey => TcmObjectKeys.S3;
        public override string[] TcmPath => new string[] { TcmObjectKeys.FileUpload, TcmKey };

        public BaseValue<string> Region = new BaseValue<string>("region", "region", false, "region");
        public BaseValue<string> BucketName = new BaseValue<string>("bucketName", "bucketName", false, "bucketName");
        public BaseValue<string> Path = new BaseValue<string>("path", "path", false, "path");
        public BaseValue<int> NumberOfRetries = new BaseValue<int>("numberOfRetries", 90, false, "NumberOfRetries");


    }
}
