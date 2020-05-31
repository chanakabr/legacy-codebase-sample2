using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class IotAdapterConfiguration : BaseConfig<IotAdapterConfiguration>
    {
        public BaseValue<string> AdapterUrl = new BaseValue<string>("iot_adapter_url", string.Empty);
        public override string TcmKey => TcmObjectKeys.IotConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };
    }
}
