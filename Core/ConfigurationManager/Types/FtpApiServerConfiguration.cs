using ConfigurationManager.ConfigurationSettings.ConfigurationBase;


namespace ConfigurationManager
{
    public class FtpApiServerConfiguration : BaseConfig<FtpApiServerConfiguration>
    {

        public override string TcmKey => TcmObjectKeys.FtpApiServerConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<int> FtpServerPort = new BaseValue<int>("ftp_server_port", 21);
        public BaseValue<string> FtpServerAddress = new BaseValue<string>("ftp_server_address", "localhost");
        public BaseValue<string> PhoenixServerUrl = new BaseValue<string>("phoenix_server_url", "localhost:8080");
    }
}
