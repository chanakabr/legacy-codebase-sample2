using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TVPApiModule.Context;
using TVPApiModule.Objects;

namespace TVPApiModule.Services
{
    public class ServicesManager
    {
        #region Members

        private ConcurrentDictionary<string, BaseService> m_Services;
        private char SPLITTER = '.';
        private readonly int m_FailOverLimit;


        //private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region CTOR

        private ServicesManager()
        {
            m_Services = new ConcurrentDictionary<string, BaseService>();
            m_FailOverLimit = TCMClient.Settings.Instance.GetValue<int>("FailOverLimit");
        }

        #endregion

        #region Singleton

        public static ServicesManager Instance
        {
            get { return Nested.Instance; }
        }

        public int FailOverLimit
        {
            get { return m_FailOverLimit; }
        }
        
        class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly ServicesManager Instance = new ServicesManager();
        }

        public BaseService GetService(int groupId, PlatformType platform, eService serviceType)
        {
            /////// Implement GetInstance Logic /////
            string serviceTcmConfigurationKey = string.Format("{0}{1}{2}{3}{4}", groupId, SPLITTER, platform, SPLITTER, serviceType);
            string serviceUrl = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}{1}{2}", serviceTcmConfigurationKey, SPLITTER, "URL"));

            if (!string.IsNullOrEmpty(serviceUrl) && !m_Services.ContainsKey(serviceUrl))
            {
                BaseService serviceInserted = ServiceFactory.GetService(groupId, platform, serviceUrl, serviceType);

                if (serviceInserted != null)
                {
                    m_Services.TryAdd(serviceUrl, serviceInserted);
                }
            }

            BaseService service = null;
            m_Services.TryGetValue(serviceUrl, out service);

            if (service != null)
            {
                if (HttpContext.Current.Items != null && !HttpContext.Current.Items.Contains("m_wsUserName") && !HttpContext.Current.Items.Contains("m_wsPassword"))
                {
                    string serviceUser = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}{1}{2}", serviceTcmConfigurationKey, SPLITTER, "USER"));
                    string servicePass = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}{1}{2}", serviceTcmConfigurationKey, SPLITTER, "PASSWORD"));
                    if (!string.IsNullOrEmpty(serviceUser) && !string.IsNullOrEmpty(servicePass))
                    {
                        HttpContext.Current.Items.Add("m_wsUserName", serviceUser);
                        HttpContext.Current.Items.Add("m_wsPassword", servicePass);
                    }
                }
            }

            return service;            
        }

        #endregion
    }
}
