using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;


namespace ConfigurationManager.Types
{
    public class DataLakeConfiguration : BaseConfig<DataLakeConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.DataLake;

        public override string[] TcmPath => new string[] { TcmKey };

        public S3Configuration S3 = new S3DataLakeConfiguration();

        public BaseValue<string> PrefixFormat = new BaseValue<string>("prefixFormat", "tvmexport/{0}/{1}/{2}/{3}", false, "prefixFormat");
    }
}
