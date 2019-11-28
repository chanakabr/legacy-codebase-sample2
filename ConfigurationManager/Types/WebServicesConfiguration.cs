using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Users = new ConfigurationManager.WebServiceConfiguration("Users", "http://webservices/users/module.asmx");
            ConditionalAccess = new ConfigurationManager.WebServiceConfiguration("ConditionalAccess", "http://webservices/cas/module.asmx");
            Api = new ConfigurationManager.WebServiceConfiguration("Api", "http://webservices/api/api.asmx");
            Pricing = new ConfigurationManager.WebServiceConfiguration("Pricing", "http://webservices/pricing/module.asmx");
            Billing = new ConfigurationManager.WebServiceConfiguration("Billing", "http://webservices/billing/module.asmx");
            Domains = new ConfigurationManager.WebServiceConfiguration("Domains", "http://webservices/domains/module.asmx");
            Social = new ConfigurationManager.WebServiceConfiguration("Social", "http://webservices/social/module.asmx");
            Notification = new ConfigurationManager.WebServiceConfiguration("Notification", "http://webservices/notification/service.svc");
            Catalog = new ConfigurationManager.CatalogWebServiceConfiguration("Catalog", "http://webservices/catalog/service.svc");
        }

        public override string TcmKey => "WebServices";

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


        public override string[] TcmPath => new[] { "WebServices", TcmKey };
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
