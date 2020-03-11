using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class WebServicesConfiguration : ConfigurationValue
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

        public WebServicesConfiguration(string key) : base(key)
        {
            Users = new ConfigurationManager.WebServiceConfiguration("Users", this);
            ConditionalAccess = new ConfigurationManager.WebServiceConfiguration("ConditionalAccess", this);
            Api = new ConfigurationManager.WebServiceConfiguration("Api", this);
            Pricing = new ConfigurationManager.WebServiceConfiguration("Pricing", this);
            Billing = new ConfigurationManager.WebServiceConfiguration("Billing", this);
            Domains = new ConfigurationManager.WebServiceConfiguration("Domains", this);
            Social = new ConfigurationManager.WebServiceConfiguration("Social", this);
            Notification = new ConfigurationManager.WebServiceConfiguration("Notification", this);
            Catalog = new ConfigurationManager.CatalogWebServiceConfiguration("Catalog", this);
        }
    }

    public class WebServiceConfiguration : ConfigurationValue
    {
        public StringConfigurationValue URL;
        
        public WebServiceConfiguration(string key) : base(key)
        {
            this.Initialize();
        }

        public WebServiceConfiguration(string key, ConfigurationValue parent) : base(key, parent)
        {
            this.Initialize();
        }

        protected virtual void Initialize()
        {
            this.URL = new ConfigurationManager.StringConfigurationValue("URL", this)
            {
                ShouldAllowEmpty = true
            };
        }
    }

    public class CatalogWebServiceConfiguration : WebServiceConfiguration
    {
        public StringConfigurationValue SignatureKey;
        public NumericConfigurationValue CacheDurationSeconds;

        public CatalogWebServiceConfiguration(string key) : base(key)
        {
            this.Initialize();
        }

        public CatalogWebServiceConfiguration(string key, ConfigurationValue parent) : base(key, parent)
        {
            this.Initialize();
        }
        
        protected override void Initialize()
        {
            base.Initialize();

            this.SignatureKey = new ConfigurationManager.StringConfigurationValue("SignatureKey", this)
            {
                DefaultValue = "liat regev"
            };
            this.CacheDurationSeconds = new NumericConfigurationValue("CacheDurationSeconds", this)
            {
                ShouldAllowEmpty = true
            };
        }
    }
}
