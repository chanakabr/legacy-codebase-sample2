using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class AnnouncementManagerConfiguration : BaseConfig<AnnouncementManagerConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.AnnouncementManagerConfiguration;
        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<string> PushDomainName = new BaseValue<string>("pushdomainname", "push-as.ott.kaltura.com");
        public BaseValue<string> PushServerKey = new BaseValue<string>("pushserverkey", TcmObjectKeys.Stub, true);
        public BaseValue<string> PushServerIV = new BaseValue<string>("pushserveriv", TcmObjectKeys.Stub, true);
    }
}
