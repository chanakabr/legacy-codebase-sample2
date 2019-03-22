using System;
using System.Configuration;

namespace TCMClient
{
    public class TCMConfiguration : ConfigurationSection
    {
        private string m_URL = null;

        private string m_Application = null;

        private string m_Host = null;

        private string m_Environment = null;

        private string m_AppID = null;

        private string m_AppSecret = null;

        private bool? m_VerifySSL = null;

        private bool? m_FromLocal = null;

        private string m_LocalPath = null;

        public void OverrideEnvironmentVariable()
        {
            string application = System.Environment.GetEnvironmentVariable("TCM_APP");
            string url = System.Environment.GetEnvironmentVariable("TCM_URL");
            string environment = System.Environment.GetEnvironmentVariable("TCM_SECTION");
            string appID = System.Environment.GetEnvironmentVariable("TCM_APP_ID");
            string appSecret = System.Environment.GetEnvironmentVariable("TCM_APP_SECRET");
            string verifySSL = System.Environment.GetEnvironmentVariable("TCM_VERIFY_SSL");

            if (application != null)
            {
                Application = application;
            }

            if (url != null)
            {
                URL = url;
            }

            if (environment != null)
            {
                Environment = environment;
            }

            if (appID != null)
            {
                AppID = appID;
            }

            if (appSecret != null)
            {
                AppSecret = appSecret;
            }

            if (verifySSL != null && (verifySSL.Equals("1") || verifySSL.ToLower().Equals("true")))
            {
                VerifySSL = true;
            }
            else
            {
                VerifySSL = false;
            }
        }

        [ConfigurationProperty("URL", IsRequired = true)]
        public string URL
        {
            get
            {
                return m_URL != null ? m_URL : this["URL"] as string;
            }


            set => m_URL = value;
        }

        [ConfigurationProperty("Application", IsRequired = true)]
        public string Application
        {
            get
            {
                return m_Application != null ? m_Application : this["Application"] as string;
            }

            set
            {
                m_Application = value;
            }
        }

        [ConfigurationProperty("Host", IsRequired = true)]
        public string Host
        {

            get
            {
                return m_Host != null ? m_Host : this["Host"] as string;
            }

            set
            {
                m_Host = value;
            }
        }

        [ConfigurationProperty("Environment", IsRequired = true)]
        public string Environment
        {

            get
            {
                return m_Environment != null ? m_Environment : this["Environment"] as string;
            }

            set
            {
                m_Environment = value;
            }
        }

        [ConfigurationProperty("AppID", IsRequired = true)]
        public string AppID
        {

            get
            {
                return m_AppID != null ? m_AppID : this["AppID"] as string;
            }

            set
            {
                m_AppID = value;
            }
        }

        [ConfigurationProperty("AppSecret", IsRequired = true)]
        public string AppSecret
        {

            get
            {
                return m_AppSecret != null ? m_AppSecret : this["AppSecret"] as string;
            }

            set
            {
                m_AppSecret = value;
            }
        }

        [ConfigurationProperty("VerifySSL", DefaultValue = "false", IsRequired = false)]
        public bool VerifySSL
        {

            get
            {
                return m_VerifySSL.HasValue ? m_VerifySSL.Value : (Boolean)this["VerifySSL"];
            }

            set
            {
                m_VerifySSL = value;
            }
        }

        [ConfigurationProperty("FromLocal", DefaultValue = "false", IsRequired = false)]
        public Boolean FromLocal
        {

            get
            {
                return m_FromLocal.HasValue ? m_FromLocal.Value : (Boolean)this["FromLocal"];
            }

            set
            {
                m_FromLocal = value;
            }
        }

        [ConfigurationProperty("LocalPath", IsRequired = false)]
        public string LocalPath
        {

            get
            {
                return m_LocalPath != null ? m_LocalPath : this["LocalPath"] as string;
            }

            set
            {
                m_LocalPath = value;
            }
        }
    }
}
