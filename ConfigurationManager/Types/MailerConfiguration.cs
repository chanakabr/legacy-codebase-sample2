using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class MailerConfiguration : BaseConfig<MailerConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.MailerConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<string> MCKey = new BaseValue<string>("MCKey", "5DcCPYFCdFMpSi_994pa4w");
        public BaseValue<string> MCURL = new BaseValue<string>("MCURL", "https://mandrillapp.com/api/1.0/messages/send-template.json");



    }
}