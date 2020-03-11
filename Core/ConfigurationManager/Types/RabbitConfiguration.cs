using ConfigurationManager.ConfigurationSettings.ConfigurationBase;


namespace ConfigurationManager
{
    public class RabbitConfiguration : BaseConfig<RabbitConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.MainRabbitConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public ProfessionalServicesRabbitConfiguration ProfessionalServices = new ProfessionalServicesRabbitConfiguration();
        public SocialFeedRabbitConfiguration SocialFeed = new SocialFeedRabbitConfiguration();
        public DefaultRabbitConfiguration Default = new DefaultRabbitConfiguration();
        public PictureRabbitConfiguration Picture = new PictureRabbitConfiguration();
        public EPGRabbitConfiguration EPG = new EPGRabbitConfiguration();
        public IndexingRabbitConfiguration Indexing = new IndexingRabbitConfiguration();
        public PushNotificationRabbitConfiguration PushNotification = new PushNotificationRabbitConfiguration();
        public ImageUploadRabbitConfiguration ImageUpload = new ImageUploadRabbitConfiguration();
        public EventBusRabbitConfiguration EventBus = new EventBusRabbitConfiguration();

    }



    public class DefaultRabbitConfiguration : BaseRabbitConfiguration
    {
        public override string TcmKey => TcmObjectKeys.DefaultRabbitConfiguration;

        public override string[] TcmPath => new string[] { TcmObjectKeys.MainRabbitConfiguration, TcmKey };
    }

    public class SocialFeedRabbitConfiguration : BaseRabbitConfiguration
    {
        public override string TcmKey => TcmObjectKeys.SocialFeedRabbitConfiguration;

        public override string[] TcmPath => new string[] { TcmObjectKeys.MainRabbitConfiguration, TcmKey };
    }

    public class PictureRabbitConfiguration : BaseRabbitConfiguration
    {
        public override string TcmKey => TcmObjectKeys.PictureRabbitConfiguration;

        public override string[] TcmPath => new string[] { TcmObjectKeys.MainRabbitConfiguration, TcmKey };

        public new BaseValue<string> RoutingKey = new BaseValue<string>("routingKey", "PROCESS_IMAGE");
        public new BaseValue<string> Queue = new BaseValue<string>("queue", "tasks.process_image");
    }

    public class EPGRabbitConfiguration : BaseRabbitConfiguration
    {
        public override string TcmKey => TcmObjectKeys.EPGRabbitConfiguration;

        public override string[] TcmPath => new string[] { TcmObjectKeys.MainRabbitConfiguration, TcmKey };
    }

    public class EventBusRabbitConfiguration : BaseRabbitConfiguration
    {
        public override string TcmKey => TcmObjectKeys.EventBusRabbitConfiguration;

        public override string[] TcmPath => new string[] { TcmObjectKeys.MainRabbitConfiguration, TcmKey };

        public new BaseValue<string> Exchange = new BaseValue<string>("exchange", "kaltura_event_bus", false, "RabbitMQ exchange. Only for 'default' it is mandatory.");
    }

    public class PushNotificationRabbitConfiguration : BaseRabbitConfiguration
    {
        public override string TcmKey => TcmObjectKeys.PushNotificationRabbitConfiguration;

        public override string[] TcmPath => new string[] { TcmObjectKeys.MainRabbitConfiguration, TcmKey };
    }

    public class ImageUploadRabbitConfiguration : BaseRabbitConfiguration
    {
        public override string TcmKey => TcmObjectKeys.ImageUploadRabbitConfiguration;

        public override string[] TcmPath => new string[] { TcmObjectKeys.MainRabbitConfiguration, TcmKey };
    }

    public class IndexingRabbitConfiguration : BaseRabbitConfiguration
    {
        public override string TcmKey => TcmObjectKeys.IndexingRabbitConfiguration;

        public override string[] TcmPath => new string[] { TcmObjectKeys.MainRabbitConfiguration, TcmKey };
    }
}