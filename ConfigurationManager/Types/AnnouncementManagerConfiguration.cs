using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class AnnouncementManagerConfiguration : BaseConfig<AnnouncementManagerConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.AnnouncementManagerConfiguration;
        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<string> PushDomainName = new BaseValue<string>("PushDomainName", null,true);
        public BaseValue<string> PushServerKey = new BaseValue<string>("PushServerKey", null,true);
        public BaseValue<string> PushServerIV = new BaseValue<string>("PushServerIV", null,true);


    }
}
