

namespace ConfigurationManager.ConfigurationSettings.ConfigurationBase
{
    public interface IBaseConfig
    {
        string TcmKey { get; }

        void UpdateWithTcm(string[] path = null);
    }
}
