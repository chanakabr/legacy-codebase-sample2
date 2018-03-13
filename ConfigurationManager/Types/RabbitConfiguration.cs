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
            Default = new BaseRabbitConfiguration("default", this);
            ProfessionalServices = new ProfessionalServicesRabbitConfiguration("professional_services", this);
            ProfessionalServices.RoutingKey.DefaultValue = "CDR_NOTIFICATION";
            ProfessionalServices.RoutingKey.ShouldAllowEmpty = false;

            SocialFeed = new BaseRabbitConfiguration("social_fieed", this);
            Picture = new BaseRabbitConfiguration("picture", this);
            EPG = new BaseRabbitConfiguration("epg", this);
            Indexing = new BaseRabbitConfiguration("indexing", this);
            PushNotification = new BaseRabbitConfiguration("push_notification", this);
            ImageUpload = new BaseRabbitConfiguration("image_upload", this);

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