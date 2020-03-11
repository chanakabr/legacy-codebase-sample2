using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class WebServicesConfiguration : BaseConfig<WebServiceConfiguration>
    {
        public WebServiceConfiguration Users;
        public WebServiceConfiguration ConditionalAccess;
        public WebServiceConfiguration Api;
        public WebServiceConfiguration Pricing;
        public WebServiceConfiguration Billing;
        public WebServiceConfiguration Domains;
        public WebServiceConfiguration Social;
        public WebServiceConfiguration Notification;
        public CatalogWebServiceConfiguration Catalog;

        public WebServicesConfiguration()
        {
            Users = new WebServiceConfiguration("Users",                         "https://webservices.service.consul:48080/ws_users_module.asmx");
            ConditionalAccess = new WebServiceConfiguration("ConditionalAccess", "https://webservices.service.consul:48080/ws_cas_module.asmx");
            Api = new WebServiceConfiguration("Api",                             "https://webservices.service.consul:48080/api.asmx");
            Pricing = new WebServiceConfiguration("Pricing",                     "https://webservices.service.consul:48080/ws_pricing_module.asmx");
            Billing = new WebServiceConfiguration("Billing",                     "https://webservices.service.consul:48080/ws_billing_module.asmx");
            Domains = new WebServiceConfiguration("Domains",                     "https://webservices.service.consul:48080/ws_domains_module.asmx");
            Social = new WebServiceConfiguration("Social",                       "https://webservices.service.consul:48080/ws_social_module.asmx");
            Notification = new WebServiceConfiguration("Notification",           "https://webservices.service.consul:48080/ws_notification_service.svc");
            Catalog = new CatalogWebServiceConfiguration("Catalog",              "https://webservices.service.consul:48080/ws_catalog_service.svc");
        }

        public override string TcmKey => TcmObjectKeys.WebServicesConfiguration;

        public override string[] TcmPath => new[] { TcmKey };
    }

    public class WebServiceConfiguration : BaseConfig<WebServiceConfiguration>
    {
        public BaseValue<string> URL;


        private string _SelfKey;
        public override string TcmKey => _SelfKey;

        public WebServiceConfiguration(string key, string defaultUrl)
        {
            _SelfKey = key;
            URL = new BaseValue<string>("URL", defaultUrl, false, "");
        }


        public override string[] TcmPath => new[] { TcmObjectKeys.WebServicesConfiguration, TcmKey };
    }

    public class CatalogWebServiceConfiguration : WebServiceConfiguration
    {
        public BaseValue<string> SignatureKey = new BaseValue<string>("SignatureKey", "liat regev", false,"");
        public BaseValue<int> CacheDurationSeconds = new BaseValue<int>("CacheDurationSeconds", 0,false, "");

        public CatalogWebServiceConfiguration(string key, string defaultUrl) : base(key, defaultUrl)
        {
        }
    }
}
