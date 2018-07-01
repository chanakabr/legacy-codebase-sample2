using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class RabbitConfiguration : ConfigurationValue
    {
        public BaseRabbitConfiguration Default;
        public ProfessionalServicesRabbitConfiguration ProfessionalServices;
        public BaseRabbitConfiguration SocialFeed;
        public BaseRabbitConfiguration Picture;
        public BaseRabbitConfiguration EPG;
        public BaseRabbitConfiguration Indexing;
        public BaseRabbitConfiguration PushNotification;
        public BaseRabbitConfiguration ImageUpload;

        public RabbitConfiguration(string key) : base(key)
        {
            this.Default = new BaseRabbitConfiguration("default", this);
            this.Default.HostName.OriginalKey = "hostName";
            this.Default.UserName.OriginalKey = "userName";
            this.Default.Password.OriginalKey = "password";
            this.Default.Port.OriginalKey = "port";
            this.Default.RoutingKey.OriginalKey = "routingKey";
            this.Default.Exchange.OriginalKey = "exchange";
            this.Default.Queue.OriginalKey = "queue";
            this.Default.VirtualHost.OriginalKey = "virtualHost";
            this.Default.ExchangeType.OriginalKey = "exchangeType";
            
            this.ProfessionalServices = new ProfessionalServicesRabbitConfiguration("professional_services", this);
            this.ProfessionalServices.RoutingKey.DefaultValue = "CDR_NOTIFICATION";
            this.ProfessionalServices.RoutingKey.ShouldAllowEmpty = false;
            this.ProfessionalServices.RoutingKey.OriginalKey = "ProfessionalServices.routingKey";
            this.ProfessionalServices.Exchange.OriginalKey = "ProfessionalServices.exchange";
            this.ProfessionalServices.Queue.DefaultValue = ".";
            this.ProfessionalServices.VirtualHost.OriginalKey = "ProfessionalServices.virtualHost";
            this.ProfessionalServices.ExchangeType.OriginalKey = "ProfessionalServices.exchangeType";

            this.SocialFeed = new BaseRabbitConfiguration("social_feed", this);
            this.SocialFeed.RoutingKey.OriginalKey = "routingKeySocialFeed";
            this.SocialFeed.Exchange.OriginalKey = "exchangeSocialFeed";
            this.SocialFeed.Queue.OriginalKey = "queueSocialFeed";
            this.SocialFeed.VirtualHost.OriginalKey = "virtualHostSocialFeed";
            this.SocialFeed.ExchangeType.OriginalKey = "exchangeTypeSocialFeed";

            this.Picture = new BaseRabbitConfiguration("picture", this);
            this.Picture.RoutingKey.OriginalKey = "routingKeyPicture";
            this.Picture.Exchange.OriginalKey = "exchangePicture";
            this.Picture.Queue.OriginalKey = "queuePicture";
            this.Picture.VirtualHost.OriginalKey = "virtualHostPicture";
            this.Picture.ExchangeType.OriginalKey = "exchangeTypePicture";

            this.EPG = new BaseRabbitConfiguration("epg", this);
            this.EPG.RoutingKey.OriginalKey = "routingKeyEPG";
            this.EPG.Exchange.OriginalKey = "exchangeEPG";
            this.EPG.Queue.OriginalKey = "queueEPG";
            this.EPG.VirtualHost.OriginalKey = "virtualHostEPG";
            this.EPG.ExchangeType.OriginalKey = "exchangeTypeEPG";

            this.Indexing = new BaseRabbitConfiguration("indexing", this);
            this.Indexing.RoutingKey.OriginalKey = "IndexingData.routingKey";
            this.Indexing.Exchange.OriginalKey = "IndexingData.exchange";
            this.Indexing.Queue.DefaultValue = ".";
            this.Indexing.VirtualHost.OriginalKey = "IndexingData.virtualHost";
            this.Indexing.ExchangeType.OriginalKey = "IndexingData.exchangeType";

            this.PushNotification = new BaseRabbitConfiguration("push_notification", this);
            this.PushNotification.RoutingKey.OriginalKey = "PushNotifications.routingKey";
            this.PushNotification.Exchange.OriginalKey = "PushNotifications.exchange";
            this.PushNotification.Queue.DefaultValue = ".";
            this.PushNotification.VirtualHost.OriginalKey = "PushNotifications.virtualHost";
            this.PushNotification.ExchangeType.OriginalKey = "PushNotifications.exchangeType";

            this.ImageUpload = new BaseRabbitConfiguration("image_upload", this);
            this.ImageUpload.RoutingKey.OriginalKey = "routingKey";
            this.ImageUpload.Exchange.OriginalKey = "ImageUpload.exchange";
            this.ImageUpload.Queue.OriginalKey = "queue";
            this.ImageUpload.VirtualHost.OriginalKey = "virtualHost";
            this.ImageUpload.ExchangeType.OriginalKey = "exchangeType";

            ProfessionalServices.CopyBaseValues(this.Default);
            SocialFeed.CopyBaseValues(this.Default);
            Picture.CopyBaseValues(this.Default);
            EPG.CopyBaseValues(this.Default);
            Indexing.CopyBaseValues(this.Default);
            PushNotification.CopyBaseValues(this.Default);
            ImageUpload.CopyBaseValues(this.Default);
        }

        internal override bool Validate()
        {
            bool result = true;

            result &= Default.Validate();
            result &= ProfessionalServices.Validate();
            result &= SocialFeed.Validate();
            result &= Picture.Validate();
            result &= EPG.Validate();
            result &= Indexing.Validate();
            result &= PushNotification.Validate();
            result &= ImageUpload.Validate();

            return result;
        }
    }
}