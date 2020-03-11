using Newtonsoft.Json.Linq;

namespace ConfigurationManager.ConfigurationSettings.ConfigurationBase
{
    public interface IBaseConfig
    {
        string TcmKey { get; }
        string[] TcmPath { get; }

        void SetActualValue<TV>(JToken token, BaseValue<TV> defaultData);

        bool Validate();
    }
}
