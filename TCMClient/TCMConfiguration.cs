using System;
using System.Configuration;

namespace TCMClient
{
#if NET45
    public class TCMConfiguration : ConfigurationSection
#else
    public class TCMConfiguration
#endif
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

#if NET45
        [ConfigurationProperty("URL", IsRequired = true)]
#endif
        public string URL
        {
            get
            {
#if NET45
                return m_URL != null ? m_URL : this["URL"] as string;
#else
                return m_URL;
#endif
            }


            set
            {
                m_URL = value;
            }
        }

#if NET45
        [ConfigurationProperty("Application", IsRequired = true)]
#endif
        public string Application
        {
            get
            {
#if NET45
                return m_Application != null ? m_Application : this["Application"] as string;
#else
                return m_Application;
#endif
            }

            set
            {
                m_Application = value;
            }
        }

#if NET45
        [ConfigurationProperty("Host", IsRequired = true)]
#endif
        public string Host
        {

            get
            {
#if NET45
                return m_Host != null ? m_Host : this["Host"] as string;
#else
                return m_Host;
#endif
            }

            set
            {
                m_Host = value;
            }
        }

#if NET45
        [ConfigurationProperty("Environment", IsRequired = true)]
#endif
        public string Environment
        {

            get
            {
#if NET45
                return m_Environment != null ? m_Environment : this["Environment"] as string;
#else
                return m_Environment;
#endif
            }

            set
            {
                m_Environment = value;
            }
        }

#if NET45
        [ConfigurationProperty("AppID", IsRequired = true)]
#endif
        public string AppID
        {

            get
            {
#if NET45
                return m_AppID != null ? m_AppID : this["AppID"] as string;
#else
                return m_AppID;
#endif
            }

            set
            {
                m_AppID = value;
            }
        }

#if NET45
        [ConfigurationProperty("AppSecret", IsRequired = true)]
#endif
        public string AppSecret
        {

            get
            {
#if NET45
                return m_AppSecret != null ? m_AppSecret : this["AppSecret"] as string;
#else
                return m_AppSecret;
#endif
            }

            set
            {
                m_AppSecret = value;
            }
        }

#if NET45
        [ConfigurationProperty("VerifySSL", DefaultValue = "false", IsRequired = false)]
#endif
        public bool VerifySSL
        {

            get
            {
#if NET45
                return m_VerifySSL.HasValue ? m_VerifySSL.Value : (Boolean)this["VerifySSL"];
#else
                return m_VerifySSL.Value;
#endif
            }

            set
            {
                m_VerifySSL = value;
            }
        }

#if NET45
        [ConfigurationProperty("FromLocal", DefaultValue = "false", IsRequired = false)]
#endif
        public Boolean FromLocal
        {

            get
            {
#if NET45
                return m_FromLocal.HasValue ? m_FromLocal.Value : (Boolean)this["FromLocal"];
#else
                return m_FromLocal.Value;
#endif
            }

            set
            {
                m_FromLocal = value;
            }
        }

#if NET45
        [ConfigurationProperty("LocalPath", IsRequired = false)]
#endif
        public string LocalPath
        {

            get
            {
#if NET45
                return m_LocalPath != null ? m_LocalPath : this["LocalPath"] as string;
#else
                return m_LocalPath;
#endif
            }

            set
            {
                m_LocalPath = value;
            }
        }
    }
}
