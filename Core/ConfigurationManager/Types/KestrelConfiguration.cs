using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager.Types
{
    public class KestrelConfiguration : BaseConfig<KestrelConfiguration>
    {
        public BaseValue<long> MaxRequestBodySize = new BaseValue<long>("max_request_body_size_in_meagbyte", 30, false, "maximum allowed size of any request body in megabytes");

        public override string TcmKey => TcmObjectKeys.KestrelConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };
    }
}